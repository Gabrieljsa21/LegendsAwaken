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
