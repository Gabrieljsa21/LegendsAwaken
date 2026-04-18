using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using Xunit;

namespace LegendsAwaken.Tests.Services;

public class FragmentServiceTests
{
    private readonly Mock<IBiomaRepository>       _biomaRepo       = new();
    private readonly Mock<IFragmentoRepository>   _fragmentoRepo   = new();
    private readonly Mock<IContratoRepository>    _contratoRepo    = new();
    private readonly Mock<IHeroiConfigRepository> _heroiConfigRepo = new();

    private FragmentService CreateService() =>
        new(_biomaRepo.Object, _fragmentoRepo.Object, _contratoRepo.Object, _heroiConfigRepo.Object);

    [Fact]
    public async Task AdicionarFragmentosAsync_CriaNovoProgresso_QuandoNaoExiste()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        FragmentoProgresso? salvo = null;

        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId))
            .ReturnsAsync((FragmentoProgresso?)null);
        _fragmentoRepo.Setup(r => r.UpsertAsync(It.IsAny<FragmentoProgresso>()))
            .Callback<FragmentoProgresso>(p => salvo = p)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.AdicionarFragmentosAsync(usuarioId, TipoFragmento.Heroi, heroiId, 5);

        Assert.NotNull(salvo);
        Assert.Equal(5, salvo!.Quantidade);
        Assert.Equal(heroiId, salvo.HeroiId);
        Assert.Equal(TipoFragmento.Heroi, salvo.TipoFragmento);
    }

    [Fact]
    public async Task AdicionarFragmentosAsync_Acumula_QuandoJaExiste()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var existente = new FragmentoProgresso
        {
            Id = Guid.NewGuid(), UsuarioId = usuarioId, HeroiId = heroiId,
            Quantidade = 10, TipoFragmento = TipoFragmento.Heroi
        };
        FragmentoProgresso? salvo = null;

        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync(existente);
        _fragmentoRepo.Setup(r => r.UpsertAsync(It.IsAny<FragmentoProgresso>()))
            .Callback<FragmentoProgresso>(p => salvo = p)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.AdicionarFragmentosAsync(usuarioId, TipoFragmento.Heroi, heroiId, 5);

        Assert.Equal(15, salvo!.Quantidade);
    }

    [Fact]
    public async Task ObterMultiplicadorAsync_RetornaBaseQuandoSemContrato()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();

        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo))
            .ReturnsAsync((Contrato?)null);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado))
            .ReturnsAsync((Contrato?)null);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId))
            .ReturnsAsync(new HeroiConfig { Id = heroiId, Arquetipo = Profissao.Guerreiro });

        var service = CreateService();
        float mult = await service.ObterMultiplicadorAsync(usuarioId, heroiId);

        Assert.Equal(1.0f, mult);
    }

    [Fact]
    public async Task ObterMultiplicadorAsync_AplicaArquetipoBonus_QuandoArquetipoCorreto()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var config    = new HeroiConfig { Id = heroiId, Arquetipo = Profissao.Arqueiro };

        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(config);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo))
            .ReturnsAsync(new Contrato { Arquetipo = Profissao.Arqueiro, Ativo = true });
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado))
            .ReturnsAsync((Contrato?)null);

        var service = CreateService();
        float mult = await service.ObterMultiplicadorAsync(usuarioId, heroiId);

        Assert.Equal(1.0f + ContractConfig.ArchetypeBonus, mult, precision: 2);
    }

    [Fact]
    public async Task ObterMultiplicadorAsync_AplicaAmbosBonus_QuandoAmboContratosAtivos()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var config    = new HeroiConfig { Id = heroiId, Arquetipo = Profissao.Arqueiro };

        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(config);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo))
            .ReturnsAsync(new Contrato { Arquetipo = Profissao.Arqueiro, Ativo = true });
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado))
            .ReturnsAsync(new Contrato { HeroiId = heroiId, Ativo = true });

        var service = CreateService();
        float mult = await service.ObterMultiplicadorAsync(usuarioId, heroiId);

        Assert.Equal(1.0f + ContractConfig.ArchetypeBonus + ContractConfig.NamedBonus, mult, precision: 2);
    }
}
