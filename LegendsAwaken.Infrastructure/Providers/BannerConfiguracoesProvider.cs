using LegendsAwaken.Bot.Models.Banner;
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Infrastructure.Providers
{
    public static class BannerConfiguracoesProvider
    {        
        public static BannerConfiguracao BannerPadrao => new BannerConfiguracao
        {
            Id = "banner_padrao",
            Nome = "Banner Padrão",
            RacaPorRaridade = new Dictionary<Raridade, Dictionary<Raca, int>>
            {
                {
                    Raridade.Estrela1, new Dictionary<Raca, int>
                    {
                        { Raca.Humano, 100 },
                        { Raca.Bestial, 0 },
                        { Raca.Anao, 0 },
                        { Raca.Elfo, 0 },
                        { Raca.Draconato, 0 },
                        { Raca.Fada, 0 }
                    }
                },
                {
                    Raridade.Estrela2, new Dictionary<Raca, int>
                    {
                        { Raca.Humano, 100 },
                        { Raca.Bestial, 0 },
                        { Raca.Anao, 0 },
                        { Raca.Elfo, 0 },
                        { Raca.Draconato, 0 },
                        { Raca.Fada, 0 }
                    }
                },
                {
                    Raridade.Estrela3, new Dictionary<Raca, int>
                    {
                        { Raca.Humano, 90 },
                        { Raca.Bestial, 2 },
                        { Raca.Anao, 2 },
                        { Raca.Elfo, 2 },
                        { Raca.Draconato, 2 },
                        { Raca.Fada, 2 }
                    }
                },
                {
                    Raridade.Estrela4, new Dictionary<Raca, int>
                    {
                        { Raca.Humano, 85 },
                        { Raca.Bestial, 3 },
                        { Raca.Anao, 3 },
                        { Raca.Elfo, 3 },
                        { Raca.Draconato, 3 },
                        { Raca.Fada, 3 }
                    }
                },
                {
                    Raridade.Estrela5, new Dictionary<Raca, int>
                    {
                        { Raca.Humano, 75 },
                        { Raca.Bestial, 5 },
                        { Raca.Anao, 5 },
                        { Raca.Elfo, 5 },
                        { Raca.Draconato, 5 },
                        { Raca.Fada, 5 }
                    }
                }
            }
        };
    }
}
