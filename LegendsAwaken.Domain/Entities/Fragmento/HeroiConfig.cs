using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class HeroiConfig
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Titulo { get; set; }
    public Raridade RaridadeBase { get; set; }
    public Profissao Arquetipo { get; set; }
    public string? Tag { get; set; }
    public string? ImageUrl { get; set; }        // R2 key: heroes/display/001.webp
    public string? ImageUrlThumb { get; set; }   // R2 key: heroes/thumb/001.webp

    public string NomeCompleto => Titulo is null ? Nome : $"{Nome}, {Titulo}";
}
