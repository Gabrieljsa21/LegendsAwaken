# Fase Q — Fundações de Qualidade: Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fechar a dívida técnica antes da Fase 3B: substituir Console.WriteLine por ILogger, mover config hardcoded para appsettings, criar HeroiGuard para validação de heróis, e estabelecer cobertura de testes nos serviços core.

**Architecture:** Mudanças cirúrgicas em serviços existentes. Nenhuma nova entidade de domínio. Testes em `LegendsAwaken.Tests/Services/`. `CombatService.CalcularDano` precisa ser `public` para ser testável diretamente.

**Tech Stack:** C# 13 / net10.0, xUnit 2.9, Moq 4.20, EF Core 10 Sqlite (in-memory para integration test), `Microsoft.Extensions.Logging.Abstractions` (já no projeto via DI)

---

## File Map

| Arquivo | Ação |
|---|---|
| `LegendsAwaken.Application/Services/GeracaoDeDadosService.cs` | Modify: add `ILogger<GeracaoDeDadosService>`, replace 3 `Console.WriteLine` |
| `LegendsAwaken.Infrastructure/Repositories/HeroiRepository.cs` | Modify: add `ILogger<HeroiRepository>`, replace 4 `Console.WriteLine` |
| `LegendsAwaken.Bot/appsettings.json` | Modify: add `Discord.GuildId`, change DB path para relativo |
| `LegendsAwaken.Bot/Program.cs` | Modify: ler `GUILD_ID` de `configuration["Discord:GuildId"]` |
| `LegendsAwaken.Application/Services/HeroiGuard.cs` | Create: classe estática com 2 métodos de validação |
| `LegendsAwaken.Bot/Commands/ArenaCommand.cs` | Modify: usar `HeroiGuard.ValidarTodosDisponiveis` |
| `LegendsAwaken.Application/Services/CombatService.cs` | Modify: `CalcularDano` de `internal` para `public` |
| `LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs` | Create: 8 testes unitários |
| `LegendsAwaken.Tests/Services/CombatServiceTests.cs` | Create: 6 testes unitários |
| `LegendsAwaken.Tests/Services/CidadeServiceTests.cs` | Create: 4 testes unitários |
| `LegendsAwaken.Tests/Integration/FragmentosRecrutarIntegrationTests.cs` | Create: 2 testes de integração |

---

## Task 1: ILogger em GeracaoDeDadosService

**Files:**
- Modify: `LegendsAwaken.Application/Services/GeracaoDeDadosService.cs`

O construtor atual não tem logger. Há 3 `Console.WriteLine` nas linhas 55, 120 e 136–139.

- [ ] **Step 1: Substituir `Console.WriteLine` por ILogger**

Edite `GeracaoDeDadosService.cs`:

```csharp
// Adicionar campo (após os outros campos privados, antes do construtor):
private readonly ILogger<GeracaoDeDadosService> _logger;

// Adicionar parâmetro no construtor (após IJogadorItemRepository jogadorItemRepo):
ILogger<GeracaoDeDadosService> logger

// No body do construtor, adicionar:
_logger = logger;

// Linha 55 — substituir:
Console.WriteLine($"[DEBUG] Banco em uso (DbContext): {connection.ConnectionString}");
// por:
_logger.LogDebug("Banco em uso (DbContext): {ConnectionString}", connection.ConnectionString);

// Linha 120 — substituir:
Console.WriteLine("[Migration] Coluna Inimigos adicionada à tabela Andares.");
// por:
_logger.LogInformation("Migration: coluna Inimigos adicionada à tabela Andares");

// Linhas 136-139 — substituir:
Console.WriteLine("Tabelas no banco:");
while (await reader.ReadAsync())
{
    Console.WriteLine(reader.GetString(0));
}
// por:
var tabelas = new System.Collections.Generic.List<string>();
while (await reader.ReadAsync())
    tabelas.Add(reader.GetString(0));
_logger.LogInformation("Tabelas no banco: {Tabelas}", string.Join(", ", tabelas));
```

- [ ] **Step 2: Build para verificar zero erros**

```powershell
dotnet build LegendsAwaken.Application\LegendsAwaken.Application.csproj
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)` — se falhar, `ILogger<T>` já está disponível via `Microsoft.Extensions.Logging.Abstractions` (sem novo pacote necessário).

- [ ] **Step 3: Build o Bot (DI chain)**

```powershell
dotnet build LegendsAwaken.Bot\LegendsAwaken.Bot.csproj
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)` — o DI já registra `ILogger<T>` via `AddLogging` em `Program.cs`.

- [ ] **Step 4: Commit**

