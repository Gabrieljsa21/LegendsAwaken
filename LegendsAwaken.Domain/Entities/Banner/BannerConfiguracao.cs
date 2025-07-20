using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Bot.Models.Banner
{
    public class BannerConfiguracao
    {
        public string Id { get; set; } = "banner_padrao";
        public string Nome { get; set; } = "Banner Padrão";
        public int PityMaximo { get; set; } = 100;

        public Dictionary<Raridade, int> RaridadeChances { get; set; } = new()
        {
            { Raridade.Estrela1, 70 },
            { Raridade.Estrela2, 24 },
            { Raridade.Estrela3, 5 },
            { Raridade.Estrela4, 1 },
            { Raridade.Estrela5, 0 },
        };

        public Dictionary<Raridade, Dictionary<Raca, int>> RacaPorRaridade { get; set; } = new();
        public Dictionary<Raridade, Dictionary<string, int>> ProfissaoPorRaridade { get; set; } = new();

        public string MostrarChances()
        {
            var linhas = new List<string>();
            foreach (var raridade in RaridadeChances.Keys.OrderBy(r => r))
            {
                linhas.Add($"★ {raridade}: {RaridadeChances[raridade]}%");
                if (RacaPorRaridade.TryGetValue(raridade, out var racas))
                {
                    foreach (var raca in racas.OrderBy(r => r.Key.ToString()))
                    {
                        linhas.Add($" - {raca.Key}: {raca.Value}%");
                    }
                }
            }

            return string.Join("\n", linhas);
        }
    }
}
