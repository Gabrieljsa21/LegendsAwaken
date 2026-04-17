using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class HeroiConfig
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Raridade RaridadeBase { get; set; }
    public Profissao Arquetipo { get; set; }
    public string? Tag { get; set; }
}