```powershell
git add LegendsAwaken.Application/Services/GeracaoDeDadosService.cs
git commit -m @'
refactor(infra): replace Console.WriteLine with ILogger in GeracaoDeDadosService

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Task 2: ILogger em HeroiRepository

**Files:**
- Modify: `LegendsAwaken.Infrastructure/Repositories/HeroiRepository.cs`

Há 4 `Console.WriteLine` nos métodos `AdicionarAsync` (linhas 64–65) e `AtualizarAsync` (linhas 83–84) dentro de blocos `catch`.

- [ ] **Step 1: Substituir `Console.WriteLine` por ILogger**

Edite `HeroiRepository.cs`:

```csharp
// Adicionar field (após _dbContext):
private readonly ILogger<HeroiRepository> _logger;

// Adicionar parâmetro no construtor:
public HeroiRepository(LegendsAwakenDbContext dbContext, ILogger<HeroiRepository> logger)
{
    _dbContext = dbContext;
    _logger = logger;
}

// Em AdicionarAsync — substituir as 2 linhas no catch:
Console.WriteLine("Erro ao adicionar heroi no banco de dados:");
Console.WriteLine(ex.ToString());
// por:
_logger.LogError(ex, "Erro ao adicionar herói no banco de dados");

// Em AtualizarAsync — substituir as 2 linhas no catch:
Console.WriteLine("Erro ao atualizar heroi no banco de dados:");
Console.WriteLine(ex.ToString());
// por:
_logger.LogError(ex, "Erro ao atualizar herói no banco de dados");
```

- [ ] **Step 2: Build Infrastructure**

```powershell
dotnet build LegendsAwaken.Infrastructure\LegendsAwaken.Infrastructure.csproj
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 3: Build Bot (DI chain)**

```powershell
dotnet build LegendsAwaken.Bot\LegendsAwaken.Bot.csproj
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Commit**

```powershell
git add LegendsAwaken.Infrastructure/Repositories/HeroiRepository.cs
git commit -m @'
refactor(infra): replace Console.WriteLine with ILogger in HeroiRepository

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Task 3: GuildId para appsettings + DB path relativo

**Files:**
- Modify: `LegendsAwaken.Bot/appsettings.json`
- Modify: `LegendsAwaken.Bot/Program.cs`

O `GUILD_ID = 1388541192806989834` está hardcoded em `Program.cs:31`. O path do banco é absoluto (`C:\Workspace\LegendsAwaken\...`), quebrando em qualquer outra máquina.

