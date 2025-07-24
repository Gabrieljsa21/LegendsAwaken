using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class AtributoBonusService : IAtributoBonusService
    {
        public AtributosBase ObterBonus(List<HeroiHabilidade> habilidadesHeroi)
        {
            var totalBonus = new AtributosBase();

            if (habilidadesHeroi == null)
                return totalBonus;

            foreach (var heroHabilidade in habilidadesHeroi)
            {
                if (heroHabilidade.Habilidade?.HabilidadeBonusAtributos == null)
                    continue;

                foreach (var b in heroHabilidade.Habilidade.HabilidadeBonusAtributos)
                {
                    if (b.BonusTipo != BonusTipo.Atributo)
                        continue;

                    int valorComNivel = b.BonusValor * heroHabilidade.Nivel;

                    switch (b.Atributo)
                    {
                        case Atributo.Forca:
                            totalBonus.Forca += valorComNivel;
                            break;
                        case Atributo.Agilidade:
                            totalBonus.Agilidade += valorComNivel;
                            break;
                        case Atributo.Vitalidade:
                            totalBonus.Vitalidade += valorComNivel;
                            break;
                        case Atributo.Inteligencia:
                            totalBonus.Inteligencia += valorComNivel;
                            break;
                        case Atributo.Percepcao:
                            totalBonus.Percepcao += valorComNivel;
                            break;
                    }
                }
            }

            return totalBonus;
        }

    }


}
