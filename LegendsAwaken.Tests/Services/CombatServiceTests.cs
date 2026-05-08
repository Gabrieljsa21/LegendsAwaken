using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Combate;

namespace LegendsAwaken.Tests.Services;

public class CombatServiceTests
{
    private readonly CombatService _service = new();

    // ── Helper ────────────────────────────────────────────────────────────────

    private static Combatente CriarCombatente(
        int forca,
        int vitalidade,
        int vidaMaxima,
        int nivel      = 1,
        int percepcao  = 0,
        int agilidade  = 0,
        int? vidaAtual = null,
        bool isHeroi   = false)
    {
        return new Combatente
        {
            Id        = Guid.NewGuid(),
            Nome      = "Test",
            Nivel     = nivel,
            Atributos = new AtributosBase
            {
                Forca        = forca,
                Constituicao = vitalidade,
                Sabedoria    = percepcao,
                Destreza     = agilidade
            },
            Status = new StatusCombate
            {
                VidaMaxima = vidaMaxima,
                VidaAtual  = vidaAtual ?? vidaMaxima
            },
            IsHeroi = isHeroi
        };
    }

    // ── Teste 1: Sem defesa → dano == ATK ────────────────────────────────────

    [Fact(DisplayName = "Sem defesa, dano base deve ser igual a ATK")]
    [Trait("Category", "CombatService")]
    public void CalcularDano_SemDefesa_DanoIgualAAtk()
    {
        //Arrange
        // DEF.Vitalidade=0 → mitigacao=0 → danoBase=200; burstCap=int(2000*0.65)=1300
        // Percepcao=0 → critChance=5%; sem crit: 200, com crit: int(200*1.5)=300
        var atk = CriarCombatente(forca: 200, vitalidade: 0, vidaMaxima: 100,  percepcao: 0);
        var def = CriarCombatente(forca: 0,   vitalidade: 0, vidaMaxima: 2000, percepcao: 0);

        //Act
        int dano = _service.CalcularDano(atk, def, skillMult: 1.0);

        //Assert
        Assert.InRange(dano, 200, 300);
    }

    // ── Teste 2: Alta defesa reduz dano ──────────────────────────────────────

    [Fact(DisplayName = "Alta defesa reduz dano em 50%")]
    [Trait("Category", "CombatService")]
    public void CalcularDano_AltaDefesa_DanoReduzido50Porcento()
    {
        //Arrange
        // K = 1000 + 1*50 = 1050; mitigacao = 1050/(1050+1050) = 0.5
        // danoBase = 200 * 0.5 = 100; burstCap = int(2000*0.65) = 1300
        // Percepcao=0 → critChance=5%; sem crit: 100, com crit: 150
        var atk = CriarCombatente(forca: 200, vitalidade: 0,    vidaMaxima: 100,  nivel: 1, percepcao: 0);
        var def = CriarCombatente(forca: 0,   vitalidade: 1050, vidaMaxima: 2000, nivel: 1, percepcao: 0);

        //Act
        int dano = _service.CalcularDano(atk, def, skillMult: 1.0);

        //Assert
        Assert.InRange(dano, 100, 150);
    }

    // ── Teste 3: BurstCap é respeitado ───────────────────────────────────────

    [Fact(DisplayName = "Burst cap limita dano a 65% da vida maxima do alvo")]
    [Trait("Category", "CombatService")]
    public void CalcularDano_BurstCap_DanoLimitadoA65PorcentoDaVida()
    {
        //Arrange
        // ATK.Forca=10000, DEF.Vitalidade=0 → danoBase=10000 (sem crit) ou 15000 (com crit)
        // burstCap = int(100 * 0.65) = 65
        // Ambos os caminhos excedem o cap → resultado sempre == 65 (determinístico)
        var atk = CriarCombatente(forca: 10000, vitalidade: 0, vidaMaxima: 100);
        var def = CriarCombatente(forca: 0,     vitalidade: 0, vidaMaxima: 100);

        //Act
        int dano = _service.CalcularDano(atk, def, skillMult: 1.0);

        //Assert
        Assert.Equal(65, dano);
    }

    // ── Teste 4: skillMult escala linearmente ────────────────────────────────

    [Fact(DisplayName = "skillMult 2.0 dobra o dano base")]
    [Trait("Category", "CombatService")]
    public void CalcularDano_SkillMultDois_DobraDanoBase()
    {
        //Arrange
        // ATK.Forca=100, DEF.Vitalidade=0 → danoBase = 100 * 2.0 = 200
        // burstCap = int(2000 * 0.65) = 1300 — cap não é atingido
        // Percepcao=0 → critChance=5%; sem crit: 200, com crit: 300
        var atk = CriarCombatente(forca: 100, vitalidade: 0, vidaMaxima: 100,  percepcao: 0);
        var def = CriarCombatente(forca: 0,   vitalidade: 0, vidaMaxima: 2000, percepcao: 0);

        //Act
        int dano = _service.CalcularDano(atk, def, skillMult: 2.0);

        //Assert
        Assert.InRange(dano, 200, 300);
    }

    // ── Teste 5: Mínimo 1 ────────────────────────────────────────────────────

    [Fact(DisplayName = "Dano nunca e inferior a 1 mesmo com Forca zero")]
    [Trait("Category", "CombatService")]
    public void CalcularDano_ForcaZero_DanoMinimoDe1()
    {
        //Arrange
        // ATK.Forca=0 → danoBase = 0; crit de 0 continua 0
        // Math.Clamp(0, 1, burstCap) = 1 — determinístico independente de crit
        var atk = CriarCombatente(forca: 0, vitalidade: 0, vidaMaxima: 100,  percepcao: 0);
        var def = CriarCombatente(forca: 0, vitalidade: 0, vidaMaxima: 1000, percepcao: 0);

        //Act
        int dano = _service.CalcularDano(atk, def, skillMult: 1.0);

        //Assert
        Assert.Equal(1, dano);
    }

    // ── Teste 6: ExecutarRound mata defensor ─────────────────────────────────

    [Fact(DisplayName = "ExecutarRound com atacante forte finaliza o encontro em um round")]
    [Trait("Category", "CombatService")]
    public void ExecutarRound_AtacanteForte_FinalizaEncontro()
    {
        //Arrange
        // Inimigo: VidaMaxima=2, VidaAtual=1
        //   burstCap = int(2 * 0.65) = 1
        //   CalcularDano retorna Math.Clamp(x, 1, 1) = 1 para qualquer Forca>0
        //   Após 1 hit: VidaAtual = max(0, 1-1) = 0 → inimigo morto → IsFinished=true
        // IsHeroi=true no atk para que ele mire enc.Inimigos (regra de targeting do serviço)
        var atk = CriarCombatente(forca: 5000, vitalidade: 0, vidaMaxima: 9999, agilidade: 100, isHeroi: true);
        var def = CriarCombatente(forca: 0,    vitalidade: 0, vidaMaxima: 2,    vidaAtual: 1);

        var enc = new CombatEncounter
        {
            Aliados  = new List<Combatente> { atk },
            Inimigos = new List<Combatente> { def }
        };

        //Act
        _service.ExecutarRound(enc);

        //Assert
        Assert.True(enc.IsFinished);
        Assert.Equal(0, def.Status.VidaAtual);
    }
}
