using System;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services;

public sealed class EventoRng
{
    private readonly Random _rng;

    public EventoRng(int seed) => _rng = new Random(seed);

    public int Next(int min, int max) => _rng.Next(min, max);

    public double NextDouble() => _rng.NextDouble();

    public T Choose<T>(IList<T> items) => items[_rng.Next(items.Count)];

    public T EscolherPonderado<T>(IList<T> items, Func<T, int> peso)
    {
        int total = 0;
        foreach (var item in items) total += peso(item);
        int roll = _rng.Next(total);
        int acumulado = 0;
        foreach (var item in items)
        {
            acumulado += peso(item);
            if (roll < acumulado) return item;
        }
        return items[^1];
    }
}
