using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Services;

public class TorreTicker(IServiceScopeFactory scopeFactory, ILogger<TorreTicker> logger)
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(1);
    private Timer? _timer;

    public void Start()
    {
        _timer = new Timer(Tick, null, Intervalo, Intervalo);
        logger.LogInformation("[TorreTicker] Iniciado — tick a cada {Min} minuto(s).", (int)Intervalo.TotalMinutes);
    }

    public void Stop() => _timer?.Dispose();

    private async void Tick(object? _)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repo            = scope.ServiceProvider.GetRequiredService<ITorreExploracaoRepository>();
            var exploracaoSvc   = scope.ServiceProvider.GetRequiredService<TorreExploracaoService>();

            var ativas = await repo.ObterTodasAtivasAsync();
            if (ativas.Count == 0) return;

            logger.LogInformation("[TorreTicker] Processando {N} exploração(ões) ativa(s).", ativas.Count);

            foreach (var exp in ativas)
            {
                try
                {
                    await exploracaoSvc.ProcessarAsync(exp.UsuarioId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[TorreTicker] Erro ao processar exploração {Id}.", exp.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[TorreTicker] Erro no tick global.");
        }
    }
}
