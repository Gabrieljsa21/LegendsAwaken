using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class TorreBoosterRepository : ITorreBoosterRepository
{
    private readonly string _connectionString;

    public TorreBoosterRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task EnsureTableAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS TorreBoosters (
                Id         TEXT NOT NULL PRIMARY KEY,
                UsuarioId  TEXT NOT NULL,
                Tipo       INTEGER NOT NULL,
                Quantidade INTEGER NOT NULL DEFAULT 0
            )";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> ObterQuantidadeAsync(Guid usuarioId, TipoBooster tipo)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT Quantidade FROM TorreBoosters
            WHERE UsuarioId = $uid AND Tipo = $tipo
            LIMIT 1";
        cmd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        cmd.Parameters.AddWithValue("$tipo", (int)tipo);
        var result = await cmd.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    public async Task<List<(TipoBooster Tipo, int Quantidade)>> ListarAsync(Guid usuarioId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT Tipo, Quantidade FROM TorreBoosters
            WHERE UsuarioId = $uid AND Quantidade > 0";
        cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
        using var reader = await cmd.ExecuteReaderAsync();

        var lista = new List<(TipoBooster, int)>();
        while (await reader.ReadAsync())
        {
            var tipo = (TipoBooster)Convert.ToInt32(reader["Tipo"]);
            int qtd  = Convert.ToInt32(reader["Quantidade"]);
            lista.Add((tipo, qtd));
        }
        return lista;
    }

    public async Task AdicionarAsync(Guid usuarioId, TipoBooster tipo, int quantidade)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Upsert: ensure row exists, then increment
        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT OR IGNORE INTO TorreBoosters (Id, UsuarioId, Tipo, Quantidade)
            VALUES ($id, $uid, $tipo, 0)";
        insertCmd.Parameters.AddWithValue("$id",   Guid.NewGuid().ToString());
        insertCmd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        insertCmd.Parameters.AddWithValue("$tipo", (int)tipo);
        await insertCmd.ExecuteNonQueryAsync();

        var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE TorreBoosters
            SET Quantidade = Quantidade + $qtd
            WHERE UsuarioId = $uid AND Tipo = $tipo";
        updateCmd.Parameters.AddWithValue("$qtd",  quantidade);
        updateCmd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        updateCmd.Parameters.AddWithValue("$tipo", (int)tipo);
        await updateCmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ConsumirAsync(Guid usuarioId, TipoBooster tipo)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT Quantidade FROM TorreBoosters
            WHERE UsuarioId = $uid AND Tipo = $tipo
            LIMIT 1";
        selectCmd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        selectCmd.Parameters.AddWithValue("$tipo", (int)tipo);
        var result = await selectCmd.ExecuteScalarAsync();

        int quantidade = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        if (quantidade <= 0) return false;

        var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE TorreBoosters
            SET Quantidade = Quantidade - 1
            WHERE UsuarioId = $uid AND Tipo = $tipo";
        updateCmd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        updateCmd.Parameters.AddWithValue("$tipo", (int)tipo);
        await updateCmd.ExecuteNonQueryAsync();
        return true;
    }
}
