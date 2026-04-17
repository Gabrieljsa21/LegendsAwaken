using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;

namespace LegendsAwaken.Domain.Entities
{
    public class Item
    {
        public Guid Id { get; set; }
        public required string Nome { get; set; }
        public SlotEquipamento Slot { get; set; }
        public Qualidade Qualidade { get; set; }
        public ulong ProprietarioId { get; set; }
        public bool EstaEquipado { get; set; }
        public Guid? HeroiEquipadoId { get; set; }
        public List<ItemBonus> Bonus { get; set; } = new();
    }

    public class ItemBonus
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public Atributo Atributo { get; set; }
        public int Valor { get; set; }
    }
}
