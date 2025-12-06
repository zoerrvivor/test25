using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System.Collections.Generic;

namespace Test25.Entities
{
    public class MirvProjectile : Projectile
    {
        public int SplitCount { get; set; } = 3;
        public bool HasSplit { get; private set; } = false;
        private float _peakY;
        private bool _goingDown = false;

        // We need a way to spawn new projectiles. 
        // Since we don't have reference to GameManager here, we might need a callback or a list to add to.
        // For now, we can expose a list of new projectiles.
        public List<Projectile> NewProjectiles { get; private set; } = new List<Projectile>();

        public MirvProjectile(Vector2 position, Vector2 velocity, Texture2D texture, int splitCount)
            : base(position, velocity, texture)
        {
            SplitCount = splitCount;
            _peakY = position.Y;
        }

        public override void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            float prevY = Position.Y;
            base.UpdatePhysics(gameTime, wind, gravity);

            if (Position.Y < _peakY) _peakY = Position.Y;

            if (Position.Y > prevY) // Going down
            {
                if (!_goingDown)
                {
                    _goingDown = true;
                    // Apex reached (roughly), split now!
                    Split();
                }
            }
        }

        private void Split()
        {
            if (HasSplit) return;
            HasSplit = true;

            for (int i = 0; i < SplitCount; i++)
            {
                // Spread velocities
                float spread = (i - (SplitCount - 1) / 2f) * 50f;
                Vector2 newVel = Velocity + new Vector2(spread, -50); // Pop up a bit

                var p = new ExplosiveProjectile(Position, newVel, Texture);
                p.ExplosionRadius = ExplosionRadius;
                p.Damage = Damage;
                NewProjectiles.Add(p);
            }

            IsDead = true; // The main casing is gone
        }
    }
}
