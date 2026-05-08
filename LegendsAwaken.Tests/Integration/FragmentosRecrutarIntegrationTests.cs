using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using LegendsAwaken.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LegendsAwaken.Tests.Integration;

/// <summary>
/// Integration tests for the fragmentos → recrutar flow.
/// Uses a real in-memory SQLite database with real repository implementations.
/// HeroiService is constructed with mocked sub-dependencies because it is a
/// concrete class (not behind an interface) whose internal hero-creation path
/// is outside the scope of this flow's assertions.
/// </summary>
public class FragmentosRecrutarIntegrationTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly LegendsAwakenDbContext _db;
    private readonly HeroiDesbloqueadoRepository _desbloqueadoRepo;
    private readonly HeroiConfigRepository _heroiConfigRepo;
    private readonly FragmentoRepository _fragmentoRepo;
    private readonly HeroiService _heroiService;

    public FragmentosRecrutarIntegrationTests()
    {
        _conn = new SqliteConnection($"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        _conn.Open();

        var opts = new DbContextOptionsBuilder<LegendsAwakenDbContext>()
            .UseSqlite(_conn)
            .Options;

        _db = new LegendsAwakenDbContext(opts);
        _db.Database.EnsureCreated();

        _desbloqueadoRepo = new HeroiDesbloqueadoRepository(_db);
        _heroiConfigRepo   = new HeroiConfigRepository(_db);
        _fragmentoRepo     = new FragmentoRepository(_db);

        //Arrange HeroiService with mocked sub-dependencies
        //ObterHeroisPorUsuarioAsync returns empty list → Desbloquear will call CriarHeroiAsync
        //CriarHeroiAsync calls AdicionarAsync on the hero repo mock → returns completed task
        var heroiRepoMock      = new Mock<IHeroiRepository>();
        var habilidadeRepoMock = new Mock<IHabilidadeRepository>();
        var atributoBonusMock  = new Mock<IAtributoBonusService>();
        var itemRepoMock       = new Mock<IItemRepository>();
        var periciaRepoMock    = new Mock<IHeroiPericiaRepository>();

        heroiRepoMock
            .Setup(r => r.ObterPorUsuarioIdAsync(It.IsAny<ulong>()))
            .ReturnsAsync(new List<Heroi>());

        heroiRepoMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Heroi>()))
            .Returns(Task.CompletedTask);

        habilidadeRepoMock
            .Setup(r => r.ObterTodasAsync())
            .ReturnsAsync(new List<Habilidade>());

        atributoBonusMock
            .Setup(s => s.ObterBonus(It.IsAny<List<HeroiHabilidade>>()))
            .Returns(new AtributosBase());

        periciaRepoMock
            .Setup(r => r.AdicionarMuitosAsync(It.IsAny<IEnumerable<HeroiPericia>>()))
            .Returns(Task.CompletedTask);

        var habilidadeService = new HabilidadeService(habilidadeRepoMock.Object);
        var levelUpService    = new HeroiLevelUpService();

        _heroiService = new HeroiService(
            heroiRepoMock.Object,
            habilidadeService,
            atributoBonusMock.Object,
            levelUpService,
            itemRepoMock.Object,
            periciaRepoMock.Object);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
        _conn.Dispose();
    }

    private RecruitmentService CreateService() =>
        new(_desbloqueadoRepo, _heroiConfigRepo, _fragmentoRepo, _heroiService);

    private static (HeroiConfig config, HeroiUnlockConfig unlock) SeedHeroi(
        LegendsAwakenDbContext db,
        Guid heroiId,
        int quantidadeFragmentos = 30)
    {
        var config = new HeroiConfig
        {
            Id          = heroiId,
            Nome        = "Grom",
            RaridadeBase = Raridade.Estrela3,
            Arquetipo   = Profissao.Guerreiro,
        };

        var unlock = new HeroiUnlockConfig
        {
            HeroiId              = heroiId,
            TipoUnlock           = TipoUnlock.Fragmentos,
            QuantidadeFragmentos = quantidadeFragmentos,
        };

        db.HeroiConfigs.Add(config);
        db.HeroiUnlockConfigs.Add(unlock);
        db.SaveChanges();

        return (config, unlock);
    }

    private static FragmentoProgresso SeedFragmento(
        LegendsAwakenDbContext db,
        Guid usuarioId,
        Guid heroiId,
        int quantidade)
    {
        var progresso = new FragmentoProgresso
        {
            Id            = Guid.NewGuid(),
            UsuarioId     = usuarioId,
            HeroiId       = heroiId,
            TipoFragmento = TipoFragmento.Heroi,
            Quantidade    = quantidade,
            AtualizadoEm  = DateTime.UtcNow,
        };

        db.FragmentosProgresso.Add(progresso);
        db.SaveChanges();

        return progresso;
    }

    [Fact(DisplayName = "Recrutamento com fragmentos suficientes (30/30) deve retornar sucesso e persistir desbloqueio")]
    [Trait("Category", "Integration - Recrutamento")]
    public async Task TentarRecrutarPorFragmentosAsync_FragmentosSuficientes_DeveRetornarSucessoEPersistirDesbloqueio()
    {
        //Arrange
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();

        SeedHeroi(_db, heroiId, quantidadeFragmentos: 30);
        SeedFragmento(_db, usuarioId, heroiId, quantidade: 30);

        var service = CreateService();

        //Act
        var resultado = await service.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);

        //Assert
        Assert.True(resultado.Sucesso);
        Assert.True(await _desbloqueadoRepo.JaDesbloqueadoAsync(usuarioId, heroiId));
    }

    [Fact(DisplayName = "Recrutamento com fragmentos insuficientes (15/30) deve retornar falha com mensagem indicando progresso")]
    [Trait("Category", "Integration - Recrutamento")]
    public async Task TentarRecrutarPorFragmentosAsync_FragmentosInsuficientes_DeveRetornarFalhaComMensagemDeProgresso()
    {
        //Arrange
        var usuarioId = Guid.NewGuid();
        var heroiId   = Guid.NewGuid();

        SeedHeroi(_db, heroiId, quantidadeFragmentos: 30);
        SeedFragmento(_db, usuarioId, heroiId, quantidade: 15);

        var service = CreateService();

        //Act
        var resultado = await service.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);

        //Assert
        Assert.False(resultado.Sucesso);
        Assert.Contains("15/30", resultado.Mensagem);
        Assert.False(await _desbloqueadoRepo.JaDesbloqueadoAsync(usuarioId, heroiId));
    }
}
