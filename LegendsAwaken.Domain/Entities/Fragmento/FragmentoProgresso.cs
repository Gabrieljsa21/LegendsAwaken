using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities.Fragmento;

public class FragmentoProgresso
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public TipoFragmento TipoFragmento { get; set; }
    public Guid? HeroiId { get; set; }
    public HeroiConfig? Heroi { get; set; }
    // Preenchido se TipoFragmento == Arquetipo. Nulo se Heroi ou Generico.
    public Profissao? Arquetipo { get; set; }
    public int Quantidade { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
