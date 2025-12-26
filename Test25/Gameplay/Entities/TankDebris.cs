using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Test25.Gameplay.Managers;
using Test25.Gameplay.World;
using Test25.Utilities;

namespace Test25.Gameplay.Entities
{
    public class TankDebris
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float RotationSpeed;
        public Texture2D Texture;
        public bool IsStatic;
        public float Lifetime;
        public float SmokeDuration = 30f; // Smoke lasts for a while, debris persists forever

        private float _smokeTimer;
        private Vector2 _origin;

        public TankDebris(Vector2 position, Vector2 velocity, Texture2D texture)
        {
            Position = position;
            Velocity = velocity;
            Texture = texture;
            IsStatic = false;
            Lifetime = 0f;

            Rotation = (float)Rng.Instance.NextDouble() * MathHelper.TwoPi;
            RotationSpeed = Rng.Range(-5f, 5f);

            _origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        }

        public void Update(GameTime gameTime, Terrain terrain, float wind)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Lifetime += dt;

            // Emit smoke if static or flying (as "smoking junk")
            _smokeTimer += dt;
            if (_smokeTimer > 0.1f)
            {
                _smokeTimer = 0f;
                // Emit smoke logic will be handled by manager or we pass smoke manager here?
                // Ideally Manager handles it or we expose a way to add smoke.
                // For now, we will rely on DebrisManager to check this or pass SmokeManager.
                // Let's defer actual emission to DebrisManager to keep this class simple data-ish.
            }

            // Check ground support if static
            if (IsStatic)
            {
                int cx = (int)Position.X;
                int cy = (int)Position.Y + 1; // Check pixel below
                if (cx >= 0 && cx < terrain.Width && cy < terrain.Height)
                {
                    if (!terrain.IsPixelSolid(cx, cy))
                    {
                        IsStatic = false; // Ground gone, wake up!
                    }
                }

                if (IsStatic) return; // Still supported
            }

            // Physics
            Velocity.Y += Constants.Gravity * dt;
            Position += Velocity * dt;
            Rotation += RotationSpeed * dt;

            // Collision
            int x = (int)Position.X;
            int y = (int)Position.Y;

            if (x >= 0 && x < terrain.Width)
            {
                if (y >= terrain.Height)
                {
                    IsStatic = true; // Fell out of world (technically)
                    return;
                }

                // Check collision with terrain
                if (y > 0 && terrain.IsPixelSolid(x, y))
                {
                    // Hit ground
                    IsStatic = true;
                    Velocity = Vector2.Zero;
                    RotationSpeed = 0f;

                    // Embed slightly or sit on top? 
                    // Let's just stop.

                    // Correction: move up until not solid?
                    // Simple snap for now.
                }
            }
            else
            {
                // Out of bounds X
                if (y > terrain.Height + 100)
                {
                    IsStatic = true; // Stop processing if way off screen
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, _origin, 1f, SpriteEffects.None,
                0f); // Render white (burned look?) or original color? User said "smoking junk", maybe darken it?
        }
    }
}
