// Version: 0.1

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Test25.Gameplay.Entities
{
    public abstract class GameObject
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public bool IsActive { get; set; } = true;

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch, SpriteFont font);
    }
}
