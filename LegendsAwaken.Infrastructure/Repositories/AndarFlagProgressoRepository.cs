using LegendsAwaken.Domain.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public interface IAndarFlagProgressoRepository
{
    Task EnsureTableAsync();
    Task GerarFlagAsync(Guid userId, int andar, string flagNome);
    Task MarcarExpiradoAsync(Guid userId, int andar, string flagNome);
    Task<IReadOnlyList<string>> ObterFlagsGeradasAsync(Guid userId);
}

public sealed class AndarFlagProgressoRepository(string connectionString) : IAndarFlagProgressoRepository
{
    public async Task EnsureTableAsync()
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS AndarFlagProgresso (
                UsuarioId TEXT NOT NULL,
                Andar     INTEGER NOT NULL,
                FlagNome  TEXT NOT NULL,
                Gerada    INTEGER NOT NULL DEFAULT 0,
                Expirou   INTEGER NOT NULL DEFAULT 0,
                GeradaEm  TEXT,
                PRIMARY KEY (UsuarioId, Andar, FlagNome)
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task GerarFlagAsync(Guid userId, int andar, string flagNome)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO AndarFlagProgresso (UsuarioId, Andar, FlagNome, Gerada, Expirou, GeradaEm)
            VALUES ($uid, $andar, $flag, 1, 0, $now)
            ON CONFLICT(UsuarioId, Andar, FlagNome) DO UPDATE SET Gerada=1, GeradaEm=$now WHERE Gerada=0;
            """;
        cmd.Parameters.AddWithValue("$uid", userId.ToString());
        cmd.Parameters.AddWithValue("$andar", andar);
        cmd.Parameters.AddWithValue("$flag", flagNome);
        cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task MarcarExpiradoAsync(Guid userId, int andar, string flagNome)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR IGNORE INTO AndarFlagProgresso (UsuarioId, Andar, FlagNome, Gerada, Expirou)
            VALUES ($uid, $andar, $flag, 0, 1);
            """;
        cmd.Parameters.AddWithValue("$uid", userId.ToString());
        cmd.Parameters.AddWithValue("$andar", andar);
        cmd.Parameters.AddWithValue("$flag", flagNome);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<string>> ObterFlagsGeradasAsync(Guid userId)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT FlagNome FROM AndarFlagProgresso
            WHERE UsuarioId=$uid AND Gerada=1;
            """;
        cmd.Parameters.AddWithValue("$uid", userId.ToString());
        var result = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(reader.GetString(0));
        return result;
    }
}
