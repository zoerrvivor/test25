using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Test25.Core.UI.Controls
{
    public class Panel : GuiElement
    {
        public Color BackgroundColor { get; set; } = Constants.UiPanelColor;
        public Color BorderColor { get; set; } = Color.Black;
        public int BorderThickness { get; set; } = 0;
        private Texture2D _texture => GuiResources.WhiteTexture;

        public Panel(GraphicsDevice graphicsDevice, Rectangle bounds)
        {
            Bounds = bounds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            spriteBatch.Draw(_texture, Bounds, BackgroundColor);
            
            if (BorderThickness > 0)
            {
                GuiResources.DrawHollowRect(spriteBatch, Bounds, BorderColor, BorderThickness);
            }
        }
    }
}
