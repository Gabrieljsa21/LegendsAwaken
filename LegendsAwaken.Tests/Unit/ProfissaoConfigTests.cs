using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Tests.Unit;

public class ProfissaoConfigTests
{
    [Theory]
    [InlineData(Profissao.Guerreiro)]
    [InlineData(Profissao.Mago)]
    [InlineData(Profissao.Ladino)]
    [InlineData(Profissao.Pesquisador)]
    public void ProfissaoConfig_DistribuicaoInicial_total_equals_60(Profissao profissao)
    {
        var dist = ProfissaoConfig.DistribuicaoInicial[profissao];
        int total = dist.ToEnumerable().Sum(t => t.Valor);
        Assert.Equal(60, total);
    }
}
