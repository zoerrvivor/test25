using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Core.Gameplay.Managers;
using Test25.Core.Gameplay.World;
using Test25.Core.Services;

namespace Test25.Core.Gameplay.Entities.Projectiles
{
    public class Projectile : GameObject
    {
        public float ExplosionRadius { get; set; } = 20f;
        public float Damage { get; set; } = 20f;
        public Tank Owner { get; set; }
        public bool IsDead { get; set; }
        public bool HasTrail { get; set; } = true;

        public Texture2D Texture;

        private List<Vector2> _trail = new List<Vector2>();
        private float _trailTimer = 0f;

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

            // Update Trail
            if (HasTrail)
            {
                _trailTimer += deltaTime;
                if (_trailTimer >= Constants.ProjectileTrailFrequency)
                {
                    _trailTimer = 0f;
                    _trail.Insert(0, Position);
                    if (_trail.Count > Constants.ProjectileTrailLength)
                    {
                        _trail.RemoveAt(_trail.Count - 1);
                    }
                }
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

        public virtual void OnHit(GameManager gameManager)
        {
            // Default behavior: Explode
            SoundManager.PlaySound("explosion");
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
                        gameManager.HandleTankDeath(player, Owner);
                    }
                }
            }

            IsDead = true;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw Trail
            if (HasTrail && _trail.Count > 0 && Texture != null)
            {
                for (int i = 0; i < _trail.Count; i++)
                {
                    float factor = 1.0f - ((float)i / Constants.ProjectileTrailLength);
                    float alpha = factor * 0.5f; // Max alpha 0.5 for trail
                    float scale = factor * 0.8f; // Fades and shrinks

                    spriteBatch.Draw(Texture, _trail[i], null, Color.White * alpha, 0f,
                        new Vector2(Texture.Width / 2f, Texture.Height / 2f), scale, SpriteEffects.None, 0f);
                }
            }

            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, Rotation,
                    new Vector2(Texture.Width / 2f, Texture.Height / 2f), 1f, SpriteEffects.None, 0f);
            }
        }
    }
}