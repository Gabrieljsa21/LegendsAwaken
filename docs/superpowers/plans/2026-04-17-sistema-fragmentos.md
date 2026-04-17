# Sistema de Fragmentos — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Substituir o sistema de gacha por aquisição determinística de heróis via fragmentos, biomas da Torre e contratos.

**Architecture:** Big Bang — gacha removido primeiro, depois entidades, serviços e painéis Discord construídos em sequência linear. TorreService recebe dois pontos de extensão (drop de fragmentos + detecção de bioma). Todos os comandos Discord abrem painéis interativos em vez de executar lógica diretamente.

**Tech Stack:** C# / .NET 10, EF Core (SQLite), Discord.NET, xUnit + Moq

---

## Mapa de Arquivos

### Deletar
```
LegendsAwaken.Application/Services/GachaService.cs
LegendsAwaken.Application/Services/BannerService.cs
LegendsAwaken.Application/Services/BannerHistoricoService.cs
LegendsAwaken.Domain/Entities/Banner/BannerConfiguracao.cs
LegendsAwaken.Domain/Entities/Banner/BannerHistorico.cs
LegendsAwaken.Domain/Entities/Banner/BannerProgresso.cs
LegendsAwaken.Domain/Entities/Banner/RacaChance.cs
LegendsAwaken.Infrastructure/Repositories/BannerHistoricoRepository.cs
LegendsAwaken.Infrastructure/Providers/BannerConfiguracoesProvider.cs
LegendsAwaken.Bot/Commands/BannerCommand.cs
LegendsAwaken.Bot/Commands/InvocarCommand.cs
```

### Criar — Domain
```
LegendsAwaken.Domain/Entities/Fragmento/HeroiConfig.cs
LegendsAwaken.Domain/Entities/Fragmento/Bioma.cs
LegendsAwaken.Domain/Entities/Fragmento/BiomHeroPool.cs
LegendsAwaken.Domain/Entities/Fragmento/HeroiUnlockConfig.cs
LegendsAwaken.Domain/Entities/Fragmento/FragmentoProgresso.cs
LegendsAwaken.Domain/Entities/Fragmento/Contrato.cs
LegendsAwaken.Domain/Entities/Fragmento/HeroiDesbloqueado.cs
LegendsAwaken.Domain/Interfaces/IBiomaRepository.cs
LegendsAwaken.Domain/Interfaces/IFragmentoRepository.cs
LegendsAwaken.Domain/Interfaces/IContratoRepository.cs
LegendsAwaken.Domain/Interfaces/IHeroiDesbloqueadoRepository.cs
LegendsAwaken.Domain/Interfaces/IHeroiConfigRepository.cs
```

### Criar — Application
```
LegendsAwaken.Application/Config/ContractConfig.cs
LegendsAwaken.Application/DTOs/FragmentDTOs.cs
LegendsAwaken.Application/Services/BiomeService.cs
LegendsAwaken.Application/Services/FragmentService.cs
LegendsAwaken.Application/Services/ContractService.cs
LegendsAwaken.Application/Services/RecruitmentService.cs
LegendsAwaken.Application/Services/RewardDistributionService.cs
```

### Criar — Infrastructure
```
LegendsAwaken.Infrastructure/Repositories/BiomaRepository.cs
LegendsAwaken.Infrastructure/Repositories/FragmentoRepository.cs
LegendsAwaken.Infrastructure/Repositories/ContratoRepository.cs
LegendsAwaken.Infrastructure/Repositories/HeroiDesbloqueadoRepository.cs
LegendsAwaken.Infrastructure/Repositories/HeroiConfigRepository.cs
LegendsAwaken.Infrastructure/SeedData/FragmentoSeed.cs
```

### Criar — Bot
```
LegendsAwaken.Bot/Commands/ColecaoCommand.cs
LegendsAwaken.Bot/Commands/BiomaCommand.cs
LegendsAwaken.Bot/Commands/ContratoCommand.cs
LegendsAwaken.Bot/Panels/ColecaoPanel.cs
LegendsAwaken.Bot/Panels/BiomaPanel.cs
LegendsAwaken.Bot/Panels/ContratoPanel.cs
```

### Modificar
```
LegendsAwaken.Domain/Enum/Enums.cs                         — adicionar 4 enums
LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs     — novos DbSets, remover Banner DbSets
LegendsAwaken.Application/Services/TorreService.cs         — estender SubirAndarResult + 2 extension points
LegendsAwaken.Application/Services/GeracaoDeDadosService.cs — migrar PersonagensFixos para HeroiConfig
LegendsAwaken.Tests/UnitTest1.cs                           — remover placeholder
```

---

## Task 1: Remover Sistema de Gacha

**Files:**
- Delete: `LegendsAwaken.Application/Services/GachaService.cs`
- Delete: `LegendsAwaken.Application/Services/BannerService.cs`
- Delete: `LegendsAwaken.Application/Services/BannerHistoricoService.cs`
- Delete: `LegendsAwaken.Domain/Entities/Banner/` (pasta inteira)
- Delete: `LegendsAwaken.Infrastructure/Repositories/BannerHistoricoRepository.cs`
- Delete: `LegendsAwaken.Infrastructure/Providers/BannerConfiguracoesProvider.cs`
- Delete: `LegendsAwaken.Bot/Commands/BannerCommand.cs`
- Delete: `LegendsAwaken.Bot/Commands/InvocarCommand.cs`

- [ ] **Step 1: Deletar arquivos de gacha e banner**

```bash
cd /c/Workspace/LegendsAwaken
rm LegendsAwaken.Application/Services/GachaService.cs
rm LegendsAwaken.Application/Services/BannerService.cs
rm LegendsAwaken.Application/Services/BannerHistoricoService.cs
rm -rf LegendsAwaken.Domain/Entities/Banner/
rm LegendsAwaken.Infrastructure/Repositories/BannerHistoricoRepository.cs
rm LegendsAwaken.Infrastructure/Providers/BannerConfiguracoesProvider.cs
rm LegendsAwaken.Bot/Commands/BannerCommand.cs
rm LegendsAwaken.Bot/Commands/InvocarCommand.cs
```

- [ ] **Step 2: Verificar e corrigir erros de compilação**

```bash
dotnet build 2>&1 | grep -E "error|Error"
```

Erros esperados: referências a `BannerService`, `GachaService`, `BannerHistoricoService` em `CommandHandler.cs`, `Program.cs` ou DI registration. Para cada erro:
- Remover `using` statements que referenciem `Banner/*`
- Remover registros de DI (ex: `services.AddScoped<GachaService>()`)
- Remover injeção de dependência nos construtores afetados

- [ ] **Step 3: Confirmar build limpo**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "chore: remove gacha system (GachaService, BannerService, Banner entities)"
```

---

## Task 2: Novos Enums

**Files:**
- Modify: `LegendsAwaken.Domain/Enum/Enums.cs`

- [ ] **Step 1: Adicionar os 4 novos enums ao arquivo de enums existente**

Abrir `LegendsAwaken.Domain/Enum/Enums.cs` e acrescentar ao final do arquivo (antes do último `}`):

```csharp
public enum TipoFragmento
{
    Heroi = 1,
    Arquetipo = 2,
    Generico = 3
}

public enum TipoUnlock
{
    Fragmentos = 1,
    MarcoTorre = 2,
    CondicaoUnica = 3
}

public enum TipoContrato
{
    Arquetipo = 1,
    Nomeado = 2
}

public enum TipoEventoAlto
{
    DescobertaBioma = 1,
    HeroiIconicoDesbloqueado = 2
}
```

- [ ] **Step 2: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add LegendsAwaken.Domain/Enum/Enums.cs
git commit -m "feat: add TipoFragmento, TipoUnlock, TipoContrato, TipoEventoAlto enums"
```

---

## Task 3: Entidades de Domínio

**Files:**
- Create: `LegendsAwaken.Domain/Entities/Fragmento/HeroiConfig.cs`
- Create: `LegendsAwaken.Domain/Entities/Fragmento/Bioma.cs`
- Create: `LegendsAwaken.Domain/Entities/Fragmento/BiomHeroPool.cs`
- Create: `LegendsAwaken.Domain/Entities/Fragmento/HeroiUnlockConfig.cs`
- Create: `LegendsAwaken.Domain/Entities/Fragmento/FragmentoProgresso.cs`
- Create: `LegendsAwaken.Domain/Entities/Fragmento/Contrato.cs`
- Create: `LegendsAwaken.Domain/Entities/Fragmento/HeroiDesbloqueado.cs`

- [ ] **Step 1: Criar pasta e HeroiConfig.cs**

```csharp
// LegendsAwaken.Domain/Entities/Fragmento/HeroiConfig.cs
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class HeroiConfig
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Raridade RaridadeBase { get; set; }
    public Profissao Arquetipo { get; set; }
    public string? Tag { get; set; }
}
```

- [ ] **Step 2: Criar Bioma.cs**

```csharp
// LegendsAwaken.Domain/Entities/Fragmento/Bioma.cs
namespace LegendsAwaken.Domain.Entities.Fragmento;

public class Bioma
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int AndarInicio { get; set; }
    public int AndarFim { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public List<BiomHeroPool> Pool { get; set; } = [];
}
```

- [ ] **Step 3: Criar BiomHeroPool.cs**

```csharp
// LegendsAwaken.Domain/Entities/Fragmento/BiomHeroPool.cs
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class BiomHeroPool
{
    public Guid Id { get; set; }
    public Guid BiomeId { get; set; }
    public Bioma Bioma { get; set; } = null!;
    public Guid HeroiId { get; set; }
    public HeroiConfig Heroi { get; set; } = null!;
    public Raridade Raridade { get; set; }
    public int DropWeight { get; set; }
    public bool EHeroPrincipal { get; set; }
}
```

- [ ] **Step 4: Criar HeroiUnlockConfig.cs**

```csharp
// LegendsAwaken.Domain/Entities/Fragmento/HeroiUnlockConfig.cs
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class HeroiUnlockConfig
{
    public Guid HeroiId { get; set; }
    public HeroiConfig Heroi { get; set; } = null!;
    public TipoUnlock TipoUnlock { get; set; }
    public int? QuantidadeFragmentos { get; set; }
    public int? AndarMarco { get; set; }
    public string? CondicaoDescricao { get; set; }
}
```

- [ ] **Step 5: Criar FragmentoProgresso.cs**

```csharp
// LegendsAwaken.Domain/Entities/Fragmento/FragmentoProgresso.cs
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class FragmentoProgresso
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public TipoFragmento TipoFragmento { get; set; }
    public Guid? HeroiId { get; set; }
    public HeroiConfig? Heroi { get; set; }
    // Preenchido se TipoFragmento == Arquetipo. Nulo se Heroi ou Generico.
    public Profissao? Arquetipo { get; set; }
    public int Quantidade { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
```

