using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Test25.Core.Gameplay.Entities.Projectiles
{
    public class ExplosiveProjectile : Projectile
    {
        public ExplosiveProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
        }

        // Uses default OnHit which explodes
    }
}
