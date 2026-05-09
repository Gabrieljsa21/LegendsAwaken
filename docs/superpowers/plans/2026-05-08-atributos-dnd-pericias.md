# Atributos D&D + Perícias Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace LA's 5-attribute system with 6 D&D attributes (STR/DEX/CON/INT/WIS/CHA), add the 18-skill Perícias system, rework progression to profession-based initial stats + ASI every 4 levels, and add a SkillCheckService wired into Torre exploration.

**Architecture:** Foundation-first — enum rename cascades through the codebase, so Task 1 fixes all compile errors before any new code is added. New types (HeroiPericia, SkillCheckService, ProfissaoConfig) are added after the base compiles cleanly. A data migration script resets existing hero stats to the new profession templates.

**Tech Stack:** .NET 10, EF Core + SQLite, Discord.Net, xUnit. No new NuGet packages needed.

**Spec:** `docs/superpowers/specs/2026-05-08-atributos-dnd-pericias.md`

---

## File Map

| File | Action |
|---|---|
| `LegendsAwaken.Domain/Enum/Enums.cs` | Rename 3 Atributo values, add Carisma; add Pericia enum; add AdvantageType enum |
| `LegendsAwaken.Domain/Entities/AtributosBase.cs` | Rename 3 properties, add Carisma; update Get/Set |
| `LegendsAwaken.Domain/Extensions/StatusCombateExtensions.cs` | New HP formula (no CON×level) |
| `LegendsAwaken.Domain/Entities/HeroiPericia.cs` | New entity |
| `LegendsAwaken.Domain/Interfaces/IHeroiPericiaRepository.cs` | New interface |
| `LegendsAwaken.Application/Services/AtributoBonusService.cs` | Replace switch with AdicionarPorTipo |
| `LegendsAwaken.Application/Services/CombatService.cs` | Remap formulas + leadership CHA |
| `LegendsAwaken.Application/Services/HeroPowerScoreService.cs` | Rename attrs + add Carisma weight |
| `LegendsAwaken.Application/Services/TorreExploracaoService.cs` | Vitalidade→Constituicao; add skill event hook |
| `LegendsAwaken.Application/Services/HeroiLevelUpService.cs` | BonusRacial +2; RaridadeConfig new values; nivel%4 ASI |
| `LegendsAwaken.Application/Services/PredioConfig.cs` | Vitalidade→Constituicao, Agilidade→Destreza |
| `LegendsAwaken.Application/Services/ProfissaoConfig.cs` | New — initial stat tables + HP base + proficiências |
| `LegendsAwaken.Application/Services/SkillCheckService.cs` | New — SkillRollContext + TestePericiaEvento + service |
| `LegendsAwaken.Application/Services/HeroiService.cs` | CriarHeroiAsync: profession stats + initial péricias |
| `LegendsAwaken.Application/Services/HeroiAtributosResetService.cs` | New — one-time migration for existing heroes |
| `LegendsAwaken.Bot/Panels/HeroisPanel.cs` | 6 attrs + modifiers display |
| `LegendsAwaken.Infrastructure/Repositories/HeroiPericiaRepository.cs` | New |
| `LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs` | Add DbSet<HeroiPericia> + modelBuilder config |
| `LegendsAwaken.Infrastructure/Migrations/YYYYMMDD_AtributosDnD.cs` | EF migration |
| `LegendsAwaken.Tests/Services/CombatServiceTests.cs` | Update attr names in helper |
| `LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs` | Update expected values for new Configs |
| `LegendsAwaken.Tests/Services/SkillCheckServiceTests.cs` | New |

---

## Task 1: Enum + AtributosBase foundation

**Files:**
- Modify: `LegendsAwaken.Domain/Enum/Enums.cs`
- Modify: `LegendsAwaken.Domain/Entities/AtributosBase.cs`
- Test: `LegendsAwaken.Tests/Unit/AtributosBaseTests.cs` (new)

- [ ] **Step 1: Write the failing test**

Create `LegendsAwaken.Tests/Unit/AtributosBaseTests.cs`:

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Tests.Unit;

public class AtributosBaseTests
{
    [Fact]
    public void Atributo_enum_has_six_values()
    {
        var values = Enum.GetValues<Atributo>();
        Assert.Equal(6, values.Length);
    }

    [Fact]
    public void AtributosBase_Carisma_property_exists_and_rounds_to_zero_by_default()
    {
        var a = new AtributosBase();
        Assert.Equal(0, a.Carisma);
        Assert.Equal(0, a.Get(Atributo.Carisma));
    }

    [Fact]
    public void AtributosBase_Destreza_replaces_Agilidade()
    {
        var a = new AtributosBase { Destreza = 14 };
        Assert.Equal(14, a.Get(Atributo.Destreza));
    }

    [Fact]
    public void AtributosBase_Constituicao_replaces_Vitalidade()
    {
        var a = new AtributosBase { Constituicao = 12 };
        Assert.Equal(12, a.Get(Atributo.Constituicao));
    }

    [Fact]
    public void AtributosBase_Sabedoria_replaces_Percepcao()
    {
        var a = new AtributosBase { Sabedoria = 10 };
        Assert.Equal(10, a.Get(Atributo.Sabedoria));
    }

    [Fact]
    public void Distribute_60_across_6_attrs_gives_10_each()
    {
        var a = AtributosBase.Distribute(60);
        foreach (var attr in Enum.GetValues<Atributo>())
            Assert.Equal(10, a.Get(attr));
    }

    [Fact]
    public void With_sets_single_attribute()
    {
        var a = AtributosBase.With(Atributo.Carisma, 16);
        Assert.Equal(16, a.Carisma);
        Assert.Equal(0, a.Forca);
    }

