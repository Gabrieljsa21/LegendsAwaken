# UX-0: Infraestrutura de Interação Discord Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Estabelecer a infraestrutura de UX híbrida do LA — convenção de customId, InteractionRouter, PanelResult, ConfirmationPanel — e validar tudo convertendo `/cidade` como primeiro sistema.

**Architecture:** Novo `InteractionRouter` coexiste com o switch legado do `CommandHandler` — roteia customIds no formato `sistema:acao[:p1:p2]` e deixa os customIds legados (`sistema_acao|param`) caírem no switch existente. Nenhum sistema além de `/cidade` é tocado neste plano. `CidadeCommand` implementa `IInteractionHandler` e se registra no router.

**Tech Stack:** C# .NET 10, Discord.Net, xUnit, Moq — sem novas dependências.

---

## Mapa de Arquivos

### Criar
| Arquivo | Responsabilidade |
|---|---|
| `LegendsAwaken.Bot/Interactions/PanelResult.cs` | Record imutável `(Embed, MessageComponent)` — output dos painéis |
| `LegendsAwaken.Bot/Interactions/IInteractionHandler.cs` | Interface que toda command-class implementa para registrar no router |
| `LegendsAwaken.Bot/Interactions/InteractionRouter.cs` | Parseia customId, despacha para o handler registrado |
| `LegendsAwaken.Bot/Interactions/ConfirmationPanel.cs` | Builder estático de painel efêmero `[Confirmar] [Cancelar]` |
| `LegendsAwaken.Tests/InteractionRouterTests.cs` | Testes unitários do router (lógica pura, sem Discord types) |

### Modificar
| Arquivo | O que muda |
|---|---|
| `LegendsAwaken.Bot/CommandHandler.cs` | Injeta `InteractionRouter`; no início de `HandleButtonExecutedAsync` e `HandleSelectMenuAsync`, tenta rotear pelo router antes do switch legado |
| `LegendsAwaken.Bot/Program.cs` | Registra `InteractionRouter` como Singleton e chama `RegisterHandlers()` |
| `LegendsAwaken.Bot/Panels/CidadePanel.cs` | Migra todos os customIds de `cidade_acao` e `cidade_acao\|param` para `cidade:acao` e `cidade:acao:param` |
| `LegendsAwaken.Bot/Commands/CidadeCommand.cs` | Implementa `IInteractionHandler`; switch interno por `parts[1]`; adiciona `HandleDesalocarConfirmarAsync` e `HandleConstruirConfirmarAsync`; adiciona `HandleCancelarAsync` |

---

## Task 1: PanelResult record + IInteractionHandler interface

**Files:**
- Create: `LegendsAwaken.Bot/Interactions/PanelResult.cs`
- Create: `LegendsAwaken.Bot/Interactions/IInteractionHandler.cs`

- [ ] **Step 1: Criar diretório e PanelResult**

```csharp
// LegendsAwaken.Bot/Interactions/PanelResult.cs
using Discord;

namespace LegendsAwaken.Bot.Interactions;

public record PanelResult(Embed Embed, MessageComponent Components);
```

- [ ] **Step 2: Criar IInteractionHandler**

```csharp
// LegendsAwaken.Bot/Interactions/IInteractionHandler.cs
using Discord.WebSocket;

namespace LegendsAwaken.Bot.Interactions;

public interface IInteractionHandler
{
    /// <summary>Prefixo do sistema, ex: "cidade". Deve ser único por handler.</summary>
    string CustomIdPrefix { get; }

    /// <summary>
    /// Chamado pelo InteractionRouter quando customId começa com CustomIdPrefix.
    /// parts = customId.Split(':') — parts[0] é o prefix, parts[1] é a ação.
    /// </summary>
    Task HandleAsync(SocketMessageComponent component, string[] parts);
}
```

- [ ] **Step 3: Build para garantir zero erros**

