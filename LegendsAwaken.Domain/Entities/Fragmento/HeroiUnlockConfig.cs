using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class HeroiUnlockConfig
{
    public Guid HeroiId { get; set; }
    public HeroiConfig Heroi { get; set; } = null!;
    public TipoUnlock TipoUnlock { get; set; }
    public int? QuantidadeFragmentos { get; set; }
    public int? AndarMarco { get; set; }
    public string? CondicaoDescricao { get; set; }
}
