using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Combate;
using LegendsAwaken.Domain.Extensions;
using System.Linq;

namespace LegendsAwaken.Application.Services
{
    public class CombatService
    {
        // 2.1) Iniciar combate: transforma Heroi/Inimigo em Combatente
        public CombatEncounter IniciarCombate(List<Heroi> herois, List<Inimigo> inimigos)
        {
            var encounter = new CombatEncounter();
            encounter.Aliados = herois.Select(h => new Combatente
            {
                Id = h.Id,
                Nome = h.Nome,
                Atributos = h.ObterAtributosTotais(new AtributosBase()),
                Status = h.Status,
                Habilidades = h.Habilidades,
                IsHeroi = true
            }).ToList();

            encounter.Inimigos = inimigos.Select(i => new Combatente
            {
                Id = i.Id,
                Nome = i.Nome,
                Atributos = i.Atributos,
                Status = i.Atributos.FromAtributos(),
                Habilidades = null,
                IsHeroi = false
            }).ToList();

            // opcional: ordenar por iniciativa = Agilidade + Percepção/2
            return encounter;
        }

        // 2.2) Executar um round completo
        public void ExecutarRound(CombatEncounter enc)
        {
            enc.Round++;
            var todos = enc.Aliados.Concat(enc.Inimigos)
                .Where(c => c.Status.VidaAtual > 0)
                .OrderByDescending(c => c.Atributos.Agilidade + c.Atributos.Percepcao / 2)
                .ToList();

            foreach (var actor in todos)
            {
                if (actor.Status.VidaAtual <= 0) continue;
                var targetList = actor.IsHeroi ? enc.Inimigos : enc.Aliados;
                var target = targetList.FirstOrDefault(t => t.Status.VidaAtual > 0);
                if (target == null) break;

                // ação: se tiver habilidade, usa; se não, ataque básico
                int dano = actor.Habilidades != null && actor.Habilidades.Any()
                    ? CalcularDanoFisico(actor, target)
                    : CalcularDanoFisico(actor, target);

                target.Status.VidaAtual = Math.Max(0, target.Status.VidaAtual - dano);
            }

            // checar fim
            if (enc.Aliados.All(a => a.Status.VidaAtual == 0))
            {
                enc.IsFinished = true;
                enc.Winner = enc.Inimigos.FirstOrDefault();
            }
            else if (enc.Inimigos.All(i => i.Status.VidaAtual == 0))
            {
                enc.IsFinished = true;
                enc.Winner = enc.Aliados.FirstOrDefault();
            }
        }

        private int CalcularDanoFisico(Combatente atk, Combatente def)
        {
            var baseDmg = (int)(atk.Atributos.Forca * 1.5);
            var mitig = (int)(def.Atributos.Vitalidade * 0.5);
            return Math.Max(baseDmg - mitig, 1);
        }
    }
}
