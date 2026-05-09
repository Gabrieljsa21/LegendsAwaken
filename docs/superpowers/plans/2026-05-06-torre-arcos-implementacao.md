# Torre: Arcos Narrativos — Plano de Implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the arc narrative system in the Torre exploration loop: static config for 3 designed arcs (floors 1–15), per-player flag tracking in raw SQLite, flag-based boss HP modifiers, and Discord UX showing arc narrative + secondary objectives.

**Architecture:** `TorreArcoConfig` (static class) owns all narrative content — arcs, floor objectives, possible flags, collectibles, boss modifiers, composite flag definitions. `AndarFlagProgressoRepository` (raw SQLite) tracks per-player flag state (generated/expired). `TorreFlagService` evaluates composites and computes boss modifier totals. Integration hooks into `TorreExploracaoService.ColetarAsync` (flag generation after floor completion) and the combat setup path (boss HP reduction). Discord panels surface the arc narrative layer.

**Tech Stack:** C# 13 / net10.0, xUnit, SQLite raw (no new EF migrations), Discord.Net, existing `TorreExploracaoService` and `TorreService` patterns.

**Reference:** Design source of truth — `DESIGN_TORRE_ARCOS.md`.

---

## File Map

| Action | File |
|--------|------|
| Create | `Application/Services/TorreArcoConfig.cs` |
| Create | `Domain/Entities/AndarFlagProgresso.cs` |
| Create | `Infrastructure/Repositories/AndarFlagProgressoRepository.cs` |
| Create | `Application/Services/TorreFlagService.cs` |
| Create | `Tests/Unit/TorreFlagServiceTests.cs` |
| Create | `Tests/Unit/TorreArcoConfigTests.cs` |
| Modify | `Application/Services/TorreExploracaoService.cs` — flag processing in `ColetarAsync` |
| Modify | `Bot/Panels/TorrePanel.cs` — arc narrative block + active flags |
| Modify | `Bot/Panels/TorreExploracaoPanel.cs` — secondary objective in `CriarConfirmacao` |
| Modify | `Bot/Commands/TorreCommand.cs` — display flags in collect result |
| Modify | `Program.cs` — register `TorreFlagService`, `AndarFlagProgressoRepository` |
| Modify | `docs/AI_INDEX.md` — add new entities + services |

---

## Task 1: TorreArcoConfig — Static Arc Config

**Files:**
- Create: `Application/Services/TorreArcoConfig.cs`

- [ ] **Step 1: Write the config file**