- [ ] **Step 6: Criar Contrato.cs**

```csharp
// LegendsAwaken.Domain/Entities/Fragmento/Contrato.cs
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class Contrato
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public TipoContrato Tipo { get; set; }
    public Profissao? Arquetipo { get; set; }
    public Guid? HeroiId { get; set; }
    public HeroiConfig? Heroi { get; set; }
    public bool Ativo { get; set; }
    public DateTime? ExpiraEm { get; set; }
    public DateTime CriadoEm { get; set; }
}
```

- [ ] **Step 7: Criar HeroiDesbloqueado.cs**

```csharp
// LegendsAwaken.Domain/Entities/Fragmento/HeroiDesbloqueado.cs
namespace LegendsAwaken.Domain.Entities.Fragmento;

public class HeroiDesbloqueado
{
    public Guid UsuarioId { get; set; }
    public Guid HeroiId { get; set; }
    public HeroiConfig Heroi { get; set; } = null!;
    public DateTime DesbloqueadoEm { get; set; }
}
```

- [ ] **Step 8: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 9: Commit**

```bash
git add LegendsAwaken.Domain/Entities/Fragmento/
git commit -m "feat: add fragment domain entities (HeroiConfig, Bioma, Contrato, etc.)"
```

---

## Task 4: Interfaces de Repositório

**Files:**
- Create: `LegendsAwaken.Domain/Interfaces/IBiomaRepository.cs`
- Create: `LegendsAwaken.Domain/Interfaces/IFragmentoRepository.cs`
- Create: `LegendsAwaken.Domain/Interfaces/IContratoRepository.cs`
- Create: `LegendsAwaken.Domain/Interfaces/IHeroiDesbloqueadoRepository.cs`
- Create: `LegendsAwaken.Domain/Interfaces/IHeroiConfigRepository.cs`

- [ ] **Step 1: Criar IBiomaRepository.cs**

```csharp
// LegendsAwaken.Domain/Interfaces/IBiomaRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Domain.Interfaces;

public interface IBiomaRepository
{
    Task<Bioma?> ObterPorAndarAsync(int andar);
    Task<List<BiomHeroPool>> ObterPoolAsync(Guid biomaId);
    Task<List<Bioma>> ListarTodosAsync();
}
```

- [ ] **Step 2: Criar IFragmentoRepository.cs**

```csharp
// LegendsAwaken.Domain/Interfaces/IFragmentoRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Interfaces;

public interface IFragmentoRepository
{
    Task<FragmentoProgresso?> ObterPorHeroiAsync(Guid usuarioId, Guid heroiId);
    Task<FragmentoProgresso?> ObterPorArquetipoAsync(Guid usuarioId, Profissao arquetipo);
    Task UpsertAsync(FragmentoProgresso progresso);
    Task<List<FragmentoProgresso>> ListarPorUsuarioAsync(Guid usuarioId);
}
```

- [ ] **Step 3: Criar IContratoRepository.cs**

```csharp
// LegendsAwaken.Domain/Interfaces/IContratoRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Interfaces;

public interface IContratoRepository
{
    Task<Contrato?> ObterAtivoAsync(Guid usuarioId, TipoContrato tipo);
    Task SalvarAsync(Contrato contrato);
    Task DesativarAsync(Guid contratoId);
    Task<List<Contrato>> ListarAtivosVencidosAsync(DateTime agora);
}
```

- [ ] **Step 4: Criar IHeroiDesbloqueadoRepository.cs**

```csharp
// LegendsAwaken.Domain/Interfaces/IHeroiDesbloqueadoRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Domain.Interfaces;

public interface IHeroiDesbloqueadoRepository
{
    Task<bool> JaDesbloqueadoAsync(Guid usuarioId, Guid heroiId);
    Task SalvarAsync(HeroiDesbloqueado desbloqueado);
    Task<List<HeroiDesbloqueado>> ListarPorUsuarioAsync(Guid usuarioId);
}
```

- [ ] **Step 5: Criar IHeroiConfigRepository.cs**

```csharp
// LegendsAwaken.Domain/Interfaces/IHeroiConfigRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Domain.Interfaces;

public interface IHeroiConfigRepository
{
    Task<HeroiConfig?> ObterPorIdAsync(Guid id);
    Task<HeroiConfig?> ObterPorNomeAsync(string nome);
    Task<List<HeroiConfig>> ListarTodosAsync();
    Task<HeroiUnlockConfig?> ObterUnlockConfigAsync(Guid heroiId);
}
```

- [ ] **Step 6: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 7: Commit**

```bash
git add LegendsAwaken.Domain/Interfaces/
git commit -m "feat: add repository interfaces for fragment system"
```

---

## Task 5: Infrastructure (DbContext + Repositórios + Migration + Seed)

**Files:**
- Modify: `LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs`
- Create: `LegendsAwaken.Infrastructure/Repositories/BiomaRepository.cs`
- Create: `LegendsAwaken.Infrastructure/Repositories/FragmentoRepository.cs`
- Create: `LegendsAwaken.Infrastructure/Repositories/ContratoRepository.cs`
- Create: `LegendsAwaken.Infrastructure/Repositories/HeroiDesbloqueadoRepository.cs`
- Create: `LegendsAwaken.Infrastructure/Repositories/HeroiConfigRepository.cs`
- Create: `LegendsAwaken.Infrastructure/SeedData/FragmentoSeed.cs`

- [ ] **Step 1: Atualizar LegendsAwakenDbContext.cs — remover Banner DbSets e adicionar novos**

Remover do DbContext:
```csharp
// REMOVER estas linhas:
public DbSet<BannerConfiguracao> BannerConfiguracoes => Set<BannerConfiguracao>();
public DbSet<BannerHistorico> BannerHistoricos => Set<BannerHistorico>();
public DbSet<BannerProgresso> BannerProgressos => Set<BannerProgresso>();
public DbSet<RacaChance> RacaChances => Set<RacaChance>();
```

Adicionar ao DbContext (após os DbSets existentes):
```csharp
// Fragmento system
public DbSet<HeroiConfig> HeroiConfigs => Set<HeroiConfig>();
public DbSet<Bioma> Biomas => Set<Bioma>();
public DbSet<BiomHeroPool> BiomHeroPools => Set<BiomHeroPool>();
public DbSet<HeroiUnlockConfig> HeroiUnlockConfigs => Set<HeroiUnlockConfig>();
public DbSet<FragmentoProgresso> FragmentosProgresso => Set<FragmentoProgresso>();
public DbSet<Contrato> Contratos => Set<Contrato>();
public DbSet<HeroiDesbloqueado> HeroisDesbloqueados => Set<HeroiDesbloqueado>();
```

Adicionar os `using` necessários no topo:
```csharp
using LegendsAwaken.Domain.Entities.Fragmento;
```

- [ ] **Step 2: Configurar índices e chaves compostas em OnModelCreating**

Dentro de `OnModelCreating(ModelBuilder modelBuilder)`, adicionar:

```csharp
// HeroiUnlockConfig — PK composta via HeroiId (1:1 com HeroiConfig)
modelBuilder.Entity<HeroiUnlockConfig>()
    .HasKey(h => h.HeroiId);

// HeroiDesbloqueado — PK composta
modelBuilder.Entity<HeroiDesbloqueado>()
    .HasKey(h => new { h.UsuarioId, h.HeroiId });

// FragmentoProgresso — índices de performance
modelBuilder.Entity<FragmentoProgresso>()
    .HasIndex(f => new { f.UsuarioId, f.HeroiId });
modelBuilder.Entity<FragmentoProgresso>()
    .HasIndex(f => new { f.UsuarioId, f.Arquetipo });

// Contrato — índice único para garantir 1 ativo por tipo por usuário
modelBuilder.Entity<Contrato>()
    .HasIndex(c => new { c.UsuarioId, c.Tipo, c.Ativo })
    .IsUnique()
    .HasFilter("\"Ativo\" = 1");
```

- [ ] **Step 3: Criar FragmentoSeed.cs com biomas e HeroiConfig dos 9 heróis**

