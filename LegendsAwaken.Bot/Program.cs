using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Application.Services;
using System.IO;
using LegendsAwaken.Bot;
using LegendsAwaken.Bot.Commands;
using LegendsAwaken.Bot.Interactions;
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

    public static Task Main(string[] args) => new Program().IniciarAsync();

    public async Task IniciarAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var guildIdStr = configuration["Discord:GuildId"]
            ?? throw new InvalidOperationException("Discord:GuildId não configurado em appsettings.json.");
        var GUILD_ID = ulong.Parse(guildIdStr);

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
            .AddScoped<ITorreRepository>(sp =>
                new TorreRepository(configuration.GetConnectionString("DefaultConnection")!))
            .AddScoped<ITorreOperacaoRepository>(sp =>
                new TorreOperacaoRepository(configuration.GetConnectionString("DefaultConnection")!))
            .AddScoped<ITorreExploracaoRepository>(sp =>
                new TorreExploracaoRepository(configuration.GetConnectionString("DefaultConnection")!))
            .AddScoped<ITorreBoosterRepository>(sp =>
                new TorreBoosterRepository(configuration.GetConnectionString("DefaultConnection")!))
            .AddScoped<ICidadeBoosterRepository>(sp =>
                new CidadeBoosterRepository(configuration.GetConnectionString("DefaultConnection")!))
            .AddScoped<IRecursoEstoqueRepository>(sp =>
                new RecursoEstoqueRepository(configuration.GetConnectionString("DefaultConnection")!))
            .AddScoped<IJogadorItemRepository>(sp =>
                new JogadorItemRepository(configuration.GetConnectionString("DefaultConnection")!))
            .AddScoped<IHeroiRepository, HeroiRepository>()
            .AddScoped<IUsuarioRepository, UsuarioRepository>()
            .AddScoped<IHabilidadeRepository, HabilidadeRepository>()
            .AddScoped<IAtributoBonusService, AtributoBonusService>()
            .AddScoped<IPartyRepository, PartyRepository>()
            .AddScoped<IItemRepository, ItemRepository>()
            .AddScoped<ISlotOcupacaoRepository, SlotOcupacaoRepository>()
            .AddScoped<IBiomaRepository, BiomaRepository>()
            .AddScoped<IFragmentoRepository, FragmentoRepository>()
            .AddScoped<IContratoRepository, ContratoRepository>()
            .AddScoped<IHeroiDesbloqueadoRepository, HeroiDesbloqueadoRepository>()
            .AddScoped<IHeroiConfigRepository, HeroiConfigRepository>()
            .AddScoped<IAndarFlagProgressoRepository>(sp =>
                new AndarFlagProgressoRepository(configuration.GetConnectionString("DefaultConnection")!))

            // Serviços de aplicação
            .AddScoped<HeroiLevelUpService>()
            .AddScoped<GeracaoDeDadosService>()
            .AddScoped<HeroiService>()
            .AddScoped<TreinamentoService>()
            .AddScoped<TorreService>()
            .AddScoped<TorreOperacaoService>()
            .AddScoped<SustentoService>()
            .AddScoped<CidadeBoosterService>()
            .AddScoped<CidadeService>()
            .AddScoped<UsuarioService>()
            .AddScoped<RacaService>()
            .AddScoped<HabilidadeService>()
            .AddScoped<AtributoBonusService>()
            .AddScoped<CombatService>()
            .AddScoped<PartyService>()
            .AddScoped<CraftingService>()
            .AddScoped<ArenaService>()
            .AddScoped<BiomeService>()
            .AddScoped<FragmentService>()
            .AddScoped<ContractService>()
            .AddScoped<RecruitmentService>()
            .AddScoped<RewardDistributionService>()
            .AddScoped<TorreExploracaoService>()
            .AddScoped<TorreFlagService>()
            .AddScoped<RecursoService>()
            .AddScoped<JogadorItemService>()
            .AddScoped<HeroiDataLoader>()
            .AddSingleton<R2ImageService>()
            .AddSingleton<InteractionRouter>()

            .AddSingleton(_cliente)
            .AddSingleton<IConfiguration>(configuration)

            .AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
                // Silencia ruído dos frameworks
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                builder.AddFilter("Microsoft.Extensions",          LogLevel.Warning);
                builder.AddFilter("System.Net.Http",               LogLevel.Warning);
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
            services.GetRequiredService<UsuarioService>(),
            services.GetRequiredService<RacaService>(),
            services.GetRequiredService<AtributoBonusService>(),
            services.GetRequiredService<CombatService>(),
            services.GetRequiredService<PartyService>(),
            services.GetRequiredService<CidadeService>(),
            services.GetRequiredService<CidadeBoosterService>(),
            services.GetRequiredService<CraftingService>(),
            services.GetRequiredService<ArenaService>(),
            services.GetRequiredService<IHeroiConfigRepository>(),
            services.GetRequiredService<IHeroiDesbloqueadoRepository>(),
            services.GetRequiredService<IFragmentoRepository>(),
            services.GetRequiredService<RecruitmentService>(),
            services.GetRequiredService<BiomeService>(),
            services.GetRequiredService<ContractService>(),
            services.GetRequiredService<IContratoRepository>(),
            services.GetRequiredService<ITorreRepository>(),
            services.GetRequiredService<TorreService>(),
            services.GetRequiredService<TorreOperacaoService>(),
            services.GetRequiredService<TorreExploracaoService>(),
            services.GetRequiredService<SustentoService>(),
            services.GetRequiredService<RecursoService>(),
            services.GetRequiredService<JogadorItemService>(),
            services.GetRequiredService<R2ImageService>(),
            services.GetRequiredService<InteractionRouter>(),
            services.GetRequiredService<TorreFlagService>()
        );

        handler.Initialize();

        // Register IInteractionHandler implementations with the router
        var interactionRouter = services.GetRequiredService<InteractionRouter>();
        var cidadeCommand = new CidadeCommand(
            services.GetRequiredService<CidadeService>(),
            services.GetRequiredService<HeroiService>(),
            services.GetRequiredService<CidadeBoosterService>(),
            services.GetRequiredService<ILogger<CidadeCommand>>()
        );
        interactionRouter.Register(cidadeCommand);

        await handler.SetupCommandsAsync();

        // Criação e população do banco de dados
        await CriarBancoEDadosBaseAsync();

        // Pre-warm the shared DbContext so the first slash command doesn't hit 3s timeout
        try
        {
            await services.GetRequiredService<LegendsAwakenDbContext>().Database.ExecuteSqlRawAsync("SELECT 1");
        }
        catch { }

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

            var heroiLoader = scope.ServiceProvider.GetRequiredService<HeroiDataLoader>();
            var heroesJsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "heroes.json");
            await heroiLoader.SincronizarAsync(heroesJsonPath);
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
