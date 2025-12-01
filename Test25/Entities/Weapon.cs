// Version: 0.1
namespace Test25.Entities
{
    public class Weapon : InventoryItem
    {
        public float Damage { get; set; }
        public float ExplosionRadius { get; set; }
        public string ProjectileTextureName { get; set; } // Simple way to differentiate textures if needed

        public Weapon(string name, string description, float damage, float explosionRadius, int count = 1, bool isInfinite = false)
            : base(name, description, count, isInfinite)
        {
            Damage = damage;
            ExplosionRadius = explosionRadius;
        }
    }
}
