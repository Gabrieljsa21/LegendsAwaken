using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class RecruitmentService(
    IHeroiDesbloqueadoRepository desbloqueadoRepository,
    IHeroiConfigRepository heroiConfigRepository,
    IFragmentoRepository fragmentoRepository)
{
    public async Task<RecruitmentResult> TentarRecrutarPorFragmentosAsync(Guid usuarioId, Guid heroiId)
    {
        if (await desbloqueadoRepository.JaDesbloqueadoAsync(usuarioId, heroiId))
            return new RecruitmentResult(false, null, "Herói já desbloqueado.");

        var heroi = await heroiConfigRepository.ObterPorIdAsync(heroiId);
        if (heroi is null)
            return new RecruitmentResult(false, null, "Herói não encontrado.");

        var unlock = await heroiConfigRepository.ObterUnlockConfigAsync(heroiId);
        if (unlock is null || unlock.TipoUnlock != TipoUnlock.Fragmentos)
            return new RecruitmentResult(false, heroi, $"{heroi.Nome} não é desbloqueável por fragmentos.");

        var progresso = await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId);
        int atual = progresso?.Quantidade ?? 0;
        int necessario = unlock.QuantidadeFragmentos!.Value;

        if (atual < necessario)
            return new RecruitmentResult(false, heroi, $"Fragmentos insuficientes: {atual}/{necessario}.");

        await Desbloquear(usuarioId, heroiId, heroi);
        return new RecruitmentResult(true, heroi, $"{heroi.Nome} recrutado com sucesso!");
    }

    public async Task<RecruitmentResult?> ProcessarMarcoTorreAsync(Guid usuarioId, int andar)
    {
        var todosHerois = await heroiConfigRepository.ListarTodosAsync();

        foreach (var heroi in todosHerois)
        {
            var unlock = await heroiConfigRepository.ObterUnlockConfigAsync(heroi.Id);
            if (unlock?.TipoUnlock != TipoUnlock.MarcoTorre || unlock.AndarMarco != andar)
                continue;

            if (await desbloqueadoRepository.JaDesbloqueadoAsync(usuarioId, heroi.Id))
                continue;

            await Desbloquear(usuarioId, heroi.Id, heroi);
            return new RecruitmentResult(true, heroi, $"{heroi.Nome} se une à sua equipe!");
        }

        return null;
    }

    public async Task<RecruitmentResult> DesbloquearPorCondicaoAsync(Guid usuarioId, Guid heroiId)
    {
        if (await desbloqueadoRepository.JaDesbloqueadoAsync(usuarioId, heroiId))
            return new RecruitmentResult(false, null, "Herói já desbloqueado.");

        var heroi = await heroiConfigRepository.ObterPorIdAsync(heroiId);
        if (heroi is null)
            return new RecruitmentResult(false, null, "Herói não encontrado.");

        await Desbloquear(usuarioId, heroiId, heroi);
        return new RecruitmentResult(true, heroi, $"{heroi.Nome} revelou-se a você!");
    }

    private async Task Desbloquear(Guid usuarioId, Guid heroiId, HeroiConfig heroi)
    {
        await desbloqueadoRepository.SalvarAsync(new HeroiDesbloqueado
        {
            UsuarioId = usuarioId,
            HeroiId = heroiId,
            Heroi = heroi,
            DesbloqueadoEm = DateTime.UtcNow
        });
    }
}
