using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services;

public static class PericiaEventoConfig
{
    public const double ChanceEventoPorAndar = 0.20;

    public static readonly IReadOnlyList<TestePericiaEvento> Eventos =
        new List<TestePericiaEvento>
        {
            new("Passagem estreita — equilíbrio ou quedas.",
                Pericia.Acrobacia, DC: 10, EhGrupo: false,
                "Progresso +5%", "Progresso -5%"),

            new("Rastros de inimigos — seguir ou perder.",
                Pericia.Sobrevivencia, DC: 12, EhGrupo: false,
                "Rota ótima: +3% progresso", "Rota errada: -8% progresso"),

            new("Armadilha arcana bloqueia a passagem.",
                Pericia.Arcanismo, DC: 15, EhGrupo: true,
                "Desarmada: +10% progresso", "Ativada: -15% progresso"),

            new("Patrulha inimiga pode ser evitada.",
                Pericia.Furtividade, DC: 12, EhGrupo: true,
                "Passagem silenciosa: +5% progresso", "Emboscada: -10% progresso"),

            new("Negociação com mercador hostil.",
                Pericia.Persuasao, DC: 12, EhGrupo: false,
                "Aliado temporário: +8% progresso", "Recusado: sem efeito"),

            new("Escuridão total — percepção salva.",
                Pericia.Percepcao, DC: 10, EhGrupo: false,
                "Caminho seguro: +5%", "Armadilha: -10% progresso"),
        }.AsReadOnly();
}
