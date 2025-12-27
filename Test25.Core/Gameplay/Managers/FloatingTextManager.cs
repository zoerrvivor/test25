using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Core.Gameplay.Entities;

namespace Test25.Core.Gameplay.Managers
{
    public class FloatingTextManager
    {
        private readonly List<FloatingText> _activeTexts = new();

        public void AddText(Vector2 position, string text, Color color)
        {
            // Random slight horizontal velocity for variety
            Vector2 velocity = new Vector2(Utilities.Rng.Range(-20f, 20f), -Constants.DamageNumberSpeed);
            _activeTexts.Add(new FloatingText(position, text, color, velocity));
        }

        public void Update(GameTime gameTime)
        {
            for (int i = _activeTexts.Count - 1; i >= 0; i--)
            {
                _activeTexts[i].Update(gameTime);
                if (_activeTexts[i].IsExpired)
                {
                    _activeTexts.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            foreach (var text in _activeTexts)
            {
                // Draw with shadow for better readability
                spriteBatch.DrawString(font, text.Text, text.Position + new Vector2(1, 1), Color.Black * text.Alpha);
                spriteBatch.DrawString(font, text.Text, text.Position, text.Color * text.Alpha);
            }
        }

        public void Reset()
        {
            _activeTexts.Clear();
        }
    }
}
