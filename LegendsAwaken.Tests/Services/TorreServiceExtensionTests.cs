using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using EntityTipoAndar = LegendsAwaken.Domain.Entities.TipoAndar;

namespace LegendsAwaken.Tests.Services;

public class TorreServiceExtensionTests
{
    // ── Shared infrastructure ───────────────────────────────────────────────

    private readonly Mock<ITorreRepository>          _torreRepo       = new();
    private readonly Mock<IHeroiRepository>          _heroiRepo       = new();
    private readonly Mock<IBiomaRepository>          _biomaRepo       = new();
    private readonly Mock<IFragmentoRepository>      _fragmentoRepo   = new();
    private readonly Mock<IContratoRepository>       _contratoRepo    = new();
    private readonly Mock<IHeroiConfigRepository>    _heroiConfigRepo = new();
    private readonly Mock<IHeroiDesbloqueadoRepository> _desbloquRepo = new();

    private static HeroiService BuildHeroiService(
        Mock<IHeroiRepository> heroiRepo,
        Mock<IFragmentoRepository> fragmentoRepo)
    {
        heroiRepo.Setup(r => r.AdicionarAsync(It.IsAny<Heroi>())).Returns(Task.CompletedTask);
        heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(It.IsAny<ulong>())).ReturnsAsync(new List<Heroi>());

        var habilidadeRepo = new Mock<IHabilidadeRepository>();
        habilidadeRepo.Setup(r => r.ObterTodasAsync()).ReturnsAsync(new List<Habilidade>());

        var atributoBonusService = new Mock<IAtributoBonusService>();
        atributoBonusService
            .Setup(s => s.ObterBonus(It.IsAny<List<HeroiHabilidade>>()))
            .Returns(new AtributosBase());

        var itemRepo = new Mock<IItemRepository>();
        itemRepo.Setup(r => r.ObterPorProprietarioAsync(It.IsAny<ulong>())).ReturnsAsync(new List<Item>());