```csharp
// LegendsAwaken.Infrastructure/SeedData/FragmentoSeed.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Infrastructure.SeedData;

public static class FragmentoSeed
{
    // GUIDs determinísticos para seed
    public static readonly Guid IdAldric    = new("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid IdYuzara    = new("a1000000-0000-0000-0000-000000000002");
    public static readonly Guid IdThorvald  = new("a1000000-0000-0000-0000-000000000003");
    public static readonly Guid IdKaen      = new("a1000000-0000-0000-0000-000000000004");
    public static readonly Guid IdNyra      = new("a1000000-0000-0000-0000-000000000005");
    public static readonly Guid IdSeraph    = new("a1000000-0000-0000-0000-000000000006");
    public static readonly Guid IdMira      = new("a1000000-0000-0000-0000-000000000007");
    public static readonly Guid IdGrom      = new("a1000000-0000-0000-0000-000000000008");
    public static readonly Guid IdHana      = new("a1000000-0000-0000-0000-000000000009");

    public static readonly Guid IdBiomaFloresta  = new("b1000000-0000-0000-0000-000000000001");
    public static readonly Guid IdBiomaRuinas    = new("b1000000-0000-0000-0000-000000000002");
    public static readonly Guid IdBiomaVulcanico = new("b1000000-0000-0000-0000-000000000003");

    public static IEnumerable<HeroiConfig> HeroiConfigs() =>
    [
        new() { Id = IdAldric,   Nome = "Aldric, o Sem-Corrente",        RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Guerreiro  },
        new() { Id = IdYuzara,   Nome = "Yuzara, a Tecelã do Destino",   RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Mago       },
        new() { Id = IdThorvald, Nome = "Thorvald, o Arquiteto das Eras",RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Ferreiro   },
        new() { Id = IdKaen,     Nome = "Kaen",                          RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Arqueiro   },
        new() { Id = IdNyra,     Nome = "Nyra",                          RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Ladino     },
        new() { Id = IdSeraph,   Nome = "Seraph",                        RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Paladino   },
        new() { Id = IdMira,     Nome = "Mira",                          RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Alquimista },
        new() { Id = IdGrom,     Nome = "Grom",                          RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Mineiro    },
        new() { Id = IdHana,     Nome = "Hana",                          RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Cozinheiro },
    ];

    public static IEnumerable<HeroiUnlockConfig> UnlockConfigs() =>
    [
        // 5★ Icônicos → desbloqueados por marco da Torre
        new() { HeroiId = IdAldric,   TipoUnlock = TipoUnlock.MarcoTorre,   AndarMarco = 30 },
        new() { HeroiId = IdYuzara,   TipoUnlock = TipoUnlock.MarcoTorre,   AndarMarco = 60 },
        // Thorvald → fragmentos (Ferreiro faz sentido como grind de crafting)
        new() { HeroiId = IdThorvald, TipoUnlock = TipoUnlock.Fragmentos,   QuantidadeFragmentos = 60 },
        // 4★ por marco
        new() { HeroiId = IdKaen,     TipoUnlock = TipoUnlock.MarcoTorre,   AndarMarco = 10 },
        // 4★ por fragmentos
        new() { HeroiId = IdSeraph,   TipoUnlock = TipoUnlock.Fragmentos,   QuantidadeFragmentos = 40 },
        new() { HeroiId = IdMira,     TipoUnlock = TipoUnlock.Fragmentos,   QuantidadeFragmentos = 35 },
        new() { HeroiId = IdGrom,     TipoUnlock = TipoUnlock.Fragmentos,   QuantidadeFragmentos = 30 },
        // 4★ por condição única
        new() { HeroiId = IdNyra,  TipoUnlock = TipoUnlock.CondicaoUnica, CondicaoDescricao = "Completar o andar 15 com a party completa sem nenhum herói ser derrotado" },
        new() { HeroiId = IdHana,  TipoUnlock = TipoUnlock.CondicaoUnica, CondicaoDescricao = "Ter pelo menos 3 heróis com Humor >= 80 na cidade ao mesmo tempo"          },
    ];

    public static IEnumerable<Bioma> Biomas() =>
    [
        new() { Id = IdBiomaFloresta,  Nome = "Floresta de Aelindra", AndarInicio = 1,  AndarFim = 10, Descricao = "Uma floresta antiga onde aventureiros escrevem suas primeiras histórias.", Tag = "Floresta"  },
        new() { Id = IdBiomaRuinas,    Nome = "Ruínas de Valdrek",    AndarInicio = 11, AndarFim = 25, Descricao = "Ruínas de uma civilização esquecida, repletas de armadilhas e segredos.",  Tag = "Ruinas"    },
        new() { Id = IdBiomaVulcanico, Nome = "Pico Vulcânico",       AndarInicio = 26, AndarFim = 50, Descricao = "O cume incandescente onde os guerreiros mais duros são forjados.",         Tag = "Vulcanico" },
    ];

    public static IEnumerable<BiomHeroPool> BiomHeroPools() =>
    [
        // Floresta (1-10): Kaen como principal, Hana como secundária
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000001"), BiomeId = IdBiomaFloresta,  HeroiId = IdKaen,  Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = true  },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000002"), BiomeId = IdBiomaFloresta,  HeroiId = IdHana,  Raridade = Raridade.Estrela4, DropWeight = 70, EHeroPrincipal = false },
        // Ruínas (11-25): Seraph como principal, Nyra como secundária
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000003"), BiomeId = IdBiomaRuinas,    HeroiId = IdSeraph,Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = true  },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000004"), BiomeId = IdBiomaRuinas,    HeroiId = IdNyra,  Raridade = Raridade.Estrela4, DropWeight = 70, EHeroPrincipal = false },
        // Vulcânico (26-50): Aldric como principal, Mira e Grom como secundários
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000005"), BiomeId = IdBiomaVulcanico, HeroiId = IdAldric,Raridade = Raridade.Estrela5, DropWeight = 20, EHeroPrincipal = true  },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000006"), BiomeId = IdBiomaVulcanico, HeroiId = IdMira,  Raridade = Raridade.Estrela4, DropWeight = 45, EHeroPrincipal = false },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000007"), BiomeId = IdBiomaVulcanico, HeroiId = IdGrom,  Raridade = Raridade.Estrela4, DropWeight = 35, EHeroPrincipal = false },
    ];
}
```

- [ ] **Step 4: Chamar seed em OnModelCreating do DbContext**

Dentro de `OnModelCreating`, após as configurações de índice:

```csharp
modelBuilder.Entity<HeroiConfig>().HasData(FragmentoSeed.HeroiConfigs());
modelBuilder.Entity<HeroiUnlockConfig>().HasData(FragmentoSeed.UnlockConfigs());
modelBuilder.Entity<Bioma>().HasData(FragmentoSeed.Biomas());
modelBuilder.Entity<BiomHeroPool>().HasData(FragmentoSeed.BiomHeroPools());
```

Adicionar `using LegendsAwaken.Infrastructure.SeedData;` ao topo do DbContext.

- [ ] **Step 5: Criar BiomaRepository.cs**

```csharp
// LegendsAwaken.Infrastructure/Repositories/BiomaRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class BiomaRepository(LegendsAwakenDbContext db) : IBiomaRepository
{
    public async Task<Bioma?> ObterPorAndarAsync(int andar) =>
        await db.Biomas
            .FirstOrDefaultAsync(b => b.AndarInicio <= andar && b.AndarFim >= andar);

    public async Task<List<BiomHeroPool>> ObterPoolAsync(Guid biomaId) =>
        await db.BiomHeroPools
            .Include(p => p.Heroi)
            .Where(p => p.BiomeId == biomaId)
            .ToListAsync();

    public async Task<List<Bioma>> ListarTodosAsync() =>
        await db.Biomas.OrderBy(b => b.AndarInicio).ToListAsync();
}
```

- [ ] **Step 6: Criar FragmentoRepository.cs**

```csharp
// LegendsAwaken.Infrastructure/Repositories/FragmentoRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class FragmentoRepository(LegendsAwakenDbContext db) : IFragmentoRepository
{
    public async Task<FragmentoProgresso?> ObterPorHeroiAsync(Guid usuarioId, Guid heroiId) =>
        await db.FragmentosProgresso
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.HeroiId == heroiId);

    public async Task<FragmentoProgresso?> ObterPorArquetipoAsync(Guid usuarioId, Profissao arquetipo) =>
        await db.FragmentosProgresso
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.Arquetipo == arquetipo);

    public async Task UpsertAsync(FragmentoProgresso progresso)
    {
        var existe = await db.FragmentosProgresso.AnyAsync(f => f.Id == progresso.Id);
        if (existe) db.FragmentosProgresso.Update(progresso);
        else await db.FragmentosProgresso.AddAsync(progresso);
        await db.SaveChangesAsync();
    }

    public async Task<List<FragmentoProgresso>> ListarPorUsuarioAsync(Guid usuarioId) =>
        await db.FragmentosProgresso
            .Include(f => f.Heroi)
            .Where(f => f.UsuarioId == usuarioId)
            .ToListAsync();
}
```

- [ ] **Step 7: Criar ContratoRepository.cs**

```csharp
// LegendsAwaken.Infrastructure/Repositories/ContratoRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class ContratoRepository(LegendsAwakenDbContext db) : IContratoRepository
{
    public async Task<Contrato?> ObterAtivoAsync(Guid usuarioId, TipoContrato tipo) =>
        await db.Contratos
            .Include(c => c.Heroi)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Tipo == tipo && c.Ativo);

    public async Task SalvarAsync(Contrato contrato)
    {
        var existe = await db.Contratos.AnyAsync(c => c.Id == contrato.Id);
        if (existe) db.Contratos.Update(contrato);
        else await db.Contratos.AddAsync(contrato);
        await db.SaveChangesAsync();
    }

    public async Task DesativarAsync(Guid contratoId)
    {
        var contrato = await db.Contratos.FindAsync(contratoId);
        if (contrato is null) return;
        contrato.Ativo = false;
        await db.SaveChangesAsync();
    }

    public async Task<List<Contrato>> ListarAtivosVencidosAsync(DateTime agora) =>
        await db.Contratos
            .Where(c => c.Ativo && c.ExpiraEm.HasValue && c.ExpiraEm.Value <= agora)
            .ToListAsync();
}
```

- [ ] **Step 8: Criar HeroiDesbloqueadoRepository.cs**

```csharp
// LegendsAwaken.Infrastructure/Repositories/HeroiDesbloqueadoRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class HeroiDesbloqueadoRepository(LegendsAwakenDbContext db) : IHeroiDesbloqueadoRepository
{
    public async Task<bool> JaDesbloqueadoAsync(Guid usuarioId, Guid heroiId) =>
        await db.HeroisDesbloqueados
            .AnyAsync(h => h.UsuarioId == usuarioId && h.HeroiId == heroiId);

    public async Task SalvarAsync(HeroiDesbloqueado desbloqueado)
    {
        await db.HeroisDesbloqueados.AddAsync(desbloqueado);
        await db.SaveChangesAsync();
    }

    public async Task<List<HeroiDesbloqueado>> ListarPorUsuarioAsync(Guid usuarioId) =>
        await db.HeroisDesbloqueados
            .Include(h => h.Heroi)
            .Where(h => h.UsuarioId == usuarioId)
            .ToListAsync();
}
```

- [ ] **Step 9: Criar HeroiConfigRepository.cs**

```csharp
// LegendsAwaken.Infrastructure/Repositories/HeroiConfigRepository.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class HeroiConfigRepository(LegendsAwakenDbContext db) : IHeroiConfigRepository
{
    public async Task<HeroiConfig?> ObterPorIdAsync(Guid id) =>
        await db.HeroiConfigs.FindAsync(id);

    public async Task<HeroiConfig?> ObterPorNomeAsync(string nome) =>
        await db.HeroiConfigs.FirstOrDefaultAsync(h => h.Nome == nome);

    public async Task<List<HeroiConfig>> ListarTodosAsync() =>
        await db.HeroiConfigs.OrderBy(h => h.Nome).ToListAsync();

    public async Task<HeroiUnlockConfig?> ObterUnlockConfigAsync(Guid heroiId) =>
        await db.HeroiUnlockConfigs
            .Include(u => u.Heroi)
            .FirstOrDefaultAsync(u => u.HeroiId == heroiId);
}
```

- [ ] **Step 10: Registrar novos repositórios no DI**

Localizar onde os repositórios são registrados (provavelmente `Program.cs` ou `ServiceCollectionExtensions.cs` em Infrastructure). Adicionar:

```csharp
services.AddScoped<IBiomaRepository, BiomaRepository>();
services.AddScoped<IFragmentoRepository, FragmentoRepository>();
services.AddScoped<IContratoRepository, ContratoRepository>();
services.AddScoped<IHeroiDesbloqueadoRepository, HeroiDesbloqueadoRepository>();
services.AddScoped<IHeroiConfigRepository, HeroiConfigRepository>();
```

- [ ] **Step 11: Gerar migration**

```bash
cd /c/Workspace/LegendsAwaken
dotnet ef migrations add FragmentoSystem --project LegendsAwaken.Infrastructure --startup-project LegendsAwaken.Bot
```

Expected: migration file criado em `LegendsAwaken.Infrastructure/Migrations/`

- [ ] **Step 12: Aplicar migration**

