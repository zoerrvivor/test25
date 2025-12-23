// Version: 0.1

namespace Test25.Gameplay.Entities
{
    public abstract class InventoryItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Count { get; set; } = 1;
        public bool IsInfinite { get; set; } = false;

        public InventoryItem(string name, string description, int count = 1, bool isInfinite = false)
        {
            Name = name;
            Description = description;
            Count = count;
            IsInfinite = isInfinite;
        }
    }
}
