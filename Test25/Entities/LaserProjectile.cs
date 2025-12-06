using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System;
using System.Collections.Generic;

namespace Test25.Entities
{
    public class LaserProjectile : Projectile
    {
        private Random _random;
        private Vector2 _endPosition;
        private float _lifeTime;
        private const float MaxLifeTime = 0.5f; // Visual duration
        private bool _hasFired = false;

        public LaserProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
            _random = new Random();
            ExplosionRadius = 10f; // Beam thickness/tunnel radius
            Damage = 5f;
            _lifeTime = MaxLifeTime;
        }

        public override void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            // Decrease lifetime
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lifeTime -= deltaTime;

            if (_lifeTime <= 0)
            {
                IsDead = true;
                return;
            }

            // Physics/Raycast only runs once
            if (_hasFired) return;
        }

        // We override OnHit to do nothing because we handle everything in the initial raycast
        // actually, we don't want the default projectile behavior anymore.
        // We will perform the raycast in CheckCollision because that's where we get the Terrain reference.

        public override bool CheckCollision(Terrain terrain, WallType wallType)
        {
            if (_hasFired) return false; // Already processed
            _hasFired = true;

            // Calculate Beam Trajectory
            Vector2 direction = Velocity;
            direction.Normalize();

            // Raycast
            Vector2 currentPos = Position;
            Vector2 step = direction * 4f; // 4px step for checks
            int maxSteps = 2000; // Max distance

            for (int i = 0; i < maxSteps; i++)
            {
                currentPos += step;

                // 1. Terrain Destruction
                if (currentPos.X >= 0 && currentPos.X < terrain.Width && currentPos.Y >= 0 && currentPos.Y < terrain.Height)
                {
                    terrain.Destruct((int)currentPos.X, (int)currentPos.Y, (int)ExplosionRadius);
                }

                // 2. World Bounds Check
                if (currentPos.X < 0 || currentPos.X >= terrain.Width || currentPos.Y > terrain.Height + 500) // Lower bound margin
                {
                    // We can stop here or continue off-screen? 
                    // Let's stop slightly off-screen for visual cleanliness
                    break;
                }
            }

            _endPosition = currentPos;

            // 3. Entity Damage (We need access to GameManager players... which we don't have here easily)
            // But CheckCollision calls OnHit if true. We check directly?
            // Projectile doesn't have reference to Players list.
            // But we can defer player damage to OnHit if we pass through checks.
            // Problem: OnHit is called on "collision". We don't "collide" with terrain.
            // We need to hit players.
            // Accessing players is tricky without passing them.
            // However, we can use the `OnHit(GameManager)` override I added!
            // But CheckCollision needs to return TRUE to trigger OnHit.
            // If we return true, the projectile is marked dead usually.
            // But we want it to stay alive for visuals.
            // So we must handle damage HERE or ensure OnHit doesn't kill it?
            // Actually, OnHit is called -> IsDead=true.

            // Let's rely on `GameManager` logic.
            // `GameManager.Update` does: `if (p.CheckCollision(Terrain, ...)) { p.OnHit(this); if(p.IsDead) Remove; }`
            // If I return `true`, `OnHit` is called.
            // Inside `OnHit(GameManager)`, I can access players and perform the Raycast damage there!
            // But `CheckCollision` is where I have `Terrain`.
            // Use `OnHit(GameManager)` for everything?
            // `GameManager` calls `OnHit` ONLY if `CheckCollision` returns true.
            // So I should return `true` on the first frame to trigger logic, BUT
            // `OnHit` usually kills the projectile.
            // I need to override `OnHit` to NOT kill it, just do logic.
            // And then `GameManager` removes it if `IsDead`.
            // So I must NOT set `IsDead = true` in `OnHit`.

            // Re-plan:
            // 1. CheckCollision returns TRUE on first frame.
            // 2. OnHit(GameManager) is called.
            // 3. Inside OnHit:
            //    - Perform Raycast using `gameManager.Terrain`.
            //    - Destroy terrain.
            //    - Check collision with `gameManager.Players`.
            //    - Set `_endPosition`.
            //    - Do NOT set IsDead.
            // 4. UpdatePhysics handles lifetime and sets IsDead later.
            // 5. CheckCollision needs to return FALSE on subsequent frames.

            return true;
        }

        public override void OnHit(Managers.GameManager gameManager)
        {
            // Only execute logic once
            // Actually CheckCollision ensures we only return true once?
            // Wait, if I return true, OnHit is called.
            // If I don't set IsDead, next frame Update is called.
            // Next frame CheckCollision is called.
            // I need a flag to prevent re-triggering.
            // Use `_hasFired` state (which I set in CheckCollision).

            // Raycast Logic
            Vector2 direction = Velocity;
            direction.Normalize();

            Vector2 currentPos = Position;
            Vector2 step = direction * 4f;
            int maxSteps = 2000;

            // Keep track of visited players to avoid multi-hitting?
            HashSet<Tank> hitPlayers = new HashSet<Tank>();

            for (int i = 0; i < maxSteps; i++)
            {
                currentPos += step;

                // Terrain Destruction
                if (currentPos.X >= 0 && currentPos.X < gameManager.Terrain.Width &&
                    currentPos.Y >= 0 && currentPos.Y < gameManager.Terrain.Height)
                {
                    gameManager.Terrain.Destruct((int)currentPos.X, (int)currentPos.Y, (int)ExplosionRadius);
                }

                // Player Damage
                foreach (var player in gameManager.Players)
                {
                    if (!player.IsActive || hitPlayers.Contains(player)) continue;
                    if (player.BoundingBox.Contains(currentPos))
                    {
                        player.TakeDamage(Damage);
                        hitPlayers.Add(player);
                        // Visual hit effect?
                        gameManager.AddExplosion(currentPos, 15, Color.Red);
                    }
                }

                // World Bounds
                if (currentPos.X < 0 || currentPos.X >= gameManager.Terrain.Width || currentPos.Y > gameManager.Terrain.Height + 500)
                {
                    break;
                }
            }
            _endPosition = currentPos;

            // Do NOT set IsDead = true; 
            // We want to persist for visuals.
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw Ray using a 1x1 pixel Texture?
            // Since we use `_texture` which is probably the projectile ball...
            // We can stretch it?
            // Or better, generate a simple 1x1 white texture if possible, or use the injected texture if it's white.
            // Assuming `_texture` is the projectile texture (white circle?).
            // We'll draw a stretched sprite from Position to _endPosition.

            if (_texture != null && _endPosition != Vector2.Zero)
            {
                Vector2 edge = _endPosition - Position;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                float length = edge.Length();

                // Flicker
                Color c = _random.NextDouble() > 0.5 ? Color.Red : Color.White;

                // Scale: X = length / width, Y = thickness / height
                // Assume texture is ~10-20px
                float thickness = ExplosionRadius * 2f;
                Vector2 scale = new Vector2(length / _texture.Width, thickness / _texture.Height);

                spriteBatch.Draw(_texture, Position, null, c, angle, new Vector2(0, _texture.Height / 2), scale, SpriteEffects.None, 0f);
            }
        }
    }
}
