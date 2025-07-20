using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Bot.Models.Banner;
using System;
using System.Collections.Generic;
using System.Linq;
using LegendsAwaken.Domain.Entities.Banner;

namespace LegendsAwaken.Domain.Factories
{
    public static class HeroiFactory
    {
        private static readonly Random _random = new();

        public static Heroi CriarHeroi(
            ulong usuarioId,
            string nome,
            Raridade raridade,
            string raca,
            string classe,
            string antecedente,
            List<HeroiAfinidadeElemental> afinidade,
            FuncaoTatica? funcao = null
        )
        {

            var heroi = new Heroi
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Nome = nome,
                Raridade = raridade,
                Raca = raca.ToString().ToLower(),
                Profissao = classe,
                Antecedente = antecedente,
                Nivel = 1,
                XP = 0,
                Atributos = GerarAtributosIniciais(raridade),
                Status = new StatusCombate
                {
                    VidaMaxima = 100,
                    VidaAtual = 100,
                    ManaMaxima = 50,
                    ManaAtual = 50
                },
                Habilidades = new List<Habilidade>(),
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

            return heroi;
        }

        private static Atributos GerarAtributosIniciais(Raridade raridade)
        {
            return raridade switch
            {
                Raridade.Estrela1 => new Atributos { Forca = 5, Destreza = 5, Inteligencia = 5, Constituicao = 5, Sabedoria = 5, Carisma = 5 },
                Raridade.Estrela2 => new Atributos { Forca = 7, Destreza = 7, Inteligencia = 7, Constituicao = 7, Sabedoria = 7, Carisma = 7 },
                Raridade.Estrela3 => new Atributos { Forca = 10, Destreza = 10, Inteligencia = 10, Constituicao = 10, Sabedoria = 10, Carisma = 10 },
                Raridade.Estrela4 => new Atributos { Forca = 13, Destreza = 13, Inteligencia = 13, Constituicao = 13, Sabedoria = 13, Carisma = 13 },
                Raridade.Estrela5 => new Atributos { Forca = 16, Destreza = 16, Inteligencia = 16, Constituicao = 16, Sabedoria = 16, Carisma = 16 },
                _ => new Atributos()
            };
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