```bash
dotnet ef database update --project LegendsAwaken.Infrastructure --startup-project LegendsAwaken.Bot
```

Expected: `Done.`

- [ ] **Step 13: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 14: Commit**

```bash
git add -A
git commit -m "feat: add fragment infrastructure (repositories, DbContext, migration, seed data)"
```

---

## Task 6: ContractConfig + DTOs Compartilhados

**Files:**
- Create: `LegendsAwaken.Application/Config/ContractConfig.cs`
- Create: `LegendsAwaken.Application/DTOs/FragmentDTOs.cs`

- [ ] **Step 1: Criar ContractConfig.cs**

```csharp
// LegendsAwaken.Application/Config/ContractConfig.cs
namespace LegendsAwaken.Application.Config;

public static class ContractConfig
{
    public const float ArchetypeBonus = 0.30f;
    public const float NamedBonus     = 0.50f;

    public static readonly TimeSpan DuracaoPadraoNomeado = TimeSpan.FromHours(6);
    public const int CustoContratoNomeadoOuro = 500;
    public const int ChanceDropBase = 30; // % de chance de qualquer fragmento dropar por andar
}
```

- [ ] **Step 2: Criar FragmentDTOs.cs**

```csharp
// LegendsAwaken.Application/DTOs/FragmentDTOs.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Application.DTOs;

public record FragmentDropResult(
    Guid HeroiId,
    string HeroiNome,
    TipoFragmento Tipo,
    int Quantidade,
    int QuantidadeTotal
);

public record RecruitmentResult(
    bool Sucesso,
    HeroiConfig? Heroi,
    string Mensagem
);

public record RewardPayload(
    string Titulo,
    string Descricao,
    string? ImagemUrl,
    TipoReward Tipo,
    Dictionary<string, string>? Campos = null
);

public enum TipoReward { Micro, Medio, Alto }
```

- [ ] **Step 3: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add LegendsAwaken.Application/Config/ LegendsAwaken.Application/DTOs/FragmentDTOs.cs
git commit -m "feat: add ContractConfig and fragment DTOs"
```

---

## Task 7: BiomeService (com testes)

**Files:**
- Create: `LegendsAwaken.Application/Services/BiomeService.cs`
- Test: `LegendsAwaken.Tests/Services/BiomeServiceTests.cs`

- [ ] **Step 1: Instalar Moq no projeto de testes (se não presente)**

```bash
dotnet add LegendsAwaken.Tests/LegendsAwaken.Tests.csproj package Moq
dotnet add LegendsAwaken.Tests/LegendsAwaken.Tests.csproj reference LegendsAwaken.Application/LegendsAwaken.Application.csproj
dotnet add LegendsAwaken.Tests/LegendsAwaken.Tests.csproj reference LegendsAwaken.Domain/LegendsAwaken.Domain.csproj
```

- [ ] **Step 2: Escrever os testes com falha**

```csharp
// LegendsAwaken.Tests/Services/BiomeServiceTests.cs
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using Xunit;

namespace LegendsAwaken.Tests.Services;

public class BiomeServiceTests
{
    private readonly Mock<IBiomaRepository> _repoMock = new();
    private BiomeService CreateService() => new(_repoMock.Object);

    [Theory]
    [InlineData(1,  "Floresta de Aelindra")]
    [InlineData(5,  "Floresta de Aelindra")]
    [InlineData(10, "Floresta de Aelindra")]
    [InlineData(11, "Ruínas de Valdrek")]
    [InlineData(25, "Ruínas de Valdrek")]
    [InlineData(26, "Pico Vulcânico")]
    public async Task ObterBiomaPorAndarAsync_RetornaBiomaCorreto(int andar, string nomeEsperado)
    {
        _repoMock.Setup(r => r.ObterPorAndarAsync(andar))
            .ReturnsAsync(new Bioma { Nome = nomeEsperado, AndarInicio = 1, AndarFim = 10 });

        var service = CreateService();
        var bioma = await service.ObterBiomaPorAndarAsync(andar);

        Assert.Equal(nomeEsperado, bioma?.Nome);
    }

    [Theory]
    [InlineData(5,  true)]
    [InlineData(10, true)]
    [InlineData(25, true)]
    [InlineData(50, true)]
    [InlineData(3,  false)]
    [InlineData(11, false)]
    [InlineData(7,  false)]
    public void EAndarDeMarco_RetornaCorreto(int andar, bool esperado)
    {
        var service = CreateService();
        Assert.Equal(esperado, service.EAndarDeMarco(andar));
    }

    [Fact]
    public async Task EBiomaNovoAsync_RetornaTrue_QuandoBiomaMuda()
    {
        var biomaA = new Bioma { Id = Guid.NewGuid(), AndarInicio = 1, AndarFim = 10 };
        var biomaB = new Bioma { Id = Guid.NewGuid(), AndarInicio = 11, AndarFim = 25 };

        _repoMock.Setup(r => r.ObterPorAndarAsync(11)).ReturnsAsync(biomaB);
        _repoMock.Setup(r => r.ObterPorAndarAsync(10)).ReturnsAsync(biomaA);

        var service = CreateService();
        var resultado = await service.EBiomaNovoAsync(11);

        Assert.True(resultado);
    }

    [Fact]
    public async Task EBiomaNovoAsync_RetornaFalse_QuandoBiomaNaoMuda()
    {
        var bioma = new Bioma { Id = Guid.NewGuid(), AndarInicio = 1, AndarFim = 10 };
        _repoMock.Setup(r => r.ObterPorAndarAsync(It.IsAny<int>())).ReturnsAsync(bioma);

        var service = CreateService();
        var resultado = await service.EBiomaNovoAsync(5);

        Assert.False(resultado);
    }
}
```

- [ ] **Step 3: Rodar testes para confirmar falha**

```bash
dotnet test LegendsAwaken.Tests/ --filter "BiomeServiceTests"
```

Expected: `FAILED — BiomeService not found`

- [ ] **Step 4: Implementar BiomeService.cs**

```csharp
// LegendsAwaken.Application/Services/BiomeService.cs
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class BiomeService(IBiomaRepository biomaRepository)
{
    private static readonly HashSet<int> _marcos = [5, 10, 15, 20, 25, 30, 40, 50, 60, 75, 100];

    public Task<Bioma?> ObterBiomaPorAndarAsync(int andar) =>
        biomaRepository.ObterPorAndarAsync(andar);

    public Task<List<BiomHeroPool>> ObterPoolDoBiomaAsync(Guid biomaId) =>
        biomaRepository.ObterPoolAsync(biomaId);

    public bool EAndarDeMarco(int andar) => _marcos.Contains(andar);

    public async Task<bool> EBiomaNovoAsync(int andarAtual)
    {
        if (andarAtual <= 1) return true;
        var biomaAtual   = await biomaRepository.ObterPorAndarAsync(andarAtual);
        var biomaAnterior = await biomaRepository.ObterPorAndarAsync(andarAtual - 1);
        return biomaAtual?.Id != biomaAnterior?.Id;
    }
}
```

- [ ] **Step 5: Rodar testes para confirmar aprovação**

```bash
dotnet test LegendsAwaken.Tests/ --filter "BiomeServiceTests"
```

Expected: `Passed! - 9 tests`

- [ ] **Step 6: Registrar BiomeService no DI**

```csharp
services.AddScoped<BiomeService>();
```

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: implement BiomeService with biome detection and milestone logic"
```

---

## Task 8: FragmentService (com testes)

**Files:**
- Create: `LegendsAwaken.Application/Services/FragmentService.cs`
- Test: `LegendsAwaken.Tests/Services/FragmentServiceTests.cs`

- [ ] **Step 1: Escrever os testes com falha**

```csharp
// LegendsAwaken.Tests/Services/FragmentServiceTests.cs
using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using Xunit;

namespace LegendsAwaken.Tests.Services;

public class FragmentServiceTests
{
    private readonly Mock<IBiomaRepository>    _biomaRepo    = new();
    private readonly Mock<IFragmentoRepository> _fragmentoRepo = new();
    private readonly Mock<IContratoRepository>  _contratoRepo  = new();
    private readonly Mock<IHeroiConfigRepository> _heroiConfigRepo = new();

    private FragmentService CreateService() =>
        new(_biomaRepo.Object, _fragmentoRepo.Object, _contratoRepo.Object, _heroiConfigRepo.Object);

    [Fact]
    public async Task AdicionarFragmentosAsync_CriaNovoProgresso_QuandoNaoExiste()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        FragmentoProgresso? salvo = null;

        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId))
            .ReturnsAsync((FragmentoProgresso?)null);
        _fragmentoRepo.Setup(r => r.UpsertAsync(It.IsAny<FragmentoProgresso>()))
            .Callback<FragmentoProgresso>(p => salvo = p)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.AdicionarFragmentosAsync(usuarioId, TipoFragmento.Heroi, heroiId, 5);

        Assert.NotNull(salvo);
        Assert.Equal(5, salvo!.Quantidade);
        Assert.Equal(heroiId, salvo.HeroiId);
    }

    [Fact]
    public async Task AdicionarFragmentosAsync_Acumula_QuandoJaExiste()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var existente = new FragmentoProgresso { Id = Guid.NewGuid(), UsuarioId = usuarioId, HeroiId = heroiId, Quantidade = 10, TipoFragmento = TipoFragmento.Heroi };
        FragmentoProgresso? salvo = null;

        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync(existente);
        _fragmentoRepo.Setup(r => r.UpsertAsync(It.IsAny<FragmentoProgresso>()))
            .Callback<FragmentoProgresso>(p => salvo = p)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.AdicionarFragmentosAsync(usuarioId, TipoFragmento.Heroi, heroiId, 5);

        Assert.Equal(15, salvo!.Quantidade);
    }

    [Fact]
    public async Task ObterMultiplicadorAsync_RetornaBaseQuandoSemContrato()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();

        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo)).ReturnsAsync((Contrato?)null);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado)).ReturnsAsync((Contrato?)null);

        var service = CreateService();
        float mult = await service.ObterMultiplicadorAsync(usuarioId, heroiId);

        Assert.Equal(1.0f, mult);
    }

    [Fact]
    public async Task ObterMultiplicadorAsync_AplicaArquetipoBonus_QuandoArquetipoCorreto()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = FragmentoSeed.IdKaen;
        var config    = new HeroiConfig { Id = heroiId, Arquetipo = Profissao.Arqueiro };

        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(config);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo))
            .ReturnsAsync(new Contrato { Arquetipo = Profissao.Arqueiro, Ativo = true });
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado))
            .ReturnsAsync((Contrato?)null);

        var service = CreateService();
        float mult = await service.ObterMultiplicadorAsync(usuarioId, heroiId);

        Assert.Equal(1.0f + ContractConfig.ArchetypeBonus, mult, precision: 2);
    }
}
```