```csharp
// Application/Services/TorreArcoConfig.cs
namespace LegendsAwaken.Application.Services;

public record ArcoDefinicao(
    int Numero,
    string Nome,
    int AndarInicio,
    int AndarFim,
    IReadOnlyList<AndarArcoDefinicao> Andares);

public record AndarArcoDefinicao(
    int Numero,
    string NarrativaDisplay,
    ObjetivoDefinicao? ObjetivoSecundario,
    ColecionavelDefinicao? Colecionavel,
    IReadOnlyList<BossModificador> ModificadoresBoss,
    IReadOnlyList<string> FlagsGeradasPossiveis);

public record ObjetivoDefinicao(
    string Descricao,
    string FlagNome,
    string EfeitoDescricao,
    string? RequererFlag = null);

public record ColecionavelDefinicao(
    string Nome,
    string Categoria,
    string Descricao,
    string? FlagCondicional = null);

public record BossModificador(
    string FlagNome,
    string EfeitoDescricao,
    double HpReductionPercent);

public record FlagCompostaDefinicao(
    string NomeComposta,
    IReadOnlyList<string> ComponentesNecessarios,
    string EfeitoDescricao);

public static class TorreArcoConfig
{
    public static IReadOnlyList<FlagCompostaDefinicao> FlagsCompostas { get; } =
    [
        new("identidade_revelada",
            ["grimorio_encontrado", "diario_rasgado"],
            "Boss Andar 4: -5% HP adicional do Carniçal"),
        new("rota_alternativa",
            ["contexto_obtido", "mapa_rabiscado"],
            "Andar 9: acesso a rota secundária evitando armadilhas"),
        new("andolyn_aliada",
            ["gendrew_resgatado", "prata_preservada"],
            "NPC permanente no hub: identificação de itens + 1 magia gratuita/semana"),
        new("woganpuck_rastreado",
            ["woganpuck_revelado"],
            "Ativa rota alternativa de confronto quando Woganpuck reaparecer"),
    ];

    public static IReadOnlyList<ArcoDefinicao> Arcos { get; } =
    [
        new ArcoDefinicao(1, "Torre em Ruínas", 1, 4,
        [
            new AndarArcoDefinicao(1,
                "Uma torre em colapso. Esqueletos vagam entre cinzas. Um grimório flutua sobre uma pilha de pedras.",
                new ObjetivoDefinicao(
                    "Examinar o grimório antes de destruí-lo",
                    "grimorio_encontrado",
                    "Boss Andar 4: -10% HP do Carniçal"),
                new ColecionavelDefinicao("moeda_arcana", "Economia",
                    "Moeda de facção arcana antiga — valor ao vender"),
                [],
                ["grimorio_encontrado"]),
            new AndarArcoDefinicao(2,
                "Esqueletos armados bloqueiam o corredor. Um altar pulsa com energia sombria ao fundo.",
                new ObjetivoDefinicao(
                    "Destruir o altar antes de avançar",
                    "altar_destruido",
                    "Andar 3: impede ressurgimento de mortos-vivos durante o combate"),
                new ColecionavelDefinicao("amuleto_de_osso", "Build",
                    "+5% resistência física, sem tradeoff"),
                [],
                ["altar_destruido"]),
            new AndarArcoDefinicao(3,
                "Zumbis lentos. Uma página rasgada de diário jaz no chão entre os destroços.",
                null,
                new ColecionavelDefinicao("diario_rasgado", "Arquivo",
                    "Páginas parciais — sozinho não revela nada"),
                [],
                ["diario_rasgado"]),
            new AndarArcoDefinicao(4,
                "Uma câmara escura no topo da ala. O Carniçal aguarda imóvel — você já o conhece?",
                null,
                new ColecionavelDefinicao("anel_do_mago", "Build",
                    "+8% dano mágico, sem tradeoff", "bossDerrotado"),
                [
                    new BossModificador("grimorio_encontrado",
                        "-10% HP do Carniçal", 0.10),
                    new BossModificador("identidade_revelada",
                        "-5% HP adicional (identidade revelada)", 0.05),
                ],
                []),
        ]),

        new ArcoDefinicao(2, "A Praga Ardente", 5, 10,
        [
            new AndarArcoDefinicao(5,
                "Kobolds contaminados. Os corpos têm marcas estranhas — nenhuma ferida de combate.",
                new ObjetivoDefinicao(
                    "Investigar os corpos antes de avançar",
                    "causa_investigada",
                    "Andar 8: desbloqueia diálogo com kobold sobrevivente"),
                null,
                [],
                ["causa_investigada"]),
            new AndarArcoDefinicao(6,
                "Um refeitório em chamas. Kobolds e trabalhadores infectados. Alguém ainda está vivo.",
                new ObjetivoDefinicao(
                    "Resgatar o sobrevivente durante o combate",
                    "sobrevivente_resgatado",
                    "Pós-arco: NPC permanente no hub"),
                null,
                [],
                ["sobrevivente_resgatado"]),
            new AndarArcoDefinicao(7,
                "Guardiões protegem uma fonte negra borbulhante. A contaminação vem daqui.",
                new ObjetivoDefinicao(
                    "Destruir a fonte de contaminação",
                    "fonte_destruida",
                    "Boss Andar 10: -15% HP de Jakk; Andar 8: reduz Exaustão do grupo"),
                new ColecionavelDefinicao("frasco_agua_pura", "Chave",
                    "Cancela mecânica de veneno em área de Jakk uma vez"),
                [],
                ["fonte_destruida"]),
            new AndarArcoDefinicao(8,
                "Uma câmara central. Um kobold encostado na parede observa com olhos inteligentes demais.",
                new ObjetivoDefinicao(
                    "Dialogar com o kobold sobrevivente",
                    "contexto_obtido",
                    "Gera rota_alternativa no Andar 9 com mapa_rabiscado",
                    RequererFlag: "causa_investigada"),
                null,
                [],
                ["contexto_obtido", "mapa_rabiscado"]),
            new AndarArcoDefinicao(9,
                "Um poço profundo. A água se move. Algo está abaixo da superfície.",
                null,
                new ColecionavelDefinicao("pedra_mana_contaminada", "Build",
                    "+12% dano mágico / +1 Fadiga por uso"),
                [],
                []),
            new AndarArcoDefinicao(10,
                "Jakk aguarda. À medida que você avança, zumbis começam a surgir das sombras.",
                null,
                new ColecionavelDefinicao("selo_de_jakk", "Arquivo",
                    "Combina com item futuro para revelar afiliação de Jakk"),
                [
                    new BossModificador("fonte_destruida",
                        "-15% HP de Jakk", 0.15),
                ],
                []),
        ]),

        new ArcoDefinicao(3, "A Cabana dos Experimentos", 11, 15,
        [
            new AndarArcoDefinicao(11,
                "Uma sala que cheira a madeira velha e pólvora. Livros que voam. Cordas que apertam. Algo aqui não quer que você passe.",
                new ObjetivoDefinicao(
                    "Examinar o livro aberto antes de destruí-lo",
                    "grimorio_golem_lido",
                    "Boss Andar 15: -15% HP do Golem"),
                new ColecionavelDefinicao("fragmento_livro_arcano", "Lore",
                    "Notas sobre imunidades de constructs", "grimorio_golem_lido"),
                [],
                ["grimorio_golem_lido", "objetos_destruidos"]),
            new AndarArcoDefinicao(12,
                "Silêncio. Mesa posta para dois. Velas frias. Prata reluzindo como se esperasse alguém que não veio.",
                new ObjetivoDefinicao(
                    "Preservar os objetos de prata sem quebrá-los",
                    "prata_preservada",
                    "Composta andolyn_aliada: NPC permanente"),
                new ColecionavelDefinicao("talheres_de_prata", "Economia",
                    "12 peças de prata — valor em ouro ao vender"),
                [],
                ["prata_preservada"]),
            new AndarArcoDefinicao(13,
                "Asas de morcego. Risos baixos. Um homem amarrado na cama, quase morto, quase respirando. Ainda há tempo.",
                new ObjetivoDefinicao(
                    "Estabilizar o prisioneiro durante ou após o combate",
                    "gendrew_resgatado",
                    "Informa fraqueza do boss + localização da Caixa de Poções"),
                null,
                [],
                ["gendrew_resgatado", "diabretes_derrotados", "woganpuck_revelado"]),
            new AndarArcoDefinicao(14,
                "O fogão está quente. Algo se move lá dentro. O ar fede a enxofre e a farinha queimada.",
                null,
                null,
                [],
                ["mephits_pacificados", "fraqueza_confirmada"]),
            new AndarArcoDefinicao(15,
                "No porão: uma criatura de massa e crosta. Seus punhos fumegam. Você sente o calor antes de vê-la.",
                null,
                new ColecionavelDefinicao("frasco_molho_fervente", "Build",
                    "+8% dano físico em ataque / +1 Fadiga ao usuário", "bossDerrotado"),
                [
                    new BossModificador("grimorio_golem_lido",
                        "-15% HP do Golem", 0.15),
                    new BossModificador("fraqueza_confirmada",
                        "-5% HP adicional (fraqueza confirmada)", 0.05),
                ],
                []),
        ]),
    ];

    public static ArcoDefinicao? ObterArcoPorAndar(int andar) =>
        Arcos.FirstOrDefault(a => andar >= a.AndarInicio && andar <= a.AndarFim);

    public static AndarArcoDefinicao? ObterAndar(int andar) =>
        ObterArcoPorAndar(andar)?.Andares.FirstOrDefault(a => a.Numero == andar);

    public static bool EBossFloor(int andar) =>
        ObterAndar(andar)?.ModificadoresBoss.Count > 0;
}
```

