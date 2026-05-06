using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Combate;
using LegendsAwaken.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services
{
    public class CombatService
    {
        private static readonly Random _random = new();

        // ── Constantes da fórmula (GDD §5.0 / DESIGN_SISTEMAS §3) ─────────────
        private const double BurstCapFactor = 0.65;   // hit único ≤ 65% do HP máximo
        private const double CritMultiplier = 1.50;
        private const double BaseCritChance = 0.05;   // 5% base; +0.1% por ponto de Percepcao

        // ── Iniciar combate ────────────────────────────────────────────────────

        public CombatEncounter IniciarCombate(List<Heroi> herois, List<InimigoAndar> inimigos)
        {
            var encounter = new CombatEncounter();

            encounter.Aliados = herois.Select(h => new Combatente
            {
                Id         = h.Id,
                Nome       = h.Nome,
                Nivel      = h.Nivel,
                Atributos  = h.ObterAtributosTotais(new AtributosBase()),
                Status     = h.Status,
                Habilidades = h.Habilidades,
                IsHeroi    = true
            }).ToList();

            encounter.Inimigos = inimigos.Select(i => new Combatente
            {
                Id        = i.Id,
                Nome      = i.Nome,
                Nivel     = i.Nivel,
                Atributos = i.Atributos,
                Status    = i.Atributos.FromAtributos(),
                IsHeroi   = false
            }).ToList();

            return encounter;
        }

        // ── Round completo ─────────────────────────────────────────────────────

        /// <summary>
        /// Executa um round completo. Ordem de ação determinada por ATB:
        /// InitScore = Agilidade + Random(0, Agilidade × 0.1)
        /// </summary>
        public void ExecutarRound(CombatEncounter enc)
        {
            enc.Round++;

            var todos = enc.Aliados.Concat(enc.Inimigos)
                .Where(c => c.Status.VidaAtual > 0)
                .Select(c => (c, init: c.Atributos.Agilidade + _random.NextDouble() * c.Atributos.Agilidade * 0.1))
                .OrderByDescending(x => x.init)
                .Select(x => x.c)
                .ToList();

            foreach (var actor in todos)
            {
                if (actor.Status.VidaAtual <= 0) continue;

                var targetList = actor.IsHeroi ? enc.Inimigos : enc.Aliados;
                var target = targetList.FirstOrDefault(t => t.Status.VidaAtual > 0);
                if (target == null) break;

                int dano = CalcularDano(actor, target, skillMult: 1.0);
                target.Status.VidaAtual = Math.Max(0, target.Status.VidaAtual - dano);
            }

            AtualizarEstadoFinal(enc);
        }

        // ── Fórmula de dano (GDD §5.0) ────────────────────────────────────────

        /// <summary>
        /// FinalDamage = ATK × SkillMult × (1 − DEF/(DEF + K)) × TypeMult
        /// K = 1000 + target.Nivel × 50
        /// Crit: BaseCritChance + Percepcao×0.1% → ×1.5
        /// Burst cap: resultado ≤ 65% do HP máximo do alvo
        /// </summary>
        internal int CalcularDano(Combatente atk, Combatente def, double skillMult, double typeMult = 1.0)
        {
            double ataque = atk.Atributos.Forca;
            double defesa = def.Atributos.Vitalidade;
            double k      = 1000.0 + def.Nivel * 50.0;

            double mitigacao = defesa / (defesa + k);
            double danoBase  = ataque * skillMult * (1.0 - mitigacao) * typeMult;

            // Crit
            double critChance = BaseCritChance + atk.Atributos.Percepcao * 0.001;
            if (_random.NextDouble() < critChance)
                danoBase *= CritMultiplier;

            // Burst cap: hit único não pode exceder 65% do HP máximo do alvo
            int burstCap  = (int)(def.Status.VidaMaxima * BurstCapFactor);
            int danoFinal = Math.Clamp((int)danoBase, 1, burstCap);

            return danoFinal;
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static void AtualizarEstadoFinal(CombatEncounter enc)
        {
            if (enc.Aliados.All(a => a.Status.VidaAtual == 0))
            {
                enc.IsFinished = true;
                enc.Winner = enc.Inimigos.FirstOrDefault(i => i.Status.VidaAtual > 0);
            }
            else if (enc.Inimigos.All(i => i.Status.VidaAtual == 0))
            {
                enc.IsFinished = true;
                enc.Winner = enc.Aliados.FirstOrDefault(a => a.Status.VidaAtual > 0);
            }
        }
    }
}
