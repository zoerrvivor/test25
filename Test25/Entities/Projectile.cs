using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System;

namespace Test25.Entities
{
    public class Projectile : GameObject
    {
        public float ExplosionRadius { get; set; } = 20f;
        public float Damage { get; set; } = 20f;
        public bool IsDead { get; set; }

        public Texture2D Texture;

        public Projectile(Vector2 position, Vector2 velocity, Texture2D texture)
        {
            Position = position;
            Velocity = velocity;
            Texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            // Physics is handled by UpdatePhysics
        }

        public virtual void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 v = Velocity;
            v.X += wind * deltaTime;
            v.Y += gravity * deltaTime;
            Velocity = v;

            Position += Velocity * deltaTime;

            if (Velocity.LengthSquared() > 0.1f)
            {
                Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
            }
        }

        public virtual bool CheckCollision(Terrain terrain, WallType wallType)
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

        public virtual void OnHit(Managers.GameManager gameManager)
        {
            // Default behavior: Explode
            gameManager.Terrain.Destruct((int)Position.X, (int)Position.Y, (int)ExplosionRadius);
            gameManager.AddExplosion(Position, ExplosionRadius);

            foreach (var player in gameManager.Players)
            {
                if (!player.IsActive) continue;
                float dist = Vector2.Distance(player.Position, Position);
                if (dist < ExplosionRadius + 20) // Simple radius check
                {
                    // Calculate damage based on distance? For now just full damage
                    if (player.TakeDamage(Damage))
                    {
                        gameManager.HandleTankDeath(player);
                    }
                }
            }

            IsDead = true;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, Rotation,
                    new Vector2(Texture.Width / 2, Texture.Height / 2), 1f, SpriteEffects.None, 0f);
            }
        }
    }
}