using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class TorreRepository : ITorreRepository
    {
        private readonly string _connectionString;

        public TorreRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<TorreAndar?> ObterPorNumeroAsync(int numeroAndar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Andares WHERE Numero = $numero";
            command.Parameters.AddWithValue("$numero", numeroAndar);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapearTorreAndar(reader);

            return null;
        }

        public async Task CriarOuAtualizarAsync(TorreAndar andar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var existsCmd = connection.CreateCommand();
            existsCmd.CommandText = @"SELECT COUNT(*) FROM Andares WHERE Numero = $numero";
            existsCmd.Parameters.AddWithValue("$numero", andar.Numero);
            var exists = Convert.ToInt64(await existsCmd.ExecuteScalarAsync()) > 0;

            SqliteCommand command;
            if (exists)
            {
                command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Andares SET
                        Tipo = $tipo,
                        TemBoss = $temBoss,
                        DificuldadeBoss = $dificuldadeBoss,
                        NivelDificuldade = $nivelDificuldade,
                        ObjetivoCumprido = $objetivoCumprido,
                        Inimigos = $inimigos,
                        RecompensaTipo = $recompensaTipo,
                        RecompensaQuantidade = $recompensaQuantidade,
                        DataAlteracao = $dataAlteracao
                    WHERE Numero = $numero";
            }
            else
            {
                command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Andares (
                        Numero, Tipo, TemBoss, DificuldadeBoss, NivelDificuldade, ObjetivoCumprido,
                        Inimigos, RecompensaTipo, RecompensaQuantidade, CriadoEm
                    ) VALUES (
                        $numero, $tipo, $temBoss, $dificuldadeBoss, $nivelDificuldade, $objetivoCumprido,
                        $inimigos, $recompensaTipo, $recompensaQuantidade, $criadoEm
                    )";
                command.Parameters.AddWithValue("$criadoEm", andar.CriadoEm);
            }

            command.Parameters.AddWithValue("$numero", andar.Numero);
            command.Parameters.AddWithValue("$tipo", andar.Tipo.ToString());
            command.Parameters.AddWithValue("$temBoss", andar.TemBoss);
            command.Parameters.AddWithValue("$dificuldadeBoss", andar.DificuldadeBoss?.ToString() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$nivelDificuldade", andar.NivelDificuldade);
            command.Parameters.AddWithValue("$objetivoCumprido", andar.ObjetivoCumprido);
            command.Parameters.AddWithValue("$inimigos", JsonSerializer.Serialize(andar.Inimigos));
            command.Parameters.AddWithValue("$recompensaTipo", andar.RecompensaTipo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$recompensaQuantidade", andar.RecompensaQuantidade);
            command.Parameters.AddWithValue("$dataAlteracao", andar.DataAlteracao?.ToString("o") ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> ExisteAsync(int numeroAndar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM Andares WHERE Numero = $numero";
            command.Parameters.AddWithValue("$numero", numeroAndar);

            return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
        }

        public async Task<TorreAndar?> ObterAndarPorUsuarioAsync(Guid usuarioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Andares WHERE UsuarioId = $usuarioId AND ObjetivoCumprido = 0 ORDER BY Numero ASC LIMIT 1";
            command.Parameters.AddWithValue("$usuarioId", usuarioId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapearTorreAndar(reader);

            return null;
        }

        public async Task<TorreAndar?> ObterPorIdAsync(Guid andarId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Andares WHERE Id = $id";
            command.Parameters.AddWithValue("$id", andarId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapearTorreAndar(reader);

            return null;
        }

        public async Task AdicionarAsync(TorreAndar andar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Andares (
                    Id, UsuarioId, Numero, Tipo, TemBoss, DificuldadeBoss,
                    NivelDificuldade, ObjetivoCumprido, Inimigos, RecompensaTipo,
                    RecompensaQuantidade, CriadoEm, DataAlteracao
                ) VALUES (
                    $id, $usuarioId, $numero, $tipo, $temBoss, $dificuldadeBoss,
                    $nivelDificuldade, $objetivoCumprido, $inimigos, $recompensaTipo,
                    $recompensaQuantidade, $criadoEm, $dataAlteracao
                )";

            command.Parameters.AddWithValue("$id", andar.Id.ToString());
            command.Parameters.AddWithValue("$usuarioId", andar.UsuarioId.ToString());
            command.Parameters.AddWithValue("$numero", andar.Numero);
            command.Parameters.AddWithValue("$tipo", andar.Tipo.ToString());
            command.Parameters.AddWithValue("$temBoss", andar.TemBoss);
            command.Parameters.AddWithValue("$dificuldadeBoss", andar.DificuldadeBoss?.ToString() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$nivelDificuldade", andar.NivelDificuldade);
            command.Parameters.AddWithValue("$objetivoCumprido", andar.ObjetivoCumprido);
            command.Parameters.AddWithValue("$inimigos", JsonSerializer.Serialize(andar.Inimigos));
            command.Parameters.AddWithValue("$recompensaTipo", andar.RecompensaTipo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$recompensaQuantidade", andar.RecompensaQuantidade);
            command.Parameters.AddWithValue("$criadoEm", andar.CriadoEm);
            command.Parameters.AddWithValue("$dataAlteracao", andar.DataAlteracao?.ToString("o") ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task AtualizarAsync(TorreAndar andar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Andares SET
                    Numero = $numero,
                    Tipo = $tipo,
                    TemBoss = $temBoss,
                    DificuldadeBoss = $dificuldadeBoss,
                    NivelDificuldade = $nivelDificuldade,
                    ObjetivoCumprido = $objetivoCumprido,
                    Inimigos = $inimigos,
                    RecompensaTipo = $recompensaTipo,
                    RecompensaQuantidade = $recompensaQuantidade,
                    DataAlteracao = $dataAlteracao
                WHERE Id = $id";

            command.Parameters.AddWithValue("$id", andar.Id.ToString());
            command.Parameters.AddWithValue("$numero", andar.Numero);
            command.Parameters.AddWithValue("$tipo", andar.Tipo.ToString());
            command.Parameters.AddWithValue("$temBoss", andar.TemBoss);
            command.Parameters.AddWithValue("$dificuldadeBoss", andar.DificuldadeBoss?.ToString() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$nivelDificuldade", andar.NivelDificuldade);
            command.Parameters.AddWithValue("$objetivoCumprido", andar.ObjetivoCumprido);
            command.Parameters.AddWithValue("$inimigos", JsonSerializer.Serialize(andar.Inimigos));
            command.Parameters.AddWithValue("$recompensaTipo", andar.RecompensaTipo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$recompensaQuantidade", andar.RecompensaQuantidade);
            command.Parameters.AddWithValue("$dataAlteracao", andar.DataAlteracao?.ToString("o") ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        private static TorreAndar MapearTorreAndar(SqliteDataReader reader)
        {
            int ordNivelDif = -1;
            try { ordNivelDif = reader.GetOrdinal("NivelDificuldade"); } catch { }

            return new TorreAndar
            {
                Id = Guid.Parse(reader["Id"].ToString()!),
                UsuarioId = Guid.Parse(reader["UsuarioId"].ToString()!),
                Numero = Convert.ToInt32(reader["Numero"]),
                Tipo = Enum.Parse<TipoAndar>(reader["Tipo"].ToString()!),
                TemBoss = Convert.ToBoolean(reader["TemBoss"]),
                DificuldadeBoss = reader["DificuldadeBoss"] == DBNull.Value ? null : Enum.Parse<NivelBoss>(reader["DificuldadeBoss"].ToString()!),
                NivelDificuldade = ordNivelDif >= 0 && reader[ordNivelDif] != DBNull.Value ? Convert.ToInt32(reader[ordNivelDif]) : 0,
                ObjetivoCumprido = Convert.ToBoolean(reader["ObjetivoCumprido"]),
                Inimigos = reader["Inimigos"] != DBNull.Value && !string.IsNullOrEmpty(reader["Inimigos"].ToString())
                    ? JsonSerializer.Deserialize<List<Inimigo>>(reader["Inimigos"].ToString()!) ?? new()
                    : new(),
                RecompensaTipo = reader["RecompensaTipo"] == DBNull.Value ? null : reader["RecompensaTipo"].ToString(),
                RecompensaQuantidade = Convert.ToInt32(reader["RecompensaQuantidade"]),
                CriadoEm = DateTime.Parse(reader["CriadoEm"].ToString()!),
                DataAlteracao = reader["DataAlteracao"] == DBNull.Value ? null : DateTime.Parse(reader["DataAlteracao"].ToString()!)
            };
        }
    }
}