```
dotnet build LegendsAwaken.Bot --no-restore
```
Resultado esperado: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Bot/Interactions/
git commit -m "feat(ux0): add PanelResult record and IInteractionHandler interface"
```

---

## Task 2: InteractionRouter com testes

**Files:**
- Create: `LegendsAwaken.Bot/Interactions/InteractionRouter.cs`
- Create: `LegendsAwaken.Tests/InteractionRouterTests.cs`

- [ ] **Step 1: Escrever os testes primeiro**

```csharp
// LegendsAwaken.Tests/InteractionRouterTests.cs
using Discord.WebSocket;
using LegendsAwaken.Bot.Interactions;
using Moq;

namespace LegendsAwaken.Tests;

public class InteractionRouterTests
{
    [Fact]
    public void CanRoute_ReturnsFalse_WhenNoColon()
    {
        var router = new InteractionRouter();
        Assert.False(router.CanRoute("cidade_coletar"));
    }

    [Fact]
    public void CanRoute_ReturnsFalse_WhenPrefixNotRegistered()
    {
        var router = new InteractionRouter();
        Assert.False(router.CanRoute("cidade:coletar"));
    }

    [Fact]
    public void CanRoute_ReturnsTrue_WhenPrefixRegistered()
    {
        var router = new InteractionRouter();
        var handler = new Mock<IInteractionHandler>();
        handler.Setup(h => h.CustomIdPrefix).Returns("cidade");
        router.Register(handler.Object);

        Assert.True(router.CanRoute("cidade:coletar"));
    }

    [Fact]
    public void ParseParts_SplitsOnColon()
    {
        var parts = InteractionRouter.ParseParts("cidade:node_para_heroi:abc-123");
        Assert.Equal(new[] { "cidade", "node_para_heroi", "abc-123" }, parts);
    }

    [Fact]
    public void Register_OverwritesPreviousHandler_ForSamePrefix()
    {
        var router = new InteractionRouter();
        var h1 = new Mock<IInteractionHandler>();
        h1.Setup(h => h.CustomIdPrefix).Returns("cidade");
        var h2 = new Mock<IInteractionHandler>();
        h2.Setup(h => h.CustomIdPrefix).Returns("cidade");

        router.Register(h1.Object);
        router.Register(h2.Object);

        // Não deve lançar; CanRoute ainda retorna true
        Assert.True(router.CanRoute("cidade:coletar"));
    }
}
```

- [ ] **Step 2: Rodar testes e confirmar que falham**

```
dotnet test LegendsAwaken.Tests --filter "InteractionRouterTests" -v
```
Resultado esperado: erros de compilação (`InteractionRouter` não existe ainda).

- [ ] **Step 3: Implementar InteractionRouter**

```csharp
// LegendsAwaken.Bot/Interactions/InteractionRouter.cs
using Discord.WebSocket;

namespace LegendsAwaken.Bot.Interactions;

public class InteractionRouter
{
    private readonly Dictionary<string, IInteractionHandler> _handlers = new();

    public void Register(IInteractionHandler handler)
        => _handlers[handler.CustomIdPrefix] = handler;

    /// <summary>Retorna true se o customId usa ':' e o prefix está registrado.</summary>
    public bool CanRoute(string customId)
    {
        if (!customId.Contains(':')) return false;
        var prefix = customId.Split(':')[0];
        return _handlers.ContainsKey(prefix);
    }

    public static string[] ParseParts(string customId) => customId.Split(':');

    /// <summary>
    /// Tenta rotear. Retorna true e chama o handler se prefix reconhecido.
    /// Retorna false sem lançar se customId não pertence ao router.
    /// </summary>
    public async Task<bool> TryRouteAsync(SocketMessageComponent component)
    {
        var customId = component.Data.CustomId;
        if (!CanRoute(customId)) return false;
        var parts = ParseParts(customId);
        await _handlers[parts[0]].HandleAsync(component, parts);
        return true;
    }
}
```

- [ ] **Step 4: Rodar testes e confirmar que passam**

```
dotnet test LegendsAwaken.Tests --filter "InteractionRouterTests" -v
```
Resultado esperado: `4 passed, 0 failed`

- [ ] **Step 5: Commit**

```
git add LegendsAwaken.Bot/Interactions/InteractionRouter.cs LegendsAwaken.Tests/InteractionRouterTests.cs
git commit -m "feat(ux0): add InteractionRouter with unit tests"
```

---

## Task 3: ConfirmationPanel

**Files:**
- Create: `LegendsAwaken.Bot/Interactions/ConfirmationPanel.cs`

Nota: Discord não tem timeout client-side configurável — a interação expira naturalmente em 15 min. O padrão efêmero garante que só o usuário que acionou vê e responde ao painel.

- [ ] **Step 1: Criar ConfirmationPanel**

```csharp
// LegendsAwaken.Bot/Interactions/ConfirmationPanel.cs
using Discord;

