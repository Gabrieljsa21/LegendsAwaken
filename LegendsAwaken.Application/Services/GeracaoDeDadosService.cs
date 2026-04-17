using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Factories;
using LegendsAwaken.Infrastructure;
using LegendsAwaken.Infrastructure.Repositories;
using LegendsAwaken.Infrastructure.SeedData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class GeracaoDeDadosService
    {
        private readonly string _connectionString;
        private readonly LegendsAwakenDbContext _db;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="GeracaoDeDadosService"/>.
        /// </summary>
        /// <param name="configuration">Configuração da aplicação contendo a connection string.</param>
        /// <param name="db">Contexto do banco de dados.</param>

        public GeracaoDeDadosService(IConfiguration configuration, LegendsAwakenDbContext db)
        {
            _db = db;
            var connection = _db.Database.GetDbConnection();
            var path = new SqliteConnectionStringBuilder(connection.ConnectionString).DataSource;

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' não encontrada.");

            // Log para debug:
            Console.WriteLine($"[DEBUG] Banco em uso (DbContext): {connection.ConnectionString}");
        }

        /// <summary>
        /// Cria todas as tabelas necessárias no banco de dados, caso não existam.
        /// </summary>
        public async Task CriarTabelasAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();

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

            await PopularPersonagensFixosAsync();
        }

        // Fixed characters pool (UsuarioId = 0 = system-owned)
        private static readonly (string Nome, Raridade Raridade, Profissao Profissao, Raca Raca, Elemento Elemento, string Lore)[] PersonagensFixos =
        [
            ("Aldric, o Sem-Corrente",        Raridade.Estrela5, Profissao.Guerreiro,  Raca.Humano,   Elemento.Metal,    "Mercenário solitário com espada enorme que nunca serviu a nenhum mestre por vontade própria... até encontrar o Mestre."),
            ("Yuzara, a Tecelã do Destino",   Raridade.Estrela5, Profissao.Mago,       Raca.Elfo,     Elemento.Luz,      "Capaz de antever o futuro, raramente escolhe interferir. Sempre sorri como se soubesse o que está por vir."),
            ("Thorvald, o Arquiteto das Eras", Raridade.Estrela5, Profissao.Ferreiro,  Raca.Anao,     Elemento.Terra,    "Ergueu três cidades antes de ser invocado. Diz que a quarta será a mais grandiosa de todas."),
            ("Kaen",                          Raridade.Estrela4, Profissao.Arqueiro,   Raca.Humano,   Elemento.Fogo,     "Aventureiro carismático que entrou em cada batalha sorrindo. Nunca perdeu — ainda."),
            ("Nyra",                          Raridade.Estrela4, Profissao.Ladino,     Raca.Bestial,  Elemento.Ar,       "Aparece quando quer, desaparece quando bem entende. Diz que trabalha melhor sozinha, mas raramente está sozinha de verdade."),
            ("Seraph",                        Raridade.Estrela4, Profissao.Paladino,   Raca.Humano,   Elemento.Luz,      "Jovem idealista convicto de que proteger todos é possível. Ainda não foi provado errado."),
            ("Mira",                          Raridade.Estrela4, Profissao.Alquimista, Raca.Humano,   Elemento.Fogo,     "Prodígio da alquimia que transformou o laboratório da cidade em algo que nenhum mestre esperava. Teimosa. Brilhante."),
            ("Grom",                          Raridade.Estrela4, Profissao.Mineiro,    Raca.Anao,     Elemento.Terra,    "Nunca abandona uma veia de minério. Nunca. Dizem que ele encontra metal onde outros só veem pedra comum."),
            ("Hana",                          Raridade.Estrela4, Profissao.Cozinheiro, Raca.Humano,   Elemento.Natureza, "A culinária dela tem efeitos que nenhuma poção replica. O time rende 20% a mais depois do almoço dela."),
        ];

        private async Task PopularPersonagensFixosAsync()
        {
            var levelUpSvc = new HeroiLevelUpService();
            var habilidadeRepo = new HabilidadeRepository(_db);
            var habilidadeService = new HabilidadeService(habilidadeRepo);

            foreach (var p in PersonagensFixos)
            {
                // Check if already seeded by name and UsuarioId == 0
                bool existe = await _db.Herois.AnyAsync(h => h.Nome == p.Nome && h.UsuarioId == 0);
                if (existe) continue;

                int r = (int)p.Raridade;
                var atributosBase = levelUpSvc.ObterAtributosBaseParaRaridade(r)
                    + HeroiLevelUpService.BonusRacial.GetValueOrDefault(p.Raca, new AtributosBase());

                var habilidades = await HeroiService.GerarHabilidadesIniciaisAsync(p.Raridade, habilidadeService);

                var heroi = HeroiFactory.CriarHeroi(
                    usuarioId: 0,
                    nome: p.Nome,
                    raridade: p.Raridade,
                    raca: p.Raca,
                    antecedente: "Lendário",
                    afinidade: new List<HeroiAfinidadeElemental>(),
                    habilidades: habilidades,
                    atributosBase: atributosBase,
                    funcao: null);

                heroi.Profissao = p.Profissao;
                heroi.Lore = p.Lore;
                heroi.DataCriacao = DateTime.UtcNow;
                heroi.DataAlteracao = DateTime.UtcNow;

                // Set HeroiId on afinidade after hero Id is established
                var afinidade = new HeroiAfinidadeElemental
                {
                    HeroiId = heroi.Id,
                    Elemento = p.Elemento
                };
                heroi.AfinidadeElemental.Add(afinidade);

                _db.Herois.Add(heroi);
            }

            await _db.SaveChangesAsync();
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
            Console.WriteLine("Tabelas no banco:");
            while (await reader.ReadAsync())
            {
                Console.WriteLine(reader.GetString(0));
            }
        }
    }
}
