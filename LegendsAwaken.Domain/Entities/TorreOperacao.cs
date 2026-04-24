using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities
{
    public class TorreOperacao
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public int AndarNumero { get; set; }
        public ObjetivoOperacao Objetivo { get; set; }
        public PerfilRisco PerfilRisco { get; set; }
        public StatusOperacao Status { get; set; }
        public DateTime IniciadoEm { get; set; }
        public int DuracaoHoras { get; set; }

        public int? ResultadoOuro { get; set; }
        public string? ResultadoRecursoNome { get; set; }
        public int? ResultadoRecursoQtd { get; set; }
        public DateTime? ConcluidoEm { get; set; }
    }
}
