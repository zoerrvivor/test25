using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Utilities;

namespace Test25.GUI
{
    public class Panel : GuiElement
    {
        public Color BackgroundColor { get; set; } = Constants.UiPanelColor;
        private Texture2D _texture => GuiResources.WhiteTexture;

        public Panel(GraphicsDevice graphicsDevice, Rectangle bounds)
        {
            Bounds = bounds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            spriteBatch.Draw(_texture, Bounds, BackgroundColor);
        }
    }
}