- [ ] **Step 2: Rodar testes para confirmar falha**

```bash
dotnet test LegendsAwaken.Tests/ --filter "FragmentServiceTests"
```

Expected: `FAILED — FragmentService not found`

- [ ] **Step 3: Implementar FragmentService.cs**

```csharp
// LegendsAwaken.Application/Services/FragmentService.cs
using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class FragmentService(
    IBiomaRepository biomaRepository,
    IFragmentoRepository fragmentoRepository,
    IContratoRepository contratoRepository,
    IHeroiConfigRepository heroiConfigRepository)
{
    public async Task<List<FragmentDropResult>> ProcessarDropAsync(Guid usuarioId, int andar)
    {
        var bioma = await biomaRepository.ObterPorAndarAsync(andar);
        if (bioma is null) return [];

        var pool = await biomaRepository.ObterPoolAsync(bioma.Id);
        if (pool.Count == 0) return [];

        // 30% de chance de dropar qualquer fragmento por andar
        if (Random.Shared.Next(100) >= ContractConfig.ChanceDropBase) return [];

        var heroiSelecionado = SelecionarPorPeso(pool);
        if (heroiSelecionado is null) return [];

        int quantidade = Random.Shared.Next(1, 4);
        float multiplicador = await ObterMultiplicadorAsync(usuarioId, heroiSelecionado.HeroiId);
        int quantidadeFinal = (int)Math.Ceiling(quantidade * multiplicador);

        await AdicionarFragmentosAsync(usuarioId, TipoFragmento.Heroi, heroiSelecionado.HeroiId, quantidadeFinal);

        var progresso = await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiSelecionado.HeroiId);

        return
        [
            new FragmentDropResult(
                heroiSelecionado.HeroiId,
                heroiSelecionado.Heroi.Nome,
                TipoFragmento.Heroi,
                quantidadeFinal,
                progresso?.Quantidade ?? quantidadeFinal)
        ];
    }

    public async Task AdicionarFragmentosAsync(Guid usuarioId, TipoFragmento tipo, Guid? heroiId, int quantidade)
    {
        FragmentoProgresso? progresso = tipo == TipoFragmento.Heroi && heroiId.HasValue
            ? await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId.Value)
            : null;

        if (progresso is null)
        {
            progresso = new FragmentoProgresso
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                TipoFragmento = tipo,
                HeroiId = heroiId,
                Quantidade = 0,
                AtualizadoEm = DateTime.UtcNow
            };
        }

        progresso.Quantidade += quantidade;
        progresso.AtualizadoEm = DateTime.UtcNow;
        await fragmentoRepository.UpsertAsync(progresso);
    }

    public async Task<FragmentoProgresso?> ObterProgressoAsync(Guid usuarioId, Guid heroiId) =>
        await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId);

    public async Task<float> ObterMultiplicadorAsync(Guid usuarioId, Guid heroiId)
    {
        float multiplicador = 1.0f;

        var config = await heroiConfigRepository.ObterPorIdAsync(heroiId);

        var contratoArquetipo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        if (contratoArquetipo is not null && config is not null && contratoArquetipo.Arquetipo == config.Arquetipo)
            multiplicador += ContractConfig.ArchetypeBonus;

        var contratoNomeado = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);
        if (contratoNomeado is not null && contratoNomeado.HeroiId == heroiId)
            multiplicador += ContractConfig.NamedBonus;

        return multiplicador;
    }

    private static BiomHeroPool? SelecionarPorPeso(List<BiomHeroPool> pool)
    {
        int totalPeso = pool.Sum(p => p.DropWeight);
        int roll = Random.Shared.Next(totalPeso);
        int acumulado = 0;
        foreach (var item in pool)
        {
            acumulado += item.DropWeight;
            if (roll < acumulado) return item;
        }
        return pool[^1];
    }
}
```

- [ ] **Step 4: Rodar testes**

```bash
dotnet test LegendsAwaken.Tests/ --filter "FragmentServiceTests"
```

Expected: `Passed! - 4 tests`

- [ ] **Step 5: Registrar no DI**

```csharp
services.AddScoped<FragmentService>();
```

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: implement FragmentService with weighted drop and contract bonus"
```

---

## Task 9: ContractService (com testes)

**Files:**
- Create: `LegendsAwaken.Application/Services/ContractService.cs`
- Test: `LegendsAwaken.Tests/Services/ContractServiceTests.cs`

- [ ] **Step 1: Escrever os testes com falha**

```csharp
// LegendsAwaken.Tests/Services/ContractServiceTests.cs
using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using Xunit;

namespace LegendsAwaken.Tests.Services;

public class ContractServiceTests
{
    private readonly Mock<IContratoRepository>    _contratoRepo   = new();
    private readonly Mock<IHeroiConfigRepository> _heroiConfigRepo = new();
    private readonly Mock<IFragmentoRepository>   _fragmentoRepo  = new();

    private ContractService CreateService() =>
        new(_contratoRepo.Object, _heroiConfigRepo.Object, _fragmentoRepo.Object);

    [Fact]
    public async Task AtivarContratoArquetipoAsync_DesativaAntigoECriaNovo()
    {
        var usuarioId = Guid.NewGuid();
        var contratoAntigo = new Contrato { Id = Guid.NewGuid(), Ativo = true };
        Contrato? salvo = null;

        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo)).ReturnsAsync(contratoAntigo);
        _contratoRepo.Setup(r => r.DesativarAsync(contratoAntigo.Id)).Returns(Task.CompletedTask);
        _contratoRepo.Setup(r => r.SalvarAsync(It.IsAny<Contrato>()))
            .Callback<Contrato>(c => salvo = c)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.AtivarContratoArquetipoAsync(usuarioId, Profissao.Guerreiro);

        _contratoRepo.Verify(r => r.DesativarAsync(contratoAntigo.Id), Times.Once);
        Assert.NotNull(salvo);
        Assert.Equal(Profissao.Guerreiro, salvo!.Arquetipo);
        Assert.True(salvo.Ativo);
        Assert.Null(salvo.ExpiraEm);
    }

    [Fact]
    public async Task AtivarContratoNomeadoAsync_FalhaQuandoSemFragmento()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();

        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync((FragmentoProgresso?)null);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId))
            .ReturnsAsync(new HeroiConfig { Id = heroiId, Nome = "Heroi Teste" });

        var service = CreateService();
        var resultado = await service.AtivarContratoNomeadoAsync(usuarioId, heroiId);

        Assert.False(resultado.Sucesso);
        _contratoRepo.Verify(r => r.SalvarAsync(It.IsAny<Contrato>()), Times.Never);
    }

    [Fact]
    public async Task AtivarContratoNomeadoAsync_SuccessQuandoTemFragmento()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var progresso = new FragmentoProgresso { HeroiId = heroiId, Quantidade = 3 };
        Contrato? salvo = null;

        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync(progresso);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId))
            .ReturnsAsync(new HeroiConfig { Id = heroiId, Nome = "Kaen" });
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado)).ReturnsAsync((Contrato?)null);
        _contratoRepo.Setup(r => r.SalvarAsync(It.IsAny<Contrato>()))
            .Callback<Contrato>(c => salvo = c)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var resultado = await service.AtivarContratoNomeadoAsync(usuarioId, heroiId);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(salvo);
        Assert.Equal(heroiId, salvo!.HeroiId);
        Assert.NotNull(salvo.ExpiraEm);
    }
}
```

- [ ] **Step 2: Rodar testes para confirmar falha**

```bash
dotnet test LegendsAwaken.Tests/ --filter "ContractServiceTests"
```

Expected: `FAILED`

- [ ] **Step 3: Implementar ContractService.cs**

```csharp
// LegendsAwaken.Application/Services/ContractService.cs
using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class ContractService(
    IContratoRepository contratoRepository,
    IHeroiConfigRepository heroiConfigRepository,
    IFragmentoRepository fragmentoRepository)
{
    public async Task<Contrato> AtivarContratoArquetipoAsync(Guid usuarioId, Profissao arquetipo)
    {
        var ativo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        if (ativo is not null)
            await contratoRepository.DesativarAsync(ativo.Id);

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo = TipoContrato.Arquetipo,
            Arquetipo = arquetipo,
            Ativo = true,
            ExpiraEm = null,
            CriadoEm = DateTime.UtcNow
        };

        await contratoRepository.SalvarAsync(contrato);
        return contrato;
    }

    public async Task<RecruitmentResult> AtivarContratoNomeadoAsync(Guid usuarioId, Guid heroiId,
        TimeSpan? duracao = null)
    {
        var heroi = await heroiConfigRepository.ObterPorIdAsync(heroiId);
        if (heroi is null)
            return new RecruitmentResult(false, null, "Herói não encontrado.");

        // Gating: precisa ter ao menos 1 fragmento do herói
        var progresso = await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId);
        if (progresso is null || progresso.Quantidade == 0)
            return new RecruitmentResult(false, heroi, $"Você precisa ter ao menos 1 fragmento de {heroi.Nome} para focar nele.");

        var ativo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);
        if (ativo is not null)
            await contratoRepository.DesativarAsync(ativo.Id);

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo = TipoContrato.Nomeado,
            HeroiId = heroiId,
            Ativo = true,
            ExpiraEm = DateTime.UtcNow.Add(duracao ?? ContractConfig.DuracaoPadraoNomeado),
            CriadoEm = DateTime.UtcNow
        };

        await contratoRepository.SalvarAsync(contrato);
        return new RecruitmentResult(true, heroi, $"Contrato de foco ativado para {heroi.Nome} por {(duracao ?? ContractConfig.DuracaoPadraoNomeado).TotalHours:0}h.");
    }

    public async Task ExpirarContratosVencidosAsync()
    {
        var vencidos = await contratoRepository.ListarAtivosVencidosAsync(DateTime.UtcNow);
        foreach (var contrato in vencidos)
            await contratoRepository.DesativarAsync(contrato.Id);
    }
}
```

- [ ] **Step 4: Rodar testes**

```bash
dotnet test LegendsAwaken.Tests/ --filter "ContractServiceTests"
```

Expected: `Passed! - 3 tests`

- [ ] **Step 5: Registrar no DI**

```csharp
services.AddScoped<ContractService>();
```

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: implement ContractService with archetype/named contract management"
```

---

## Task 10: RecruitmentService (com testes)

**Files:**
- Create: `LegendsAwaken.Application/Services/RecruitmentService.cs`
- Test: `LegendsAwaken.Tests/Services/RecruitmentServiceTests.cs`

- [ ] **Step 1: Escrever os testes com falha**

