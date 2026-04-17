using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
using System;
using System.Collections.Generic;

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
            AtributosBase atributosBase,
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
                AtributosBase = atributosBase,
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
    }
}