- [ ] **Step 1: Atualizar appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=legendsawaken.db"
  },
  "Discord": {
    "GuildId": "1388541192806989834"
  },
  "R2": {
    "Endpoint": "https://580f5ff694c261829cd0495bc1feeb17.r2.cloudflarestorage.com",
    "Bucket": "game-assets"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

- [ ] **Step 2: Atualizar Program.cs**

Substituir linha 31:
```csharp
private static readonly ulong GUILD_ID = 1388541192806989834;
```
por (colocar dentro de `IniciarAsync`, após criar `configuration`):
```csharp
var guildIdStr = configuration["Discord:GuildId"]
    ?? throw new InvalidOperationException("Discord:GuildId não configurado em appsettings.json.");
var GUILD_ID = ulong.Parse(guildIdStr);
```

Remover o campo estático `private static readonly ulong GUILD_ID`.

- [ ] **Step 3: Build Bot**

```powershell
dotnet build LegendsAwaken.Bot\LegendsAwaken.Bot.csproj
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Commit**

```powershell
git add LegendsAwaken.Bot/appsettings.json LegendsAwaken.Bot/Program.cs
git commit -m @'
chore(bot): move GuildId to appsettings; use relative DB path

- GUILD_ID hardcode removed; read from Discord:GuildId config
- DB path changed from absolute to relative (works on any machine)

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Task 4: HeroiGuard — validação centralizada de heróis

**Files:**
- Create: `LegendsAwaken.Application/Services/HeroiGuard.cs`
- Modify: `LegendsAwaken.Bot/Commands/ArenaCommand.cs`

Atualmente `ArenaCommand` tem duas checagens inline (`EstadoSustento.Degradado` e filtro de `Inativo`) que vão se repetir em múltiplos comandos (Torre, Arena, Crafting). `HeroiGuard` centraliza.

- [ ] **Step 1: Criar HeroiGuard.cs**

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services;

public static class HeroiGuard
{
    // Retorna mensagem de erro se herói não pode ser usado; null = disponível.
    public static string? ValidarDisponivel(Heroi heroi)
    {
        if (heroi.EstadoSustento == EstadoSustento.Degradado)
            return $"{heroi.Nome} está degradado (sem sustento). Produza Comida antes de continuar.";
        if (heroi.EstadoSustento == EstadoSustento.Inativo)
            return $"{heroi.Nome} está inativo e não pode ser usado em combate.";
        return null;
    }

    // Retorna a primeira mensagem de erro se qualquer herói da lista não puder ser usado; null = todos disponíveis.
    public static string? ValidarTodosDisponiveis(IEnumerable<Heroi> herois)
        => herois.Select(ValidarDisponivel).FirstOrDefault(m => m != null);
}
```

- [ ] **Step 2: Integrar em ArenaCommand**

Em `ArenaCommand.cs`, substituir o bloco de checagem manual (linhas 47–65 aprox):

```csharp
// Substituir:
if (herois.Any(h => h.EstadoSustento == EstadoSustento.Degradado))
{
    await command.RespondAsync("🔴 Seus heróis estão degradados (sem comida)...", ephemeral: true);
    return;
}

var party = herois
    .Where(h => h.EstadoSustento != EstadoSustento.Inativo)
    .OrderByDescending(h => h.Nivel)
    .ThenByDescending(h => (int)h.Raridade)
    .Take(5)
    .ToList();

if (!party.Any())
{
    await command.RespondAsync("Nenhum herói ativo disponível para o desafio.", ephemeral: true);
    return;
}

// por:
var party = herois
    .OrderByDescending(h => h.Nivel)
    .ThenByDescending(h => (int)h.Raridade)
    .Take(5)
    .ToList();

var guardErro = HeroiGuard.ValidarTodosDisponiveis(party);
if (guardErro != null)
{
    await command.RespondAsync($"🔴 {guardErro}", ephemeral: true);
    return;
}
```

- [ ] **Step 3: Build**

```powershell
dotnet build LegendsAwaken.Bot\LegendsAwaken.Bot.csproj
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Commit**

```powershell
git add LegendsAwaken.Application/Services/HeroiGuard.cs LegendsAwaken.Bot/Commands/ArenaCommand.cs
git commit -m @'
feat(guard): add HeroiGuard static class; integrate in ArenaCommand

- ValidarDisponivel and ValidarTodosDisponiveis centralize hero availability logic
- ArenaCommand migrated from inline checks to HeroiGuard

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Task 5: Unit tests — HeroiLevelUpService

**Files:**
- Create: `LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs`

`HeroiLevelUpService` é pure logic (sem DI). Testar os invariantes da fórmula de XP e os caps.

Configs relevantes:
- 1★: Cap=20, BaseStatsTotal=50, GanhoPorNivel=2, BaseXp=80
- 4★: Cap=80, BaseStatsTotal=130, GanhoPorNivel=6, BaseXp=150
- 5★: Cap=100, BaseStatsTotal=175, GanhoPorNivel=8, GanhoSuperacao=12, BaseXp=200 (superação: acima do cap 4★=80)
- Humano XP mult=1.10, Bestial XP mult=1.00

- [ ] **Step 1: Escrever os testes (RED)**

```csharp
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Tests.Services;

public class HeroiLevelUpServiceTests
{
    private readonly HeroiLevelUpService _sut = new();

    // ── XP para próximo nível ─────────────────────────────────────────────
    [Fact]
    public void XpParaProximoNivel_1star_nivel1_retorna_80()
    {
        // 1★: BaseXp=80, nivel=1 → 80×1=80
        Assert.Equal(80, _sut.XpParaProximoNivel(1, raridade: 1));
    }

    [Fact]
    public void XpParaProximoNivel_5star_nivel10_retorna_2000()
    {
        // 5★: BaseXp=200, nivel=10 → 200×10=2000
        Assert.Equal(2000, _sut.XpParaProximoNivel(10, raridade: 5));
    }

    // ── Cap de nível ──────────────────────────────────────────────────────
    [Theory]
    [InlineData(1, 20)]
    [InlineData(3, 60)]
    [InlineData(5, 100)]
    public void CapParaRaridade_retorna_valor_correto(int raridade, int expectedCap)
    {
        Assert.Equal(expectedCap, _sut.CapParaRaridade(raridade));
    }

    // ── Pontos por level-up ───────────────────────────────────────────────
    [Fact]
    public void PontosAtributos_5star_acima_cap4star_usa_GanhoSuperacao()
    {
        // 5★ GanhoSuperacao=12 ativado quando nivel > cap(4★)=80
        Assert.Equal(12, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 81, raridade: 5));
    }

    [Fact]
    public void PontosAtributos_5star_abaixo_cap4star_usa_GanhoPorNivel()
    {
        // 5★ GanhoPorNivel=8 quando nivel ≤ 80
        Assert.Equal(8, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 50, raridade: 5));
    }

    // ── AplicarXp: multiplicador racial e level-up ────────────────────────
    [Fact]
    public void AplicarXp_Humano_aplica_multiplicador_110()
    {
        var heroi = new Heroi { Raca = Raca.Humano, Raridade = Raridade.UmaEstrela, Nivel = 1, XP = 0 };
        // 1★ BaseXp=80, nivel=1 → xpNecessario=80. Dar 73 XP → com mult 1.10 → 80.3 → int=80 → level-up
        _sut.AplicarXp(heroi, 73);
        Assert.Equal(2, heroi.Nivel);
    }

    [Fact]
    public void AplicarXp_nao_ultrapassa_cap()
    {
        var heroi = new Heroi { Raca = Raca.Bestial, Raridade = Raridade.UmaEstrela, Nivel = 20, XP = 0 };
        // Já no cap — nenhum level-up deve ocorrer e XP excedente fica 0
        _sut.AplicarXp(heroi, 99999);
        Assert.Equal(20, heroi.Nivel);
        Assert.Equal(0, heroi.XP);
    }

    // ── Grant de ascensão ─────────────────────────────────────────────────
    [Fact]
    public void CalcularGrantAscensao_4to5_nivel1_retorna_diferenca_bases()
    {
        // 4★ nativo lv1 = BaseStatsTotal(4★)=130; 5★ nativo lv1 = 175 → grant=45
        int grant = _sut.CalcularGrantAscensao(nivelAtual: 1, raridadeAtual: 4);
        Assert.Equal(45, grant);
    }
}
```

- [ ] **Step 2: Rodar testes para confirmar RED (sem implementação nova — os testes devem passar)**

```powershell
dotnet test LegendsAwaken.Tests\LegendsAwaken.Tests.csproj --filter "FullyQualifiedName~HeroiLevelUpServiceTests" --verbosity normal
```

Expected: todos PASS (a lógica já existe — apenas os testes estão sendo escritos agora). Se algum falhar, verifique os valores das constantes em `HeroiLevelUpService.Configs`.

- [ ] **Step 3: Commit**

```powershell
git add LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs
git commit -m @'
test: unit tests for HeroiLevelUpService (XP formula, caps, racial bonus, ascension grant)

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Task 6: Unit tests — CombatService (+ tornar CalcularDano public)

