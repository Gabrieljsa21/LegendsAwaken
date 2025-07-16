using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;

namespace LegendsAwaken.Domain
{
    public class Heroi
    {
        public Guid Id { get; set; }
        public ulong UsuarioId { get; set; } // ID do jogador (usuário do Discord)

        public string Nome { get; set; }
        public Raridade Raridade { get; set; } // 1 a 5 estrelas
        public string Raca { get; set; }
        public string Classe { get; set; } // Pode ser null para 1★/2★
        public string Antecedente { get; set; }

        public int Nivel { get; set; } = 1;
        public int XP { get; set; } = 0;

        public Atributos Atributos { get; set; } = new();
        public Status Status { get; set; } = new();
        public HabilidadeContainer Habilidades { get; set; } = new();
        public Equipamentos Equipamentos { get; set; } = new();
        public Treinamento? Treinamento { get; set; }

        public List<string> Tags { get; set; } = new(); // Ex: "Mago", "Suporte"

        public DateTime DataInvocacao { get; set; } = DateTime.UtcNow;
        public string? ImagemUrl { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;


    }
    public class Status
    {
        public int VidaAtual { get; set; }
        public int VidaMaxima { get; set; }
        public int ManaAtual { get; set; }
        public int ManaMaxima { get; set; }
    }

    public class Habilidades
    {
        public List<HabilidadeAtiva> Ativas { get; set; } = new();
        public List<HabilidadePassiva> Passivas { get; set; } = new();
    }

    public class HabilidadeAtiva
    {
        public string Nome { get; set; }
        public int Nivel { get; set; } = 1;
        public int XPAtual { get; set; } = 0;
        public int XPMaximo { get; set; } = 100;
        public string Descricao { get; set; }
        public string? StatusTreinamento { get; set; }
    }

    public class HabilidadePassiva
    {
        public string Nome { get; set; }
        public int Nivel { get; set; } = 1;
        public int XPAtual { get; set; } = 0;
        public int XPMaximo { get; set; } = 50;
        public string Descricao { get; set; }
        public string? StatusTreinamento { get; set; }
    }

    public class Equipamentos
    {
        public int Id { get; set; }
        public string? Arma { get; set; }
        public string? Armadura { get; set; }
        public List<string> Acessorios { get; set; } = new();
    }

    public class Treinamento
    {
        public string Tipo { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string ResultadoEsperado { get; set; }
    }
}
