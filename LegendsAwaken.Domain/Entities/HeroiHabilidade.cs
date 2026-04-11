using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities
{
    public class HeroiHabilidade
    {
        public Guid HeroiId { get; set; }
        public Heroi Heroi { get; set; } = null!;
        public string HabilidadeId { get; set; } = string.Empty;
        public Habilidade Habilidade { get; set; } = null!;
        public bool EstaEmTreinamento { get; set; }  // Indica se est� em treinamento (opcional)
        public int Nivel { get; set; }            // N�vel atual da habilidade (1-10)
        public int XPAtual { get; set; }          // XP acumulado atual para o n�vel
        public int XPMaximo { get; set; }         // XP necess�rio para subir de n�vel
    }
    public class Habilidade
    {
        public string Id { get; set; } = string.Empty; // Identificador �nico da habilidade
        public required string Nome { get; set; }   // Nome da habilidade
        public string? Descricao { get; set; }      // Descri��o da habilidade
        public int Rank { get; set; }               // raridade
        public TipoHabilidade TipoHabilidade { get; set; }  // Atributo da habilidade (CombatEncounter, Coleta, Craft)
        public Profissao ProfissaoVinculada { get; set; }   // Profiss�o que pode masterizar essa habilidade
        public List<HabilidadeBonusAtributos> HabilidadeBonusAtributos { get; set; } = new(); //Atributo incrementado
    }

    public class HabilidadeBonusAtributos
    {
        public string HabilidadeId { get; set; } = string.Empty;
        public Habilidade Habilidade { get; set; } = null!;
        public Atributo Atributo { get; set; }      //Atributo incrementado
        public BonusTipo BonusTipo { get; set; }
        public int BonusValor { get; set; }
    }
    public enum BonusTipo
    {
        // CombatEncounter
        Atributo,   // Aumenta atributos como For�a, Destreza, etc.

        // Coleta / Craft
        Rendimento, // Aumenta a quantidade obtida
        Velocidade, // Aumenta a velocidade de produ��o
        Qualidade,  // Aumenta a qualidade do item produzido
        ChanceRaro, // Aumenta a chance de obter itens raros
        Economia    // Reduz o custo de produ��o
    }

}
