using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Test25.Utilities;

namespace Test25.World
{
    public class Terrain
    {
        private GraphicsDevice _graphicsDevice;


        // Pixel-based terrain
        private Color[] _terrainData;
        private Texture2D _terrainTexture;
        private bool _isDirty = false;

        // Keep heightmap for generation only, or if needed for legacy spawn logic (approximated)
        private int[] _heightMap;

        private Texture2D _skyTexture;

        // Water
        private float _waveTime;
        private Texture2D _waterTexture;
        private Effect _waterEffect;
        private VertexBuffer _waterVertexBuffer;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int WaterLevel { get; private set; }
        public GraphicsDevice GraphicsDevice => _graphicsDevice;

        public Terrain(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            Width = width;
            Height = height;
            WaterLevel = (int)(height * Constants.WaterLevelRatio);

            _terrainData = new Color[Width * Height];
            _terrainTexture = new Texture2D(_graphicsDevice, Width, Height);
            _heightMap = new int[Width];

            _skyTexture = TextureGenerator.CreateGradientTexture(_graphicsDevice, 1, 512, Color.CornflowerBlue, Color.DeepSkyBlue);
            _waterTexture = TextureGenerator.CreateNoiseTexture(_graphicsDevice, 1, 1, new Color(0, 0, 200, 150), new Color(0, 0, 255, 150));
        }

        public void Generate(int seed)
        {
            Random rand = new(seed);
            int size = Constants.TerrainGenerationSize;
            float[] map = new float[size];

            map[0] = Height / 2 + rand.Next(-100, 100);
            map[size - 1] = Height / 2 + rand.Next(-100, 100);

            Divide(map, 0, size - 1, Constants.TerrainDisplacement, Constants.TerrainRoughness, rand);

            // clear terrain data
            for (int i = 0; i < _terrainData.Length; i++) _terrainData[i] = Color.Transparent;

            Color groundColor = Color.Brown;

            for (int x = 0; x < Width; x++)
            {
                int index = (int)((float)x / Width * (size - 1));
                int h = (int)MathHelper.Clamp(map[index], Constants.TerrainMinHeight, Height - Constants.TerrainMaxHeightOffset);
                _heightMap[x] = h;

                // Fill from height down to bottom
                for (int y = h; y < Height; y++)
                {
                    _terrainData[y * Width + x] = groundColor;
                }
            }

            _isDirty = true;
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

        // Legacy support - approximated surface height (highest solid pixel)
        public int GetHeight(int x)
        {
            if (x < 0) x = 0;
            if (x >= Width) x = Width - 1;

            // Scan from top to find first solid pixel
            for (int y = 0; y < Height; y++)
            {
                if (_terrainData[y * Width + x].A > 0) return y;
            }
            return Height;
        }

        public bool IsPixelSolid(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            return _terrainData[y * Width + x].A > 0;
        }

        public void Destruct(int cx, int cy, int radius)
        {
            int minX = Math.Max(0, cx - radius);
            int maxX = Math.Min(Width - 1, cx + radius);
            int minY = Math.Max(0, cy - radius);
            int maxY = Math.Min(Height - 1, cy + radius);
            int radiusSq = radius * radius;

            bool changed = false;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    if (dx * dx + dy * dy < radiusSq)
                    {
                        if (_terrainData[y * Width + x].A > 0)
                        {
                            _terrainData[y * Width + x] = Color.Transparent;
                            changed = true;
                        }
                    }
                }
            }

            if (changed) _isDirty = true;
        }

        public void Construct(int cx, int cy, int radius)
        {
            int minX = Math.Max(0, cx - radius);
            int maxX = Math.Min(Width - 1, cx + radius);
            int minY = Math.Max(0, cy - radius);
            int maxY = Math.Min(Height - 1, cy + radius);
            int radiusSq = radius * radius;

            bool changed = false;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    if (dx * dx + dy * dy < radiusSq)
                    {
                        // Only modify if empty? Or overwrite? Dirt clod adds dirt.
                        if (_terrainData[y * Width + x].A == 0)
                        {
                            _terrainData[y * Width + x] = Color.Brown; // Dirt color
                            changed = true;
                        }
                    }
                }
            }

            if (changed) _isDirty = true;
        }

        public void Update(GameTime gameTime)
        {
            _waveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isDirty)
            {
                _terrainTexture.SetData(_terrainData);
                _isDirty = false;
            }
        }

        public void DrawSky(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_skyTexture, new Rectangle(0, 0, Width, Height), Color.White);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the terrain texture
            spriteBatch.Draw(_terrainTexture, Vector2.Zero, Color.White);
        }

        public void DrawWater(SpriteBatch spriteBatch)
        {
            // Set shader parameters
            _waterEffect?.Parameters["WaveTime"]?.SetValue(_waveTime);
            _waterEffect?.Parameters["WaterTexture"]?.SetValue(_waterTexture);
            var projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);
            _waterEffect?.Parameters["WorldViewProjection"]?.SetValue(projection);

            // End SpriteBatch to switch to GPU rendering
            spriteBatch.End();

            // Apply effect and draw full-screen quad for water
            foreach (EffectPass pass in _waterEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.SetVertexBuffer(_waterVertexBuffer);
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
            // Reset vertex buffer
            _graphicsDevice.SetVertexBuffer(null);
            // Restart SpriteBatch for subsequent UI drawing
            spriteBatch.Begin();
        }

        public void LoadContent(ContentManager content)
        {
            // Load water effect
            _waterEffect = content.Load<Effect>("Effects/WaterEffect");

            // Create a full-screen quad for water rendering
            float margin = Constants.WaterMargin;
            float left = -margin;
            float right = Width + margin;
            float bottom = Height + margin;
            float top = WaterLevel;

            var vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(left, top, 0), new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(new Vector3(right, top, 0), new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(left, bottom, 0), new Vector2(0, 1));
            vertices[3] = new VertexPositionTexture(new Vector3(right, bottom, 0), new Vector2(1, 1));

            _waterVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            _waterVertexBuffer.SetData(vertices);
        }
    }
}