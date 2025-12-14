using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Entities;
using Test25.Utilities;

namespace Test25.Managers
{
    public class ExplosionManager
    {
        private List<Explosion> _explosions;
        private Texture2D _explosionTexture;

        public bool HasActiveExplosions => _explosions.Count > 0;

        public ExplosionManager(GraphicsDevice graphicsDevice)
        {
            _explosions = new List<Explosion>();
            // Create a simple white circle for explosions that can be tinted
            // Assuming TextureGenerator is static and accessible
            _explosionTexture = TextureGenerator.CreateCircleTexture(graphicsDevice, 100, Color.White);
        }

        public void Reset()
        {
            _explosions.Clear();
        }

        public void AddExplosion(Vector2 position, float radius, Color? color = null)
        {
            _explosions.Add(new Explosion(_explosionTexture, position, radius, 0.5f, color));
        }

        public void Update(GameTime gameTime)
        {
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                _explosions[i].Update(gameTime);
                if (!_explosions[i].IsActive)
                {
                    _explosions.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var ex in _explosions)
            {
                ex.Draw(spriteBatch);
            }
        }
    }
}