namespace LegendsAwaken.Bot.Interactions;

public static class ConfirmationPanel
{
    /// <summary>
    /// Cria um embed efêmero de confirmação com botões [Confirmar] e [Cancelar].
    /// </summary>
    /// <param name="mensagem">Texto descritivo da ação a confirmar.</param>
    /// <param name="confirmId">CustomId do botão Confirmar. Formato: "sistema:acao:param"</param>
    /// <param name="cancelId">CustomId do botão Cancelar. Padrão: "global:cancelar"</param>
    public static PanelResult Criar(string mensagem, string confirmId, string cancelId = "global:cancelar")
    {
        var embed = new EmbedBuilder()
            .WithTitle("⚠️ Confirmar ação")
            .WithDescription(mensagem)
            .WithColor(Color.Orange)
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Confirmar", confirmId, ButtonStyle.Danger)
            .WithButton("Cancelar", cancelId, ButtonStyle.Secondary)
            .Build();

        return new PanelResult(embed, components);
    }
}
```

- [ ] **Step 2: Build**

```
dotnet build LegendsAwaken.Bot --no-restore
```
Resultado esperado: `0 Error(s)`

- [ ] **Step 3: Commit**

```
git add LegendsAwaken.Bot/Interactions/ConfirmationPanel.cs
git commit -m "feat(ux0): add ConfirmationPanel for ephemeral confirm/cancel pattern"
```

---

## Task 4: Integrar InteractionRouter no CommandHandler

**Files:**
- Modify: `LegendsAwaken.Bot/CommandHandler.cs`
- Modify: `LegendsAwaken.Bot/Program.cs`

O router é registrado como `Singleton` no DI. `CommandHandler` recebe o router e tenta rotear antes do switch legado.

- [ ] **Step 1: Registrar InteractionRouter e handler global:cancelar no Program.cs**

Localize em `Program.cs` onde o `CommandHandler` é registrado/instanciado. Adicione antes:

```csharp
// Adicionar junto aos outros singletons, ANTES do CommandHandler
services.AddSingleton<InteractionRouter>();
```

Após o build do ServiceProvider, adicione um handler global para cancelar (evita erro quando usuário clica Cancelar):

```csharp
// Após var router = provider.GetRequiredService<InteractionRouter>();
// ou no Initialize() do CommandHandler
```

Na verdade, o `global:cancelar` é mais simples de tratar inline no CommandHandler — veja Step 2.

- [ ] **Step 2: Modificar CommandHandler — injetar router e integrar**

No construtor de `CommandHandler`, adicione `InteractionRouter interactionRouter` como parâmetro (primary constructor style, igual ao resto da classe) e guarde em campo `_interactionRouter`.

No início de `HandleButtonExecutedAsync` (antes do switch existente), adicione:

```csharp
// Rota para o novo sistema se customId usa ':' separator
if (await _interactionRouter.TryRouteAsync(component))
    return;

// Tratamento global de cancelamento efêmero
if (component.Data.CustomId == "global:cancelar")
{
    await component.UpdateAsync(m =>
    {
        m.Content = "Ação cancelada.";
        m.Embed = null;
        m.Components = new ComponentBuilder().Build();
    });
    return;
}

// ... switch legado existente continua aqui
```

Faça o mesmo no início de `HandleSelectMenuAsync` (ou `HandleSelectMenuExecutedAsync`, qualquer que seja o nome do método que trata SelectMenuExecuted):

```csharp
if (await _interactionRouter.TryRouteAsync(component))
    return;
