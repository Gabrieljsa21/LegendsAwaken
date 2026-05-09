# Torre: Modelo Híbrido de Eventos — Design Spec

**Data:** 2026-05-08  
**Status:** Aprovado — pronto para implementação

---

## Visão Geral

A Torre passa de ticks automáticos com eventos silenciosos para um **loop de exploração híbrido**: tick contínuo para recompensas passivas + checkpoints fixos em 25/50/75/100% para eventos visíveis com escolha real e narrativa. Eventos maiores pausam a exploração até resposta do jogador; eventos menores auto-resolvem com log visível.

---

## Arquitetura Geral

Duas camadas independentes sobre o `ProcessarAsync` existente:

**Camada de Tick (inalterada):** A cada tick (≥ 0.1 min), progressão passiva — ouro, fragmentos, skill events de 20% já existentes — continua sem bloqueio.

**Camada de Checkpoint (nova):** Quando `Progresso` cruza 25/50/75/100%, um `TorreEvento` é gerado:
- **Tier.Maior** → pausa exploração (`StatusExploracao.AguardandoEscolha`), persiste `TorreEvento`, notifica jogador
- **Tier.Menor** → auto-resolve inline, registra em `TorreEventoLog`, exploração continua

**Regra fundamental:** 1 checkpoint por tick máximo. Progresso congela no threshold — bônus de evento nunca ultrapassa o próximo checkpoint não processado.

**Componentes novos:**
- `TorreEvento` — entidade persistida para eventos maiores
- `TorreEventoLog` — tabela leve para eventos menores (ExploracaoId, Texto, CriadoEm)
- `CheckpointEventoConfig` — catálogo de eventos com pesos e filtros
- `TorreEventoService` — geração, resolução, expiração
- `INotificacaoService` / `NotificacaoService` — ping Discord desacoplado
- `UsuarioNotificacao` — preferências por usuário
- `TorreEventoPanel` — painel Discord dedicado para eventos Maiores
- `CustomIdFactory` — helper para IDs de componente Discord

---

## Modelo de Dados

### `TorreEvento`

```csharp
public class TorreEvento
{
    public Guid Id { get; set; }
    public Guid ExploracaoId { get; set; }
    public EventoStatus Status { get; set; }           // Ativo | Resolvido | Expirado | Cancelado
    public TipoEvento Tipo { get; set; }               // BlockingChoice | PassiveEvent | GroupCheck | Encounter | Reward
    public TierEvento Tier { get; set; }               // Menor | Maior
    public string EventoKey { get; set; }              // chave no CheckpointEventoConfig
    public int ProgressoNoCheckpoint { get; set; }     // 25 | 50 | 75 | 100
    public int AndarOrigem { get; set; }               // andar real quando evento foi gerado
    public int EventoSeed { get; set; }                // reprodutibilidade e auditoria
    public string? OpcaoKey { get; set; }              // chave config-driven, null até resolução
    public int ResultadoSchemaVersion { get; set; } = 1; // versão do schema de ResultadoJson
    public string? ResultadoJson { get; set; }         // { titulo, descricao, efeitos[], recompensas[], consequencias[], publico, grauSucesso }
    public string? SnapshotCombatStateJson { get; set; } // [{ heroiId, hp, atributosTemporarios, perks }]
    public DateTime CriadoEm { get; set; }
    public DateTime? ExpiraEm { get; set; }            // null = sem expiração; Tier.Maior DEVE ter valor
    public DateTime? ResolvidoEm { get; set; }
    public DateTime? ProcessadoEm { get; set; }        // quando background job aplicou efeito
    public TorreExploracao Exploracao { get; set; }
}
```

### `TorreEventoLog`

```csharp
public class TorreEventoLog
{
    public Guid Id { get; set; }
    public Guid ExploracaoId { get; set; }
    public string Texto { get; set; }
    public DateTime CriadoEm { get; set; }
}
```

### `UsuarioNotificacao`

```csharp
public class UsuarioNotificacao
{
    public ulong UsuarioId { get; set; }
    public bool NotificacoesAtivas { get; set; }        // default true
    public ulong? CanalPreferido { get; set; }          // null = DM
    public NotificacaoPreferencia Preferencia { get; set; } // Tudo | ApenasEventosMaiores | ApenasConclusao | Desativado
}
```

### Alterações em `TorreExploracao`

- `CheckpointsProcessados` (`CheckpointFlags`) — bitmask; substitui qualquer campo de "último checkpoint"
- `ConsequenceTags` (`string`, JSON de `string[]`) — tags acumuladas por eventos encadeados
- `StatusExploracao` ganha `AguardandoEscolha`

