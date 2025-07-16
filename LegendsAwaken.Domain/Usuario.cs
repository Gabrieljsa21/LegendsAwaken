using System;
using System.Collections.Generic;

namespace LegendsAwaken.Domain
{
    public class Usuario
    {
        public ulong DiscordId { get; set; } // ID único do usuário no Discord

        public string Nome { get; set; } = string.Empty;
        public int NivelConta { get; set; } = 1;
        public int XP { get; set; } = 0;

        public int Moedas { get; set; } = 0;
        public int Cristais { get; set; } = 0;
        public int Fragmentos { get; set; } = 0;
        public int PergaminhosSecretos { get; set; } = 0;

        public List<Guid> HeroisIds { get; set; } = new(); // IDs dos heróis invocados
        public Guid? CidadeId { get; set; } // ID da cidade que ele comanda

        public int AndarMaisAlto { get; set; } = 0; // Registro do progresso na torre
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime UltimoLogin { get; set; } = DateTime.UtcNow;
    }
}
