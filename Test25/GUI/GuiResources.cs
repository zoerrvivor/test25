using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Utilities;

namespace Test25.GUI
{
    public static class GuiResources
    {
        public static Texture2D WhiteTexture { get; private set; }

        public static void Init(GraphicsDevice graphicsDevice)
        {
            if (WhiteTexture == null || WhiteTexture.IsDisposed || WhiteTexture.GraphicsDevice != graphicsDevice)
            {
                WhiteTexture = TextureGenerator.CreateSolidColorTexture(graphicsDevice, 1, 1, Color.White);
            }
        }
    }
}