### Enums novos

```csharp
[Flags]
public enum CheckpointFlags
{
    None = 0,
    P25  = 1,
    P50  = 2,
    P75  = 4,
    P100 = 8
}

public enum EventoStatus          { Ativo, Resolvido, Expirado, Cancelado }
public enum TipoEvento            { BlockingChoice, PassiveEvent, GroupCheck, Encounter, Reward }
public enum TierEvento            { Menor, Maior }
public enum NotificacaoPreferencia { Tudo, ApenasEventosMaiores, ApenasConclusao, Desativado }
```

### Nota arquitetural — progresso como abstração

25/50/75/100 são hoje percentuais lineares. O sistema de `CheckpointFlags` já suporta evolução para nodes narrativos discretos (Sala → Corredor → Câmara → Boss) sem refactor — percentual vira representação visual da posição no grafo.

---

## `CheckpointEventoConfig`

```csharp
public record CheckpointEventoConfig(
    string Key,
    TipoEvento Tipo,
    TierEvento Tier,
    EventoRaridade Raridade,    // Comum | Raro | Epico | Unico
    bool TemImpactoMecanico,    // false = evento de lore/ambientação puro
    string Titulo,
    string Descricao,
    OpcaoConfig[]? Opcoes,      // null para PassiveEvent/Reward
    Pericia? Pericia,           // para GroupCheck
    int? DC,
    int Peso,
    int MinAndar,
    int MaxAndar,
    string[] Tags,
    string[] Biomas,
    string[]? Requisitos,       // ex: ["has_ranger", "ConsequenceTag:trilha_aberta"]
    string[]? ConsequenceTags   // tags adicionadas à exploração ao resolver este evento
);

public enum EventoRaridade { Comum, Raro, Epico, Unico }

public record OpcaoConfig(
    string Key,
    string TextoExibido,
    RiscoTom RiscoTom           // Seguro | Arriscado | Neutro
);

public enum RiscoTom { Seguro, Arriscado, Neutro }
```

**Efeitos nunca são exibidos no painel de escolha.** Apenas `TextoExibido` e `RiscoTom` são visíveis ao jogador.

### Catálogo de Launch (6 eventos)

| Key | Tipo | Tier | Raridade | TemImpactoMecanico | Andar | Descrição |
|---|---|---|---|---|---|---|
| `encruzilhada_mercador` | BlockingChoice | Maior | Comum | true | 1–15 | Mercador bloqueia caminho — pagar, forçar ou recuar |
| `trilha_oculta` | BlockingChoice | Maior | Comum | true | 5–15 | Passagem secreta detectada — explorar ou ignorar |
| `chuva_de_fragmentos` | Reward | Menor | Comum | true | 1–15 | Câmara abandonada com fragmentos — auto-resolve |
| `armadilha_detectada` | PassiveEvent | Menor | Comum | false | 1–15 | Herói detecta armadilha — evita dano, lore log |
| `teste_forca_porta` | GroupCheck | Maior | Comum | true | 3–15 | Porta selada — DC 14 STR, resolução parcial, progresso ±5/10% |
| `sombra_perseguindo` | Encounter | Maior | Comum | true | 8–15 | Presença hostil — fugir (DEX) ou enfrentar (STR), resolução parcial |

Todos com `Peso = 10`, `Tags = []`, `Biomas = []` na v1. `teste_forca_porta` e `sombra_perseguindo` implementam resolução parcial (3 graus).

---

## Fluxo de Processamento (`ProcessarAsync`)

```
1. Carrega TorreExploracao (RowVersion incluído para optimistic concurrency)
2. Se Status == AguardandoEscolha → return (tick bloqueado)
3. Se Status != Ativa → return
4. Verifica debounce (≥ 0.1 min) → return se muito cedo

5. [Pré-tick: calcular teto de progresso]
   próximoThreshold = menor de [25,50,75,100] NOT IN CheckpointsProcessados
   Se próximoThreshold existe:
     progressoMaxEsseTick = min(progressoGanho, próximoThreshold - Progresso)
   Senão:
     progressoMaxEsseTick = progressoGanho

6. [Tick layer — inalterado]
   Aplica progressoMaxEsseTick, calcula ouro/fragmentos proporcionais
   Atualiza Progresso, skill events 20% existentes

7. [Checkpoint layer]
   Se Progresso == próximoThreshold:
     Marca flag em CheckpointsProcessados
     seed = HashCode.Combine(Seed, próximoThreshold, AndarAtual)
     rng = new EventoRng(seed)   // wrapper determinístico sobre Random
     Sorteia evento via CheckpointEventoConfig(rng, andar, bioma, ConsequenceTags)
     
     Se Tier == Maior:
       Cria TorreEvento { ExpiraEm = now + 7d, SnapshotCombatStateJson, ... }
       Seta Status = AguardandoEscolha
       Dispara INotificacaoService.NotificarEventoCheckpointAsync(usuarioId, evento)
       Salva com RowVersion check → return
     
     Senão (Menor):
       Resolve inline, aplica efeito (respeitando cap do próximo checkpoint)
       Cria TorreEventoLog { Texto = resultadoFormatado }
       → return  ← 1 checkpoint por tick

8. Se Progresso == 100 e Status == Ativa → conclui normalmente
9. Salva com RowVersion check
```

