using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System.Collections.Generic;

namespace Test25.Entities
{
    public class DirtProjectile : Projectile
    {
        public DirtProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
        }

        public override void OnHit(Terrain terrain, List<Tank> players)
        {
            // Add dirt instead of destroying it
            terrain.Construct((int)Position.X, (int)Position.Y, (int)ExplosionRadius);

            // Still damage players if direct hit? Maybe less damage or just bury them?
            // For now, let's deal small damage
            foreach (var player in players)
            {
                if (!player.IsActive) continue;
                float dist = Vector2.Distance(player.Position, Position);
                if (dist < ExplosionRadius + 20)
                {
                    player.TakeDamage(Damage / 2); // Half damage
                }
            }

            IsDead = true;
        }
    }
}
