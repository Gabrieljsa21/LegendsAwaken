using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Services;

public static class HeroiGuard
{
    public static string? ValidarDisponivel(Heroi heroi)
    {
        if (heroi.EstadoSustento == EstadoSustento.Degradado)
            return $"{heroi.Nome} está degradado (sem sustento). Produza Comida antes de continuar.";
        if (heroi.EstadoSustento == EstadoSustento.Inativo)
            return $"{heroi.Nome} está inativo e não pode ser usado em combate.";
        return null;
    }

    public static string? ValidarTodosDisponiveis(IEnumerable<Heroi> herois)
        => herois.Select(ValidarDisponivel).FirstOrDefault(m => m != null);
}
