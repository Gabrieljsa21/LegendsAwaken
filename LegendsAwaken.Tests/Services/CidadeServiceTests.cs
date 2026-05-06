using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;

namespace LegendsAwaken.Tests.Services;

public class CidadeServiceTests
{
    private readonly Mock<ICidadeRepository>       _cidadeRepo  = new();
    private readonly Mock<IHeroiRepository>        _heroiRepo   = new();
    private readonly Mock<ISlotOcupacaoRepository> _slotRepo    = new();
    private readonly Mock<ICidadeBoosterRepository> _boosterRepo = new();

    private CidadeService CreateService()
    {
        var boosterService = new CidadeBoosterService(_boosterRepo.Object);
        return new CidadeService(_cidadeRepo.Object, _heroiRepo.Object, _slotRepo.Object, boosterService);
    }

    // Booster repo returns null active booster → multiplier 1.0
    private void SetupNullBooster(ulong usuarioId)
    {
        _boosterRepo.Setup(r => r.ObterAtivoAsync(usuarioId))
                    .ReturnsAsync((CidadeBoosterAtivo?)null);
    }

    // Slot repo returns no occupied building slots
    private void SetupEmptySlots()
    {
        _slotRepo.Setup(r => r.ObterPorConstrucaoAsync(It.IsAny<Guid>()))
                 .ReturnsAsync(new List<SlotOcupacao>());
    }

    // ── Teste 1: 1 trabalhador, 2 horas → produção > 0 ──────────────────────

