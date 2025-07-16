using System;

namespace LegendsAwaken.Domain
{
    public class Habilidade
    {
        public Guid Id { get; set; }              // Identificador �nico da habilidade
        public string Nome { get; set; }          // Nome da habilidade
        public string Descricao { get; set; }     // Descri��o da habilidade
        public TipoHabilidade Tipo { get; set; }  // Tipo: ativa ou passiva

        public int Nivel { get; set; }            // N�vel atual da habilidade (1-10)
        public int XPAtual { get; set; }          // XP acumulado atual para o n�vel
        public int XPMaximo { get; set; }         // XP necess�rio para subir de n�vel

        public bool EstaEmTreinamento { get; set; }  // Indica se est� em treinamento (opcional)

        public Habilidade()
        {
            Nivel = 1;
            XPAtual = 0;
            XPMaximo = 100; // Valor inicial padr�o
        }
    }
    public enum TipoHabilidade
    {
        Ativa,
        Passiva
    }

}
