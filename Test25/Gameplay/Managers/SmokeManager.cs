using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System;
using Test25.Gameplay.Entities;
using Test25.Utilities;

namespace Test25.Gameplay.Managers
{
    public class SmokeManager : IDisposable
    {
        private List<SmokeParticle> _particles = new();
        private Effect _smokeEffect;
        private Texture2D _baseTexture;
        private GraphicsDevice _graphicsDevice;
        private float _totalTime;
        private float _lastWind;


        public SmokeManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            // Use a simple circle texture as the base for particles
            _baseTexture = TextureGenerator.CreateSoftCircleTexture(graphicsDevice, 32);
        }

        public void LoadContent(ContentManager content)
        {
            _smokeEffect = content.Load<Effect>("Effects/SmokeEffect");
        }

        public void EmitSmoke(Vector2 position)
        {
            // Randomize velocity and size slightly
            float angle = -MathHelper.PiOver2 + ((float)Rng.Instance.NextDouble() * 0.4f - 0.2f);
            float speed = 20f + (float)Rng.Instance.NextDouble() * 20f;
            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

            float lifetime = 2f + (float)Rng.Instance.NextDouble() * 2f;
            float size = 0.15f + (float)Rng.Instance.NextDouble() * 0.15f; // Much smaller base size

            _particles.Add(new SmokeParticle(position, velocity, lifetime, size));
        }

        public void Update(GameTime gameTime, float wind)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _totalTime += deltaTime;
            _lastWind = wind;

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update(deltaTime, wind);
                if (_particles[i].IsDead)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Matrix viewMatrix)
        {
            if (_particles.Count == 0 || _smokeEffect == null) return;

            // Set global shader parameters
            _smokeEffect.Parameters["Time"]?.SetValue(_totalTime);
            // Pass the wind value
            _smokeEffect.Parameters["Wind"]?.SetValue(_lastWind);

            // MANUALLY calculate the matrix if SpriteBatch isn't doing it correctly
            // Projection for 2D Viewport
            var projection = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width,
                _graphicsDevice.Viewport.Height, 0, 0, 1);
            Matrix wvp = viewMatrix * projection;
            _smokeEffect.Parameters["MatrixTransform"]?.SetValue(wvp);

            spriteBatch.End();
            // Passing viewMatrix to Begin might apply it TWICE if our shader also uses MatrixTransform.
            // Let's use Identity here for SpriteBatch because we already baked the view into MatrixTransform.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null,
                _smokeEffect, Matrix.Identity);

            foreach (var p in _particles)
            {
                float alpha = 1.0f - p.NormalizedLifetime;
                // Scale based on size and lifetime (getting slightly larger as it rises)
                float scale = p.Size * (1.0f + p.NormalizedLifetime * 1.5f);


                spriteBatch.Draw(_baseTexture, p.Position, null, Color.White * alpha, p.Rotation,
                    new Vector2(_baseTexture.Width / 2f, _baseTexture.Height / 2f), scale, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            // Restart SpriteBatch with original settings
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, viewMatrix);
        }


        public void Reset()
        {
            _particles.Clear();
        }

        public void Dispose()
        {
            _baseTexture?.Dispose();
            // Effect is managed by ContentManager
        }
    }
}
