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
        public static Texture2D CreateSolidColorTexture(GraphicsDevice gd, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(gd, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateCircleTexture(GraphicsDevice gd, int radius, Color color)
        {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(gd, diameter, diameter);
            Color[] data = new Color[diameter * diameter];
            float radiusSq = radius * radius;

            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 center = new Vector2(radius, radius);
                    Vector2 pos = new Vector2(x, y);
                    if (Vector2.DistanceSquared(center, pos) <= radiusSq)
                    {
                        data[y * diameter + x] = color;
                    }
                    else
                    {
                        data[y * diameter + x] = Color.Transparent;
                    }
                }
            }
            texture.SetData(data);
            return texture;
        }
    }
}