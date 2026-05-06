using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class JogadorItemRepository : IJogadorItemRepository
{
    private readonly string _connectionString;

    public JogadorItemRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task EnsureTableAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS JogadorItens (
                Id           TEXT NOT NULL PRIMARY KEY,
                UsuarioId    TEXT NOT NULL,
                ItemConfigId TEXT NOT NULL,
                Nome         TEXT NOT NULL,
                Tipo         INTEGER NOT NULL,
                Icone        TEXT NOT NULL DEFAULT '📦',
                Efeito       TEXT NOT NULL DEFAULT '',
                Quantidade   INTEGER NOT NULL DEFAULT 1,
                ObtidoEm     TEXT NOT NULL,
                ExtraData    TEXT,
                UNIQUE(UsuarioId, ItemConfigId)
            )";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpsertAsync(
        Guid usuarioId, string itemConfigId, string nome,
        TipoItemJogador tipo, string icone, string efeito, int quantidade)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO JogadorItens (Id, UsuarioId, ItemConfigId, Nome, Tipo, Icone, Efeito, Quantidade, ObtidoEm)
            VALUES ($id, $uid, $configId, $nome, $tipo, $icone, $efeito, $qtd, $obtidoEm)
            ON CONFLICT(UsuarioId, ItemConfigId) DO UPDATE
            SET Quantidade = Quantidade + excluded.Quantidade,
                ObtidoEm  = excluded.ObtidoEm";
        cmd.Parameters.AddWithValue("$id",       Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("$uid",      usuarioId.ToString());
        cmd.Parameters.AddWithValue("$configId", itemConfigId);
        cmd.Parameters.AddWithValue("$nome",     nome);
        cmd.Parameters.AddWithValue("$tipo",     (int)tipo);
        cmd.Parameters.AddWithValue("$icone",    icone);
        cmd.Parameters.AddWithValue("$efeito",   efeito);
        cmd.Parameters.AddWithValue("$qtd",      quantidade);
        cmd.Parameters.AddWithValue("$obtidoEm", DateTime.UtcNow.ToString("o"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<JogadorItem>> ListarAsync(Guid usuarioId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM JogadorItens WHERE UsuarioId = $uid ORDER BY Tipo, Nome";
        cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
        var lista = new List<JogadorItem>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) lista.Add(Mapear(reader));
        return lista;
    }

    public async Task<JogadorItem?> ObterPorConfigAsync(Guid usuarioId, string itemConfigId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM JogadorItens WHERE UsuarioId = $uid AND ItemConfigId = $configId LIMIT 1";
        cmd.Parameters.AddWithValue("$uid",      usuarioId.ToString());
        cmd.Parameters.AddWithValue("$configId", itemConfigId);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    private static JogadorItem Mapear(SqliteDataReader r) => new()
    {
        Id           = Guid.Parse(r["Id"].ToString()!),
        UsuarioId    = Guid.Parse(r["UsuarioId"].ToString()!),
        ItemConfigId = r["ItemConfigId"].ToString()!,
        Nome         = r["Nome"].ToString()!,
        Tipo         = (TipoItemJogador)Convert.ToInt32(r["Tipo"]),
        Icone        = r["Icone"].ToString()!,
        Efeito       = r["Efeito"].ToString()!,
        Quantidade   = Convert.ToInt32(r["Quantidade"]),
        ObtidoEm     = DateTime.Parse(r["ObtidoEm"].ToString()!),
        ExtraData    = r["ExtraData"] == DBNull.Value ? null : r["ExtraData"].ToString()
    };
}
