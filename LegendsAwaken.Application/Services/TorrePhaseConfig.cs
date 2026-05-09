namespace LegendsAwaken.Application.Services;

/// <summary>
/// Per-phase configuration for Tower balance.
/// Each phase is tuned independently — adjust CDI or progress knobs here
/// without touching the core formulas in HeroPowerScoreService or TorreExploracaoService.
/// </summary>
public static class TorrePhaseConfig
{
    // ── CDI phases ────────────────────────────────────────────────────────────
    // CDI is interpolated exponentially from CDIInicio (at AndarMin) to CDIFim (at AndarMax).
    // Tune only the boundary values to reshape difficulty ramp per phase.

    public record CDIFase(int AndarMin, int AndarMax, double CDIInicio, double CDIFim);

    public static readonly IReadOnlyList<CDIFase> CDIFases =
    [
        new(  1,  10,      105,     210),   // Onboarding: 75%→50% WinChance for starter teams
        new( 11,  50,      220,   2_000),   // Mid-game: steady ramp
        new( 51, 150,    2_200,  80_000),   // Late-game
        new(151, 999,   90_000, 3_000_000), // Endgame
    ];

    // ── Progress/failure phases ───────────────────────────────────────────────
    // Decouples progression speed and failure risk from each other and from CDI.
    //
    // Progress formula:  min(TaxaBase + RatioMult * max(0, ratio - 0.5), Cap)  [%/min]
    // Failure formula:   FalhaMultiplicador * (1 - ratio)  [chance/min when ratio < 1]
    //                    capped at FalhaCapPorTick per tick

    public record ProgressoFase(
        int AndarMin,
        int AndarMax,
        double TaxaBase,           // guaranteed %/min regardless of ratio
        double RatioMult,          // bonus %/min per unit of ratio above 0.5
        double Cap,                // max %/min
        double FalhaMultiplicador, // fail chance per minute when ratio < 1
        double FalhaCapPorTick);   // max fail chance per single tick

    public static readonly IReadOnlyList<ProgressoFase> ProgressoFases =
    [
        //          min   max  base  mult  cap   failMult  failCap
        new(  1,  10,  5.0,  0.5,  6.0,  0.010,  0.05),  // Onboarding: ~20min, near-zero risk
        new( 11,  50,  1.5,  1.5,  4.0,  0.030,  0.35),  // Mid-game: moderate
        new( 51, 150,  0.5,  1.5,  3.0,  0.050,  0.80),  // Late-game: current behavior
        new(151, 999,  0.3,  1.2,  2.5,  0.070,  0.90),  // Endgame: punishing
    ];

    // ── Lookup helpers ────────────────────────────────────────────────────────

    public static CDIFase ObterCDIFase(int andar)
    {
        foreach (var f in CDIFases)
            if (andar >= f.AndarMin && andar <= f.AndarMax) return f;
        return CDIFases[CDIFases.Count - 1];
    }

    public static ProgressoFase ObterProgressoFase(int andar)
    {
        foreach (var f in ProgressoFases)
            if (andar >= f.AndarMin && andar <= f.AndarMax) return f;
        return ProgressoFases[ProgressoFases.Count - 1];
    }

    public static double InterpolaCDI(CDIFase fase, int andar)
    {
        int range = fase.AndarMax - fase.AndarMin;
        if (range <= 0) return fase.CDIInicio;
        double t = (double)(andar - fase.AndarMin) / range;
        return fase.CDIInicio * Math.Pow(fase.CDIFim / fase.CDIInicio, t);
    }
}
