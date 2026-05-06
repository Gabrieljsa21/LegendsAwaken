namespace LegendsAwaken.Domain.Entities;

public class RecursoEstoque
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Recurso { get; set; } = "";   // lowercase key — mesmo padrão de ResourceNodeConfig
    public int Quantidade { get; set; }
}