    [Fact]
    public void Plus_operator_sums_all_six_attrs()
    {
        var a = AtributosBase.Distribute(60);
        var b = AtributosBase.With(Atributo.Forca, 2);
        var c = a + b;
        Assert.Equal(12, c.Forca);
        Assert.Equal(10, c.Carisma);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```
cd C:\Workspace\LegendsAwaken
dotnet test LegendsAwaken.Tests --filter "FullyQualifiedName~AtributosBaseTests" 2>&1 | head -20
```

Expected: Compile error — `Destreza`, `Constituicao`, `Sabedoria`, `Carisma` not found.

- [ ] **Step 3: Update `Enums.cs` — rename Atributo values, add Pericia + AdvantageType**

In `LegendsAwaken.Domain/Enum/Enums.cs`, replace the `Atributo` enum:

```csharp
public enum Atributo
{
    Forca,
    Destreza,       // was Agilidade — DEX: initiative, ranged, DEX skills
    Constituicao,   // was Vitalidade — CON: HP, status resistance
    Inteligencia,
    Sabedoria,      // was Percepcao  — WIS: perception, healing, mental saves, crit
    Carisma         // new            — CHA: leadership, social events
}
```

After the existing enums, add:

```csharp
public enum Pericia
{
    Atletismo,
    Acrobacia,
    Prestidigitacao,
    Furtividade,
    Arcanismo,
    Historia,
    Investigacao,
    Natureza,
    Religiao,
    AdestrarAnimais,
    Intuicao,
    Medicina,
    Percepcao,
    Sobrevivencia,
    Enganacao,
    Intimidacao,
    Atuacao,
    Persuasao
}

public enum AdvantageType { Disadvantage = -1, Normal = 0, Advantage = 1 }
```

- [ ] **Step 4: Update `AtributosBase.cs` — rename properties, add Carisma, update Get/Set**

Replace the contents of `LegendsAwaken.Domain/Entities/AtributosBase.cs`:

```csharp
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Domain.Entities
{
    public class AtributosBase
    {
        // ── EF Core columns (one property per attribute) ───────────────────────
        // Adding a new attribute:
        //   1. Add value to the Atributo enum (Enums.cs)
        //   2. Add property here
        //   3. Add two lines to Get() and Set() below
        //   Everything else adapts automatically via Enum.GetValues<Atributo>().
        public int Forca        { get; set; }
        public int Destreza     { get; set; }   // was Agilidade
        public int Constituicao { get; set; }   // was Vitalidade
        public int Inteligencia { get; set; }
        public int Sabedoria    { get; set; }   // was Percepcao
        public int Carisma      { get; set; }

        public int Get(Atributo attr) => attr switch
        {
            Atributo.Forca        => Forca,
            Atributo.Destreza     => Destreza,
            Atributo.Constituicao => Constituicao,
            Atributo.Inteligencia => Inteligencia,
            Atributo.Sabedoria    => Sabedoria,
            Atributo.Carisma      => Carisma,
            _                     => 0
        };

        public void Set(Atributo attr, int value)
        {
            switch (attr)
            {
                case Atributo.Forca:        Forca        = value; break;
                case Atributo.Destreza:     Destreza     = value; break;
                case Atributo.Constituicao: Constituicao = value; break;
                case Atributo.Inteligencia: Inteligencia = value; break;
                case Atributo.Sabedoria:    Sabedoria    = value; break;
                case Atributo.Carisma:      Carisma      = value; break;
            }
        }

        public static AtributosBase operator +(AtributosBase a, AtributosBase b)
        {
            var result = new AtributosBase();
            foreach (var attr in System.Enum.GetValues<Atributo>())
                result.Set(attr, a.Get(attr) + b.Get(attr));
            return result;
        }

        public void AdicionarPorTipo(Atributo tipo, int valor)
            => Set(tipo, Get(tipo) + valor);

        public static AtributosBase Distribute(int total)
        {
            var attrs = System.Enum.GetValues<Atributo>();
            int perAttr = total / attrs.Length;
            var result = new AtributosBase();
            foreach (var attr in attrs)
                result.Set(attr, perAttr);
            return result;
        }

        public static AtributosBase With(Atributo attr, int value)
        {
            var result = new AtributosBase();
            result.Set(attr, value);
            return result;
        }

        public IEnumerable<(Atributo Atributo, int Valor)> ToEnumerable()
            => System.Enum.GetValues<Atributo>().Select(a => (a, Get(a)));
    }
}
```

- [ ] **Step 5: Run AtributosBase tests to verify they pass**

```
dotnet test LegendsAwaken.Tests --filter "FullyQualifiedName~AtributosBaseTests"
```

Expected: All 8 tests PASS. The rest of the solution will not compile yet — that's fine.

- [ ] **Step 6: Commit**

```
git add LegendsAwaken.Domain/Enum/Enums.cs
git add LegendsAwaken.Domain/Entities/AtributosBase.cs
git add LegendsAwaken.Tests/Unit/AtributosBaseTests.cs
git commit -m "feat(attrs): rename 3 Atributo values, add Carisma; add Pericia + AdvantageType enums"
```

---

## Task 2: Fix compile errors from the rename

**Files:**
- Modify: `LegendsAwaken.Domain/Extensions/StatusCombateExtensions.cs`
- Modify: `LegendsAwaken.Application/Services/AtributoBonusService.cs`
- Modify: `LegendsAwaken.Application/Services/CombatService.cs`
- Modify: `LegendsAwaken.Application/Services/HeroPowerScoreService.cs`
- Modify: `LegendsAwaken.Application/Services/TorreExploracaoService.cs`
- Modify: `LegendsAwaken.Application/Services/CraftingService.cs`
- Modify: `LegendsAwaken.Application/Services/PredioConfig.cs`
- Modify: `LegendsAwaken.Bot/Panels/HeroisPanel.cs`
- Modify: `LegendsAwaken.Tests/Services/CombatServiceTests.cs`

Goal: make `dotnet build` green.

- [ ] **Step 1: Fix `StatusCombateExtensions.cs`**

New HP formula per spec §4.5 — linear, no multiplicative CON:

```csharp
namespace LegendsAwaken.Domain.Extensions
{
    public static class StatusCombateExtensions
    {
        // HP = 8 (base) + nivel + MOD_CON (floor((CON-10)/2))
        // For enemies and non-profession heroes: nivel defaults to 1.
        // Heroes get HP recalculated in HeroiService using ProfissaoConfig.BaseHpPorProfissao.
        public static StatusCombate FromAtributos(this AtributosBase atr, int nivel = 1)
        {
            int modCon = (int)Math.Floor((atr.Constituicao - 10.0) / 2.0);
            int hp = 8 + nivel + modCon;
            if (hp < 1) hp = 1;
            return new StatusCombate
            {
                VidaMaxima = hp,
                VidaAtual  = hp,
                ManaMaxima = atr.Inteligencia * 5,
                ManaAtual  = atr.Inteligencia * 5
            };
        }
    }
}
```

- [ ] **Step 2: Fix `AtributoBonusService.cs` — replace switch with AdicionarPorTipo**

```csharp
public AtributosBase ObterBonus(List<HeroiHabilidade> habilidadesHeroi)
{
    var totalBonus = new AtributosBase();
    if (habilidadesHeroi == null) return totalBonus;

    foreach (var heroHabilidade in habilidadesHeroi)
    {
        if (heroHabilidade.Habilidade?.HabilidadeBonusAtributos == null) continue;
        foreach (var b in heroHabilidade.Habilidade.HabilidadeBonusAtributos)
        {
            if (b.BonusTipo != BonusTipo.Atributo) continue;
            totalBonus.AdicionarPorTipo(b.Atributo, b.BonusValor * heroHabilidade.Nivel);
        }
    }
    return totalBonus;
}
```

- [ ] **Step 3: Fix `CombatService.cs` — rename Agilidade + Vitalidade + Percepcao**

In `ExecutarRound` (line 61), replace:
```csharp
// OLD:
.Select(c => (c, init: c.Atributos.Agilidade + _random.NextDouble() * c.Atributos.Agilidade * 0.1))
// NEW:
.Select(c => (c, init: c.Atributos.Destreza + _random.NextDouble() * Math.Max(1, c.Atributos.Destreza) * 0.1))
```

In `CalcularDano` (lines 91-99), replace:
```csharp
// OLD:
double ataque = atk.Atributos.Forca;
double defesa = def.Atributos.Vitalidade;
// ...
double critChance = BaseCritChance + atk.Atributos.Percepcao * 0.001;

// NEW:
double ataque = atk.Atributos.Forca;
double defesa = def.Atributos.Constituicao;
double k      = 1000.0 + def.Nivel * 50.0;
double mitigacao = defesa / (defesa + k);
double danoBase  = ataque * skillMult * (1.0 - mitigacao) * typeMult;

int modWis = (int)Math.Floor((atk.Atributos.Sabedoria - 10.0) / 2.0);
double critChance = BaseCritChance + modWis * 0.01;  // +1% per WIS modifier
```

Add leadership bonus at the top of `IniciarCombate`, before building `encounter.Aliados`:
```csharp
// Leadership: hero with highest CHA adds MOD_CHA×1% to all party effective attrs
double liderancaMult = 1.0;
if (herois.Count > 0)
{
    int maxCha = herois.Max(h => h.ObterAtributosTotais(new AtributosBase()).Carisma);
    int modCha = (int)Math.Floor((maxCha - 10.0) / 2.0);
    if (modCha > 0) liderancaMult = 1.0 + modCha * 0.01;
}

encounter.Aliados = herois.Select(h => {
    var totais = h.ObterAtributosTotais(new AtributosBase());
    var withLeadership = new AtributosBase();
    foreach (var attr in System.Enum.GetValues<Atributo>())
        withLeadership.Set(attr, (int)(totais.Get(attr) * liderancaMult));
    return new Combatente
    {
        Id          = h.Id,
        Nome        = h.Nome,
        Nivel       = h.Nivel,
        Atributos   = withLeadership,
        Status      = h.Status,
        Habilidades = h.Habilidades,
        IsHeroi     = true
    };
}).ToList();
```

Update the constant comment at top of file:
```csharp
private const double BaseCritChance = 0.05;   // 5% base; +1% per WIS modifier
```

- [ ] **Step 4: Fix `HeroPowerScoreService.cs` — rename attrs + add Carisma weight**

Replace the `baseStats` calculation:
```csharp
double baseStats =
    (totais.Forca        * 1.2) +
    (totais.Destreza     * 1.0) +
    (totais.Inteligencia * 1.1) +
    (totais.Constituicao * 0.9) +
    (totais.Sabedoria    * 1.0) +
    (totais.Carisma      * 0.8);
```

- [ ] **Step 5: Fix `TorreExploracaoService.cs` — Vitalidade→Constituicao**

Line 273 — replace:
```csharp
// OLD:
inimigo.Atributos.Vitalidade = (int)(inimigo.Atributos.Vitalidade * fator);
// NEW:
inimigo.Atributos.Constituicao = (int)(inimigo.Atributos.Constituicao * fator);
```

- [ ] **Step 6: Fix `CraftingService.cs` — rename Percepcao + Vitalidade + Agilidade in Receitas**

Replace:
```csharp
new("arco-simples",      "Arco Simples",          SlotEquipamento.Arma,
    new Dictionary<string, int> { ["madeira"] = 4, ["ouro"] = 3 },
    new Dictionary<Atributo, int> { [Atributo.Sabedoria] = 10 }),   // was Percepcao

new("armadura-couro",    "Armadura de Couro",     SlotEquipamento.Armadura,
    new Dictionary<string, int> { ["madeira"] = 3, ["comida"] = 2 },
    new Dictionary<Atributo, int> { [Atributo.Constituicao] = 12 }), // was Vitalidade

new("amuleto-agilidade", "Amuleto de Destreza",   SlotEquipamento.Acessorio,
    new Dictionary<string, int> { ["erva"] = 2, ["madeira"] = 2 },
    new Dictionary<Atributo, int> { [Atributo.Destreza] = 8 }),      // was Agilidade
```

- [ ] **Step 7: Fix `PredioConfig.cs` — rename Vitalidade + Agilidade**

Replace in the `Slots` dictionary:
```csharp
{ (TipoPredio.Fazenda,  1), new(1, 0,  Atributo.Constituicao, 10, 2, 8)  },
{ (TipoPredio.Fazenda,  2), new(1, 0,  Atributo.Constituicao, 20, 3, 14) },
{ (TipoPredio.Fazenda,  3), new(2, 20, Atributo.Constituicao, 35, 4, 20) },
// Serraria, Mina, Forja, Arena use Forca — no change needed
{ (TipoPredio.Guilda,   1), new(1, 0,  Atributo.Destreza, 10, 2, 0)  },
{ (TipoPredio.Guilda,   2), new(1, 0,  Atributo.Destreza, 20, 3, 0)  },
```

- [ ] **Step 8: Fix `HeroisPanel.cs` — update NomeAtributo switch**

Replace `NomeAtributo` (keep existing display format for now — Task 8 adds modifiers):
```csharp
private static string NomeAtributo(Atributo attr) => attr switch
{
    Atributo.Forca        => "Força        (STR)",
    Atributo.Destreza     => "Destreza     (DEX)",
    Atributo.Constituicao => "Constituição (CON)",
    Atributo.Inteligencia => "Inteligência (INT)",
    Atributo.Sabedoria    => "Sabedoria    (WIS)",
    Atributo.Carisma      => "Carisma      (CHA)",
    _                     => attr.ToString()
};
```

- [ ] **Step 9: Fix `CombatServiceTests.cs` — rename helper parameters**

In `CriarCombatente`, rename parameters and properties:

```csharp
private static Combatente CriarCombatente(
    int forca,
    int constituicao,
    int vidaMaxima,
    int nivel       = 1,
    int sabedoria   = 0,
    int destreza    = 0,
    int? vidaAtual  = null,
    bool isHeroi    = false)
{
    return new Combatente
    {
        Id        = Guid.NewGuid(),
        Nome      = "Test",
        Nivel     = nivel,
        Atributos = new AtributosBase
        {
            Forca        = forca,
            Constituicao = constituicao,
            Sabedoria    = sabedoria,
            Destreza     = destreza
        },
        Status = new StatusCombate
        {
            VidaMaxima = vidaMaxima,
            VidaAtual  = vidaAtual ?? vidaMaxima
        },
        IsHeroi = isHeroi
    };
}
```

Update all call sites in the file to use `constituicao:` instead of `vitalidade:`, `sabedoria:` instead of `percepcao:`, `destreza:` instead of `agilidade:`.

- [ ] **Step 10: Verify build is clean**

```
dotnet build LegendsAwaken.sln
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s)` (or warnings only — no errors).

- [ ] **Step 11: Run all tests**

```
dotnet test LegendsAwaken.Tests
```

Expected: All previously passing tests still pass. `CombatServiceTests` pass with renamed params.

- [ ] **Step 12: Commit**

```
git add -p   # stage all modified files listed in this task
git commit -m "refactor(attrs): fix all compile errors after Atributo enum rename to D&D names"
```

---

## Task 3: ProfissaoConfig + HeroiLevelUpService overhaul

**Files:**
- Create: `LegendsAwaken.Application/Services/ProfissaoConfig.cs`
- Modify: `LegendsAwaken.Application/Services/HeroiLevelUpService.cs`

- [ ] **Step 1: Write failing tests for new progression**

In `LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs`, add these tests (they will fail until the implementation changes):

```csharp
[Fact]
public void PontosAtributos_nivel4_retorna_1()
{
    // nivel 4 is divisible by 4 → 1 ASI point
    Assert.Equal(1, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 4, raridade: 1));
}

[Fact]
public void PontosAtributos_nivel3_retorna_0()
{
    // nivel 3 is not divisible by 4 → no point
    Assert.Equal(0, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 3, raridade: 1));
}

[Fact]
public void PontosAtributos_5star_nivel81_retorna_1_superacao()
{
    // nivel 81 > cap4star(80) → superação → GanhoSuperacao=1
    Assert.Equal(1, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 81, raridade: 5));
}

[Fact]
public void TotalPontosNativo_1star_nivel20_igual_65()
{
    // base=60, 5 ASI points (levels 4,8,12,16,20)
    Assert.Equal(65, _sut.CalcularTotalPontosNativo(raridade: 1, nivel: 20));
}
```

- [ ] **Step 2: Run tests to verify they fail**

```
dotnet test LegendsAwaken.Tests --filter "FullyQualifiedName~HeroiLevelUpServiceTests"
```

Expected: New tests FAIL (current Configs return 2/level, not 0/1).

- [ ] **Step 3: Create `ProfissaoConfig.cs`**

Create `LegendsAwaken.Application/Services/ProfissaoConfig.cs`:

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services;

public static class ProfissaoConfig
{
    // ── Initial stat distributions (total=60 per spec §4.2) ──────────────────
    // Order: STR, DEX, CON, INT, WIS, CHA
    public static readonly IReadOnlyDictionary<Profissao, AtributosBase> DistribuicaoInicial =
        new Dictionary<Profissao, AtributosBase>
        {
            { Profissao.Guerreiro,  B(14, 10, 12,  8,  9,  7) },
            { Profissao.Arqueiro,   B( 9, 14, 10,  8, 12,  7) },
            { Profissao.Mago,       B( 7,  9, 10, 14, 12,  8) },
            { Profissao.Ladino,     B( 8, 14,  7, 12,  9, 10) },
            { Profissao.Paladino,   B(14,  8, 10,  7,  9, 12) },
            { Profissao.Clerigo,    B( 7,  8, 10,  9, 14, 12) },
            { Profissao.Bardo,      B( 8, 10,  8,  8, 10, 16) },
            { Profissao.Invocador,  B( 7,  8, 10, 14, 12,  9) },
            { Profissao.Agricultor, B(10,  9, 12,  8, 13,  8) },
            { Profissao.Pescador,   B( 9, 12, 10,  8, 12,  9) },
            { Profissao.Caçador,    B( 9, 14, 10,  8, 12,  7) },
            { Profissao.Lenhador,   B(14,  9, 12,  7, 10,  8) },
            { Profissao.Mineiro,    B(13,  8, 14,  8,  9,  8) },
            { Profissao.Cozinheiro, B( 8,  9, 10, 11, 13,  9) },
            { Profissao.Ferreiro,   B(14,  8, 12, 10,  9,  7) },
            { Profissao.Alfaiate,   B( 7, 13,  8, 11,  9, 12) },
            { Profissao.Joalheiro,  B( 7, 11,  8, 13,  9, 12) },
            { Profissao.Alquimista, B( 7,  9, 10, 14, 12,  8) },
            { Profissao.Construtor, B(13,  8, 12, 11, 10,  6) },
            { Profissao.Pesquisador,B( 6,  9,  8, 14, 12,  9) },
        };

    // ── Base HP per profession (spec §4.5) ───────────────────────────────────
    public static readonly IReadOnlyDictionary<Profissao, int> BaseHpPorProfissao =
        new Dictionary<Profissao, int>
        {
            { Profissao.Guerreiro,  12 },
            { Profissao.Paladino,   12 },
            { Profissao.Arqueiro,   10 },
            { Profissao.Ladino,     10 },
            { Profissao.Bardo,      10 },
            { Profissao.Mago,        8 },
            { Profissao.Clerigo,     8 },
            { Profissao.Invocador,   8 },
            // Civil professions: 8
        };

    private const int GanhoHpPorNivel = 1;

    public static int CalcularHpMaximo(Profissao? profissao, int nivel, int constituicao)
    {
        int baseHp = profissao.HasValue && BaseHpPorProfissao.TryGetValue(profissao.Value, out var b) ? b : 8;
        int modCon = (int)System.Math.Floor((constituicao - 10.0) / 2.0);
        int hp = baseHp + (nivel * GanhoHpPorNivel) + modCon;
        return System.Math.Max(1, hp);
    }

    // ── Initial proficiências per profession (spec §6.3) ─────────────────────
    public static readonly IReadOnlyDictionary<Profissao, Pericia[]> ProficienciasIniciais =
        new Dictionary<Profissao, Pericia[]>
        {
            { Profissao.Guerreiro,  [Pericia.Atletismo, Pericia.Intimidacao] },
            { Profissao.Arqueiro,   [Pericia.Furtividade, Pericia.Percepcao] },
            { Profissao.Mago,       [Pericia.Arcanismo, Pericia.Historia] },
            { Profissao.Ladino,     [Pericia.Prestidigitacao, Pericia.Furtividade, Pericia.Enganacao] },
            { Profissao.Paladino,   [Pericia.Atletismo, Pericia.Religiao, Pericia.Persuasao] },
            { Profissao.Clerigo,    [Pericia.Medicina, Pericia.Religiao, Pericia.Intuicao] },
            { Profissao.Bardo,      [Pericia.Persuasao, Pericia.Atuacao, Pericia.Enganacao] },
            { Profissao.Invocador,  [Pericia.Arcanismo, Pericia.Investigacao] },
            { Profissao.Agricultor, [Pericia.Natureza, Pericia.Sobrevivencia] },
            { Profissao.Pescador,   [Pericia.Natureza, Pericia.Atletismo] },
            { Profissao.Caçador,    [Pericia.Sobrevivencia, Pericia.Furtividade, Pericia.Percepcao] },
            { Profissao.Lenhador,   [Pericia.Natureza, Pericia.Atletismo] },
            { Profissao.Mineiro,    [Pericia.Atletismo, Pericia.Historia] },
            { Profissao.Cozinheiro, [Pericia.Medicina, Pericia.Natureza] },
            { Profissao.Ferreiro,   [Pericia.Atletismo, Pericia.Historia] },
            { Profissao.Alfaiate,   [Pericia.Prestidigitacao] },
            { Profissao.Joalheiro,  [Pericia.Historia, Pericia.Investigacao] },
            { Profissao.Alquimista, [Pericia.Arcanismo, Pericia.Natureza, Pericia.Medicina] },
            { Profissao.Construtor, [Pericia.Atletismo, Pericia.Historia] },
            { Profissao.Pesquisador,[Pericia.Arcanismo, Pericia.Historia, Pericia.Investigacao, Pericia.Religiao] },
        };

    // ── Fallback for null profissao ───────────────────────────────────────────
    public static AtributosBase ObterDistribuicao(Profissao? profissao)
        => profissao.HasValue && DistribuicaoInicial.TryGetValue(profissao.Value, out var d)
            ? d
            : AtributosBase.Distribute(60);

    // Helper: build AtributosBase from positional args (STR,DEX,CON,INT,WIS,CHA)
    private static AtributosBase B(int str, int dex, int con, int intel, int wis, int cha)
        => new AtributosBase
        {
            Forca        = str,
            Destreza     = dex,
            Constituicao = con,
            Inteligencia = intel,
            Sabedoria    = wis,
            Carisma      = cha,
        };
}
```

- [ ] **Step 4: Update `HeroiLevelUpService.cs`**

Replace `Configs` dictionary:
```csharp
public static readonly IReadOnlyDictionary<int, RaridadeConfig> Configs =
    new Dictionary<int, RaridadeConfig>
    {
        { 1, new(Cap:  20, BaseStatsTotal:  60, GanhoPorNivel: 0,                  BaseXp:  80) },
        { 2, new(Cap:  40, BaseStatsTotal:  60, GanhoPorNivel: 0,                  BaseXp: 100) },
        { 3, new(Cap:  60, BaseStatsTotal:  60, GanhoPorNivel: 0,                  BaseXp: 120) },
        { 4, new(Cap:  80, BaseStatsTotal:  60, GanhoPorNivel: 0,                  BaseXp: 150) },
        { 5, new(Cap: 100, BaseStatsTotal:  60, GanhoPorNivel: 0, GanhoSuperacao: 1, BaseXp: 200) },
    };
```

Replace `BonusRacial` dictionary (spec §3.3):
```csharp
public static readonly IReadOnlyDictionary<Raca, AtributosBase> BonusRacial =
    new Dictionary<Raca, AtributosBase>
    {
        { Raca.Humano,     new AtributosBase { Forca=1, Destreza=1, Constituicao=1, Inteligencia=1, Sabedoria=1, Carisma=1 } },
        { Raca.Bestial,    AtributosBase.With(Atributo.Forca,        2) },
        { Raca.Anao,       AtributosBase.With(Atributo.Constituicao, 2) },
        { Raca.Elfo,       AtributosBase.With(Atributo.Sabedoria,    2) },
        { Raca.Draconato,  AtributosBase.With(Atributo.Inteligencia, 2) },
        { Raca.Fada,       AtributosBase.With(Atributo.Destreza,     2) },
        { Raca.AnjoCaido,  AtributosBase.With(Atributo.Carisma,      2) },
        { Raca.Serafim,    AtributosBase.With(Atributo.Sabedoria,    2) },
    };
```

Replace `CalcularPontosAtributosPorLevelUp`:
```csharp
public int CalcularPontosAtributosPorLevelUp(int nivelAtual, int raridade)
{
    if (!Configs.TryGetValue(raridade, out var config)) return 0;

    // 5★ superação phase: 1 ASI point every level above 4★ cap
    if (config.GanhoSuperacao > 0
        && Configs.TryGetValue(raridade - 1, out var anterior)
        && nivelAtual > anterior.Cap)
        return config.GanhoSuperacao;

    // Normal phase: 1 ASI point every 4 levels
    return nivelAtual % 4 == 0 ? 1 : 0;
}
```

Remove or update the `ObterAtributosBaseParaRaridade` comment to note it returns evenly distributed stats (used for grant calculations, not for hero creation):
```csharp
// Returns base stats distributed evenly across all 6 attrs.
// Used for CalcularGrantAscensao math only.
// Hero creation uses ProfissaoConfig.ObterDistribuicao(profissao) instead.
public AtributosBase ObterAtributosBaseParaRaridade(int raridade)
{
    if (!Configs.TryGetValue(raridade, out var config)) return new AtributosBase();
    return AtributosBase.Distribute(config.BaseStatsTotal);
}
```

- [ ] **Step 5: Update the existing failing HeroiLevelUpService tests**

In `HeroiLevelUpServiceTests.cs`, update the two old superação tests:

```csharp
[Fact]
public void PontosAtributos_5star_acima_cap4star_usa_GanhoSuperacao()
{
    // nivel 81 > 80 (4★ cap) → superação → GanhoSuperacao=1
    Assert.Equal(1, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 81, raridade: 5));
}

[Fact]
public void PontosAtributos_5star_abaixo_cap4star_usa_nivel_mod4()
{
    // nivel 50 <= 80 (cap 4★) → normal phase → 50 % 4 = 2 ≠ 0 → 0
    Assert.Equal(0, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 50, raridade: 5));
}

