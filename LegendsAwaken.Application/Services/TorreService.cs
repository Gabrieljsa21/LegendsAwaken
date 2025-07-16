using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class TorreService
    {
        private readonly ITorreRepository _torreRepository;

        public TorreService(ITorreRepository torreRepository)
        {
            _torreRepository = torreRepository;
        }

        /// <summary>
        /// Obtém o andar atual da torre para um usuário.
        /// </summary>
        public async Task<TorreAndar> ObterAndarAtualAsync(Guid usuarioId)
        {
            // Lógica para obter o andar atual do usuário.
            return await _torreRepository.ObterAndarPorUsuarioAsync(usuarioId);
        }

        /// <summary>
        /// Avança o usuário para o próximo andar, validando o objetivo do andar atual.
        /// </summary>
        public async Task<bool> SubirAndarAsync(Guid usuarioId)
        {
            var andarAtual = await ObterAndarAtualAsync(usuarioId);
            if (andarAtual == null)
                throw new Exception("Andar atual não encontrado.");

            // Verificar se o objetivo do andar foi cumprido (exemplo simplificado)
            if (!andarAtual.ObjetivoCumprido)
                return false;

            var proximoNumero = andarAtual.Numero + 1;

            var proximoAndar = new TorreAndar
            {
                Id = Guid.NewGuid(),
                Numero = proximoNumero,
                Tipo = DefinirTipoAndar(proximoNumero),
                UsuarioId = usuarioId,
                CriadoEm = DateTime.UtcNow,
                ObjetivoCumprido = false,
                // Outros campos...
            };

            await _torreRepository.AdicionarAsync(proximoAndar);
            return true;
        }

        /// <summary>
        /// Define o tipo do andar com base no número do andar (ex: boss fácil, médio, difícil, etc).
        /// </summary>
        private TipoAndar DefinirTipoAndar(int numeroAndar)
        {
            if (numeroAndar % 25 == 0)
                return TipoAndar.BossDificil;
            if (numeroAndar % 10 == 0)
                return TipoAndar.BossMedio;
            if (numeroAndar % 5 == 0)
                return TipoAndar.BossFacil;

            return TipoAndar.Normal;
        }

        /// <summary>
        /// Método para registrar que o objetivo do andar foi cumprido.
        /// </summary>
        public async Task MarcarObjetivoCumpridoAsync(Guid andarId)
        {
            var andar = await _torreRepository.ObterPorIdAsync(andarId);
            if (andar == null)
                throw new Exception("Andar não encontrado.");

            andar.ObjetivoCumprido = true;
            andar.DataAlteracao = DateTime.UtcNow;

            await _torreRepository.AtualizarAsync(andar);
        }

        // Outros métodos para carregar inimigos, gerenciar grupos, etc.
    }
}