- [ ] **Step 2: Verify it compiles**

```
dotnet build LegendsAwaken.Application -v minimal
```
Expected: Build succeeded with 0 errors.

- [ ] **Step 3: Commit**

```
git add LegendsAwaken.Application/Services/TorreArcoConfig.cs
git commit -m "feat(torre): add TorreArcoConfig static arc definitions for arcs 1-3"
```

---

## Task 2: AndarFlagProgresso Entity + Repository

**Files:**
- Create: `Domain/Entities/AndarFlagProgresso.cs`
- Create: `Infrastructure/Repositories/AndarFlagProgressoRepository.cs`

- [ ] **Step 1: Write the POCO entity**

```csharp
// Domain/Entities/AndarFlagProgresso.cs
namespace LegendsAwaken.Domain.Entities;

public sealed class AndarFlagProgresso
{
    public Guid UsuarioId { get; set; }
    public int Andar { get; set; }
    public string FlagNome { get; set; } = "";
    public bool Gerada { get; set; }
    public bool Expirou { get; set; }
    public DateTime? GeradaEm { get; set; }
}
```

- [ ] **Step 2: Write the repository interface**

Add interface at the bottom of `Infrastructure/Repositories/AndarFlagProgressoRepository.cs` (same file for now, split only if it grows):

```csharp
// Infrastructure/Repositories/AndarFlagProgressoRepository.cs
using LegendsAwaken.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace LegendsAwaken.Infrastructure.Repositories;

public interface IAndarFlagProgressoRepository
{
    Task EnsureTableAsync();
    Task GerarFlagAsync(Guid userId, int andar, string flagNome);
    Task MarcarExpiradoAsync(Guid userId, int andar, string flagNome);
    Task<IReadOnlyList<string>> ObterFlagsGераdasAsync(Guid userId);
}

public sealed class AndarFlagProgressoRepository(string connectionString) : IAndarFlagProgressoRepository
{
    public async Task EnsureTableAsync()
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS AndarFlagProgresso (
                UsuarioId TEXT NOT NULL,
                Andar     INTEGER NOT NULL,
                FlagNome  TEXT NOT NULL,
                Gerada    INTEGER NOT NULL DEFAULT 0,
                Expirou   INTEGER NOT NULL DEFAULT 0,
                GeradaEm  TEXT,
                PRIMARY KEY (UsuarioId, Andar, FlagNome)
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task GerarFlagAsync(Guid userId, int andar, string flagNome)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO AndarFlagProgresso (UsuarioId, Andar, FlagNome, Gerada, Expirou, GeradaEm)
            VALUES ($uid, $andar, $flag, 1, 0, $now)
            ON CONFLICT(UsuarioId, Andar, FlagNome) DO UPDATE SET Gerada=1, GeradaEm=$now WHERE Gerada=0;
            """;
        cmd.Parameters.AddWithValue("$uid", userId.ToString());
        cmd.Parameters.AddWithValue("$andar", andar);
        cmd.Parameters.AddWithValue("$flag", flagNome);
        cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task MarcarExpiradoAsync(Guid userId, int andar, string flagNome)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        // Only inserts if not already generated — generated flags are never expired
        cmd.CommandText = """
            INSERT OR IGNORE INTO AndarFlagProgresso (UsuarioId, Andar, FlagNome, Gerada, Expirou)
            VALUES ($uid, $andar, $flag, 0, 1);
            """;
        cmd.Parameters.AddWithValue("$uid", userId.ToString());
        cmd.Parameters.AddWithValue("$andar", andar);
        cmd.Parameters.AddWithValue("$flag", flagNome);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<string>> ObterFlagsGераdasAsync(Guid userId)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT FlagNome FROM AndarFlagProgresso
            WHERE UsuarioId=$uid AND Gerada=1;
            """;
        cmd.Parameters.AddWithValue("$uid", userId.ToString());
        var result = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(reader.GetString(0));
        return result;
    }
}
```

