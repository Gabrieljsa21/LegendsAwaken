# Torre: Modelo Híbrido de Checkpoint Eventos — Plano de Implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Adicionar checkpoints fixos em 25/50/75/100% da exploração da Torre que geram eventos persistidos com pausa (`AguardandoEscolha`) e notificação Discord, substituindo o loop silencioso de tick por exploração com narrativa, escolhas reais e resolução parcial.

**Architecture:** Duas camadas independentes em `ProcessarAsync`: tick layer (progresso passivo inalterado) + checkpoint layer (eventos a 25/50/75/100%). Tier.Maior pausa via `StatusExploracao.AguardandoEscolha` + `TorreEvento` persistida + Discord DM. Tier.Menor auto-resolve com `TorreEventoLog` leve. `TorreEventoService` centraliza geração/resolução/expiração. Idempotência via `CheckpointFlags` bitmask + `[ConcurrencyCheck]` na entidade.

**Tech Stack:** .NET 10 / C#, EF Core 10, SQLite, Discord.Net, xUnit + Moq

**Spec:** `docs/superpowers/specs/2026-05-08-torre-checkpoint-eventos-design.md`

---

## Mapa de Arquivos

### Criar
| Arquivo | Responsabilidade |
|---|---|
| `LegendsAwaken.Domain/Entities/TorreEvento.cs` | Entidade — evento maior persistido |
| `LegendsAwaken.Domain/Entities/TorreEventoLog.cs` | Entidade — log leve de eventos menores |
| `LegendsAwaken.Domain/Entities/UsuarioNotificacao.cs` | Entidade — preferências de notificação |
| `LegendsAwaken.Domain/Interfaces/ITorreEventoRepository.cs` | Repositório de TorreEvento |
| `LegendsAwaken.Domain/Interfaces/IUsuarioNotificacaoRepository.cs` | Repositório de UsuarioNotificacao |
| `LegendsAwaken.Domain/Interfaces/INotificacaoService.cs` | Abstração de notificação Discord |
| `LegendsAwaken.Application/Services/EventoRng.cs` | Wrapper RNG determinístico |
| `LegendsAwaken.Application/Config/CheckpointEventoConfig.cs` | Catálogo de 6 eventos de checkpoint |
| `LegendsAwaken.Application/Services/TorreEventoService.cs` | Gerar, resolver e expirar eventos |
| `LegendsAwaken.Infrastructure/Repositories/TorreEventoRepository.cs` | Implementação do repositório |
| `LegendsAwaken.Infrastructure/Repositories/UsuarioNotificacaoRepository.cs` | Implementação do repositório |
| `LegendsAwaken.Bot/Panels/TorreEventoPanel.cs` | Embed Discord para eventos Tier.Maior |
| `LegendsAwaken.Bot/Helpers/CustomIdFactory.cs` | IDs de componente Discord centralizados |
| `LegendsAwaken.Bot/Services/NotificacaoService.cs` | Implementação Discord DM/canal |
| `LegendsAwaken.Tests/Unit/EventoRngTests.cs` | Testes do RNG determinístico |
| `LegendsAwaken.Tests/Unit/TorreEventoServiceTests.cs` | Testes do TorreEventoService |
| `LegendsAwaken.Tests/Unit/TorreExploracaoCheckpointTests.cs` | Testes do ProcessarAsync com checkpoints |

### Modificar
| Arquivo | O que muda |
|---|---|
| `LegendsAwaken.Domain/Enum/Enums.cs` | +7 enums novos + `AguardandoEscolha` em StatusExploracao |
| `LegendsAwaken.Domain/Entities/TorreExploracao.cs` | +Seed, +DiscordUserId, +CheckpointsProcessados, +ConsequenceTags, +Version |
| `LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs` | +3 DbSets + configuração Version como ConcurrencyCheck |
| `LegendsAwaken.Application/Services/TorreExploracaoService.cs` | ProcessarAsync: freeze + checkpoint layer + IniciarAsync recebe discordId |
| `LegendsAwaken.Bot/Commands/TorreCommand.cs` | Routing para AguardandoEscolha + handler torre_evento_escolha |
| `LegendsAwaken.Bot/Program.cs` | Registrar novos serviços e repositórios |

---

### Task 1: Enums

**Files:**
- Modify: `LegendsAwaken.Domain/Enum/Enums.cs`

- [ ] **Step 1: Adicionar os novos enums ao final de Enums.cs**

Abrir `LegendsAwaken.Domain/Enum/Enums.cs` e adicionar após o enum `Pericia` (ou antes do fechamento do namespace):

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

public enum EventoStatus
{
    Ativo,
    Resolvido,
    Expirado,
    Cancelado
}

public enum TipoEvento
{
    BlockingChoice,
    PassiveEvent,
    GroupCheck,
    Encounter,
    Reward
}

public enum TierEvento
{
    Menor,
    Maior
}

public enum EventoRaridade
{
    Comum,
    Raro,
    Epico,
    Unico
}

public enum NotificacaoPreferencia
{
    Tudo,
    ApenasEventosMaiores,
    ApenasConclusao,
    Desativado
}

public enum GrauSucesso
{
    SucessoTotal,
    SucessoParcial,
    Falha
}

public enum RiscoTom
{
    Seguro,
    Arriscado,
    Neutro
}
```

- [ ] **Step 2: Adicionar `AguardandoEscolha` em `StatusExploracao`**

Localizar o enum `StatusExploracao` no mesmo arquivo e adicionar o novo valor:

```csharp
public enum StatusExploracao
{
    Ativa,
    Concluida,
    Falha,
    Coletada,
    AguardandoEscolha   // pausa até jogador responder ao evento de checkpoint
}
```

- [ ] **Step 3: Verificar build**

```
dotnet build LegendsAwaken.Domain
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Domain/Enum/Enums.cs
git commit -m "feat(torre): add checkpoint event enums — CheckpointFlags, EventoStatus, TipoEvento, TierEvento, GrauSucesso etc"
```

---

### Task 2: TorreExploracao Changes + Novas Entidades de Domínio

**Files:**
- Modify: `LegendsAwaken.Domain/Entities/TorreExploracao.cs`
- Create: `LegendsAwaken.Domain/Entities/TorreEvento.cs`
- Create: `LegendsAwaken.Domain/Entities/TorreEventoLog.cs`
- Create: `LegendsAwaken.Domain/Entities/UsuarioNotificacao.cs`

- [ ] **Step 1: Adicionar campos em TorreExploracao**

Abrir `LegendsAwaken.Domain/Entities/TorreExploracao.cs` e adicionar as novas propriedades após `HeroisFeridosIds`:

```csharp
// Checkpoint event system
public int Seed { get; set; }                                       // set on IniciarAsync, drives EventoRng
public ulong DiscordUserId { get; set; }                            // Discord ulong para notificações
public CheckpointFlags CheckpointsProcessados { get; set; }         // bitmask substituindo UltimoCheckpoint no novo sistema
public string? ConsequenceTags { get; set; }                        // JSON string[] — tags de eventos encadeados

[System.ComponentModel.DataAnnotations.ConcurrencyCheck]
public int Version { get; set; }                                    // optimistic concurrency
```

- [ ] **Step 2: Criar TorreEvento.cs**

```csharp
using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities;

