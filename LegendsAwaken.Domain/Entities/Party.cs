namespace LegendsAwaken.Domain.Entities
{
    public class Party
    {
        public Guid Id { get; set; }
        public ulong UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool NomeModoManual { get; set; } = false;
        public List<PartyHero> Membros { get; set; } = new();
    }

    public class PartyHero
    {
        public Guid PartyId { get; set; }
        public Party Party { get; set; } = null!;  // nav para Party

        public Guid HeroiId { get; set; }
        public Heroi Heroi { get; set; } = null!;  // nav para Heroi
    }
}
