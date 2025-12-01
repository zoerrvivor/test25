// Version: 0.1
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;

namespace Test25.Entities
{
    public class Projectile : GameObject
    {
        public new Vector2 Velocity { get; set; }
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
            // Default update required by base class
        }

        public void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Apply wind and gravity
            Velocity += new Vector2(wind, gravity) * deltaTime;
            Position += Velocity * deltaTime;

            // Rotation follows velocity
            Rotation = (float)System.Math.Atan2(Velocity.Y, Velocity.X);
        }

        public bool CheckCollision(Terrain terrain, WallType wallType)
        {
            if (Position.Y > terrain.Height) return true;

            if (wallType == WallType.Solid)
            {
                if (Position.X < 0 || Position.X >= terrain.Width) return true;
            }

            // Simple collision check
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