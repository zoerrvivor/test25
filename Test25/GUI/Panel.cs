using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Utilities;

namespace Test25.GUI
{
    public class Panel : GuiElement
    {
        public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 150); // Semi-transparent black
        private Texture2D _texture;

        public Panel(GraphicsDevice graphicsDevice, Rectangle bounds)
        {
            Bounds = bounds;
            _texture = TextureGenerator.CreateSolidColorTexture(graphicsDevice, 1, 1, Color.White);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            spriteBatch.Draw(_texture, Bounds, BackgroundColor);
        }
    }
}
