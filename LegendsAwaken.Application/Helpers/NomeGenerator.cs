using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Helpers
{
    public class NomeGenerator
    {
        private readonly Dictionary<Raca, (string[] prefixos, string[] sufixos)> _nomesPorRaca = new()
        {
            [Raca.Humano] = (
                new[] {
                        "Al", "Bran", "Ced", "Dan", "Edr", "Har", "Jor", "Kev", "Lor", "Mar",
                        "Ela", "Vea", "Nia", "Sira", "Fae", "Lia", "My", "Zia", "Ara", "Yve",
                        "Tia", "Ina", "Rae", "Mae", "Sel", "Val", "Ser", "Nyx", "Lun", "Cia",
                        "Nya", "Ely", "Fia", "Gia", "Rin", "Aya", "Zea", "Noa", "Yla", "Syl",
                        "Vyn", "Kya", "Lia", "Sae", "Tyn", "Wyn", "Rya", "Myn", "Zyn", "Dae"
                },
                new[] {
                        "ric", "ton", "ald", "den", "wyn", "don", "ver", "son", "lin", "gar",
                        "lia", "na", "ra", "sa", "la", "tha", "ria", "nia", "va", "lia",
                        "len", "wen", "fin", "min", "rin", "sin", "lyn", "ryn", "tin", "vin",
                        "dell", "fell", "gwen", "hal", "kel", "nel", "pel", "rel", "sel", "tel",
                        "ven", "zel", "yl", "ar", "el", "il", "or", "ur", "an", "en"
                }),

            [Raca.Bestial] = (
                new[] { "Gor", "Rok", "Thar", "Mak", "Zug", "Grak", "Ur", "Vol", "Drek", "Kaz", "Ria", "Zea", "Sha", "Lia", "Nia", "Fae", "Vea", "Mira", "Zia", "Ara" },
                new[] { "nak", "gar", "mog", "ruk", "zor", "tok", "nash", "ruk", "mar", "gul", "a", "ia", "ea", "na", "ra", "sa", "tha", "la", "ya", "va" }),

            [Raca.Anao] = (
                new[] { "Bald", "Durr", "Fund", "Krag", "Throm", "Gunn", "Thar", "Varn", "Mor", "Dor", "Dala", "Mira", "Nala", "Fina", "Rina", "Vira", "Zina", "Tara", "Sina", "Lira" },
                new[] { "in", "ar", "dun", "mir", "grin", "grom", "rak", "grim", "gar", "bek", "na", "ra", "la", "ta", "sa", "ma", "va", "za", "fa", "ya" }),

            [Raca.Elfo] = (
                new[] { "Ael", "Lia", "Syl", "Thel", "Elar", "Myra", "Cael", "Faer", "Thia", "Zin", "Aira", "Elia", "Sila", "Thea", "Eira", "Mira", "Nira", "Tila", "Zira", "Yela" },
                new[] { "riel", "dril", "wyn", "siel", "nor", "loth", "thil", "veth", "myr", "lira", "na", "ra", "sa", "la", "tha", "ria", "nia", "va", "lia", "ma" }),

            [Raca.Draconato] = (
                new[] { "Zar", "Vor", "Kra", "Ska", "Nyr", "Drak", "Thaz", "Rhaz", "Vorr", "Zhul", "Zira", "Vora", "Kara", "Skia", "Nyra", "Draka", "Thaza", "Rha", "Vora", "Zhula" },
                new[] { "zeth", "kar", "thos", "mir", "vax", "dros", "gar", "vyr", "xen", "roth", "a", "ia", "ra", "sa", "la", "ta", "va", "na", "ma", "za" }),

            [Raca.Fada] = (
                new[] { "Pix", "Fay", "Nym", "Twi", "Lira", "Zel", "Miri", "Tula", "Wyn", "Sari", "Aela", "Faya", "Nima", "Tina", "Lina", "Zina", "Mina", "Tara", "Wina", "Sara" },
                new[] { "elle", "wyn", "flit", "belle", "dora", "fizz", "lune", "shine", "dria", "lace", "na", "ra", "sa", "la", "tha", "ria", "nia", "va", "lia", "ma" }),
        };

        private readonly Dictionary<Raca, (string[] prefixos, string[] sufixos)> _sobrenomesPorRaca = new()
        {
            [Raca.Humano] = (
                new[] {
                        "Silver", "Moon", "Star", "Sun", "Wind", "Shadow", "Light", "Dawn", "Night", "Rain",
                        "Frost", "Storm", "Ash", "Flame", "Bright", "Dark", "Swift", "Gold", "Snow", "Fire",
                        "Sky", "Mist", "Stone", "Wolf", "Heart", "Leaf", "Vale", "Brook", "Ember", "Glade",
                        "Hawk", "Lark", "Rose", "Sage", "Thorn", "Willow", "Zephyr", "Blaze", "Echo", "Fawn",
                        "Gale", "Hollow", "Ivy", "Jade", "Kestrel", "Loom", "Marsh", "Noble", "Oak", "Pine"
                },
                new[] {
                        "bourne", "field", "wood", "mere", "ford", "stone", "brook", "hill", "well", "ridge",
                        "vale", "cliff", "shore", "port", "crest", "grove", "haven", "moor", "peak", "fall",
                        "marsh", "glade", "spring", "dell", "ridge", "barrow", "cove", "fall", "gate", "hollow",
                        "knoll", "ledge", "meadow", "pool", "run", "stead", "view", "water", "wharf", "wind",
                        "field", "holt", "stead", "bourne", "fell", "mere", "row", "wood", "yard", "wick"
                }),

            [Raca.Bestial] = (
                new[] { "Rag", "Tor", "Gruk", "Zan", "Vok", "Laz", "Brug", "Urk", "Maz", "Nog", "Ara", "Ira", "Sira", "Nara", "Mira", "Tira", "Fira", "Vira", "Zira", "Lira" },
                new[] { "fang", "snarl", "tooth", "claw", "hide", "horn", "scar", "maw", "gut", "paw", "a", "ia", "ea", "na", "ra", "sa", "tha", "la", "ya", "va" }),

            [Raca.Anao] = (
                new[] { "Bron", "Dur", "Thrag", "Mor", "Bar", "Kuld", "Ston", "Grim", "Thrum", "Dorn", "Nara", "Mira", "Lira", "Sara", "Tira", "Vira", "Zira", "Fira", "Sera", "Nira" },
                new[] { "breaker", "forge", "hammer", "beard", "stone", "delver", "carver", "cleaver", "keeper", "helm", "na", "ra", "sa", "la", "tha", "ria", "nia", "va", "lia", "ma" }),

            [Raca.Elfo] = (
                new[] { "Alth", "Cael", "Elen", "Fael", "Gal", "Illy", "Naer", "Quel", "Sael", "Yril", "Aela", "Eira", "Lira", "Sira", "Nira", "Tira", "Vira", "Zira", "Fira", "Sara" },
                new[] { "thir", "veth", "loth", "dell", "mir", "nore", "sira", "wyn", "mira", "lanth", "na", "ra", "sa", "la", "tha", "ria", "nia", "va", "lia", "ma" }),

            [Raca.Draconato] = (
                new[] { "Drak", "Thaz", "Rhaz", "Zorr", "Vorr", "Xal", "Kraz", "Ghaz", "Uzor", "Brak", "Zira", "Vora", "Kara", "Skia", "Nyra", "Draka", "Thaza", "Rha", "Vora", "Zhula" },
                new[] { "tharn", "rax", "dor", "zeth", "vorn", "kosh", "gorn", "vor", "zen", "nox", "a", "ia", "ra", "sa", "la", "ta", "va", "na", "ma", "za" }),

            [Raca.Fada] = (
                new[] { "Twil", "Glim", "Star", "Sun", "Lume", "Daz", "Shay", "Wisp", "Laz", "Fizz", "Aela", "Faya", "Nima", "Tina", "Lina", "Zina", "Mina", "Tara", "Wina", "Sara" },
                new[] { "dust", "gleam", "shine", "flutter", "twist", "glen", "leaf", "spark", "petal", "whirl", "na", "ra", "sa", "la", "tha", "ria", "nia", "va", "lia", "ma" }),
        };

        private readonly HashSet<string> _nomesCompletosGerados = new();
        private readonly Random _random = new(Guid.NewGuid().GetHashCode());

        public string GerarNome(Raca raca)
        {
            if (!_nomesPorRaca.TryGetValue(raca, out var partesNome))
                partesNome = (new[] { "X" }, new[] { "x" });

            if (!_sobrenomesPorRaca.TryGetValue(raca, out var partesSobrenome))
                partesSobrenome = (new[] { "X" }, new[] { "x" });

            string nomeCompleto;
            int tentativas = 0;

            do
            {
                var prefixoNome = partesNome.prefixos[_random.Next(partesNome.prefixos.Length)];
                var sufixoNome = partesNome.sufixos[_random.Next(partesNome.sufixos.Length)];
                var prefixoSobrenome = partesSobrenome.prefixos[_random.Next(partesSobrenome.prefixos.Length)];
                var sufixoSobrenome = partesSobrenome.sufixos[_random.Next(partesSobrenome.sufixos.Length)];

                nomeCompleto = $"{prefixoNome}{sufixoNome} {prefixoSobrenome}{sufixoSobrenome}";
                tentativas++;
            }
            while (_nomesCompletosGerados.Contains(nomeCompleto) && tentativas < 100);

            _nomesCompletosGerados.Add(nomeCompleto);
            return nomeCompleto;
        }
    }
}
