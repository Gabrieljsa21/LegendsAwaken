using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations;

/// <summary>
/// Rebuilds TorreExploracoes to make ChannelId nullable (INTEGER, no NOT NULL constraint).
/// Handles the case where ChannelId was previously added as NOT NULL via PendingModelSync.
/// Existing rows receive ChannelId = 0 (falls back to DM notification in NotificacaoService).
/// </summary>
public partial class ChannelIdFix : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQLite does not support ALTER COLUMN, so we rebuild the table.
        // This drops ChannelId regardless of its current constraint state,
        // then re-adds it as nullable INTEGER DEFAULT 0.
        migrationBuilder.Sql(@"
            PRAGMA foreign_keys=OFF;

            CREATE TABLE ""TorreExploracoes_tmp"" (
                ""Id""                   TEXT    NOT NULL CONSTRAINT ""PK_TorreExploracoes"" PRIMARY KEY,
                ""AndarNumero""          INTEGER NOT NULL,
                ""BoosterAtivo""         INTEGER,
                ""CheckpointInterval""   INTEGER NOT NULL,
                ""CheckpointsProcessados"" INTEGER NOT NULL,
                ""ConcluidoEm""          TEXT,
                ""ConsequenceTags""      TEXT,
                ""DiscordUserId""        INTEGER NOT NULL,
                ""HeroisFeridosIds""     TEXT    NOT NULL,
                ""HeroisIds""            TEXT    NOT NULL,
                ""IniciadoEm""           TEXT    NOT NULL,
                ""LootFragmentosHeroiId"" TEXT   NOT NULL,
                ""LootFragmentosQtd""    INTEGER NOT NULL,
                ""LootOuro""             INTEGER NOT NULL,
                ""Progresso""            REAL    NOT NULL,
                ""Seed""                 INTEGER NOT NULL,
                ""Status""               INTEGER NOT NULL,
                ""UltimoCheckpoint""     INTEGER NOT NULL,
                ""UltimoTickEm""         TEXT    NOT NULL,
                ""UsuarioId""            TEXT    NOT NULL,
                ""Version""              INTEGER NOT NULL
            );

            INSERT INTO ""TorreExploracoes_tmp"" (
                ""Id"", ""AndarNumero"", ""BoosterAtivo"", ""CheckpointInterval"",
                ""CheckpointsProcessados"", ""ConcluidoEm"", ""ConsequenceTags"", ""DiscordUserId"",
                ""HeroisFeridosIds"", ""HeroisIds"", ""IniciadoEm"", ""LootFragmentosHeroiId"",
                ""LootFragmentosQtd"", ""LootOuro"", ""Progresso"", ""Seed"", ""Status"",
                ""UltimoCheckpoint"", ""UltimoTickEm"", ""UsuarioId"", ""Version"")
            SELECT
                ""Id"", ""AndarNumero"", ""BoosterAtivo"", ""CheckpointInterval"",
                ""CheckpointsProcessados"", ""ConcluidoEm"", ""ConsequenceTags"", ""DiscordUserId"",
                ""HeroisFeridosIds"", ""HeroisIds"", ""IniciadoEm"", ""LootFragmentosHeroiId"",
                ""LootFragmentosQtd"", ""LootOuro"", ""Progresso"", ""Seed"", ""Status"",
                ""UltimoCheckpoint"", ""UltimoTickEm"", ""UsuarioId"", ""Version""
            FROM ""TorreExploracoes"";

            DROP TABLE ""TorreExploracoes"";

            ALTER TABLE ""TorreExploracoes_tmp"" RENAME TO ""TorreExploracoes"";

            ALTER TABLE ""TorreExploracoes"" ADD COLUMN ""ChannelId"" INTEGER DEFAULT 0;

            PRAGMA foreign_keys=ON;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ChannelId",
            table: "TorreExploracoes");
    }
}
