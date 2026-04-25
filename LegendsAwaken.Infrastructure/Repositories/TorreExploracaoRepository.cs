using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class TorreExploracaoRepository : ITorreExploracaoRepository
{
    private readonly string _connectionString;

    public TorreExploracaoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task EnsureTableAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS TorreExploracoes (
                Id                    TEXT NOT NULL PRIMARY KEY,
                UsuarioId             TEXT NOT NULL,
                AndarNumero           INTEGER NOT NULL,
                Progresso             REAL NOT NULL DEFAULT 0,
                UltimoCheckpoint      INTEGER NOT NULL DEFAULT 0,
                CheckpointInterval    INTEGER NOT NULL DEFAULT 25,
                Status                INTEGER NOT NULL DEFAULT 0,
                IniciadoEm            TEXT NOT NULL,
                UltimoTickEm          TEXT NOT NULL,
                HeroisIds             TEXT NOT NULL DEFAULT '',
                BoosterAtivo          INTEGER NULL,
                LootOuro              INTEGER NOT NULL DEFAULT 0,
                LootFragmentosQtd     INTEGER NOT NULL DEFAULT 0,
                LootFragmentosHeroiId TEXT NOT NULL DEFAULT '',
                ConcluidoEm           TEXT NULL,
                HeroisFeridosIds      TEXT NOT NULL DEFAULT ''
            )";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SalvarAsync(TorreExploracao exploracao)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO TorreExploracoes (
                Id, UsuarioId, AndarNumero, Progresso, UltimoCheckpoint,
                CheckpointInterval, Status, IniciadoEm, UltimoTickEm,
                HeroisIds, BoosterAtivo, LootOuro, LootFragmentosQtd,
                LootFragmentosHeroiId, ConcluidoEm, HeroisFeridosIds
            ) VALUES (
                $id, $uid, $andar, $progresso, $ultimoCheckpoint,
                $checkpointInterval, $status, $iniciadoEm, $ultimoTickEm,
                $heroisIds, $boosterAtivo, $lootOuro, $lootFragmentosQtd,
                $lootFragmentosHeroiId, $concluidoEm, $heroisFeridosIds
            )";
        BindParams(cmd, exploracao);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task AtualizarAsync(TorreExploracao exploracao)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE TorreExploracoes SET
                AndarNumero           = $andar,
                Progresso             = $progresso,
                UltimoCheckpoint      = $ultimoCheckpoint,
                CheckpointInterval    = $checkpointInterval,
                Status                = $status,
                IniciadoEm            = $iniciadoEm,
                UltimoTickEm          = $ultimoTickEm,
                HeroisIds             = $heroisIds,
                BoosterAtivo          = $boosterAtivo,
                LootOuro              = $lootOuro,
                LootFragmentosQtd     = $lootFragmentosQtd,
                LootFragmentosHeroiId = $lootFragmentosHeroiId,
                ConcluidoEm           = $concluidoEm,
                HeroisFeridosIds      = $heroisFeridosIds
            WHERE Id = $id";
        BindParams(cmd, exploracao);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<TorreExploracao?> ObterAtivaAsync(Guid usuarioId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM TorreExploracoes
            WHERE UsuarioId = $uid AND Status = $status
            ORDER BY IniciadoEm DESC LIMIT 1";
        cmd.Parameters.AddWithValue("$uid",    usuarioId.ToString());
        cmd.Parameters.AddWithValue("$status", (int)StatusExploracao.Ativa);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    public async Task<TorreExploracao?> ObterPendenteAsync(Guid usuarioId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        // Concluida = 1, Falha = 2
        cmd.CommandText = @"
            SELECT * FROM TorreExploracoes
            WHERE UsuarioId = $uid AND (Status = 1 OR Status = 2)
            ORDER BY IniciadoEm DESC LIMIT 1";
        cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static void BindParams(SqliteCommand cmd, TorreExploracao e)
    {
        cmd.Parameters.AddWithValue("$id",                   e.Id.ToString());
        cmd.Parameters.AddWithValue("$uid",                  e.UsuarioId.ToString());
        cmd.Parameters.AddWithValue("$andar",                e.AndarNumero);
        cmd.Parameters.AddWithValue("$progresso",            e.Progresso);
        cmd.Parameters.AddWithValue("$ultimoCheckpoint",     e.UltimoCheckpoint);
        cmd.Parameters.AddWithValue("$checkpointInterval",   e.CheckpointInterval);
        cmd.Parameters.AddWithValue("$status",               (int)e.Status);
        cmd.Parameters.AddWithValue("$iniciadoEm",           e.IniciadoEm.ToString("o"));
        cmd.Parameters.AddWithValue("$ultimoTickEm",         e.UltimoTickEm.ToString("o"));
        cmd.Parameters.AddWithValue("$heroisIds",            e.HeroisIds);
        cmd.Parameters.AddWithValue("$boosterAtivo",         e.BoosterAtivo.HasValue
                                                                 ? (object)(int)e.BoosterAtivo.Value
                                                                 : DBNull.Value);
        cmd.Parameters.AddWithValue("$lootOuro",             e.LootOuro);
        cmd.Parameters.AddWithValue("$lootFragmentosQtd",    e.LootFragmentosQtd);
        cmd.Parameters.AddWithValue("$lootFragmentosHeroiId",e.LootFragmentosHeroiId);
        cmd.Parameters.AddWithValue("$concluidoEm",          e.ConcluidoEm.HasValue
                                                                 ? (object)e.ConcluidoEm.Value.ToString("o")
                                                                 : DBNull.Value);
        cmd.Parameters.AddWithValue("$heroisFeridosIds",     e.HeroisFeridosIds);
    }

    private static TorreExploracao Mapear(SqliteDataReader r) => new()
    {
        Id                    = Guid.Parse(r["Id"].ToString()!),
        UsuarioId             = Guid.Parse(r["UsuarioId"].ToString()!),
        AndarNumero           = Convert.ToInt32(r["AndarNumero"]),
        Progresso             = Convert.ToDouble(r["Progresso"]),
        UltimoCheckpoint      = Convert.ToInt32(r["UltimoCheckpoint"]),
        CheckpointInterval    = Convert.ToInt32(r["CheckpointInterval"]),
        Status                = (StatusExploracao)Convert.ToInt32(r["Status"]),
        IniciadoEm            = DateTime.Parse(r["IniciadoEm"].ToString()!, null, DateTimeStyles.RoundtripKind),
        UltimoTickEm          = DateTime.Parse(r["UltimoTickEm"].ToString()!, null, DateTimeStyles.RoundtripKind),
        HeroisIds             = r["HeroisIds"].ToString() ?? "",
        BoosterAtivo          = r["BoosterAtivo"] == DBNull.Value
                                    ? null
                                    : (TipoBooster)Convert.ToInt32(r["BoosterAtivo"]),
        LootOuro              = Convert.ToInt32(r["LootOuro"]),
        LootFragmentosQtd     = Convert.ToInt32(r["LootFragmentosQtd"]),
        LootFragmentosHeroiId = r["LootFragmentosHeroiId"].ToString() ?? "",
        ConcluidoEm           = r["ConcluidoEm"] == DBNull.Value
                                    ? null
                                    : DateTime.Parse(r["ConcluidoEm"].ToString()!),
        HeroisFeridosIds      = r["HeroisFeridosIds"].ToString() ?? "",
    };
}