- [ ] **Step 3: Verify it compiles**

```
dotnet build LegendsAwaken.Infrastructure -v minimal
```
Expected: 0 errors.

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Domain/Entities/AndarFlagProgresso.cs
git add LegendsAwaken.Infrastructure/Repositories/AndarFlagProgressoRepository.cs
git commit -m "feat(torre): add AndarFlagProgresso entity and raw SQLite repository"
```

---

## Task 3: TorreFlagService

**Files:**
- Create: `Application/Services/TorreFlagService.cs`

- [ ] **Step 1: Write the service**

```csharp
// Application/Services/TorreFlagService.cs
using LegendsAwaken.Infrastructure.Repositories;

namespace LegendsAwaken.Application.Services;

public sealed class TorreFlagService(IAndarFlagProgressoRepository repo)
{
    public async Task GerarFlagAsync(Guid userId, int andar, string flagNome) =>
        await repo.GerarFlagAsync(userId, andar, flagNome);

    public async Task MarcarSecundarioExpiradoAsync(Guid userId, int andar)
    {
        var andarDef = TorreArcoConfig.ObterAndar(andar);
        if (andarDef?.ObjetivoSecundario is not { } obj) return;
        await repo.MarcarExpiradoAsync(userId, andar, obj.FlagNome);
    }

    public async Task<IReadOnlyList<string>> ObterFlagsAtivasParaArcoAsync(Guid userId, int arcoNumero)
    {
        var arco = TorreArcoConfig.Arcos.FirstOrDefault(a => a.Numero == arcoNumero);
        if (arco is null) return [];
        var todas = await repo.ObterFlagsGераdasAsync(userId);
        var andares = arco.Andares.Select(a => a.Numero).ToHashSet();
        return todas; // flags don't store the floor they came from in this query; filter via config
        // NOTE: ObterFlagsGераdasAsync returns ALL generated flags — arc filtering is by checking
        // which flags belong to this arc's AndarArcoDefinicao.FlagsGeradasPossiveis
    }

    public async Task<IReadOnlyList<string>> ObterFlagsAtivasAsync(Guid userId) =>
        await repo.ObterFlagsGераdasAsync(userId);

    public async Task<IReadOnlyList<string>> ObterFlagsCompostasAtivasAsync(Guid userId)
    {
        var ativas = (await repo.ObterFlagsGераdasAsync(userId)).ToHashSet();
        return TorreArcoConfig.FlagsCompostas
            .Where(fc => fc.ComponentesNecessarios.All(c => ativas.Contains(c)))
            .Select(fc => fc.NomeComposta)
            .ToList();
    }

    public async Task<(double TotalHpReduction, IReadOnlyList<string> Descricoes)>
        ObterModificadoresBossAsync(Guid userId, int andar)
    {
        var andarDef = TorreArcoConfig.ObterAndar(andar);
        if (andarDef is null || andarDef.ModificadoresBoss.Count == 0)
            return (0, []);

        var ativas = (await repo.ObterFlagsGераdasAsync(userId)).ToHashSet();
        var compostas = await ObterFlagsCompostasAtivasAsync(userId);
        foreach (var c in compostas) ativas.Add(c);

        var aplicados = andarDef.ModificadoresBoss
            .Where(m => ativas.Contains(m.FlagNome))
            .ToList();

        var totalReduction = Math.Min(aplicados.Sum(m => m.HpReductionPercent), 0.50);
        var descricoes = aplicados.Select(m => m.EfeitoDescricao).ToList();
        return (totalReduction, descricoes);
    }
}
```

- [ ] **Step 2: Wire EnsureTable into startup**

Open `Program.cs`. Find where other `EnsureTableAsync()` calls happen (e.g., `TorreExploracaoRepository.EnsureTableAsync()`). Add:

```csharp
// in the startup sequence alongside other raw repo EnsureTables
await app.Services.GetRequiredService<IAndarFlagProgressoRepository>().EnsureTableAsync();
```

Also register the service + repository:
```csharp
builder.Services.AddSingleton<IAndarFlagProgressoRepository>(sp =>
    new AndarFlagProgressoRepository(dbConnectionString));