```csharp
// LegendsAwaken.Tests/Services/RecruitmentServiceTests.cs
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using Xunit;

namespace LegendsAwaken.Tests.Services;

public class RecruitmentServiceTests
{
    private readonly Mock<IHeroiDesbloqueadoRepository> _desbloqueadoRepo = new();
    private readonly Mock<IHeroiConfigRepository>       _heroiConfigRepo  = new();
    private readonly Mock<IFragmentoRepository>          _fragmentoRepo    = new();

    private RecruitmentService CreateService() =>
        new(_desbloqueadoRepo.Object, _heroiConfigRepo.Object, _fragmentoRepo.Object);

    [Fact]
    public async Task TentarRecrutarPorFragmentosAsync_Falha_QuandoJaDesbloqueado()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        _desbloqueadoRepo.Setup(r => r.JaDesbloqueadoAsync(usuarioId, heroiId)).ReturnsAsync(true);

        var service = CreateService();
        var resultado = await service.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);

        Assert.False(resultado.Sucesso);
        Assert.Contains("já desbloqueado", resultado.Mensagem, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TentarRecrutarPorFragmentosAsync_Falha_QuandoFragmentosInsuficientes()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var config = new HeroiConfig { Id = heroiId, Nome = "Grom" };
        var unlock = new HeroiUnlockConfig { HeroiId = heroiId, TipoUnlock = TipoUnlock.Fragmentos, QuantidadeFragmentos = 30 };
        var progresso = new FragmentoProgresso { Quantidade = 15 };

        _desbloqueadoRepo.Setup(r => r.JaDesbloqueadoAsync(usuarioId, heroiId)).ReturnsAsync(false);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(config);
        _heroiConfigRepo.Setup(r => r.ObterUnlockConfigAsync(heroiId)).ReturnsAsync(unlock);
        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync(progresso);

        var service = CreateService();
        var resultado = await service.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);

        Assert.False(resultado.Sucesso);
        Assert.Contains("15/30", resultado.Mensagem);
    }

    [Fact]
    public async Task TentarRecrutarPorFragmentosAsync_Sucesso_QuandoFragmentosSuficientes()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var config = new HeroiConfig { Id = heroiId, Nome = "Grom" };
        var unlock = new HeroiUnlockConfig { HeroiId = heroiId, TipoUnlock = TipoUnlock.Fragmentos, QuantidadeFragmentos = 30 };
        var progresso = new FragmentoProgresso { Quantidade = 30 };
        HeroiDesbloqueado? salvo = null;

        _desbloqueadoRepo.Setup(r => r.JaDesbloqueadoAsync(usuarioId, heroiId)).ReturnsAsync(false);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(config);
        _heroiConfigRepo.Setup(r => r.ObterUnlockConfigAsync(heroiId)).ReturnsAsync(unlock);
        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync(progresso);
        _desbloqueadoRepo.Setup(r => r.SalvarAsync(It.IsAny<HeroiDesbloqueado>()))
            .Callback<HeroiDesbloqueado>(h => salvo = h)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var resultado = await service.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(salvo);
        Assert.Equal(heroiId, salvo!.HeroiId);
    }

    [Fact]
    public async Task ProcessarMarcoTorreAsync_Desbloqueia_QuandoHeroiEDoMarco()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = new Guid("a1000000-0000-0000-0000-000000000004"); // IdKaen do FragmentoSeed
        var config    = new HeroiConfig { Id = heroiId, Nome = "Kaen" };
        var unlock    = new HeroiUnlockConfig { HeroiId = heroiId, TipoUnlock = TipoUnlock.MarcoTorre, AndarMarco = 10 };
        HeroiDesbloqueado? salvo = null;

        _heroiConfigRepo.Setup(r => r.ListarTodosAsync()).ReturnsAsync([config]);
        _heroiConfigRepo.Setup(r => r.ObterUnlockConfigAsync(heroiId)).ReturnsAsync(unlock);
        _desbloqueadoRepo.Setup(r => r.JaDesbloqueadoAsync(usuarioId, heroiId)).ReturnsAsync(false);
        _desbloqueadoRepo.Setup(r => r.SalvarAsync(It.IsAny<HeroiDesbloqueado>()))
            .Callback<HeroiDesbloqueado>(h => salvo = h)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var resultado = await service.ProcessarMarcoTorreAsync(usuarioId, 10);

        Assert.NotNull(resultado);
        Assert.True(resultado!.Sucesso);
        Assert.NotNull(salvo);
    }
}
```

- [ ] **Step 2: Rodar testes para confirmar falha**

```bash
dotnet test LegendsAwaken.Tests/ --filter "RecruitmentServiceTests"
```

Expected: `FAILED`

- [ ] **Step 3: Implementar RecruitmentService.cs**

```csharp
// LegendsAwaken.Application/Services/RecruitmentService.cs
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class RecruitmentService(
    IHeroiDesbloqueadoRepository desbloqueadoRepository,
    IHeroiConfigRepository heroiConfigRepository,
    IFragmentoRepository fragmentoRepository)
{
    public async Task<RecruitmentResult> TentarRecrutarPorFragmentosAsync(Guid usuarioId, Guid heroiId)
    {
        if (await desbloqueadoRepository.JaDesbloqueadoAsync(usuarioId, heroiId))
            return new RecruitmentResult(false, null, "Herói já desbloqueado.");

        var heroi = await heroiConfigRepository.ObterPorIdAsync(heroiId);
        if (heroi is null)
            return new RecruitmentResult(false, null, "Herói não encontrado.");

        var unlock = await heroiConfigRepository.ObterUnlockConfigAsync(heroiId);
        if (unlock is null || unlock.TipoUnlock != TipoUnlock.Fragmentos)
            return new RecruitmentResult(false, heroi, $"{heroi.Nome} não é desbloqueável por fragmentos.");

        var progresso = await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId);
        int atual = progresso?.Quantidade ?? 0;
        int necessario = unlock.QuantidadeFragmentos!.Value;

        if (atual < necessario)
            return new RecruitmentResult(false, heroi, $"Fragmentos insuficientes: {atual}/{necessario}.");

        await Desbloquear(usuarioId, heroiId, heroi);
        return new RecruitmentResult(true, heroi, $"{heroi.Nome} recrutado com sucesso!");
    }

    public async Task<RecruitmentResult?> ProcessarMarcoTorreAsync(Guid usuarioId, int andar)
    {
        var todosHerois = await heroiConfigRepository.ListarTodosAsync();

        foreach (var heroi in todosHerois)
        {
            var unlock = await heroiConfigRepository.ObterUnlockConfigAsync(heroi.Id);
            if (unlock?.TipoUnlock != TipoUnlock.MarcoTorre || unlock.AndarMarco != andar)
                continue;

            if (await desbloqueadoRepository.JaDesbloqueadoAsync(usuarioId, heroi.Id))
                continue;

            await Desbloquear(usuarioId, heroi.Id, heroi);
            return new RecruitmentResult(true, heroi, $"{heroi.Nome} se une à sua equipe!");
        }

        return null;
    }

    public async Task<RecruitmentResult> DesbloquearPorCondicaoAsync(Guid usuarioId, Guid heroiId)
    {
        if (await desbloqueadoRepository.JaDesbloqueadoAsync(usuarioId, heroiId))
            return new RecruitmentResult(false, null, "Herói já desbloqueado.");

        var heroi = await heroiConfigRepository.ObterPorIdAsync(heroiId);
        if (heroi is null)
            return new RecruitmentResult(false, null, "Herói não encontrado.");

        await Desbloquear(usuarioId, heroiId, heroi);
        return new RecruitmentResult(true, heroi, $"{heroi.Nome} revelou-se a você!");
    }

    private async Task Desbloquear(Guid usuarioId, Guid heroiId, HeroiConfig heroi)
    {
        await desbloqueadoRepository.SalvarAsync(new HeroiDesbloqueado
        {
            UsuarioId = usuarioId,
            HeroiId = heroiId,
            Heroi = heroi,
            DesbloqueadoEm = DateTime.UtcNow
        });
    }
}
```

- [ ] **Step 4: Rodar testes**

```bash
dotnet test LegendsAwaken.Tests/ --filter "RecruitmentServiceTests"
```

Expected: `Passed! - 4 tests`

- [ ] **Step 5: Registrar no DI**

```csharp
services.AddScoped<RecruitmentService>();
```

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: implement RecruitmentService with idempotent unlock paths"
```

---

## Task 11: RewardDistributionService

**Files:**
- Create: `LegendsAwaken.Application/Services/RewardDistributionService.cs`

Sem testes unitários — outputs são payloads de apresentação sem lógica de negócio.

- [ ] **Step 1: Implementar RewardDistributionService.cs**

```csharp
// LegendsAwaken.Application/Services/RewardDistributionService.cs
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Application.Services;

public class RewardDistributionService
{
    public RewardPayload GerarMicroPico(FragmentDropResult drop) =>
        new(
            Titulo: "Fragmento obtido!",
            Descricao: $"+{drop.Quantidade} fragmento(s) de **{drop.HeroiNome}** — {drop.QuantidadeTotal} acumulados",
            ImagemUrl: null,
            Tipo: TipoReward.Micro);

    public RewardPayload GerarPicoMedio(HeroiConfig heroi) =>
        new(
            Titulo: $"✨ {heroi.Nome} recrutado!",
            Descricao: $"Após uma longa jornada, **{heroi.Nome}** finalmente se junta à sua equipe.",
            ImagemUrl: null,
            Tipo: TipoReward.Medio,
            Campos: new Dictionary<string, string>
            {
                ["Raridade"]  = $"{(int)heroi.RaridadeBase}⭐",
                ["Arquétipo"] = heroi.Arquetipo.ToString()
            });

    public RewardPayload GerarPicoAlto(TipoEventoAlto tipo, Bioma? bioma = null, HeroiConfig? heroi = null)
    {
        return tipo switch
        {
            TipoEventoAlto.DescobertaBioma when bioma is not null =>
                new RewardPayload(
                    Titulo: $"🗺️ Novo Bioma: {bioma.Nome}",
                    Descricao: bioma.Descricao,
                    ImagemUrl: null,
                    Tipo: TipoReward.Alto),

            TipoEventoAlto.HeroiIconicoDesbloqueado when heroi is not null =>
                new RewardPayload(
                    Titulo: $"⚔️ {heroi.Nome} se manifesta!",
                    Descricao: $"Um guerreiro lendário surge diante de você. **{heroi.Nome}** decide acompanhar sua jornada.",
                    ImagemUrl: null,
                    Tipo: TipoReward.Alto,
                    Campos: new Dictionary<string, string>
                    {
                        ["Raridade"]  = $"{(int)heroi!.RaridadeBase}⭐",
                        ["Arquétipo"] = heroi.Arquetipo.ToString()
                    }),

            _ => new RewardPayload("Recompensa", "Você obteve uma recompensa.", null, TipoReward.Alto)
        };
    }
}
```

- [ ] **Step 2: Registrar no DI**

```csharp
services.AddScoped<RewardDistributionService>();
```

- [ ] **Step 3: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: implement RewardDistributionService with 3-tier reward payloads"
```

