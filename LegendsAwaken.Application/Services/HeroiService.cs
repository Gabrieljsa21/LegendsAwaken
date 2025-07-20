using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Entities.Banner;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Factories;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Cria um novo her�i usando a HeroiFactory e salva no reposit�rio.
        /// </summary>
        public async Task<Heroi> CriarHeroiAsync(
            ulong usuarioId,
            string nome,
            Raridade raridade,
            string raca,
            string profissao,
            string antecedente,
            List<HeroiAfinidadeElemental> afinidade,
            FuncaoTatica? funcao = null)
        {
            // Cria o her�i usando a factory
            var heroi = HeroiFactory.CriarHeroi(
                usuarioId,
                nome,
                raridade,
                raca,
                profissao,
                antecedente,
                afinidade,
                funcao);

            // Define datas de cria��o/altera��o
            heroi.DataCriacao = DateTime.UtcNow;
            heroi.DataAlteracao = DateTime.UtcNow;

            // Salva no reposit�rio
            await _heroiRepository.AdicionarAsync(heroi);

            return heroi;
        }

        /// <summary>
        /// Obt�m her�i pelo ID.
        /// </summary>
        public async Task<Heroi?> ObterHeroiPorIdAsync(Guid heroiId)
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

            var habilidade = heroi.Habilidades
                .FirstOrDefault(h => h.Nome.Equals(nomeHabilidade, StringComparison.OrdinalIgnoreCase));

            if (habilidade == null)
                throw new Exception("Habilidade n�o encontrada.");

            habilidade.XPAtual += xpGanho;
            while (habilidade.Nivel < 10 && habilidade.XPAtual >= habilidade.XPMaximo)
            {
                habilidade.XPAtual -= habilidade.XPMaximo;
                habilidade.Nivel++;
                habilidade.XPMaximo += 50; // Exemplo de progress�o
            }

            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

        public string GerarNomeAleatorio(string username, int numero)
        {
            // Prefixos e sufixos simples para variedade
            var prefixos = new[] { "Ar", "Bel", "Dor", "Eli", "Fen", "Gal", "Hor", "Ith", "Jor", "Kel" };
            var sufixos = new[] { "ion", "ar", "eth", "mir", "dor", "an", "iel", "or", "us", "wyn" };

            var random = new Random(Guid.NewGuid().GetHashCode());
            var prefixo = prefixos[random.Next(prefixos.Length)];
            var sufixo = sufixos[random.Next(sufixos.Length)];

            return $"{prefixo}{sufixo}_{username}_{numero}";
        }

    }
}
