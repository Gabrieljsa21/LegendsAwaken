using LegendsAwaken.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public sealed class TorreFlagService(IAndarFlagProgressoRepository repo)
{
    public async Task GerarFlagAsync(Guid userId, int andar, string flagNome) =>
        await repo.GerarFlagAsync(userId, andar, flagNome);

    public async Task MarcarSecundarioExpiradoAsync(Guid userId, int andar)
    {
        var andarDef = TorreArcoConfig.ObterAndar(andar);
        if (andarDef?.ObjetivoSecundario is not { } obj) return;
        await repo.MarcarExpiradoAsync(userId, andar, obj.FlagNome);
    }

    public async Task<IReadOnlyList<string>> ObterFlagsAtivasAsync(Guid userId) =>
        await repo.ObterFlagsGeradasAsync(userId);

    public async Task<IReadOnlyList<string>> ObterFlagsCompostasAtivasAsync(Guid userId)
    {
        var ativas = (await repo.ObterFlagsGeradasAsync(userId)).ToHashSet();
        return TorreArcoConfig.FlagsCompostas
            .Where(fc => fc.ComponentesNecessarios.All(c => ativas.Contains(c)))
            .Select(fc => fc.NomeComposta)
            .ToList();
    }

    public async Task<(double TotalHpReduction, IReadOnlyList<string> Descricoes)>
        ObterModificadoresBossAsync(Guid userId, int andar)
    {
        var andarDef = TorreArcoConfig.ObterAndar(andar);
        if (andarDef is null || andarDef.ModificadoresBoss.Count == 0)
            return (0, []);

        var ativas = (await repo.ObterFlagsGeradasAsync(userId)).ToHashSet();
        var compostas = await ObterFlagsCompostasAtivasAsync(userId);
        foreach (var c in compostas) ativas.Add(c);

        var aplicados = andarDef.ModificadoresBoss
            .Where(m => ativas.Contains(m.FlagNome))
            .ToList();

        var totalReduction = Math.Min(aplicados.Sum(m => m.HpReductionPercent), 0.50);
        var descricoes = aplicados.Select(m => m.EfeitoDescricao).ToList();
        return (totalReduction, descricoes);
    }
}