    [Fact(DisplayName = "1 worker in Campo node after 2h must produce Comida > 0")]
    [Trait("Category", "CidadeService")]
    public async Task ColetarProducaoAsync_UmTrabalhadorApos2Horas_DeveProduzirComida()
    {
        //Arrange
        const ulong usuarioId = 1111UL;
        var heroiId = Guid.NewGuid();

        var heroi = new Heroi
        {
            Id        = heroiId,
            Nome      = "Herói Teste",
            Raca      = Raca.Humano,
            Profissao = null,   // sem bônus de profissão
            Humor     = 50
        };

        var trabalhador = new PersonagemTrabalhador
        {
            Id            = Guid.NewGuid(),
            HeroiId       = heroiId,
            ResourceNode  = TipoResourceNode.Campo,
            InicioTrabalho = DateTime.UtcNow.AddHours(-2)
        };

        var cidade = new Cidade
        {
            Id            = Guid.NewGuid(),
            UsuarioId     = usuarioId,
            Nome          = "Cidade Teste",
            UltimaColeta  = DateTime.UtcNow.AddHours(-2),
            Recursos      = new Recursos(),
            Construcoes   = new List<Construcao>(),
            Trabalhadores = new List<PersonagemTrabalhador> { trabalhador }
        };

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId)).ReturnsAsync(cidade);
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId)).ReturnsAsync(new List<Heroi> { heroi });
        _cidadeRepo.Setup(r => r.AtualizarAsync(cidade)).Returns(Task.CompletedTask);
        SetupNullBooster(usuarioId);
        SetupEmptySlots();

        var service = CreateService();

        //Act
        var (_, produzido) = await service.ColetarProducaoAsync(usuarioId);

        //Assert
        // Campo base: 6 comida/h × 2h = 12
        Assert.True(produzido.Comida > 0,
            $"Esperava Comida > 0, mas obteve {produzido.Comida}.");
    }

    // ── Teste 2: UltimaColeta há 30s → retorna Recursos vazios ──────────────

    [Fact(DisplayName = "Collection attempt 30s after last collect must return empty Recursos")]
    [Trait("Category", "CidadeService")]
    public async Task ColetarProducaoAsync_UltimaColetaHa30Segundos_RetornaRecursosVazios()
    {
        //Arrange
        const ulong usuarioId = 2222UL;
        var heroiId = Guid.NewGuid();

        var trabalhador = new PersonagemTrabalhador
        {
            Id            = Guid.NewGuid(),
            HeroiId       = heroiId,
            ResourceNode  = TipoResourceNode.Floresta,
            InicioTrabalho = DateTime.UtcNow.AddMinutes(-30)
        };

        var cidade = new Cidade
        {
            Id            = Guid.NewGuid(),
            UsuarioId     = usuarioId,
            Nome          = "Cidade Teste Guard",
            UltimaColeta  = DateTime.UtcNow.AddSeconds(-30),   // apenas 30 segundos
            Recursos      = new Recursos(),
            Construcoes   = new List<Construcao>(),
            Trabalhadores = new List<PersonagemTrabalhador> { trabalhador }
        };

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId)).ReturnsAsync(cidade);
        SetupNullBooster(usuarioId);
        SetupEmptySlots();

        var service = CreateService();

        //Act
        var (_, produzido) = await service.ColetarProducaoAsync(usuarioId);

        //Assert
        Assert.Equal(0, produzido.Comida);
        Assert.Equal(0, produzido.Madeira);
        Assert.Equal(0, produzido.Pedra);
        Assert.Equal(0, produzido.Ouro);
        Assert.Equal(0, produzido.Erva);
    }

    // ── Teste 3: 48h e 24h produzem o mesmo (cap de 24h) ────────────────────

    [Fact(DisplayName = "Collection after 48h must produce the same as 24h (cap enforced)")]
    [Trait("Category", "CidadeService")]
    public async Task ColetarProducaoAsync_48Horas_ProduzeIgualA24Horas()
    {
        //Arrange
        const ulong usuarioId24 = 3001UL;
        const ulong usuarioId48 = 3002UL;

        var heroiId = Guid.NewGuid();

        var heroi = new Heroi
        {
            Id        = heroiId,
            Nome      = "Herói Cap",
            Raca      = Raca.Humano,
            Profissao = null,
            Humor     = 50
        };

        // City with UltimaColeta 24h ago
        var trabalhador24 = new PersonagemTrabalhador
        {
            Id            = Guid.NewGuid(),
            HeroiId       = heroiId,
            ResourceNode  = TipoResourceNode.Mina,
            InicioTrabalho = DateTime.UtcNow.AddHours(-24)
        };
        var cidade24 = new Cidade
        {
            Id            = Guid.NewGuid(),
            UsuarioId     = usuarioId24,
            Nome          = "Cidade 24h",
            UltimaColeta  = DateTime.UtcNow.AddHours(-24),
            Recursos      = new Recursos(),
            Construcoes   = new List<Construcao>(),
            Trabalhadores = new List<PersonagemTrabalhador> { trabalhador24 }
        };

        // City with UltimaColeta 48h ago — separate instance to avoid mutation side-effects
        var trabalhador48 = new PersonagemTrabalhador
        {
            Id            = Guid.NewGuid(),
            HeroiId       = heroiId,
            ResourceNode  = TipoResourceNode.Mina,
            InicioTrabalho = DateTime.UtcNow.AddHours(-48)
        };
        var cidade48 = new Cidade
        {
            Id            = Guid.NewGuid(),
            UsuarioId     = usuarioId48,
            Nome          = "Cidade 48h",
            UltimaColeta  = DateTime.UtcNow.AddHours(-48),
            Recursos      = new Recursos(),
            Construcoes   = new List<Construcao>(),
            Trabalhadores = new List<PersonagemTrabalhador> { trabalhador48 }
        };

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId24)).ReturnsAsync(cidade24);
        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId48)).ReturnsAsync(cidade48);
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId24)).ReturnsAsync(new List<Heroi> { heroi });
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId48)).ReturnsAsync(new List<Heroi> { heroi });
        _cidadeRepo.Setup(r => r.AtualizarAsync(It.IsAny<Cidade>())).Returns(Task.CompletedTask);

        _boosterRepo.Setup(r => r.ObterAtivoAsync(usuarioId24)).ReturnsAsync((CidadeBoosterAtivo?)null);
        _boosterRepo.Setup(r => r.ObterAtivoAsync(usuarioId48)).ReturnsAsync((CidadeBoosterAtivo?)null);
        SetupEmptySlots();

        var service = CreateService();

        //Act
        var (_, produzido24) = await service.ColetarProducaoAsync(usuarioId24);
        var (_, produzido48) = await service.ColetarProducaoAsync(usuarioId48);

        //Assert
        // Mina base: 4 pedra/h × 24h = 96; the 48h case must be capped to the same 96
        Assert.Equal(produzido24.Pedra, produzido48.Pedra);
        Assert.True(produzido24.Pedra > 0,
            "Expected Pedra > 0 for 24h case.");
    }

    // ── Teste 4: sem trabalhadores e sem prédios → produz 0 ─────────────────

    [Fact(DisplayName = "No workers and no buildings must produce 0 of every resource")]
    [Trait("Category", "CidadeService")]
    public async Task ColetarProducaoAsync_SemTrabalhadores_ProduzeZero()
    {
        //Arrange
        const ulong usuarioId = 4444UL;

        var cidade = new Cidade
        {
            Id            = Guid.NewGuid(),
            UsuarioId     = usuarioId,
            Nome          = "Cidade Vazia",
            UltimaColeta  = DateTime.UtcNow.AddHours(-8),
            Recursos      = new Recursos(),
            Construcoes   = new List<Construcao>(),
            Trabalhadores = new List<PersonagemTrabalhador>()
        };

        _cidadeRepo.Setup(r => r.ObterPorProprietarioIdAsync(usuarioId)).ReturnsAsync(cidade);
        _heroiRepo.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId)).ReturnsAsync(new List<Heroi>());
        _cidadeRepo.Setup(r => r.AtualizarAsync(cidade)).Returns(Task.CompletedTask);
        SetupNullBooster(usuarioId);
        SetupEmptySlots();

        var service = CreateService();

        //Act
        var (_, produzido) = await service.ColetarProducaoAsync(usuarioId);

        //Assert
        Assert.Equal(0, produzido.Comida);
        Assert.Equal(0, produzido.Madeira);
        Assert.Equal(0, produzido.Pedra);
        Assert.Equal(0, produzido.Ouro);
        Assert.Equal(0, produzido.Erva);
    }
}
