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
            command.CommandText = @"SELECT * FROM TorreAndares WHERE Numero = $numero";
            command.Parameters.AddWithValue("$numero", numeroAndar);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new TorreAndar
                {
                    Numero = reader.GetInt32(reader.GetOrdinal("Numero")),
                    Tipo = Enum.Parse<TipoAndar>(reader.GetString(reader.GetOrdinal("Tipo"))),
                    TemBoss = reader.GetBoolean(reader.GetOrdinal("TemBoss")),
                    DificuldadeBoss = reader.IsDBNull(reader.GetOrdinal("DificuldadeBoss")) ? null : Enum.Parse<NivelBoss>(reader.GetString(reader.GetOrdinal("DificuldadeBoss"))),
                    Inimigos = JsonSerializer.Deserialize<List<Inimigo>>(reader.GetString(reader.GetOrdinal("Inimigos"))) ?? new List<Inimigo>(),
                    RecompensaTipo = reader.GetString(reader.GetOrdinal("RecompensaTipo")),
                    RecompensaQuantidade = reader.GetInt32(reader.GetOrdinal("RecompensaQuantidade")),
                    CriadoEm = reader.GetDateTime(reader.GetOrdinal("CriadoEm"))
                };
            }

            return null;
        }

        public async Task CriarOuAtualizarAsync(TorreAndar andar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Verifica se já existe
            var existsCmd = connection.CreateCommand();
            existsCmd.CommandText = @"SELECT COUNT(*) FROM TorreAndares WHERE Numero = $numero";
            existsCmd.Parameters.AddWithValue("$numero", andar.Numero);
            var exists = Convert.ToInt64(await existsCmd.ExecuteScalarAsync()) > 0;

            SqliteCommand command;
            if (exists)
            {
                command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE TorreAndares SET
                        Tipo = $tipo,
                        TemBoss = $temBoss,
                        DificuldadeBoss = $dificuldadeBoss,
                        ObjetivoCumprido = $objetivoCumprido,
                        Inimigos = $inimigos,
                        RecompensaTipo = $recompensaTipo,
                        RecompensaQuantidade = $recompensaQuantidade,
                        CriadoEm = $criadoEm
                    WHERE Numero = $numero";
            }
            else
            {
                command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO TorreAndares (
                        Numero, Tipo, TemBoss, DificuldadeBoss,ObjetivoCumprido,
                        Inimigos, RecompensaTipo, RecompensaQuantidade, CriadoEm
                    ) VALUES (
                        $numero, $tipo, $temBoss, $dificuldadeBoss, $objetivoCumprido,
                        $inimigos, $recompensaTipo, $recompensaQuantidade, $criadoEm
                    )";
            }

            command.Parameters.AddWithValue("$numero", andar.Numero);
            command.Parameters.AddWithValue("$tipo", andar.Tipo.ToString());
            command.Parameters.AddWithValue("$temBoss", andar.TemBoss);
            command.Parameters.AddWithValue("$dificuldadeBoss", andar.DificuldadeBoss?.ToString() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$objetivoCumprido", andar.ObjetivoCumprido);
            command.Parameters.AddWithValue("$inimigos", JsonSerializer.Serialize(andar.Inimigos));
            command.Parameters.AddWithValue("$recompensaTipo", andar.RecompensaTipo);
            command.Parameters.AddWithValue("$recompensaQuantidade", andar.RecompensaQuantidade);
            command.Parameters.AddWithValue("$criadoEm", andar.CriadoEm);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> ExisteAsync(int numeroAndar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM TorreAndares WHERE Numero = $numero";
            command.Parameters.AddWithValue("$numero", numeroAndar);

            return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
        }

        public async Task<TorreAndar?> ObterAndarPorUsuarioAsync(Guid usuarioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM TorreAndares WHERE UsuarioId = $usuarioId ORDER BY Numero DESC LIMIT 1";
            command.Parameters.AddWithValue("$usuarioId", usuarioId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapearTorreAndar(reader);
            }

            return null;
        }

        public async Task<TorreAndar?> ObterPorIdAsync(Guid andarId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM TorreAndares WHERE Id = $id";
            command.Parameters.AddWithValue("$id", andarId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapearTorreAndar(reader);
            }

            return null;
        }

        public async Task AdicionarAsync(TorreAndar andar)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
        INSERT INTO TorreAndares (
            Id, UsuarioId, Numero, Tipo, TemBoss, DificuldadeBoss,
            ObjetivoCumprido, Inimigos, RecompensaTipo, RecompensaQuantidade,
            CriadoEm, DataAlteracao
        ) VALUES (
            $id, $usuarioId, $numero, $tipo, $temBoss, $dificuldadeBoss,
            $objetivoCumprido, $inimigos, $recompensaTipo, $recompensaQuantidade,
            $criadoEm, $dataAlteracao
        )";

            command.Parameters.AddWithValue("$id", andar.Id.ToString());
            command.Parameters.AddWithValue("$usuarioId", andar.UsuarioId.ToString());
            command.Parameters.AddWithValue("$numero", andar.Numero);
            command.Parameters.AddWithValue("$tipo", andar.Tipo.ToString());
            command.Parameters.AddWithValue("$temBoss", andar.TemBoss);
            command.Parameters.AddWithValue("$dificuldadeBoss", andar.DificuldadeBoss?.ToString() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$objetivoCumprido", andar.ObjetivoCumprido);
            command.Parameters.AddWithValue("$inimigos", JsonSerializer.Serialize(andar.Inimigos));
            command.Parameters.AddWithValue("$recompensaTipo", andar.RecompensaTipo);
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
        UPDATE TorreAndares SET
            Numero = $numero,
            Tipo = $tipo,
            TemBoss = $temBoss,
            DificuldadeBoss = $dificuldadeBoss,
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
            command.Parameters.AddWithValue("$objetivoCumprido", andar.ObjetivoCumprido);
            command.Parameters.AddWithValue("$inimigos", JsonSerializer.Serialize(andar.Inimigos));
            command.Parameters.AddWithValue("$recompensaTipo", andar.RecompensaTipo);
            command.Parameters.AddWithValue("$recompensaQuantidade", andar.RecompensaQuantidade);
            command.Parameters.AddWithValue("$dataAlteracao", andar.DataAlteracao?.ToString("o") ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        private TorreAndar MapearTorreAndar(SqliteDataReader reader)
        {
            return new TorreAndar
            {
                Id = Guid.Parse(reader["Id"].ToString()!),
                UsuarioId = Guid.Parse(reader["UsuarioId"].ToString()!),
                Numero = Convert.ToInt32(reader["Numero"]),
                Tipo = Enum.Parse<TipoAndar>(reader["Tipo"].ToString()!),
                TemBoss = Convert.ToBoolean(reader["TemBoss"]),
                DificuldadeBoss = reader["DificuldadeBoss"] == DBNull.Value ? null : Enum.Parse<NivelBoss>(reader["DificuldadeBoss"].ToString()!),
                ObjetivoCumprido = Convert.ToBoolean(reader["ObjetivoCumprido"]),
                Inimigos = JsonSerializer.Deserialize<List<Inimigo>>(reader["Inimigos"].ToString()!) ?? new(),
                RecompensaTipo = reader["RecompensaTipo"].ToString()!,
                RecompensaQuantidade = Convert.ToInt32(reader["RecompensaQuantidade"]),
                CriadoEm = DateTime.Parse(reader["CriadoEm"].ToString()!),
                DataAlteracao = reader["DataAlteracao"] == DBNull.Value ? null : DateTime.Parse(reader["DataAlteracao"].ToString()!)
            };
        }

    }
}
