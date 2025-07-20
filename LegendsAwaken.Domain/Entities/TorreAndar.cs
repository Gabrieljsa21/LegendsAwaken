using System;
using System.Collections.Generic;

namespace LegendsAwaken.Domain.Entities
{
    public class TorreAndar
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }

        public int Numero { get; set; } // Ex: 1, 2, ...
        public TipoAndar Tipo { get; set; } // Subjugacao, Fuga, etc.
        public bool TemBoss { get; set; } = false;
        public NivelBoss? DificuldadeBoss { get; set; }

        public List<Inimigo> Inimigos { get; set; } = new();

        public string? RecompensaTipo { get; set; }
        public int RecompensaQuantidade { get; set; }

        public bool ObjetivoCumprido { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? DataAlteracao { get; set; }
    }


    public enum TipoAndar
    {
        Normal,
        BossFacil,
        BossMedio,
        BossDificil,
        Subjugacao,
        Fuga,
        Escolta,
        Defesa,
        Armadilha,
        EventoEspecial
    }


    public enum NivelBoss
    {
        Facil,
        Medio,
        Dificil
    }

    public class Inimigo
    {
        public Guid Id { get; set; }
        public required string Nome { get; set; }
        public int Nivel { get; set; }
        public required string Tipo { get; set; } // Ex: "Morto-vivo", "Fera"
        public required Atributos Atributos { get; set; }
        public List<string> Habilidades { get; set; } = new();
    }

    public class Atributos
    {
        public int Forca { get; set; }
        public int Destreza { get; set; }
        public int Constituicao { get; set; }
        public int Inteligencia { get; set; }
        public int Sabedoria { get; set; }
        public int Carisma { get; set; }
    }
}
