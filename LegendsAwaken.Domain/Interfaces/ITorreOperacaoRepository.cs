using LegendsAwaken.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface ITorreOperacaoRepository
    {
        Task<TorreOperacao?> ObterAtivaAsync(Guid usuarioId);
        Task<TorreOperacao?> ObterConcluidaAsync(Guid usuarioId);
        Task AdicionarAsync(TorreOperacao operacao);
        Task AtualizarAsync(TorreOperacao operacao);
        Task EnsureTableAsync();
    }
}