[Fact]
public void PontosAtributos_5star_nivel48_retorna_1()
{
    // nivel 48 % 4 == 0 → 1
    Assert.Equal(1, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 48, raridade: 5));
}
```

- [ ] **Step 6: Run tests**

```
dotnet test LegendsAwaken.Tests --filter "FullyQualifiedName~HeroiLevelUpServiceTests"
```

Expected: All PASS.

- [ ] **Step 7: Commit**

```
git add LegendsAwaken.Application/Services/ProfissaoConfig.cs
git add LegendsAwaken.Application/Services/HeroiLevelUpService.cs
git add LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs
git commit -m "feat(attrs): add ProfissaoConfig with D&D stat tables; rework levelup to ASI every-4-levels"
```

---

## Task 4: HeroiPericia entity + repository + EF migration

**Files:**
- Create: `LegendsAwaken.Domain/Entities/HeroiPericia.cs`
- Create: `LegendsAwaken.Domain/Interfaces/IHeroiPericiaRepository.cs`
- Create: `LegendsAwaken.Infrastructure/Repositories/HeroiPericiaRepository.cs`
- Modify: `LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs`
- Run: EF migration

- [ ] **Step 1: Create `HeroiPericia.cs`**

```csharp
using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities;

public class HeroiPericia
{
    public Guid Id { get; set; }
    public Guid HeroiId { get; set; }
    public Pericia Pericia { get; set; }
    public bool TemProficiencia { get; set; }
    public int Rank { get; set; } = 0;  // reserved for future progression
    public Heroi Heroi { get; set; } = null!;
}
```

- [ ] **Step 2: Create `IHeroiPericiaRepository.cs`**

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface IHeroiPericiaRepository
{
    Task<List<HeroiPericia>> ObterPorHeroiAsync(Guid heroiId);
    Task<List<HeroiPericia>> ObterPorUsuarioAsync(ulong usuarioId);
    Task AdicionarMuitosAsync(IEnumerable<HeroiPericia> pericias);
    Task AtualizarAsync(HeroiPericia pericia);
}
```

