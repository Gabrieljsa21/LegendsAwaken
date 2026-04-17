using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Domain.Entities
{
    public class AtributosBase
    {
        // ── EF Core columns (one property per attribute) ───────────────────────
        // Adding a new attribute:
        //   1. Add value to the Atributo enum (Enums.cs)
        //   2. Add property here
        //   3. Add two lines to Get() and Set() below
        //   Everything else (operator +, AdicionarPorTipo, Distribute, With,
        //   ToEnumerable, BonusRacial, ObterAtributosBaseParaRaridade) adapts
        //   automatically via Enum.GetValues<Atributo>().
        public int Forca        { get; set; }
        public int Agilidade    { get; set; }
        public int Vitalidade   { get; set; }
        public int Inteligencia { get; set; }
        public int Percepcao    { get; set; }

        // ── Indexed accessor ──────────────────────────────────────────────────
        // The two canonical places to update when adding a new attribute.

        public int Get(Atributo attr) => attr switch
        {
            Atributo.Forca        => Forca,
            Atributo.Agilidade    => Agilidade,
            Atributo.Vitalidade   => Vitalidade,
            Atributo.Inteligencia => Inteligencia,
            Atributo.Percepcao    => Percepcao,
            _                     => 0
        };

        public void Set(Atributo attr, int value)
        {
            switch (attr)
            {
                case Atributo.Forca:        Forca        = value; break;
                case Atributo.Agilidade:    Agilidade    = value; break;
                case Atributo.Vitalidade:   Vitalidade   = value; break;
                case Atributo.Inteligencia: Inteligencia = value; break;
                case Atributo.Percepcao:    Percepcao    = value; break;
            }
        }

        // ── Generic operations — never need editing when attributes change ─────

        /// <summary>
        /// Adds two AtributosBase together across all attributes in the enum.
        /// </summary>
        public static AtributosBase operator +(AtributosBase a, AtributosBase b)
        {
            var result = new AtributosBase();
            foreach (var attr in System.Enum.GetValues<Atributo>())
                result.Set(attr, a.Get(attr) + b.Get(attr));
            return result;
        }

        /// <summary>
        /// Increments a specific attribute. Replaces the old switch statement.
        /// </summary>
        public void AdicionarPorTipo(Atributo tipo, int valor)
            => Set(tipo, Get(tipo) + valor);

        /// <summary>
        /// Distributes <paramref name="total"/> evenly across all attributes.
        /// Automatically scales to the number of values in <see cref="Atributo"/>.
        /// </summary>
        public static AtributosBase Distribute(int total)
        {
            var attrs = System.Enum.GetValues<Atributo>();
            int perAttr = total / attrs.Length;
            var result = new AtributosBase();
            foreach (var attr in attrs)
                result.Set(attr, perAttr);
            return result;
        }

        /// <summary>
        /// Creates an instance with a single attribute set to <paramref name="value"/>.
        /// Use for racial bonuses and targeted modifiers.
        /// </summary>
        public static AtributosBase With(Atributo attr, int value)
        {
            var result = new AtributosBase();
            result.Set(attr, value);
            return result;
        }

        /// <summary>
        /// Iterates all attributes in enum declaration order.
        /// Use for display, serialization, and generic math.
        /// </summary>
        public IEnumerable<(Atributo Atributo, int Valor)> ToEnumerable()
            => System.Enum.GetValues<Atributo>().Select(a => (a, Get(a)));
    }
}
