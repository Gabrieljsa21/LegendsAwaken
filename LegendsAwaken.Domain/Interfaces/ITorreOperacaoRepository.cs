using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface ITorreOperacaoRepository
    {
        Task<TorreOperacao?> ObterAtivaAsync(Guid usuarioId);
        Task<List<TorreOperacao>> ListarAtivasAsync(Guid usuarioId);
        Task<TorreOperacao?> ObterConcluidaAsync(Guid usuarioId);
        Task<List<TorreOperacao>> ListarConcluidasAsync(Guid usuarioId);
        Task<TorreOperacao?> ObterPorAndarAsync(Guid usuarioId, int andar);
        Task AdicionarAsync(TorreOperacao operacao);
        Task AtualizarAsync(TorreOperacao operacao);
        Task EnsureTableAsync();
    }
}
