namespace LegendsAwaken.Domain.Entities.Fragmento;

public class Bioma
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int AndarInicio { get; set; }
    public int AndarFim { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public List<BiomHeroPool> Pool { get; set; } = [];
}
