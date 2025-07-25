using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities
{
    public class HeroiHabilidade
    {
        public Guid HeroiId { get; set; }
        public Heroi Heroi { get; set; }
        public string HabilidadeId { get; set; }
        public Habilidade Habilidade { get; set; }
        public bool EstaEmTreinamento { get; set; }  // Indica se está em treinamento (opcional)
        public int Nivel { get; set; }            // Nível atual da habilidade (1-10)
        public int XPAtual { get; set; }          // XP acumulado atual para o nível
        public int XPMaximo { get; set; }         // XP necessário para subir de nível
    }
    public class Habilidade
    {
        public string Id { get; set; }              // Identificador único da habilidade
        public required string Nome { get; set; }   // Nome da habilidade
        public string? Descricao { get; set; }      // Descrição da habilidade
        public int Rank { get; set; }               // raridade
        public TipoHabilidade TipoHabilidade { get; set; }  // Atributo da habilidade (CombatEncounter, Coleta, Craft)
        public Profissao ProfissaoVinculada { get; set; }   // Profissão que pode masterizar essa habilidade
        public List<HabilidadeBonusAtributos> HabilidadeBonusAtributos { get; set; } //Atributo incrementado
    }

    public class HabilidadeBonusAtributos
    {
        public string HabilidadeId { get; set; }
        public Habilidade Habilidade { get; set; }
        public Atributo Atributo { get; set; }      //Atributo incrementado
        public BonusTipo BonusTipo { get; set; }
        public int BonusValor { get; set; }
    }
    public enum BonusTipo
    {
        // CombatEncounter
        Atributo,   // Aumenta atributos como Força, Destreza, etc.

        // Coleta / Craft
        Rendimento, // Aumenta a quantidade obtida
        Velocidade, // Aumenta a velocidade de produção
        Qualidade,  // Aumenta a qualidade do item produzido
        ChanceRaro, // Aumenta a chance de obter itens raros
        Economia    // Reduz o custo de produção
    }

}
