using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Test25.World
{
    public class Terrain
    {
        private int[] _heightMap;
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

            _waterTexture = new Texture2D(graphicsDevice, 1, 1);
            _waterTexture.SetData(new[] { new Color(0, 0, 255, 128) }); // Semi-transparent blue
        }

        // Generate terrain using Midpoint Displacement (Fractal)
        public void Generate(int seed)
        {
            Random rand = new Random(seed);

            // Use a power of 2 size for the algorithm that covers the screen width
            // 2^10 = 1024, which is > 800
            int size = 1025;
            float[] map = new float[size];

            // Initial endpoints
            map[0] = Height / 2 + rand.Next(-100, 100);
            map[size - 1] = Height / 2 + rand.Next(-100, 100);

            float displacement = 300f; // Initial displacement magnitude
            float roughness = 0.55f; // Controls how quickly displacement reduces (0.5 is standard smooth, higher is rougher)

            Divide(map, 0, size - 1, displacement, roughness, rand);

            // Map the fractal array to our screen width
            for (int x = 0; x < Width; x++)
            {
                // Scale x to the map size
                int index = (int)((float)x / Width * (size - 1));
                _heightMap[x] = (int)MathHelper.Clamp(map[index], 50, Height - 50); // Keep some padding
            }

            CreateTexture();
        }

        private void Divide(float[] map, int left, int right, float displacement, float roughness, Random rand)
        {
            if (right - left <= 1) return;

            int mid = (left + right) / 2;
            float average = (map[left] + map[right]) / 2;

            // Random offset
            float change = (float)(rand.NextDouble() * 2 - 1) * displacement;

            map[mid] = average + change;

            // Reduce displacement
            float newDisplacement = displacement * roughness;

            Divide(map, left, mid, newDisplacement, roughness, rand);
            Divide(map, mid, right, newDisplacement, roughness, rand);
        }

        private void CreateTexture()
        {
            // Create a texture from the heightmap
            Color[] data = new Color[Width * Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (y >= _heightMap[x])
                    {
                        data[y * Width + x] = Color.SaddleBrown; // Ground
                    }
                    else
                    {
                        data[y * Width + x] = Color.Transparent; // Sky
                    }
                }
            }

            _texture = new Texture2D(_graphicsDevice, Width, Height);
            _texture.SetData(data);
        }

        public int GetHeight(int x)
        {
            if (x < 0 || x >= Width) return Height; // Out of bounds
            return _heightMap[x];
        }

        public void Destruct(int x, int y, int radius)
        {
            // Carve out a circle from the terrain
            for (int i = x - radius; i <= x + radius; i++)
            {
                if (i >= 0 && i < Width)
                {
                    // Calculate the vertical distance from the center of the explosion at this x
                    int dx = i - x;
                    // Circle equation: x^2 + y^2 = r^2  =>  y = sqrt(r^2 - x^2)
                    // We want to carve out everything *above* the bottom of the circle at this x.
                    // The bottom of the circle at x+dx is y + sqrt(r^2 - dx^2).

                    int dy = (int)Math.Sqrt(radius * radius - dx * dx);
                    int circleBottom = y + dy;

                    // If the current ground height is higher (smaller Y value) than the bottom of the circle,
                    // push it down to the bottom of the circle.
                    if (_heightMap[i] < circleBottom)
                    {
                        _heightMap[i] = circleBottom;
                    }

                    if (_heightMap[i] > Height) _heightMap[i] = Height;
                }
            }
            // Update texture for the whole terrain (simple approach)
            CreateTexture();
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
            float waveOffset = (float)Math.Sin(_waveTime * 2f) * 5f; // Simple wave
            int waterY = waterBaseHeight + (int)waveOffset;

            Rectangle waterRect = new Rectangle(0, waterY, Width, Height - waterY);
            spriteBatch.Draw(_waterTexture, waterRect, Color.White);
        }
    }
}
