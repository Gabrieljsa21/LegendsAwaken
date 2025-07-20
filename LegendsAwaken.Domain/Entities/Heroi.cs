using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LegendsAwaken.Domain.Entities
{
    public class Heroi
    {
        public Guid Id { get; set; }
        public ulong UsuarioId { get; set; }

        public required string Nome { get; set; }
        public Raridade Raridade { get; set; }
        public required string Raca { get; set; }
        public string? Profissao { get; set; }
        public string? Antecedente { get; set; }

        public int Nivel { get; set; } = 1;
        public int XP { get; set; } = 0;

        public Atributos Atributos { get; set; } = new();
        public StatusCombate Status { get; set; } = new();
        public List<Habilidade> Habilidades { get; set; } = new();
        public Equipamentos Equipamentos { get; set; } = new();
        public Treinamento? Treinamento { get; set; }

        public FuncaoTatica? Funcao { get; set; }

        public bool EstaAtivo { get; set; } = true;

        public string? ImagemUrl { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
        public int Vitorias { get; set; }
        public int Derrotas { get; set; }
        public int AndaresConquistados { get; set; }
        public int Lealdade { get; set; }
        public string? Historia { get; set; }
        public string? Personalidade { get; set; }

        public List<HeroiAfinidadeElemental> AfinidadeElemental { get; set; } = new();
        public List<HeroiVinculo> VinculosHeroicos { get; set; } = new();
        public List<HeroiTag> Tags { get; set; } = new();
    }


}
