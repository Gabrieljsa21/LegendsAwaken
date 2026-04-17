using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public record ReceitaCrafting(
        string Id,
        string Nome,
        SlotEquipamento Slot,
        IReadOnlyDictionary<string, int> Custo,
        IReadOnlyDictionary<Atributo, int> BonusBase
    );

    public class CraftingService
    {
        private readonly ICidadeRepository _cidadeRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ISlotOcupacaoRepository _slotRepository;
        private readonly IHeroiRepository _heroiRepository;

        // Static recipe list — Phase 3A uses existing resources only.
        // Phase 3B will add processed resources (Barra de Ferro, Tabua de Madeira).
        public static readonly IReadOnlyList<ReceitaCrafting> Receitas = new List<ReceitaCrafting>
        {
            new("espada-ferro",   "Espada de Ferro",      SlotEquipamento.Arma,
                new Dictionary<string, int> { ["pedra"] = 4, ["ouro"] = 3 },
                new Dictionary<Atributo, int> { [Atributo.Forca] = 10 }),

            new("arco-simples",   "Arco Simples",         SlotEquipamento.Arma,
                new Dictionary<string, int> { ["madeira"] = 4, ["ouro"] = 3 },
                new Dictionary<Atributo, int> { [Atributo.Percepcao] = 10 }),

            new("armadura-couro", "Armadura de Couro",    SlotEquipamento.Armadura,
                new Dictionary<string, int> { ["madeira"] = 3, ["comida"] = 2 },
                new Dictionary<Atributo, int> { [Atributo.Vitalidade] = 12 }),

            new("anel-arcano",    "Anel Arcano",          SlotEquipamento.Acessorio,
                new Dictionary<string, int> { ["erva"] = 4, ["ouro"] = 3 },
                new Dictionary<Atributo, int> { [Atributo.Inteligencia] = 8 }),

            new("amuleto-agilidade", "Amuleto de Agilidade", SlotEquipamento.Acessorio,
                new Dictionary<string, int> { ["erva"] = 2, ["madeira"] = 2 },
                new Dictionary<Atributo, int> { [Atributo.Agilidade] = 8 }),
        }.AsReadOnly();

        public CraftingService(
            ICidadeRepository cidadeRepository,
            IItemRepository itemRepository,
            ISlotOcupacaoRepository slotRepository,
            IHeroiRepository heroiRepository)
        {
            _cidadeRepository = cidadeRepository;
            _itemRepository = itemRepository;
            _slotRepository = slotRepository;
            _heroiRepository = heroiRepository;
        }

        public IReadOnlyList<ReceitaCrafting> ListarReceitas() => Receitas;

        /// <summary>
        /// Crafts an item: validates resources, consumes them, creates and persists the item.
        /// Phase 3A: always Comum quality. Phase 3A.2 will add quality checks.
        /// </summary>
        public async Task<(Item? item, string? erro)> CraftarAsync(ulong usuarioId, string receitaId)
        {
            var receita = Receitas.FirstOrDefault(r => r.Id == receitaId);
            if (receita == null)
                return (null, "Receita nao encontrada.");

            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);
            if (cidade == null)
                return (null, "Voce precisa ter uma cidade para craftar itens.");

            if (!VerificarRecursos(cidade.Recursos, receita.Custo))
            {
                var falta = FormatarCusto(receita.Custo);
                return (null, $"Recursos insuficientes. Custo: {falta}");
            }

            ConsumirRecursos(cidade.Recursos, receita.Custo);
            await _cidadeRepository.AtualizarAsync(cidade);

            // Quality check
            int skillCraft = 0;
            int bonusPredio = 0;

            // Find Forja responsável hero for skill_craft
            var forja = cidade.Construcoes.FirstOrDefault(c => c.TipoPredio == TipoPredio.Forja);
            if (forja != null)
            {
                bonusPredio = forja.Nivel * 2;
                var slots = await _slotRepository.ObterPorConstrucaoAsync(forja.Id);
                var responsavelSlot = slots.FirstOrDefault(s => s.SlotTipo == SlotTipo.Responsabilidade);
                if (responsavelSlot != null)
                {
                    var responsavelHeroi = await _heroiRepository.ObterPorIdAsync(responsavelSlot.HeroiId);
                    if (responsavelHeroi != null)
                    {
                        skillCraft = responsavelHeroi.Habilidades
                            .Where(h => h.Habilidade.TipoHabilidade == TipoHabilidade.Craft)
                            .Sum(h => h.Nivel);
                    }
                }
            }

            int roll = Random.Shared.Next(1, 21); // 1..20
            int resultado = skillCraft + bonusPredio + roll;
            var qualidade = resultado switch
            {
                < 10 => Qualidade.Comum,
                < 15 => Qualidade.Bom,
                < 20 => Qualidade.Raro,
                < 25 => Qualidade.Excepcional,
                _    => Qualidade.Mestre
            };

            var item = new Item
            {
                Id = Guid.NewGuid(),
                Nome = receita.Nome,
                Slot = receita.Slot,
                Qualidade = qualidade,
                ProprietarioId = usuarioId,
                Bonus = receita.BonusBase
                    .Select(kvp => new ItemBonus
                    {
                        Id = Guid.NewGuid(),
                        Atributo = kvp.Key,
                        Valor = kvp.Value
                    })
                    .ToList()
            };

            await _itemRepository.AdicionarAsync(item);
            return (item, null);
        }

        private static bool VerificarRecursos(Recursos recursos, IReadOnlyDictionary<string, int> custo)
        {
            foreach (var (tipo, qtd) in custo)
            {
                int disponivel = tipo switch
                {
                    "ouro"    => recursos.Ouro,
                    "madeira" => recursos.Madeira,
                    "pedra"   => recursos.Pedra,
                    "comida"  => recursos.Comida,
                    "erva"    => recursos.Erva,
                    _         => 0
                };
                if (disponivel < qtd) return false;
            }
            return true;
        }

        private static void ConsumirRecursos(Recursos recursos, IReadOnlyDictionary<string, int> custo)
        {
            foreach (var (tipo, qtd) in custo)
                recursos.Adicionar(-qtd, tipo);
        }

        private static string FormatarCusto(IReadOnlyDictionary<string, int> custo)
            => string.Join(", ", custo.Select(kvp => $"{kvp.Value} {kvp.Key}"));
    }
}
