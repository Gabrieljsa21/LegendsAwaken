using System;
using System.Collections.Generic;
using LegendsAwaken.Domain.Entities.Banner;

namespace LegendsAwaken.Domain.Entities
{
    public class Usuario
    {
        public required ulong Id { get; set; } // ID único do usuário no Discord

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
        public List<BannerProgresso> ProgressoPorBanner { get; set; } = new();
        public List<BannerHistorico> HistoricoBanners { get; set; } = new();

    }
    

}
