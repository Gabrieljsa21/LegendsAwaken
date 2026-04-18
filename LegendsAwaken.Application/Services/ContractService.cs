using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class ContractService(
    IContratoRepository contratoRepository,
    IHeroiConfigRepository heroiConfigRepository,
    IFragmentoRepository fragmentoRepository)
{
    public async Task<Contrato> AtivarContratoArquetipoAsync(Guid usuarioId, Profissao arquetipo)
    {
        var ativo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        if (ativo is not null)
            await contratoRepository.DesativarAsync(ativo.Id);

        var agora = DateTime.UtcNow;
        var contrato = new Contrato
        {
            Id        = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo      = TipoContrato.Arquetipo,
            Arquetipo = arquetipo,
            Ativo     = true,
            ExpiraEm  = null,
            CriadoEm  = agora
        };

        await contratoRepository.SalvarAsync(contrato);
        return contrato;
    }

    public async Task<RecruitmentResult> AtivarContratoNomeadoAsync(Guid usuarioId, Guid heroiId,
        TimeSpan? duracao = null)
    {
        var heroi = await heroiConfigRepository.ObterPorIdAsync(heroiId);
        if (heroi is null)
            return new RecruitmentResult(false, null, "Herói não encontrado.");

        var progresso = await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId);
        if (progresso is null || progresso.Quantidade == 0)
            return new RecruitmentResult(false, heroi,
                $"Você precisa ter ao menos 1 fragmento de {heroi.Nome} para focar nele.");

        var ativo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);
        if (ativo is not null)
            await contratoRepository.DesativarAsync(ativo.Id);

        var agora = DateTime.UtcNow;
        var duracaoEfetiva = duracao ?? ContractConfig.DuracaoPadraoNomeado;
        var contrato = new Contrato
        {
            Id        = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo      = TipoContrato.Nomeado,
            HeroiId   = heroiId,
            Ativo     = true,
            ExpiraEm  = agora.Add(duracaoEfetiva),
            CriadoEm  = agora
        };

        await contratoRepository.SalvarAsync(contrato);
        return new RecruitmentResult(true, heroi,
            $"Contrato de foco ativado para {heroi.Nome} por {duracaoEfetiva.TotalHours:0}h.");
    }

    public async Task ExpirarContratosVencidosAsync()
    {
        var vencidos = await contratoRepository.ListarAtivosVencidosAsync(DateTime.UtcNow);
        foreach (var contrato in vencidos)
            await contratoRepository.DesativarAsync(contrato.Id);
    }
}
