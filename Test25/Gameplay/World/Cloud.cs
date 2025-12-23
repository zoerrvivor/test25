// Version: 0.2

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Gameplay.Entities;

namespace Test25.Gameplay.World
{
    /// <summary>
    /// Represents a single cloud moving across the sky.
    /// </summary>
    public class Cloud : GameObject
    {
        private Texture2D _texture;
        public new Vector2 Position { get; private set; }
        public float Scale { get; internal set; }

        private float _speed;
        private int _screenWidth;
        private float _scale;

        public Cloud(Texture2D texture, Vector2 startPosition, float speed, float scale, int screenWidth)
        {
            _texture = texture;
            Position = startPosition;
            _speed = speed;
            _scale = scale;
            _screenWidth = screenWidth;
        }

        public override void Update(GameTime gameTime)
        {
            Update(gameTime, 0);
        }

        public void Update(GameTime gameTime, float wind)
        {
            Vector2 pos = Position;
            // Wind affects cloud movement. Multiplier 2f makes it more noticeable.
            float movement = (_speed + wind * 2f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            pos.X += movement;

            // Wrap around
            if (pos.X > _screenWidth)
                pos.X = -_texture.Width * _scale;
            else if (pos.X < -_texture.Width * _scale)
                pos.X = _screenWidth;

            Position = pos;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            Draw(spriteBatch);
        }
    }
}