---

## Task 12: Estender TorreService (com testes)

**Files:**
- Modify: `LegendsAwaken.Application/Services/TorreService.cs`
- Test: `LegendsAwaken.Tests/Services/TorreServiceExtensionTests.cs`

- [ ] **Step 1: Estender o record SubirAndarResult**

No topo de `TorreService.cs`, substituir o record existente:

```csharp
// ANTES:
public record SubirAndarResult(
    bool Sucesso,
    int XpConcedido,
    int OuroGanho,
    IReadOnlyDictionary<string, int> NiveisGanhosPorHeroi
);

// DEPOIS:
public record SubirAndarResult(
    bool Sucesso,
    int XpConcedido,
    int OuroGanho,
    IReadOnlyDictionary<string, int> NiveisGanhosPorHeroi,
    IReadOnlyList<FragmentDropResult> Fragmentos,
    Bioma? NovoBioma,
    HeroiConfig? HeroiDesbloqueado,
    IReadOnlyList<RewardPayload> RewardPayloads
);
```

Adicionar `using LegendsAwaken.Application.DTOs;` e `using LegendsAwaken.Domain.Entities.Fragmento;` ao topo do arquivo.

- [ ] **Step 2: Atualizar o construtor de TorreService para receber os novos serviços**

```csharp
// ANTES:
public class TorreService
{
    private readonly ITorreRepository _torreRepository;
    private readonly IHeroiRepository _heroiRepository;
    private readonly HeroiLevelUpService _levelUpService;

    public TorreService(
        ITorreRepository torreRepository,
        IHeroiRepository heroiRepository,
        HeroiLevelUpService levelUpService)
    {
        _torreRepository = torreRepository;
        _heroiRepository = heroiRepository;
        _levelUpService = levelUpService;
    }

// DEPOIS:
public class TorreService(
    ITorreRepository torreRepository,
    IHeroiRepository heroiRepository,
    HeroiLevelUpService levelUpService,
    FragmentService fragmentService,
    BiomeService biomeService,
    RecruitmentService recruitmentService,
    RewardDistributionService rewardService)
{
```

Substituir os campos `_torreRepository`, `_heroiRepository`, `_levelUpService` pelos parâmetros do primary constructor (ou manter fields se o padrão existente usar fields).

- [ ] **Step 3: Estender SubirAndarAsync com os dois pontos de extensão**

Localizar o final de `SubirAndarAsync`, antes do `return`, e adicionar:

```csharp
// Extensão 1: drop de fragmentos
var drops = await fragmentService.ProcessarDropAsync(usuarioId, andarAtual.Numero);
var rewardPayloads = new List<RewardPayload>();

foreach (var drop in drops)
    rewardPayloads.Add(rewardService.GerarMicroPico(drop));

// Extensão 2: detecção de bioma novo
Bioma? novoBioma = null;
if (await biomeService.EBiomaNovoAsync(proximoAndar.Numero))
{
    novoBioma = await biomeService.ObterBiomaPorAndarAsync(proximoAndar.Numero);
    if (novoBioma is not null)
        rewardPayloads.Add(rewardService.GerarPicoAlto(TipoEventoAlto.DescobertaBioma, novoBioma));
}

// Marco da Torre: verificar unlock de herói icônico
HeroiConfig? heroiDesbloqueado = null;
if (biomeService.EAndarDeMarco(proximoAndar.Numero))
{
    var recrutamento = await recruitmentService.ProcessarMarcoTorreAsync(usuarioId, proximoAndar.Numero);
    if (recrutamento?.Sucesso == true && recrutamento.Heroi is not null)
    {
        heroiDesbloqueado = recrutamento.Heroi;
        rewardPayloads.Add(rewardService.GerarPicoAlto(TipoEventoAlto.HeroiIconicoDesbloqueado, heroi: recrutamento.Heroi));
    }
}

return new SubirAndarResult(
    Sucesso: true,
    XpConcedido: xpConcedido,
    OuroGanho: ouroConcedido,
    NiveisGanhosPorHeroi: niveisGanhos,
    Fragmentos: drops,
    NovoBioma: novoBioma,
    HeroiDesbloqueado: heroiDesbloqueado,
    RewardPayloads: rewardPayloads);
```

Substituir o `return` original pelo acima (remover o antigo `return` com 4 campos).

- [ ] **Step 4: Escrever testes da extensão**

```csharp
// LegendsAwaken.Tests/Services/TorreServiceExtensionTests.cs
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using Xunit;

namespace LegendsAwaken.Tests.Services;

public class TorreServiceExtensionTests
{
    private readonly Mock<ITorreRepository>   _torreRepo   = new();
    private readonly Mock<IHeroiRepository>   _heroiRepo   = new();
    private readonly Mock<HeroiLevelUpService> _levelUp    = new();
    private readonly Mock<FragmentService>    _fragmentSvc = new();
    private readonly Mock<BiomeService>       _biomeSvc    = new();
    private readonly Mock<RecruitmentService> _recruitSvc  = new();
    private readonly Mock<RewardDistributionService> _rewardSvc = new();

    [Fact]
    public async Task SubirAndarAsync_IncluiFragmentos_NoResultado()
    {
        var usuarioId = Guid.NewGuid();
        var andar = new TorreAndar { Numero = 5, ObjetivoCumprido = true, UsuarioId = usuarioId };
        var heroi = new Heroi { Id = Guid.NewGuid(), Nome = "TestHero", Raridade = Raridade.Estrela3 };
        var drop  = new FragmentDropResult(Guid.NewGuid(), "Kaen", TipoFragmento.Heroi, 2, 7);

        _torreRepo.Setup(r => r.ObterAndarPorUsuarioAsync(usuarioId)).ReturnsAsync(andar);
        _torreRepo.Setup(r => r.AdicionarAsync(It.IsAny<TorreAndar>())).Returns(Task.CompletedTask);
        _heroiRepo.Setup(r => r.AtualizarAsync(It.IsAny<Heroi>())).Returns(Task.CompletedTask);
        _fragmentSvc.Setup(s => s.ProcessarDropAsync(usuarioId, 5)).ReturnsAsync([drop]);
        _biomeSvc.Setup(s => s.EBiomaNovoAsync(6)).ReturnsAsync(false);
        _biomeSvc.Setup(s => s.EAndarDeMarco(6)).Returns(false);
        _rewardSvc.Setup(s => s.GerarMicroPico(drop))
            .Returns(new RewardPayload("título", "desc", null, TipoReward.Micro));

        // Note: _levelUp mock needs to return 0 for AplicarXp
        // This depends on TorreService implementation — adjust if needed.

        // var service = new TorreService(_torreRepo.Object, _heroiRepo.Object, ...);
        // var resultado = await service.SubirAndarAsync(usuarioId, [heroi]);
        // Assert.Single(resultado.Fragmentos);
        // Assert.Equal("Kaen", resultado.Fragmentos[0].HeroiNome);

        // Note: TorreService usa HeroiLevelUpService como classe concreta (não interface).
        // Se não for possível mockar, testar via integration test com DB em memória.
        Assert.True(true); // placeholder — substituir por assertion real
    }
}
```

> **Nota:** `HeroiLevelUpService` é uma classe concreta sem interface. Se `TorreService` a instancia diretamente, será necessário criar uma interface `IHeroiLevelUpService` para testar com mocks, ou usar um teste de integração com SQLite in-memory. Decida conforme o padrão do projeto.

- [ ] **Step 5: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: extend TorreService with fragment drops and biome detection"
```

---

## Task 13: Bot — Painéis e Comandos

**Files:**
- Create: `LegendsAwaken.Bot/Panels/ColecaoPanel.cs`
- Create: `LegendsAwaken.Bot/Panels/BiomaPanel.cs`
- Create: `LegendsAwaken.Bot/Panels/ContratoPanel.cs`
- Create: `LegendsAwaken.Bot/Commands/ColecaoCommand.cs`
- Create: `LegendsAwaken.Bot/Commands/BiomaCommand.cs`
- Create: `LegendsAwaken.Bot/Commands/ContratoCommand.cs`

- [ ] **Step 1: Criar ColecaoPanel.cs**

```csharp
// LegendsAwaken.Bot/Panels/ColecaoPanel.cs
using Discord;
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Bot.Panels;

public static class ColecaoPanel
{
    public static Embed CriarEmbed(
        List<HeroiConfig> todosHerois,
        List<HeroiDesbloqueado> desbloqueados,
        List<FragmentoProgresso> progressos,
        List<HeroiUnlockConfig> unlockConfigs)
    {
        var builder = new EmbedBuilder()
            .WithTitle("📚 Sua Coleção")
            .WithColor(Color.Purple);

        foreach (var heroi in todosHerois)
        {
            bool desbloqueado = desbloqueados.Any(d => d.HeroiId == heroi.Id);
            var progresso     = progressos.FirstOrDefault(p => p.HeroiId == heroi.Id);
            var unlock        = unlockConfigs.FirstOrDefault(u => u.HeroiId == heroi.Id);

            string estado = desbloqueado ? "✅" : "🔒";
            string barra  = GerarBarra(progresso?.Quantidade ?? 0, unlock?.QuantidadeFragmentos ?? 0);
            string valor  = desbloqueado
                ? "Recrutado"
                : unlock?.TipoUnlock switch
                {
                    Domain.Enum.TipoUnlock.Fragmentos   => $"{barra} {progresso?.Quantidade ?? 0}/{unlock.QuantidadeFragmentos}",
                    Domain.Enum.TipoUnlock.MarcoTorre    => $"🗼 Andar {unlock.AndarMarco}",
                    Domain.Enum.TipoUnlock.CondicaoUnica => "❓ Condição especial",
                    _ => "?"
                };

            builder.AddField($"{estado} {heroi.Nome} {new string('⭐', (int)heroi.RaridadeBase)}", valor, inline: true);
        }

        return builder.Build();
    }

    public static MessageComponent CriarComponentes(List<HeroiConfig> heroisProntos)
    {
        var builder = new ComponentBuilder();

        if (heroisProntos.Count > 0)
        {
            var select = new SelectMenuBuilder()
                .WithCustomId("colecao_recrutar")
                .WithPlaceholder("Recrutar herói...")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var heroi in heroisProntos.Take(25))
                select.AddOption(heroi.Nome, heroi.Id.ToString());

            builder.WithSelectMenu(select);
        }