**Files:**
- Modify: `LegendsAwaken.Application/Services/CombatService.cs`
- Create: `LegendsAwaken.Tests/Services/CombatServiceTests.cs`

`CalcularDano` está `internal` → mudar para `public` para permitir testes unitários diretos. A fórmula:
- `K = 1000 + nivel × 50`
- `mitigacao = DEF / (DEF + K)`
- `danoBase = ATK × skillMult × (1 - mitigacao) × typeMult`
- BurstCap = `int(VidaMaxima × 0.65)`
- Crit = `5% base + Percepcao × 0.1%` → ×1.5

- [ ] **Step 1: Tornar CalcularDano public**

Em `CombatService.cs` linha 89, alterar:
```csharp
internal int CalcularDano(Combatente atk, Combatente def, double skillMult, double typeMult = 1.0)
```
para:
```csharp
public int CalcularDano(Combatente atk, Combatente def, double skillMult, double typeMult = 1.0)
```

- [ ] **Step 2: Build Application**

```powershell
dotnet build LegendsAwaken.Application\LegendsAwaken.Application.csproj
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 3: Escrever os testes**

Você precisará importar `LegendsAwaken.Domain.Entities.Combate` e criar `Combatente` manualmente.

```csharp
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Combate;

namespace LegendsAwaken.Tests.Services;

public class CombatServiceTests
{
    private readonly CombatService _sut = new();

    private static Combatente Atacante(int forca, int percepcao = 0, int nivel = 1) => new()
    {
        Id = Guid.NewGuid(),
        Nome = "Atk",
        Nivel = nivel,
        IsHeroi = true,
        Atributos = new AtributosBase { Forca = forca, Percepcao = percepcao },
        Status = new StatusCombate { VidaAtual = 500, VidaMaxima = 500 }
    };

    private static Combatente Defensor(int vitalidade, int vidaMaxima = 1000, int nivel = 1) => new()
    {
        Id = Guid.NewGuid(),
        Nome = "Def",
        Nivel = nivel,
        IsHeroi = false,
        Atributos = new AtributosBase { Vitalidade = vitalidade },
        Status = new StatusCombate { VidaAtual = vidaMaxima, VidaMaxima = vidaMaxima }
    };

