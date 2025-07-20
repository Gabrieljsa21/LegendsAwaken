using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot;
using LegendsAwaken.Bot.Commands;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using LegendsAwaken.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Classe principal responsável por iniciar e configurar o bot Discord.
/// </summary>
class Program
{
    private static DiscordSocketClient? _cliente;
    private static IServiceProvider? _services;
    private static string? _token;

    private static readonly ulong GUILD_ID = 1388541192806989834;

    public static Task Main(string[] args) => new Program().IniciarAsync();

    public async Task IniciarAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        _token = Environment.GetEnvironmentVariable("LegendsAwakenToken");
        if (string.IsNullOrWhiteSpace(_token))
        {
            Console.WriteLine("❌ Token não encontrado.");
            return;
        }

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent
        };
        _cliente = new DiscordSocketClient(config);
        _cliente.Log += LogAsync;

        var services = new ServiceCollection()
            .AddDbContext<LegendsAwakenDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")))

            // Repositórios
            .AddScoped<ICidadeRepository, CidadeRepository>()
            .AddScoped<ITorreRepository, TorreRepository>()
            .AddScoped<IHeroiRepository, HeroiRepository>()
            .AddScoped<IBannerHistoricoRepository, BannerHistoricoRepository>()
            .AddScoped<IUsuarioRepository, UsuarioRepository>()

            // Serviços de aplicação
            .AddScoped<GeracaoDeDadosService>()
            .AddScoped<HeroiService>()
            .AddScoped<GachaService>()
            .AddScoped<TreinamentoService>()
            .AddScoped<TorreService>()
            .AddScoped<CidadeService>()
            .AddScoped<BannerService>()
            .AddScoped<BannerHistoricoService>()
            .AddScoped<UsuarioService>()
            .AddScoped<RacaService>()
            .AddScoped<BannerCommand>()  // Comando para listar banners

            .AddSingleton(_cliente)
            .AddSingleton<IConfiguration>(configuration)

            .AddLogging(builder =>
            {
                builder
                    .AddConsole() // Exibe logs no console
                    .SetMinimumLevel(LogLevel.Error); // Define o nível mínimo de log global

                // Opções de LogLevel (do mais detalhado ao mais crítico):
                // LogLevel.Trace       → Tudo, inclusive rastreamento interno detalhado (muito verboso)
                // LogLevel.Debug       → Informações úteis para desenvolvimento e depuração
                // LogLevel.Information → Eventos normais e informativos (fluxo geral da aplicação)
                // LogLevel.Warning     → Algo inesperado, mas a aplicação continua normalmente
                // LogLevel.Error       → Erros que afetam partes da aplicação
                // LogLevel.Critical    → Erros graves que impedem o funcionamento da aplicação
                // LogLevel.None        → Nenhum log será emitido
            })


            .BuildServiceProvider();

        _services = services;

        // Login e início do bot
        await _cliente.LoginAsync(TokenType.Bot, _token);
        await _cliente.StartAsync();

        // Inicializa o manipulador de comandos
        var handler = new CommandHandler(
            _cliente,
            services.GetRequiredService<ILogger<CommandHandler>>(),
            GUILD_ID,
            services.GetRequiredService<HeroiService>(),
            services.GetRequiredService<GeracaoDeDadosService>(),
            services.GetRequiredService<BannerService>(),
            services.GetRequiredService<BannerHistoricoService>(),
            services.GetRequiredService<UsuarioService>(),
            services.GetRequiredService<GachaService>(),
            services.GetRequiredService<RacaService>()
        );

        // Eventos de botões para rolagem interativa
        _cliente.ButtonExecuted += handler.HandleButtonExecutedAsync;

        handler.Initialize();

        // Criação e população do banco de dados
        await CriarBancoEDadosBaseAsync();

        await Task.Delay(-1);
    }

    private async Task CriarBancoEDadosBaseAsync()
    {
        if (_services == null)
        {
            Console.WriteLine("❌ Serviços não inicializados.");
            return;
        }

        try
        {
            using var scope = _services.CreateScope();
            var geracaoService = scope.ServiceProvider.GetRequiredService<GeracaoDeDadosService>();
            await geracaoService.CriarTabelasAsync();
            await geracaoService.PopularDadosBaseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao preparar banco: {ex.Message}");
        }
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine($"[{log.Severity}] {log.Source}: {log.Message}");
        if (log.Exception != null)
            Console.WriteLine($"❗ Exceção: {log.Exception}");
        return Task.CompletedTask;
    }
}
