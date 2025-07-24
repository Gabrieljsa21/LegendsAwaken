using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities
{
    public class AtributosBase
    {
        public int Forca { get; set; }
        public int Agilidade { get; set; }
        public int Vitalidade { get; set; }
        public int Inteligencia { get; set; }
        public int Percepcao { get; set; }

        public static AtributosBase operator +(AtributosBase a, AtributosBase b)
        {
            return new AtributosBase
            {
                Forca = a.Forca + b.Forca,
                Agilidade = a.Agilidade + b.Agilidade,
                Vitalidade = a.Vitalidade + b.Vitalidade,
                Inteligencia = a.Inteligencia + b.Inteligencia,
                Percepcao = a.Percepcao + b.Percepcao
            };
        }

        public void AdicionarPorTipo(Atributo tipo, int valor)
        {
            switch (tipo)
            {
                case Atributo.Forca: Forca += valor; break;
                case Atributo.Agilidade: Agilidade += valor; break;
                case Atributo.Vitalidade: Vitalidade += valor; break;
                case Atributo.Inteligencia: Inteligencia += valor; break;
                case Atributo.Percepcao: Percepcao += valor; break;
            }
        }
    }

}
