using LegendsAwaken.Bot.Models.Banner;
using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Entities.Banner;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Domain.Factories
{
    public static class HeroiFactory
    {
        private static readonly Random _random = new();

        public static Heroi CriarHeroi(
            ulong usuarioId,
            string nome,
            Raridade raridade,
            Raca raca,
            string antecedente,
            List<HeroiAfinidadeElemental> afinidade,
            List<HeroiHabilidade> habilidades,
            FuncaoTatica? funcao = null
        )
        {

            var heroi = new Heroi
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Nome = nome,
                Raridade = raridade,
                Raca = raca,
                Antecedente = antecedente,
                Nivel = 1,
                XP = 0,
                AtributosBase = GerarAtributosIniciais(raridade, raca),
                Habilidades = habilidades,
                Equipamentos = new Equipamentos(),
                Tags = new List<HeroiTag>(),
                AfinidadeElemental = afinidade,
                VinculosHeroicos = new List<HeroiVinculo>(),
                Funcao = funcao,
                EstaAtivo = true,
                DataCriacao = DateTime.UtcNow,
                DataAlteracao = DateTime.UtcNow,
                Lealdade = 0,
                Historia = null,
                Personalidade = "Neutro"
            };

            heroi.Status = heroi.AtributosBase.FromAtributos();

            return heroi;
        }

        private static AtributosBase GerarAtributosIniciais(Raridade raridade, Raca raca)
        {
            int totalPontos = raridade switch
            {
                Raridade.Estrela1 => 20,
                Raridade.Estrela2 => 40,
                Raridade.Estrela3 => 60,
                Raridade.Estrela4 => 80,
                Raridade.Estrela5 => 100,
                _ => 20
            };

            int numAtributos = 5;
            int pontosRestantes = totalPontos - numAtributos;

            var random = new Random();
            int[] distribuicao = new int[numAtributos];

            // Inicializa cada Atributo com 1 ponto
            for (int i = 0; i < numAtributos; i++)
                distribuicao[i] = 1;

            // Distribui os pontos restantes aleatoriamente
            for (int i = 0; i < pontosRestantes; i++)
            {
                int index = random.Next(numAtributos);
                distribuicao[index]++;
            }

            var atributos = new AtributosBase
            {
                Forca = distribuicao[0],
                Agilidade = distribuicao[1],
                Vitalidade = distribuicao[2],
                Inteligencia = distribuicao[3],
                Percepcao = distribuicao[4]
            };

            // Aplica bônus racial (exceto humano)
            if (raca != Raca.Humano)
            {
                int bonus = raridade switch
                {
                    Raridade.Estrela1 => 10,
                    Raridade.Estrela2 => 20,
                    Raridade.Estrela3 => 30,
                    Raridade.Estrela4 => 40,
                    Raridade.Estrela5 => 50,
                    _ => 0
                };

                // Monta dicionário para identificar maior Atributo
                var atributosDict = new Dictionary<string, int>
        {
            { nameof(atributos.Forca), atributos.Forca },
            { nameof(atributos.Agilidade), atributos.Agilidade },
            { nameof(atributos.Vitalidade), atributos.Vitalidade },
            { nameof(atributos.Inteligencia), atributos.Inteligencia },
            { nameof(atributos.Percepcao), atributos.Percepcao }
        };

                var maiorAtributo = atributosDict.Aggregate((l, r) => l.Value >= r.Value ? l : r).Key;

                switch (maiorAtributo)
                {
                    case nameof(AtributosBase.Forca):
                        atributos.Forca += bonus;
                        break;
                    case nameof(AtributosBase.Agilidade):
                        atributos.Agilidade += bonus;
                        break;
                    case nameof(AtributosBase.Vitalidade):
                        atributos.Vitalidade += bonus;
                        break;
                    case nameof(AtributosBase.Inteligencia):
                        atributos.Inteligencia += bonus;
                        break;
                    case nameof(AtributosBase.Percepcao):
                        atributos.Percepcao += bonus;
                        break;
                }
            }

            return atributos;
        }



        private static Raca SortearRaca(Raridade raridade, Dictionary<Raridade, List<RacaChance>> racaPorRaridade)
        {
            if (!racaPorRaridade.TryGetValue(raridade, out var racasDisponiveis))
                throw new Exception($"Nenhuma raça configurada para a raridade {raridade}");

            int total = racasDisponiveis.Sum(r => r.Chance);
            int rolagem = _random.Next(1, total + 1);
            int acumulado = 0;

            foreach (var racaChance in racasDisponiveis)
            {
                acumulado += racaChance.Chance;
                if (rolagem <= acumulado)
                    return racaChance.Raca;
            }

            // Fallback de segurança
            return racasDisponiveis.First().Raca;
        }
    }
}
