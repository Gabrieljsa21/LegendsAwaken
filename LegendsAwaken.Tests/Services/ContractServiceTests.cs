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
    private readonly Mock<IContratoRepository>    _contratoRepo    = new();
    private readonly Mock<IHeroiConfigRepository> _heroiConfigRepo = new();
    private readonly Mock<IFragmentoRepository>   _fragmentoRepo   = new();

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
