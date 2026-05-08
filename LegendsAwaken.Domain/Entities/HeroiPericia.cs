using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities;

public class HeroiPericia
{
    public Guid Id { get; set; }
    public Guid HeroiId { get; set; }
    public Pericia Pericia { get; set; }
    public bool TemProficiencia { get; set; }
    public int Rank { get; set; } = 0;
    public Heroi Heroi { get; set; } = null!;
}
