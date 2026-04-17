using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Interfaces;

public interface IContratoRepository
{
    Task<Contrato?> ObterAtivoAsync(Guid usuarioId, TipoContrato tipo);
    Task SalvarAsync(Contrato contrato);
    Task DesativarAsync(Guid contratoId);
    Task<List<Contrato>> ListarAtivosVencidosAsync(DateTime agora);
}
