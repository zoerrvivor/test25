using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Entities;

namespace Test25.Managers
{
    public class ProjectileManager
    {
        public List<Projectile> Projectiles { get; private set; }
        public bool IsProjectileInAir => Projectiles.Count > 0;

        public ProjectileManager()
        {
            Projectiles = [];
        }

        public void Reset()
        {
            Projectiles.Clear();
        }

        public void AddProjectile(Projectile projectile)
        {
            Projectiles.Add(projectile);
        }

        // We pass GameManager here because Projectile.OnHit(GameManager) requires it.
        // In a future refactor, we should decouple Projectile.OnHit from GameManager.
        public void Update(GameTime gameTime, GameManager gameManager)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                p.UpdatePhysics(gameTime, gameManager.Wind, Constants.Gravity);

                // Handle MIRV splitting logic
                // This logic is specific to MirvProjectile, checking its internal state
                if (p is MirvProjectile mirv)
                {
                    if (mirv.NewProjectiles.Count > 0)
                    {
                        Projectiles.AddRange(mirv.NewProjectiles);
                        mirv.NewProjectiles.Clear();
                    }
                }

                if (p.IsDead)
                {
                    Projectiles.RemoveAt(i);
                    continue;
                }

                // Wall Bounds Logic
                var wallType = gameManager.Settings.WallType;
                var terrain = gameManager.Terrain;

                if (wallType == WallType.Wrap)
                {
                    if (p.Position.X < 0) p.Position = new Vector2(terrain.Width - 1, p.Position.Y);
                    else if (p.Position.X >= terrain.Width) p.Position = new Vector2(0, p.Position.Y);
                }
                else if (wallType == WallType.Rubber)
                {
                    if (p.Position.X < 0)
                    {
                        p.Position = new Vector2(0, p.Position.Y);
                        p.Velocity = new Vector2(-p.Velocity.X, p.Velocity.Y);
                    }
                    else if (p.Position.X >= terrain.Width)
                    {
                        p.Position = new Vector2(terrain.Width - 1, p.Position.Y);
                        p.Velocity = new Vector2(-p.Velocity.X, p.Velocity.Y);
                    }
                }

                bool hit = false;

                // 1. Check Player Collision
                foreach (var player in gameManager.Players)
                {
                    if (!player.IsActive) continue;

                    if (player.BoundingBox.Contains(p.Position))
                    {
                        hit = true;
                        break;
                    }
                }

                // 2. Check Terrain Collision
                if (!hit)
                {
                    if (p.Position.Y >= 0 && p.CheckCollision(terrain, wallType))
                    {
                        hit = true;
                    }
                }

                if (hit)
                {
                    p.OnHit(gameManager);
                    if (p.IsDead)
                    {
                        Projectiles.RemoveAt(i);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            foreach (var p in Projectiles)
            {
                p.Draw(spriteBatch, font);
            }
        }
    }
}
