using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities
{
    public class SlotOcupacao
    {
        public Guid Id { get; set; }
        public Guid ConstrucaoId { get; set; }
        public Guid HeroiId { get; set; }
        public SlotTipo SlotTipo { get; set; }
        public int PosicaoSlot { get; set; }
    }
}
