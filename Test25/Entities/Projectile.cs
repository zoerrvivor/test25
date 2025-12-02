// Version: 0.3 (Optimized)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System;

namespace Test25.Entities
{
    public class Projectile : GameObject
    {
        // KORREKTUR: Keine Verdeckung (new) von Velocity.

        public float ExplosionRadius { get; set; } = 20f;
        public float Damage { get; set; } = 20f;

        private Texture2D _texture;

        public Projectile(Vector2 position, Vector2 velocity, Texture2D texture)
        {
            Position = position;
            Velocity = velocity;
            _texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            // Physics wird zentral vom GameManager über UpdatePhysics gesteuert
        }

        public void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 v = Velocity;
            v.X += wind * deltaTime;
            v.Y += gravity * deltaTime;
            Velocity = v;

            Position += Velocity * deltaTime;

            // PERFORMANCE: Rotation nur berechnen, wenn sich Objekt signifikant bewegt.
            // Spart teure Math.Atan2 Calls für fast stillstehende Objekte (obwohl Projektile selten stillstehen).
            if (Velocity.LengthSquared() > 0.1f)
            {
                Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
            }
        }

        public bool CheckCollision(Terrain terrain, WallType wallType)
        {
            if (Position.Y > terrain.Height) return true;

            if (wallType == WallType.Solid)
            {
                if (Position.X < 0 || Position.X >= terrain.Width) return true;
            }

            if (Position.X >= 0 && Position.X < terrain.Width)
            {
                if (Position.Y >= terrain.GetHeight((int)Position.X))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, Position, null, Color.White, Rotation, new Vector2(_texture.Width / 2, _texture.Height / 2), 1f, SpriteEffects.None, 0f);
            }
        }
    }
}