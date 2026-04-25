using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface ITorreBoosterRepository
{
    Task EnsureTableAsync();
    Task<int> ObterQuantidadeAsync(Guid usuarioId, TipoBooster tipo);
    Task<List<(TipoBooster Tipo, int Quantidade)>> ListarAsync(Guid usuarioId);
    Task AdicionarAsync(Guid usuarioId, TipoBooster tipo, int quantidade);
    Task<bool> ConsumirAsync(Guid usuarioId, TipoBooster tipo); // returns false if none available
}