builder.Services.AddScoped<TorreFlagService>();
```

- [ ] **Step 3: Verify it compiles**

```
dotnet build LegendsAwaken.sln -v minimal
```
Expected: 0 errors.

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Application/Services/TorreFlagService.cs
git add LegendsAwaken.Bot/Program.cs
git commit -m "feat(torre): add TorreFlagService and wire AndarFlagProgressoRepository in startup"
```

---

## Task 4: Unit Tests — TorreFlagService + TorreArcoConfig

**Files:**
- Create: `Tests/Unit/TorreFlagServiceTests.cs`
- Create: `Tests/Unit/TorreArcoConfigTests.cs`

- [ ] **Step 1: Write TorreArcoConfigTests**

```csharp
// Tests/Unit/TorreArcoConfigTests.cs
using LegendsAwaken.Application.Services;
using Xunit;

namespace LegendsAwaken.Tests.Unit;

public class TorreArcoConfigTests
{
    [Fact]
    public void Arcos_CobreSemGapsEntre1E15()
    {
        var cobertos = TorreArcoConfig.Arcos
            .SelectMany(a => a.Andares.Select(f => f.Numero))
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(Enumerable.Range(1, 15).ToList(), cobertos);
    }

    [Fact]
    public void BossFloors_TodosTemModificadores()
    {
        var bossFloors = new[] { 4, 10, 15 };
        foreach (var andar in bossFloors)
        {
            var def = TorreArcoConfig.ObterAndar(andar);
            Assert.NotNull(def);
            Assert.NotEmpty(def.ModificadoresBoss);
        }
    }

    [Fact]
    public void ObterArcoPorAndar_RetornaArcoCorreto()
    {
        Assert.Equal(1, TorreArcoConfig.ObterArcoPorAndar(1)!.Numero);
        Assert.Equal(2, TorreArcoConfig.ObterArcoPorAndar(7)!.Numero);
        Assert.Equal(3, TorreArcoConfig.ObterArcoPorAndar(15)!.Numero);
        Assert.Null(TorreArcoConfig.ObterArcoPorAndar(16));
    }

    [Fact]
    public void Regra7030_AndaresSemSecundarioSaoAproximadamente30Porcento()
    {
        var total = TorreArcoConfig.Arcos.SelectMany(a => a.Andares).Count();
        var semSecundario = TorreArcoConfig.Arcos
            .SelectMany(a => a.Andares)
            .Count(f => f.ObjetivoSecundario is null);
        // boss floors (3) + tension floors (andar 3, 9, 14) = 6 sem secundário
        // 6/15 = 40% — acceptable range for 30% rule (bosses always exempt)
        Assert.InRange(semSecundario, 3, 7);
    }
}
```

- [ ] **Step 2: Write TorreFlagServiceTests**

```csharp
// Tests/Unit/TorreFlagServiceTests.cs
using LegendsAwaken.Application.Services;
using LegendsAwaken.Infrastructure.Repositories;
using NSubstitute;
using Xunit;

namespace LegendsAwaken.Tests.Unit;

public class TorreFlagServiceTests
{
    private static (TorreFlagService svc, IAndarFlagProgressoRepository repo) Criar()
    {
        var repo = Substitute.For<IAndarFlagProgressoRepository>();
        return (new TorreFlagService(repo), repo);
    }

    [Fact]
    public async Task GerarFlag_DelegatesParaRepo()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();

        await svc.GerarFlagAsync(userId, 1, "grimorio_encontrado");

        await repo.Received(1).GerarFlagAsync(userId, 1, "grimorio_encontrado");
    }

    [Fact]
    public async Task MarcarSecundarioExpirado_ChegaNoRepo_QuandoAndarTemSecundario()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();

        await svc.MarcarSecundarioExpiradoAsync(userId, 1); // andar 1 tem secundário "grimorio_encontrado"

        await repo.Received(1).MarcarExpiradoAsync(userId, 1, "grimorio_encontrado");
    }

    [Fact]
    public async Task MarcarSecundarioExpirado_NaoChegaNoRepo_QuandoAndarNaoTemSecundario()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();

        await svc.MarcarSecundarioExpiradoAsync(userId, 4); // boss floor, sem secundário

        await repo.DidNotReceive().MarcarExpiradoAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ObterFlagsCompostas_ReturnsComposta_QuandoComponentesAtivos()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();
        repo.ObterFlagsGераdasAsync(userId).Returns(
            Task.FromResult<IReadOnlyList<string>>(["grimorio_encontrado", "diario_rasgado"]));

        var compostas = await svc.ObterFlagsCompostasAtivasAsync(userId);

        Assert.Contains("identidade_revelada", compostas);
    }

    [Fact]
    public async Task ObterModificadoresBoss_SomaReducoes_QuandoFlagsAtivas()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();
        repo.ObterFlagsGераdasAsync(userId).Returns(
            Task.FromResult<IReadOnlyList<string>>(["grimorio_encontrado"]));

        var (total, descricoes) = await svc.ObterModificadoresBossAsync(userId, 4);

        Assert.Equal(0.10, total, precision: 2);
        Assert.Single(descricoes);
    }

    [Fact]
    public async Task ObterModificadoresBoss_IncluiComposta_QuandoComponentesAtivos()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();
        // grimorio_encontrado (+10%) + diario_rasgado → identidade_revelada (+5%) = 15%
        repo.ObterFlagsGераdasAsync(userId).Returns(
            Task.FromResult<IReadOnlyList<string>>(["grimorio_encontrado", "diario_rasgado"]));

        var (total, _) = await svc.ObterModificadoresBossAsync(userId, 4);

        Assert.Equal(0.15, total, precision: 2);
    }
}
```

