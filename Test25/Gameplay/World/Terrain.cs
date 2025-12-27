// START OF FILE Test25/World/Terrain.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using Test25.Utilities;

namespace Test25.Gameplay.World
{
    public class Terrain : IDisposable
    {
        private GraphicsDevice _graphicsDevice;

        // --- CPU Daten für Physik (Kollision) ---
        private Color[] _physicsData; // "Truth" im RAM
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int WaterLevel { get; private set; }

        // --- GPU Daten für Grafik (Rendering) ---
        private RenderTarget2D _terrainRenderTarget; // Die dauerhafte Textur im VRAM
        private Texture2D _craterBrush; // Der "Radiergummi"
        private Texture2D _burnBrush; // Der "Brenner" für den Rand
        private BlendState _eraserBlendState; // Die Magie zum Löcher schneiden
        private BlendState _burnBlendState; // Abdunkeln, aber nur wo bereits Terrain ist

        // Water Effects
        private float _waveTime;
        private Texture2D _waterTexture;
        private Effect _waterEffect;
        private VertexBuffer _waterVertexBuffer;
        private Texture2D _skyTexture;

        public GraphicsDevice GraphicsDevice => _graphicsDevice;

        public Terrain(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            Width = width;
            Height = height;
            WaterLevel = (int)(height * Constants.WaterLevelRatio);

            _physicsData = new Color[Width * Height];

            // 1. RenderTarget erstellen
            // PreserveContents ist wichtig, damit das Bild nicht gelöscht wird, wenn wir es mal nicht zeichnen
            _terrainRenderTarget = new RenderTarget2D(_graphicsDevice, Width, Height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            // 2. BlendState für das "Radieren" erstellen
            // Dieser State sorgt dafür, dass dort, wo wir zeichnen, der Alpha-Wert des Ziels auf 0 gesetzt wird.
            _eraserBlendState = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend =
                    Blend.InverseSourceAlpha, // Wo der Pinsel deckt, wird die alte Farbe weggeskaliert

                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.InverseSourceAlpha // Wo der Pinsel deckt (1), wird das Ziel (0)
            };

            // 3. BlendState für den "Burn" Effekt (Abdunkeln ohne Alpha-Änderung)
            _burnBlendState = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceAlpha,

                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.DestinationAlpha // Wir behalten das bestehende Alpha (Sky bleibt Sky)
            };

            // Pinsel erstellen (Radius 50 ist nur die Texturgröße, skaliert wird beim Zeichnen)
            _craterBrush = TextureGenerator.CreateCircleTexture(_graphicsDevice, 50, Color.White);
            _burnBrush = TextureGenerator.CreateSoftCircleTexture(_graphicsDevice, 50);

            _skyTexture = TextureGenerator.CreateGradientTexture(_graphicsDevice, 1, 512,
                Color.CornflowerBlue, Color.DeepSkyBlue);

            _waterTexture = TextureGenerator.CreateNoiseTexture(_graphicsDevice, 64, 64,
                new Color(0, 0, 200, 150), new Color(0, 0, 255, 150));
        }

        public void Generate(int seed)
        {
            Random rand = new(seed);
            int size = Constants.TerrainGenerationSize;
            float[] map = new float[size];

            map[0] = Height / 2 + rand.Next(-100, 100);
            map[size - 1] = Height / 2 + rand.Next(-100, 100);

            Divide(map, 0, size - 1, Constants.TerrainDisplacement, Constants.TerrainRoughness, rand);

            // Reset Physics Data
            for (int i = 0; i < _physicsData.Length; i++) _physicsData[i] = Color.Transparent;
            Color groundColor = Color.SaddleBrown; // Etwas dunkler für besseren Kontrast

            // Fill Physics Array
            for (int x = 0; x < Width; x++)
            {
                int index = (int)((float)x / Width * (size - 1));
                int h = (int)MathHelper.Clamp(map[index], Constants.TerrainMinHeight,
                    Height - Constants.TerrainMaxHeightOffset);

                for (int y = h; y < Height; y++)
                {
                    _physicsData[y * Width + x] = groundColor;
                }
            }

            // Initial GPU Upload
            // Wir machen das EINMAL am Anfang des Spiels. Hier ist SetData okay.
            UpdateRenderTargetFull();
        }

