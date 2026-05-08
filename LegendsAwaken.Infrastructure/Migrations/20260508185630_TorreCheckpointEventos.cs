using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TorreCheckpointEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TorreEventoLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExploracaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Texto = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorreEventoLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TorreExploracao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AndarNumero = table.Column<int>(type: "INTEGER", nullable: false),
                    Progresso = table.Column<double>(type: "REAL", nullable: false),
                    UltimoCheckpoint = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckpointInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IniciadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimoTickEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HeroisIds = table.Column<string>(type: "TEXT", nullable: false),
                    BoosterAtivo = table.Column<int>(type: "INTEGER", nullable: true),
                    LootOuro = table.Column<int>(type: "INTEGER", nullable: false),
                    LootFragmentosQtd = table.Column<int>(type: "INTEGER", nullable: false),
                    LootFragmentosHeroiId = table.Column<string>(type: "TEXT", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HeroisFeridosIds = table.Column<string>(type: "TEXT", nullable: false),
                    Seed = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CheckpointsProcessados = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsequenceTags = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorreExploracao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosNotificacao",
                columns: table => new
                {
                    UsuarioId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NotificacoesAtivas = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanalPreferido = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Preferencia = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosNotificacao", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "TorreEventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExploracaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Tier = table.Column<int>(type: "INTEGER", nullable: false),
                    Raridade = table.Column<int>(type: "INTEGER", nullable: false),
                    EventoKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProgressoNoCheckpoint = table.Column<int>(type: "INTEGER", nullable: false),
                    AndarOrigem = table.Column<int>(type: "INTEGER", nullable: false),
                    EventoSeed = table.Column<int>(type: "INTEGER", nullable: false),
                    ResultadoSchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    OpcaoKey = table.Column<string>(type: "TEXT", nullable: true),
                    ResultadoJson = table.Column<string>(type: "TEXT", nullable: true),
                    SnapshotCombatStateJson = table.Column<string>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvidoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProcessadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorreEventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorreEventos_TorreExploracao_ExploracaoId",
                        column: x => x.ExploracaoId,
                        principalTable: "TorreExploracao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TorreEventos_ExploracaoId",
                table: "TorreEventos",
                column: "ExploracaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TorreEventoLogs");

            migrationBuilder.DropTable(
                name: "TorreEventos");

            migrationBuilder.DropTable(
                name: "UsuariosNotificacao");

            migrationBuilder.DropTable(
                name: "TorreExploracao");
        }
    }
}