    [Fact]
    public void CalcularDano_sem_defesa_retorna_proximo_de_ATK()
    {
        // DEF=0 → mitigacao=0 → danoBase = ATK × 1.0 × 1.0
        // K=1000+1×50=1050; mitigacao=0/(0+1050)=0; dano=200
        var atk = Atacante(forca: 200);
        var def = Defensor(vitalidade: 0, vidaMaxima: 2000);
        int dano = _sut.CalcularDano(atk, def, skillMult: 1.0);
        Assert.Equal(200, dano);
    }

    [Fact]
    public void CalcularDano_alta_defesa_reduz_dano()
    {
        // DEF=1050 → mitigacao=1050/2100=0.5 → dano=200×0.5=100
        var atk = Atacante(forca: 200);
        var def = Defensor(vitalidade: 1050, vidaMaxima: 2000);
        int dano = _sut.CalcularDano(atk, def, skillMult: 1.0);
        Assert.Equal(100, dano);
    }

    [Fact]
    public void CalcularDano_respeitaBurstCap()
    {
        // BurstCap = int(100 × 0.65) = 65
        // ATK=10000, DEF=0 → danoBase=10000, mas cap=65
        var atk = Atacante(forca: 10000);
        var def = Defensor(vitalidade: 0, vidaMaxima: 100);
        int dano = _sut.CalcularDano(atk, def, skillMult: 1.0);
        Assert.Equal(65, dano);
    }

    [Fact]
    public void CalcularDano_skillMult_escala_linearmente()
    {
        // DEF=0, ATK=100, skillMult=2.0 → dano=200
        var atk = Atacante(forca: 100);
        var def = Defensor(vitalidade: 0, vidaMaxima: 2000);
        int dano = _sut.CalcularDano(atk, def, skillMult: 2.0);
        Assert.Equal(200, dano);
    }

    [Fact]
    public void CalcularDano_minimo_1()
    {
        // ATK=0 → danoBase=0 → Math.Clamp(0, 1, burstCap) = 1
        var atk = Atacante(forca: 0);
        var def = Defensor(vitalidade: 0, vidaMaxima: 1000);
        int dano = _sut.CalcularDano(atk, def, skillMult: 1.0);
        Assert.Equal(1, dano);
    }

    [Fact]
    public void ExecutarRound_mata_defensor_com_0_HP()
    {
        // Atacante com ATK altíssimo deve zerar HP do defensor em 1 round
        var heroi = Atacante(forca: 99999, percepcao: 0);
        heroi.IsHeroi = true;
        var inimigo = Defensor(vitalidade: 0, vidaMaxima: 10);
        inimigo.IsHeroi = false;

        var enc = new CombatEncounter
        {
            Aliados = [heroi],
            Inimigos = [inimigo]
        };

        _sut.ExecutarRound(enc);

        Assert.True(enc.IsFinished);
        Assert.Equal(0, inimigo.Status.VidaAtual);
    }
}
```

> **Nota:** Se `AtributosBase`, `StatusCombate` ou `CombatEncounter` não tiverem os campos exatos acima, ajuste para os nomes reais no domínio. Leia `LegendsAwaken.Domain/Entities/Combate/` antes de escrever.

- [ ] **Step 4: Rodar testes**

```powershell
dotnet test LegendsAwaken.Tests\LegendsAwaken.Tests.csproj --filter "FullyQualifiedName~CombatServiceTests" --verbosity normal
```

Expected: todos PASS.

- [ ] **Step 5: Commit**

```powershell
git add LegendsAwaken.Application/Services/CombatService.cs LegendsAwaken.Tests/Services/CombatServiceTests.cs
git commit -m @'
test: unit tests for CombatService; make CalcularDano public

- Burst cap, formula, skillMult, minimum damage, round execution
- CalcularDano: internal → public for testability

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Task 7: Unit tests — CidadeService produção passiva (ResourceNode)

**Files:**
- Create: `LegendsAwaken.Tests/Services/CidadeServiceTests.cs`

Testar `ColetarProducaoAsync` para Tier 1 (ResourceNode workers). O método lê heroes e `UltimaColeta` do repositório. Usar Moq.

- [ ] **Step 1: Identificar dependências para mock**

`CidadeService` depende de:
- `ICidadeRepository` → retorna `Cidade` com `Trabalhadores` e `UltimaColeta`
- `IHeroiRepository` → retorna lista de `Heroi` com `Profissao` e `Id`
- `ISlotOcupacaoRepository` → retorna lista de slots
- `CidadeBoosterService` → retorna booster (pode retornar null para mult=1.0)

`CidadeBoosterService` tem suas próprias dependências. Mockar `ICidadeBoosterRepository` e criar instância real de `CidadeBoosterService`.

- [ ] **Step 2: Escrever os testes**

