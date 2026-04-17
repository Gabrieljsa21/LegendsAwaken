using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class CidadeRepository : ICidadeRepository
    {
        private readonly LegendsAwakenDbContext _context;

        public CidadeRepository(LegendsAwakenDbContext context)
        {
            _context = context;
        }

        public async Task<Cidade?> ObterPorIdAsync(Guid cidadeId)
        {
            return await _context.Cidades
                .Include(c => c.Construcoes)
                .Include(c => c.Trabalhadores)
                .FirstOrDefaultAsync(c => c.Id == cidadeId);
        }

        public async Task<Cidade?> ObterPorProprietarioIdAsync(ulong usuarioId)
        {
            return await _context.Cidades
                .Include(c => c.Construcoes)
                .Include(c => c.Trabalhadores)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }

        public async Task AdicionarAsync(Cidade cidade)
        {
            _context.Cidades.Add(cidade);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Cidade cidade)
        {
            cidade.DataAlteracao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsync(Guid cidadeId)
        {
            return await _context.Cidades.AnyAsync(c => c.Id == cidadeId);
        }

        public async Task<List<Cidade>> ObterTodasAsync()
        {
            return await _context.Cidades
                .Include(c => c.Construcoes)
                .Include(c => c.Trabalhadores)
                .ToListAsync();
        }
    }
}