```

- [ ] **Step 3: Build + verificar zero erros**

```
dotnet build LegendsAwaken.Bot --no-restore
```
Resultado esperado: `0 Error(s)`

- [ ] **Step 4: Rodar todos os testes existentes para garantir nenhuma regressão**

```
dotnet test LegendsAwaken.Tests -v
```
Resultado esperado: todos os testes anteriores ainda passando.

- [ ] **Step 5: Commit**

```
git add LegendsAwaken.Bot/CommandHandler.cs LegendsAwaken.Bot/Program.cs
git commit -m "feat(ux0): integrate InteractionRouter into CommandHandler alongside legacy switch"
```

---

## Task 5: Migrar customIds do CidadePanel para convenção ':'

**Files:**
- Modify: `LegendsAwaken.Bot/Panels/CidadePanel.cs`

Esta task migra apenas as **strings de customId** no painel. Os handlers correspondentes no CommandHandler legado ainda usam as strings antigas — eles serão substituídos na Task 6. Por isso, esta task NÃO quebra nada: o router ainda não conhece "cidade", então todos os eventos continuam caindo no switch legado.

Mapeamento completo de migração:

| Antes | Depois |
|---|---|
| `"cidade_coletar"` | `"cidade:coletar"` |
| `"cidade_alocar_node"` | `"cidade:alocar_node"` |
| `"cidade_alocar_heroi_para_node"` | `"cidade:alocar_heroi_para_node"` |
| `"cidade_node_para_heroi\|{heroiId}"` | `$"cidade:node_para_heroi:{heroiId}"` |
| `"cidade_alocar_predio"` | `"cidade:alocar_predio"` |
| `"cidade_alocar_heroi_para_predio"` | `"cidade:alocar_heroi_para_predio"` |
| `"cidade_predio_para_heroi\|{heroiId}"` | `$"cidade:predio_para_heroi:{heroiId}"` |
| `"cidade_desalocar"` | `"cidade:desalocar"` |
| `"cidade_desalocar_heroi"` | `"cidade:desalocar_heroi"` |
| `"cidade_construir"` | `"cidade:construir"` |
| `"cidade_construir_predio"` | `"cidade:construir_predio"` |
| `"cidade_atualizar"` | `"cidade:atualizar"` |
| `"cidade_booster"` | `"cidade:booster"` |
| `"cidade_booster_ativar"` | `"cidade:booster_ativar"` |

- [ ] **Step 1: Aplicar todas as substituições em CidadePanel.cs**

Leia o arquivo e substitua cada string usando Edit. Verifique que nenhum `cidade_` ou `|` sobrou no arquivo após as edições.

```
grep -n "cidade_\|cidade.*|" LegendsAwaken.Bot/Panels/CidadePanel.cs
```
Resultado esperado: nenhuma linha retornada.

- [ ] **Step 2: Build — o projeto NÃO deve quebrar**

```
dotnet build LegendsAwaken.Bot --no-restore
```
Resultado esperado: `0 Error(s)` — o switch legado do CommandHandler ainda terá as strings antigas, mas isso não é erro de compilação.

- [ ] **Step 3: Commit**

```
git add LegendsAwaken.Bot/Panels/CidadePanel.cs
git commit -m "feat(ux0): migrate CidadePanel customIds to ':' convention"
```

---

## Task 6: Refatorar CidadeCommand para implementar IInteractionHandler

**Files:**
- Modify: `LegendsAwaken.Bot/Commands/CidadeCommand.cs`
- Modify: `LegendsAwaken.Bot/Program.cs` (registrar handler no router)

Esta é a task central. `CidadeCommand` implementa `IInteractionHandler`, monta um switch interno por `parts[1]`, e é registrado no router. Os blocos `if/else` que matchavam as strings antigas em `CommandHandler` são **removidos** — o roteamento agora passa pelo router.

- [ ] **Step 1: Adicionar IInteractionHandler à classe e implementar HandleAsync**

No topo de `CidadeCommand.cs`, adicione `: IInteractionHandler` à declaração da classe.

Adicione a propriedade e o método de despacho:

```csharp
public string CustomIdPrefix => "cidade";

