using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class TreinamentoService
    {
        private readonly IHeroiRepository _heroiRepository;

        public TreinamentoService(IHeroiRepository heroiRepository)
        {
            _heroiRepository = heroiRepository;
        }

        /// <summary>
        /// Inicia um treinamento para um her�i.
        /// </summary>
        /// <param name="heroiId">ID do her�i a ser treinado.</param>
        /// <param name="tipoTreinamento">Tipo de treinamento (ex: atributo, habilidade, desbloqueio).</param>
        /// <param name="duracao">Dura��o em minutos do treinamento.</param>
        /// <param name="resultadoEsperado">Descri��o do resultado esperado.</param>
        public async Task IniciarTreinamentoAsync(Guid heroiId, string tipoTreinamento, TimeSpan duracao, string resultadoEsperado)
        {
            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId);
            if (heroi == null)
                throw new Exception("Her�i n�o encontrado.");

            if (heroi.Treinamento != null && heroi.Treinamento.Fim > DateTime.UtcNow)
                throw new Exception("Her�i j� est� em treinamento.");

            heroi.Treinamento = new Treinamento
            {
                Tipo = tipoTreinamento,
                Inicio = DateTime.UtcNow,
                Fim = DateTime.UtcNow.Add(duracao),
                ResultadoEsperado = resultadoEsperado
            };

            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

        /// <summary>
        /// Finaliza o treinamento do her�i, aplicando os resultados esperados.
        /// </summary>
        public async Task FinalizarTreinamentoAsync(Guid heroiId)
        {
            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId);
            if (heroi == null)
                throw new Exception("Her�i n�o encontrado.");

            if (heroi.Treinamento == null || heroi.Treinamento.Fim > DateTime.UtcNow)
                throw new Exception("Treinamento ainda n�o finalizado.");

            // Exemplo: aplicar melhorias conforme tipoTreinamento e resultadoEsperado
            switch (heroi.Treinamento.Tipo)
            {
                case "Atributo":
                    // L�gica para aumentar atributo
                    break;
                case "Habilidade":
                    // L�gica para evoluir habilidade
                    break;
                case "Desbloqueio":
                    // L�gica para desbloquear habilidades novas
                    break;
                default:
                    break;
            }

            heroi.Treinamento = null;
            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

        /// <summary>
        /// Verifica se o her�i est� em treinamento.
        /// </summary>
        public bool EstaEmTreinamento(Heroi heroi)
        {
            return heroi.Treinamento != null && heroi.Treinamento.Fim > DateTime.UtcNow;
        }
    }
}
