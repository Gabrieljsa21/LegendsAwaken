using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using LegendsAwaken.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Classe principal responsável por iniciar e configurar o bot Discord.
/// </summary>
class Program
{
    private static DiscordSocketClient _cliente;         // Cliente do Discord usado para interações em tempo real
    private static IServiceProvider _services;           // Container de injeção de dependência
    private static string _token;                        // Token do bot, carregado do ambiente

    // ID da Guild (servidor) onde os comandos de slash serão registrados
    private static readonly ulong GUILD_ID = 1388541192806989834;

    /// <summary>
    /// Ponto de entrada principal do programa.
    /// </summary>
    public static Task Main(string[] args) => new Program().IniciarAsync();

    /// <summary>
    /// Inicializa o bot e seus serviços.
    /// </summary>
    public async Task IniciarAsync()
    {
        // Carrega configurações do arquivo appsettings.json e variáveis de ambiente
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Recupera o token do bot do ambiente
        _token = Environment.GetEnvironmentVariable("LegendsAwakenToken");

        if (string.IsNullOrWhiteSpace(_token))
        {
            Console.WriteLine("❌ Token não encontrado.");
            return;
        }

        // Configura o cliente do Discord
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        _cliente = new DiscordSocketClient(config);
        _cliente.Log += LogAsync; // Redireciona logs para o console

        // Registra os serviços do sistema (injeção de dependência)
        var services = new ServiceCollection()
            .AddDbContext<LegendsAwakenDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")))
            .AddScoped<ICidadeRepository, CidadeRepository>()
            .AddScoped<ITorreRepository, TorreRepository>()
            .AddScoped<GeracaoDeDadosService>()
            .AddScoped<HeroiService>()
            .AddScoped<GachaService>()
            .AddScoped<TreinamentoService>()
            .AddScoped<TorreService>()
            .AddScoped<CidadeService>()
            .AddSingleton(_cliente)
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();

        _services = services;

        // Evento disparado quando o bot estiver pronto
        _cliente.Ready += async () =>
        {
            Console.WriteLine($"✅ Bot conectado como {_cliente.CurrentUser}");
        };

        // Login e inicialização do bot
        await _cliente.LoginAsync(TokenType.Bot, _token);
        await _cliente.StartAsync();

        // Inicializa o manipulador de comandos do Discord
        var handler = new CommandHandler(_cliente, services.GetRequiredService<ILogger<CommandHandler>>(), GUILD_ID);
        handler.Initialize();

        // Cria e popula o banco de dados
        await CriarBancoEDadosBaseAsync();

        // Mantém o bot rodando indefinidamente
        await Task.Delay(-1);
    }

    /// <summary>
    /// Cria as tabelas e popula o banco com os dados iniciais.
    /// </summary>
    private async Task CriarBancoEDadosBaseAsync()
    {
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

    /// <summary>
    /// Trata os logs do cliente Discord, exibindo no console.
    /// </summary>
    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine($"[{log.Severity}] {log.Source}: {log.Message}");
        if (log.Exception != null)
            Console.WriteLine($"❗ Exceção: {log.Exception}");
        return Task.CompletedTask;
    }
}
