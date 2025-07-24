using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities.Auxiliares
{

    public class HeroiAfinidadeElemental
    {
        public Guid HeroiId { get; set; }
        public Heroi Heroi { get; set; } = null!;
        public Elemento Elemento { get; set; }
    }

    public class HeroiVinculo
    {
        public Guid HeroiId { get; set; }
        public Heroi Heroi { get; set; } = null!;
        public Guid VinculadoId { get; set; }

    }

    public class HeroiTag
    {
        public Guid HeroiId { get; set; }
        public Heroi Heroi { get; set; } = null!;
        public string Tag { get; set; } = string.Empty;

    }

    public class HeroiBonusAtributo
    {
        public Guid Id { get; set; }
        public Guid HeroiId { get; set; }
        public Heroi Heroi { get; set; } = null!;

        public Atributo Atributo { get; set; } // Forca, Agilidade, etc.
        public int Valor { get; set; }
        public OrigemBonusAtributo Origem { get; set; } // Profissao, Habilidade, Antecedente, Equipamento, etc.
    }

}
