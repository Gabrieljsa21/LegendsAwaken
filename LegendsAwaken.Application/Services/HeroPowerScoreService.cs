using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services;

/// <summary>
/// Stateless helper for computing Hero Power Score and Tower CDI.
/// All methods are static — no DI required.
/// Formulas sourced from DESIGN_SISTEMAS.md §2 and §5.2.
/// </summary>
public static class HeroPowerScoreService
{
    // ── Calibration constants ────────────────────────────────────────────────
    // CALIBRAR NO BETA
    private const double P0Ref = 300.0;

    private static readonly IReadOnlyDictionary<Raridade, double> RarityMultiplier =
        new Dictionary<Raridade, double>
        {
            { Raridade.Estrela1, 0.80 },
            { Raridade.Estrela2, 1.00 },
            { Raridade.Estrela3, 1.15 },
            { Raridade.Estrela4, 1.35 },
            { Raridade.Estrela5, 1.60 },
        };

    private static readonly IReadOnlyDictionary<Raca, double> RaceModifier =
        new Dictionary<Raca, double>
        {
            { Raca.Humano,    1.00 },
            { Raca.Bestial,   1.07 },
            { Raca.Anao,      1.05 },
            { Raca.Elfo,      1.03 },
            { Raca.Draconato, 1.06 },
            { Raca.Fada,      1.04 },
        };

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the HeroPowerScore for a single hero.
    /// Formula: (BaseStats * LevelFactor * GrowthFactor * RaceModifier) + GearPower
    /// ClassScaling = 1.0 for now (per spec).
    /// </summary>
    public static double Calcular(Heroi heroi)
    {
        var totais = heroi.ObterAtributosTotais(new AtributosBase());

        double baseStats =
            (totais.Forca        * 1.2) +
            (totais.Agilidade    * 1.0) +
            (totais.Inteligencia * 1.1) +
            (totais.Vitalidade   * 0.9) +
            (totais.Percepcao    * 1.0);

        double levelFactor = 1.0 + Math.Pow(heroi.Nivel, 1.25) / 100.0;

        double rarityMult = RarityMultiplier.TryGetValue(heroi.Raridade, out var rm) ? rm : 1.0;
        double growthFactor = rarityMult; // ClassScaling = 1.0

        double raceMod = RaceModifier.TryGetValue(heroi.Raca, out var rc) ? rc : 1.0;

        double gearPower = heroi.BonusAtributos
            .Where(b => b.Origem == OrigemBonusAtributo.Equipamento)
            .Sum(b => b.Valor * 0.5);

        return (baseStats * levelFactor * growthFactor * raceMod) + gearPower;
    }

    /// <summary>
    /// Sums HeroPowerScore for a party.
    /// </summary>
    public static double CalcularParty(IEnumerable<Heroi> herois)
        => herois.Sum(Calcular);

    /// <summary>
    /// Computes the Tower Content Difficulty Index for a given floor.
    /// TowerCDI(floor) = P0_ref * (1.10)^floor * FloorModifier
    /// </summary>
    public static double CalcularCDI(int andar)
    {
        double floorModifier = andar switch
        {
            <= 20  => 0.80,
            <= 80  => 1.00,
            <= 150 => 1.25,
            _      => 1.50,
        };

        return P0Ref * Math.Pow(1.10, andar) * floorModifier;
    }

    /// <summary>
    /// Computes team power score / CDI ratio.
    /// </summary>
    public static double CalcularRatio(double teamPS, double cdi)
        => cdi > 0 ? teamPS / cdi : 0.0;

    /// <summary>
    /// Estimates win probability capped at 95%.
    /// WinChance = min(0.95, ratio * 0.50)
    /// </summary>
    public static double CalcularWinChance(double ratio)
        => Math.Min(0.95, ratio * 0.50);

    /// <summary>
    /// Returns a human-readable label for the win chance.
    /// </summary>
    public static string DescricaoWinChance(double winChance) => winChance switch
    {
        > 0.80 => "Esmagador",
        > 0.60 => "Favoravel",
        > 0.40 => "Equilibrado",
        > 0.20 => "Dificil",
        _      => "Critico",
    };
}
