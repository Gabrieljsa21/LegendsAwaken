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

        var agora = DateTime.UtcNow;
        var horas = Math.Min((agora - cidade.UltimoSustentoEm).TotalHours, 24.0);

        if (horas >= 0.1 && candidatos.Count > 0)
        {
            int consumo = (int)(candidatos.Count * horas);
            cidade.Recursos.Comida = Math.Max(0, cidade.Recursos.Comida - consumo);
            cidade.UltimoSustentoEm = agora;
            await cidadeRepo.AtualizarAsync(cidade);
        }
        else if (horas >= 0.1)
        {
            cidade.UltimoSustentoEm = agora;
            await cidadeRepo.AtualizarAsync(cidade);
        }

        var consumoPorHora = candidatos.Count;
        var novoEstado = ComputarEstado(cidade.Recursos.Comida, consumoPorHora);

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
        var ativos = herois.Count(h => h.EstadoSustento != EstadoSustento.Inativo);
        if (ativos == 0)
            return (0, double.MaxValue, EstadoSustento.Ativo);

        var horasRestantes = (double)cidade.Recursos.Comida / ativos;
        return (ativos, horasRestantes, ComputarEstado(cidade.Recursos.Comida, ativos));
    }

    private static EstadoSustento ComputarEstado(int comida, int consumoPorHora)
    {
        if (consumoPorHora == 0) return EstadoSustento.Ativo;
        var horas = (double)comida / consumoPorHora;
        return horas switch
        {
            >= 8.0 => EstadoSustento.Ativo,
            >= 2.0 => EstadoSustento.Instavel,
            _ => EstadoSustento.Degradado
        };
    }
}