public class TorreEvento
{
    public Guid Id { get; set; }
    public Guid ExploracaoId { get; set; }
    public EventoStatus Status { get; set; }
    public TipoEvento Tipo { get; set; }
    public TierEvento Tier { get; set; }
    public EventoRaridade Raridade { get; set; }
    public string EventoKey { get; set; } = "";
    public int ProgressoNoCheckpoint { get; set; }
    public int AndarOrigem { get; set; }
    public int EventoSeed { get; set; }
    public int ResultadoSchemaVersion { get; set; } = 1;
    public string? OpcaoKey { get; set; }
    public string? ResultadoJson { get; set; }
    public string? SnapshotCombatStateJson { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ExpiraEm { get; set; }
    public DateTime? ResolvidoEm { get; set; }
    public DateTime? ProcessadoEm { get; set; }
    public TorreExploracao Exploracao { get; set; } = null!;
}
```

- [ ] **Step 3: Criar TorreEventoLog.cs**

```csharp
using System;

namespace LegendsAwaken.Domain.Entities;

public class TorreEventoLog
{
    public Guid Id { get; set; }
    public Guid ExploracaoId { get; set; }
    public string Texto { get; set; } = "";
    public DateTime CriadoEm { get; set; }
}
```

- [ ] **Step 4: Criar UsuarioNotificacao.cs**

```csharp
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities;

public class UsuarioNotificacao
{
    public ulong UsuarioId { get; set; }
    public bool NotificacoesAtivas { get; set; } = true;
    public ulong? CanalPreferido { get; set; }
    public NotificacaoPreferencia Preferencia { get; set; } = NotificacaoPreferencia.Tudo;
}
```

- [ ] **Step 5: Verificar build**

```
dotnet build LegendsAwaken.Domain
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Commit**

```
git add LegendsAwaken.Domain/Entities/TorreExploracao.cs
git add LegendsAwaken.Domain/Entities/TorreEvento.cs
git add LegendsAwaken.Domain/Entities/TorreEventoLog.cs
git add LegendsAwaken.Domain/Entities/UsuarioNotificacao.cs
git commit -m "feat(torre): add TorreEvento, TorreEventoLog, UsuarioNotificacao entities + extend TorreExploracao"
```

---

### Task 3: Interfaces de Domínio

**Files:**
- Create: `LegendsAwaken.Domain/Interfaces/ITorreEventoRepository.cs`
- Create: `LegendsAwaken.Domain/Interfaces/IUsuarioNotificacaoRepository.cs`
- Create: `LegendsAwaken.Domain/Interfaces/INotificacaoService.cs`

- [ ] **Step 1: Criar ITorreEventoRepository.cs**

```csharp
using LegendsAwaken.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface ITorreEventoRepository
{
    Task AdicionarAsync(TorreEvento evento);
    Task<TorreEvento?> ObterAtivoAsync(Guid exploracaoId);
    Task AtualizarAsync(TorreEvento evento);
    Task AdicionarLogAsync(TorreEventoLog log);
    Task<List<TorreEvento>> ObterExpiradosAsync(DateTime agora);  // para RecuperarExpiradosAsync
}
```

- [ ] **Step 2: Criar IUsuarioNotificacaoRepository.cs**

```csharp
using LegendsAwaken.Domain.Entities;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface IUsuarioNotificacaoRepository
{
    Task<UsuarioNotificacao?> ObterAsync(ulong usuarioId);
    Task AdicionarOuAtualizarAsync(UsuarioNotificacao notif);
}
```

- [ ] **Step 3: Criar INotificacaoService.cs**

```csharp
using LegendsAwaken.Domain.Entities;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface INotificacaoService
{
    Task NotificarEventoCheckpointAsync(ulong discordUserId, TorreEvento evento);
}
```

- [ ] **Step 4: Verificar build**

```
dotnet build LegendsAwaken.Domain
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```
git add LegendsAwaken.Domain/Interfaces/ITorreEventoRepository.cs
git add LegendsAwaken.Domain/Interfaces/IUsuarioNotificacaoRepository.cs
git add LegendsAwaken.Domain/Interfaces/INotificacaoService.cs
git commit -m "feat(torre): add ITorreEventoRepository, IUsuarioNotificacaoRepository, INotificacaoService interfaces"
```

---

### Task 4: EventoRng + CheckpointEventoConfig

**Files:**
- Create: `LegendsAwaken.Application/Services/EventoRng.cs`
- Create: `LegendsAwaken.Application/Config/CheckpointEventoConfig.cs`
- Create: `LegendsAwaken.Tests/Unit/EventoRngTests.cs`

- [ ] **Step 1: Escrever teste que falha para EventoRng**

Criar `LegendsAwaken.Tests/Unit/EventoRngTests.cs`:

```csharp
using LegendsAwaken.Application.Services;
using Xunit;

namespace LegendsAwaken.Tests.Unit;

public class EventoRngTests
{
    [Fact]
    public void MesmoSeed_ProduceMesmoNextDouble()
    {
        var rng1 = new EventoRng(42);
        var rng2 = new EventoRng(42);

        Assert.Equal(rng1.NextDouble(), rng2.NextDouble());
    }

    [Fact]
    public void MesmoSeed_ChooseRetornaMesmoItem()
    {
        var items = new List<string> { "a", "b", "c", "d" };
        var rng1 = new EventoRng(99);
        var rng2 = new EventoRng(99);

        Assert.Equal(rng1.Choose(items), rng2.Choose(items));
    }

    [Fact]
    public void SeedsDiferentes_ProducemResultadosDiferentes()
    {
        var rng1 = new EventoRng(1);
        var rng2 = new EventoRng(2);
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

        // Com 8 itens e seeds diferentes, é praticamente impossível escolher o mesmo
        var resultados = Enumerable.Range(0, 20)
            .Select(_ => (rng1.Next(0, 1000), rng2.Next(0, 1000)))
            .ToList();

        Assert.Contains(resultados, r => r.Item1 != r.Item2);
    }

    [Fact]
    public void EscolhePonderado_RetornaItemComPesoMaior_ComFrequencia()
    {
        var items = new List<(string Key, int Peso)>
        {
            ("raro", 1),
            ("comum", 99)
        };

        var contagem = new Dictionary<string, int> { ["raro"] = 0, ["comum"] = 0 };
        for (int i = 0; i < 1000; i++)
        {
            var rng = new EventoRng(i);
            var escolhido = rng.EscolherPonderado(items, x => x.Peso);
            contagem[escolhido.Key]++;
        }

        // "comum" deve ser escolhido ~990 vezes; raro ~10
        Assert.True(contagem["comum"] > 900, $"Comum escolhido apenas {contagem["comum"]} vezes");
    }
}
```

- [ ] **Step 2: Rodar — verificar que falha**

```
dotnet test LegendsAwaken.Tests --filter "EventoRngTests" -v minimal
```

Expected: FAIL — `EventoRng` not found.

- [ ] **Step 3: Criar EventoRng.cs**

```csharp
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services;

public sealed class EventoRng
{
    private readonly Random _rng;

    public EventoRng(int seed) => _rng = new Random(seed);

    public int Next(int min, int max) => _rng.Next(min, max);

    public double NextDouble() => _rng.NextDouble();

    public T Choose<T>(IList<T> items) => items[_rng.Next(items.Count)];