**Regra de economia:** `TorreEventoService.AplicarEfeito` clipa bônus de progresso em `min(bonus, proximoThreshold - Progresso)` antes de somar.

---

## Resolução Parcial

Eventos do tipo `GroupCheck` e `Encounter` suportam **três graus de sucesso**, não apenas binário:

| Grau | Condição | Consequência típica |
|---|---|---|
| `SucessoTotal` | Roll >= DC por ≥ 3 | Efeito positivo máximo |
| `SucessoParcial` | Roll >= DC por < 3 | Efeito positivo reduzido + custo colateral |
| `Falha` | Roll < DC | Efeito negativo ou neutro |

Exemplo — `teste_forca_porta`:
- SucessoTotal: +10% progresso, porta aberta sem custo
- SucessoParcial: +5% progresso, herói mais fraco sofre -20% HP temporário
- Falha: sem progresso bônus, party sofre -10% HP temporário

`ResultadoJson.grauSucesso` armazena o grau para exibição e analytics. O `CheckpointEventoConfig` define os 3 efeitos por evento que suportar parcial — eventos simples (Reward, PassiveEvent) usam apenas `SucessoTotal`.

---

## Retenção de Dados (Política v1)

Política explícita, mesmo que conservadora na v1:

- `TorreEvento` — mantido indefinidamente (log de auditoria + replay)
- `TorreEventoLog` — mantido indefinidamente (volume baixo na v1)
- `SnapshotCombatStateJson` — mantido junto ao `TorreEvento`; purge futuro possível após 90 dias pós-resolução
- `UsuarioNotificacao` — mantido enquanto usuário existir

**v1: sem purge automático.** Revisitar se `TorreEventoLog` crescer além de 100k registros por usuário.

---

## `EventoRng` — Wrapper Determinístico

```csharp
public sealed class EventoRng
{
    private readonly Random _rng;

    public EventoRng(int seed) => _rng = new Random(seed);

    public int Next(int min, int max) => _rng.Next(min, max);
    public double NextDouble() => _rng.NextDouble();
    public T Choose<T>(IList<T> items) => items[_rng.Next(items.Count)];
}
```

`EventoSeed` na entidade armazena o `HashCode.Combine(exploracao.Seed, threshold, andarAtual)` para reprodutibilidade total. O mesmo seed em qualquer runtime produz o mesmo evento sorteado.

---

## Anti-Softlock

Requisito não-funcional. O sistema deve garantir:

1. Todo evento `Tier.Maior` **deve** ter `ExpiraEm` não-nulo (validado em `GerarEventoAsync`)
2. Background job diário varre eventos com `Status == Ativo` e `ExpiraEm < now` → auto-resolve com `OpcaoKey = "expirado"`, consequência neutra, retoma `StatusExploracao.Ativa`
3. Startup recovery: ao iniciar, `TorreEventoService.RecuperarExpiradosAsync()` executa antes do primeiro tick
4. Nunca existe caminho de código que deixa `AguardandoEscolha` sem evento Ativo correspondente

---

## Sistema de Notificação

`INotificacaoService.NotificarEventoCheckpointAsync(ulong usuarioId, TorreEvento evento)`:

1. Lê `UsuarioNotificacao` do usuário
2. Se `NotificacoesAtivas == false` ou `Preferencia == Desativado` → return
3. Resolve canal: `CanalPreferido ?? DM`
4. Posta mensagem:
   ```
   ⚠️ Exploração pausada — Checkpoint {X}%
   Torre — Andar {Y} | 🗼 {PartyName}

   {evento.Titulo}
   Use /torre para ver suas opções.
   ```
5. Se DM falhar (usuário bloqueou) → loga warning, não lança exceção

Exploração **não depende** de entrega da notificação para pausar.

