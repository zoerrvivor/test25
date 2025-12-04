using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Test25.Utilities
{
    public static class TextureGenerator
    {
        public static Texture2D CreateNoiseTexture(GraphicsDevice gd, int width, int height, Color c1, Color c2)
        {
            Texture2D texture = new Texture2D(gd, width, height);
            Color[] data = new Color[width * height];
            Random rand = new Random();

            for (int i = 0; i < data.Length; i++)
            {
                // Simple noise
                float noise = (float)rand.NextDouble();
                data[i] = Color.Lerp(c1, c2, noise);
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateGradientTexture(GraphicsDevice gd, int width, int height, Color top, Color bottom)
        {
            Texture2D texture = new Texture2D(gd, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                Color c = Color.Lerp(top, bottom, y / (float)height);
                for (int x = 0; x < width; x++)
                {
                    data[y * width + x] = c;
                }
            }
            texture.SetData(data);
            return texture;
        }
    }
}