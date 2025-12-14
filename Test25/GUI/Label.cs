using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Test25.GUI
{
    public class Label : GuiElement
    {
        public string Text { get; set; }
        public SpriteFont Font { get; set; }
        public Color TextColor { get; set; } = Constants.UiLabelColor;
        public float Scale { get; set; } = 1.0f;

        public Label(string text, SpriteFont font, Vector2 position)
        {
            Text = text;
            Font = font;
            Bounds = new Rectangle((int)position.X, (int)position.Y, 0, 0); // Bounds size flexible for label
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible || string.IsNullOrEmpty(Text) || Font == null) return;
            spriteBatch.DrawString(Font, Text, new Vector2(Bounds.X, Bounds.Y), TextColor, 0f, Vector2.Zero, Scale,
                SpriteEffects.None, 0f);
        }
    }
}
