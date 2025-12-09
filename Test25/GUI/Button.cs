using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Utilities;

namespace Test25.GUI
{
    public class Button : GuiElement
    {
        public string Text { get; set; }
        public SpriteFont Font { get; set; }
        public Color BackgroundColor { get; set; } = Color.Gray;
        public Color HoverColor { get; set; } = Color.LightGray;
        public Color TextColor { get; set; } = Color.White;

        private Texture2D _texture;

        public Button(GraphicsDevice graphicsDevice, Rectangle bounds, string text, SpriteFont font)
        {
            Bounds = bounds;
            Text = text;
            Font = font;
            _texture = TextureGenerator.CreateSolidColorTexture(graphicsDevice, 1, 1, Color.White);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            Color colorToDraw = _isHovered ? HoverColor : BackgroundColor;
            spriteBatch.Draw(_texture, Bounds, colorToDraw);

            if (!string.IsNullOrEmpty(Text) && Font != null)
            {
                Vector2 textSize = Font.MeasureString(Text);
                Vector2 textPos = new Vector2(
                    Bounds.X + (Bounds.Width - textSize.X) / 2,
                    Bounds.Y + (Bounds.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(Font, Text, textPos, TextColor);
            }
        }
    }
}