---

## UI do Bot

### `TorreEventoPanel.CriarEmbedEscolha(TorreEvento evento)`

- Embed com título narrativo, descrição do evento, checkpoint e andar
- Footer: `⏳ Expira em {diasRestantes}d {horasRestantes}h`
- **Efeitos não exibidos** — apenas texto narrativo e opções com `RiscoTom`
- Botões via `CustomIdFactory.EventoEscolha(eventoId, opcaoKey)` — centraliza IDs

### `InteractionRouter` — handler `torre_evento_escolha`

1. Parseia `eventoId` e `opcaoKey` do custom ID
2. **Valida ownership**: `if (evento.Exploracao.UsuarioId != interaction.User.Id)` → responde efêmero "Você não pode responder este evento."
3. Valida `opcaoKey` contra `CheckpointEventoConfig.Opcoes` — rejeita chaves inválidas
4. Chama `TorreEventoService.ResolverAsync(eventoId, opcaoKey)`
5. Edita mensagem original: embed com resultado + **botões desabilitados** (`IsDisabled = true`)
6. Resultado exibe: título, texto narrativo, efeitos mecânicos (agora visíveis), barra de progresso atualizada

### `/torre` com evento ativo

`TorreCommand.ExibirStatusAsync` detecta `Status == AguardandoEscolha` e renderiza `TorreEventoPanel.CriarEmbedEscolha` em vez do painel normal.

### `/torre notificacoes`

Toggle `NotificacoesAtivas`. UI expõe on/off; modelo já suporta granularidade futura via `NotificacaoPreferencia`.

---

## Serviços

### `TorreEventoService`

- `GerarEventoAsync(TorreExploracao, int threshold, int andarAtual, Random rng)` — sorteia evento do catálogo, valida `ExpiraEm`, cria `TorreEvento`
- `ResolverAsync(Guid eventoId, string opcaoKey)` — aplica efeito, preenche `ResultadoJson`, seta `ResolvidoEm`, retoma `StatusExploracao.Ativa`
- `RecuperarExpiradosAsync()` — varre eventos expirados, auto-resolve neutro
- `ObterEventoAtivoAsync(Guid exploracaoId)` — retorna `TorreEvento` com `Status == Ativo`, ou null

### `NotificacaoService`

Implementa `INotificacaoService`. Injetado em `TorreExploracaoService`. Discord client resolvido via DI.

---

## Migration

Uma migration `TorreCheckpointEventos`:
- Cria tabela `TorreEventos` 
- Cria tabela `TorreEventoLogs`
- Cria tabela `UsuariosNotificacao`
- Adiciona coluna `CheckpointsProcessados` (int) em `TorreExploracoes`
- Adiciona coluna `ConsequenceTags` (text, nullable) em `TorreExploracoes`
- Adiciona valor `AguardandoEscolha` ao enum `StatusExploracao` (coluna int — sem migration de enum em SQLite)
- Seed: todos os usuários existentes em `UsuariosNotificacao` com `NotificacoesAtivas = true`

---

## Estratégia de Testes

**Unidade — `TorreEventoService`:**
```
GerarEventoAsync_RetornaEventoCorreto_ParaAndarEThreshold
GerarEventoAsync_LancaException_SeSemExpiraEmParaMaior
ResolverAsync_AplicaEfeito_ERetomandoExploracaoAtiva
ResolverAsync_LancaException_SeEventoNaoAtivo
ResolverAsync_LancaException_SeOpcaoKeyInvalida
ResolverAsync_NaoUltrapassaProximoCheckpoint_ComBonusProgresso
GerarEvento_ComMesmoSeed_ProduceMesmoResultado
```

**Unidade — `ProcessarAsync` com checkpoint:**
```
ProcessarAsync_CongelaProgresso_NoThreshold
ProcessarAsync_NaoProcessa_QuandoAguardandoEscolha
ProcessarAsync_NaoDuplicaCheckpoint_ComBitmask
ProcessarAsync_ProcessaUmCheckpointPorTick_MesmoComMultiplosThresholds
ProcessarAsync_EventoMenor_NaoPersisteTorreEvento_UsaLog
```

**Integração:**
```
ExploracaoCompleta_GeraCheckpoints_EmSequencia
EventoMaior_PausaExploracao_ENotificaUsuario
EventoMenor_AutoResolve_ESalvaLog
Expiracao_AutoResolveNeutro_ERetomandoExploracao
Handler_RejeicaoOwnership_QuandoUsuarioDiferente
RecuperarExpirados_NaoLancaException_SemEventos
```