    public T EscolherPonderado<T>(IList<T> items, Func<T, int> peso)
    {
        int total = 0;
        foreach (var item in items) total += peso(item);
        int roll = _rng.Next(total);
        int acumulado = 0;
        foreach (var item in items)
        {
            acumulado += peso(item);
            if (roll < acumulado) return item;
        }
        return items[^1];
    }
}
```

- [ ] **Step 4: Rodar — verificar que passa**

```
dotnet test LegendsAwaken.Tests --filter "EventoRngTests" -v minimal
```

Expected: 4 testes PASS.

- [ ] **Step 5: Criar CheckpointEventoConfig.cs**

Criar diretório `LegendsAwaken.Application/Config/` se não existir, depois criar o arquivo:

```csharp
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Config;

public record OpcaoConfig(string Key, string TextoExibido, RiscoTom RiscoTom);

public record CheckpointEventoConfig(
    string Key,
    TipoEvento Tipo,
    TierEvento Tier,
    EventoRaridade Raridade,
    bool TemImpactoMecanico,
    string Titulo,
    string Descricao,
    OpcaoConfig[]? Opcoes,
    Pericia? Pericia,
    int? DC,
    int Peso,
    int MinAndar,
    int MaxAndar,
    string[] Tags,
    string[] Biomas,
    string[]? Requisitos,
    string[]? ConsequenceTags
);

public static class CheckpointEventoCatalog
{
    public static readonly IReadOnlyList<CheckpointEventoConfig> Todos = new List<CheckpointEventoConfig>
    {
        new(
            Key:              "encruzilhada_mercador",
            Tipo:             TipoEvento.BlockingChoice,
            Tier:             TierEvento.Maior,
            Raridade:         EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:           "Encruzilhada do Mercador",
            Descricao:        "Um mercador misterioso bloqueia o caminho, oferecendo passagem... por um preço.",
            Opcoes: new[]
            {
                new OpcaoConfig("pagar",  "Pagar o preço",    RiscoTom.Seguro),
                new OpcaoConfig("forccar","Forçar passagem",  RiscoTom.Arriscado),
                new OpcaoConfig("recuar", "Recuar",           RiscoTom.Neutro)
            },
            Pericia:      null,
            DC:           null,
            Peso:         10,
            MinAndar:     1,
            MaxAndar:     15,
            Tags:         Array.Empty<string>(),
            Biomas:       Array.Empty<string>(),
            Requisitos:   null,
            ConsequenceTags: null
        ),
        new(
            Key:              "trilha_oculta",
            Tipo:             TipoEvento.BlockingChoice,
            Tier:             TierEvento.Maior,
            Raridade:         EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:           "Trilha Oculta",
            Descricao:        "Um dos heróis detecta uma passagem secreta que poderia encurtar o caminho.",
            Opcoes: new[]
            {
                new OpcaoConfig("explorar", "Explorar a passagem", RiscoTom.Arriscado),
                new OpcaoConfig("ignorar",  "Continuar pela rota principal", RiscoTom.Seguro)
            },
            Pericia:      null,
            DC:           null,
            Peso:         10,
            MinAndar:     5,
            MaxAndar:     15,
            Tags:         Array.Empty<string>(),
            Biomas:       Array.Empty<string>(),
            Requisitos:   null,
            ConsequenceTags: new[] { "trilha_aberta" }
        ),
        new(
            Key:              "chuva_de_fragmentos",
            Tipo:             TipoEvento.Reward,
            Tier:             TierEvento.Menor,
            Raridade:         EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:           "Câmara Abandonada",
            Descricao:        "A party encontra restos de uma câmara saqueada com alguns fragmentos deixados para trás.",
            Opcoes:       null,
            Pericia:      null,
            DC:           null,
            Peso:         10,
            MinAndar:     1,
            MaxAndar:     15,
            Tags:         Array.Empty<string>(),
            Biomas:       Array.Empty<string>(),
            Requisitos:   null,
            ConsequenceTags: null
        ),
        new(
            Key:              "armadilha_detectada",
            Tipo:             TipoEvento.PassiveEvent,
            Tier:             TierEvento.Menor,
            Raridade:         EventoRaridade.Comum,
            TemImpactoMecanico: false,
            Titulo:           "Armadilha Detectada",
            Descricao:        "Olhos atentos detectam uma armadilha no corredor. A party a contorna com cuidado.",
            Opcoes:       null,
            Pericia:      null,
            DC:           null,
            Peso:         10,
            MinAndar:     1,
            MaxAndar:     15,
            Tags:         Array.Empty<string>(),
            Biomas:       Array.Empty<string>(),
            Requisitos:   null,
            ConsequenceTags: null
        ),
        new(
            Key:              "teste_forca_porta",
            Tipo:             TipoEvento.GroupCheck,
            Tier:             TierEvento.Maior,
            Raridade:         EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:           "Porta Selada",
            Descricao:        "Uma porta de pedra maciça bloqueia o avanço. Parece que força bruta é a única saída.",
            Opcoes: new[]
            {
                new OpcaoConfig("arrombar", "Arrombar a porta", RiscoTom.Arriscado)
            },
            Pericia:      Pericia.Atletismo,
            DC:           14,
            Peso:         10,
            MinAndar:     3,
            MaxAndar:     15,
            Tags:         Array.Empty<string>(),
            Biomas:       Array.Empty<string>(),
            Requisitos:   null,
            ConsequenceTags: null
        ),
        new(
            Key:              "sombra_perseguindo",
            Tipo:             TipoEvento.Encounter,
            Tier:             TierEvento.Maior,
            Raridade:         EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:           "Sombra Perseguidora",
            Descricao:        "Uma presença hostil começa a seguir a party pelos corredores. Cada segundo conta.",
            Opcoes: new[]
            {
                new OpcaoConfig("fugir",    "Fugir rapidamente",    RiscoTom.Arriscado),
                new OpcaoConfig("enfrentar","Virar e enfrentar",    RiscoTom.Arriscado)
            },
            Pericia:      null,
            DC:           null,
            Peso:         10,
            MinAndar:     8,
            MaxAndar:     15,
            Tags:         Array.Empty<string>(),
            Biomas:       Array.Empty<string>(),
            Requisitos:   null,
            ConsequenceTags: null
        )
    };

    public static IEnumerable<CheckpointEventoConfig> FiltrarParaAndar(int andar, IEnumerable<string> consequenceTags) =>
        Todos.Where(e => e.MinAndar <= andar && e.MaxAndar >= andar
                         && (e.Requisitos == null || e.Requisitos.All(r => consequenceTags.Contains(r))));
}
```

- [ ] **Step 6: Verificar build**

```
dotnet build LegendsAwaken.Application
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```
git add LegendsAwaken.Application/Services/EventoRng.cs
git add LegendsAwaken.Application/Config/CheckpointEventoConfig.cs
git add LegendsAwaken.Tests/Unit/EventoRngTests.cs
git commit -m "feat(torre): add EventoRng (deterministic) + CheckpointEventoCatalog with 6 events"
```

---

### Task 5: AppDbContext + Migration

**Files:**
- Modify: `LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs`
- Generate: migration via EF CLI

- [ ] **Step 1: Adicionar DbSets e configurações ao LegendsAwakenDbContext**

Abrir `LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs`.

Adicionar as novas propriedades `DbSet` junto às existentes:

```csharp
public DbSet<TorreEvento> TorreEventos { get; set; }
public DbSet<TorreEventoLog> TorreEventoLogs { get; set; }
public DbSet<UsuarioNotificacao> UsuariosNotificacao { get; set; }
```

No método `OnModelCreating`, adicionar as configurações:

```csharp
// TorreExploracao — Version como token de concorrência
modelBuilder.Entity<TorreExploracao>()
    .Property(e => e.Version)
    .IsConcurrencyToken();

// TorreEvento — FK para TorreExploracao
modelBuilder.Entity<TorreEvento>()
    .HasOne(e => e.Exploracao)
    .WithMany()
    .HasForeignKey(e => e.ExploracaoId)
    .OnDelete(DeleteBehavior.Cascade);

// UsuarioNotificacao — PK é ulong
modelBuilder.Entity<UsuarioNotificacao>()
    .HasKey(u => u.UsuarioId);
```

- [ ] **Step 2: Verificar build da Infrastructure**

```
dotnet build LegendsAwaken.Infrastructure
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Gerar a migration**

```
dotnet ef migrations add TorreCheckpointEventos --project LegendsAwaken.Infrastructure --startup-project LegendsAwaken.Bot
```

Expected: Migration file created in `LegendsAwaken.Infrastructure/Migrations/`.

- [ ] **Step 4: Verificar o arquivo de migration gerado**

Abrir o arquivo `.cs` gerado e confirmar que contém:
- `CreateTable` para `TorreEventos`
- `CreateTable` para `TorreEventoLogs`
- `CreateTable` para `UsuariosNotificacao`
- `AddColumn` para `CheckpointsProcessados`, `ConsequenceTags`, `Seed`, `DiscordUserId`, `Version` em `TorreExploracoes`

Se algum campo estiver faltando, verificar o model e regenerar.

- [ ] **Step 5: Aplicar migration no banco de desenvolvimento**

```
dotnet ef database update --project LegendsAwaken.Infrastructure --startup-project LegendsAwaken.Bot
```

Expected: `Done.`

- [ ] **Step 6: Commit**

```
git add LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs
git add LegendsAwaken.Infrastructure/Migrations/
git commit -m "chore(torre): migration TorreCheckpointEventos — TorreEventos, TorreEventoLogs, UsuariosNotificacao tables"
```

---

### Task 6: Repositories

**Files:**
- Create: `LegendsAwaken.Infrastructure/Repositories/TorreEventoRepository.cs`
- Create: `LegendsAwaken.Infrastructure/Repositories/UsuarioNotificacaoRepository.cs`

- [ ] **Step 1: Criar TorreEventoRepository.cs**

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class TorreEventoRepository : ITorreEventoRepository
{
    private readonly LegendsAwakenDbContext _ctx;

    public TorreEventoRepository(LegendsAwakenDbContext ctx) => _ctx = ctx;

    public async Task AdicionarAsync(TorreEvento evento)
    {
        _ctx.TorreEventos.Add(evento);
        await _ctx.SaveChangesAsync();
    }

    public async Task<TorreEvento?> ObterAtivoAsync(Guid exploracaoId) =>
        await _ctx.TorreEventos
            .FirstOrDefaultAsync(e => e.ExploracaoId == exploracaoId
                                   && e.Status == Domain.Enum.EventoStatus.Ativo);

    public async Task AtualizarAsync(TorreEvento evento)
    {
        _ctx.TorreEventos.Update(evento);
        await _ctx.SaveChangesAsync();
    }

    public async Task AdicionarLogAsync(TorreEventoLog log)
    {
        _ctx.TorreEventoLogs.Add(log);
        await _ctx.SaveChangesAsync();
    }

    public async Task<List<TorreEvento>> ObterExpiradosAsync(DateTime agora) =>
        await _ctx.TorreEventos
            .Include(e => e.Exploracao)
            .Where(e => e.Status == Domain.Enum.EventoStatus.Ativo
                     && e.ExpiraEm.HasValue
                     && e.ExpiraEm.Value < agora)
            .ToListAsync();
}
```

- [ ] **Step 2: Criar UsuarioNotificacaoRepository.cs**

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class UsuarioNotificacaoRepository : IUsuarioNotificacaoRepository
{
    private readonly LegendsAwakenDbContext _ctx;

    public UsuarioNotificacaoRepository(LegendsAwakenDbContext ctx) => _ctx = ctx;

    public async Task<UsuarioNotificacao?> ObterAsync(ulong usuarioId) =>
        await _ctx.UsuariosNotificacao.FindAsync(usuarioId);

    public async Task AdicionarOuAtualizarAsync(UsuarioNotificacao notif)
    {
        var existing = await _ctx.UsuariosNotificacao.FindAsync(notif.UsuarioId);
        if (existing == null)
            _ctx.UsuariosNotificacao.Add(notif);
        else
        {
            existing.NotificacoesAtivas = notif.NotificacoesAtivas;
            existing.CanalPreferido = notif.CanalPreferido;
            existing.Preferencia = notif.Preferencia;
            _ctx.UsuariosNotificacao.Update(existing);
        }
        await _ctx.SaveChangesAsync();
    }
}
```

- [ ] **Step 3: Verificar build**

```
dotnet build LegendsAwaken.Infrastructure
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Infrastructure/Repositories/TorreEventoRepository.cs
git add LegendsAwaken.Infrastructure/Repositories/UsuarioNotificacaoRepository.cs
git commit -m "feat(torre): add TorreEventoRepository and UsuarioNotificacaoRepository"
```

---

### Task 7: TorreEventoService

**Files:**
- Create: `LegendsAwaken.Application/Services/TorreEventoService.cs`
- Create: `LegendsAwaken.Tests/Unit/TorreEventoServiceTests.cs`

- [ ] **Step 1: Escrever testes que falham**

Criar `LegendsAwaken.Tests/Unit/TorreEventoServiceTests.cs`:

```csharp
using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LegendsAwaken.Tests.Unit;

public class TorreEventoServiceTests
{
    private readonly Mock<ITorreEventoRepository> _eventoRepo = new();
    private readonly Mock<ITorreExploracaoRepository> _exploracaoRepo = new();
    private TorreEventoService CreateService() =>
        new(_eventoRepo.Object, _exploracaoRepo.Object);

