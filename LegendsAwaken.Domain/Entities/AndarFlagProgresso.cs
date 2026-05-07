namespace LegendsAwaken.Domain.Entities;

public sealed class AndarFlagProgresso
{
    public Guid UsuarioId { get; set; }
    public int Andar { get; set; }
    public string FlagNome { get; set; } = "";
    public bool Gerada { get; set; }
    public bool Expirou { get; set; }
    public DateTime? GeradaEm { get; set; }
}
