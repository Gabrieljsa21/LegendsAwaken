using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class CidadeBoosterRepository : ICidadeBoosterRepository
{
    private readonly string _connectionString;

    public CidadeBoosterRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task EnsureTablesAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS CidadeBoosterInventario (
                UsuarioId  TEXT    NOT NULL,
                Tipo       INTEGER NOT NULL,
                Quantidade INTEGER NOT NULL DEFAULT 0,
                PRIMARY KEY (UsuarioId, Tipo)
            );
            CREATE TABLE IF NOT EXISTS CidadeBoosterAtivo (
                Id        TEXT PRIMARY KEY,
                UsuarioId TEXT NOT NULL UNIQUE,
                Tipo      INTEGER NOT NULL,
                AtivadoEm TEXT NOT NULL,
                ExpiraEm  TEXT NOT NULL
            );";
        await cmd.ExecuteNonQueryAsync();
    }

    // ── Inventory ────────────────────────────────────────────────────────────

    public async Task<int> ObterQuantidadeAsync(ulong usuarioId, TipoBoosterCidade tipo)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Quantidade FROM CidadeBoosterInventario
            WHERE UsuarioId = $uid AND Tipo = $tipo LIMIT 1";
        cmd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        cmd.Parameters.AddWithValue("$tipo", (int)tipo);
        var result = await cmd.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    public async Task<List<(TipoBoosterCidade Tipo, int Quantidade)>> ListarInventarioAsync(ulong usuarioId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Tipo, Quantidade FROM CidadeBoosterInventario
            WHERE UsuarioId = $uid AND Quantidade > 0
            ORDER BY Tipo";
        cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
        using var reader = await cmd.ExecuteReaderAsync();

        var lista = new List<(TipoBoosterCidade, int)>();
        while (await reader.ReadAsync())
            lista.Add(((TipoBoosterCidade)Convert.ToInt32(reader["Tipo"]), Convert.ToInt32(reader["Quantidade"])));
        return lista;
    }

    public async Task AdicionarInventarioAsync(ulong usuarioId, TipoBoosterCidade tipo, int quantidade)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var ins = conn.CreateCommand();
        ins.CommandText = @"
            INSERT OR IGNORE INTO CidadeBoosterInventario (UsuarioId, Tipo, Quantidade)
            VALUES ($uid, $tipo, 0)";
        ins.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        ins.Parameters.AddWithValue("$tipo", (int)tipo);
        await ins.ExecuteNonQueryAsync();

        var upd = conn.CreateCommand();
        upd.CommandText = @"
            UPDATE CidadeBoosterInventario
            SET Quantidade = Quantidade + $qtd
            WHERE UsuarioId = $uid AND Tipo = $tipo";
        upd.Parameters.AddWithValue("$qtd",  quantidade);
        upd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        upd.Parameters.AddWithValue("$tipo", (int)tipo);
        await upd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ConsumirInventarioAsync(ulong usuarioId, TipoBoosterCidade tipo)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var sel = conn.CreateCommand();
        sel.CommandText = @"
            SELECT Quantidade FROM CidadeBoosterInventario
            WHERE UsuarioId = $uid AND Tipo = $tipo LIMIT 1";
        sel.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        sel.Parameters.AddWithValue("$tipo", (int)tipo);
        var result = await sel.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value || Convert.ToInt32(result) <= 0) return false;

        var upd = conn.CreateCommand();
        upd.CommandText = @"
            UPDATE CidadeBoosterInventario
            SET Quantidade = Quantidade - 1
            WHERE UsuarioId = $uid AND Tipo = $tipo";
        upd.Parameters.AddWithValue("$uid",  usuarioId.ToString());
        upd.Parameters.AddWithValue("$tipo", (int)tipo);
        await upd.ExecuteNonQueryAsync();
        return true;
    }

    // ── Active booster ────────────────────────────────────────────────────────

    public async Task<CidadeBoosterAtivo?> ObterAtivoAsync(ulong usuarioId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, Tipo, AtivadoEm, ExpiraEm FROM CidadeBoosterAtivo
            WHERE UsuarioId = $uid LIMIT 1";
        cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        return new CidadeBoosterAtivo
        {
            Id        = Guid.Parse(reader["Id"].ToString()!),
            UsuarioId = usuarioId,
            Tipo      = (TipoBoosterCidade)Convert.ToInt32(reader["Tipo"]),
            AtivadoEm = DateTime.Parse(reader["AtivadoEm"].ToString()!),
            ExpiraEm  = DateTime.Parse(reader["ExpiraEm"].ToString()!),
        };
    }

    public async Task SalvarAtivoAsync(CidadeBoosterAtivo ativo)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO CidadeBoosterAtivo (Id, UsuarioId, Tipo, AtivadoEm, ExpiraEm)
            VALUES ($id, $uid, $tipo, $ativado, $expira)";
        cmd.Parameters.AddWithValue("$id",     ativo.Id.ToString());
        cmd.Parameters.AddWithValue("$uid",    ativo.UsuarioId.ToString());
        cmd.Parameters.AddWithValue("$tipo",   (int)ativo.Tipo);
        cmd.Parameters.AddWithValue("$ativado", ativo.AtivadoEm.ToString("O"));
        cmd.Parameters.AddWithValue("$expira",  ativo.ExpiraEm.ToString("O"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DesativarAsync(ulong usuarioId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM CidadeBoosterAtivo WHERE UsuarioId = $uid";
        cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
        await cmd.ExecuteNonQueryAsync();
    }
}