```csharp
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Tests.Services;

public class CidadeServiceTests
{
    private readonly Mock<ICidadeRepository> _cidadeRepo = new();
    private readonly Mock<IHeroiRepository> _heroiRepo = new();
    private readonly Mock<ISlotOcupacaoRepository> _slotRepo = new();
    private readonly Mock<ICidadeBoosterRepository> _boosterRepo = new();

    private CidadeService CreateService()
    {
        var boosterService = new CidadeBoosterService(_boosterRepo.Object);
        return new CidadeService(_cidadeRepo.Object, _heroiRepo.Object, _slotRepo.Object, boosterService);
    }

    private static Cidade CidadeComTrabalhador(Guid heroiId, TipoResourceNode node, DateTime ultimaColeta)
        => new()
        {
            Id = Guid.NewGuid(),
            UsuarioId = 111UL,
            Recursos = new Recursos(),
            Construcoes = new List<Construcao>(),
            UltimaColeta = ultimaColeta,
            Trabalhadores = new List<PersonagemTrabalhador>
            {
                new() { HeroiId = heroiId, ResourceNode = node, InicioTrabalho = ultimaColeta }
            }
        };

    [Fact]
    public async Task ColetarProducao_ResourceNode_produz_recurso_apos_1h()
    {
        var usuarioId = 111UL;
        var heroiId = Guid.NewGuid();
        var heroi = new Heroi { Id = heroiId, UsuarioId = usuarioId };
        var cidade = CidadeComTrabalhador(heroiId, TipoResourceNode.Campo, DateTime.UtcNow.AddHours(-2));

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId)).ReturnsAsync(cidade);
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId)).ReturnsAsync(new List<Heroi> { heroi });
        _slotRepo.Setup(r => r.ObterPorConstrucaoAsync(It.IsAny<Guid>())).ReturnsAsync(new List<SlotOcupacao>());
        _boosterRepo.Setup(r => r.ObterAtivoAsync(usuarioId)).ReturnsAsync((CidadeBooster?)null);

        var service = CreateService();
        var (_, produzido) = await service.ColetarProducaoAsync(usuarioId);

        // Campo produz Comida — verificar que alguma coisa foi produzida
        Assert.True(produzido.Comida > 0 || produzido.Erva > 0 || produzido.Ouro > 0,
            "Deve produzir algum recurso após 2h no Campo");
    }

    [Fact]
    public async Task ColetarProducao_retorna_vazio_se_menos_de_1min()
    {
        var usuarioId = 222UL;
        var heroiId = Guid.NewGuid();
        var cidade = CidadeComTrabalhador(heroiId, TipoResourceNode.Campo, DateTime.UtcNow.AddSeconds(-30));

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId)).ReturnsAsync(cidade);
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId)).ReturnsAsync(new List<Heroi>());
        _boosterRepo.Setup(r => r.ObterAtivoAsync(usuarioId)).ReturnsAsync((CidadeBooster?)null);

        var service = CreateService();
        var (_, produzido) = await service.ColetarProducaoAsync(usuarioId);

        Assert.Equal(0, produzido.Comida);
        Assert.Equal(0, produzido.Ouro);
        Assert.Equal(0, produzido.Madeira);
    }

    [Fact]
    public async Task ColetarProducao_cap_24h_nao_acumula_alem()
    {
        var usuarioId = 333UL;
        var heroiId = Guid.NewGuid();
        var heroi = new Heroi { Id = heroiId, UsuarioId = usuarioId };
        // 48h atrás — deve ser clamped para 24h de produção
        var cidade = CidadeComTrabalhador(heroiId, TipoResourceNode.Campo, DateTime.UtcNow.AddHours(-48));

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId)).ReturnsAsync(cidade);
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId)).ReturnsAsync(new List<Heroi> { heroi });
        _slotRepo.Setup(r => r.ObterPorConstrucaoAsync(It.IsAny<Guid>())).ReturnsAsync(new List<SlotOcupacao>());
        _boosterRepo.Setup(r => r.ObterAtivoAsync(usuarioId)).ReturnsAsync((CidadeBooster?)null);

        var service24h = CreateService();
        var (_, prod48) = await service24h.ColetarProducaoAsync(usuarioId);

        // Resetar UltimaColeta para 24h atrás
        cidade.UltimaColeta = DateTime.UtcNow.AddHours(-24);
        var (_, prod24) = await service24h.ColetarProducaoAsync(usuarioId);

        // 48h deve produzir o mesmo que 24h (cap)
        Assert.Equal(prod24.Comida, prod48.Comida);
    }

    [Fact]
    public async Task ColetarProducao_sem_trabalhadores_retorna_zero()
    {
        var usuarioId = 444UL;
        var cidade = new Cidade
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Recursos = new Recursos(),
            Construcoes = new List<Construcao>(),
            UltimaColeta = DateTime.UtcNow.AddHours(-2),
            Trabalhadores = new List<PersonagemTrabalhador>()
        };

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId)).ReturnsAsync(cidade);
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId)).ReturnsAsync(new List<Heroi>());
        _boosterRepo.Setup(r => r.ObterAtivoAsync(usuarioId)).ReturnsAsync((CidadeBooster?)null);

        var service = CreateService();
        var (_, produzido) = await service.ColetarProducaoAsync(usuarioId);

        Assert.Equal(0, produzido.Comida + produzido.Ouro + produzido.Madeira + produzido.Pedra);
    }
}
```

