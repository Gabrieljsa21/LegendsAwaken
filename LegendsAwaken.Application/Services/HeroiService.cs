using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
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
        private readonly HeroiLevelUpService _levelUpService;
        private readonly IItemRepository _itemRepository;

        public HeroiService(IHeroiRepository heroiRepository, HabilidadeService habilidadeService, IAtributoBonusService atributoBonusProvider, HeroiLevelUpService levelUpService, IItemRepository itemRepository)
        {
            _heroiRepository = heroiRepository;
            _habilidadeService = habilidadeService;
            _atributoBonusProvider = atributoBonusProvider;
            _levelUpService = levelUpService;
            _itemRepository = itemRepository;
        }

        /// <summary>
        /// Cria um novo heroi usando a HeroiFactory e salva no repositorio.
        /// </summary>
        public async Task<Heroi> CriarHeroiAsync(
            ulong usuarioId,
            string nome,
            Raridade raridade,
            Raca raca,
            string antecedente,
            List<HeroiAfinidadeElemental> afinidade,
            FuncaoTatica? funcao = null,
            string? titulo = null)
        {

            var habilidades = await GerarHabilidadesIniciaisAsync(raridade, _habilidadeService);

            // Stats base iguais para todos da raridade + bonus racial fixo por raca
            int r = (int)raridade;
            var atributosBase = _levelUpService.ObterAtributosBaseParaRaridade(r)
                + HeroiLevelUpService.BonusRacial.GetValueOrDefault(raca, new AtributosBase());

            var heroi = HeroiFactory.CriarHeroi(
                usuarioId,
                nome,
                raridade,
                raca,
                antecedente,
                afinidade,
                habilidades,
                atributosBase,
                funcao,
                titulo);

            heroi.DataCriacao = DateTime.UtcNow;
            heroi.DataAlteracao = DateTime.UtcNow;

            await _heroiRepository.AdicionarAsync(heroi);

            return heroi;
        }

        public async Task<AtributosBase> ObterAtributosFinaisAsync(Guid heroiId)
        {
            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId)
                ?? throw new InvalidOperationException($"Heroi {heroiId} nao encontrado.");
            var bonus = _atributoBonusProvider.ObterBonus(heroi.Habilidades);
            return heroi.ObterAtributosTotais(bonus);
        }

        public static async Task<List<HeroiHabilidade>> GerarHabilidadesIniciaisAsync(Raridade raridade, HabilidadeService habilidadeService)
        {
            var habilidades = new List<HeroiHabilidade>();
            var todasHabilidades = (await habilidadeService.ObterTodasAsync())
                .Where(h => h.Rank <= (int)raridade)
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
        /// Obtem heroi pelo ID.
        /// </summary>
        public async Task<Heroi?> ObterHeroiPorIdAsync(Guid heroiId)
        {
            return await _heroiRepository.ObterPorIdAsync(heroiId);
        }

        /// <summary>
        /// Atualiza os dados do heroi.
        /// </summary>
        public async Task AtualizarHeroiAsync(Heroi heroi)
        {
            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

        /// <summary>
        /// Lista todos os herois do usuario.
        /// </summary>
        public async Task<List<Heroi>> ObterHeroisPorUsuarioAsync(ulong usuarioId)
        {
            return await _heroiRepository.ObterPorUsuarioIdAsync(usuarioId);
        }

        public async Task<List<Item>> ObterItensAsync(ulong usuarioId)
            => await _itemRepository.ObterPorProprietarioAsync(usuarioId);

        public async Task<string?> DesequiparItemAsync(Guid itemId, ulong usuarioId)
        {
            var item = await _itemRepository.ObterPorIdAsync(itemId);
            if (item == null) return "Item não encontrado.";
            if (item.ProprietarioId != usuarioId) return "Este item não pertence a você.";
            if (!item.EstaEquipado || !item.HeroiEquipadoId.HasValue) return "Este item não está equipado.";

            var heroi = await _heroiRepository.ObterPorIdAsync(item.HeroiEquipadoId.Value);
            if (heroi == null) return "Herói não encontrado.";

            heroi.BonusAtributos.RemoveAll(b => b.ItemId == itemId);

            switch (item.Slot)
            {
                case SlotEquipamento.Arma:      heroi.Equipamentos.ArmaId      = null; break;
                case SlotEquipamento.Armadura:  heroi.Equipamentos.ArmaduraId  = null; break;
                case SlotEquipamento.Acessorio: heroi.Equipamentos.AcessorioId = null; break;
            }

            item.EstaEquipado    = false;
            item.HeroiEquipadoId = null;
            heroi.DataAlteracao  = DateTime.UtcNow;

            await _heroiRepository.AtualizarAsync(heroi);
            await _itemRepository.AtualizarAsync(item);
            return null;
        }

        /// <summary>
        /// Equips an item on a hero. Removes old equipment bonuses from HeroiBonusAtributo,
        /// adds new ones, updates Equipamentos slot FK, and persists both hero and item.
        /// </summary>
        public async Task<string?> EquiparItemAsync(Guid heroiId, Guid itemId, ulong usuarioId)
        {
            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId);
            if (heroi == null) return "Heroi nao encontrado.";
            if (heroi.UsuarioId != usuarioId) return "Este heroi nao pertence a voce.";

            var item = await _itemRepository.ObterPorIdAsync(itemId);
            if (item == null) return "Item nao encontrado.";
            if (item.ProprietarioId != usuarioId) return "Este item nao pertence a voce.";
            if (item.EstaEquipado && item.HeroiEquipadoId != heroiId) return "Item ja equipado em outro heroi.";

            // Unequip current item in same slot (if any)
            Guid? antigoItemId = item.Slot switch
            {
                SlotEquipamento.Arma      => heroi.Equipamentos.ArmaId,
                SlotEquipamento.Armadura  => heroi.Equipamentos.ArmaduraId,
                SlotEquipamento.Acessorio => heroi.Equipamentos.AcessorioId,
                _                         => null
            };

            if (antigoItemId.HasValue)
            {
                heroi.BonusAtributos.RemoveAll(b => b.ItemId == antigoItemId);
                var antigoItem = await _itemRepository.ObterPorIdAsync(antigoItemId.Value);
                if (antigoItem != null)
                {
                    antigoItem.EstaEquipado = false;
                    antigoItem.HeroiEquipadoId = null;
                    await _itemRepository.AtualizarAsync(antigoItem);
                }
            }

            // Equip new item — add stat bonuses to hero
            foreach (var bonus in item.Bonus)
            {
                heroi.BonusAtributos.Add(new HeroiBonusAtributo
                {
                    Id = Guid.NewGuid(),
                    HeroiId = heroiId,
                    Atributo = bonus.Atributo,
                    Valor = bonus.Valor,
                    Origem = OrigemBonusAtributo.Equipamento,
                    ItemId = item.Id
                });
            }

            // Update slot FK on Equipamentos
            switch (item.Slot)
            {
                case SlotEquipamento.Arma:      heroi.Equipamentos.ArmaId      = item.Id; break;
                case SlotEquipamento.Armadura:  heroi.Equipamentos.ArmaduraId  = item.Id; break;
                case SlotEquipamento.Acessorio: heroi.Equipamentos.AcessorioId = item.Id; break;
            }

            item.EstaEquipado = true;
            item.HeroiEquipadoId = heroiId;
            heroi.DataAlteracao = DateTime.UtcNow;

            await _heroiRepository.AtualizarAsync(heroi);
            await _itemRepository.AtualizarAsync(item);
            return null; // null = success
        }

        /// <summary>
        /// Incrementa XP de uma habilidade especifica do heroi.
        /// </summary>
        public async Task TreinarHabilidadeAsync(Guid heroiId, string nomeHabilidade, int xpGanho)
        {
            var heroi = await ObterHeroiPorIdAsync(heroiId);
            if (heroi == null)
                throw new Exception("Heroi nao encontrado.");

            var habilidade = heroi.Habilidades.FirstOrDefault(h => h.Habilidade.Nome.Equals(nomeHabilidade, StringComparison.OrdinalIgnoreCase));

            if (habilidade == null)
                throw new Exception("Habilidade nao encontrada.");

            habilidade.XPAtual += xpGanho;
            while (habilidade.Nivel < 10 && habilidade.XPAtual >= habilidade.XPMaximo)
            {
                habilidade.XPAtual -= habilidade.XPMaximo;
                habilidade.Nivel++;
                habilidade.XPMaximo += 50;
            }

            heroi.DataAlteracao = DateTime.UtcNow;
            await _heroiRepository.AtualizarAsync(heroi);
        }

    }
}
