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
    public void AtributosBase_Carisma_property_exists_and_defaults_to_zero()
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
