using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Utilities;

namespace Test25.UI.Controls
{
    public class Checkbox : GuiElement
    {
        public bool IsChecked { get; set; }
        public string Text { get; set; }
        public SpriteFont Font { get; set; }
        public Color BoxColor { get; set; } = Color.White;
        public Color CheckColor { get; set; } = Color.Black;

        private Texture2D _texture => GuiResources.WhiteTexture;

        public Checkbox(GraphicsDevice graphicsDevice, Rectangle bounds, string text, SpriteFont font)
        {
            Bounds = bounds;
            Text = text;
            Font = font;

            OnClick += (element) => { IsChecked = !IsChecked; };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            // Draw Box
            Rectangle boxRect = new Rectangle(Bounds.X, Bounds.Y, Bounds.Height, Bounds.Height); // Square box
            spriteBatch.Draw(_texture, boxRect, BoxColor);

            // Draw Checkmark
            if (IsChecked)
            {
                Rectangle checkRect =
                    new Rectangle(boxRect.X + 4, boxRect.Y + 4, boxRect.Width - 8, boxRect.Height - 8);
                spriteBatch.Draw(_texture, checkRect, CheckColor);
            }

            // Draw Label
            if (!string.IsNullOrEmpty(Text) && Font != null)
            {
                Vector2 textPos = new Vector2(boxRect.Right + 10,
                    Bounds.Y + (Bounds.Height - Font.MeasureString(Text).Y) / 2);
                spriteBatch.DrawString(Font, Text, textPos, Color.White);
            }
        }
    }
}
