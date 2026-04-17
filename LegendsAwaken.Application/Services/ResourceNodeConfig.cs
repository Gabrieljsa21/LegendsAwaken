using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services
{
    public static class ResourceNodeConfig
    {
        // Base rate per hero per hour
        public static readonly IReadOnlyDictionary<TipoResourceNode, (string recurso, int basePorHora)> BaseRates =
            new Dictionary<TipoResourceNode, (string, int)>
            {
                { TipoResourceNode.Campo,    ("comida",  6) },
                { TipoResourceNode.Floresta, ("madeira", 5) },
                { TipoResourceNode.Mina,     ("pedra",   4) },
                { TipoResourceNode.Prado,    ("erva",    3) },
            };

        // Profession bonuses as multiplier additions (e.g. 0.5 = +50%)
        public static readonly IReadOnlyDictionary<(TipoResourceNode, Profissao), double> ProfissaoBonus =
            new Dictionary<(TipoResourceNode, Profissao), double>
            {
                { (TipoResourceNode.Campo,    Profissao.Agricultor), 0.50 },
                { (TipoResourceNode.Campo,    Profissao.Cozinheiro), 0.25 },
                { (TipoResourceNode.Floresta, Profissao.Lenhador),   0.50 },
                { (TipoResourceNode.Floresta, Profissao.Caçador),    0.30 },
                { (TipoResourceNode.Mina,     Profissao.Mineiro),    0.60 },
                { (TipoResourceNode.Prado,    Profissao.Agricultor), 0.40 },
                { (TipoResourceNode.Prado,    Profissao.Caçador),    0.25 },
            };
    }
}
