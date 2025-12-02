// Version: 0.4 (Optimized)
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

        // Caching colors to avoid reconstructing the whole array, 
        // though we now mostly use partial updates.
        private Color[] _colorData;

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
            _colorData = new Color[width * height];
            _texture = new Texture2D(_graphicsDevice, Width, Height);

            _waterTexture = new Texture2D(graphicsDevice, 1, 1);
            _waterTexture.SetData(new[] { new Color(0, 0, 255, 128) });
        }

        public void Generate(int seed)
        {
            Random rand = new(seed);
            int size = 1025;
            float[] map = new float[size];

            map[0] = Height / 2 + rand.Next(-100, 100);
            map[size - 1] = Height / 2 + rand.Next(-100, 100);

            Divide(map, 0, size - 1, 300f, 0.55f, rand);

            for (int x = 0; x < Width; x++)
            {
                int index = (int)((float)x / Width * (size - 1));
                _heightMap[x] = (int)MathHelper.Clamp(map[index], 50, Height - 50);
            }

            // Initial full texture upload
            UpdateFullTexture();
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

        private void UpdateFullTexture()
        {
            for (int x = 0; x < Width; x++)
            {
                int groundY = _heightMap[x];
                for (int y = 0; y < Height; y++)
                {
                    _colorData[y * Width + x] = (y >= groundY) ? Color.SaddleBrown : Color.Transparent;
                }
            }
            _texture.SetData(_colorData);
        }

        public int GetHeight(int x)
        {
            if (x < 0 || x >= Width) return Height;
            return _heightMap[x];
        }

        // PERFORMANCE: Optimized Destruct with partial texture update
        public void Destruct(int cx, int cy, int radius)
        {
            int minX = Math.Max(0, cx - radius);
            int maxX = Math.Min(Width - 1, cx + radius);

            // Optimization: Only scan Y lines that could possibly be affected
            // We only need to go down to the bottom of the circle
            int maxY = Math.Min(Height - 1, cy + radius);

            bool changed = false;
            int radiusSq = radius * radius;

            // 1. Update HeightMap (Physics)
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - cx;
                int dxSq = dx * dx;

                // Circle equation: dy^2 = r^2 - dx^2
                // We want the bottom of the circle: cy + sqrt(...)
                if (dxSq < radiusSq)
                {
                    // Integer sqrt is faster than float Math.Sqrt for physics logic if accuracy isn't critical,
                    // but Math.Sqrt is usually intrinsically optimized. Keeping Sqrt for correctness.
                    int dy = (int)Math.Sqrt(radiusSq - dxSq);
                    int circleBottom = cy + dy;

                    if (_heightMap[x] < circleBottom)
                    {
                        _heightMap[x] = Math.Min(circleBottom, Height);
                        changed = true;
                    }
                }
            }

            // 2. Update Texture (Graphics) - ONLY if changed and ONLY the affected rectangle
            if (changed)
            {
                // Determine the dirty rectangle. 
                // We need to redraw from the top of the terrain down to the bottom of the explosion.
                // Actually, simpler: just redraw the rectangle around the explosion.
                // However, falling dirt logic isn't here yet, so we just clear pixels above the new heightmap.

                int rectW = maxX - minX + 1;
                // We need to update from the highest possible ground point (0) down to the bottom of the explosion?
                // No, just the affected X columns are enough, but we need the full Y height for those columns 
                // to correctly clear the sky and draw the ground.
                // Optimization: Just update the rectangle defined by the explosion radius? 
                // No, because the ground level drops, so pixels *above* the explosion center become transparent.
                // Safer approach: Update the vertical strip for the affected X range.

                // Let's optimize: Find the min height in this X range to minimize upload
                int minYScan = 0;

                int rectH = maxY - minYScan + 1;

                // Safety check
                if (rectH <= 0 || rectW <= 0) return;

                Color[] patchData = new Color[rectW * rectH];

                for (int x = 0; x < rectW; x++)
                {
                    int worldX = minX + x;
                    int groundH = _heightMap[worldX];

                    for (int y = 0; y < rectH; y++)
                    {
                        int worldY = minYScan + y;
                        // Directly decide color
                        patchData[y * rectW + x] = (worldY >= groundH) ? Color.SaddleBrown : Color.Transparent;
                    }
                }

                // Upload ONLY the strip
                Rectangle dirtyRect = new Rectangle(minX, minYScan, rectW, rectH);
                _texture.SetData(0, dirtyRect, patchData, 0, patchData.Length);
            }
        }

        public void Update(GameTime gameTime)
        {
            _waveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw without blending if possible for performance, but here we need alpha for sky
            spriteBatch.Draw(_texture, Vector2.Zero, Color.White);
        }

        public void DrawWater(SpriteBatch spriteBatch)
        {
            int waterBaseHeight = WaterLevel;
            // Precalculate sin
            float waveOffset = (float)Math.Sin(_waveTime * 2f) * 5f;
            int waterY = waterBaseHeight + (int)waveOffset;

            // Simple rectangle draw is very fast
            spriteBatch.Draw(_waterTexture, new Rectangle(0, waterY, Width, Height - waterY), Color.White);
        }
    }
}