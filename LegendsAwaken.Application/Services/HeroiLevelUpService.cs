namespace LegendsAwaken.Application.Services
{
    public class HeroiLevelUpService
    {
        public int CalcularPontosAtributosPorLevelUp(int nivelAtual, int raridadeOriginal)
        {
            if (raridadeOriginal == 1)
            {
                if (nivelAtual <= 40) return 2;
                else if (nivelAtual <= 80) return 3;
                else return 10;
            }
            else if (raridadeOriginal == 2)
            {
                if (nivelAtual <= 80) return 3;
                else return 10;
            }
            else if (raridadeOriginal == 3)
            {
                if (nivelAtual <= 80) return 4;
                else return 10;
            }
            else if (raridadeOriginal == 4)
            {
                if (nivelAtual <= 80) return 6;
                else return 10;
            }

            // Caso padrão ou inválido
            return 0;
        }
    }

}