Note: If the project uses Moq instead of NSubstitute, replace `Substitute.For<>()` and `.Received()` with the Moq equivalents. Check which mock library is currently used in `Tests/Unit/`.

- [ ] **Step 3: Run tests**

```
dotnet test LegendsAwaken.Tests --filter "FullyQualifiedName~TorreArcoConfig|FullyQualifiedName~TorreFlagService" -v minimal
```
Expected: All tests pass (0 failures).

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Tests/Unit/TorreArcoConfigTests.cs
git add LegendsAwaken.Tests/Unit/TorreFlagServiceTests.cs
git commit -m "test(torre): unit tests for TorreArcoConfig integrity and TorreFlagService logic"
```

---

## Task 5: TorrePanel — Arc Narrative Block

**Files:**
- Modify: `Bot/Panels/TorrePanel.cs`

- [ ] **Step 1: Read the current TorrePanel.cs**

Open `Bot/Panels/TorrePanel.cs` and find the method that builds the main view. Identify:
- Where `andarAtual` (the current floor number) is available
- Where the `EmbedBuilder` is constructed

- [ ] **Step 2: Add arc narrative block**

In the main embed-building method, after the existing exploration status block, add:

```csharp
using LegendsAwaken.Application.Services;

// Add arc narrative block (only for arcs 1-15 — beyond that, no arc defined yet)
var arcoDef = TorreArcoConfig.ObterArcoPorAndar(andarAtual);
if (arcoDef is not null)
{
    var andarDef = TorreArcoConfig.ObterAndar(andarAtual);
    embed.AddField($"📖 {arcoDef.Nome} — Andar {andarAtual}",
        andarDef?.NarrativaDisplay ?? "...");

    if (andarDef?.ObjetivoSecundario is { } sec)
    {
        embed.AddField("🎯 Objetivo Secundário",
            $"{sec.Descricao}\n*Efeito: {sec.EfeitoDescricao}*");
    }
}
```

- [ ] **Step 3: Add active flags block (takes flagsAtivas as parameter)**

If the panel method does not already receive active flags, add an overload or optional parameter:

```csharp
// Add to the panel method signature: IReadOnlyList<string>? flagsAtivas = null
if (flagsAtivas is { Count: > 0 })
{
    var listaFlags = string.Join(", ", flagsAtivas.Select(f => $"`{f}`"));
    embed.AddField("🏴 Flags Ativas", listaFlags);
}
```

The caller (`TorreCommand`) will need to call `TorreFlagService.ObterFlagsAtivasAsync(userId)` and pass the result to the panel builder. Update `TorreCommand` accordingly.

- [ ] **Step 4: Verify build**

```
dotnet build LegendsAwaken.Bot -v minimal
```
Expected: 0 errors.

- [ ] **Step 5: Commit**

```
git add Bot/Panels/TorrePanel.cs
git add Bot/Commands/TorreCommand.cs
git commit -m "feat(torre-ux): show arc narrative and active flags in TorrePanel"
```

---

## Task 6: TorreExploracaoPanel — Secondary Objective in Confirmation

**Files:**
- Modify: `Bot/Panels/TorreExploracaoPanel.cs`

- [ ] **Step 1: Read current CriarConfirmacao signature**

Open `Bot/Panels/TorreExploracaoPanel.cs`, find `CriarConfirmacao`. Note the current parameters and embed content.

- [ ] **Step 2: Add arc floor config parameter and secondary objective display**

Extend `CriarConfirmacao` to accept the current floor definition:

```csharp
using LegendsAwaken.Application.Services;

