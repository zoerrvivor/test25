using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Test25.Entities
{
    public class Decoration
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public float Scale { get; set; } = 1f;
        public Rectangle SourceRectangle { get; set; }
        public Color Color { get; set; } = Color.White;

        public Decoration(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
            SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw centered horizontally, bottom anchored to Position.Y
            // Actually, for "embedded", Position.Y will be the terrain height + offset
            // We'll just draw top-left relative to Position and handle alignment when setting Position.
            spriteBatch.Draw(Texture, Position, SourceRectangle, Color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }
    }
}
