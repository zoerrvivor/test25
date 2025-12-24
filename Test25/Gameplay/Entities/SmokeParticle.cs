using Microsoft.Xna.Framework;
using System;

namespace Test25.Gameplay.Entities
{
    public class SmokeParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Lifetime;
        public float MaxLifetime;
        public float Size;
        public float Rotation;
        public float RotationSpeed;
        public float RandomSeed;

        public float NormalizedLifetime => MathHelper.Clamp(Lifetime / MaxLifetime, 0f, 1f);

        public SmokeParticle(Vector2 position, Vector2 velocity, float maxLifetime, float size)
        {
            Position = position;
            Velocity = velocity;
            MaxLifetime = maxLifetime;
            Lifetime = 0f;
            Size = size;
            Rotation = (float)Utilities.Rng.Instance.NextDouble() * MathHelper.TwoPi;
            RotationSpeed = ((float)Utilities.Rng.Instance.NextDouble() * 2f - 1f) * 2f;
            RandomSeed = (float)Utilities.Rng.Instance.NextDouble();
        }

        public void Update(float deltaTime, float wind)
        {
            Lifetime += deltaTime;
            
            // Apply wind as a force
            Velocity.X += wind * 0.5f * deltaTime;
            
            // Rising force (upward)
            Velocity.Y -= 20f * deltaTime;

            Position += Velocity * deltaTime;
            Rotation += RotationSpeed * deltaTime;
        }

        public bool IsDead => Lifetime >= MaxLifetime;
    }
}
