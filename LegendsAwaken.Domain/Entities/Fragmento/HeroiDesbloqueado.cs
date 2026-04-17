namespace LegendsAwaken.Domain.Entities.Fragmento;

public class HeroiDesbloqueado
{
    public Guid UsuarioId { get; set; }
    public Guid HeroiId { get; set; }
    public HeroiConfig Heroi { get; set; } = null!;
    public DateTime DesbloqueadoEm { get; set; }
}
