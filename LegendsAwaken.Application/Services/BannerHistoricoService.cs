using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Banner;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class BannerHistoricoService
    {
        private readonly IBannerHistoricoRepository _bannerHistoricoRepository;

        public BannerHistoricoService(IBannerHistoricoRepository bannerHistoricoRepository)
        {
            _bannerHistoricoRepository = bannerHistoricoRepository;
        }

        // Incrementa o contador de invocações para um usuário e banner
        public async Task IncrementarInvocacaoAsync(ulong usuarioId, string bannerId)
        {
            var historico = await _bannerHistoricoRepository.ObterPorUsuarioEbannerAsync(usuarioId, bannerId);
            if (historico == null)
            {
                historico = new BannerHistorico
                {
                    UsuarioId = usuarioId,
                    BannerId = bannerId,
                    QuantidadeInvocacoes = 1,
                    DataUltimoReset = DateTime.UtcNow
                };

                await _bannerHistoricoRepository.AdicionarAsync(historico);
            }
            else
            {
                historico.QuantidadeInvocacoes++;
                await _bannerHistoricoRepository.AtualizarAsync(historico);
            }
        }

        // Reseta o contador quando o jogador obtém a raça especial (ou outra condição)
        public async Task ResetarHistoricoAsync(ulong usuarioId, string bannerId)
        {
            var historico = await _bannerHistoricoRepository.ObterPorUsuarioEbannerAsync(usuarioId, bannerId);
            if (historico != null)
            {
                historico.QuantidadeInvocacoes = 0;
                historico.DataUltimoReset = DateTime.UtcNow;
                await _bannerHistoricoRepository.AtualizarAsync(historico);
            }
        }

        // Retorna o número de invocações feitas por um usuário em um banner específico
        public async Task<int> ObterContadorAsync(ulong usuarioId, string bannerId)
        {
            var historico = await _bannerHistoricoRepository.ObterPorUsuarioEbannerAsync(usuarioId, bannerId);
            return historico?.QuantidadeInvocacoes ?? 0;
        }

    }
}
