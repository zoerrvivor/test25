using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Utilities;

namespace Test25.UI.Controls
{
    public class Button : GuiElement
    {
        public string Text { get; set; }
        public SpriteFont Font { get; set; }
        public Color BackgroundColor { get; set; } = Constants.UiButtonColor;
        public Color HoverColor { get; set; } = Constants.UiButtonHoverColor;
        public Color TextColor { get; set; } = Constants.UiButtonTextColor;

        // Shared texture
        private Texture2D _texture => GuiResources.WhiteTexture;

        public Button(GraphicsDevice graphicsDevice, Rectangle bounds, string text, SpriteFont font)
        {
            Bounds = bounds;
            Text = text;
            Font = font;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            Color colorToDraw = IsHovered ? HoverColor : BackgroundColor;
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
