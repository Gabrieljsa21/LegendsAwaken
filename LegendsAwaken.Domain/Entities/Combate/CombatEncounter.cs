namespace LegendsAwaken.Domain.Entities.Combate
{
    public class CombatEncounter
    {
        public List<Combatente> Aliados { get; set; } = new();
        public List<Combatente> Inimigos { get; set; } = new();
        public int Round { get; set; } = 0;
        public bool IsFinished { get; set; }
        public Combatente? Winner { get; set; }
    }
}
