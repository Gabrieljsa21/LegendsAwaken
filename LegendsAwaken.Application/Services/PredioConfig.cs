using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services
{
    public record SlotDefinicao(
        int NumResponsabilidade,
        int ConfiancaMin,
        Atributo AtributoReq,
        int AtributoMin,
        int NumOperacao,
        int BaseProdPorHora  // 0 for non-production buildings (Forja, Arena, Guilda)
    );

    public record ConstrucaoCusto(
        int Ouro,
        int Madeira,
        int Pedra,
        int Comida
    );

    public static class PredioConfig
    {
        // (TipoPredio, Nivel) → SlotDefinicao
        public static readonly IReadOnlyDictionary<(TipoPredio, int), SlotDefinicao> Slots =
            new Dictionary<(TipoPredio, int), SlotDefinicao>
            {
                { (TipoPredio.Fazenda,  1), new(1, 0,  Atributo.Vitalidade, 10, 2, 8)  },
                { (TipoPredio.Fazenda,  2), new(1, 0,  Atributo.Vitalidade, 20, 3, 14) },
                { (TipoPredio.Fazenda,  3), new(2, 20, Atributo.Vitalidade, 35, 4, 20) },
                { (TipoPredio.Serraria, 1), new(1, 0,  Atributo.Forca,      10, 2, 6)  },
                { (TipoPredio.Serraria, 2), new(1, 0,  Atributo.Forca,      20, 3, 10) },
                { (TipoPredio.Serraria, 3), new(2, 20, Atributo.Forca,      35, 4, 15) },
                { (TipoPredio.Mina,     1), new(1, 0,  Atributo.Forca,      10, 2, 5)  },
                { (TipoPredio.Mina,     2), new(1, 0,  Atributo.Forca,      20, 3, 9)  },
                { (TipoPredio.Mina,     3), new(2, 20, Atributo.Forca,      35, 4, 13) },
                { (TipoPredio.Forja,    1), new(1, 0,  Atributo.Forca,      10, 1, 0)  },
                { (TipoPredio.Forja,    2), new(1, 0,  Atributo.Forca,      25, 2, 0)  },
                { (TipoPredio.Forja,    3), new(2, 60, Atributo.Forca,      40, 3, 0)  },
                { (TipoPredio.Arena,    1), new(1, 0,  Atributo.Forca,      10, 2, 0)  },
                { (TipoPredio.Arena,    2), new(1, 0,  Atributo.Forca,      20, 3, 0)  },
                { (TipoPredio.Guilda,   1), new(1, 0,  Atributo.Agilidade,  10, 2, 0)  },
                { (TipoPredio.Guilda,   2), new(1, 0,  Atributo.Agilidade,  20, 3, 0)  },
            };

        // Construction costs for Nivel 1 of each building
        public static readonly IReadOnlyDictionary<TipoPredio, ConstrucaoCusto> CustosConstrucao =
            new Dictionary<TipoPredio, ConstrucaoCusto>
            {
                { TipoPredio.Fazenda,  new(50,  40, 0,  0) },
                { TipoPredio.Serraria, new(30,  0,  50, 0) },
                { TipoPredio.Mina,     new(40,  0,  20, 0) },
                { TipoPredio.Forja,    new(80,  0,  60, 0) },
                { TipoPredio.Arena,    new(100, 50, 50, 0) },
                { TipoPredio.Guilda,   new(100, 50, 50, 0) },
            };

        // Which resource does this building produce?
        public static readonly IReadOnlyDictionary<TipoPredio, string?> RecursoProducao =
            new Dictionary<TipoPredio, string?>
            {
                { TipoPredio.Fazenda,  "comida"  },
                { TipoPredio.Serraria, "madeira" },
                { TipoPredio.Mina,     "pedra"   },
                { TipoPredio.Forja,    null       },
                { TipoPredio.Arena,    null       },
                { TipoPredio.Guilda,   null       },
            };
    }
}
