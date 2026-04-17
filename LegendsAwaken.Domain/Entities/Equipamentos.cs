using System;

namespace LegendsAwaken.Domain.Entities
{
    public class Equipamentos
    {
        public int Id { get; set; }
        public Guid? ArmaId { get; set; }
        public Guid? ArmaduraId { get; set; }
        public Guid? AcessorioId { get; set; }
    }
}
