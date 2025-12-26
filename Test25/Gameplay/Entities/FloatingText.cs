using Microsoft.Xna.Framework;

namespace Test25.Gameplay.Entities
{
    public class FloatingText
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public string Text { get; set; }
        public Color Color { get; set; }
        public float Alpha { get; set; } = 1.0f;
        public float Lifetime { get; set; }
        public bool IsExpired => Lifetime <= 0;

        public FloatingText(Vector2 position, string text, Color color, Vector2 velocity)
        {
            Position = position;
            Text = text;
            Color = color;
            Velocity = velocity;
            Lifetime = Constants.DamageNumberLifetime;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * deltaTime;
            Lifetime -= deltaTime;

            if (Lifetime < Constants.DamageNumberFadeTime)
            {
                Alpha = Lifetime / Constants.DamageNumberFadeTime;
            }
        }
    }
}
