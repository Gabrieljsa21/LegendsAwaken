using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services;

public static class TorreOperacaoConfig
{
    public const int DuracaoHoras = 8;

    // Returns (recurso, quantidade base por operacao, icone)
    // Quantidade is FIXED — no risk/efficiency scaling
    public static (string Recurso, int Quantidade, string Icone) ObterProducao(int andar) => andar switch
    {
        >= 76 => ("Núcleo Primordial",   20, "💠"),
        >= 51 => ("Cristal Dimensional", 15, "🔮"),
        >= 26 => ("Fragmento Arcano",    10, "🔵"),
        >= 11 => ("Essência Corrompida",  8, "🟣"),
        >= 6  => ("Gema Rústica",         5, "🟡"),
        _     => ("Ouro",               100, "💰"),
    };

    // Light hero affinity bonus by race/biome — returns multiplier delta (e.g. 0.10 = +10%)
    public static double ObterAfinidade(Heroi heroi, int andar)
    {
        return (andar, heroi.Raca) switch
        {
            (<= 10, Raca.Humano)    => 0.05,
            (<= 25, Raca.Elfo)      => 0.10,
            (<= 25, Raca.Fada)      => 0.05,
            (>= 11 and <= 50, Raca.Anao)  => 0.10,
            (>= 11 and <= 50, Raca.Bestial) => 0.05,
            (>= 51, Raca.Draconato) => 0.10,
            (>= 51, Raca.Elfo)      => 0.05,
            _ => 0.0
        };
    }

    // Base slots — will be driven by building level in the future
    // For now: 2 (base) + 2 per Guilda level found in city
    public static int CalcularMaxSlots(IEnumerable<Construcao> construcoes)
    {
        var guilda = construcoes.FirstOrDefault(c => c.TipoPredio == TipoPredio.Guilda);
        return 2 + (guilda?.Nivel ?? 0) * 2;
    }
}