        // Hilfsmethode, um das CPU-Array komplett auf die GPU zu schieben (nur bei Init)
        private void UpdateRenderTargetFull()
        {
            // Temporäre Textur erstellen, um Daten hochzuladen
            Texture2D tempTex = new Texture2D(_graphicsDevice, Width, Height);
            tempTex.SetData(_physicsData);

            // Auf das RenderTarget zeichnen
            _graphicsDevice.SetRenderTarget(_terrainRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);

            using (SpriteBatch sb = new SpriteBatch(_graphicsDevice))
            {
                sb.Begin();
                sb.Draw(tempTex, Vector2.Zero, Color.White);
                sb.End();
            }

            _graphicsDevice.SetRenderTarget(null); // Zurück zum Backbuffer
            tempTex.Dispose();
        }

        // Rekursive Terrain-Funktion (unverändert)
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

        // --- SCHNELLE GPU ZERSTÖRUNG ---
        public void Destruct(int cx, int cy, int radius)
        {
            // 1. GPU: Loch schneiden (Visuell)
            // Wir zeichnen auf das RenderTarget, nicht auf den Bildschirm!
            _graphicsDevice.SetRenderTarget(_terrainRenderTarget);

            // Wichtig: Keinen Clear() aufrufen, wir wollen das bestehende Bild behalten!

            using (SpriteBatch sb = new SpriteBatch(_graphicsDevice))
            {
                // Zuerst den Rand "verbrennen" (abdunkeln)
                // Wir nutzen den speziellen _burnBlendState, damit nur Terrain betroffen ist
                sb.Begin(SpriteSortMode.Immediate, _burnBlendState);
                float burnScale = (radius * 2.3f) / _burnBrush.Width;
                Vector2 burnOrigin = new Vector2(_burnBrush.Width / 2f, _burnBrush.Height / 2f);
                // Mit _burnBlendState und Color.White bewirkt das Brush-Alpha eine Multiplikation (Abdunklung)
                sb.Draw(_burnBrush, new Vector2(cx, cy), null, Color.White, 0f, burnOrigin, burnScale,
                    SpriteEffects.None, 0f);
                sb.End();

                // Dann das eigentliche Loch radieren
                // Hier nutzen wir unseren "Eraser" BlendState
                sb.Begin(SpriteSortMode.Immediate, _eraserBlendState);

                // Berechne Position und Skalierung für den Pinsel
                // Der Pinsel ist z.B. 100x100px. Wenn Radius 40 ist, Durchmesser 80. Scale = 0.8
                float scale = (radius * 2f) / _craterBrush.Width;
                Vector2 origin = new Vector2(_craterBrush.Width / 2f, _craterBrush.Height / 2f);

                sb.Draw(_craterBrush, new Vector2(cx, cy), null, Color.White, 0f, origin, scale, SpriteEffects.None,
                    0f);

                sb.End();
            }

            // Zurücksetzen
            _graphicsDevice.SetRenderTarget(null);


            // 2. CPU: Array updaten (Physik & Konsistenz)
            // Wir müssen weiterhin wissen, wo Pixel sind, aber wir iterieren nur über das kleine Rechteck
            int burnRadius = (int)(radius * 1.15f);
            int burnRadiusSq = burnRadius * burnRadius;
            int radiusSq = radius * radius;

            int minX = Math.Max(0, cx - burnRadius);
            int maxX = Math.Min(Width - 1, cx + burnRadius);
            int minY = Math.Max(0, cy - burnRadius);
            int maxY = Math.Min(Height - 1, cy + burnRadius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    int distSq = dx * dx + dy * dy;

                    if (distSq < radiusSq)
                    {
                        // Einfach auf Transparent setzen im RAM
                        _physicsData[y * Width + x] = Color.Transparent;
                    }
                    else if (distSq < burnRadiusSq)
                    {
                        // Pixel abdunkeln für den "Burned" Effekt (Konsistenz)
                        Color c = _physicsData[y * Width + x];
                        if (c.A > 0)
                        {
                            _physicsData[y * Width + x] = new Color(
                                (byte)(c.R * 0.4f),
                                (byte)(c.G * 0.4f),
                                (byte)(c.B * 0.4f),
                                c.A
                            );
                        }
                    }
                }
            }

