using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class GachaService
    {
        private readonly IHeroiRepository _heroiRepository;
        private readonly Random _random;

        public GachaService(IHeroiRepository heroiRepository)
        {
            _heroiRepository = heroiRepository;
            _random = new Random();
        }

        /// <summary>
        /// Realiza uma invocação comum de herói.
        /// </summary>
        /// <returns>Resultado da invocação.</returns>
        public async Task<GachaResultadoDTO> InvocarComumAsync()
        {
            // Definir as chances conforme tabela do projeto
            // Exemplo simplificado: 10% de 3 estrelas, 90% entre 1 e 2 estrelas

            var todosHerois = await _heroiRepository.ObterTodosAsync();

            // Filtrar heróis 3 estrelas
            var raros = todosHerois.Where(h => h.Raridade == Raridade.Estrela3).ToList();
            var comuns = todosHerois.Where(h => h.Raridade <= Raridade.Estrela2).ToList();

            int chance = _random.Next(1, 101); // 1 a 100

            Heroi heroiEscolhido;

            if (chance <= 10 && raros.Count > 0)
            {
                heroiEscolhido = raros[_random.Next(raros.Count)];
            }
            else if (comuns.Count > 0)
            {
                heroiEscolhido = comuns[_random.Next(comuns.Count)];
            }
            else
            {
                // fallback: qualquer herói
                heroiEscolhido = todosHerois[_random.Next(todosHerois.Count)];
            }

            // Montar DTO para retorno
            var resultado = new GachaResultadoDTO
            {
                HeroiId = heroiEscolhido.Id.ToString(),
                Nome = heroiEscolhido.Nome,
                Raridade = heroiEscolhido.Raridade,
                Classe = heroiEscolhido.Classe,
                Descricao = $"Você invocou {heroiEscolhido.Nome}!",
                Categoria = $"⭐{heroiEscolhido.Raridade}",
                ImagemUrl = heroiEscolhido.ImagemUrl
            };

            return resultado;
        }

        // Métodos para invocação rara, épica, evento etc podem ser adicionados aqui.
    }
}