- [ ] **Step 3: Create `HeroiPericiaRepository.cs`**

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class HeroiPericiaRepository(LegendsAwakenDbContext db) : IHeroiPericiaRepository
{
    public async Task<List<HeroiPericia>> ObterPorHeroiAsync(Guid heroiId)
        => await db.HeroisPericias.Where(p => p.HeroiId == heroiId).ToListAsync();

    public async Task<List<HeroiPericia>> ObterPorUsuarioAsync(ulong usuarioId)
        => await db.HeroisPericias
            .Include(p => p.Heroi)
            .Where(p => p.Heroi.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task AdicionarMuitosAsync(IEnumerable<HeroiPericia> pericias)
    {
        await db.HeroisPericias.AddRangeAsync(pericias);
        await db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(HeroiPericia pericia)
    {
        db.HeroisPericias.Update(pericia);
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 4: Update `LegendsAwakenDbContext.cs`**

Add DbSet after the existing sets:
```csharp
public DbSet<HeroiPericia> HeroisPericias => Set<HeroiPericia>();
```

Add to `OnModelCreating` (after the Contrato config block):
```csharp
// HeroiPericia
modelBuilder.Entity<HeroiPericia>()
    .HasKey(p => p.Id);

modelBuilder.Entity<HeroiPericia>()
    .HasOne(p => p.Heroi)
    .WithMany()
    .HasForeignKey(p => p.HeroiId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<HeroiPericia>()
    .HasIndex(p => new { p.HeroiId, p.Pericia })
    .IsUnique();
```

- [ ] **Step 5: Register repository in DI**

Find where other repositories are registered (typically `Program.cs` or a `ServiceCollectionExtensions`). Add:
```csharp
services.AddScoped<IHeroiPericiaRepository, HeroiPericiaRepository>();
```

- [ ] **Step 6: Create EF migration**

```
cd C:\Workspace\LegendsAwaken
dotnet ef migrations add AtributosDnD --project LegendsAwaken.Infrastructure --startup-project LegendsAwaken.Bot
```

Expected output: `Build succeeded. Done. To undo this action, use 'ef migrations remove'`

Open the generated migration file `Migrations/YYYYMMDDHHMMSS_AtributosDnD.cs` and verify it contains:
- `RenameColumn` calls for the 3 renamed attrs in `Herois` owned tables
- `AddColumn` for `Carisma` (both `AtributosBase_Carisma` and `AtributosDistribuidos_Carisma`)
- `CreateTable` for `HeroisPericias`

If EF generates `DropColumn` + `AddColumn` instead of `RenameColumn`, manually change them to `RenameColumn` in both `Up` and `Down`:

```csharp
// In Up():
migrationBuilder.RenameColumn("AtributosBase_Agilidade",    "Herois", "AtributosBase_Destreza");
migrationBuilder.RenameColumn("AtributosBase_Vitalidade",   "Herois", "AtributosBase_Constituicao");
migrationBuilder.RenameColumn("AtributosBase_Percepcao",    "Herois", "AtributosBase_Sabedoria");
migrationBuilder.RenameColumn("AtributosDistribuidos_Agilidade",  "Herois", "AtributosDistribuidos_Destreza");
migrationBuilder.RenameColumn("AtributosDistribuidos_Vitalidade", "Herois", "AtributosDistribuidos_Constituicao");
migrationBuilder.RenameColumn("AtributosDistribuidos_Percepcao",  "Herois", "AtributosDistribuidos_Sabedoria");

// In Down(): reverse each rename
```

- [ ] **Step 7: Apply migration**

```
dotnet ef database update --project LegendsAwaken.Infrastructure --startup-project LegendsAwaken.Bot
```

Expected: `Done.`

- [ ] **Step 8: Commit**

```
git add LegendsAwaken.Domain/Entities/HeroiPericia.cs
git add LegendsAwaken.Domain/Interfaces/IHeroiPericiaRepository.cs
git add LegendsAwaken.Infrastructure/Repositories/HeroiPericiaRepository.cs
git add LegendsAwaken.Infrastructure/LegendsAwakenDbContext.cs
git add LegendsAwaken.Infrastructure/Migrations/
git commit -m "feat(pericias): add HeroiPericia entity, repository, and EF migration AtributosDnD"
```

---

## Task 5: HeroiService — profession-based stats + initial péricias

**Files:**
- Modify: `LegendsAwaken.Application/Services/HeroiService.cs`

- [ ] **Step 1: Update constructor to accept `IHeroiPericiaRepository`**

```csharp
public HeroiService(
    IHeroiRepository heroiRepository,
    HabilidadeService habilidadeService,
    IAtributoBonusService atributoBonusProvider,
    HeroiLevelUpService levelUpService,
    IItemRepository itemRepository,
    IHeroiPericiaRepository periciaRepository)
{
    _heroiRepository       = heroiRepository;
    _habilidadeService     = habilidadeService;
    _atributoBonusProvider = atributoBonusProvider;
    _levelUpService        = levelUpService;
    _itemRepository        = itemRepository;
    _periciaRepository     = periciaRepository;
}

private readonly IHeroiPericiaRepository _periciaRepository;
```

- [ ] **Step 2: Update `CriarHeroiAsync`**

Replace the `atributosBase` computation and add péricias creation after persisting the hero:

```csharp
public async Task<Heroi> CriarHeroiAsync(
    ulong usuarioId,
    string nome,
    Raridade raridade,
    Raca raca,
    string antecedente,
    List<HeroiAfinidadeElemental> afinidade,
    FuncaoTatica? funcao = null,
    string? titulo = null,
    Profissao? profissao = null)
{
    var habilidades = await GerarHabilidadesIniciaisAsync(raridade, _habilidadeService);

    // Stats: profession template + racial bonus
    var atributosBase = ProfissaoConfig.ObterDistribuicao(profissao)
        + HeroiLevelUpService.BonusRacial.GetValueOrDefault(raca, new AtributosBase());

    var heroi = HeroiFactory.CriarHeroi(
        usuarioId, nome, raridade, raca, antecedente,
        afinidade, habilidades, atributosBase, funcao, titulo);

    heroi.Profissao = profissao;

    // Override HP with profession-based formula
    int hp = ProfissaoConfig.CalcularHpMaximo(profissao, nivel: 1, heroi.AtributosBase.Constituicao);
    heroi.Status.VidaMaxima = hp;
    heroi.Status.VidaAtual  = hp;

    heroi.DataCriacao    = DateTime.UtcNow;
    heroi.DataAlteracao  = DateTime.UtcNow;

    await _heroiRepository.AdicionarAsync(heroi);

    // Create initial pericias from profissao
    if (profissao.HasValue
        && ProfissaoConfig.ProficienciasIniciais.TryGetValue(profissao.Value, out var profs))
    {
        var pericias = profs.Select(p => new HeroiPericia
        {
            Id             = Guid.NewGuid(),
            HeroiId        = heroi.Id,
            Pericia        = p,
            TemProficiencia = true,
            Rank           = 0
        });
        await _periciaRepository.AdicionarMuitosAsync(pericias);
    }

    return heroi;
}
```

**Note:** `CriarHeroiAsync` callers in `UsuarioService` pass `config.Titulo` as last positional arg. After this change the signature has `profissao` at the end as optional, so existing call sites that don't pass it still compile (profissao defaults to null). Update `UsuarioService` to pass `config.Arquetipo` as profissao:

In `UsuarioService.DesbloquearHeroisIniciaisAsync`, change:
```csharp
var novo = await heroiService.CriarHeroiAsync(
    discordId, config.Nome, Raridade.Estrela1,
    RacaDeTag(config.Tag), "", [],
    FuncaoDeArquetipo(config.Arquetipo),
    config.Titulo,
    profissao: config.Arquetipo);   // add this
```

- [ ] **Step 3: Build and run tests**

```
dotnet build LegendsAwaken.sln
dotnet test LegendsAwaken.Tests
```

Expected: Build succeeds, all tests pass.

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Application/Services/HeroiService.cs
git add LegendsAwaken.Application/Services/UsuarioService.cs
git commit -m "feat(heroes): CriarHeroiAsync uses profession-based D&D stats and creates initial HeroiPericia records"
```

---

## Task 6: SkillCheckService (TDD)

**Files:**
- Create: `LegendsAwaken.Application/Services/SkillCheckService.cs`
- Create: `LegendsAwaken.Tests/Services/SkillCheckServiceTests.cs`

- [ ] **Step 1: Write failing tests**

Create `LegendsAwaken.Tests/Services/SkillCheckServiceTests.cs`:

```csharp
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Tests.Services;

public class SkillCheckServiceTests
{
    // ── BonusProficiencia ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(1,  2)]
    [InlineData(4,  2)]
    [InlineData(5,  3)]
    [InlineData(8,  3)]
    [InlineData(9,  4)]
    [InlineData(12, 4)]
    [InlineData(13, 5)]
    [InlineData(16, 5)]
    [InlineData(17, 6)]
    [InlineData(20, 6)]
    public void BonusProficiencia_returns_correct_value(int nivel, int expected)
    {
        Assert.Equal(expected, SkillCheckService.BonusProficiencia(nivel));
    }

    // ── AtributoDePericia ────────────────────────────────────────────────────

    [Theory]
    [InlineData(Pericia.Atletismo,    Atributo.Forca)]
    [InlineData(Pericia.Acrobacia,    Atributo.Destreza)]
    [InlineData(Pericia.Arcanismo,    Atributo.Inteligencia)]
    [InlineData(Pericia.Percepcao,    Atributo.Sabedoria)]
    [InlineData(Pericia.Persuasao,    Atributo.Carisma)]
    public void AtributoDePericia_maps_correctly(Pericia pericia, Atributo expected)
    {
        Assert.Equal(expected, SkillCheckService.AtributoDePericia(pericia));
    }

    // ── Rolar (individual) ────────────────────────────────────────────────────

    [Fact]
    public void Rolar_heroi_proficiente_nivel1_Atletismo_vs_DC5_succeeds_most_of_the_time()
    {
        // STR=14 → MOD=+2; nivel1 → prof=+2; total=+4; 2d10 range 2–20
        // Against DC5: minimum roll 1+1=2+4=6 ≥ 5 → always succeeds
        var heroi = MakeHeroi(str: 14, nivel: 1);
        var pericias = new List<HeroiPericia>
        {
            new() { Id = Guid.NewGuid(), HeroiId = heroi.Id,
                    Pericia = Pericia.Atletismo, TemProficiencia = true }
        };

        int successCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var (success, _) = SkillCheckService.Rolar(
                heroi, Pericia.Atletismo, dc: 5, pericias, new SkillRollContext());
            if (success) successCount++;
        }
        Assert.Equal(100, successCount); // 2+4=6 always beats DC5
    }

    [Fact]
    public void Rolar_heroi_sem_proficiencia_STR8_vs_DC20_fails_most_of_the_time()
    {
        // STR=8 → MOD=-1; no prof; total=-1; 2d10 max=20-1=19 < DC20 → always fails
        var heroi = MakeHeroi(str: 8, nivel: 1);
        var pericias = new List<HeroiPericia>();

        int failCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var (success, _) = SkillCheckService.Rolar(
                heroi, Pericia.Atletismo, dc: 20, pericias, new SkillRollContext());
            if (!success) failCount++;
        }
        Assert.Equal(100, failCount); // max roll 20-1=19 never beats DC20
    }

    // ── RolarGrupo (aggregate) ────────────────────────────────────────────────

    [Fact]
    public void RolarGrupo_empty_list_returns_false()
    {
        var (success, _) = SkillCheckService.RolarGrupo(
            [], Pericia.Furtividade, dc: 10, [], new SkillRollContext());
        Assert.False(success);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static Heroi MakeHeroi(int str = 10, int nivel = 1) => new Heroi
    {
        Id     = Guid.NewGuid(),
        Nome   = "Test",
        Nivel  = nivel,
        Raca   = Raca.Humano,
        AtributosBase = new AtributosBase
        {
            Forca        = str,
            Destreza     = 10,
            Constituicao = 10,
            Inteligencia = 10,
            Sabedoria    = 10,
            Carisma      = 10
        }
    };
}
```

- [ ] **Step 2: Run tests to confirm they fail (types not yet defined)**

```
dotnet test LegendsAwaken.Tests --filter "FullyQualifiedName~SkillCheckServiceTests" 2>&1 | head -20
```

Expected: Compile error — `SkillCheckService` and `SkillRollContext` not found.

- [ ] **Step 3: Create `SkillCheckService.cs`**

Create `LegendsAwaken.Application/Services/SkillCheckService.cs`:

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services;

// ── Value types used by SkillCheckService ─────────────────────────────────────

public record SkillRollContext(
    AdvantageType Advantage = AdvantageType.Normal,
    int FlatBonus = 0,
    int? AutoSuccessThreshold = null,
    bool CritEnabled = false
);

public record TestePericiaEvento(
    string Descricao,
    Pericia PericiaExigida,
    int DC,
    bool EhGrupo,
    string RecompensaSucesso,
    string PenalidadeFalha,
    SkillRollContext? RollContext = null
);

// ── Service ───────────────────────────────────────────────────────────────────

public static class SkillCheckService
{
    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Individual skill check: 2d10 + MOD + BonusProf vs DC.
    /// Returns (success, roll+bonus).
    /// </summary>
    public static (bool Success, int Total) Rolar(
        Heroi heroi,
        Pericia pericia,
        int dc,
        IEnumerable<HeroiPericia> pericias,
        SkillRollContext context)
    {
        int bonus  = ObterBonusSkill(heroi, pericia, pericias) + context.FlatBonus;
        int roll   = Rolar2d10(context.Advantage);
        int total  = roll + bonus;
        bool success = total >= dc;
        return (success, total);
    }

    /// <summary>
    /// Group aggregate check: weighted average of top-3 heroes' bonuses.
    /// Weight: best×0.6 + second×0.3 + third×0.1, then one roll vs DC.
    /// </summary>
    public static (bool Success, int Total) RolarGrupo(
        IEnumerable<Heroi> herois,
        Pericia pericia,
        int dc,
        IEnumerable<HeroiPericia> todasPericias,
        SkillRollContext context)
    {
        var heroisList = herois.ToList();
        if (heroisList.Count == 0) return (false, 0);

        var pericias = todasPericias.ToList();
        var boni = heroisList
            .Select(h => ObterBonusSkill(h, pericia, pericias))
            .OrderByDescending(x => x)
            .ToList();

        double scoreAgregado = boni.Count switch
        {
            1 => boni[0],
            2 => boni[0] * 0.6 + boni[1] * 0.3,
            _ => boni[0] * 0.6 + boni[1] * 0.3 + boni[2] * 0.1
        };

        int roll  = Rolar2d10(context.Advantage);
        int total = roll + (int)Math.Round(scoreAgregado) + context.FlatBonus;
        return (total >= dc, total);
    }

    // ── Static helpers ────────────────────────────────────────────────────────

    public static Atributo AtributoDePericia(Pericia pericia) => pericia switch
    {
        Pericia.Atletismo        => Atributo.Forca,
        Pericia.Acrobacia        => Atributo.Destreza,
        Pericia.Prestidigitacao  => Atributo.Destreza,
        Pericia.Furtividade      => Atributo.Destreza,
        Pericia.Arcanismo        => Atributo.Inteligencia,
        Pericia.Historia         => Atributo.Inteligencia,
        Pericia.Investigacao     => Atributo.Inteligencia,
        Pericia.Natureza         => Atributo.Inteligencia,
        Pericia.Religiao         => Atributo.Inteligencia,
        Pericia.AdestrarAnimais  => Atributo.Sabedoria,
        Pericia.Intuicao         => Atributo.Sabedoria,
        Pericia.Medicina         => Atributo.Sabedoria,
        Pericia.Percepcao        => Atributo.Sabedoria,
        Pericia.Sobrevivencia    => Atributo.Sabedoria,
        Pericia.Enganacao        => Atributo.Carisma,
        Pericia.Intimidacao      => Atributo.Carisma,
        Pericia.Atuacao          => Atributo.Carisma,
        Pericia.Persuasao        => Atributo.Carisma,
        _                        => Atributo.Sabedoria
    };

    public static int BonusProficiencia(int nivel) => nivel switch
    {
        <= 4  => 2,
        <= 8  => 3,
        <= 12 => 4,
        <= 16 => 5,
        _     => 6
    };

    // ── Internals ────────────────────────────────────────────────────────────

    private static int ObterBonusSkill(Heroi heroi, Pericia pericia, IEnumerable<HeroiPericia> pericias)
    {
        var atributo = AtributoDePericia(pericia);
        var totais   = heroi.ObterAtributosTotais(new AtributosBase());
        int mod      = (int)Math.Floor((totais.Get(atributo) - 10.0) / 2.0);

        var hp       = pericias.FirstOrDefault(p => p.HeroiId == heroi.Id && p.Pericia == pericia);
        int profBonus = hp?.TemProficiencia == true ? BonusProficiencia(heroi.Nivel) : 0;

        return mod + profBonus;
    }

    private static int Rolar2d10(AdvantageType advantage)
    {
        static int Roll() => Random.Shared.Next(1, 11) + Random.Shared.Next(1, 11);

        return advantage switch
        {
            AdvantageType.Advantage    => Math.Max(Roll(), Roll()),
            AdvantageType.Disadvantage => Math.Min(Roll(), Roll()),
            _                          => Roll()
        };
    }
}
```

- [ ] **Step 4: Run tests**

```
dotnet test LegendsAwaken.Tests --filter "FullyQualifiedName~SkillCheckServiceTests"
```

Expected: All PASS.

- [ ] **Step 5: Commit**

```
git add LegendsAwaken.Application/Services/SkillCheckService.cs
git add LegendsAwaken.Tests/Services/SkillCheckServiceTests.cs
git commit -m "feat(pericias): add SkillCheckService with 2d10+MOD+Prof model and group aggregate roll"
```

---

## Task 7: Torre skill event hook

**Files:**
- Create: `LegendsAwaken.Application/Services/PericiaEventoConfig.cs`
- Modify: `LegendsAwaken.Application/Services/TorreExploracaoService.cs`

- [ ] **Step 1: Create `PericiaEventoConfig.cs`**

```csharp
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services;

public static class PericiaEventoConfig
{
    public const double ChanceEventoPorAndar = 0.20;

    public static readonly IReadOnlyList<TestePericiaEvento> Eventos =
        new List<TestePericiaEvento>
        {
            new("Passagem estreita — equilíbrio ou quedas.",
                Pericia.Acrobacia, DC: 10, EhGrupo: false,
                "Progresso +5%", "Progresso -5%"),

            new("Rastros de inimigos — seguir ou perder.",
                Pericia.Sobrevivencia, DC: 12, EhGrupo: false,
                "Rota ótima: +3% progresso", "Rota errada: -8% progresso"),

            new("Armadilha arcana bloqueia a passagem.",
                Pericia.Arcanismo, DC: 15, EhGrupo: true,
                "Desarmada: +10% progresso", "Ativada: -15% progresso"),

            new("Patrulha inimiga pode ser evitada.",
                Pericia.Furtividade, DC: 12, EhGrupo: true,
                "Passagem silenciosa: +5% progresso", "Emboscada: -10% progresso"),

            new("Negociação com mercador hostil.",
                Pericia.Persuasao, DC: 12, EhGrupo: false,
                "Aliado temporário: +8% progresso", "Recusado: sem efeito"),

            new("Escuridão total — percepção salva.",
                Pericia.Percepcao, DC: 10, EhGrupo: false,
                "Caminho seguro: +5%", "Armadilha: -10% progresso"),
        }.AsReadOnly();
}
```

- [ ] **Step 2: Update `TorreExploracaoService` constructor to inject `IHeroiPericiaRepository`**

Add `IHeroiPericiaRepository periciaRepo` parameter, store in field `_periciaRepo`.

- [ ] **Step 3: Add skill event check in `ProcessarAsync`**

After the progress calculation block (after `double progressoGanho = ...;` and before checkpoints), add:

```csharp
// ── Skill event (20% chance per tick) ────────────────────────────────────
if (Random.Shared.NextDouble() < PericiaEventoConfig.ChanceEventoPorAndar)
{
    var eventoIdx = Random.Shared.Next(PericiaEventoConfig.Eventos.Count);
    var evento    = PericiaEventoConfig.Eventos[eventoIdx];
    var pericias  = await _periciaRepo.ObterPorUsuarioAsync(usuarioId);

    (bool sucesso, _) = evento.EhGrupo
        ? SkillCheckService.RolarGrupo(herois, evento.PericiaExigida, evento.DC,
            pericias, evento.RollContext ?? new SkillRollContext())
        : SkillCheckService.Rolar(
            herois.OrderByDescending(h =>
                herois.Count == 1 ? 0 :
                h.ObterAtributosTotais(new AtributosBase())
                    .Get(SkillCheckService.AtributoDePericia(evento.PericiaExigida)))
            .First(),
            evento.PericiaExigida, evento.DC, pericias,
            evento.RollContext ?? new SkillRollContext());

    // Apply result as % adjustment to this tick's progress
    double eventBonus = sucesso ? 0.05 : -0.10;
    progressoGanho = Math.Clamp(progressoGanho + eventBonus * 100.0, 0.0, 100.0 - exploracao.Progresso);
}
```

- [ ] **Step 4: Build + run tests**

```
dotnet build LegendsAwaken.sln
dotnet test LegendsAwaken.Tests
```

Expected: Build succeeds, all tests pass.

- [ ] **Step 5: Commit**

```
git add LegendsAwaken.Application/Services/PericiaEventoConfig.cs
git add LegendsAwaken.Application/Services/TorreExploracaoService.cs
git commit -m "feat(torre): add 20% per-tick skill check events using SkillCheckService"
```

---

## Task 8: HeroisPanel — 6 attrs with modifiers

**Files:**
- Modify: `LegendsAwaken.Bot/Panels/HeroisPanel.cs`

- [ ] **Step 1: Add modifier helpers and update `CriarEmbedDetalhe`**

Add private helpers at the bottom of the class:
```csharp
private static int Modificador(int valor) => (int)Math.Floor((valor - 10.0) / 2.0);
private static string ModStr(int mod) => mod >= 0 ? $"+{mod}" : $"{mod}";
```

In `CriarEmbedDetalhe`, replace the attrSb loop:
```csharp
var attrSb = new StringBuilder();
foreach (var (attr, valor) in totalAtributos.ToEnumerable())
{
    int mod = Modificador(valor);
    attrSb.AppendLine($"{NomeAtributo(attr)}: **{valor}** ({ModStr(mod)})");
}
```

- [ ] **Step 2: Build**

```
dotnet build LegendsAwaken.Bot
```

Expected: No errors.

- [ ] **Step 3: Commit**

```
git add LegendsAwaken.Bot/Panels/HeroisPanel.cs
git commit -m "feat(ux): HeroisPanel shows all 6 D&D attrs with modifier (e.g. STR: 14 (+2))"
```

---

## Task 9: Data migration for existing heroes

**Files:**
- Create: `LegendsAwaken.Application/Services/HeroiAtributosResetService.cs`
- Modify: `LegendsAwaken.Bot/Program.cs` (call at startup)

This service runs once at startup. It detects heroes whose AtributosBase total ≠ 60±5 (still on old scale) and resets them.

- [ ] **Step 1: Create `HeroiAtributosResetService.cs`**

```csharp
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class HeroiAtributosResetService(
    IHeroiRepository heroiRepo,
    IHeroiPericiaRepository periciaRepo)
{
    // Heroes whose base stat total is above D&D max (60+8racial+spread) = 80
    // still have old LA-scale stats (typically 200+) and need migration.
    private const int MaxDndBaseStatTotal = 80;

    public async Task MigrarAsync()
    {
        // Iterate by fetching all heroes. This is a one-time startup op.
        // In production with many users, process in batches.
        var cidade = await heroiRepo.ObterTodosAsync();
        foreach (var heroi in cidade)
            await MigrarHeroiAsync(heroi);
    }

    private async Task MigrarHeroiAsync(Heroi heroi)
    {
        bool needsReset = heroi.AtributosBase.ToEnumerable().Sum(t => t.Valor) > MaxDndBaseStatTotal;
        if (!needsReset) return;

        // Reset AtributosBase to profession template + racial bonus
        heroi.AtributosBase = ProfissaoConfig.ObterDistribuicao(heroi.Profissao)
            + HeroiLevelUpService.BonusRacial.GetValueOrDefault(heroi.Raca, new AtributosBase());

        // Zero out distributed points; recalculate available ASI points
        heroi.AtributosDistribuidos = new AtributosBase();
        heroi.PontosAtributosDisponiveis = heroi.Nivel / 4;

        // Recalculate HP
        int hp = ProfissaoConfig.CalcularHpMaximo(heroi.Profissao, heroi.Nivel, heroi.AtributosBase.Constituicao);
        heroi.Status.VidaMaxima = hp;
        heroi.Status.VidaAtual  = Math.Min(heroi.Status.VidaAtual, hp);

        heroi.DataAlteracao = DateTime.UtcNow;
        await heroiRepo.AtualizarAsync(heroi);

        // Create initial péricias if none exist yet
        var existing = await periciaRepo.ObterPorHeroiAsync(heroi.Id);
        if (existing.Count == 0 && heroi.Profissao.HasValue
            && ProfissaoConfig.ProficienciasIniciais.TryGetValue(heroi.Profissao.Value, out var profs))
        {
            var pericias = profs.Select(p => new HeroiPericia
            {
                Id              = Guid.NewGuid(),
                HeroiId         = heroi.Id,
                Pericia         = p,
                TemProficiencia = true,
                Rank            = 0
            });
            await periciaRepo.AdicionarMuitosAsync(pericias);
        }
    }
}
```

**Note:** `IHeroiRepository` needs an `ObterTodosAsync()` method. Check if it exists; if not, add:

In `IHeroiRepository.cs`:
```csharp
Task<List<Heroi>> ObterTodosAsync();
```

In `HeroiRepository.cs`:
```csharp
public async Task<List<Heroi>> ObterTodosAsync()
    => await _db.Herois
        .Include(h => h.Habilidades).ThenInclude(hh => hh.Habilidade)
            .ThenInclude(hab => hab.HabilidadeBonusAtributos)
        .Include(h => h.BonusAtributos)
        .ToListAsync();
```

- [ ] **Step 2: Register service and call at startup**

In `Program.cs` / `Startup`, register:
```csharp
services.AddScoped<HeroiAtributosResetService>();
```

In the startup run sequence (after database update, before bot connects):
```csharp
using var scope = app.Services.CreateScope();
var resetSvc = scope.ServiceProvider.GetRequiredService<HeroiAtributosResetService>();
await resetSvc.MigrarAsync();
```

- [ ] **Step 3: Build + start the bot locally, verify migration runs without errors**

```
dotnet build LegendsAwaken.sln
dotnet run --project LegendsAwaken.Bot
```

Expected: Startup log shows no exceptions from `MigrarAsync`. Existing heroes in the DB now have D&D-scale stats.

- [ ] **Step 4: Commit**

```
git add LegendsAwaken.Application/Services/HeroiAtributosResetService.cs
git add LegendsAwaken.Domain/Interfaces/IHeroiRepository.cs
git add LegendsAwaken.Infrastructure/Repositories/HeroiRepository.cs
git add LegendsAwaken.Bot/Program.cs
git commit -m "feat(migration): add HeroiAtributosResetService to migrate existing heroes to D&D-scale stats on startup"
```

---

## Task 10: Fix remaining tests

**Files:**
- Modify: `LegendsAwaken.Tests/Services/CombatServiceTests.cs`
- Modify: `LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs`

- [ ] **Step 1: Run all tests and note failures**

```
dotnet test LegendsAwaken.Tests --verbosity normal 2>&1 | grep -E "FAIL|PASS|Error"
```

- [ ] **Step 2: Update `CombatServiceTests` — crit chance formula changed**

The crit test that checks exact crit chance (`BaseCritChance + Percepcao*0.001`) now uses WIS modifier. Update any test that sets `percepcao:` to set `sabedoria:` instead and adjust expected values.

The crit chance is now `0.05 + modWis * 0.01`. With `sabedoria=0` → modWis=-5 → critChance=0% (clamped to 0 minimum). Update the `CalcularDano_SemDefesa_DanoIgualAAtk` test's comment:

```csharp
// Sabedoria=0 → MOD=-5 → critChance = max(0, 0.05 + (-5)*0.01) = 0%
// With no crit: dano always = 200
var atk = CriarCombatente(forca: 200, constituicao: 0, vidaMaxima: 100, sabedoria: 0);
var def = CriarCombatente(forca: 0,   constituicao: 0, vidaMaxima: 2000);
int dano = _service.CalcularDano(atk, def, skillMult: 1.0);
Assert.Equal(200, dano); // deterministic: no crit possible
```

Update `CombatService.CalcularDano` to clamp critChance to ≥0:
```csharp
double critChance = Math.Max(0, BaseCritChance + modWis * 0.01);
```

- [ ] **Step 3: Update `HeroiLevelUpServiceTests` — remove tests for old values**

Remove or update tests that depended on the old `GanhoPorNivel=2` values. The failing ones will be obvious from the test run. Update expected values to match the new `nivel%4` logic.

The test `XpParaProximoNivel_1star_nivel1_retorna_80` — this still passes since `BaseXp` is unchanged.

The test `XpParaProximoNivel_5star_nivel10_retorna_2000` — still passes (`200 * 10 = 2000`).

The test for `CalcularTotalPontosNativo` — update expected values:
- 1★ nivel 5: base=60 + 1 point (at nivel 4) = 61
- 1★ nivel 9: base=60 + 2 points (at 4,8) = 62

- [ ] **Step 4: Run all tests — all must pass**

```
dotnet test LegendsAwaken.Tests
```

Expected: All tests pass. 0 failures.

- [ ] **Step 5: Final commit**

```
git add LegendsAwaken.Tests/Services/CombatServiceTests.cs
git add LegendsAwaken.Tests/Services/HeroiLevelUpServiceTests.cs
git commit -m "test: update CombatService + HeroiLevelUpService tests for D&D attribute model"
```

---

## Self-Review Checklist

**Spec coverage:**
- §2 Hierarquia de cálculo: `ObterAtributosTotais` unchanged — existing layer composition preserved ✓
- §3 6 Atributos: enum + AtributosBase updated → Task 1 ✓; Carisma leadership → Task 2 CombatService ✓
- §3.3 Bônus racial: +2 values → Task 3 BonusRacial ✓
- §4.2 Distribuição inicial: ProfissaoConfig.DistribuicaoInicial → Task 3 ✓
- §4.3 ASI custo crescente: nivel%4 logic in CalcularPontosAtributosPorLevelUp → Task 3 ✓
- §4.4 RaridadeConfig: new values → Task 3 ✓
- §4.5 HP formula: ProfissaoConfig.CalcularHpMaximo + StatusCombateExtensions → Task 2 + 5 ✓
- §5 Combat formulas: CombatService remap → Task 2 ✓
- §6.1 Pericia enum: → Task 1 ✓
- §6.2 Bônus proficiência: BonusProficiencia() → Task 6 ✓
- §6.3 Proficiências iniciais: ProfissaoConfig.ProficienciasIniciais → Task 3 ✓
- §6.4 HeroiPericia entity: → Task 4 ✓
- §6.5 SkillRollContext: → Task 6 ✓
- §7.1 DC tiers: referenced in PericiaEventoConfig events → Task 7 ✓
- §7.2 2d10: SkillCheckService.Rolar2d10 → Task 6 ✓
- §7.3 Aggregate group: RolarGrupo → Task 6 ✓
- §7.5 TestePericiaEvento: → Task 6 ✓
- §7.6 Torre integration: → Task 7 ✓
- §8 Data migration: HeroiAtributosResetService → Task 9 ✓

**Out of scope confirmed:**
- Dice system for damage (§10)
- Racial traits/passivas (§10)
- Profissao vs Classe separation (§3.4)
- XP mastery for perícias (Rank reserved but not activated)
