using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Interfaces;

public interface IFragmentoRepository
{
    Task<FragmentoProgresso?> ObterPorHeroiAsync(Guid usuarioId, Guid heroiId);
    Task<FragmentoProgresso?> ObterPorArquetipoAsync(Guid usuarioId, Profissao arquetipo);
    Task UpsertAsync(FragmentoProgresso progresso);
    Task<List<FragmentoProgresso>> ListarPorUsuarioAsync(Guid usuarioId);
}
