using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class BiomHeroPool
{
    public Guid Id { get; set; }
    public Guid BiomeId { get; set; }
    public Bioma Bioma { get; set; } = null!;
    public Guid HeroiId { get; set; }
    public HeroiConfig Heroi { get; set; } = null!;
    public Raridade Raridade { get; set; }
    public int DropWeight { get; set; }
    public bool EHeroPrincipal { get; set; }
}