        return new HeroiService(
            heroiRepo.Object,
            new HabilidadeService(habilidadeRepo.Object),
            atributoBonusService.Object,
            new HeroiLevelUpService(),
            itemRepo.Object);
    }

    private TorreService CreateService()
    {
        var levelUpService     = new HeroiLevelUpService();
        var fragmentService    = new FragmentService(_biomaRepo.Object, _fragmentoRepo.Object, _contratoRepo.Object, _heroiConfigRepo.Object);
        var biomeService       = new BiomeService(_biomaRepo.Object);
        var heroiService       = BuildHeroiService(_heroiRepo, _fragmentoRepo);
        var recruitmentService = new RecruitmentService(_desbloquRepo.Object, _heroiConfigRepo.Object, _fragmentoRepo.Object, heroiService);
        var rewardService      = new RewardDistributionService();

        return new TorreService(
            _torreRepo.Object,
            _heroiRepo.Object,
            levelUpService,
            fragmentService,
            biomeService,
            recruitmentService,
            rewardService);
    }

    // ── Helper builders ─────────────────────────────────────────────────────

    private static TorreAndar BuildAndar(int numero, bool objetivoCumprido) =>
        new()
        {
            Id               = Guid.NewGuid(),
            UsuarioId        = Guid.NewGuid(),
            Numero           = numero,
            Tipo             = EntityTipoAndar.Normal,
            ObjetivoCumprido = objetivoCumprido,
            CriadoEm        = DateTime.UtcNow,
        };

    private static Heroi BuildHeroi() =>
        new()
        {
            Id       = Guid.NewGuid(),
            Nome     = "TestHeroi",
            Raridade = Raridade.Estrela1,
            Raca     = Raca.Humano,
            Nivel    = 1,
            XP       = 0,
        };

    // ── Tests: early-failure path ───────────────────────────────────────────

    [Fact]
    public async Task SubirAndar_RetornaFalso_QuandoObjetivoCumpridoFalso()
    {
        var usuarioId  = Guid.NewGuid();
        var andar      = BuildAndar(1, objetivoCumprido: false);

        _torreRepo.Setup(r => r.ObterAndarPorUsuarioAsync(usuarioId)).ReturnsAsync(andar);

        var service   = CreateService();
        var resultado = await service.SubirAndarAsync(usuarioId, []);

        Assert.False(resultado.Sucesso);
        Assert.Equal(0, resultado.XpConcedido);
        Assert.Equal(0, resultado.OuroGanho);
        Assert.Empty(resultado.Fragmentos);
        Assert.Null(resultado.NovoBioma);
        Assert.Null(resultado.HeroiDesbloqueado);
        Assert.Empty(resultado.RewardPayloads);
    }

    // ── Tests: fragment drop path ───────────────────────────────────────────

    [Fact]
    public async Task SubirAndar_RetornaFragmentos_QuandoDropOcorre()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        var andar     = BuildAndar(3, objetivoCumprido: true);

        var bioma   = new Bioma { Id = Guid.NewGuid(), Nome = "Floresta", AndarInicio = 1, AndarFim = 10, Descricao = "Verde" };
        var heroCfg = new HeroiConfig { Id = heroiId, Nome = "Grom", Arquetipo = Profissao.Guerreiro, RaridadeBase = Raridade.Estrela1 };
        var pool    = new List<BiomHeroPool>
        {
            new() { Id = Guid.NewGuid(), BiomeId = bioma.Id, HeroiId = heroiId, Heroi = heroCfg, DropWeight = 100, Raridade = Raridade.Estrela1 }
        };
        var progresso = new FragmentoProgresso
        {
            Id            = Guid.NewGuid(),
            UsuarioId     = usuarioId,
            HeroiId       = heroiId,
            Quantidade    = 3,
            TipoFragmento = TipoFragmento.Heroi
        };

        _torreRepo.Setup(r => r.ObterAndarPorUsuarioAsync(usuarioId)).ReturnsAsync(andar);
        _torreRepo.Setup(r => r.AdicionarAsync(It.IsAny<TorreAndar>())).Returns(Task.CompletedTask);
        _heroiRepo.Setup(r => r.AtualizarAsync(It.IsAny<Heroi>())).Returns(Task.CompletedTask);

        // Fragment drop: bioma exists, pool exists, no contracts
        _biomaRepo.Setup(r => r.ObterPorAndarAsync(andar.Numero)).ReturnsAsync(bioma);
        _biomaRepo.Setup(r => r.ObterPoolAsync(bioma.Id)).ReturnsAsync(pool);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo)).ReturnsAsync((Contrato?)null);
        _contratoRepo.Setup(r => r.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado)).ReturnsAsync((Contrato?)null);
        _heroiConfigRepo.Setup(r => r.ObterPorIdAsync(heroiId)).ReturnsAsync(heroCfg);
        _fragmentoRepo.Setup(r => r.ObterPorHeroiAsync(usuarioId, heroiId)).ReturnsAsync(progresso);
        _fragmentoRepo.Setup(r => r.UpsertAsync(It.IsAny<FragmentoProgresso>())).Returns(Task.CompletedTask);

        // Next floor biome: same biome (no new biome event)
        _biomaRepo.Setup(r => r.ObterPorAndarAsync(andar.Numero + 1)).ReturnsAsync(bioma);

        // Marco check: floor 4 is not a marco
        _heroiConfigRepo.Setup(r => r.ListarTodosAsync()).ReturnsAsync([]);

        var service = CreateService();

        // ProcessarDropAsync has a random chance — loop until a drop occurs or 50 attempts
        SubirAndarResult? resultado = null;
        for (int i = 0; i < 50; i++)
        {
            resultado = await service.SubirAndarAsync(usuarioId, []);
            if (resultado.Fragmentos.Count > 0) break;
        }

        Assert.NotNull(resultado);
        Assert.True(resultado!.Sucesso);

        if (resultado.Fragmentos.Count > 0)
        {
            Assert.Equal(heroiId, resultado.Fragmentos[0].HeroiId);
            Assert.True(resultado.Fragmentos[0].Quantidade >= 1);
            // A RewardPayload should be generated for the drop
            Assert.Contains(resultado.RewardPayloads, p => p.Tipo == TipoReward.Micro);
        }
    }

    // ── Tests: new biome detection ──────────────────────────────────────────

    [Fact]
    public async Task SubirAndar_RetornaNovoBioma_QuandoBiomasMuda()
    {
        var usuarioId = Guid.NewGuid();
        var andar     = BuildAndar(10, objetivoCumprido: true);

        var biomaAtual    = new Bioma { Id = Guid.NewGuid(), Nome = "Ruinas", AndarInicio = 1,  AndarFim = 10, Descricao = "Velhas ruinas" };
        var biomaProximo  = new Bioma { Id = Guid.NewGuid(), Nome = "Vulcao", AndarInicio = 11, AndarFim = 25, Descricao = "Calor intenso" };

        _torreRepo.Setup(r => r.ObterAndarPorUsuarioAsync(usuarioId)).ReturnsAsync(andar);
        _torreRepo.Setup(r => r.AdicionarAsync(It.IsAny<TorreAndar>())).Returns(Task.CompletedTask);
        _heroiRepo.Setup(r => r.AtualizarAsync(It.IsAny<Heroi>())).Returns(Task.CompletedTask);

        // Biome lookups: cleared floor (10) returns biomaAtual, next floor (11) returns biomaProximo
        _biomaRepo.Setup(r => r.ObterPorAndarAsync(andar.Numero)).ReturnsAsync(biomaAtual);
        _biomaRepo.Setup(r => r.ObterPorAndarAsync(andar.Numero + 1)).ReturnsAsync(biomaProximo);

        // Fragment drop: pool is empty → ProcessarDropAsync returns [] immediately
        _biomaRepo.Setup(r => r.ObterPoolAsync(biomaAtual.Id)).ReturnsAsync([]);

        // Floor 11 is a marco (present in BiomeService._marcos): yes, 10 is a marco
        // ProcessarMarcoTorreAsync with floor 11: no hero matches
        _heroiConfigRepo.Setup(r => r.ListarTodosAsync()).ReturnsAsync([]);

        var service   = CreateService();
        var resultado = await service.SubirAndarAsync(usuarioId, []);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.NovoBioma);
        Assert.Equal("Vulcao", resultado.NovoBioma!.Nome);
        Assert.Contains(resultado.RewardPayloads, p => p.Tipo == TipoReward.Alto);
    }

    // ── Tests: marco floor unlocks iconic hero ──────────────────────────────

    [Fact]
    public async Task SubirAndar_RetornaHeroiDesbloqueado_QuandoMarcoCorresponde()
    {
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();
        // Floor 9 → advances to floor 10 (a marco floor)
        var andar     = BuildAndar(9, objetivoCumprido: true);

        var heroCfg = new HeroiConfig { Id = heroiId, Nome = "Kaen", Arquetipo = Profissao.Guerreiro, RaridadeBase = Raridade.Estrela4 };
        var unlock  = new HeroiUnlockConfig { HeroiId = heroiId, TipoUnlock = TipoUnlock.MarcoTorre, AndarMarco = 10 };

        _torreRepo.Setup(r => r.ObterAndarPorUsuarioAsync(usuarioId)).ReturnsAsync(andar);
        _torreRepo.Setup(r => r.AdicionarAsync(It.IsAny<TorreAndar>())).Returns(Task.CompletedTask);
        _heroiRepo.Setup(r => r.AtualizarAsync(It.IsAny<Heroi>())).Returns(Task.CompletedTask);

        // Biome lookups: same bioma for floor 9 and 10 → no new biome event
        var bioma = new Bioma { Id = Guid.NewGuid(), Nome = "Floresta", AndarInicio = 1, AndarFim = 10, Descricao = "x" };
        _biomaRepo.Setup(r => r.ObterPorAndarAsync(andar.Numero)).ReturnsAsync(bioma);
        _biomaRepo.Setup(r => r.ObterPorAndarAsync(andar.Numero + 1)).ReturnsAsync(bioma);

        // Fragment drop: pool is empty → ProcessarDropAsync returns [] immediately
        _biomaRepo.Setup(r => r.ObterPoolAsync(bioma.Id)).ReturnsAsync([]);

        // Marco floor 10: hero Kaen should unlock
        _heroiConfigRepo.Setup(r => r.ListarTodosAsync()).ReturnsAsync([heroCfg]);
        _heroiConfigRepo.Setup(r => r.ObterUnlockConfigAsync(heroiId)).ReturnsAsync(unlock);
        _desbloquRepo.Setup(r => r.JaDesbloqueadoAsync(usuarioId, heroiId)).ReturnsAsync(false);
        _desbloquRepo.Setup(r => r.SalvarAsync(It.IsAny<HeroiDesbloqueado>())).Returns(Task.CompletedTask);

        var service   = CreateService();
        var resultado = await service.SubirAndarAsync(usuarioId, []);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.HeroiDesbloqueado);
        Assert.Equal(heroiId, resultado.HeroiDesbloqueado!.Id);
        Assert.Equal("Kaen", resultado.HeroiDesbloqueado.Nome);
        Assert.Contains(resultado.RewardPayloads, p => p.Tipo == TipoReward.Alto);
    }
}
