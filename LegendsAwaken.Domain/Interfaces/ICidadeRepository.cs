using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface ICidadeRepository
    {
        Task<Cidade?> ObterPorIdAsync(Guid cidadeId);
        Task AdicionarAsync(Cidade cidade);
        Task AtualizarAsync(Cidade cidade);
        Task<List<Cidade>> ObterTodasAsync();
        Task<Cidade?> ObterPorProprietarioIdAsync(ulong usuarioId);
        Task AdicionarTrabalhadorAsync(Guid cidadeId, PersonagemTrabalhador trabalhador);
        Task RemoverTrabalhadorAsync(Guid trabalhadorId);
        Task AdicionarConstrucaoAsync(Guid cidadeId, Construcao construcao);
    }
}