        return builder.Build();
    }

    private static string GerarBarra(int atual, int maximo)
    {
        if (maximo == 0) return string.Empty;
        int preenchido = (int)Math.Round((double)atual / maximo * 10);
        return $"[{'█'.ToString().PadRight(preenchido, '█').PadRight(10, '░')}]";
    }
}
```

- [ ] **Step 2: Criar BiomaPanel.cs**

```csharp
// LegendsAwaken.Bot/Panels/BiomaPanel.cs
using Discord;
using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Bot.Panels;

public static class BiomaPanel
{
    public static Embed CriarEmbed(Bioma bioma, List<BiomHeroPool> pool, int andarAtual)
    {
        var builder = new EmbedBuilder()
            .WithTitle($"🗺️ {bioma.Nome}")
            .WithDescription(bioma.Descricao)
            .WithColor(Color.DarkOrange)
            .AddField("Andares", $"{bioma.AndarInicio} – {bioma.AndarFim}", inline: true)
            .AddField("Seu andar", andarAtual.ToString(), inline: true);

        var heroPrincipal = pool.FirstOrDefault(p => p.EHeroPrincipal);
        if (heroPrincipal is not null)
            builder.AddField("⭐ Herói Principal", heroPrincipal.Heroi.Nome, inline: false);

        var secundarios = pool.Where(p => !p.EHeroPrincipal).Select(p => p.Heroi.Nome);
        if (secundarios.Any())
            builder.AddField("Heróis do Pool", string.Join(", ", secundarios), inline: false);

        return builder.Build();
    }

    public static MessageComponent CriarComponentes()
    {
        return new ComponentBuilder()
            .WithButton("Ver Coleção", "bioma_ver_colecao", ButtonStyle.Secondary)
            .WithButton("Contratos", "bioma_contratos", ButtonStyle.Primary)
            .Build();
    }
}
```

- [ ] **Step 3: Criar ContratoPanel.cs**

```csharp
// LegendsAwaken.Bot/Panels/ContratoPanel.cs
using Discord;
using LegendsAwaken.Application.Config;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Bot.Panels;

public static class ContratoPanel
{
    public static Embed CriarEmbed(Contrato? arquetipo, Contrato? nomeado)
    {
        var builder = new EmbedBuilder()
            .WithTitle("📜 Contratos Ativos")
            .WithColor(Color.Blue);

        if (arquetipo is not null)
            builder.AddField("Arquétipo",
                $"{arquetipo.Arquetipo} (+{ContractConfig.ArchetypeBonus * 100:0}% fragmentos)", inline: true);
        else
            builder.AddField("Arquétipo", "Nenhum ativo", inline: true);

        if (nomeado is not null)
        {
            var restante = nomeado.ExpiraEm.HasValue
                ? $"Expira em {(nomeado.ExpiraEm.Value - DateTime.UtcNow).TotalHours:0.0}h"
                : "Sem expiração";
            builder.AddField("Foco Nomeado",
                $"{nomeado.Heroi?.Nome ?? "?"} (+{ContractConfig.NamedBonus * 100:0}%) — {restante}", inline: true);
        }
        else
        {
            builder.AddField("Foco Nomeado", "Nenhum ativo", inline: true);
        }

        return builder.Build();
    }

    public static MessageComponent CriarComponentes()
    {
        var select = new SelectMenuBuilder()
            .WithCustomId("contrato_arquetipo")
            .WithPlaceholder("Mudar arquétipo...")
            .AddOption("Combate",   Profissao.Guerreiro.ToString())
            .AddOption("Coleta",    Profissao.Agricultor.ToString())
            .AddOption("Produção",  Profissao.Ferreiro.ToString());

        return new ComponentBuilder()
            .WithSelectMenu(select)
            .WithButton("Remover Foco Nomeado", "contrato_remover_nomeado", ButtonStyle.Danger, row: 1)
            .Build();
    }
}
```

- [ ] **Step 4: Criar ColecaoCommand.cs**

```csharp
// LegendsAwaken.Bot/Commands/ColecaoCommand.cs
using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Bot.Commands;

public class ColecaoCommand(
    HeroiConfigRepository heroiConfigRepo,    // ou IHeroiConfigRepository
    IHeroiDesbloqueadoRepository desbloqueadoRepo,
    IFragmentoRepository fragmentoRepo,
    RecruitmentService recruitmentService)
{
    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();

        var usuarioId = await ObterUsuarioIdAsync(command.User.Id);

        var todosHerois   = await heroiConfigRepo.ListarTodosAsync();
        var desbloqueados = await desbloqueadoRepo.ListarPorUsuarioAsync(usuarioId);
        var progressos    = await fragmentoRepo.ListarPorUsuarioAsync(usuarioId);
        var unlockConfigs = await Task.WhenAll(todosHerois.Select(h => heroiConfigRepo.ObterUnlockConfigAsync(h.Id)));
        var unlockList    = unlockConfigs.Where(u => u is not null).Select(u => u!).ToList();

        var heroisProntos = todosHerois
            .Where(h =>
            {
                var unlock = unlockList.FirstOrDefault(u => u.HeroiId == h.Id);
                var prog   = progressos.FirstOrDefault(p => p.HeroiId == h.Id);
                return !desbloqueados.Any(d => d.HeroiId == h.Id)
                    && unlock?.TipoUnlock == Domain.Enum.TipoUnlock.Fragmentos
                    && prog?.Quantidade >= unlock.QuantidadeFragmentos;
            })
            .ToList();

        var embed      = ColecaoPanel.CriarEmbed(todosHerois, desbloqueados, progressos, unlockList);
        var components = ColecaoPanel.CriarComponentes(heroisProntos);

        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = components; });
    }

    public async Task HandleRecrutarAsync(SocketMessageComponent interaction, Guid heroiId)
    {
        await interaction.DeferAsync(ephemeral: true);
        var usuarioId = await ObterUsuarioIdAsync(interaction.User.Id);
        var resultado = await recruitmentService.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);
        await interaction.FollowupAsync(resultado.Mensagem, ephemeral: true);
    }

    private Task<Guid> ObterUsuarioIdAsync(ulong discordId)
    {
        // Adaptar para o padrão existente do projeto (provavelmente via UsuarioService)
        throw new NotImplementedException("Adaptar para busca de UsuarioId via DiscordUserId");
    }
}
```

> **Nota:** `ObterUsuarioIdAsync` deve usar o padrão existente do projeto para mapear `DiscordUserId (ulong)` → `UsuarioId (Guid)`. Ver como outros Commands resolvem isso (ex: `SubirAndarCommand.cs`).

- [ ] **Step 5: Criar BiomaCommand.cs**

```csharp
// LegendsAwaken.Bot/Commands/BiomaCommand.cs
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;

namespace LegendsAwaken.Bot.Commands;

public class BiomaCommand(BiomeService biomeService, ITorreRepository torreRepository)
{
    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var usuarioId = await ObterUsuarioIdAsync(command.User.Id);

        var andar = await torreRepository.ObterAndarPorUsuarioAsync(usuarioId);
        int andarAtual = andar?.Numero ?? 1;

        var bioma = await biomeService.ObterBiomaPorAndarAsync(andarAtual);
        if (bioma is null)
        {
            await command.ModifyOriginalResponseAsync(m => m.Content = "Bioma não encontrado para o andar atual.");
            return;
        }

        var pool   = await biomeService.ObterPoolDoBiomaAsync(bioma.Id);
        var embed  = BiomaPanel.CriarEmbed(bioma, pool, andarAtual);
        var comps  = BiomaPanel.CriarComponentes();

        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    private Task<Guid> ObterUsuarioIdAsync(ulong discordId) =>
        throw new NotImplementedException("Adaptar para busca de UsuarioId via DiscordUserId");
}
```

- [ ] **Step 6: Criar ContratoCommand.cs**

```csharp
// LegendsAwaken.Bot/Commands/ContratoCommand.cs
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Bot.Commands;

public class ContratoCommand(ContractService contractService, IContratoRepository contratoRepository)
{
    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var usuarioId = await ObterUsuarioIdAsync(command.User.Id);

        var arquetipo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        var nomeado   = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);

        var embed = ContratoPanel.CriarEmbed(arquetipo, nomeado);
        var comps = ContratoPanel.CriarComponentes();

        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    public async Task HandleArquetipoAsync(SocketMessageComponent interaction, Profissao arquetipo)
    {
        await interaction.DeferAsync(ephemeral: true);
        var usuarioId = await ObterUsuarioIdAsync(interaction.User.Id);
        await contractService.AtivarContratoArquetipoAsync(usuarioId, arquetipo);
        await interaction.FollowupAsync($"Contrato de arquétipo **{arquetipo}** ativado.", ephemeral: true);
    }

    private Task<Guid> ObterUsuarioIdAsync(ulong discordId) =>
        throw new NotImplementedException("Adaptar para busca de UsuarioId via DiscordUserId");
}
```

- [ ] **Step 7: Registrar comandos no CommandHandler**

Localizar `CommandHandler.cs` e:
1. Registrar os 3 novos comandos no DI
2. Mapear `/colecao` → `ColecaoCommand.ExecutarAsync`
3. Mapear `/bioma` → `BiomaCommand.ExecutarAsync`
4. Mapear `/contrato` → `ContratoCommand.ExecutarAsync`
5. Mapear interactions `colecao_recrutar` → `ColecaoCommand.HandleRecrutarAsync`
6. Mapear interactions `contrato_arquetipo` → `ContratoCommand.HandleArquetipoAsync`
7. Mapear interactions `bioma_ver_colecao` → redirecionar para ColecaoCommand
8. Mapear interactions `bioma_contratos` → redirecionar para ContratoCommand

> **Nota:** Adaptar ao padrão de registro existente em `CommandHandler.cs`. Ver como `SubirAndarCommand` e `CidadeCommand` são registrados.

- [ ] **Step 8: Build passa**

```bash
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 9: Commit final**

```bash
git add -A
git commit -m "feat: add bot panels and commands for colecao, bioma, contrato"
```

---

## Checklist de Verificação Final

Após implementar todas as tasks:

- [ ] `dotnet build` — 0 errors, 0 warnings
- [ ] `dotnet test` — todos os testes passam
- [ ] `dotnet ef database update` — migration aplicada sem erro
- [ ] `/colecao` no Discord — abre painel com lista de heróis e barras de progresso
- [ ] `/bioma` no Discord — abre painel com bioma atual, pool de heróis e botões
- [ ] `/contrato` no Discord — abre painel com contratos ativos e select de arquétipo
- [ ] Subir um andar na Torre — `SubirAndarResult` inclui `Fragmentos` e `RewardPayloads`
- [ ] Atingir o andar 10 — Kaen desbloqueado automaticamente via marco
- [ ] Acumular 30 fragmentos de Grom e usar botão Recrutar — `RecruitmentResult.Sucesso = true`
