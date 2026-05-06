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
    private readonly Mock<HeroiService>                  _heroiServiceMock = new();

    private RecruitmentService CreateService() =>
        new(_desbloqueadoRepo.Object, _heroiConfigRepo.Object, _fragmentoRepo.Object, _heroiServiceMock.Object);

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
        var heroiId   = Guid.NewGuid();
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

    [Fact]
    public async Task ProcessarMarcoTorreAsync_RetornaNull_QuandoNenhumHeroiCorresponde()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var config = new HeroiConfig { Id = heroiId, Nome = "Grom" };
        var unlock = new HeroiUnlockConfig { HeroiId = heroiId, TipoUnlock = TipoUnlock.MarcoTorre, AndarMarco = 25 };

        _heroiConfigRepo.Setup(r => r.ListarTodosAsync()).ReturnsAsync([config]);
        _heroiConfigRepo.Setup(r => r.ObterUnlockConfigAsync(heroiId)).ReturnsAsync(unlock);

        var service = CreateService();
        var resultado = await service.ProcessarMarcoTorreAsync(usuarioId, 10); // wrong floor

        Assert.Null(resultado);
    }

    [Fact]
    public async Task DesbloquearPorCondicaoAsync_Sucesso_QuandoHeroiExisteENaoDesbloqueado()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var config = new HeroiConfig { Id = heroiId, Nome = "Nyra" };
        HeroiDesbloqueado? salvo = null;

        _desbloqueadoRepo.Setup(r => r.JaDesbloqueadoAsync(usuarioId, heroiId)).ReturnsAsync(false);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(config);
        _desbloqueadoRepo.Setup(r => r.SalvarAsync(It.IsAny<HeroiDesbloqueado>()))
            .Callback<HeroiDesbloqueado>(h => salvo = h)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var resultado = await service.DesbloquearPorCondicaoAsync(usuarioId, heroiId);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(salvo);
        Assert.Equal(heroiId, salvo!.HeroiId);
    }

    [Fact]
    public async Task DesbloquearPorCondicaoAsync_Falha_QuandoJaDesbloqueado()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();

        _desbloqueadoRepo.Setup(r => r.JaDesbloqueadoAsync(usuarioId, heroiId)).ReturnsAsync(true);

        var service = CreateService();
        var resultado = await service.DesbloquearPorCondicaoAsync(usuarioId, heroiId);

        Assert.False(resultado.Sucesso);
        _desbloqueadoRepo.Verify(r => r.SalvarAsync(It.IsAny<HeroiDesbloqueado>()), Times.Never);
    }
}
