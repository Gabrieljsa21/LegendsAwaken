using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class CidadeService
    {
        private readonly ICidadeRepository _cidadeRepository;

        public CidadeService(ICidadeRepository cidadeRepository)
        {
            _cidadeRepository = cidadeRepository;
        }

        public async Task<Cidade> CriarCidadeAsync(string nome, ulong usuarioId)
        {
            // Cria uma nova cidade para o usu�rio
            var cidade = new Cidade
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                UsuarioId = usuarioId,
                Recursos = new Recursos(),
                DataCriacao = DateTime.UtcNow,
                DataAlteracao = DateTime.UtcNow
            };

            await _cidadeRepository.AdicionarAsync(cidade);
            return cidade;
        }

        public async Task<Cidade> ObterCidadePorUsuarioAsync(ulong usuarioId)
        {
            return await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);
        }

        public async Task AtualizarRecursosAsync(Guid cidadeId, string tipoRecurso, int quantidade)
        {
            var cidade = await _cidadeRepository.ObterPorIdAsync(cidadeId);
            if (cidade == null) throw new Exception("Cidade n�o encontrada.");

            cidade.Recursos.Adicionar(quantidade, tipoRecurso);
            cidade.DataAlteracao = DateTime.UtcNow;

            await _cidadeRepository.AtualizarAsync(cidade);
        }


        public async Task<List<Cidade>> ObterTodasCidadesAsync()
        {
            return await _cidadeRepository.ObterTodasAsync();
        }

        // Outros m�todos para gerenciar profiss�es, miss�es, etc.
    }
}