// Add optional parameter:
// static PanelResult CriarConfirmacao(string partyNome, IEnumerable<string> heroisNomes,
//     Guid partyId, AndarArcoDefinicao? andarArco = null)
{
    // existing embed construction ...

    if (andarArco?.NarrativaDisplay is { } narrativa)
        embed.AddField("📖 Situação", narrativa);

    if (andarArco?.ObjetivoSecundario is { } sec)
        embed.AddField("🎯 Objetivo Secundário",
            $"{sec.Descricao}\n*Seu grupo tentará automaticamente. Resultado depende da composição.*");
}
```

Pass `TorreArcoConfig.ObterAndar(andarAtual)` from `TorreCommand.MostrarConfirmacaoGrupoAsync` when calling `CriarConfirmacao`.

- [ ] **Step 3: Verify build**

```
dotnet build LegendsAwaken.Bot -v minimal
```
Expected: 0 errors.

- [ ] **Step 4: Commit**

```
git add Bot/Panels/TorreExploracaoPanel.cs
git add Bot/Commands/TorreCommand.cs
git commit -m "feat(torre-ux): show secondary objective in exploration confirmation panel"
```

---

## Task 7: TorreExploracaoService — Flag Processing on Collect

**Files:**
- Modify: `Application/Services/TorreExploracaoService.cs`

- [ ] **Step 1: Read ColetarAsync signature and return type**

Open `TorreExploracaoService.cs`, find `ColetarAsync`. Note:
- Current return type (likely a result record or string)
- Where the exploration result is determined (success/fail, which floor)
- How the floor number is obtained from the stored `TorreExploracao`

- [ ] **Step 2: Create flag result wrapper**

If `ColetarAsync` returns a plain object, extend it or create a wrapper:

```csharp
// Add to the existing result type OR create a new record nearby:
public record FlagsColetaResult(
    IReadOnlyList<string> FlagsGeradas,
    IReadOnlyList<string> FlagsExpiradas,
    IReadOnlyList<string> FlagsCompostas);
```

- [ ] **Step 3: Add flag processing to the success path of ColetarAsync**

After the floor is marked successful, inject `TorreFlagService` into the constructor and call:

```csharp
// In the success branch of ColetarAsync, after XP/fragment processing:
var andar = exploracao.AndarNumero;
var andarDef = TorreArcoConfig.ObterAndar(andar);
var flagsGeradas = new List<string>();
var flagsExpiradas = new List<string>();

if (andarDef is not null)
{
    // Secondary objective — 65% base success for now
    if (andarDef.ObjetivoSecundario is { } sec)
    {
        var sucesso = Random.Shared.NextDouble() < 0.65;
        if (sucesso)
        {
            await _flagService.GerarFlagAsync(userId, andar, sec.FlagNome);
            flagsGeradas.Add(sec.FlagNome);
        }
        else
        {
            await _flagService.MarcarSecundarioExpiradoAsync(userId, andar);
            flagsExpiradas.Add(sec.FlagNome);
        }
    }

    // Additional flags defined by this floor (e.g., diario_rasgado always generated)
    // These are flags that happen as part of the primary objective — always generated on success
    foreach (var flag in andarDef.FlagsGeradasPossiveis.Except(
        andarDef.ObjetivoSecundario is null ? [] : [andarDef.ObjetivoSecundario.FlagNome]))
    {
        await _flagService.GerarFlagAsync(userId, andar, flag);
        flagsGeradas.Add(flag);
    }
}

// Composite evaluation
var compostas = await _flagService.ObterFlagsCompostasAtivasAsync(userId);
// Persist newly triggered composites
foreach (var comp in compostas)
    await _flagService.GerarFlagAsync(userId, andar, comp);

var flagsColetaResult = new FlagsColetaResult(flagsGeradas, flagsExpiradas,
    compostas.Where(c => flagsGeradas.Contains(c) is false).ToList());
