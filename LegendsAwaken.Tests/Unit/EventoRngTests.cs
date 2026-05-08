using LegendsAwaken.Application.Services;
using System.Collections.Generic;
using System.Linq;
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

        Assert.True(contagem["comum"] > 900, $"Comum escolhido apenas {contagem["comum"]} vezes");
    }
}
