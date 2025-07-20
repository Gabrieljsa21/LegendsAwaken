using System;
using System.Collections.Generic;

namespace LegendsAwaken.Domain.Entities
{
    public class Cidade
    {
        public Guid Id { get; set; }
        public ulong UsuarioId { get; set; } // ID do jogador (usuário do Discord)

        public required string Nome { get; set; }
        public int Nivel { get; set; } = 1;

        public int Populacao { get; set; } = 0;
        public int CapacidadeMaxima { get; set; } = 10;

        public Recursos Recursos { get; set; } = new();
        public List<Construcao> Construcoes { get; set; } = new();
        public List<PersonagemTrabalhador> Trabalhadores { get; set; } = new();

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
    }

    public class Recursos
    {
        public int Comida { get; set; } = 0;
        public int Madeira { get; set; } = 0;
        public int Pedra { get; set; } = 0;
        public int Ouro { get; set; } = 0;

        public void Adicionar(int quantidade, string tipo)
        {
            switch (tipo.ToLower())
            {
                case "ouro":
                    Ouro += quantidade;
                    break;
                case "madeira":
                    Madeira += quantidade;
                    break;
                case "pedra":
                    Pedra += quantidade;
                    break;
            }
        }
    }

    public class Construcao
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Ex: "ferreiro", "fazenda"
        public required string Nome { get; set; }
        public int Nivel { get; set; } = 1;
        public bool EstaAtiva { get; set; } = true;
    }

    public class PersonagemTrabalhador
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HeroiId { get; set; }
        public required string Profissao { get; set; } // Ex: "ferreiro", "agricultor"
        public DateTime InicioTrabalho { get; set; }
        public DateTime? TerminoTrabalho { get; set; } // pode ser null se for contínuo
    }
}
