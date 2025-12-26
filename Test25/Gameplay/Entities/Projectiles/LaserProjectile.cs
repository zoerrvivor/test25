using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Gameplay.World;
using Test25.Gameplay.Managers;
using System;
using System.Collections.Generic;
using Test25.Utilities;

namespace Test25.Gameplay.Entities.Projectiles
{
    public class LaserProjectile : Projectile
    {
        private Vector2 _endPosition;
        private float _lifeTime;
        private const float MaxLifeTime = 2.0f; // Visual duration extended for sound effect
        private bool _hasFired;

        // Sound control
        private float _soundTimer;
        private float _soundInterval = 0.4f; // Start slow
        private float _currentPitch = 0.0f; // Start normal pitch

        public LaserProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
            ExplosionRadius = 10f; // Beam thickness/tunnel radius
            Damage = 20f; // Increased damage
            _lifeTime = MaxLifeTime;

            // Play initial sound
            Test25.Services.SoundManager.PlaySound("laser", _currentPitch);
        }

        public override void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            // Decrease lifetime
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lifeTime -= deltaTime;

            // Sound Logic
            _soundTimer += deltaTime;
            if (_soundTimer >= _soundInterval)
            {
                _soundTimer = 0f;
                // Play sound
                Test25.Services.SoundManager.PlaySound("laser", _currentPitch);

                // Accelerate: decrease interval, increase pitch
                _soundInterval = Math.Max(0.05f, _soundInterval * 0.7f); // Speed up
                _currentPitch = Math.Min(1.0f, _currentPitch + 0.15f); // Pitch up
            }

            if (_lifeTime <= 0)
            {
                IsDead = true;
                return;
            }

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
                if (currentPos.X >= 0 && currentPos.X < terrain.Width && currentPos.Y >= 0 &&
                    currentPos.Y < terrain.Height)
                {
                    terrain.Destruct((int)currentPos.X, (int)currentPos.Y, (int)ExplosionRadius);
                }

                // 2. World Bounds Check
                if (currentPos.X < 0 || currentPos.X >= terrain.Width ||
                    currentPos.Y > terrain.Height + 500) // Lower bound margin
                {
                    // We can stop here or continue off-screen? 
                    // Let's stop slightly off-screen for visual cleanliness
                    break;
                }
            }

            _endPosition = currentPos;
            return true;
        }

        public override void OnHit(GameManager gameManager)
        {
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
                        if (player.TakeDamage(Damage))
                        {
                            gameManager.HandleTankDeath(player, Owner);
                        }

                        hitPlayers.Add(player);
                        gameManager.AddExplosion(currentPos, 15, Color.Red);
                    }
                }

                // World Bounds
                if (currentPos.X < 0 || currentPos.X >= gameManager.Terrain.Width ||
                    currentPos.Y > gameManager.Terrain.Height + 500)
                {
                    break;
                }
            }

            _endPosition = currentPos;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (Texture != null && _endPosition != Vector2.Zero)
            {
                Vector2 edge = _endPosition - Position;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                float length = edge.Length();

                // Flicker
                Color c = Rng.Instance.NextDouble() > 0.5 ? Color.Red : Color.White;

                // Scale: X = length / width, Y = thickness / height
                float thickness = ExplosionRadius * 2f;
                Vector2 scale = new Vector2(length / Texture.Width, thickness / Texture.Height);

                spriteBatch.Draw(Texture, Position, null, c, angle, new Vector2(0, Texture.Height / 2f), scale,
                    SpriteEffects.None, 0f);
            }
        }
    }
}
