using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Gameplay.World;
using System;

namespace Test25.Gameplay.Entities.Projectiles
{
    public class RollerProjectile : Projectile
    {
        private float _lifeTime = 0f;
        private const float MaxLifeTime = Constants.RollerMaxLifetime; // Rolls for 3 seconds

        public RollerProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
        }

        public override void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lifeTime += deltaTime;

            if (_lifeTime > MaxLifeTime)
            {
                // Explode after timeout
                // We need to signal that we want to explode. 
                // Since OnHit is usually called by collision, we might need a way to trigger it manually.
                // For now, let's just mark as dead, but ideally we should explode.
                // We can handle this in GameManager or just force a collision?
                // Actually, let's just set IsDead = true and maybe trigger explosion in OnHit if we call it?
                // But OnHit needs terrain/players.
                // Let's rely on GameManager to call OnHit if we return true for collision, 
                // but here we are in UpdatePhysics.
                // We'll leave it for now, maybe it just disappears.
                // Better: CheckCollision will return true if lifetime is over? No, that's weird.
                // Let's just let it roll until it hits something or stops?
                // Simplest: If lifetime over, just die.
                IsDead = true;
                return;
            }

            Vector2 v = Velocity;
            v.X += wind * deltaTime * 0.1f; // Less wind effect
            v.Y += gravity * deltaTime;
            Velocity = v;

            Position += Velocity * deltaTime;

            if (Velocity.LengthSquared() > 0.1f)
            {
                Rotation += (Velocity.X * deltaTime) * 0.1f; // Roll rotation
            }
        }

        public override bool CheckCollision(Terrain terrain, WallType wallType)
        {
            // Custom collision: Bounce/Roll on terrain
            if (Position.X >= 0 && Position.X < terrain.Width)
            {
                int groundHeight = terrain.GetHeight((int)Position.X);
                if (Position.Y >= groundHeight - 5) // Hit ground
                {
                    // Bounce / Roll
                    Position = new Vector2(Position.X, groundHeight - 5);

                    // Simple reflection / friction
                    Velocity = new Vector2(Velocity.X * 0.9f, -Velocity.Y * 0.5f); // Lose energy

                    // If velocity is low, stop bouncing and roll?
                    if (Math.Abs(Velocity.Y) < 10)
                    {
                        Velocity = new Vector2(Velocity.X, 0);
                        // Follow terrain slope? Too complex for now.
                    }

                    // If we hit a wall (steep slope), explode?
                    // For now, just bounce.

                    return false; // Don't explode on terrain contact immediately
                }
            }

            // Check walls
            if (wallType == WallType.Solid)
            {
                if (Position.X < 0 || Position.X >= terrain.Width) return true;
            }

            // Check players?
            // If we hit a player, we should explode.
            // This check is done in GameManager usually.

            return false;
        }

        // We need a way to explode if we hit a player. 
        // GameManager checks player collision. If it returns true, OnHit is called.
        // So that works.
    }
}
