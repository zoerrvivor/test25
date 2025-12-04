using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Test25.Utilities;

namespace Test25.World
{
    public class Terrain
    {
        private int[] _heightMap;
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _effect;

        // CHANGED: Now using VertexPositionColorTexture to support gradient coloring
        private VertexPositionColorTexture[] _vertices;
        private Texture2D _groundTexture;
        private Texture2D _skyTexture;

        // Water
        private float _waveTime;
        private Texture2D _waterTexture;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int WaterLevel { get; private set; }

        public Terrain(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            Width = width;
            Height = height;
            WaterLevel = (int)(height * 0.85f);

            _heightMap = new int[width];

            // Initialize rendering effect
            _effect = new BasicEffect(_graphicsDevice);
            _effect.World = Matrix.Identity;
            _effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);

            // FIX: Enable Textures and Vertex Colors, Disable Lighting (which caused the black screen)
            _effect.TextureEnabled = true;
            _effect.VertexColorEnabled = true;
            _effect.LightingEnabled = false;

            // Generate visuals (Procedural textures)
            // Using slightly brighter colors since they will be modulated by vertex colors
            _groundTexture = TextureGenerator.CreateNoiseTexture(_graphicsDevice, 64, 64, new Color(160, 82, 45), new Color(205, 133, 63));
            _skyTexture = TextureGenerator.CreateGradientTexture(_graphicsDevice, 1, 512, Color.CornflowerBlue, Color.DeepSkyBlue);
            _waterTexture = TextureGenerator.CreateNoiseTexture(_graphicsDevice, 1, 1, new Color(0, 0, 200, 150), new Color(0, 0, 255, 150));

            InitializeVertices();
        }

        private void InitializeVertices()
        {
            // We create a triangle strip. 2 vertices per X coordinate (Top and Bottom).
            _vertices = new VertexPositionColorTexture[Width * 2];

            for (int x = 0; x < Width; x++)
            {
                // Bottom Vertex (Fixed at screen bottom)
                // Color.Gray simulates a shadow/ambient occlusion at the bottom of the map
                _vertices[x * 2] = new VertexPositionColorTexture(
                    new Vector3(x, Height, 0),
                    Color.DarkGray,
                    new Vector2(x / 64f, Height / 64f));

                // Top Vertex (Variable based on heightmap)
                // Color.White means full texture brightness
                _vertices[x * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(x, Height, 0),
                    Color.White,
                    new Vector2(x / 64f, 0));
            }
        }

        public void Generate(int seed)
        {
            Random rand = new(seed);
            int size = 1025;
            float[] map = new float[size];

            map[0] = Height / 2 + rand.Next(-100, 100);
            map[size - 1] = Height / 2 + rand.Next(-100, 100);

            Divide(map, 0, size - 1, 350f, 0.5f, rand);

            for (int x = 0; x < Width; x++)
            {
                int index = (int)((float)x / Width * (size - 1));
                int h = (int)MathHelper.Clamp(map[index], 50, Height - 20);
                _heightMap[x] = h;

                UpdateVertexHeight(x, h);
            }
        }

        private void UpdateVertexHeight(int x, int y)
        {
            // Update the Top Vertex (Index: x*2 + 1)
            // We directly modify the struct in the array
            _vertices[x * 2 + 1].Position.Y = y;
            _vertices[x * 2 + 1].TextureCoordinate.Y = y / 64f;
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

        public int GetHeight(int x)
        {
            if (x < 0) x = 0;
            if (x >= Width) x = Width - 1;
            return _heightMap[x];
        }

        public void Destruct(int cx, int cy, int radius)
        {
            int minX = Math.Max(0, cx - radius);
            int maxX = Math.Min(Width - 1, cx + radius);
            int radiusSq = radius * radius;

            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - cx;
                int dxSq = dx * dx;

                if (dxSq < radiusSq)
                {
                    int dy = (int)Math.Sqrt(radiusSq - dxSq);
                    int circleBottom = cy + dy;

                    if (_heightMap[x] < circleBottom)
                    {
                        int newHeight = Math.Min(circleBottom, Height);
                        _heightMap[x] = newHeight;
                        UpdateVertexHeight(x, newHeight);
                    }
                }
            }
        }

        public void Construct(int cx, int cy, int radius)
        {
            int minX = Math.Max(0, cx - radius);
            int maxX = Math.Min(Width - 1, cx + radius);
            int radiusSq = radius * radius;

            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - cx;
                int dxSq = dx * dx;

                if (dxSq < radiusSq)
                {
                    int dy = (int)Math.Sqrt(radiusSq - dxSq);
                    int circleTop = cy - dy;

                    if (circleTop < 0) circleTop = 0;

                    if (circleTop < _heightMap[x])
                    {
                        _heightMap[x] = circleTop;
                        UpdateVertexHeight(x, circleTop);
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            _waveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // 1. Draw Sky (Background) using SpriteBatch
            spriteBatch.Draw(_skyTexture, new Rectangle(0, 0, Width, Height), Color.White);

            // End SpriteBatch to switch to Geometry rendering
            spriteBatch.End();

            // --- CRITICAL FIX START ---
            // We must enable Texture Wrapping, otherwise the texture streaks (clamps) 
            // after the first 64 pixels.
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            // Optional: Ensure Depth/Rasterizer states are correct for 3D primitives
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            // --- CRITICAL FIX END ---

            // 2. Draw Terrain (Mesh)
            _effect.Texture = _groundTexture;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, (Width * 2) - 2);
            }

            // Restart SpriteBatch for the rest of the game (Tanks, UI)
            // Note: SpriteBatch.Begin() resets the SamplerState to Clamp by default, 
            // which is fine for UI/Sprites.
            spriteBatch.Begin();
        }

        public void DrawWater(SpriteBatch spriteBatch)
        {
            float offset = (float)Math.Sin(_waveTime * 3f) * 5f;
            float offset2 = (float)Math.Cos(_waveTime * 2f) * 4f;

            spriteBatch.Draw(_waterTexture, new Rectangle(0, WaterLevel + (int)offset + 10, Width, Height - WaterLevel), new Color(0, 0, 150, 100));
            spriteBatch.Draw(_waterTexture, new Rectangle(0, WaterLevel + (int)offset2, Width, Height - WaterLevel), new Color(0, 50, 255, 100));
        }
    }
}