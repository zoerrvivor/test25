// Version: 0.1
using System;

namespace Test25.Entities
{
    public enum ItemType
    {
        Instant, // Used immediately upon purchase or selection
        Passive, // Automatically used when needed (e.g. Parachute)
        Active   // Activated by player
    }

    public class Item : InventoryItem
    {
        public ItemType Type { get; set; }
        public Action<Tank> Effect { get; set; }

        public Item(string name, string description, ItemType type, Action<Tank> effect, int count = 1)
            : base(name, description, count, false)
        {
            Type = type;
            Effect = effect;
        }
    }
}
