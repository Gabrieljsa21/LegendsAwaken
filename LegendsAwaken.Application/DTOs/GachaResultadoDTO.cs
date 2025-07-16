using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Application.DTOs
{
    public class GachaResultadoDTO
    {
        /// <summary>
        /// ID do herói invocado.
        /// </summary>
        public string HeroiId { get; set; }

        /// <summary>
        /// Nome do herói invocado.
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Raridade do herói invocado.
        /// </summary>
        public Raridade Raridade { get; set; }

        /// <summary>
        /// Classe do herói (pode ser nulo se ainda não definida).
        /// </summary>
        public string Classe { get; set; }

        /// <summary>
        /// Descrição curta ou frase especial da invocação.
        /// </summary>
        public string Descricao { get; set; }

        /// <summary>
        /// Indica se o herói é raro, épico, etc (pode ser usado para exibir ícones).
        /// </summary>
        public string Categoria { get; set; }

        /// <summary>
        /// URL da imagem do herói (para exibir em embed).
        /// </summary>
        public string ImagemUrl { get; set; }
    }
}
