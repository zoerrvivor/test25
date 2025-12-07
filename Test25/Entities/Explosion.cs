using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Test25.Entities
{
    public class Explosion
    {
        public Vector2 Position { get; private set; }
        public float MaxRadius { get; private set; }
        public float CurrentRadius { get; private set; }
        public float Duration { get; private set; }
        public float TimeAlive { get; private set; }
        public bool IsActive { get; private set; }

        private Texture2D _texture;
        private Color _baseColor;
        private Color _currentColor;


        public Explosion(Texture2D texture, Vector2 position, float maxRadius, float duration = 0.5f,
            Color? color = null)
        {
            _texture = texture;
            Position = position;
            MaxRadius = maxRadius;
            Duration = duration;
            TimeAlive = 0f;
            IsActive = true;
            CurrentRadius = 0f;
            _baseColor = color ?? Color.OrangeRed; // Default fire color
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TimeAlive += dt;

            if (TimeAlive >= Duration)
            {
                IsActive = false;
                return;
            }

            // Expand
            float progress = TimeAlive / Duration;
            CurrentRadius = MathHelper.Lerp(0, MaxRadius, progress);

            // Flicker effect
            float flicker = (float)Utilities.Rng.Instance.NextDouble() * 0.5f + 0.5f; // 0.5 to 1.0

            // Fade out near end
            float alpha = 1.0f;
            if (progress > 0.7f)
            {
                alpha = 1.0f - ((progress - 0.7f) / 0.3f);
            }

            _currentColor = _baseColor * flicker * alpha;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive) return;

            float scale = (CurrentRadius * 2) / _texture.Width;
            Vector2 origin = new Vector2(_texture.Width / 2, _texture.Height / 2);

            spriteBatch.Draw(_texture, Position, null, _currentColor, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
