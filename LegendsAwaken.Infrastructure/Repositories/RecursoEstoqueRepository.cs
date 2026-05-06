using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class RecursoEstoqueRepository : IRecursoEstoqueRepository
{
    private readonly string _connectionString;

    public RecursoEstoqueRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task EnsureTableAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS RecursoEstoque (
                Id          TEXT NOT NULL PRIMARY KEY,
                UsuarioId   TEXT NOT NULL,
                Recurso     TEXT NOT NULL,
                Quantidade  INTEGER NOT NULL DEFAULT 0,
                UNIQUE(UsuarioId, Recurso)
            )";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpsertAsync(Guid usuarioId, string recurso, int quantidade)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO RecursoEstoque (Id, UsuarioId, Recurso, Quantidade)
            VALUES ($id, $uid, $recurso, $qtd)
            ON CONFLICT(UsuarioId, Recurso) DO UPDATE
            SET Quantidade = Quantidade + excluded.Quantidade";
        cmd.Parameters.AddWithValue("$id",     Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("$uid",    usuarioId.ToString());
        cmd.Parameters.AddWithValue("$recurso", recurso);
        cmd.Parameters.AddWithValue("$qtd",    quantidade);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<RecursoEstoque?> ObterAsync(Guid usuarioId, string recurso)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM RecursoEstoque WHERE UsuarioId = $uid AND Recurso = $recurso LIMIT 1";
        cmd.Parameters.AddWithValue("$uid",    usuarioId.ToString());
        cmd.Parameters.AddWithValue("$recurso", recurso);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    public async Task<List<RecursoEstoque>> ListarAsync(Guid usuarioId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM RecursoEstoque WHERE UsuarioId = $uid ORDER BY Recurso";
        cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
        var lista = new List<RecursoEstoque>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) lista.Add(Mapear(reader));
        return lista;
    }

    private static RecursoEstoque Mapear(SqliteDataReader r) => new()
    {
        Id         = Guid.Parse(r["Id"].ToString()!),
        UsuarioId  = Guid.Parse(r["UsuarioId"].ToString()!),
        Recurso    = r["Recurso"].ToString()!,
        Quantidade = Convert.ToInt32(r["Quantidade"])
    };
}
