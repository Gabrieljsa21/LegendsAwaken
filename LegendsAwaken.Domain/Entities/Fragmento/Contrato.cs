using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class Contrato
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public TipoContrato Tipo { get; set; }
    public Profissao? Arquetipo { get; set; }
    public Guid? HeroiId { get; set; }
    public HeroiConfig? Heroi { get; set; }
    public bool Ativo { get; set; }
    public DateTime? ExpiraEm { get; set; }
    public DateTime CriadoEm { get; set; }
}
