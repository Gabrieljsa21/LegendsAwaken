using LegendsAwaken.Application.Services;

namespace LegendsAwaken.Tests.Unit;

public class TorreArcoConfigTests
{
    [Fact]
    public void Arcos_CobreTodos15Andares_SemGaps()
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
    public void EBossFloor_RetornaTrueParaBossFloors()
    {
        Assert.True(TorreArcoConfig.EBossFloor(4));
        Assert.True(TorreArcoConfig.EBossFloor(10));
        Assert.True(TorreArcoConfig.EBossFloor(15));
        Assert.False(TorreArcoConfig.EBossFloor(1));
        Assert.False(TorreArcoConfig.EBossFloor(99));
    }
}
