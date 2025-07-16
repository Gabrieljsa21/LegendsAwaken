using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class HeroiService
    {
        private readonly IHeroiRepository _heroiRepository;

        public HeroiService(IHeroiRepository heroiRepository)
        {
            _heroiRepository = heroiRepository;
        }

        /// <summary>
        /// Cria um novo her�i a partir de dados base.
        /// </summary>
        public async Task<Heroi> CriarHeroiAsync(Heroi heroi)
        {
            heroi.Id = Guid.NewGuid();
            heroi.DataCriacao = DateTime.UtcNow;
            heroi.DataAlteracao = DateTime.UtcNow;

            await _heroiRepository.AdicionarAsync(heroi);
            return heroi;
        }

        /// <summary>
        /// Obt�m her�i pelo ID.
        /// </summary>
        public async Task<Heroi> ObterHeroiPorIdAsync(Guid heroiId)
        {
            return await _heroiRepository.ObterPorIdAsync(heroiId);
        }

        /// <summary>
        /// Atualiza os dados do her�i.
        /// </summary>
        public async Task AtualizarHeroiAsync(Heroi heroi)
        {
            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

        /// <summary>
        /// Lista todos os her�is do usu�rio.
        /// </summary>
        public async Task<List<Heroi>> ObterHeroisPorUsuarioAsync(ulong usuarioId)
        {
            return await _heroiRepository.ObterPorUsuarioIdAsync(usuarioId);
        }

        /// <summary>
        /// Incrementa XP de uma habilidade espec�fica do her�i.
        /// </summary>
        public async Task TreinarHabilidadeAsync(Guid heroiId, string nomeHabilidade, int xpGanho)
        {
            var heroi = await ObterHeroiPorIdAsync(heroiId);
            if (heroi == null)
                throw new Exception("Her�i n�o encontrado.");

            var habilidade = heroi.Habilidades.Ativas.Find(h => h.Nome == nomeHabilidade);
            if (habilidade == null)
                throw new Exception("Habilidade n�o encontrada.");

            habilidade.XPAtual += xpGanho;
            while (habilidade.Nivel < 10 && habilidade.XPAtual >= habilidade.XPMaximo)
            {
                habilidade.XPAtual -= habilidade.XPMaximo;
                habilidade.Nivel++;
                habilidade.XPMaximo += 50; // exemplo de aumento de XP m�ximo por n�vel
            }

            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

        // Outros m�todos para evoluir atributos, desbloquear habilidades etc.
    }
}
