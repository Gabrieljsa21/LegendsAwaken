using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class RacaService
    {
        public Task<List<string>> ObterTodasIdsAsync()
        {
            var racas = Enum.GetValues(typeof(Raca))
                .Cast<Raca>()
                .Select(r => r.ToString())
                .ToList();

            return Task.FromResult(racas);
        }
    }
}