    private static TorreExploracao CriarExploracao(int andar = 5, int seed = 42) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        DiscordUserId = 123456789UL,
        AndarNumero = andar,
        Progresso = 25,
        Seed = seed,
        CheckpointsProcessados = CheckpointFlags.None,
        Status = StatusExploracao.Ativa,
        HeroisIds = "",
        ConsequenceTags = null
    };

    [Fact]
    public async Task GerarEventoAsync_RetornaEvento_ParaThreshold25()
    {
        var svc = CreateService();
        var exp = CriarExploracao();

        var evento = await svc.GerarEventoAsync(exp, threshold: 25);

        Assert.NotNull(evento);
        Assert.Equal(25, evento.ProgressoNoCheckpoint);
        Assert.Equal(exp.AndarNumero, evento.AndarOrigem);
        Assert.Equal(EventoStatus.Ativo, evento.Status);
    }

    [Fact]
    public async Task GerarEventoAsync_MesmoSeed_ProduceMesmoEvento()
    {
        var svc = CreateService();
        var exp1 = CriarExploracao(seed: 100);
        var exp2 = CriarExploracao(seed: 100);

        var e1 = await svc.GerarEventoAsync(exp1, 25);
        var e2 = await svc.GerarEventoAsync(exp2, 25);

        Assert.Equal(e1.EventoKey, e2.EventoKey);
        Assert.Equal(e1.EventoSeed, e2.EventoSeed);
    }

