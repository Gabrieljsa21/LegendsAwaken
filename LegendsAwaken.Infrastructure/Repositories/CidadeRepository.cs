using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class CidadeRepository : ICidadeRepository
    {
        private readonly string _connectionString;

        public CidadeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Cidade?> ObterPorIdAsync(Guid cidadeId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Cidades WHERE Id = $id";
            command.Parameters.AddWithValue("$id", cidadeId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Cidade
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    UsuarioId = (ulong)reader.GetInt64(1),
                    Nome = reader.GetString(2),
                    Nivel = reader.GetInt32(3),
                    Populacao = reader.GetInt32(4),
                    CapacidadeMaxima = reader.GetInt32(5),
                    Recursos = JsonSerializer.Deserialize<Recursos>(reader.GetString(6)) ?? new Recursos(),
                    Construcoes = JsonSerializer.Deserialize<List<Construcao>>(reader.GetString(7)) ?? new List<Construcao>(),
                    Trabalhadores = JsonSerializer.Deserialize<List<PersonagemTrabalhador>>(reader.GetString(8)) ?? new List<PersonagemTrabalhador>(),
                    DataCriacao = DateTime.Parse(reader.GetString(9)),
                    DataAlteracao = DateTime.Parse(reader.GetString(10))
                };
            }

            return null;
        }

        public async Task AdicionarAsync(Cidade cidade)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Cidades (
                    Id, UsuarioId, Nome, Nivel, Populacao, CapacidadeMaxima,
                    Recursos, Construcoes, Trabalhadores, DataCriacao, UltimaAtualizacao
                ) VALUES (
                    $id, $UsuarioId, $nome, $nivel, $pop, $capacidade,
                    $recursos, $construcoes, $trabalhadores, $criado, $atualizado
                )";

            command.Parameters.AddWithValue("$id", cidade.Id.ToString());
            command.Parameters.AddWithValue("$UsuarioId", (long)cidade.UsuarioId);
            command.Parameters.AddWithValue("$nome", cidade.Nome);
            command.Parameters.AddWithValue("$nivel", cidade.Nivel);
            command.Parameters.AddWithValue("$pop", cidade.Populacao);
            command.Parameters.AddWithValue("$capacidade", cidade.CapacidadeMaxima);
            command.Parameters.AddWithValue("$recursos", JsonSerializer.Serialize(cidade.Recursos));
            command.Parameters.AddWithValue("$construcoes", JsonSerializer.Serialize(cidade.Construcoes));
            command.Parameters.AddWithValue("$trabalhadores", JsonSerializer.Serialize(cidade.Trabalhadores));
            command.Parameters.AddWithValue("$criado", cidade.DataCriacao.ToString("o"));
            command.Parameters.AddWithValue("$atualizado", cidade.DataAlteracao.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }

        public async Task AtualizarAsync(Cidade cidade)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Cidades SET
                    Nome = $nome,
                    Nivel = $nivel,
                    Populacao = $pop,
                    CapacidadeMaxima = $capacidade,
                    Recursos = $recursos,
                    Construcoes = $construcoes,
                    Trabalhadores = $trabalhadores,
                    UltimaAtualizacao = $atualizado
                WHERE Id = $id";

            command.Parameters.AddWithValue("$id", cidade.Id.ToString());
            command.Parameters.AddWithValue("$nome", cidade.Nome);
            command.Parameters.AddWithValue("$nivel", cidade.Nivel);
            command.Parameters.AddWithValue("$pop", cidade.Populacao);
            command.Parameters.AddWithValue("$capacidade", cidade.CapacidadeMaxima);
            command.Parameters.AddWithValue("$recursos", JsonSerializer.Serialize(cidade.Recursos));
            command.Parameters.AddWithValue("$construcoes", JsonSerializer.Serialize(cidade.Construcoes));
            command.Parameters.AddWithValue("$trabalhadores", JsonSerializer.Serialize(cidade.Trabalhadores));
            command.Parameters.AddWithValue("$atualizado", cidade.DataAlteracao.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> ExisteAsync(Guid cidadeId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM Cidades WHERE Id = $id";
            command.Parameters.AddWithValue("$id", cidadeId.ToString());

            var count = (long)await command.ExecuteScalarAsync();
            return count > 0;
        }


        public async Task<Cidade?> ObterPorProprietarioIdAsync(ulong usuarioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Cidades WHERE UsuarioId = $usuarioId LIMIT 1";
            command.Parameters.AddWithValue("$usuarioId", (long)usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Cidade
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    UsuarioId = (ulong)reader.GetInt64(1),
                    Nome = reader.GetString(2),
                    Nivel = reader.GetInt32(3),
                    Populacao = reader.GetInt32(4),
                    CapacidadeMaxima = reader.GetInt32(5),
                    Recursos = JsonSerializer.Deserialize<Recursos>(reader.GetString(6)) ?? new Recursos(),
                    Construcoes = JsonSerializer.Deserialize<List<Construcao>>(reader.GetString(7)) ?? new List<Construcao>(),
                    Trabalhadores = JsonSerializer.Deserialize<List<PersonagemTrabalhador>>(reader.GetString(8)) ?? new List<PersonagemTrabalhador>(),
                    DataCriacao = DateTime.Parse(reader.GetString(9)),
                    DataAlteracao = DateTime.Parse(reader.GetString(10))
                };
            }
            return null;
        }

        public async Task<List<Cidade>> ObterTodasAsync()
        {
            var cidades = new List<Cidade>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Cidades";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var cidade = new Cidade
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    UsuarioId = (ulong)reader.GetInt64(1),
                    Nome = reader.GetString(2),
                    Nivel = reader.GetInt32(3),
                    Populacao = reader.GetInt32(4),
                    CapacidadeMaxima = reader.GetInt32(5),
                    Recursos = JsonSerializer.Deserialize<Recursos>(reader.GetString(6)) ?? new Recursos(),
                    Construcoes = JsonSerializer.Deserialize<List<Construcao>>(reader.GetString(7)) ?? new List<Construcao>(),
                    Trabalhadores = JsonSerializer.Deserialize<List<PersonagemTrabalhador>>(reader.GetString(8)) ?? new List<PersonagemTrabalhador>(),
                    DataCriacao = DateTime.Parse(reader.GetString(9)),
                    DataAlteracao = DateTime.Parse(reader.GetString(10))
                };

                cidades.Add(cidade);
            }

            return cidades;
        }

    }
}