public async Task HandleAsync(SocketMessageComponent component, string[] parts)
{
    // parts[0] = "cidade", parts[1] = ação
    var action = parts.Length > 1 ? parts[1] : string.Empty;

    switch (action)
    {
        case "coletar":         await HandleColetarAsync(component); break;
        case "alocar_node":     await HandleAlocarNodeAsync(component); break;
        case "alocar_heroi_para_node": await HandleAlocarHeroiParaNodeAsync(component); break;
        case "node_para_heroi": await HandleNodeParaHeroiAsync(component, parts); break;
        case "alocar_predio":   await HandleAlocarPredioAsync(component); break;
        case "alocar_heroi_para_predio": await HandleAlocarHeroiParaPredioAsync(component); break;
        case "predio_para_heroi": await HandlePredioParaHeroiAsync(component, parts); break;
        case "desalocar":       await HandleDesalocarAsync(component); break;
        case "desalocar_heroi": await HandleDesalocarHeroiSelAsync(component); break;
        case "desalocar_confirmar": await HandleDesalocarConfirmarAsync(component, parts); break;
        case "construir":       await HandleConstruirAsync(component); break;
        case "construir_predio": await HandleConstruirPredioSelAsync(component); break;
        case "construir_confirmar": await HandleConstruirConfirmarAsync(component, parts); break;
        case "atualizar":       await HandleAtualizarAsync(component); break;
        case "booster":         await HandleBoosterAsync(component); break;
        case "booster_ativar":  await HandleBoosterAtivarAsync(component); break;
        default:
            await component.RespondAsync("Ação desconhecida.", ephemeral: true);
            break;
    }
}
```

- [ ] **Step 2: Atualizar assinaturas dos handlers que recebem parâmetro do customId**

Os handlers `HandleNodeParaHeroiAsync` e `HandlePredioParaHeroiAsync` antes extraíam o Guid do customId via `parts[1]` (depois do `|`). Agora recebem `string[] parts` e extraem de `parts[2]`:

```csharp
// Antes (via CommandHandler com split '|'):
// var heroiId = Guid.Parse(parts[1]);

// Depois (via router com split ':'):
// parts = ["cidade", "node_para_heroi", "{heroiId}"]
private async Task HandleNodeParaHeroiAsync(SocketMessageComponent component, string[] parts)
{
    var heroiId = Guid.Parse(parts[2]);
    // ... resto do handler sem alteração
}

private async Task HandlePredioParaHeroiAsync(SocketMessageComponent component, string[] parts)
{
    var heroiId = Guid.Parse(parts[2]);
    // ... resto do handler sem alteração
}
```

- [ ] **Step 3: Adicionar dois novos handlers de confirmação (stub)**

Esses handlers serão implementados na Task 7. Por ora, adicione stubs para o build passar:

```csharp
private async Task HandleDesalocarConfirmarAsync(SocketMessageComponent component, string[] parts)
{
    // Implementado na Task 7
    await component.RespondAsync("Em breve.", ephemeral: true);
}

