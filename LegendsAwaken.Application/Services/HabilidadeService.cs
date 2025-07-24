using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class HabilidadeService
    {
        private readonly IHabilidadeRepository _habilidadeRepository;

        public HabilidadeService(IHabilidadeRepository habilidadeRepository)
        {
            _habilidadeRepository = habilidadeRepository;
        }

        public async Task<List<Habilidade>> ObterTodasAsync()
        {
            return await _habilidadeRepository.ObterTodasAsync();
        }

        public async Task<Habilidade?> ObterPorIdAsync(string id)
        {
            return await _habilidadeRepository.ObterPorIdAsync(id);
        }

        public async Task CriarAsync(Habilidade habilidade)
        {
            await _habilidadeRepository.AdicionarAsync(habilidade);
        }

        public async Task AtualizarAsync(Habilidade habilidade)
        {
            await _habilidadeRepository.AtualizarAsync(habilidade);
        }

        public async Task RemoverAsync(string id)
        {
            await _habilidadeRepository.RemoverAsync(id);
        }
    }
}
