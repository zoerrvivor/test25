using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Core.Utilities;

namespace Test25.Core.UI.Controls
{
    public static class GuiResources
    {
        public static Texture2D WhiteTexture { get; private set; }

        public static void Init(GraphicsDevice graphicsDevice)
        {
            if (WhiteTexture == null || WhiteTexture.IsDisposed || WhiteTexture.GraphicsDevice != graphicsDevice)
            {
                WhiteTexture?.Dispose();
                WhiteTexture = TextureGenerator.CreateSolidColorTexture(graphicsDevice, 1, 1, Color.White);
            }
        }


        public static void DrawHollowRect(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
        {
            if (WhiteTexture == null) return;

            // Top
            spriteBatch.Draw(WhiteTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(WhiteTexture,
                new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
            // Left
            spriteBatch.Draw(WhiteTexture, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Right
            spriteBatch.Draw(WhiteTexture,
                new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
        }
    }
}
