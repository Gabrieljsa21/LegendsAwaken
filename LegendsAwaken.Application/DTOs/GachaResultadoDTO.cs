using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Application.DTOs
{
    public class GachaResultadoDTO
    {
        /// <summary>
        /// ID do her�i invocado.
        /// </summary>
        public required string HeroiId { get; set; }

        /// <summary>
        /// Nome do her�i invocado.
        /// </summary>
        public required string Nome { get; set; }

        /// <summary>
        /// Raridade do her�i invocado.
        /// </summary>
        public Raridade Raridade { get; set; }

        /// <summary>
        /// Profissao do her�i (pode ser nulo se ainda n�o definida).
        /// </summary>
        public string? Profissao { get; set; }

        /// <summary>
        /// Descri��o curta ou frase especial da invoca��o.
        /// </summary>
        public string? Descricao { get; set; }

        /// <summary>
        /// Indica se o her�i � raro, �pico, etc (pode ser usado para exibir �cones).
        /// </summary>
        public string? Categoria { get; set; }

        /// <summary>
        /// URL da imagem do her�i (para exibir em embed).
        /// </summary>
        public string? ImagemUrl { get; set; }
    }
}