private async Task HandleConstruirConfirmarAsync(SocketMessageComponent component, string[] parts)
{
    // Implementado na Task 7
    await component.RespondAsync("Em breve.", ephemeral: true);
}
```

- [ ] **Step 4: Registrar CidadeCommand no InteractionRouter no Program.cs**

Localize onde `CidadeCommand` é instanciado (ou onde o `CommandHandler` é construído). Antes de construir o `CommandHandler`, instancie `CidadeCommand` e registre-o:

```csharp
var interactionRouter = provider.GetRequiredService<InteractionRouter>();
var cidadeCommand = new CidadeCommand(
    provider.GetRequiredService<CidadeService>(),
    provider.GetRequiredService<HeroiService>(),
    provider.GetRequiredService<CidadeBoosterService>()
    // ILogger opcional se aplicável
);
interactionRouter.Register(cidadeCommand);
```

> Nota: se `CidadeCommand` é instanciado a cada interação em `CommandHandler`, mova a instância para `Singleton` ou `Scoped` no DI. A forma mais simples: passe a instância registrada acima para o construtor do `CommandHandler`.

- [ ] **Step 5: Remover os blocos cidade_ do switch legado no CommandHandler**

No `HandleButtonExecutedAsync` e `HandleSelectMenuAsync` do `CommandHandler`, remova todos os blocos que matchavam `cidade_*`. O router agora cobre esses casos.

Procure e remova blocos como:
```csharp
// Remover todos os blocos cidade_*:
else if (parts[0] == "cidade_coletar") { ... }
else if (parts[0] == "cidade_alocar_node") { ... }
// etc.
```

- [ ] **Step 6: Build + testes**

```
dotnet build LegendsAwaken.Bot --no-restore
dotnet test LegendsAwaken.Tests -v
```
Resultado esperado: `0 Error(s)`, todos os testes passando.

- [ ] **Step 7: Commit**

```
git add LegendsAwaken.Bot/Commands/CidadeCommand.cs LegendsAwaken.Bot/Program.cs LegendsAwaken.Bot/CommandHandler.cs
git commit -m "feat(ux0): CidadeCommand implements IInteractionHandler, registered in InteractionRouter"
```

---

## Task 7: Adicionar ConfirmationPanel às ações destrutivas de /cidade

**Files:**
- Modify: `LegendsAwaken.Bot/Commands/CidadeCommand.cs`

Duas ações precisam de confirmação antes de executar:
- **desalocar_heroi** → mostra confirmação → **desalocar_confirmar:{heroiId}:{slotTipo}**
- **construir_predio** → mostra confirmação com custo → **construir_confirmar:{tipoPredio}**

- [ ] **Step 1: Modificar HandleDesalocarHeroiSelAsync para exibir confirmação**

O handler atual desaloca diretamente após o select de herói. Altere para exibir um painel de confirmação em vez de executar:

```csharp
// Dentro de HandleDesalocarHeroiSelAsync, após identificar heroiId e slotTipo:
// Substitua a chamada a CidadeService.DesalocarAsync por:

