using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class SustentoService(IHeroiRepository heroiRepo, ICidadeRepository cidadeRepo)
{
    public async Task ProcessarAsync(ulong usuarioId)
    {
        var cidade = await cidadeRepo.ObterPorProprietarioIdAsync(usuarioId);
        if (cidade == null) return;

        var herois = await heroiRepo.ObterPorUsuarioIdAsync(usuarioId);
        var candidatos = herois.Where(h => h.EstadoSustento != EstadoSustento.Inativo).ToList();

        var netRate = CalcularNetRate(cidade, candidatos);
        var novoEstado = ComputarEstado(cidade.Recursos.Comida, candidatos.Count, netRate);

        foreach (var h in candidatos.Where(h => h.EstadoSustento != novoEstado))
        {
            h.EstadoSustento = novoEstado;
            await heroiRepo.AtualizarAsync(h);
        }
    }

    public async Task ToggleInativoAsync(Guid heroiId)
    {
        var heroi = await heroiRepo.ObterPorIdAsync(heroiId);
        if (heroi == null) return;
        heroi.EstadoSustento = heroi.EstadoSustento == EstadoSustento.Inativo
            ? EstadoSustento.Ativo
            : EstadoSustento.Inativo;
        await heroiRepo.AtualizarAsync(heroi);
    }

    public static (int consumoPorHora, double horasRestantes, EstadoSustento estado) ObterResumo(
        Cidade cidade, IList<Heroi> herois)
    {
        var ativos = herois.Where(h => h.EstadoSustento != EstadoSustento.Inativo).ToList();
        if (ativos.Count == 0)
            return (0, double.MaxValue, EstadoSustento.Ativo);

        double netRate = CalcularNetRate(cidade, ativos);
        double horasRestantes = netRate >= 0
            ? double.MaxValue
            : cidade.Recursos.Comida / Math.Abs(netRate);

        return (ativos.Count, horasRestantes, ComputarEstado(cidade.Recursos.Comida, ativos.Count, netRate));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Returns net food rate = production/h − consumption/h (consumption = 1 per active hero).
    private static double CalcularNetRate(Cidade cidade, IList<Heroi> ativos)
    {
        double producao = 0;
        if (!ResourceNodeConfig.BaseRates.TryGetValue(TipoResourceNode.Campo, out var campoRate))
            return -ativos.Count;

        foreach (var t in cidade.Trabalhadores.Where(t => t.ResourceNode == TipoResourceNode.Campo))
        {
            var h = ativos.FirstOrDefault(x => x.Id == t.HeroiId);
            if (h == null) continue;
            double bonus = h.Profissao.HasValue &&
                ResourceNodeConfig.ProfissaoBonus.TryGetValue((TipoResourceNode.Campo, h.Profissao.Value), out var b)
                ? b : 0.0;
            producao += campoRate.basePorHora * (1.0 + bonus);
        }

        return producao - ativos.Count;
    }

    private static EstadoSustento ComputarEstado(int comida, int consumoPorHora, double netRate)
    {
        if (consumoPorHora == 0) return EstadoSustento.Ativo;
        if (netRate >= 0) return EstadoSustento.Ativo;
        double horas = Math.Abs(netRate) > 0 ? (double)comida / Math.Abs(netRate) : 0;
        return horas switch
        {
            >= 8.0 => EstadoSustento.Ativo,
            >= 2.0 => EstadoSustento.Instavel,
            _ => EstadoSustento.Degradado
        };
    }
}
