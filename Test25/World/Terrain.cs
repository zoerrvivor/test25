// Version: 0.1
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Test25.World
{
    public class Terrain
    {
        private int[] _heightMap;
        private Color[] _colorData; // Cached color array to avoid GC pressure
        private Texture2D _texture;
        private Texture2D _waterTexture;
        private float _waveTime;
        private GraphicsDevice _graphicsDevice;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int WaterLevel { get; private set; }

        public Terrain(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            Width = width;
            Height = height;
            WaterLevel = (int)(height * 0.8f);

            _heightMap = new int[width];
            _colorData = new Color[width * height]; // Pre-allocate memory once
            _texture = new Texture2D(_graphicsDevice, Width, Height); // Allocate texture once

            _waterTexture = new Texture2D(graphicsDevice, 1, 1);
            _waterTexture.SetData([new Color(0, 0, 255, 128)]);
        }

        public void Generate(int seed)
        {
            Random rand = new(seed);
            int size = 1025; // Power of 2 + 1
            float[] map = new float[size];

            map[0] = Height / 2 + rand.Next(-100, 100);
            map[size - 1] = Height / 2 + rand.Next(-100, 100);

            float displacement = 300f;
            float roughness = 0.55f;

            Divide(map, 0, size - 1, displacement, roughness, rand);

            for (int x = 0; x < Width; x++)
            {
                int index = (int)((float)x / Width * (size - 1));
                _heightMap[x] = (int)MathHelper.Clamp(map[index], 50, Height - 50);
            }

            UpdateTexture();
        }

        private static void Divide(float[] map, int left, int right, float displacement, float roughness, Random rand)
        {
            if (right - left <= 1) return;

            int mid = (left + right) / 2;
            float average = (map[left] + map[right]) / 2;
            float change = (float)(rand.NextDouble() * 2 - 1) * displacement;

            map[mid] = average + change;

            float newDisplacement = displacement * roughness;
            Divide(map, left, mid, newDisplacement, roughness, rand);
            Divide(map, mid, right, newDisplacement, roughness, rand);
        }

        private void UpdateTexture()
        {
            // Update the reused array instead of creating a new one
            for (int x = 0; x < Width; x++)
            {
                int groundY = _heightMap[x];
                for (int y = 0; y < Height; y++)
                {
                    int index = y * Width + x;
                    if (y >= groundY)
                    {
                        _colorData[index] = Color.SaddleBrown;
                    }
                    else
                    {
                        _colorData[index] = Color.Transparent;
                    }
                }
            }
            // Upload data to GPU
            _texture.SetData(_colorData);
        }

        public int GetHeight(int x)
        {
            if (x < 0 || x >= Width) return Height;
            return _heightMap[x];
        }

        public void Destruct(int x, int y, int radius)
        {
            bool changed = false;

            // Update Heightmap
            for (int i = x - radius; i <= x + radius; i++)
            {
                if (i >= 0 && i < Width)
                {
                    int dx = i - x;
                    int dy = (int)Math.Sqrt(radius * radius - dx * dx);
                    int circleBottom = y + dy;

                    if (_heightMap[i] < circleBottom)
                    {
                        _heightMap[i] = circleBottom;
                        if (_heightMap[i] > Height) _heightMap[i] = Height;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                // Optimized: Update only based on new heightmap logic
                UpdateTexture();
            }
        }

        public void Update(GameTime gameTime)
        {
            _waveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture != null)
                spriteBatch.Draw(_texture, Vector2.Zero, Color.White);
        }

        public void DrawWater(SpriteBatch spriteBatch)
        {
            int waterBaseHeight = WaterLevel;
            float waveOffset = (float)Math.Sin(_waveTime * 2f) * 5f;
            int waterY = waterBaseHeight + (int)waveOffset;

            Rectangle waterRect = new(0, waterY, Width, Height - waterY);
            spriteBatch.Draw(_waterTexture, waterRect, Color.White);
        }
    }
}