using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Domain.Entities
{
    public class AtributosBase
    {
        // Adding a new attribute:
        //   1. Add value to Atributo enum (Enums.cs)
        //   2. Add property here
        //   3. Add two lines to Get() and Set() below
        //   Everything else adapts automatically via Enum.GetValues<Atributo>().
        public int Forca        { get; set; }
        public int Destreza     { get; set; }
        public int Constituicao { get; set; }
        public int Inteligencia { get; set; }
        public int Sabedoria    { get; set; }
        public int Carisma      { get; set; }

        public int Get(Atributo attr) => attr switch
        {
            Atributo.Forca        => Forca,
            Atributo.Destreza     => Destreza,
            Atributo.Constituicao => Constituicao,
            Atributo.Inteligencia => Inteligencia,
            Atributo.Sabedoria    => Sabedoria,
            Atributo.Carisma      => Carisma,
            _                     => 0
        };

        public void Set(Atributo attr, int value)
        {
            switch (attr)
            {
                case Atributo.Forca:        Forca        = value; break;
                case Atributo.Destreza:     Destreza     = value; break;
                case Atributo.Constituicao: Constituicao = value; break;
                case Atributo.Inteligencia: Inteligencia = value; break;
                case Atributo.Sabedoria:    Sabedoria    = value; break;
                case Atributo.Carisma:      Carisma      = value; break;
            }
        }

        public static AtributosBase operator +(AtributosBase a, AtributosBase b)
        {
            var result = new AtributosBase();
            foreach (var attr in System.Enum.GetValues<Atributo>())
                result.Set(attr, a.Get(attr) + b.Get(attr));
            return result;
        }

        public void AdicionarPorTipo(Atributo tipo, int valor)
            => Set(tipo, Get(tipo) + valor);

        public static AtributosBase Distribute(int total)
        {
            var attrs = System.Enum.GetValues<Atributo>();
            int perAttr = total / attrs.Length;
            var result = new AtributosBase();
            foreach (var attr in attrs)
                result.Set(attr, perAttr);
            return result;
        }

        public static AtributosBase With(Atributo attr, int value)
        {
            var result = new AtributosBase();
            result.Set(attr, value);
            return result;
        }

        public IEnumerable<(Atributo Atributo, int Valor)> ToEnumerable()
            => System.Enum.GetValues<Atributo>().Select(a => (a, Get(a)));
    }
}
