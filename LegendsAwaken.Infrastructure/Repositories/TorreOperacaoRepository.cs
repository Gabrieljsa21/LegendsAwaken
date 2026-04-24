using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class TorreOperacaoRepository : ITorreOperacaoRepository
    {
        private readonly string _connectionString;

        public TorreOperacaoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task EnsureTableAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS TorreOperacoes (
                    Id                  TEXT NOT NULL PRIMARY KEY,
                    UsuarioId           TEXT NOT NULL,
                    AndarNumero         INTEGER NOT NULL,
                    Objetivo            INTEGER NOT NULL,
                    PerfilRisco         INTEGER NOT NULL,
                    Status              INTEGER NOT NULL,
                    IniciadoEm          TEXT NOT NULL,
                    DuracaoHoras        INTEGER NOT NULL,
                    ResultadoOuro       INTEGER,
                    ResultadoRecursoNome TEXT,
                    ResultadoRecursoQtd INTEGER,
                    ConcluidoEm         TEXT
                )";
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<TorreOperacao?> ObterAtivaAsync(Guid usuarioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT * FROM TorreOperacoes
                WHERE UsuarioId = $uid AND Status = $status
                ORDER BY IniciadoEm DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
            cmd.Parameters.AddWithValue("$status", (int)StatusOperacao.Ativa);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Mapear(reader) : null;
        }

        public async Task<TorreOperacao?> ObterConcluidaAsync(Guid usuarioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT * FROM TorreOperacoes
                WHERE UsuarioId = $uid AND Status = $status
                ORDER BY ConcluidoEm DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$uid", usuarioId.ToString());
            cmd.Parameters.AddWithValue("$status", (int)StatusOperacao.Concluida);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Mapear(reader) : null;
        }

        public async Task AdicionarAsync(TorreOperacao op)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO TorreOperacoes (
                    Id, UsuarioId, AndarNumero, Objetivo, PerfilRisco, Status,
                    IniciadoEm, DuracaoHoras, ResultadoOuro, ResultadoRecursoNome,
                    ResultadoRecursoQtd, ConcluidoEm
                ) VALUES (
                    $id, $uid, $andar, $obj, $risco, $status,
                    $inicio, $duracao, $ouro, $recursoNome, $recursoQtd, $concluidoEm
                )";
            BindParams(cmd, op);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AtualizarAsync(TorreOperacao op)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE TorreOperacoes SET
                    AndarNumero          = $andar,
                    Objetivo             = $obj,
                    PerfilRisco          = $risco,
                    Status               = $status,
                    IniciadoEm           = $inicio,
                    DuracaoHoras         = $duracao,
                    ResultadoOuro        = $ouro,
                    ResultadoRecursoNome = $recursoNome,
                    ResultadoRecursoQtd  = $recursoQtd,
                    ConcluidoEm          = $concluidoEm
                WHERE Id = $id";
            BindParams(cmd, op);
            await cmd.ExecuteNonQueryAsync();
        }

        private static void BindParams(SqliteCommand cmd, TorreOperacao op)
        {
            cmd.Parameters.AddWithValue("$id",         op.Id.ToString());
            cmd.Parameters.AddWithValue("$uid",        op.UsuarioId.ToString());
            cmd.Parameters.AddWithValue("$andar",      op.AndarNumero);
            cmd.Parameters.AddWithValue("$obj",        (int)op.Objetivo);
            cmd.Parameters.AddWithValue("$risco",      (int)op.PerfilRisco);
            cmd.Parameters.AddWithValue("$status",     (int)op.Status);
            cmd.Parameters.AddWithValue("$inicio",     op.IniciadoEm.ToString("o"));
            cmd.Parameters.AddWithValue("$duracao",    op.DuracaoHoras);
            cmd.Parameters.AddWithValue("$ouro",       op.ResultadoOuro.HasValue ? (object)op.ResultadoOuro.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$recursoNome",op.ResultadoRecursoNome ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$recursoQtd", op.ResultadoRecursoQtd.HasValue ? (object)op.ResultadoRecursoQtd.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$concluidoEm",op.ConcluidoEm.HasValue ? (object)op.ConcluidoEm.Value.ToString("o") : DBNull.Value);
        }

        private static TorreOperacao Mapear(SqliteDataReader r) => new()
        {
            Id              = Guid.Parse(r["Id"].ToString()!),
            UsuarioId       = Guid.Parse(r["UsuarioId"].ToString()!),
            AndarNumero     = Convert.ToInt32(r["AndarNumero"]),
            Objetivo        = (ObjetivoOperacao)Convert.ToInt32(r["Objetivo"]),
            PerfilRisco     = (PerfilRisco)Convert.ToInt32(r["PerfilRisco"]),
            Status          = (StatusOperacao)Convert.ToInt32(r["Status"]),
            IniciadoEm      = DateTime.Parse(r["IniciadoEm"].ToString()!),
            DuracaoHoras    = Convert.ToInt32(r["DuracaoHoras"]),
            ResultadoOuro        = r["ResultadoOuro"]        == DBNull.Value ? null : Convert.ToInt32(r["ResultadoOuro"]),
            ResultadoRecursoNome = r["ResultadoRecursoNome"] == DBNull.Value ? null : r["ResultadoRecursoNome"].ToString(),
            ResultadoRecursoQtd  = r["ResultadoRecursoQtd"]  == DBNull.Value ? null : Convert.ToInt32(r["ResultadoRecursoQtd"]),
            ConcluidoEm          = r["ConcluidoEm"]          == DBNull.Value ? null : DateTime.Parse(r["ConcluidoEm"].ToString()!)
        };
    }
}
