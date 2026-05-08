using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Tests.Services;

public class HeroiLevelUpServiceTests
{
    private readonly HeroiLevelUpService _sut = new();

    // ── XpParaProximoNivel ────────────────────────────────────────────────────

    [Fact]
    public void XpParaProximoNivel_1star_nivel1_retorna_80()
    {
        //Arrange & Act & Assert
        Assert.Equal(80, _sut.XpParaProximoNivel(1, raridade: 1));
    }

    [Fact]
    public void XpParaProximoNivel_5star_nivel10_retorna_2000()
    {
        //Arrange & Act & Assert
        Assert.Equal(2000, _sut.XpParaProximoNivel(10, raridade: 5));
    }

    // ── CapParaRaridade ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 20)]
    [InlineData(3, 60)]
    [InlineData(5, 100)]
    public void CapParaRaridade_retorna_valor_correto(int raridade, int expectedCap)
    {
        //Arrange & Act & Assert
        Assert.Equal(expectedCap, _sut.CapParaRaridade(raridade));
    }

    // ── CalcularPontosAtributosPorLevelUp ─────────────────────────────────────

    [Fact]
    public void PontosAtributos_5star_acima_cap4star_usa_GanhoSuperacao()
    {
        //Arrange
        // nivel 81 > 80 (4★ cap) → superação → GanhoSuperacao=1
        //Act & Assert
        Assert.Equal(1, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 81, raridade: 5));
    }

    [Fact]
    public void PontosAtributos_5star_abaixo_cap4star_usa_nivel_mod4()
    {
        //Arrange
        // nivel 50 <= 80 (cap 4★) → normal phase → 50 % 4 = 2 ≠ 0 → 0 points
        //Act & Assert
        Assert.Equal(0, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 50, raridade: 5));
    }

    [Fact]
    public void PontosAtributos_nivel4_retorna_1()
    {
        Assert.Equal(1, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 4, raridade: 1));
    }

    [Fact]
    public void PontosAtributos_nivel3_retorna_0()
    {
        Assert.Equal(0, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 3, raridade: 1));
    }

    [Fact]
    public void PontosAtributos_5star_nivel81_retorna_1_superacao()
    {
        Assert.Equal(1, _sut.CalcularPontosAtributosPorLevelUp(nivelAtual: 81, raridade: 5));
    }

    [Fact]
    public void TotalPontosNativo_1star_nivel20_igual_64()
    {
        // base=60; loop counts level-ups n=1..19 → ASI triggers at n=4,8,12,16 = 4 points → 64
        // (level 20 ASI fires on the 20→21 transition, outside the range)
        Assert.Equal(64, _sut.CalcularTotalPontosNativo(raridade: 1, nivel: 20));
    }

    // ── AplicarXp ─────────────────────────────────────────────────────────────

    [Fact]
    public void AplicarXp_Humano_aplica_multiplicador_110()
    {
        //Arrange
        // 1★ BaseXp=80, nivel=1 → xpNecessario = 80*1 = 80
        // Humano mult=1.10 → 73 * 1.10 = 80.3 → (int) = 80 → level-up ocorre
        var heroi = new Heroi
        {
            Nome = "Teste",
            Raca = Raca.Humano,
            Raridade = Raridade.Estrela1,
            Nivel = 1,
            XP = 0
        };

        //Act
        _sut.AplicarXp(heroi, 73);

        //Assert
        Assert.Equal(2, heroi.Nivel);
    }

    [Fact]
    public void AplicarXp_nao_ultrapassa_cap()
    {
        //Arrange
        // 1★ Cap=20; dar XP absurdo não deve ultrapassar nível 20
        var heroi = new Heroi
        {
            Nome = "Teste",
            Raca = Raca.Bestial,
            Raridade = Raridade.Estrela1,
            Nivel = 20,
            XP = 0
        };

        //Act
        _sut.AplicarXp(heroi, 99999);

        //Assert
        Assert.Equal(20, heroi.Nivel);
        Assert.Equal(0, heroi.XP);
    }

    // ── CalcularGrantAscensao ─────────────────────────────────────────────────

    [Fact]
    public void CalcularGrantAscensao_4to5_nivel1_retorna_zero()
    {
        //Arrange
        // All raridades share BaseStatsTotal=60; at nivel 1 both natives have 60 points.
        // Grant = CalcularTotalPontosNativo(5, 1) - CalcularTotalPontosNativo(4, 1) = 60 - 60 = 0

        //Act
        int grant = _sut.CalcularGrantAscensao(nivelAtual: 1, raridadeAtual: 4);

        //Assert
        Assert.Equal(0, grant);
    }
}
