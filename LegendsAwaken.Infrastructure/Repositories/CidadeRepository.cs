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

            var entry = _context.Entry(cidade);
            if (entry.State == EntityState.Detached)
                _context.Cidades.Attach(cidade);

            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task AdicionarTrabalhadorAsync(Guid cidadeId, PersonagemTrabalhador trabalhador)
        {
            // Explicitly Add to bypass navigation-fixup ambiguity (ValueGeneratedOnAdd + non-empty Guid)
            _context.Set<PersonagemTrabalhador>().Add(trabalhador);
            _context.Entry(trabalhador).Property("CidadeId").CurrentValue = cidadeId;
            await _context.SaveChangesAsync();
        }

        public async Task RemoverTrabalhadorAsync(Guid trabalhadorId)
        {
            var t = await _context.Set<PersonagemTrabalhador>().FindAsync(trabalhadorId);
            if (t != null)
            {
                _context.Set<PersonagemTrabalhador>().Remove(t);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AdicionarConstrucaoAsync(Guid cidadeId, Construcao construcao)
        {
            _context.Set<Construcao>().Add(construcao);
            _context.Entry(construcao).Property("CidadeId").CurrentValue = cidadeId;
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
