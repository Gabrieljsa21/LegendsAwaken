using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class CidadeBoosterService(ICidadeBoosterRepository repo)
{
    private static readonly IReadOnlyDictionary<TipoBoosterCidade, double> Duracoes =
        new Dictionary<TipoBoosterCidade, double>
        {
            [TipoBoosterCidade.Producao]       = 4.0,
            [TipoBoosterCidade.Rendimento]     = 4.0,
            [TipoBoosterCidade.Eficiencia]     = 6.0,
            [TipoBoosterCidade.Qualidade]      = 4.0,
            [TipoBoosterCidade.Especializacao] = 4.0,
            [TipoBoosterCidade.Conversao]      = 4.0,
        };

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<CidadeBoosterAtivo?> ObterAtivoAsync(ulong usuarioId)
    {
        var ativo = await repo.ObterAtivoAsync(usuarioId);
        if (ativo == null) return null;
        if (ativo.ExpiraEm <= DateTime.UtcNow)
        {
            await repo.DesativarAsync(usuarioId);
            return null;
        }
        return ativo;
    }

    public Task<List<(TipoBoosterCidade Tipo, int Quantidade)>> ObterInventarioAsync(ulong usuarioId)
        => repo.ListarInventarioAsync(usuarioId);

    // ── Activation ────────────────────────────────────────────────────────────

    public async Task<(bool Sucesso, string? Erro)> AtivarAsync(ulong usuarioId, TipoBoosterCidade tipo)
    {
        int qtd = await repo.ObterQuantidadeAsync(usuarioId, tipo);
        if (qtd <= 0)
            return (false, $"Você não possui **{NomeBooster(tipo)}** no inventário.");

        bool consumido = await repo.ConsumirInventarioAsync(usuarioId, tipo);
        if (!consumido)
            return (false, "Booster não disponível.");

        await repo.DesativarAsync(usuarioId);

        double horas = Duracoes.TryGetValue(tipo, out var h) ? h : 4.0;
        await repo.SalvarAtivoAsync(new CidadeBoosterAtivo
        {
            Id        = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo      = tipo,
            AtivadoEm = DateTime.UtcNow,
            ExpiraEm  = DateTime.UtcNow.AddHours(horas),
        });
        return (true, null);
    }

    public Task AdicionarAoInventarioAsync(ulong usuarioId, TipoBoosterCidade tipo, int qtd)
        => repo.AdicionarInventarioAsync(usuarioId, tipo, qtd);

    // ── Static effect helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Production/output multiplier applied to both node and building yields.
    /// Eficiencia and Conversao act differently (consumption reduction / bonus chance).
    /// </summary>
    public static double GetMultiplicador(CidadeBoosterAtivo? b) => b?.Tipo switch
    {
        TipoBoosterCidade.Producao       => 1.30,
        TipoBoosterCidade.Rendimento     => 1.25,
        TipoBoosterCidade.Qualidade      => 1.15,
        TipoBoosterCidade.Especializacao => 1.40,
        _ => 1.0
    };

    // ── Display helpers ───────────────────────────────────────────────────────

    public static string NomeBooster(TipoBoosterCidade tipo) => tipo switch
    {
        TipoBoosterCidade.Producao       => "Booster de Produção",
        TipoBoosterCidade.Rendimento     => "Booster de Rendimento",
        TipoBoosterCidade.Eficiencia     => "Booster de Eficiência",
        TipoBoosterCidade.Qualidade      => "Booster de Qualidade",
        TipoBoosterCidade.Especializacao => "Booster de Especialização",
        TipoBoosterCidade.Conversao      => "Booster de Conversão",
        _ => "Booster"
    };

    public static string IconeBooster(TipoBoosterCidade tipo) => tipo switch
    {
        TipoBoosterCidade.Producao       => "⚡",
        TipoBoosterCidade.Rendimento     => "📈",
        TipoBoosterCidade.Eficiencia     => "🔋",
        TipoBoosterCidade.Qualidade      => "✨",
        TipoBoosterCidade.Especializacao => "🎯",
        TipoBoosterCidade.Conversao      => "🔄",
        _ => "🧪"
    };

    public static string DescricaoBooster(TipoBoosterCidade tipo) => tipo switch
    {
        TipoBoosterCidade.Producao       => "+30% produção (4h)",
        TipoBoosterCidade.Rendimento     => "+25% output (4h)",
        TipoBoosterCidade.Eficiencia     => "-20% consumo de sustento (6h)",
        TipoBoosterCidade.Qualidade      => "+15% produção de recursos (4h)",
        TipoBoosterCidade.Especializacao => "+40% todos os coletores (4h)",
        TipoBoosterCidade.Conversao      => "10% de Ouro bônus na coleta (4h)",
        _ => ""
    };
}
