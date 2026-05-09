using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <summary>
    /// Consolidates the TorreExploracao (singular) → TorreExploracoes (plural) rename and
    /// ChannelId addition into a single idempotent raw-SQL migration.
    ///
    /// The previous EF Core operations (RenameTable + AddColumn + DropForeignKey + AddForeignKey)
    /// failed when the raw-SQL EnsureTableAsync had already created "TorreExploracoes" before
    /// migrations ran, leaving "TorreExploracao" and "TorreExploracoes" as two separate tables
    /// and the FK in TorreEventos still pointing to the old singular name.
    ///
    /// This rewrite handles all DB states idempotently via raw SQL.
    /// </summary>
    public partial class PendingModelSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                PRAGMA foreign_keys=OFF;

                -- Ensure TorreExploracoes (plural, canonical) exists with all columns + ChannelId.
                -- CREATE TABLE IF NOT EXISTS is a no-op when the table already exists.
                CREATE TABLE IF NOT EXISTS ""TorreExploracoes"" (
                    ""Id""                    TEXT    NOT NULL CONSTRAINT ""PK_TorreExploracoes"" PRIMARY KEY,
                    ""AndarNumero""           INTEGER NOT NULL,
                    ""BoosterAtivo""          INTEGER,
                    ""ChannelId""             INTEGER NOT NULL DEFAULT 0,
                    ""CheckpointInterval""    INTEGER NOT NULL,
                    ""CheckpointsProcessados"" INTEGER NOT NULL,
                    ""ConcluidoEm""           TEXT,
                    ""ConsequenceTags""       TEXT,
                    ""DiscordUserId""         INTEGER NOT NULL,
                    ""HeroisFeridosIds""      TEXT    NOT NULL,
                    ""HeroisIds""             TEXT    NOT NULL,
                    ""IniciadoEm""            TEXT    NOT NULL,
                    ""LootFragmentosHeroiId"" TEXT    NOT NULL,
                    ""LootFragmentosQtd""     INTEGER NOT NULL,
                    ""LootOuro""              INTEGER NOT NULL,
                    ""Progresso""             REAL    NOT NULL,
                    ""Seed""                  INTEGER NOT NULL,
                    ""Status""                INTEGER NOT NULL,
                    ""UltimoCheckpoint""      INTEGER NOT NULL,
                    ""UltimoTickEm""          TEXT    NOT NULL,
                    ""UsuarioId""             TEXT    NOT NULL,
                    ""Version""               INTEGER NOT NULL
                );

                -- Drop TorreExploracao (singular) — this was the old EF-managed table.
                -- Any rows it contained are stale (the raw-SQL repo wrote to TorreExploracoes).
                DROP TABLE IF EXISTS ""TorreExploracao"";

                -- Rebuild TorreEventos so its FK references TorreExploracoes (plural).
                -- SQLite cannot DROP CONSTRAINT, so we rebuild the table.
                DROP TABLE IF EXISTS ""TorreEventos_migr"";
                CREATE TABLE ""TorreEventos_migr"" (
                    ""Id""                      TEXT    NOT NULL CONSTRAINT ""PK_TorreEventos"" PRIMARY KEY,
                    ""AndarOrigem""             INTEGER NOT NULL,
                    ""CriadoEm""                TEXT    NOT NULL,
                    ""EventoKey""               TEXT    NOT NULL,
                    ""EventoSeed""              INTEGER NOT NULL,
                    ""ExpiraEm""                TEXT,
                    ""ExploracaoId""            TEXT    NOT NULL,
                    ""OpcaoKey""                TEXT,
                    ""ProcessadoEm""            TEXT,
                    ""ProgressoNoCheckpoint""   INTEGER NOT NULL,
                    ""Raridade""                INTEGER NOT NULL,
                    ""ResolvidoEm""             TEXT,
                    ""ResultadoJson""           TEXT,
                    ""ResultadoSchemaVersion""  INTEGER NOT NULL,
                    ""SnapshotCombatStateJson"" TEXT,
                    ""Status""                  INTEGER NOT NULL,
                    ""Tier""                    INTEGER NOT NULL,
                    ""Tipo""                    INTEGER NOT NULL,
                    CONSTRAINT ""FK_TorreEventos_TorreExploracoes_ExploracaoId""
                        FOREIGN KEY (""ExploracaoId"") REFERENCES ""TorreExploracoes"" (""Id"") ON DELETE CASCADE
                );

                INSERT OR IGNORE INTO ""TorreEventos_migr"" (
                    ""Id"", ""AndarOrigem"", ""CriadoEm"", ""EventoKey"", ""EventoSeed"",
                    ""ExpiraEm"", ""ExploracaoId"", ""OpcaoKey"", ""ProcessadoEm"",
                    ""ProgressoNoCheckpoint"", ""Raridade"", ""ResolvidoEm"", ""ResultadoJson"",
                    ""ResultadoSchemaVersion"", ""SnapshotCombatStateJson"", ""Status"", ""Tier"", ""Tipo""
                )
                SELECT
                    ""Id"", ""AndarOrigem"", ""CriadoEm"", ""EventoKey"", ""EventoSeed"",
                    ""ExpiraEm"", ""ExploracaoId"", ""OpcaoKey"", ""ProcessadoEm"",
                    ""ProgressoNoCheckpoint"", ""Raridade"", ""ResolvidoEm"", ""ResultadoJson"",
                    ""ResultadoSchemaVersion"", ""SnapshotCombatStateJson"", ""Status"", ""Tier"", ""Tipo""
                FROM ""TorreEventos"";
                DROP TABLE ""TorreEventos"";
                ALTER TABLE ""TorreEventos_migr"" RENAME TO ""TorreEventos"";

                DROP INDEX IF EXISTS ""IX_TorreEventos_ExploracaoId"";
                CREATE INDEX ""IX_TorreEventos_ExploracaoId"" ON ""TorreEventos"" (""ExploracaoId"");

                PRAGMA foreign_keys=ON;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                PRAGMA foreign_keys=OFF;

                CREATE TABLE ""TorreExploracao"" AS SELECT * FROM ""TorreExploracoes"" WHERE 0;
                INSERT INTO ""TorreExploracao"" SELECT * FROM ""TorreExploracoes"";
                DROP TABLE ""TorreExploracoes"";

                CREATE TABLE ""TorreEventos_migr"" (
                    ""Id""                      TEXT    NOT NULL CONSTRAINT ""PK_TorreEventos"" PRIMARY KEY,
                    ""AndarOrigem""             INTEGER NOT NULL,
                    ""CriadoEm""                TEXT    NOT NULL,
                    ""EventoKey""               TEXT    NOT NULL,
                    ""EventoSeed""              INTEGER NOT NULL,
                    ""ExpiraEm""                TEXT,
                    ""ExploracaoId""            TEXT    NOT NULL,
                    ""OpcaoKey""                TEXT,
                    ""ProcessadoEm""            TEXT,
                    ""ProgressoNoCheckpoint""   INTEGER NOT NULL,
                    ""Raridade""                INTEGER NOT NULL,
                    ""ResolvidoEm""             TEXT,
                    ""ResultadoJson""           TEXT,
                    ""ResultadoSchemaVersion""  INTEGER NOT NULL,
                    ""SnapshotCombatStateJson"" TEXT,
                    ""Status""                  INTEGER NOT NULL,
                    ""Tier""                    INTEGER NOT NULL,
                    ""Tipo""                    INTEGER NOT NULL,
                    CONSTRAINT ""FK_TorreEventos_TorreExploracao_ExploracaoId""
                        FOREIGN KEY (""ExploracaoId"") REFERENCES ""TorreExploracao"" (""Id"") ON DELETE CASCADE
                );

                INSERT OR IGNORE INTO ""TorreEventos_migr"" SELECT * FROM ""TorreEventos"";
                DROP TABLE ""TorreEventos"";
                ALTER TABLE ""TorreEventos_migr"" RENAME TO ""TorreEventos"";

                CREATE INDEX ""IX_TorreEventos_ExploracaoId"" ON ""TorreEventos"" (""ExploracaoId"");

                PRAGMA foreign_keys=ON;
            ");
        }
    }
}