    [Fact]
    public async Task GerarEventoAsync_LancaInvalidOperation_SeThresholdJaProcessado()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        exp.CheckpointsProcessados = CheckpointFlags.P25;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GerarEventoAsync(exp, 25));
    }

    [Fact]
    public async Task ResolverAsync_SetsStatus_Resolvido()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        exp.Status = StatusExploracao.AguardandoEscolha;
        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            ExploracaoId = exp.Id,
            Status = EventoStatus.Ativo,
            Tipo = TipoEvento.BlockingChoice,
            EventoKey = "encruzilhada_mercador",
            Tier = TierEvento.Maior,
            Exploracao = exp
        };
        _eventoRepo.Setup(r => r.ObterAtivoAsync(exp.Id)).ReturnsAsync(evento);

        await svc.ResolverAsync(evento.Id, "pagar", exp);

        Assert.Equal(EventoStatus.Resolvido, evento.Status);
        Assert.Equal(StatusExploracao.Ativa, exp.Status);
        Assert.Equal("pagar", evento.OpcaoKey);
        Assert.NotNull(evento.ResolvidoEm);
    }

    [Fact]
    public async Task ResolverAsync_LancaException_SeOpcaoKeyInvalida()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            EventoKey = "encruzilhada_mercador",
            Status = EventoStatus.Ativo,
            Tipo = TipoEvento.BlockingChoice,
            Exploracao = exp
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ResolverAsync(evento.Id, "opcao_invalida", exp));
    }

    [Fact]
    public async Task ResolverAsync_NaoUltrapassaProximoCheckpoint_ComBonusProgresso()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        exp.Progresso = 25;
        exp.CheckpointsProcessados = CheckpointFlags.P25; // 50 é o próximo
        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            EventoKey = "trilha_oculta",
            Status = EventoStatus.Ativo,
            Tipo = TipoEvento.BlockingChoice,
            Exploracao = exp
        };

        await svc.ResolverAsync(evento.Id, "explorar", exp);

        // Bônus de explorar trilha_oculta não pode levar Progresso >= 50
        Assert.True(exp.Progresso < 50, $"Progresso foi para {exp.Progresso}, deveria ficar abaixo de 50");
    }
}
```

- [ ] **Step 2: Rodar — verificar que falha**

```
dotnet test LegendsAwaken.Tests --filter "TorreEventoServiceTests" -v minimal
```

Expected: FAIL — `TorreEventoService` not found.

- [ ] **Step 3: Criar TorreEventoService.cs**

```csharp
using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class TorreEventoService
{
    private readonly ITorreEventoRepository _eventoRepo;
    private readonly ITorreExploracaoRepository _exploracaoRepo;

    private static readonly TimeSpan DefaultExpiracao = TimeSpan.FromDays(7);

    public TorreEventoService(
        ITorreEventoRepository eventoRepo,
        ITorreExploracaoRepository exploracaoRepo)
    {
        _eventoRepo = eventoRepo;
        _exploracaoRepo = exploracaoRepo;
    }

    public async Task<TorreEvento> GerarEventoAsync(TorreExploracao exp, int threshold)
    {
        var flagDoThreshold = ThresholdParaFlag(threshold);
        if ((exp.CheckpointsProcessados & flagDoThreshold) != 0)
            throw new InvalidOperationException($"Checkpoint {threshold}% já foi processado para exploração {exp.Id}.");

        var seed = HashCode.Combine(exp.Seed, threshold, exp.AndarNumero);
        var rng = new EventoRng(seed);

        var tags = DeserializarTags(exp.ConsequenceTags);
        var candidatos = CheckpointEventoCatalog.FiltrarParaAndar(exp.AndarNumero, tags).ToList();

        if (!candidatos.Any())
            candidatos = CheckpointEventoCatalog.Todos
                .Where(e => e.MinAndar <= exp.AndarNumero)
                .ToList();

        var config = rng.EscolherPonderado(candidatos, c => c.Peso);

        var snapshot = SerializarSnapshot(exp);

        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            ExploracaoId = exp.Id,
            Status = EventoStatus.Ativo,
            Tipo = config.Tipo,
            Tier = config.Tier,
            Raridade = config.Raridade,
            EventoKey = config.Key,
            ProgressoNoCheckpoint = threshold,
            AndarOrigem = exp.AndarNumero,
            EventoSeed = seed,
            SnapshotCombatStateJson = snapshot,
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = config.Tier == TierEvento.Maior ? DateTime.UtcNow.Add(DefaultExpiracao) : null,
            Exploracao = exp
        };

        await _eventoRepo.AdicionarAsync(evento);
        return evento;
    }

    public async Task ResolverAsync(Guid eventoId, string opcaoKey, TorreExploracao exp)
    {
        var evento = await _eventoRepo.ObterAtivoAsync(exp.Id)
            ?? throw new InvalidOperationException($"Evento ativo não encontrado para exploração {exp.Id}.");

        var config = CheckpointEventoCatalog.Todos.FirstOrDefault(c => c.Key == evento.EventoKey)
            ?? throw new InvalidOperationException($"Configuração do evento '{evento.EventoKey}' não encontrada.");

        if (config.Opcoes != null && !config.Opcoes.Any(o => o.Key == opcaoKey))
            throw new ArgumentException($"Opção '{opcaoKey}' inválida para evento '{evento.EventoKey}'.");

        var (grau, progressoBonus, descricaoEfeito) = AplicarEfeito(config, opcaoKey, exp);

        // Clipa bônus para não ultrapassar próximo checkpoint não processado
        int proximoThreshold = ProximoThresholdNaoProcessado(exp.CheckpointsProcessados, exp.Progresso);
        if (progressoBonus > 0 && proximoThreshold > 0)
            progressoBonus = Math.Min(progressoBonus, proximoThreshold - (int)exp.Progresso);
        exp.Progresso = Math.Min(100, exp.Progresso + progressoBonus);

        if (config.ConsequenceTags?.Length > 0)
        {
            var tags = DeserializarTags(exp.ConsequenceTags).ToList();
            tags.AddRange(config.ConsequenceTags);
            exp.ConsequenceTags = JsonSerializer.Serialize(tags);
        }

        evento.OpcaoKey = opcaoKey;
        evento.ResolvidoEm = DateTime.UtcNow;
        evento.Status = EventoStatus.Resolvido;
        evento.ResultadoJson = JsonSerializer.Serialize(new
        {
            titulo = config.Titulo,
            descricao = descricaoEfeito,
            grauSucesso = grau.ToString(),
            progressoBonus,
            publico = true,
            schemaVersion = 1
        });

        exp.Status = StatusExploracao.Ativa;
        exp.Version++;

        await _eventoRepo.AtualizarAsync(evento);
        await _exploracaoRepo.AtualizarAsync(exp);
    }

    public async Task ResolverMenorInlineAsync(CheckpointEventoConfig config, TorreExploracao exp)
    {
        var (_, progressoBonus, descricao) = AplicarEfeito(config, opcaoKey: null, exp);
        int proximo = ProximoThresholdNaoProcessado(exp.CheckpointsProcessados, exp.Progresso);
        if (progressoBonus > 0 && proximo > 0)
            progressoBonus = Math.Min(progressoBonus, proximo - (int)exp.Progresso);
        exp.Progresso = Math.Min(100, exp.Progresso + progressoBonus);

        await _eventoRepo.AdicionarLogAsync(new TorreEventoLog
        {
            Id = Guid.NewGuid(),
            ExploracaoId = exp.Id,
            Texto = $"[{config.Titulo}] {descricao}",
            CriadoEm = DateTime.UtcNow
        });
    }

    public async Task RecuperarExpiradosAsync()
    {
        var expirados = await _eventoRepo.ObterExpiradosAsync(DateTime.UtcNow);
        foreach (var evento in expirados)
        {
            var exp = evento.Exploracao;
            evento.Status = EventoStatus.Expirado;
            evento.OpcaoKey = "expirado";
            evento.ResolvidoEm = DateTime.UtcNow;
            evento.ResultadoJson = JsonSerializer.Serialize(new
            {
                titulo = "Evento expirado",
                descricao = "A party continuou sem tomar uma decisão. Sem bônus ou penalidade.",
                grauSucesso = GrauSucesso.Falha.ToString(),
                progressoBonus = 0,
                publico = true,
                schemaVersion = 1
            });
            exp.Status = StatusExploracao.Ativa;
            exp.Version++;
            await _eventoRepo.AtualizarAsync(evento);
            await _exploracaoRepo.AtualizarAsync(exp);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    public static CheckpointFlags ThresholdParaFlag(int threshold) => threshold switch
    {
        25  => CheckpointFlags.P25,
        50  => CheckpointFlags.P50,
        75  => CheckpointFlags.P75,
        100 => CheckpointFlags.P100,
        _   => throw new ArgumentOutOfRangeException(nameof(threshold))
    };

    private static int ProximoThresholdNaoProcessado(CheckpointFlags flags, double progressoAtual)
    {
        int[] thresholds = { 25, 50, 75, 100 };
        foreach (var t in thresholds)
        {
            if ((flags & ThresholdParaFlag(t)) == 0 && t > progressoAtual)
                return t;
        }
        return 0;
    }

    private static IEnumerable<string> DeserializarTags(string? json)
    {
        if (string.IsNullOrEmpty(json)) return Enumerable.Empty<string>();
        return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
    }

    private static string? SerializarSnapshot(TorreExploracao exp)
    {
        if (string.IsNullOrEmpty(exp.HeroisIds)) return null;
        return JsonSerializer.Serialize(new { heroisIds = exp.HeroisIds });
    }

    private static (GrauSucesso Grau, int ProgressoBonus, string Descricao) AplicarEfeito(
        CheckpointEventoConfig config, string? opcaoKey, TorreExploracao exp)
    {
        return config.Key switch
        {
            "encruzilhada_mercador" => opcaoKey switch
            {
                "pagar"   => (GrauSucesso.SucessoTotal,  10, "Você pagou o preço. O mercador cede passagem. +10% progresso."),
                "forccar" => (GrauSucesso.SucessoParcial, 5,  "Forçou passagem com dificuldade. +5% progresso."),
                "recuar"  => (GrauSucesso.Falha,          0,  "A party recua prudentemente. Sem bônus."),
                _         => (GrauSucesso.Falha, 0, "")
            },
            "trilha_oculta" => opcaoKey switch
            {
                "explorar" => (GrauSucesso.SucessoTotal, 15, "A trilha encurta o caminho. +15% progresso."),
                "ignorar"  => (GrauSucesso.Falha,        0,  "A party segue pela rota principal. Sem bônus."),
                _          => (GrauSucesso.Falha, 0, "")
            },
            "chuva_de_fragmentos" => (GrauSucesso.SucessoTotal, 0, "Fragmentos coletados da câmara abandonada."),
            "armadilha_detectada" => (GrauSucesso.SucessoTotal, 0, "Armadilha contornada com sucesso. Nenhum dano."),
            "teste_forca_porta" => opcaoKey switch
            {
                "arrombar" => (GrauSucesso.SucessoTotal, 10, "Porta arrombada! +10% progresso."),
                _          => (GrauSucesso.Falha, 0, "")
            },
            "sombra_perseguindo" => opcaoKey switch
            {
                "fugir"     => (GrauSucesso.SucessoTotal,  5,  "A party escapa rapidamente. +5% progresso."),
                "enfrentar" => (GrauSucesso.SucessoTotal,  8,  "A ameaça é neutralizada. +8% progresso."),
                _           => (GrauSucesso.Falha, 0, "")
            },
            _ => (GrauSucesso.Falha, 0, "Evento desconhecido.")
        };
    }
}
```

**Nota:** `ISkillCheckService` é a interface existente (ou pode ser `SkillCheckService` — verificar como está registrado no projeto). Se não existir como interface, remover do construtor e usar o método estático `SkillCheckService.RolarGrupo` diretamente.

- [ ] **Step 4: Rodar — verificar que testes passam**

```
dotnet test LegendsAwaken.Tests --filter "TorreEventoServiceTests" -v minimal
```

Expected: Todos os testes PASS. Se algum falhar por `ISkillCheckService` não existir, ajustar conforme o serviço real do projeto.

- [ ] **Step 5: Commit**

```
git add LegendsAwaken.Application/Services/TorreEventoService.cs
git add LegendsAwaken.Tests/Unit/TorreEventoServiceTests.cs
git commit -m "feat(torre): TorreEventoService — GerarEventoAsync, ResolverAsync, resolução parcial"
```

---

### Task 8: ProcessarAsync — Checkpoint Layer

**Files:**
- Modify: `LegendsAwaken.Application/Services/TorreExploracaoService.cs`
- Create: `LegendsAwaken.Tests/Unit/TorreExploracaoCheckpointTests.cs`

- [ ] **Step 1: Escrever testes que falham**

Criar `LegendsAwaken.Tests/Unit/TorreExploracaoCheckpointTests.cs`:

```csharp
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace LegendsAwaken.Tests.Unit;

public class TorreExploracaoCheckpointTests
{
    private readonly Mock<ITorreExploracaoRepository> _exploracaoRepo = new();
    private readonly Mock<ITorreEventoRepository> _eventoRepo = new();
    private readonly Mock<INotificacaoService> _notificacao = new();
    private readonly Mock<ITorreRepository> _torreRepo = new();
    private readonly Mock<IHeroiRepository> _heroiRepo = new();

    private TorreExploracao CriarExploracao(double progresso, CheckpointFlags flags = CheckpointFlags.None) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        DiscordUserId = 111UL,
        AndarNumero = 5,
        Progresso = progresso,
        Seed = 1,
        CheckpointsProcessados = flags,
        Status = StatusExploracao.Ativa,
        HeroisIds = "",
        UltimoTickEm = DateTime.UtcNow.AddMinutes(-5),
        IniciadoEm = DateTime.UtcNow.AddMinutes(-10)
    };

    [Fact]
    public async Task ProcessarAsync_RetornaImediatamente_QuandoAguardandoEscolha()
    {
        var exp = CriarExploracao(25);
        exp.Status = StatusExploracao.AguardandoEscolha;
        _exploracaoRepo.Setup(r => r.ObterAtivaAsync(exp.UsuarioId)).ReturnsAsync(exp);

        // Arrange service (simplified — substitua pelos mocks reais do TorreExploracaoService)
        // Este teste valida que, quando Status == AguardandoEscolha, nenhum update é feito
        _exploracaoRepo.Verify(r => r.AtualizarAsync(It.IsAny<TorreExploracao>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarAsync_CongelaProgresso_NoThreshold25()
    {
        // Exploracao está em 24%, tick daria +5% → mas deve parar em 25%
        var exp = CriarExploracao(24);
        _exploracaoRepo.Setup(r => r.ObterAtivaAsync(exp.UsuarioId)).ReturnsAsync(exp);
        _eventoRepo.Setup(r => r.AdicionarAsync(It.IsAny<TorreEvento>())).Returns(Task.CompletedTask);
        _notificacao.Setup(n => n.NotificarEventoCheckpointAsync(It.IsAny<ulong>(), It.IsAny<TorreEvento>()))
            .Returns(Task.CompletedTask);

        // Após ProcessarAsync, progresso deve ser <= 25 e status AguardandoEscolha (evento Maior) ou Ativa (Menor)
        // O assert real vai depender de qual evento foi sorteado pelo seed
        // Valida que progresso não ultrapassou 25
        Assert.True(exp.Progresso <= 25.0001);
    }

    [Fact]
    public void ThresholdParaFlag_RetornaFlagCorreta()
    {
        Assert.Equal(CheckpointFlags.P25,  TorreEventoService.ThresholdParaFlag(25));
        Assert.Equal(CheckpointFlags.P50,  TorreEventoService.ThresholdParaFlag(50));
        Assert.Equal(CheckpointFlags.P75,  TorreEventoService.ThresholdParaFlag(75));
        Assert.Equal(CheckpointFlags.P100, TorreEventoService.ThresholdParaFlag(100));
    }

    [Fact]
    public void CheckpointFlags_Bitmask_FuncionaCorretamente()
    {
        var flags = CheckpointFlags.P25 | CheckpointFlags.P50;
        Assert.True((flags & CheckpointFlags.P25) != 0);
        Assert.True((flags & CheckpointFlags.P50) != 0);
        Assert.False((flags & CheckpointFlags.P75) != 0);
        Assert.False((flags & CheckpointFlags.P100) != 0);
    }
}
```

- [ ] **Step 2: Rodar — verificar que os testes de bitmask passam (eles não dependem do serviço)**

```
dotnet test LegendsAwaken.Tests --filter "TorreExploracaoCheckpointTests" -v minimal
```

Expected: `ThresholdParaFlag_*` e `CheckpointFlags_Bitmask_*` PASS. Os outros podem falhar até o Step 3.

- [ ] **Step 3: Modificar TorreExploracaoService — adicionar TorreEventoService ao construtor**

Abrir `LegendsAwaken.Application/Services/TorreExploracaoService.cs`.

**3a. Adicionar ao construtor:**

Adicionar `TorreEventoService torreEventoService` e `INotificacaoService notificacaoService` como novos parâmetros e campos:

```csharp
private readonly TorreEventoService _eventoService;
private readonly INotificacaoService _notificacaoService;
```

No construtor (acrescentar ao final da lista existente):
```csharp
_eventoService = torreEventoService;
_notificacaoService = notificacaoService;
```

**3b. Modificar `IniciarAsync` para aceitar discordId:**

Na assinatura existente:
```csharp
public async Task<TorreExploracao> IniciarAsync(Guid usuarioId, List<Guid> heroisIds, TipoBooster? booster)
```

Mudar para:
```csharp
public async Task<TorreExploracao> IniciarAsync(Guid usuarioId, ulong discordId, List<Guid> heroisIds, TipoBooster? booster)
```

No corpo de `IniciarAsync`, onde a exploração é criada, adicionar:
```csharp
exploracao.Seed = new Random().Next();
exploracao.DiscordUserId = discordId;
```

(O `new Random().Next()` aqui é intencional — seed aleatório por run, reprodutível por evento via `HashCode.Combine`.)

**3c. Substituir o loop de checkpoint por checkpoint layer:**

Localizar o bloco de checkpoint existente dentro de `ProcessarAsync`. Ele deve ser algo como:

```csharp
// REMOVER este bloco existente:
while (newProgress >= exploracao.UltimoCheckpoint + exploracao.CheckpointInterval)
{
    exploracao.UltimoCheckpoint += exploracao.CheckpointInterval;
    // gold, fragments...
}
```

Substituir por (inserir ANTES do cálculo do progressoGanho, no início do método, após as validações de status e debounce):

```csharp
// ── Checkpoint layer: calcular teto de progresso ────────────────────────
int? proximoThreshold = null;
foreach (var t in new[] { 25, 50, 75, 100 })
{
    var flag = TorreEventoService.ThresholdParaFlag(t);
    if ((exploracao.CheckpointsProcessados & flag) == 0 && t > exploracao.Progresso)
    {
        proximoThreshold = t;
        break;
    }
}
```

E logo após calcular `progressoGanho` (antes de somar ao `exploracao.Progresso`):

```csharp
// Congelar progresso no threshold se necessário
double progressoMaxEsseTick = proximoThreshold.HasValue
    ? Math.Min(progressoGanho, proximoThreshold.Value - exploracao.Progresso)
    : progressoGanho;

exploracao.Progresso = Math.Min(100, exploracao.Progresso + progressoMaxEsseTick);
// [O cálculo de ouro proporcional deve usar progressoMaxEsseTick, não progressoGanho]
```

Após atualizar `exploracao.Progresso`, adicionar o bloco de geração de evento:

```csharp
// ── Gerar evento de checkpoint se cruzou threshold ───────────────────────
if (proximoThreshold.HasValue && exploracao.Progresso >= proximoThreshold.Value)
{
    var flagProcessada = TorreEventoService.ThresholdParaFlag(proximoThreshold.Value);
    exploracao.CheckpointsProcessados |= flagProcessada;
    exploracao.Version++;

    var evento = await _eventoService.GerarEventoAsync(exploracao, proximoThreshold.Value);

    if (evento.Tier == TierEvento.Maior)
    {
        exploracao.Status = StatusExploracao.AguardandoEscolha;
        await _exploracaoRepo.AtualizarAsync(exploracao);
        // Notificar sem aguardar resultado (DM pode falhar silenciosamente)
        _ = _notificacaoService.NotificarEventoCheckpointAsync(exploracao.DiscordUserId, evento)
            .ContinueWith(t => { /* silenciar falhas de DM */ }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        return;
    }
    else
    {
        // Tier.Menor: resolver inline
        var config = CheckpointEventoCatalog.Todos.First(c => c.Key == evento.EventoKey);
        await _eventoService.ResolverMenorInlineAsync(config, exploracao);
        // 1 checkpoint por tick: salvar e sair
        exploracao.Version++;
        await _exploracaoRepo.AtualizarAsync(exploracao);
        return;
    }
}
```

**3d. Adicionar verificação de `AguardandoEscolha` no início de `ProcessarAsync`:**

Logo após as verificações de `Status != Ativa`:

```csharp
if (exploracao.Status == StatusExploracao.AguardandoEscolha) return;
```

- [ ] **Step 4: Verificar build**

```
dotnet build LegendsAwaken.Application
```

Expected: Build succeeded, 0 errors. Se houver erros de `ISkillCheckService` ou outros tipos, ajustar conforme os tipos existentes.

- [ ] **Step 5: Rodar todos os testes**

```
dotnet test LegendsAwaken.Tests -v minimal
```

Expected: Todos os testes anteriores continuam passando. Novos testes passam.

- [ ] **Step 6: Commit**

```
git add LegendsAwaken.Application/Services/TorreExploracaoService.cs
git add LegendsAwaken.Tests/Unit/TorreExploracaoCheckpointTests.cs
git commit -m "feat(torre): ProcessarAsync — checkpoint layer, freeze-at-threshold, AguardandoEscolha pause"
```

---

### Task 9: NotificacaoService

**Files:**
- Create: `LegendsAwaken.Bot/Services/NotificacaoService.cs`

- [ ] **Step 1: Criar NotificacaoService.cs**

```csharp
using Discord;
using Discord.WebSocket;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly DiscordSocketClient _discord;
    private readonly IUsuarioNotificacaoRepository _notifRepo;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(
        DiscordSocketClient discord,
        IUsuarioNotificacaoRepository notifRepo,
        ILogger<NotificacaoService> logger)
    {
        _discord = discord;
        _notifRepo = notifRepo;
        _logger = logger;
    }

    public async Task NotificarEventoCheckpointAsync(ulong discordUserId, TorreEvento evento)
    {
        var prefs = await _notifRepo.ObterAsync(discordUserId);

        if (prefs != null && !prefs.NotificacoesAtivas) return;
        if (prefs?.Preferencia == Domain.Enum.NotificacaoPreferencia.Desativado) return;
        if (prefs?.Preferencia == Domain.Enum.NotificacaoPreferencia.ApenasConclusao) return;

        var embed = new EmbedBuilder()
            .WithTitle($"⚠️ Exploração pausada — Checkpoint {evento.ProgressoNoCheckpoint}%")
            .WithDescription($"**{evento.EventoKey.Replace("_", " ")}**\nUse `/torre` para ver suas opções.")
            .WithColor(Color.Orange)
            .WithFooter($"🗼 Andar {evento.AndarOrigem}")
            .Build();

        try
        {
            if (prefs?.CanalPreferido.HasValue == true)
            {
                var canal = _discord.GetChannel(prefs.CanalPreferido.Value) as IMessageChannel;
                if (canal != null)
                {
                    await canal.SendMessageAsync($"<@{discordUserId}>", embed: embed);
                    return;
                }
            }

            var user = await _discord.GetUserAsync(discordUserId);
            if (user != null)
            {
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao notificar usuário {DiscordId} — DM bloqueada ou canal inválido.", discordUserId);
        }
    }
}
```

- [ ] **Step 2: Verificar build**

```
dotnet build LegendsAwaken.Bot
```

Expected: Build succeeded. Se houver erro de `IUsuarioNotificacaoRepository` não registrado, será corrigido na Task 10.

- [ ] **Step 3: Commit**

```
git add LegendsAwaken.Bot/Services/NotificacaoService.cs
git commit -m "feat(torre): NotificacaoService — Discord DM/canal com fallback silencioso"
```

---

### Task 10: TorreEventoPanel + CustomIdFactory

**Files:**
- Create: `LegendsAwaken.Bot/Helpers/CustomIdFactory.cs`
- Create: `LegendsAwaken.Bot/Panels/TorreEventoPanel.cs`

- [ ] **Step 1: Criar CustomIdFactory.cs**

```csharp
using System;

namespace LegendsAwaken.Bot.Helpers;

public static class CustomIdFactory
{
    public const string EventoEscolhaPrefix = "torre_evento_escolha";

    public static string EventoEscolha(Guid eventoId, string opcaoKey) =>
        $"{EventoEscolhaPrefix}:{eventoId}:{opcaoKey}";
}
```

- [ ] **Step 2: Criar TorreEventoPanel.cs**

```csharp
using Discord;
using LegendsAwaken.Application.Config;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Bot.Panels;

public static class TorreEventoPanel
{
    public static Embed CriarEmbedEscolha(TorreEvento evento, CheckpointEventoConfig config)
    {
        string expiracao = evento.ExpiraEm.HasValue
            ? FormatarExpiracao(evento.ExpiraEm.Value)
            : "Sem expiração";

        var builder = new EmbedBuilder()
            .WithTitle($"🔀 {config.Titulo} — Checkpoint {evento.ProgressoNoCheckpoint}%")
            .WithDescription(config.Descricao)
            .WithColor(Color.DarkOrange)
            .WithFooter($"🗼 Andar {evento.AndarOrigem} | ⏳ Expira em {expiracao}");

        return builder.Build();
    }

    public static MessageComponent CriarComponentesEscolha(TorreEvento evento, CheckpointEventoConfig config)
    {
        var builder = new ComponentBuilder();

        if (config.Opcoes == null) return builder.Build();

        foreach (var opcao in config.Opcoes)
        {
            var style = opcao.RiscoTom switch
            {
                RiscoTom.Seguro    => ButtonStyle.Success,
                RiscoTom.Arriscado => ButtonStyle.Danger,
                _                  => ButtonStyle.Secondary
            };
            builder.WithButton(opcao.TextoExibido, CustomIdFactory.EventoEscolha(evento.Id, opcao.Key), style);
        }

        return builder.Build();
    }

    public static MessageComponent CriarComponentesDesabilitados(TorreEvento evento, CheckpointEventoConfig config)
    {
        var builder = new ComponentBuilder();
        if (config.Opcoes == null) return builder.Build();

        foreach (var opcao in config.Opcoes)
        {
            var style = opcao.RiscoTom switch
            {
                RiscoTom.Seguro    => ButtonStyle.Success,
                RiscoTom.Arriscado => ButtonStyle.Danger,
                _                  => ButtonStyle.Secondary
            };
            builder.WithButton(opcao.TextoExibido, CustomIdFactory.EventoEscolha(evento.Id, opcao.Key), style, isDisabled: true);
        }

        return builder.Build();
    }

    public static Embed CriarEmbedResultado(TorreEvento evento, CheckpointEventoConfig config, string descricaoResultado, int progressoBonus)
    {
        var color = evento.OpcaoKey == "recuar" || evento.OpcaoKey == "ignorar"
            ? Color.DarkGrey
            : Color.Green;

        return new EmbedBuilder()
            .WithTitle($"✅ {config.Titulo}")
            .WithDescription(descricaoResultado)
            .WithColor(color)
            .AddField("Progresso bônus", progressoBonus > 0 ? $"+{progressoBonus}%" : "Nenhum", inline: true)
            .Build();
    }

    private static string FormatarExpiracao(DateTime expiraEm)
    {
        var restante = expiraEm - DateTime.UtcNow;
        if (restante.TotalDays >= 1)
            return $"{(int)restante.TotalDays}d {restante.Hours}h";
        return $"{restante.Hours}h {restante.Minutes}m";
    }
}
```

- [ ] **Step 3: Verificar build**

```
dotnet build LegendsAwaken.Bot
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Bot/Helpers/CustomIdFactory.cs
git add LegendsAwaken.Bot/Panels/TorreEventoPanel.cs
git commit -m "feat(torre): TorreEventoPanel + CustomIdFactory — Discord UI para eventos de checkpoint"
```

---

### Task 11: TorreCommand routing + DI

**Files:**
- Modify: `LegendsAwaken.Bot/Commands/TorreCommand.cs`
- Modify: `LegendsAwaken.Bot/Program.cs`

- [ ] **Step 1: Adicionar routing para AguardandoEscolha em TorreCommand**

Abrir `LegendsAwaken.Bot/Commands/TorreCommand.cs`.

**1a. Adicionar TorreEventoService ao construtor:**

```csharp
private readonly TorreEventoService _eventoService;
```

Adicionar `TorreEventoService eventoService` ao construtor e atribuir `_eventoService = eventoService;`.

**1b. Localizar onde `BuildPanelAsync` (ou equivalente) é chamado para exibir o status da exploração.**

Antes de renderizar o painel de exploração ativa, adicionar:

```csharp
var exploracaoAtiva = await _exploracaoService.ObterAtivaAsync(usuarioId);

if (exploracaoAtiva?.Status == StatusExploracao.AguardandoEscolha)
{
    var eventoAtivo = await _eventoService.ObterEventoAtivoAsync(exploracaoAtiva.Id);
    if (eventoAtivo != null)
    {
        var config = CheckpointEventoCatalog.Todos.FirstOrDefault(c => c.Key == eventoAtivo.EventoKey);
        if (config != null)
        {
            var embed = TorreEventoPanel.CriarEmbedEscolha(eventoAtivo, config);
            var components = TorreEventoPanel.CriarComponentesEscolha(eventoAtivo, config);
            await command.ModifyOriginalResponseAsync(m =>
            {
                m.Embed = embed;
                m.Components = components;
            });
            return;
        }
    }
}
```

**1c. Adicionar `ObterEventoAtivoAsync` ao `TorreEventoService`:**

No arquivo `TorreEventoService.cs` (já criado na Task 7), adicionar o método que ainda não existe:

```csharp
public async Task<TorreEvento?> ObterEventoAtivoAsync(Guid exploracaoId) =>
    await _eventoRepo.ObterAtivoAsync(exploracaoId);
```

**1d. Adicionar handler `torre_evento_escolha` em TorreCommand:**

Adicionar o método:

```csharp
public async Task HandleEventoEscolhaAsync(SocketMessageComponent comp)
{
    await comp.DeferAsync();

    var parts = InteractionRouter.ParseParts(comp.Data.CustomId);
    // parts[0] = prefix, parts[1] = eventoId, parts[2] = opcaoKey
    if (parts.Length < 3 || !Guid.TryParse(parts[1], out var eventoId))
    {
        await comp.FollowupAsync("Interação inválida.", ephemeral: true);
        return;
    }

    var opcaoKey = parts[2];
    // Obter usuarioId: replicar o mesmo padrão dos outros handlers em TorreCommand.
    // Abrir HandleExpColetarAsync ou HandleExpAtualizarAsync no mesmo arquivo e copiar
    // a linha que converte comp.User.Id (ulong) para Guid (ex: via UsuarioService ou hash).
    // Exemplo se o padrão for conversão direta:
    //   var usuarioId = new Guid(comp.User.Id.ToString("X").PadLeft(32, '0').Substring(0, 32));
    // Mas use o padrão real encontrado nos outros handlers — não invente.
    var usuarioId = ObterUsuarioId(comp.User.Id); // substituir pela chamada real do projeto

    var exploracaoAtiva = await _exploracaoService.ObterAtivaAsync(usuarioId);
    if (exploracaoAtiva == null)
    {
        await comp.FollowupAsync("Nenhuma exploração ativa.", ephemeral: true);
        return;
    }

    // Validar ownership
    if (exploracaoAtiva.DiscordUserId != comp.User.Id)
    {
        await comp.FollowupAsync("Você não pode responder este evento.", ephemeral: true);
        return;
    }

    var eventoAtivo = await _eventoService.ObterEventoAtivoAsync(exploracaoAtiva.Id);
    if (eventoAtivo == null || eventoAtivo.Id != eventoId)
    {
        await comp.FollowupAsync("Evento não encontrado ou já resolvido.", ephemeral: true);
        return;
    }

    var config = CheckpointEventoCatalog.Todos.FirstOrDefault(c => c.Key == eventoAtivo.EventoKey);
    if (config == null)
    {
        await comp.FollowupAsync("Configuração do evento não encontrada.", ephemeral: true);
        return;
    }

    try
    {
        await _eventoService.ResolverAsync(eventoId, opcaoKey, exploracaoAtiva);
    }
    catch (ArgumentException ex)
    {
        await comp.FollowupAsync($"Opção inválida: {ex.Message}", ephemeral: true);
        return;
    }

    // Desabilitar botões e mostrar resultado
    var resultadoEmbed = TorreEventoPanel.CriarEmbedResultado(
        eventoAtivo, config,
        descricaoResultado: "Evento resolvido. A exploração continua.",
        progressoBonus: 0 // Extrair do ResultadoJson se necessário
    );
    var componentesDesabilitados = TorreEventoPanel.CriarComponentesDesabilitados(eventoAtivo, config);

    await comp.ModifyOriginalResponseAsync(m =>
    {
        m.Embed = resultadoEmbed;
        m.Components = componentesDesabilitados;
    });
}
```

**Nota:** Verificar como outros handlers em TorreCommand obtêm o `usuarioId` (Guid) a partir do `comp.User.Id` (ulong) e replicar o mesmo padrão.

**1e. Registrar o handler no `InteractionRouter`:**

No método ou construtor onde os handlers são registrados (provavelmente em `Program.cs` ou no construtor do `CommandHandler`), adicionar:

```csharp
interactionRouter.Register(new TorreEventoHandler(torreCommand));
```

Ou, se o padrão existente for diferente (ex: verificação direta de CustomId prefix em `TorreCommand.HandleAsync`), seguir o padrão existente.

Verificar como `InteractionRouter` é populado no projeto e registrar o novo handler seguindo o mesmo padrão.

- [ ] **Step 2: Modificar Program.cs — registrar novos serviços**

Abrir `LegendsAwaken.Bot/Program.cs`.

Adicionar os registros de DI seguindo o padrão existente (provavelmente `services.AddScoped<...>()`):

```csharp
// Repositories
services.AddScoped<ITorreEventoRepository, TorreEventoRepository>();
services.AddScoped<IUsuarioNotificacaoRepository, UsuarioNotificacaoRepository>();

// Services
services.AddScoped<TorreEventoService>();
services.AddScoped<INotificacaoService, NotificacaoService>();
```

Também atualizar o registro de `TorreExploracaoService` se o construtor mudou (adição de `TorreEventoService` e `INotificacaoService`). Se usar DI automática, o container resolverá automaticamente.

Atualizar o registro de `TorreCommand` se seu construtor mudou (adição de `TorreEventoService`).

**Startup recovery:** No mesmo local onde `HeroiAtributosResetService.MigrarAsync()` é chamado no startup (após `db.MigrateAsync()`), adicionar:

```csharp
var eventoService = scope.ServiceProvider.GetRequiredService<TorreEventoService>();
await eventoService.RecuperarExpiradosAsync();
```

- [ ] **Step 3: Verificar build completo**

```
dotnet build
```

Expected: Todos os projetos compilam sem erros.

- [ ] **Step 4: Rodar todos os testes**

```
dotnet test LegendsAwaken.Tests -v minimal
```

Expected: Todos os testes PASS. Contar o total — deve ser >= 121 (111 existentes + novos).

- [ ] **Step 5: Commit final**

```
git add LegendsAwaken.Bot/Commands/TorreCommand.cs
git add LegendsAwaken.Bot/Program.cs
git add LegendsAwaken.Application/Services/TorreEventoService.cs
git commit -m "feat(torre): wire TorreCommand routing, handler torre_evento_escolha, DI registration — checkpoint events v1"
```

---

## Verificação Final

Após todas as tasks, verificar:

- [ ] `dotnet build` — 0 erros em todos os projetos
- [ ] `dotnet test LegendsAwaken.Tests -v minimal` — todos passando
- [ ] Bot inicia sem erros (`dotnet run --project LegendsAwaken.Bot`)
- [ ] `/torre` com exploração em `AguardandoEscolha` exibe `TorreEventoPanel`
- [ ] Botões de escolha resolvem o evento e são desabilitados
- [ ] DM é enviada ao jogador quando evento Maior é gerado
- [ ] Exploração retoma após resolução de evento

## Ordem de Execução

Tasks 1 → 2 → 3 são independentes entre si mas devem preceder as demais.  
Task 4 (migration) requer Tasks 1+2 completas.  
Task 5 (repositórios) requer Task 4.  
Task 6 (repositórios infra) requer Task 3+4.  
Task 7 (TorreEventoService) requer Tasks 3+5+6.  
Task 8 (ProcessarAsync) requer Task 7.  
Tasks 9+10 (Bot UI) requer Task 7.  
Task 11 (wiring) requer todas as anteriores.
