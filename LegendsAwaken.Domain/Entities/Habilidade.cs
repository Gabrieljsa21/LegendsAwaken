using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities
{
    public class Habilidade
    {
        public Guid Id { get; set; }              // Identificador �nico da habilidade
        public required string Nome { get; set; }          // Nome da habilidade
        public string? Descricao { get; set; }     // Descri��o da habilidade
        public TipoHabilidade Tipo { get; set; }  // Tipo: ativa ou passiva

        public int Nivel { get; set; }            // N�vel atual da habilidade (1-10)
        public int XPAtual { get; set; }          // XP acumulado atual para o n�vel
        public int XPMaximo { get; set; }         // XP necess�rio para subir de n�vel

        public bool EstaEmTreinamento { get; set; }  // Indica se est� em treinamento (opcional)

    }

}
