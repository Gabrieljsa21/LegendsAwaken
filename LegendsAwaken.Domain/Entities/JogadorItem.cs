using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities;

public class JogadorItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string ItemConfigId { get; set; } = "";   // ex: "bioma_a_12"
    public string Nome { get; set; } = "";
    public TipoItemJogador Tipo { get; set; }
    public string Icone { get; set; } = "📦";
    public string Efeito { get; set; } = "";
    public int Quantidade { get; set; }
    public DateTime ObtidoEm { get; set; } = DateTime.UtcNow;
    public string? ExtraData { get; set; }           // JSON reservado para efeitos futuros
}