> **Nota:** Confirme os nomes exatos de `TipoResourceNode` (Campo, Floresta, Mina, Prado), `PersonagemTrabalhador`, e `Recursos` lendo `LegendsAwaken.Domain/Entities/` antes de escrever.

- [ ] **Step 3: Rodar testes**

```powershell
dotnet test LegendsAwaken.Tests\LegendsAwaken.Tests.csproj --filter "FullyQualifiedName~CidadeServiceTests" --verbosity normal
```

Expected: todos PASS.

- [ ] **Step 4: Commit**

```powershell
git add LegendsAwaken.Tests/Services/CidadeServiceTests.cs
git commit -m @'
test: unit tests for CidadeService passive ResourceNode production

- 24h cap, <1min guard, zero workers, baseline production after 2h

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Task 8: Integration test — fragmentos → recrutar

**Files:**
- Create: `LegendsAwaken.Tests/Integration/FragmentosRecrutarIntegrationTests.cs`

Testa o fluxo completo: criar config de herói, adicionar fragmentos, chamar `RecruitmentService.TentarRecrutarPorFragmentosAsync`, verificar desbloqueio. Usa SQLite in-memory com `Cache=Shared`.

`RecruitmentService` construtor: `(IHeroiDesbloqueadoRepository, IHeroiConfigRepository, IFragmentoRepository, HeroiService)`. O `HeroiService` é necessário quando o recrutamento efetivamente cria o herói (Task `Desbloquear`).

- [ ] **Step 1: Identificar o que `Desbloquear` chama internamente**

Leia `RecruitmentService.cs` (método `Desbloquear` privado) para confirmar se ele chama `HeroiService.CriarHeroiAsync` ou apenas `IHeroiDesbloqueadoRepository.AdicionarAsync`. Isso determina o escopo do integration test.

Se `Desbloquear` chama apenas o repositório → usar mock para `HeroiService`.
Se chama `CriarHeroiAsync` → precisará de repositórios reais no in-memory DB.

- [ ] **Step 2: Criar o test com SQLite in-memory**

```csharp
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using LegendsAwaken.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LegendsAwaken.Tests.Integration;

public class FragmentosRecrutarIntegrationTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly LegendsAwakenDbContext _db;

    public FragmentosRecrutarIntegrationTests()
    {
        _conn = new SqliteConnection("Data Source=fa_recrutamento_test;Mode=Memory;Cache=Shared");
        _conn.Open();

        var opts = new DbContextOptionsBuilder<LegendsAwakenDbContext>()
            .UseSqlite(_conn)
            .Options;
        _db = new LegendsAwakenDbContext(opts);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }

    private RecruitmentService CreateService(
        IHeroiDesbloqueadoRepository desbloqueadoRepo,
        IHeroiConfigRepository configRepo,
        IFragmentoRepository fragmentoRepo,
        HeroiService? heroiService = null)
    {
        // Se HeroiService não for necessário para o flow de fragmentos,
        // use um mock que não faz nada (o RecruitmentService chamará null-safe)
        var mockHeroiService = new Mock<HeroiService>();
        return new RecruitmentService(
            desbloqueadoRepo,
            configRepo,
            fragmentoRepo,
            heroiService ?? mockHeroiService.Object);
    }

    [Fact]
    public async Task Recrutar_por_fragmentos_sucesso_com_30_fragmentos()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId = Guid.NewGuid();

        // Seed: HeroiConfig + HeroiUnlockConfig
        var config = new HeroiConfig { Id = heroiId, Nome = "Grom", Raridade = Raridade.TresEstrelas };
        var unlock = new HeroiUnlockConfig
        {
            Id = Guid.NewGuid(),
            HeroiId = heroiId,
            TipoUnlock = TipoUnlock.Fragmentos,
            QuantidadeFragmentos = 30
        };
        _db.Set<HeroiConfig>().Add(config);
        _db.Set<HeroiUnlockConfig>().Add(unlock);

        // Seed: Fragmento com 30 unidades
        var progresso = new FragmentoProgresso
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            HeroiId = heroiId,
            Quantidade = 30
        };
        _db.Set<FragmentoProgresso>().Add(progresso);
        await _db.SaveChangesAsync();

        var configRepo = new HeroiConfigRepository(_db);
        var fragmentoRepo = new FragmentoRepository(_db);
        var desbloqueadoRepo = new HeroiDesbloqueadoRepository(_db);

        var service = new RecruitmentService(
            desbloqueadoRepo, configRepo, fragmentoRepo,
            null!); // HeroiService só é chamado se Desbloquear instanciar herói

        var resultado = await service.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.True(await desbloqueadoRepo.JaDesbloqueadoAsync(usuarioId, heroiId));
    }

    [Fact]
    public async Task Recrutar_por_fragmentos_falha_com_fragmentos_insuficientes()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId = Guid.NewGuid();

        var config = new HeroiConfig { Id = heroiId, Nome = "Lyra" };
        var unlock = new HeroiUnlockConfig
        {
            Id = Guid.NewGuid(),
            HeroiId = heroiId,
            TipoUnlock = TipoUnlock.Fragmentos,
            QuantidadeFragmentos = 30
        };
        var progresso = new FragmentoProgresso
        {
            Id = Guid.NewGuid(), UsuarioId = usuarioId, HeroiId = heroiId, Quantidade = 15
        };

        _db.Set<HeroiConfig>().Add(config);
        _db.Set<HeroiUnlockConfig>().Add(unlock);
        _db.Set<FragmentoProgresso>().Add(progresso);
        await _db.SaveChangesAsync();

        var configRepo = new HeroiConfigRepository(_db);
        var fragmentoRepo = new FragmentoRepository(_db);
        var desbloqueadoRepo = new HeroiDesbloqueadoRepository(_db);

        var service = new RecruitmentService(desbloqueadoRepo, configRepo, fragmentoRepo, null!);
        var resultado = await service.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);

        Assert.False(resultado.Sucesso);
        Assert.Contains("15/30", resultado.Mensagem);
    }
}
```

> **Nota antes de executar:** confirme que `HeroiConfigRepository`, `FragmentoRepository` e `HeroiDesbloqueadoRepository` têm construtores que aceitam apenas `LegendsAwakenDbContext`. Se qualquer repo usa `SqliteConnection` diretamente (como `TorreRepository`), use `Mock<IHeroiDesbloqueadoRepository>` em vez de instância real para esse repo específico. Leia os construtores em `Infrastructure/Repositories/` antes.

- [ ] **Step 3: Rodar testes de integração**

```powershell
dotnet test LegendsAwaken.Tests\LegendsAwaken.Tests.csproj --filter "FullyQualifiedName~FragmentosRecrutarIntegrationTests" --verbosity normal
```

Expected: ambos PASS.

- [ ] **Step 4: Rodar suite completa**

```powershell
dotnet test LegendsAwaken.Tests\LegendsAwaken.Tests.csproj --verbosity normal
```

Expected: todos os testes existentes + novos PASS.

- [ ] **Step 5: Commit**

```powershell
git add LegendsAwaken.Tests/Integration/FragmentosRecrutarIntegrationTests.cs
git commit -m @'
test: integration test for fragmentos -> recrutar flow with real SQLite

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```

---

## Self-Review

**Spec coverage:**
- ✅ Task 1: ILogger GeracaoDeDadosService (3 Console.WriteLine substituídos)
- ✅ Task 2: ILogger HeroiRepository (4 Console.WriteLine substituídos)
- ✅ Task 3: GuildId para appsettings + DB path relativo
- ✅ Task 4: HeroiGuard + integração no ArenaCommand
- ✅ Task 5: 8 testes unitários HeroiLevelUpService
- ✅ Task 6: CalcularDano público + 6 testes CombatService
- ✅ Task 7: 4 testes CidadeService produção passiva
- ✅ Task 8: 2 testes de integração fragmentos → recrutar

**Avisos:**
- Task 6: `Combatente.Atributos` pode ser `AtributosBase` ou um tipo derivado — verificar antes de escrever. Se `new AtributosBase()` não funcionar diretamente, usar o tipo correto do domínio.
- Task 7: `CidadeBoosterService` pode ter dependências adicionais — verificar construtor antes de instanciar.
- Task 8: `HeroiService` tem muitas dependências. Se `Desbloquear` o chama, a instância `null!` vai explodir. Ler o método `Desbloquear` privado PRIMEIRO.
