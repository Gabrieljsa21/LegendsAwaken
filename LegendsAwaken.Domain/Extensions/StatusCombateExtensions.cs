using LegendsAwaken.Domain.Entities;
using System;

namespace LegendsAwaken.Domain.Extensions
{
    public static class StatusCombateExtensions
    {
        // HP = 8 (default base) + nivel + MOD_CON (floor((CON-10)/2))
        // For heroes, CriarHeroiAsync overrides VidaMaxima using ProfissaoConfig.BaseHpPorProfissao.
        public static StatusCombate FromAtributos(this AtributosBase atr, int nivel = 1)
        {
            int modCon = (int)Math.Floor((atr.Constituicao - 10.0) / 2.0);
            int hp = 8 + nivel + modCon;
            if (hp < 1) hp = 1;
            return new StatusCombate
            {
                VidaMaxima = hp,
                VidaAtual  = hp,
                ManaMaxima = atr.Inteligencia * 5,
                ManaAtual  = atr.Inteligencia * 5
            };
        }
    }
}
