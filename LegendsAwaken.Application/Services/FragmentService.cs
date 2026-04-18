using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class FragmentService(
    IBiomaRepository biomaRepository,
    IFragmentoRepository fragmentoRepository,
    IContratoRepository contratoRepository,
    IHeroiConfigRepository heroiConfigRepository)
{
    public async Task<List<FragmentDropResult>> ProcessarDropAsync(Guid usuarioId, int andar)
    {
        var bioma = await biomaRepository.ObterPorAndarAsync(andar);
        if (bioma is null) return [];

        var pool = await biomaRepository.ObterPoolAsync(bioma.Id);
        if (pool.Count == 0) return [];

        if (Random.Shared.Next(100) >= ContractConfig.ChanceDropBase) return [];

        var heroiSelecionado = SelecionarPorPeso(pool);
        if (heroiSelecionado is null) return [];

        int quantidade      = Random.Shared.Next(1, 4);
        float mult          = await ObterMultiplicadorAsync(usuarioId, heroiSelecionado.HeroiId);
        int quantidadeFinal = (int)Math.Ceiling(quantidade * mult);

        await AdicionarFragmentosAsync(usuarioId, TipoFragmento.Heroi, heroiSelecionado.HeroiId, quantidadeFinal);

        var progresso = await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiSelecionado.HeroiId)
            ?? throw new InvalidOperationException($"FragmentoProgresso not found after upsert for hero {heroiSelecionado.HeroiId}");

        return
        [
            new FragmentDropResult(
                heroiSelecionado.HeroiId,
                heroiSelecionado.Heroi?.Nome ?? heroiSelecionado.HeroiId.ToString(),
                TipoFragmento.Heroi,
                quantidadeFinal,
                progresso.Quantidade)
        ];
    }

    public async Task AdicionarFragmentosAsync(Guid usuarioId, TipoFragmento tipo, Guid? heroiId, int quantidade)
    {
        FragmentoProgresso? progresso = tipo == TipoFragmento.Heroi && heroiId.HasValue
            ? await fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId.Value)
            : null;

        if (progresso is null)
        {
            progresso = new FragmentoProgresso
            {
                Id            = Guid.NewGuid(),
                UsuarioId     = usuarioId,
                TipoFragmento = tipo,
                HeroiId       = heroiId,
                Quantidade    = 0,
                AtualizadoEm  = DateTime.UtcNow
            };
        }

        progresso.Quantidade   += quantidade;
        progresso.AtualizadoEm  = DateTime.UtcNow;
        await fragmentoRepository.UpsertAsync(progresso);
    }

    public Task<FragmentoProgresso?> ObterProgressoAsync(Guid usuarioId, Guid heroiId) =>
        fragmentoRepository.ObterPorHeroiAsync(usuarioId, heroiId);

    public async Task<float> ObterMultiplicadorAsync(Guid usuarioId, Guid heroiId)
    {
        float mult = 1.0f;

        var config = await heroiConfigRepository.ObterPorIdAsync(heroiId);

        var contratoArquetipo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        if (contratoArquetipo is not null && config is not null && contratoArquetipo.Arquetipo == config.Arquetipo)
            mult += ContractConfig.ArchetypeBonus;

        var contratoNomeado = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);
        if (contratoNomeado is not null && contratoNomeado.HeroiId == heroiId)
            mult += ContractConfig.NamedBonus;

        return mult;
    }

    private static BiomHeroPool? SelecionarPorPeso(List<BiomHeroPool> pool)
    {
        int totalPeso  = pool.Sum(p => p.DropWeight);
        if (totalPeso <= 0) return pool.Count > 0 ? pool[0] : null;
        int roll       = Random.Shared.Next(totalPeso);
        int acumulado  = 0;
        foreach (var item in pool)
        {
            acumulado += item.DropWeight;
            if (roll < acumulado) return item;
        }
        return pool[^1];
    }
}
