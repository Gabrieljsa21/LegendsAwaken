using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities
{
    public class Habilidade
    {
        public Guid Id { get; set; }              // Identificador único da habilidade
        public required string Nome { get; set; }          // Nome da habilidade
        public string? Descricao { get; set; }     // Descrição da habilidade
        public TipoHabilidade Tipo { get; set; }  // Tipo: ativa ou passiva

        public int Nivel { get; set; }            // Nível atual da habilidade (1-10)
        public int XPAtual { get; set; }          // XP acumulado atual para o nível
        public int XPMaximo { get; set; }         // XP necessário para subir de nível

        public bool EstaEmTreinamento { get; set; }  // Indica se está em treinamento (opcional)

    }

}
