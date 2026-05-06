using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using LegendsAwaken.Infrastructure.Repositories;
using LegendsAwaken.Infrastructure.SeedData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class GeracaoDeDadosService
    {
        private readonly string _connectionString;
        private readonly LegendsAwakenDbContext _db;
        private readonly ITorreOperacaoRepository _torreOpRepo;
        private readonly ITorreExploracaoRepository _torreExploracaoRepo;
        private readonly ITorreBoosterRepository _torreBoosterRepo;
        private readonly ICidadeBoosterRepository _cidadeBoosterRepo;
        private readonly IRecursoEstoqueRepository _recursoEstoqueRepo;
        private readonly IJogadorItemRepository _jogadorItemRepo;
        private readonly ILogger<GeracaoDeDadosService> _logger;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="GeracaoDeDadosService"/>.
        /// </summary>
        /// <param name="configuration">Configuração da aplicação contendo a connection string.</param>
        /// <param name="db">Contexto do banco de dados.</param>

        public GeracaoDeDadosService(
            IConfiguration configuration,
            LegendsAwakenDbContext db,
            ITorreOperacaoRepository torreOpRepo,
            ITorreExploracaoRepository torreExploracaoRepo,
            ITorreBoosterRepository torreBoosterRepo,
            ICidadeBoosterRepository cidadeBoosterRepo,
            IRecursoEstoqueRepository recursoEstoqueRepo,
            IJogadorItemRepository jogadorItemRepo,
            ILogger<GeracaoDeDadosService> logger)
        {
            _db = db;
            _torreOpRepo = torreOpRepo;
            _torreExploracaoRepo = torreExploracaoRepo;
            _torreBoosterRepo    = torreBoosterRepo;
            _cidadeBoosterRepo   = cidadeBoosterRepo;
            _recursoEstoqueRepo  = recursoEstoqueRepo;
            _jogadorItemRepo     = jogadorItemRepo;
            _logger              = logger;
            var connection = _db.Database.GetDbConnection();
            var path = new SqliteConnectionStringBuilder(connection.ConnectionString).DataSource;

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' não encontrada.");

            // Log para debug:
            _logger.LogDebug("Banco em uso (DbContext): {ConnectionString}", connection.ConnectionString);
        }

        /// <summary>
        /// Cria todas as tabelas necessárias no banco de dados, caso não existam.
        /// </summary>
        public async Task CriarTabelasAsync()
        {
            await _db.Database.MigrateAsync();
            await EnsureAndaresColunaInimigosAsync();
            await _torreOpRepo.EnsureTableAsync();
            await _torreExploracaoRepo.EnsureTableAsync();
            await _torreBoosterRepo.EnsureTableAsync();
            await _cidadeBoosterRepo.EnsureTablesAsync();
            await _recursoEstoqueRepo.EnsureTableAsync();
            await _jogadorItemRepo.EnsureTableAsync();
            await ListarTabelasAsync();
        }

        /// <summary>
        /// Popula as tabelas do banco com os dados básicos lidos dos arquivos JSON.
        /// </summary>
        public async Task PopularDadosBaseAsync()
        {
            if (!_db.Habilidades.Any())
            {
                HabilidadesSeed.PopularHabilidades(_db);
            }

            // Remove heróis legados do sistema antigo (UsuarioId == 0)
            var legados = _db.Herois.Where(h => h.UsuarioId == 0);
            if (await legados.AnyAsync())
            {
                _db.Herois.RemoveRange(legados);
                await _db.SaveChangesAsync();
            }
        }

        // TorreAndar.Inimigos is excluded from EF (.Ignore) and stored as JSON TEXT.
        // No EF migration can add it, so we guard it here at startup.
        private async Task EnsureAndaresColunaInimigosAsync()
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var pragmaCmd = conn.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA table_info(Andares)";
            bool existe = false;
            using (var reader = await pragmaCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if (reader["name"].ToString() == "Inimigos")
                    {
                        existe = true;
                        break;
                    }
                }
            }

            if (!existe)
            {
                var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE Andares ADD COLUMN Inimigos TEXT";
                await alterCmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Migration: coluna Inimigos adicionada à tabela Andares");
            }
        }

        /// <summary>
        /// Lista todas as tabelas presentes no banco de dados atual.
        /// </summary>
        public async Task ListarTabelasAsync()
        {
            using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";

            using var reader = await cmd.ExecuteReaderAsync();
            var tabelas = new System.Collections.Generic.List<string>();
            while (await reader.ReadAsync())
                tabelas.Add(reader.GetString(0));
            _logger.LogInformation("Tabelas no banco: {Tabelas}", string.Join(", ", tabelas));
        }
    }
}
