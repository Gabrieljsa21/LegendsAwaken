using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class HeroiAtributosResetService(
    IHeroiRepository heroiRepo,
    IHeroiPericiaRepository periciaRepo)
{
    // Heroes whose base stat total exceeds D&D max (60 base + 8 racial + spread buffer) = 80
    // still have old LA-scale stats and need migration.
    private const int MaxDndBaseStatTotal = 80;

    public async Task MigrarAsync()
    {
        var herois = await heroiRepo.ObterTodosAsync();
        foreach (var heroi in herois)
            await MigrarHeroiAsync(heroi);
    }

    private async Task MigrarHeroiAsync(Heroi heroi)
    {
        bool needsReset = heroi.AtributosBase.ToEnumerable().Sum(t => t.Valor) > MaxDndBaseStatTotal;
        if (!needsReset) return;

        heroi.AtributosBase = ProfissaoConfig.ObterDistribuicao(heroi.Profissao)
            + HeroiLevelUpService.BonusRacial.GetValueOrDefault(heroi.Raca, new AtributosBase());

        heroi.AtributosDistribuidos = new AtributosBase();
        heroi.PontosAtributosDisponiveis = heroi.Nivel / 4;

        int hp = ProfissaoConfig.CalcularHpMaximo(heroi.Profissao, heroi.Nivel, heroi.AtributosBase.Constituicao);
        heroi.Status.VidaMaxima = hp;
        heroi.Status.VidaAtual  = Math.Min(heroi.Status.VidaAtual, hp);

        heroi.DataAlteracao = DateTime.UtcNow;
        await heroiRepo.AtualizarAsync(heroi);

        var existing = await periciaRepo.ObterPorHeroiAsync(heroi.Id);
        if (existing.Count == 0 && heroi.Profissao.HasValue
            && ProfissaoConfig.ProficienciasIniciais.TryGetValue(heroi.Profissao.Value, out var profs))
        {
            var pericias = profs.Select(p => new HeroiPericia
            {
                Id              = Guid.NewGuid(),
                HeroiId         = heroi.Id,
                Pericia         = p,
                TemProficiencia = true
            }).ToList();
            await periciaRepo.AdicionarMuitosAsync(pericias);
        }
    }
}
