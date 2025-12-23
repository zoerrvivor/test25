using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Gameplay.Managers;


namespace Test25.Gameplay.Entities.Projectiles
{
    public class DirtProjectile : Projectile
    {
        public DirtProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
        }

        public override void OnHit(GameManager gameManager)
        {
            // Add dirt instead of destroying it
            gameManager.Terrain.Construct((int)Position.X, (int)Position.Y, (int)ExplosionRadius);

            // Visual effect for dirt
            gameManager.AddExplosion(Position, ExplosionRadius, Color.SaddleBrown);
            // The Explosion class supports color tinting in Draw, but currently takes texture color or hardcoded color.
            // Explosion class:
            // _baseColor = Color.OrangeRed;
            // It doesn't accept color in constructor.
            // I should update Explosion to accept color or expose it.
            // Implementation plan didn't specify color parameter for Explosion constructor.
            // But I can overload it or change it.
            // Let's first update the signature.

            // Still damage players if direct hit? Maybe less damage or just bury them?
            // For now, let's deal small damage
            foreach (var player in gameManager.Players)
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
