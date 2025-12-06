using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Managers;
using Test25.World;

namespace Test25.Entities
{
    public class DebrisProjectile : Projectile
    {
        public DebrisProjectile(Vector2 position, Vector2 velocity, Texture2D texture, float damage, float explosionRadius)
            : base(position, velocity, texture)
        {
            Damage = damage;
            ExplosionRadius = explosionRadius;
        }

        public override void OnHit(GameManager gameManager)
        {
            // Debris always explodes on impact
            gameManager.Terrain.Destruct((int)Position.X, (int)Position.Y, (int)ExplosionRadius);
            gameManager.AddExplosion(Position, ExplosionRadius, Color.DarkOrange); // Example color for debris explosion

            base.OnHit(gameManager);
        }
    }
}
