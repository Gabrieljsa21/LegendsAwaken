using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services;

public record SkillRollContext(
    AdvantageType Advantage = AdvantageType.Normal,
    int FlatBonus = 0,
    int? AutoSuccessThreshold = null,
    bool CritEnabled = false
);

public record TestePericiaEvento(
    string Descricao,
    Pericia PericiaExigida,
    int DC,
    bool EhGrupo,
    string RecompensaSucesso,
    string PenalidadeFalha,
    SkillRollContext? RollContext = null
);

public static class SkillCheckService
{
    public static readonly AtributosBase EmptyBonus = new();

    public static (bool Success, int Total) Rolar(
        Heroi heroi,
        Pericia pericia,
        int dc,
        IEnumerable<HeroiPericia> pericias,
        SkillRollContext context)
    {
        int bonus  = ObterBonusSkill(heroi, pericia, pericias) + context.FlatBonus;
        int roll   = Rolar2d10(context.Advantage);
        int total  = roll + bonus;
        return (total >= dc, total);
    }

    public static (bool Success, int Total) RolarGrupo(
        IEnumerable<Heroi> herois,
        Pericia pericia,
        int dc,
        IEnumerable<HeroiPericia> todasPericias,
        SkillRollContext context)
    {
        var heroisList = herois.ToList();
        if (heroisList.Count == 0) return (false, 0);

        var pericias = todasPericias.ToList();
        var boni = heroisList
            .Select(h => ObterBonusSkill(h, pericia, pericias))
            .OrderByDescending(x => x)
            .ToList();

        // Aggregate uses top-3 contributors; additional heroes are intentionally excluded.
        double scoreAgregado = boni.Count switch
        {
            1 => boni[0],
            2 => boni[0] * 0.6 + boni[1] * 0.3,
            _ => boni[0] * 0.6 + boni[1] * 0.3 + boni[2] * 0.1
        };

        int roll  = Rolar2d10(context.Advantage);
        int total = roll + (int)Math.Round(scoreAgregado) + context.FlatBonus;
        return (total >= dc, total);
    }

    public static Atributo AtributoDePericia(Pericia pericia) => pericia switch
    {
        Pericia.Atletismo        => Atributo.Forca,
        Pericia.Acrobacia        => Atributo.Destreza,
        Pericia.Prestidigitacao  => Atributo.Destreza,
        Pericia.Furtividade      => Atributo.Destreza,
        Pericia.Arcanismo        => Atributo.Inteligencia,
        Pericia.Historia         => Atributo.Inteligencia,
        Pericia.Investigacao     => Atributo.Inteligencia,
        Pericia.Natureza         => Atributo.Inteligencia,
        Pericia.Religiao         => Atributo.Inteligencia,
        Pericia.AdestrarAnimais  => Atributo.Sabedoria,
        Pericia.Intuicao         => Atributo.Sabedoria,
        Pericia.Medicina         => Atributo.Sabedoria,
        Pericia.Percepcao        => Atributo.Sabedoria,
        Pericia.Sobrevivencia    => Atributo.Sabedoria,
        Pericia.Enganacao        => Atributo.Carisma,
        Pericia.Intimidacao      => Atributo.Carisma,
        Pericia.Atuacao          => Atributo.Carisma,
        Pericia.Persuasao        => Atributo.Carisma,
        _                        => Atributo.Sabedoria
    };

    public static int BonusProficiencia(int nivel) => nivel switch
    {
        <= 4  => 2,
        <= 8  => 3,
        <= 12 => 4,
        <= 16 => 5,
        _     => 6
    };

    private static int ObterBonusSkill(Heroi heroi, Pericia pericia, IEnumerable<HeroiPericia> pericias)
    {
        var atributo = AtributoDePericia(pericia);
        var totais   = heroi.ObterAtributosTotais(EmptyBonus);
        int mod      = (int)Math.Floor((totais.Get(atributo) - 10.0) / 2.0);

        var hp       = pericias.FirstOrDefault(p => p.HeroiId == heroi.Id && p.Pericia == pericia);
        int profBonus = hp?.TemProficiencia == true ? BonusProficiencia(heroi.Nivel) : 0;

        return mod + profBonus;
    }

    private static int Rolar2d10(AdvantageType advantage)
    {
        static int Roll() => Random.Shared.Next(1, 11) + Random.Shared.Next(1, 11);

        return advantage switch
        {
            AdvantageType.Advantage    => Math.Max(Roll(), Roll()),
            AdvantageType.Disadvantage => Math.Min(Roll(), Roll()),
            _                          => Roll()
        };
    }
}
