using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;

namespace LegendsAwaken.Application.Services;

public class BiomeService(IBiomaRepository biomaRepository)
{
    private static readonly HashSet<int> _marcos = [5, 10, 15, 20, 25, 30, 40, 50, 60, 75, 100];

    public Task<Bioma?> ObterBiomaPorAndarAsync(int andar) =>
        biomaRepository.ObterPorAndarAsync(andar);

    public Task<List<BiomHeroPool>> ObterPoolDoBiomaAsync(Guid biomaId) =>
        biomaRepository.ObterPoolAsync(biomaId);

    public bool EAndarDeMarco(int andar) => _marcos.Contains(andar);

    public async Task<bool> EBiomaNovoAsync(int andarAtual)
    {
        if (andarAtual <= 1) return true;
        var biomaAtual    = await biomaRepository.ObterPorAndarAsync(andarAtual);
        var biomaAnterior = await biomaRepository.ObterPorAndarAsync(andarAtual - 1);
        return biomaAtual?.Id != biomaAnterior?.Id;
    }
}
