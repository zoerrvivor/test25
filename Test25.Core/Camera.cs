using Microsoft.Xna.Framework;
using Test25.Core.Utilities;

namespace Test25.Core
{
    public class Camera
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }
        public Vector2 Origin { get; set; }

        // Shake
        private float _shakeTrauma;
        private float _shakeDecay = 0.8f; // Trauma lost per second
        private float _maxShakeAngle = MathHelper.ToRadians(10);
        private float _maxShakeOffset = 15f;

        private int _viewWidth;
        private int _viewHeight;

        public Camera(int width, int height)
        {
            _viewWidth = width;
            _viewHeight = height;
            Zoom = 1.0f;
            Rotation = 0.0f;
            Position = Vector2.Zero;
            Origin = new Vector2(width / 2f, height / 2f);
        }

        public void Resize(int width, int height)
        {
            _viewWidth = width;
            _viewHeight = height;
            Origin = new Vector2(width / 2f, height / 2f);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_shakeTrauma > 0)
            {
                _shakeTrauma -= _shakeDecay * dt;
                if (_shakeTrauma < 0) _shakeTrauma = 0;
            }
        }

        public void Shake(float amount)
        {
            _shakeTrauma += amount;
            if (_shakeTrauma > 1.0f) _shakeTrauma = 1.0f;
        }

        public Matrix GetViewMatrix()
        {
            // Shake Effect
            float shake = _shakeTrauma * _shakeTrauma; // Quadratic falloff
            float angle = _maxShakeAngle * shake * GetNoise(0);
            float offsetX = _maxShakeOffset * shake * GetNoise(100);
            float offsetY = _maxShakeOffset * shake * GetNoise(200);

            return Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                   Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                   Matrix.CreateRotationZ(Rotation + angle) *
                   Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                   Matrix.CreateTranslation(new Vector3(Origin, 0.0f)) *
                   Matrix.CreateTranslation(new Vector3(offsetX, offsetY, 0));
        }

        // Simple pseudo-random noise for shake
        private float GetNoise(int seedOffset)
        {
            // Using time for continuous noise would be better (Perlin), 
            // but simple random sample each frame works for "violent" shake.
            // However, truly random frame-by-frame is very high frequency.
            // Let's stick to very simple random for now as requested.
            return (float)(Rng.Instance.NextDouble() * 2.0 - 1.0);
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix()));
        }
    }
}
