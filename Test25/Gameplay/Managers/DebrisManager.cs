using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Gameplay.Entities;
using Test25.Gameplay.World;
using Test25.Utilities;

namespace Test25.Gameplay.Managers
{
    public class DebrisManager
    {
        private List<TankDebris> _debrisList;
        private SmokeManager _smokeManager;

        public DebrisManager(SmokeManager smokeManager)
        {
            _debrisList = new List<TankDebris>();
            _smokeManager = smokeManager;
        }

        public void SpawnTankDebris(Vector2 position, Texture2D bodyDisplay, Texture2D barrelDisplay)
        {
            // 1. Barrel
            AddDebris(position, barrelDisplay);

            // 2. Body
            AddDebris(position, bodyDisplay);
        }

        private void AddDebris(Vector2 position, Texture2D texture)
        {
            // Random ballistic velocity
            float angle = Rng.Range(-MathHelper.Pi, 0); // Upwards arc
            float speed = Rng.Range(100f, 400f);
            Vector2 velocity = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * speed;

            var debris = new TankDebris(position, velocity, texture);
            _debrisList.Add(debris);
        }

        public void Update(GameTime gameTime, Terrain terrain, float wind)
        {
            for (int i = _debrisList.Count - 1; i >= 0; i--)
            {
                var d = _debrisList[i];
                d.Update(gameTime, terrain, wind);

                // Smoking effect (only if within smoke duration)
                if (d.Lifetime < d.SmokeDuration)
                {
                    if (Rng.Instance.NextDouble() < 0.1f)
                    {
                        if (Rng.Instance.NextDouble() < 0.2f)
                        {
                            _smokeManager.EmitSmoke(d.Position);
                        }
                    }
                }

                // Debris never expires now
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var d in _debrisList)
            {
                d.Draw(spriteBatch);
            }
        }

        public void Clear()
        {
            _debrisList.Clear();
        }
    }
}
