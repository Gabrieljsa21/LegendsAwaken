using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Interfaces
{
    public interface IAtributoBonusService
    {
        AtributosBase ObterBonus(List<HeroiHabilidade> habilidades);
    }

}
