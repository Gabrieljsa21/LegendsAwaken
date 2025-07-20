using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Bot.Models.Banner;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class GachaService
    {
        private readonly IHeroiRepository _heroiRepository;
        private readonly Random _random;

        public GachaService(IHeroiRepository heroiRepository)
        {
            _heroiRepository = heroiRepository;
            _random = new Random();
        }

        public Raridade SortearRaridade(BannerConfiguracao banner, int rollsFeitos)
        {
            var pityMax = banner.PityMaximo;
            var raridadeChances = banner.RaridadeChances;

            // Cópia mutável das chances
            var chances = new Dictionary<Raridade, double>(raridadeChances.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value));

            if (rollsFeitos >= pityMax)
                return Raridade.Estrela4;

            double pityProgress = (double)rollsFeitos / pityMax;
            double extraChance = Math.Pow(pityProgress, 3) * 100;   //O número que controla a curva é o expoente 3 em Math.Pow(pityProgress, 3): quanto maior esse valor, mais lenta a progressão no início e mais abrupta no final; reduza para amaciar o soft pity ou aumente para torná-lo mais rígido.

            // Aplica chance extra à Estrela4
            chances[Raridade.Estrela4] += extraChance;

            Console.WriteLine($"Chance atual de 4⭐ com soft pity: {chances[Raridade.Estrela4]:F2}%");

            // Reduz proporcionalmente das outras
            double totalBase = raridadeChances.Values.Sum();

            foreach (var raridade in chances.Keys.Where(r => r != Raridade.Estrela4).ToList())
            {
                double baseChance = raridadeChances[raridade];
                double redução = (baseChance / totalBase) * extraChance;
                chances[raridade] = Math.Max(0, chances[raridade] - redução);
            }

            // Sorteio com chances ajustadas
            double total = chances.Values.Sum();
            double roll = _random.NextDouble() * total;
            double acumulado = 0;

            foreach (var par in chances.OrderByDescending(p => p.Key)) // Do mais raro ao comum
            {
                acumulado += par.Value;
                if (roll <= acumulado)
                    return par.Key;
            }

            return Raridade.Estrela1; // fallback
        }

        public string SortearRaca(Raridade raridade, List<string> todasRacas)
        {
            const string humanoId = "Humano";

            // Filtra outras raças que não são humanas
            var outrasRacas = todasRacas.Where(r => r != humanoId).ToList();

            double chanceHumano = raridade switch
            {
                Raridade.Estrela1 => 100,
                Raridade.Estrela2 => 100,
                Raridade.Estrela3 => 90,
                Raridade.Estrela4 => 75,
                _ => 100
            };

            double roll = _random.NextDouble() * 100;
            if (roll <= chanceHumano || !outrasRacas.Any())
            {
                return humanoId;
            }

            // Sorteia uniformemente entre as outras raças
            int index = _random.Next(outrasRacas.Count);
            return outrasRacas[index];
        }

    }
}
