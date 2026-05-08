using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Combate;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services
{
    public class CombatService
    {
        private static Random _random => Random.Shared;

        // ── Constantes da fórmula (GDD §5.0 / DESIGN_SISTEMAS §3) ─────────────
        private const double BurstCapFactor = 0.65;   // hit único ≤ 65% do HP máximo
        private const double CritMultiplier = 1.50;
        private const double BaseCritChance = 0.05;   // 5% base; +1% per WIS modifier (MOD_WIS)

        // ── Iniciar combate ────────────────────────────────────────────────────

        public CombatEncounter IniciarCombate(List<Heroi> herois, List<InimigoAndar> inimigos)
        {
            var encounter = new CombatEncounter();

            // Leadership: hero with highest CHA adds MOD_CHA×1% to all party effective attrs
            double liderancaMult = 1.0;
            if (herois.Count > 0)
            {
                int maxCha = herois.Max(h => h.ObterAtributosTotais(new AtributosBase()).Carisma);
                int modCha = (int)Math.Floor((maxCha - 10.0) / 2.0);
                if (modCha > 0) liderancaMult = 1.0 + modCha * 0.01;
            }

            encounter.Aliados = herois.Select(h => {
                var totais = h.ObterAtributosTotais(new AtributosBase());
                var withLeadership = new AtributosBase();
                foreach (var attr in System.Enum.GetValues<Atributo>())
                    withLeadership.Set(attr, (int)(totais.Get(attr) * liderancaMult));
                return new Combatente
                {
                    Id          = h.Id,
                    Nome        = h.Nome,
                    Nivel       = h.Nivel,
                    Atributos   = withLeadership,
                    Status      = h.Status,
                    Habilidades = h.Habilidades,
                    IsHeroi     = true
                };
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
                .Select(c => (c, init: c.Atributos.Destreza + _random.NextDouble() * c.Atributos.Destreza * 0.1))
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
        public int CalcularDano(Combatente atk, Combatente def, double skillMult, double typeMult = 1.0)
        {
            double ataque = atk.Atributos.Forca;
            double defesa = def.Atributos.Constituicao;
            double k      = 1000.0 + def.Nivel * 50.0;

            double mitigacao = defesa / (defesa + k);
            double danoBase  = ataque * skillMult * (1.0 - mitigacao) * typeMult;

            // Crit — use WIS modifier (+1% per modifier point, clamped to 0 minimum)
            int modWis = (int)Math.Floor((atk.Atributos.Sabedoria - 10.0) / 2.0);
            double critChance = Math.Max(0, BaseCritChance + modWis * 0.01);
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
