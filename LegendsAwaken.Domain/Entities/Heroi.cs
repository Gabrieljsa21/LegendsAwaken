using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;

namespace LegendsAwaken.Domain.Entities
{
    public class Heroi
    {
        public Guid Id { get; set; }
        public ulong UsuarioId { get; set; }

        public required string Nome { get; set; }
        public Raridade Raridade { get; set; }
        public required Raca Raca { get; set; }
        public Profissao? Profissao { get; set; }
        public string? Antecedente { get; set; }

        public int Nivel { get; set; } = 1;
        public int XP { get; set; } = 0;

        // Atributos
        public AtributosBase AtributosBase { get; set; } = new(); // Determinados pela raça + raridade
        public AtributosBase AtributosDistribuidos { get; set; } = new(); // Pontos que o jogador alocou
        public int PontosAtributosDisponiveis { get; set; } = 0; // Pontos não alocados

        public List<HeroiBonusAtributo> BonusAtributos { get; set; } = new(); // Profissão, antecedentes, talentos etc.

        // Combate e progressão
        public StatusCombate Status { get; set; } = new();
        public List<HeroiHabilidade> Habilidades { get; set; } = new();
        public Equipamentos Equipamentos { get; set; } = new();
        public Treinamento? Treinamento { get; set; }
        public FuncaoTatica? Funcao { get; set; }

        // Metadados
        public bool EstaAtivo { get; set; } = true;
        public string? ImagemUrl { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;

        // Histórico
        public int Vitorias { get; set; }
        public int Derrotas { get; set; }
        public int AndaresConquistados { get; set; }
        public int Lealdade { get; set; }

        // Narrativa
        public string? Historia { get; set; }
        public string? Personalidade { get; set; }

        // Extras
        public List<HeroiAfinidadeElemental> AfinidadeElemental { get; set; } = new();
        public List<HeroiVinculo> VinculosHeroicos { get; set; } = new();
        public List<HeroiTag> Tags { get; set; } = new();

        // Método para calcular os atributos finais
        public AtributosBase ObterAtributosTotais(AtributosBase bonusExterno)
        {
            var totalBonus = new AtributosBase();

            // Bônus fixos (profissão, antecedentes, talentos etc)
            foreach (var bonus in BonusAtributos)
                totalBonus.AdicionarPorTipo(bonus.Atributo, bonus.Valor);

            // Bônus vindos das habilidades, multiplicando pelo nível da habilidade
            foreach (var heroHabilidade in Habilidades)
            {
                foreach (var bonus in heroHabilidade.Habilidade.HabilidadeBonusAtributos)
                {
                    int bonusTotal = bonus.BonusValor * heroHabilidade.Nivel;
                    totalBonus.AdicionarPorTipo(bonus.Atributo, bonusTotal);
                }
            }

            return AtributosBase + AtributosDistribuidos + totalBonus + bonusExterno;
        }


        // Realocação de pontos (caso liberado)
        public void RealocarAtributos(AtributosBase novos)
        {
            AtributosDistribuidos = novos;
        }
    }
}
