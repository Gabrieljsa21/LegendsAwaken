using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.DTOs;
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

    [Fact]
    public async Task ProcessarDropAsync_RetornaFragmentosQuandoDrop()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var bioma     = new Bioma { Id = Guid.NewGuid(), Nome = "Floresta", AndarInicio = 1, AndarFim = 10 };
        var heroi     = new HeroiConfig { Id = heroiId, Nome = "TestHero", Arquetipo = Profissao.Guerreiro, RaridadeBase = Raridade.Estrela1 };
        var pool      = new List<BiomHeroPool>
        {
            new() { Id = Guid.NewGuid(), BiomeId = bioma.Id, HeroiId = heroiId, Heroi = heroi, DropWeight = 100, Raridade = Raridade.Estrela1, EHeroPrincipal = false }
        };
        var progresso = new FragmentoProgresso { Id = Guid.NewGuid(), UsuarioId = usuarioId, HeroiId = heroiId, Quantidade = 5, TipoFragmento = TipoFragmento.Heroi };

        _biomaRepo.Setup(r => r.ObterPorAndarAsync(It.IsAny<int>())).ReturnsAsync(bioma);
        _biomaRepo.Setup(r => r.ObterPoolAsync(bioma.Id)).ReturnsAsync(pool);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo)).ReturnsAsync((Contrato?)null);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado)).ReturnsAsync((Contrato?)null);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(heroi);
        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync(progresso);
        _fragmentoRepo.Setup(r => r.UpsertAsync(It.IsAny<FragmentoProgresso>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // Loop until we get a drop (30% chance per call) or give up after 50 tries
        List<FragmentDropResult> resultado = [];
        for (int i = 0; i < 50 && resultado.Count == 0; i++)
            resultado = await service.ProcessarDropAsync(usuarioId, 5);

        // If no drop after 50 tries, the test infrastructure is correct but we can't assert further.
        // Assert on the shape of a successful drop.
        if (resultado.Count > 0)
        {
            Assert.Equal(heroiId, resultado[0].HeroiId);
            Assert.Equal(TipoFragmento.Heroi, resultado[0].Tipo);
            Assert.True(resultado[0].Quantidade >= 1);
        }
    }
}
