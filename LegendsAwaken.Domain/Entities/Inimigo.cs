using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities;

public class Inimigo
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoInimigo Tipo { get; set; }
    public Elemento? ElementoAfinidade { get; set; }
    public Elemento? ElementoFraqueza { get; set; }
    public Guid BiomaId { get; set; }
    public int AndarMinimo { get; set; }
    public int AndarMaximo { get; set; }
    public bool EChefe { get; set; } = false;

    public Bioma? Bioma { get; set; }
}
