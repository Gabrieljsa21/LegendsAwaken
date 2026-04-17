using LegendsAwaken.Bot.Models.Banner;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Infrastructure.Providers
{
    public static class BannerConfiguracoesProvider
    {
        public static BannerConfiguracao BannerPadrao => new BannerConfiguracao
        {
            Id   = "banner_padrao",
            Nome = "Banner Padrão",

            // Raridade → { Raca → peso }.
            // Non-human weight = (100 - chanceHumano) / count(non-human).
            // Adding a new Raca to the enum automatically distributes weight here.
            RacaPorRaridade = new Dictionary<Raridade, Dictionary<Raca, int>>
            {
                { Raridade.Estrela1, TabelaRacas(chanceHumano: 100) },
                { Raridade.Estrela2, TabelaRacas(chanceHumano: 100) },
                { Raridade.Estrela3, TabelaRacas(chanceHumano:  90) },
                { Raridade.Estrela4, TabelaRacas(chanceHumano:  85) },
                { Raridade.Estrela5, TabelaRacas(chanceHumano:  75) },
            }
        };

        /// <summary>
        /// Builds a race-weight table for a given raridade.
        /// Humano receives <paramref name="chanceHumano"/>; all non-human races
        /// split the remainder evenly. Works for any number of races in the enum.
        /// </summary>
        private static Dictionary<Raca, int> TabelaRacas(int chanceHumano)
        {
            var naoHumanas = Enum.GetValues<Raca>().Where(r => r != Raca.Humano).ToList();
            int chanceNaoHumano = naoHumanas.Count > 0
                ? (100 - chanceHumano) / naoHumanas.Count
                : 0;

            return Enum.GetValues<Raca>()
                .ToDictionary(r => r, r => r == Raca.Humano ? chanceHumano : chanceNaoHumano);
        }
    }
}