            WakeUpPhysicsArea(cx, cy, radius + 5);
        }

        // Hinzufügen (Construct) - z.B. für Dirt Gun
        public void Construct(int cx, int cy, int radius)
        {
            // 1. GPU: Zeichnen (Normaler BlendState)
            _graphicsDevice.SetRenderTarget(_terrainRenderTarget);
            using (SpriteBatch sb = new SpriteBatch(_graphicsDevice))
            {
                sb.Begin(); // Default ist AlphaBlend (Additiv)

                float scale = (radius * 2f) / _craterBrush.Width;
                Vector2 origin = new Vector2(_craterBrush.Width / 2f, _craterBrush.Height / 2f);

                sb.Draw(_craterBrush, new Vector2(cx, cy), null, Color.SaddleBrown, 0f, origin, scale,
                    SpriteEffects.None, 0f);

                sb.End();
            }

            _graphicsDevice.SetRenderTarget(null);

            // 2. CPU: Physik updaten
            int radiusSq = radius * radius;
            int minX = Math.Max(0, cx - radius);
            int maxX = Math.Min(Width - 1, cx + radius);
            int minY = Math.Max(0, cy - radius);
            int maxY = Math.Min(Height - 1, cy + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    if (dx * dx + dy * dy < radiusSq)
                    {
                        // Nur leere Pixel füllen?
                        if (_physicsData[y * Width + x].A == 0)
                        {
                            _physicsData[y * Width + x] = Color.SaddleBrown;
                        }
                    }
                }
            }
        }

        // Physik-Abfragen nutzen NUR das schnelle RAM-Array
        public bool IsPixelSolid(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            return _physicsData[y * Width + x].A > 0;
        }

        // Legacy Support für Spawning / Roller (Höhe finden)
        public int GetHeight(int x)
        {
            if (x < 0) x = 0;
            if (x >= Width) x = Width - 1;
            for (int y = 0; y < Height; y++)
            {
                if (_physicsData[y * Width + x].A > 0) return y;
            }

            return Height;
        }

        private const int ChunkSize = 64;
        private int _chunksX;
        private int _chunksY;
        private bool[,] _activeChunks;
        private bool[,] _nextActiveChunks;
        private int _physicsFrameCount;

        // Initialize chunks in Constructor! We'll do it lazily or assume called after width/height set.
        // Since we don't have access to constructor here easily without replacing whole file, 
        // we'll initialize on first Update or property usage.

        private void EnsureChunksInitialized()
        {
            if (_activeChunks == null)
            {
                _chunksX = (int)Math.Ceiling((double)Width / ChunkSize);
                _chunksY = (int)Math.Ceiling((double)Height / ChunkSize);
                _activeChunks = new bool[_chunksX, _chunksY];
                _nextActiveChunks = new bool[_chunksX, _chunksY];
            }
        }

        public void Update(GameTime gameTime)
        {
            _waveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            EnsureChunksInitialized();

            _physicsFrameCount++;
            if (_physicsFrameCount % 2 == 0)
            {
                SimulateActiveChunks();
            }
        }

        private void WakeUpPhysicsArea(int x, int y, int radius)
        {
            EnsureChunksInitialized();

            int startX = (x - radius) / ChunkSize;
            int endX = (x + radius) / ChunkSize;
            int startY = (y - radius) / ChunkSize;
            int endY = (y + radius) / ChunkSize;

            startX = Math.Max(0, startX);
            endX = Math.Min(_chunksX - 1, endX);
            startY = Math.Max(0, startY);
            endY = Math.Min(_chunksY - 1, endY);

            for (int cy = startY; cy <= endY; cy++)
            {
                for (int cx = startX; cx <= endX; cx++)
                {
                    _activeChunks[cx, cy] = true;
                    // Also wake neighbors immediately to be safe? 
                    // No, propagation will hande it, but let's be generous for initial blast
                }
            }
        }

        private void SimulateActiveChunks()
        {
            bool globalChanges = false;
            int globalMinX = Width;
            int globalMaxX = 0;
            int globalMinY = Height;
            int globalMaxY = 0;

            // Clear next frame
            Array.Clear(_nextActiveChunks, 0, _nextActiveChunks.Length);

            // Iterate chunks Bottom-Up
            for (int cy = _chunksY - 1; cy >= 0; cy--)
            {
                for (int cx = 0; cx < _chunksX; cx++)
                {
                    if (!_activeChunks[cx, cy]) continue;

                    bool chunkChanged = false;

                    // Calculate bounds for this chunk
                    int startX = cx * ChunkSize;
                    int endX = Math.Min(startX + ChunkSize, Width); // Exclusive? No, loop is <
                    int startY = cy * ChunkSize;
                    int endY = Math.Min(startY + ChunkSize, Height);

                    // We need to iterate pixels. 
                    // Physics usually needs Y-1 check.
                    // Bottom-Up logic:
                    // For y inside chunk:

                    int iterStartY = endY - 1;
                    if (iterStartY >= Height - 1) iterStartY = Height - 2; // Don't check last row down
                    int iterEndY = startY;

                    for (int y = iterStartY; y >= iterEndY; y--)
                    {
                        int rowOffset = y * Width;
                        int nextRowOffset = rowOffset + Width;

                        for (int x = startX; x < endX; x++) // x < endX so correct
                        {
                            if (_physicsData[rowOffset + x].A == 0) continue;

                            Color pixel = _physicsData[rowOffset + x];
                            bool moved = false;

                            // Check Down
                            // (Safeguard done by iterStartY)
                            if (_physicsData[nextRowOffset + x].A == 0)
                            {
                                MovePixel(x, y, x, y + 1, pixel);
                                moved = true;
                            }
                            else
                            {
                                // Slide
                                bool checkLeft = ((x ^ y) & 1) == 0;
                                if (checkLeft)
                                {
                                    if (x > 0 && _physicsData[nextRowOffset + (x - 1)].A == 0)
                                    {
                                        MovePixel(x, y, x - 1, y + 1, pixel);
                                        moved = true;
                                    }
                                }
                                else
                                {
                                    if (x < Width - 1 && _physicsData[nextRowOffset + (x + 1)].A == 0)
                                    {
                                        MovePixel(x, y, x + 1, y + 1, pixel);
                                        moved = true;
                                    }
                                }
                            }

                            if (moved)
                            {
                                chunkChanged = true;

                                // Update Global Dirty Rect
                                // Since we moved from (x,y) to (x+/-1, y+1)
                                if (x - 1 < globalMinX) globalMinX = x - 1;
                                if (x + 1 > globalMaxX) globalMaxX = x + 1;
                                if (y < globalMinY) globalMinY = y;
                                if (y + 1 > globalMaxY) globalMaxY = y + 1;
                            }
                        }
                    }

                    if (chunkChanged)
                    {
                        globalChanges = true;

                        // Wake up self for next frame
                        _nextActiveChunks[cx, cy] = true;

                        // Wake up neighbors (Up, Down, Left, Right)
                        // Especially UP is critical for "hanging" terrain
                        if (cy > 0) _nextActiveChunks[cx, cy - 1] = true;
                        if (cy < _chunksY - 1) _nextActiveChunks[cx, cy + 1] = true;
                        if (cx > 0) _nextActiveChunks[cx - 1, cy] = true;
                        if (cx < _chunksX - 1) _nextActiveChunks[cx + 1, cy] = true;
                    }
                }
            }

            // Swap arrays
            var temp = _activeChunks;
            _activeChunks = _nextActiveChunks;
            _nextActiveChunks = temp;
            // Reuse the array to avoid alloc, just need to clear it next frame.

            if (globalChanges)
            {
                ApplyPhysicsToVisuals(globalMinX, globalMaxX, globalMinY, globalMaxY);
            }
        }


        private bool TrySlideLeft(int x, int y, Color pixel, ref int minX, ref int maxX, ref int minY, ref int maxY)
        {
            if (x > 0)
            {
                int destIndex = (y + 1) * Width + (x - 1);
                if (_physicsData[destIndex].A == 0)
                {
                    MovePixel(x, y, x - 1, y + 1, pixel);
                    UpdateDirtyRect(x, y, x - 1, y + 1, ref minX, ref maxX, ref minY, ref maxY);
                    return true;
                }
            }

            return false;
        }

        private bool TrySlideRight(int x, int y, Color pixel, ref int minX, ref int maxX, ref int minY, ref int maxY)
        {
            if (x < Width - 1)
            {
                int destIndex = (y + 1) * Width + (x + 1);
                if (_physicsData[destIndex].A == 0)
                {
                    MovePixel(x, y, x + 1, y + 1, pixel);
                    UpdateDirtyRect(x, y, x + 1, y + 1, ref minX, ref maxX, ref minY, ref maxY);
                    return true;
                }
            }

            return false;
        }

        private void MovePixel(int startX, int startY, int endX, int endY, Color pixel)
        {
            int startIndex = startY * Width + startX;
            int endIndex = endY * Width + endX;

            _physicsData[endIndex] = pixel;
            _physicsData[startIndex] = Color.Transparent;
        }

        private void UpdateDirtyRect(int x1, int y1, int x2, int y2, ref int minX, ref int maxX, ref int minY,
            ref int maxY)
        {
            int lx = Math.Min(x1, x2);
            int rx = Math.Max(x1, x2);
            int ty = Math.Min(y1, y2);
            int by = Math.Max(y1, y2);

            if (lx < minX) minX = lx;
            if (rx > maxX) maxX = rx;
            if (ty < minY) minY = ty;
            if (by > maxY) maxY = by;
        }

        private Texture2D _scratchTexture;

        private void EnsureScratchTexture(int width, int height)
        {
            if (_scratchTexture == null || _scratchTexture.Width < width || _scratchTexture.Height < height)
            {
                _scratchTexture?.Dispose();
                // Create a slightly larger texture to avoid frequent resizing
                int newW = Math.Max(256, width);
                int newH = Math.Max(256, height);
                _scratchTexture = new Texture2D(_graphicsDevice, newW, newH);
            }
        }

        private void ApplyPhysicsToVisuals(int minX, int maxX, int minY, int maxY)
        {
            // Pad the rect
            minX = Math.Max(0, minX - 1);
            maxX = Math.Min(Width - 1, maxX + 1);
            minY = Math.Max(0, minY - 1);
            maxY = Math.Min(Height - 1, maxY + 1);

            int w = maxX - minX + 1;
            int h = maxY - minY + 1;

            if (w <= 0 || h <= 0) return;

            // Ensure our scratch texture is big enough
            EnsureScratchTexture(w, h);

            // Extract data directly into a temporary array (still alloc, but purely CPU buffer)
            // Optimization: Keep a persistent data buffer too? 
            // For now, let's just stick to the texture optimization as it's the biggest GPU stall.
            Color[] regionData = new Color[w * h];

            // Parallel copy? No, not worth overhead for small chunks.
            for (int y = 0; y < h; y++)
            {
                int rowStart = (minY + y) * Width + minX;
                Array.Copy(_physicsData, rowStart, regionData, y * w, w);
            }

            // Set Data on the scratch texture (only the part we use)
            // Hint: SetData allows setting a sub-rectangle, but we are setting 0,0 to w,h of the scratch texture?
            // Actually, we can just set the data to the w*h area.
            // But we need to be careful if we draw the whole scratch texture.
            // We should use 'SetData' with a rect or just set the array.
            // Let's SetData on a rectangle of the scratch texture to correspond to our data.
            // Actually, if we just overwrite the scratch texture's 0,0 corner, we can just draw that source rect.

            // Note: MonoGame SetData is: SetData(T[] data, int startIndex, int elementCount) 
            // OR SetData(int level, Rectangle? rect, T[] data, int startIndex, int elementCount)

            Rectangle updateRect = new Rectangle(0, 0, w, h);
            _scratchTexture.SetData(0, updateRect, regionData, 0, regionData.Length);

            // Blit to RenderTarget
            BlendState replacementBlend = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero
            };

            _graphicsDevice.SetRenderTarget(_terrainRenderTarget);
            using (SpriteBatch sb = new SpriteBatch(_graphicsDevice))
            {
                sb.Begin(SpriteSortMode.Immediate, replacementBlend);
                sb.Draw(_scratchTexture, new Vector2(minX, minY), updateRect, Color.White); // Draw only the valid part
                sb.End();
            }

            _graphicsDevice.SetRenderTarget(null);
        }

        public void DrawSky(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_skyTexture, new Rectangle(0, 0, Width, Height), Color.White);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Wir zeichnen einfach das RenderTarget als Textur
            spriteBatch.Draw(_terrainRenderTarget, Vector2.Zero, Color.White);
        }

        // Water Drawing Code (unverändert, nutzt aber jetzt _terrainRenderTarget nicht mehr direkt, da Wasser drüber liegt)
        // ... LoadContent und DrawWater Methoden können so bleiben wie in deinem Original ...

        public void DrawWater(SpriteBatch spriteBatch, Matrix viewTransform)
        {
            // Set shader parameters
            _waterEffect?.Parameters["WaveTime"]?.SetValue(_waveTime);
            _waterEffect?.Parameters["WaterTexture"]?.SetValue(_waterTexture);

            // Standard Orthographic Projection for 2D
            var projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);

            // Combine with Camera View (Shake/Zoom/Pan)
            // WorldViewProjection = World * View * Projection
            // Here our "World" is Identity (sprites are already at world pos).
            // So we need View * Projection. 
            // Note: SpriteBatch transform serves as the View matrix usually.

            Matrix wvp = viewTransform * projection;

            _waterEffect?.Parameters["WorldViewProjection"]?.SetValue(wvp);

            // End SpriteBatch to switch to GPU rendering
            spriteBatch.End();

            // Apply effect and draw full-screen quad for water
            if (_waterEffect != null)
                foreach (EffectPass pass in _waterEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.SetVertexBuffer(_waterVertexBuffer);
                    _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                }

            // Reset vertex buffer
            _graphicsDevice.SetVertexBuffer(null);

            // Restart SpriteBatch with the same transform for consistency if caller continues drawing
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, viewTransform);
        }

        public void LoadContent(ContentManager content)
        {
            _waterEffect = content.Load<Effect>("Effects/WaterEffect");

            // Create Fullscreen Quad for Water
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

            _waterVertexBuffer =
                new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            _waterVertexBuffer.SetData(vertices);
        }

        // Dekorations-Stamping (für Ruinen) muss auch angepasst werden
        public void Blit(Texture2D source, int x, int y)
        {
            // 1. GPU
            _graphicsDevice.SetRenderTarget(_terrainRenderTarget);
            using (SpriteBatch sb = new SpriteBatch(_graphicsDevice))
            {
                sb.Begin();
                sb.Draw(source, new Vector2(x, y), Color.White);
                sb.End();
            }

            _graphicsDevice.SetRenderTarget(null);

            // 2. CPU
            Color[] sourceData = new Color[source.Width * source.Height];
            source.GetData(sourceData);

            int startX = Math.Max(0, x);
            int startY = Math.Max(0, y);
            int endX = Math.Min(Width, x + source.Width);
            int endY = Math.Min(Height, y + source.Height);

            for (int py = startY; py < endY; py++)
            {
                for (int px = startX; px < endX; px++)
                {
                    int srcX = px - x;
                    int srcY = py - y;
                    Color srcPixel = sourceData[srcY * source.Width + srcX];
                    if (srcPixel.A > 0)
                    {
                        _physicsData[py * Width + px] = srcPixel;
                    }
                }
            }
        }

        public void Dispose()
        {
            _terrainRenderTarget?.Dispose();
            _craterBrush?.Dispose();
            _burnBrush?.Dispose();
            _skyTexture?.Dispose();
            _waterTexture?.Dispose();
            _waterVertexBuffer?.Dispose();
            _scratchTexture?.Dispose();
        }
    }
}