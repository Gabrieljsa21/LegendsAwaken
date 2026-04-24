using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class CidadeService
    {
        private readonly ICidadeRepository _cidadeRepository;
        private readonly IHeroiRepository _heroiRepository;
        private readonly ISlotOcupacaoRepository _slotRepository;

        private const double HorasMaximaProducao = 24.0;

        public CidadeService(
            ICidadeRepository cidadeRepository,
            IHeroiRepository heroiRepository,
            ISlotOcupacaoRepository slotRepository)
        {
            _cidadeRepository = cidadeRepository;
            _heroiRepository = heroiRepository;
            _slotRepository = slotRepository;
        }

        public async Task<Cidade> CriarCidadeAsync(string nome, ulong usuarioId)
        {
            var cidade = new Cidade
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                UsuarioId = usuarioId,
                Recursos = new Recursos(),
                Construcoes = new List<Construcao>(), // city starts empty
                UltimaColeta = DateTime.UtcNow,
                DataCriacao = DateTime.UtcNow,
                DataAlteracao = DateTime.UtcNow
            };
            await _cidadeRepository.AdicionarAsync(cidade);
            return cidade;
        }

        public async Task<Cidade?> ObterCidadePorUsuarioAsync(ulong usuarioId)
            => await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);

        // ── ResourceNode allocation ──────────────────────────────────────────────

        public async Task AlocarRecursoAsync(ulong usuarioId, Guid heroiId, TipoResourceNode node)
        {
            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId)
                ?? throw new InvalidOperationException("Cidade não encontrada.");

            if (cidade.Trabalhadores.Any(t => t.HeroiId == heroiId))
                throw new InvalidOperationException("Herói já está trabalhando na cidade.");

            // Check if hero is in a building slot
            var slotExistente = await _slotRepository.ObterPorHeroiAsync(heroiId);
            if (slotExistente != null)
                throw new InvalidOperationException("Herói já está alocado em um prédio.");

            var trabalhador = new PersonagemTrabalhador
            {
                HeroiId = heroiId,
                InicioTrabalho = DateTime.UtcNow,
                ResourceNode = node
            };
            await _cidadeRepository.AdicionarTrabalhadorAsync(cidade.Id, trabalhador);
        }

        // ── Building slot allocation ─────────────────────────────────────────────

        public async Task<string?> AlocarSlotPredioAsync(ulong usuarioId, Guid heroiId, Guid construcaoId, SlotTipo slotTipo)
        {
            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);
            if (cidade == null) return "Cidade não encontrada.";

            // Hero must not be already working
            if (cidade.Trabalhadores.Any(t => t.HeroiId == heroiId))
                return "Herói já está alocado em um node de recurso.";

            var slotExistente = await _slotRepository.ObterPorHeroiAsync(heroiId);
            if (slotExistente != null)
                return "Herói já está alocado em um prédio.";

            var construcao = cidade.Construcoes.FirstOrDefault(c => c.Id == construcaoId);
            if (construcao == null)
                return "Prédio não encontrado.";

            if (!PredioConfig.Slots.TryGetValue((construcao.TipoPredio, construcao.Nivel), out var def))
                return "Configuração de slots não encontrada.";

            var slotsOcupados = await _slotRepository.ObterPorConstrucaoAsync(construcao.Id);
            var slotsDoTipo = slotsOcupados.Where(s => s.SlotTipo == slotTipo).ToList();

            int maxSlots = slotTipo == SlotTipo.Responsabilidade ? def.NumResponsabilidade : def.NumOperacao;
            if (slotsDoTipo.Count >= maxSlots)
                return $"Slots de {slotTipo} cheios ({maxSlots} máximo).";

            // Validate Responsabilidade requirements
            if (slotTipo == SlotTipo.Responsabilidade)
            {
                var herois = await _heroiRepository.ObterPorUsuarioIdAsync(usuarioId);
                var heroi = herois.FirstOrDefault(h => h.Id == heroiId);
                if (heroi == null) return "Herói não encontrado.";

                if (heroi.Confianca < def.ConfiancaMin)
                    return $"Herói precisa de Confiança ≥ {def.ConfiancaMin} (atual: {heroi.Confianca}).";

                int valorAtributo = heroi.ObterAtributosTotais(new AtributosBase()).Get(def.AtributoReq);
                if (valorAtributo < def.AtributoMin)
                    return $"Herói precisa de {def.AtributoReq} ≥ {def.AtributoMin} (atual: {valorAtributo}).";
            }

            int posicao = slotsDoTipo.Count; // next available slot index
            await _slotRepository.AdicionarAsync(new SlotOcupacao
            {
                Id = Guid.NewGuid(),
                ConstrucaoId = construcao.Id,
                HeroiId = heroiId,
                SlotTipo = slotTipo,
                PosicaoSlot = posicao
            });

            return null; // null = success
        }

        // ── Desalocar (handles both ResourceNode and Building) ───────────────────

        public async Task<string?> DesalocarHeroiAsync(ulong usuarioId, Guid heroiId)
        {
            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);
            if (cidade == null) return "Cidade não encontrada.";

            // Check ResourceNode
            var trabalhador = cidade.Trabalhadores.FirstOrDefault(t => t.HeroiId == heroiId);
            if (trabalhador != null)
            {
                await _cidadeRepository.RemoverTrabalhadorAsync(trabalhador.Id);
                return null;
            }

            // Check Building slot
            var slot = await _slotRepository.ObterPorHeroiAsync(heroiId);
            if (slot != null)
            {
                await _slotRepository.RemoverAsync(slot);
                return null;
            }

            return "Herói não está alocado na cidade.";
        }

        // ── Build a new building ─────────────────────────────────────────────────

        public async Task<string?> ConstruirPredioAsync(ulong usuarioId, TipoPredio tipoPredio)
        {
            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);
            if (cidade == null) return "Cidade não encontrada.";

            if (cidade.Construcoes.Any(c => c.TipoPredio == tipoPredio))
                return $"Prédio '{tipoPredio}' já foi construído.";

            if (!PredioConfig.CustosConstrucao.TryGetValue(tipoPredio, out var custo))
                return "Prédio inválido.";

            // Check resources
            if (cidade.Recursos.Ouro    < custo.Ouro)    return $"Ouro insuficiente. Precisa: {custo.Ouro}.";
            if (cidade.Recursos.Madeira < custo.Madeira) return $"Madeira insuficiente. Precisa: {custo.Madeira}.";
            if (cidade.Recursos.Pedra   < custo.Pedra)   return $"Pedra insuficiente. Precisa: {custo.Pedra}.";
            if (cidade.Recursos.Comida  < custo.Comida)  return $"Comida insuficiente. Precisa: {custo.Comida}.";

            // Consume resources
            cidade.Recursos.Adicionar(-custo.Ouro,    "ouro");
            cidade.Recursos.Adicionar(-custo.Madeira, "madeira");
            cidade.Recursos.Adicionar(-custo.Pedra,   "pedra");
            cidade.Recursos.Adicionar(-custo.Comida,  "comida");

            var nomePredio = tipoPredio.ToString();
            cidade.Construcoes.Add(new Construcao
            {
                Nome = nomePredio,
                Nivel = 1,
                TipoPredio = tipoPredio,
                EstaAtiva = true
            });

            await _cidadeRepository.AtualizarAsync(cidade);
            return null;
        }

        // ── Collect production ───────────────────────────────────────────────────

        public async Task<(Cidade cidade, Recursos produzido)> ColetarProducaoAsync(ulong usuarioId)
        {
            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId)
                ?? throw new InvalidOperationException("Cidade não encontrada.");

            var agora = DateTime.UtcNow;
            var horasDecorridas = Math.Min((agora - cidade.UltimaColeta).TotalHours, HorasMaximaProducao);

            if (horasDecorridas < 1.0 / 60.0)
                return (cidade, new Recursos());

            // Load heroes
            var herois = await _heroiRepository.ObterPorUsuarioIdAsync(usuarioId);
            var heroiPorId = herois.ToDictionary(h => h.Id);

            var produzido = new Recursos();

            // ── Tier 1: ResourceNode workers ──────────────────────────────────
            foreach (var trabalhador in cidade.Trabalhadores.Where(t => t.ResourceNode.HasValue))
            {
                if (!heroiPorId.TryGetValue(trabalhador.HeroiId, out var heroi)) continue;
                var node = trabalhador.ResourceNode!.Value;
                if (!ResourceNodeConfig.BaseRates.TryGetValue(node, out var rate)) continue;

                double bonus = heroi.Profissao.HasValue &&
                    ResourceNodeConfig.ProfissaoBonus.TryGetValue((node, heroi.Profissao.Value), out var b) ? b : 0.0;

                int quantidade = (int)(rate.basePorHora * (1.0 + bonus) * horasDecorridas);
                if (quantidade <= 0) continue;

                produzido.Adicionar(quantidade, rate.recurso);
                cidade.Recursos.Adicionar(quantidade, rate.recurso);
            }

            // ── Tier 2: Building production ───────────────────────────────────
            double humorCidade = CalcularHumorCidade(cidade, heroiPorId);
            double humorMult = humorCidade switch
            {
                <= 25 => 0.90,
                <= 60 => 1.00,
                <= 85 => 1.10,
                _     => 1.20
            };

            foreach (var construcao in cidade.Construcoes.Where(c => c.EstaAtiva))
            {
                if (!PredioConfig.Slots.TryGetValue((construcao.TipoPredio, construcao.Nivel), out var def)) continue;
                if (def.BaseProdPorHora == 0) continue; // non-production building
                if (!PredioConfig.RecursoProducao.TryGetValue(construcao.TipoPredio, out var recurso) || recurso == null) continue;

                var slots = await _slotRepository.ObterPorConstrucaoAsync(construcao.Id);
                var responsaveis = slots.Where(s => s.SlotTipo == SlotTipo.Responsabilidade).ToList();
                var operadores = slots.Where(s => s.SlotTipo == SlotTipo.Operacao).ToList();

                // Building inactive if no Responsibility slot filled
                if (responsaveis.Count == 0) continue;

                // Mult(Responsáveis) = average efficiency of Responsabilidade heroes
                double multResp = responsaveis
                    .Select(s => heroiPorId.TryGetValue(s.HeroiId, out var h)
                        ? 1.0 + h.ObterAtributosTotais(new AtributosBase()).Get(def.AtributoReq) / 100.0
                        : 1.0)
                    .Average();

                // Soma(Operadores) = sum of individual efficiency
                double somaOp = operadores
                    .Sum(s => heroiPorId.TryGetValue(s.HeroiId, out var h)
                        ? 1.0 + h.ObterAtributosTotais(new AtributosBase()).Get(def.AtributoReq) / 100.0
                        : 1.0);

                // If no operators, treat as 1.0 contribution (building still works with just Responsável)
                if (operadores.Count == 0) somaOp = 1.0;

                double producao = def.BaseProdPorHora * multResp * somaOp * humorMult * horasDecorridas;
                int quantidade = (int)producao;
                if (quantidade <= 0) continue;

                produzido.Adicionar(quantidade, recurso);
                cidade.Recursos.Adicionar(quantidade, recurso);
            }

            cidade.UltimaColeta = agora;
            await _cidadeRepository.AtualizarAsync(cidade);

            return (cidade, produzido);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static double CalcularHumorCidade(Cidade cidade, Dictionary<Guid, Domain.Entities.Heroi> heroiPorId)
        {
            // All heroes currently allocated (ResourceNode workers only — slot heroes tracked separately)
            var humores = cidade.Trabalhadores
                .Where(t => heroiPorId.ContainsKey(t.HeroiId))
                .Select(t => (double)heroiPorId[t.HeroiId].Humor)
                .ToList();

            // In 3A.2 all heroes start at Humor=50, so this will always return 50 → mult 1.0
            return humores.Count > 0 ? humores.Average() : 50.0;
        }

        public async Task AtualizarRecursosAsync(Guid cidadeId, string tipoRecurso, int quantidade)
        {
            var cidade = await _cidadeRepository.ObterPorIdAsync(cidadeId)
                ?? throw new InvalidOperationException("Cidade não encontrada.");
            cidade.Recursos.Adicionar(quantidade, tipoRecurso);
            await _cidadeRepository.AtualizarAsync(cidade);
        }

        public async Task<List<Cidade>> ObterTodasCidadesAsync()
            => await _cidadeRepository.ObterTodasAsync();

        // ── Slot query helpers (for commands) ────────────────────────────────────

        public async Task<List<SlotOcupacao>> ObterSlotsPorPredioAsync(Guid construcaoId)
            => await _slotRepository.ObterPorConstrucaoAsync(construcaoId);
    }
}
