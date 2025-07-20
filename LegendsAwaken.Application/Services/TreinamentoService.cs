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
        /// Inicia um treinamento para um herói.
        /// </summary>
        /// <param name="heroiId">ID do herói a ser treinado.</param>
        /// <param name="tipoTreinamento">Tipo de treinamento (ex: atributo, habilidade, desbloqueio).</param>
        /// <param name="duracao">Duração em minutos do treinamento.</param>
        /// <param name="resultadoEsperado">Descrição do resultado esperado.</param>
        public async Task IniciarTreinamentoAsync(Guid heroiId, string tipoTreinamento, TimeSpan duracao, string resultadoEsperado)
        {
            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId);
            if (heroi == null)
                throw new Exception("Herói não encontrado.");

            if (heroi.Treinamento != null && heroi.Treinamento.Fim > DateTime.UtcNow)
                throw new Exception("Herói já está em treinamento.");

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
        /// Finaliza o treinamento do herói, aplicando os resultados esperados.
        /// </summary>
        public async Task FinalizarTreinamentoAsync(Guid heroiId)
        {
            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId);
            if (heroi == null)
                throw new Exception("Herói não encontrado.");

            if (heroi.Treinamento == null || heroi.Treinamento.Fim > DateTime.UtcNow)
                throw new Exception("Treinamento ainda não finalizado.");

            // Exemplo: aplicar melhorias conforme tipoTreinamento e resultadoEsperado
            switch (heroi.Treinamento.Tipo)
            {
                case "Atributo":
                    // Lógica para aumentar atributo
                    break;
                case "Habilidade":
                    // Lógica para evoluir habilidade
                    break;
                case "Desbloqueio":
                    // Lógica para desbloquear habilidades novas
                    break;
                default:
                    break;
            }

            heroi.Treinamento = null;
            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

        /// <summary>
        /// Verifica se o herói está em treinamento.
        /// </summary>
        public bool EstaEmTreinamento(Heroi heroi)
        {
            return heroi.Treinamento != null && heroi.Treinamento.Fim > DateTime.UtcNow;
        }
    }
}