var heroi = await _heroiService.ObterPorIdAsync(heroiId);
var panel = ConfirmationPanel.Criar(
    $"Remover **{heroi.Nome}** do slot de {slotTipo}? Esta ação desaloca o herói imediatamente.",
    confirmId: $"cidade:desalocar_confirmar:{heroiId}:{slotTipo}"
);
await component.UpdateAsync(m =>
{
    m.Embed = panel.Embed;
    m.Components = panel.Components;
});
```

- [ ] **Step 2: Implementar HandleDesalocarConfirmarAsync**

```csharp
private async Task HandleDesalocarConfirmarAsync(SocketMessageComponent component, string[] parts)
{
    // parts = ["cidade", "desalocar_confirmar", "{heroiId}", "{slotTipo}"]
    if (!Guid.TryParse(parts.ElementAtOrDefault(2), out var heroiId))
    {
        await component.UpdateAsync(m => { m.Content = "ID inválido."; m.Components = new ComponentBuilder().Build(); });
        return;
    }

    var userId = component.User.Id;
    var resultado = await _cidadeService.DesalocarHeroiAsync(userId, heroiId);

    // Recarrega o painel principal após a ação
    var panel = await BuildPanelAsync(userId);
    await component.UpdateAsync(m =>
    {
        m.Content = resultado ?? "Herói desalocado.";
        m.Embed = panel.Embed;
        m.Components = panel.Components;
    });
}
```

- [ ] **Step 3: Modificar HandleConstruirPredioSelAsync para exibir confirmação com custo**

O handler atual constrói diretamente após o select de prédio. Altere para exibir confirmação:

```csharp
// Dentro de HandleConstruirPredioSelAsync, após identificar tipoPredio:
var custo = PredioConfig.CustosConstrucao[tipoPredio];
var panel = ConfirmationPanel.Criar(
    $"Construir **{tipoPredio}**?\nCusto: {custo.Ouro} Ouro | {custo.Madeira} Madeira | {custo.Pedra} Pedra",
    confirmId: $"cidade:construir_confirmar:{(int)tipoPredio}"
);
await component.UpdateAsync(m =>
{
    m.Embed = panel.Embed;
    m.Components = panel.Components;
});
```

- [ ] **Step 4: Implementar HandleConstruirConfirmarAsync**

```csharp
private async Task HandleConstruirConfirmarAsync(SocketMessageComponent component, string[] parts)
{
    // parts = ["cidade", "construir_confirmar", "{tipoPredioInt}"]
    if (!int.TryParse(parts.ElementAtOrDefault(2), out var tipoPredioInt)
        || !Enum.IsDefined(typeof(TipoPredio), tipoPredioInt))
    {
        await component.UpdateAsync(m => { m.Content = "Prédio inválido."; m.Components = new ComponentBuilder().Build(); });
        return;
    }

    var tipoPredio = (TipoPredio)tipoPredioInt;
    var userId = component.User.Id;
    var resultado = await _cidadeService.ConstruirPredioAsync(userId, tipoPredio);

    var panel = await BuildPanelAsync(userId);
    await component.UpdateAsync(m =>
    {
        m.Content = resultado ?? $"{tipoPredio} construído.";
        m.Embed = panel.Embed;
        m.Components = panel.Components;
    });
}
```

- [ ] **Step 5: Build + todos os testes**

```
dotnet build LegendsAwaken.Bot --no-restore
dotnet test LegendsAwaken.Tests -v
```
Resultado esperado: `0 Error(s)`, todos os testes passando.

- [ ] **Step 6: Verificar manualmente o fluxo /cidade no Discord**

Checklist de smoke test:
- [ ] `/cidade` abre painel público com 7 botões
- [ ] `[Coletar]` executa e mostra feedback efêmero
- [ ] `[Alocar Node]` → Select de heróis → Select de nodes → herói alocado
- [ ] `[Desalocar]` → Select de heróis alocados → painel de confirmação aparece → `[Confirmar]` desaloca → painel atualiza
- [ ] `[Desalocar]` → painel de confirmação → `[Cancelar]` fecha sem ação
- [ ] `[Construir]` → Select de prédios → painel de confirmação com custo → `[Confirmar]` constrói
- [ ] `[🔄]` atualiza o painel in-place

- [ ] **Step 7: Commit final**

```
git add LegendsAwaken.Bot/Commands/CidadeCommand.cs
git commit -m "feat(ux0): add ConfirmationPanel to desalocar and construir actions in /cidade"
```

---

## Self-Review

### Spec coverage

| Requisito UX-0 | Task que implementa |
|---|---|
| Convenção customId `sistema:acao[:p1:p2]` | Task 5 (CidadePanel) + Task 6 (CidadeCommand) |
| `InteractionRouter` | Task 2 |
| `PanelBuilder` base — ViewModel → (Embed, ComponentBuilder) | Task 1 (PanelResult record); padrão já existia em CidadePanel |
| Padrão `DeferAsync` + `UpdateAsync` | Task 6 (formalizado no switch; padrão já existia) |
| Confirmação efêmera `[Confirmar] [Cancelar]` | Task 3 (ConfirmationPanel) + Task 7 |
| `/cidade` convertido (validação) | Tasks 5+6+7 juntas |

### Notas

- **DeferAsync + UpdateAsync já está implementado** em CidadeCommand (DeferAsync para resposta pública inicial, UpdateAsync para atualizar in-place, DeferAsync ephemeral para sub-fluxos). O plano formaliza o padrão sem reescrever o que já funciona.
- **PanelResult** formaliza o contrato ViewModel→(Embed,Components) mas CidadePanel já segue o padrão; não há necessidade de migrar CidadePanel para usar PanelResult agora — isso pode ser feito gradualmente.
- **Timeout de 30s** não é implementado (requer background task): os painéis expiram naturalmente após 15 min (limite do Discord). Um worker de timeout pode ser adicionado na Fase 3.5.
- **Outros sistemas** (`herois_`, `grupos_`, etc.) não são tocados — migração incremental, sem big bang.
