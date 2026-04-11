using LegendsAwaken.Application.Interfaces;
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
        private readonly HabilidadeService _habilidadeService;
        private readonly IAtributoBonusService _atributoBonusProvider;

        public HeroiService(IHeroiRepository heroiRepository, HabilidadeService habilidadeService, IAtributoBonusService atributoBonusProvider)
        {
            _heroiRepository = heroiRepository;
            _habilidadeService = habilidadeService;
            _atributoBonusProvider = atributoBonusProvider;
        }

        /// <summary>
        /// Cria um novo her�i usando a HeroiFactory e salva no reposit�rio.
        /// </summary>
        public async Task<Heroi> CriarHeroiAsync(
            ulong usuarioId,
            string nome,
            Raridade raridade,
            Raca raca,
            string antecedente,
            List<HeroiAfinidadeElemental> afinidade,
            FuncaoTatica? funcao = null)
        {

            var habilidades = await GerarHabilidadesIniciaisAsync(raridade, _habilidadeService);

            // Cria o her�i usando a factory
            var heroi = HeroiFactory.CriarHeroi(
                usuarioId,
                nome,
                raridade,
                raca,
                antecedente,
                afinidade,
                habilidades,
                funcao);

            // Define datas de cria��o/altera��o
            heroi.DataCriacao = DateTime.UtcNow;
            heroi.DataAlteracao = DateTime.UtcNow;

            // Salva no reposit�rio
            await _heroiRepository.AdicionarAsync(heroi);

            return heroi;
        }

        public async Task<AtributosBase> ObterAtributosFinaisAsync(Guid heroiId)
        {
            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId)
                ?? throw new InvalidOperationException($"Herói {heroiId} não encontrado.");
            var bonus = _atributoBonusProvider.ObterBonus(heroi.Habilidades);
            return heroi.ObterAtributosTotais(bonus);
        }

        public static async Task<List<HeroiHabilidade>> GerarHabilidadesIniciaisAsync(Raridade raridade, HabilidadeService habilidadeService)
        {
            var habilidades = new List<HeroiHabilidade>();
            var todasHabilidades = (await habilidadeService.ObterTodasAsync())
                .Where(h => h.Rank <= (int)raridade)    //Heroi aprende apenas habilidades de acordo com a raridade
                .ToList();

            var random = new Random();
            int quantidade = raridade switch
            {
                Raridade.Estrela1 => 1,
                Raridade.Estrela2 => 2,
                Raridade.Estrela3 => 3,
                Raridade.Estrela4 => 4,
                Raridade.Estrela5 => 5,
                _ => 1
            };

            for (int i = 0; i < quantidade && todasHabilidades.Any(); i++)
            {
                var habilidadeEscolhida = todasHabilidades[random.Next(todasHabilidades.Count)];
                habilidades.Add(new HeroiHabilidade
                {
                    HabilidadeId = habilidadeEscolhida.Id,
                    Habilidade = habilidadeEscolhida,
                    Nivel = 1,
                    XPAtual = 0,
                    XPMaximo = 100
                });
                todasHabilidades.Remove(habilidadeEscolhida);
            }

            return habilidades;
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

            var habilidade = heroi.Habilidades.FirstOrDefault(h => h.Habilidade.Nome.Equals(nomeHabilidade, StringComparison.OrdinalIgnoreCase));

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

    }
}
