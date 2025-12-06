using Test25.Entities;

namespace Test25.Entities
{
    public enum ProjectileType
    {
        Standard,
        Mirv,
        Dirt,
        Roller,
        Laser
    }

    public class Weapon : InventoryItem
    {
        public float Damage { get; set; }
        public float ExplosionRadius { get; set; }
        public string ProjectileTextureName { get; set; } // Simple way to differentiate textures if needed
        public ProjectileType Type { get; set; } = ProjectileType.Standard;
        public int SplitCount { get; set; } = 0; // For MIRV

        public Weapon(string name, string description, float damage, float explosionRadius, int count = 1, bool isInfinite = false, ProjectileType type = ProjectileType.Standard, int splitCount = 0)
            : base(name, description, count, isInfinite)
        {
            Damage = damage;
            ExplosionRadius = explosionRadius;
            Type = type;
            SplitCount = splitCount;
        }
    }
}