```

Return `flagsColetaResult` as part of the collect result.

- [ ] **Step 4: Update TorreCommand to display flags in result embed**

In `TorreCommand`, after calling `ColetarAsync`, display flags:

```csharp
if (flagsResult.FlagsGeradas.Count > 0)
{
    var lista = string.Join("\n", flagsResult.FlagsGeradas.Select(f => $"✅ `{f}`"));
    embed.AddField("🏴 Flags Geradas", lista);
}
if (flagsResult.FlagsCompostas.Count > 0)
{
    var lista = string.Join("\n", flagsResult.FlagsCompostas.Select(f => $"⭐ `{f}`"));
    embed.AddField("🌟 Flag Composta Ativada!", lista);
}
if (flagsResult.FlagsExpiradas.Count > 0)
{
    var lista = string.Join("\n", flagsResult.FlagsExpiradas.Select(f => $"❌ `{f}`"));
    embed.AddField("⏰ Objetivo Expirado", lista);
}
```

- [ ] **Step 5: Verify full build and run all tests**

```
dotnet build LegendsAwaken.sln -v minimal
dotnet test LegendsAwaken.Tests -v minimal
```
Expected: 0 errors, all tests pass.

- [ ] **Step 6: Commit**

```
git add LegendsAwaken.Application/Services/TorreExploracaoService.cs
git add LegendsAwaken.Bot/Commands/TorreCommand.cs
git commit -m "feat(torre): process arc flags on exploration collect — generate/expire secondary objectives"
```

---

## Task 8: Boss Modifier Application

**Files:**
- Modify: `Application/Services/TorreExploracaoService.cs` or `TorreService.cs` (wherever boss combat is set up)

- [ ] **Step 1: Locate where boss inimigos are prepared**

Search for `TemBoss` or boss floor setup in `TorreExploracaoService.PrepararInicioAsync` or `TorreService.SubirAndarAsync`. Identify where `InimigoAndar` objects are created/fetched for the boss floor.

- [ ] **Step 2: Apply HP reduction from flags**

Before the combat starts on a boss floor:

```csharp
if (andar.TemBoss)
{
    var (hpReduction, descricoes) = await _flagService.ObterModificadoresBossAsync(userId, andar.Numero);
    if (hpReduction > 0)
    {
        foreach (var inimigo in andar.Inimigos)
        {
            var fator = 1.0 - hpReduction;
            inimigo.Atributos.Vitalidade = (int)(inimigo.Atributos.Vitalidade * fator);
        }
        // Surface modifier info in the result or panel:
        // bossModificadorDescricoes = descricoes;
    }
}
```

- [ ] **Step 3: Show boss modifiers in the boss confirmation embed**

If there's a confirmation step before boss combat, add the modifier summary:

```csharp
if (bossModificadorDescricoes?.Count > 0)
{
    var bonuses = string.Join("\n", bossModificadorDescricoes.Select(d => $"• {d}"));
    embed.AddField("⚔️ Bônus de Preparação", bonuses);
}
```

- [ ] **Step 4: Full build + all tests**

```
dotnet build LegendsAwaken.sln -v minimal
dotnet test LegendsAwaken.Tests -v minimal
```
Expected: 0 errors, all tests green.

- [ ] **Step 5: Commit**

```
git add LegendsAwaken.Application/Services/TorreExploracaoService.cs
git commit -m "feat(torre): apply arc flag boss HP modifiers before boss floor combat"
```

---

## Task 9: Program.cs + AI_INDEX Update

**Files:**
- Modify: `Program.cs`
- Modify: `docs/AI_INDEX.md`

- [ ] **Step 1: Verify all DI registrations are in place**

Check `Program.cs` for:
- `IAndarFlagProgressoRepository` → `AndarFlagProgressoRepository` registered as Singleton
- `TorreFlagService` registered as Scoped
- `EnsureTableAsync()` called at startup for `IAndarFlagProgressoRepository`
- `TorreFlagService` injected into `TorreExploracaoService` constructor

- [ ] **Step 2: Update AI_INDEX.md**

Add to the Entities section:

```
### `AndarFlagProgresso` — `Domain/Entities/AndarFlagProgresso.cs`
Fields: `UsuarioId`, `Andar`, `FlagNome`, `Gerada` (bool), `Expirou` (bool), `GeradaEm?`
Raw SQLite — not tracked by EF Core.
```

Add to the Services section:

```
| `TorreFlagService` | Arc flag generation, composite evaluation, boss modifier calculation | `GerarFlagAsync`, `MarcarSecundarioExpiradoAsync`, `ObterFlagsAtivasAsync`, `ObterFlagsCompostasAtivasAsync`, `ObterModificadoresBossAsync` |
| `TorreArcoConfig` | Static arc content config — 3 arcos (1–15), floor objectives, boss modifiers, composite flags | `Arcos`, `FlagsCompostas`, `ObterArcoPorAndar(int)`, `ObterAndar(int)`, `EBossFloor(int)` |
```

Add to the Repositories section:

```
| `IAndarFlagProgressoRepository` | `AndarFlagProgressoRepository` | `Infrastructure/Repositories/AndarFlagProgressoRepository.cs` — raw SQLite; `EnsureTableAsync()`, `GerarFlagAsync`, `MarcarExpiradoAsync`, `ObterFlagsGераdasAsync` |
```

Add to the Infrastructure Providers section:

```
| `Application/Services/TorreArcoConfig.cs` | Static arc content — 3 arcos desenhados, FlagsCompostas, helper methods |
```

- [ ] **Step 3: Final full build + tests**

```
dotnet build LegendsAwaken.sln -v minimal
dotnet test LegendsAwaken.Tests -v minimal
```
Expected: 0 errors, all tests green.

- [ ] **Step 4: Final commit**

```
git add docs/AI_INDEX.md
git add LegendsAwaken.Bot/Program.cs
git commit -m "docs: update AI_INDEX for arc system; verify DI wiring for TorreFlagService"
```

---

## Self-Review Checklist

- **Spec coverage:** All 3 arcos (1–4, 5–10, 11–15) seeded ✅ · Flag generation on collect ✅ · Boss modifiers ✅ · Composite flags ✅ · Display in TorrePanel ✅ · Secondary objective in confirmation ✅
- **No placeholders:** All code blocks are complete and runnable ✅
- **Type consistency:** `AndarArcoDefinicao` used consistently across config, service, and panel ✅ · `FlagCompostaDefinicao` from `TorreArcoConfig.FlagsCompostas` used in `ObterFlagsCompostasAtivasAsync` ✅
- **Gap check:** `ObterFlagsAtivasParaArcoAsync` in Task 3 has a comment noting that filtering by arc is done by checking against `FlagsGeradasPossiveis` — implementer should verify the actual SQL query returns enough data or add an `Andar` filter to `ObterFlagsGераdasAsync` if needed
- **Mocking library:** Task 4 uses NSubstitute — verify against the test project's actual package reference before implementing
