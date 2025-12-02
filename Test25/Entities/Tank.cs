// Version: 0.3
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test25.Entities
{
    public class Tank : GameObject
    {
        public int PlayerIndex { get; private set; }
        public string Name { get; private set; }
        public float Health { get; set; } = 100f;
        public float Fuel { get; set; } = 100f;
        public float Power { get; set; } = 50f;
        public float TurretAngle { get; set; } = MathHelper.PiOver2;
        public Color Color { get; set; }
        public int Money { get; set; } = 0;
        public int Score { get; set; } = 0;
        public float DamageMultiplier { get; set; } = 1.0f;

        public List<InventoryItem> Inventory { get; private set; }
        public Weapon CurrentWeapon { get; private set; }

        public Rectangle BoundingBox => new Rectangle((int)(Position.X - _bodyTexture.Width / 2f), (int)(Position.Y - _bodyTexture.Height), _bodyTexture.Width, _bodyTexture.Height);

        private Texture2D _bodyTexture;
        private Texture2D _barrelTexture;
        private Vector2 _turretOffset;

        public bool IsAI { get; set; }
        private float _dialogueTimer;
        private string _currentDialogue;
        private static Test25.Managers.DialogueManager _dialogueManager;

        public static void SetDialogueManager(Test25.Managers.DialogueManager manager) => _dialogueManager = manager;

        private void ShowDialogue(string text)
        {
            _currentDialogue = text;
            _dialogueTimer = 3f;
        }

        public Tank(int playerIndex, string name, Vector2 startPosition, Color color, Texture2D bodyTexture, Texture2D barrelTexture, bool isAI = false)
        {
            PlayerIndex = playerIndex;
            Name = name;
            Position = startPosition;
            Color = color;
            _bodyTexture = bodyTexture;
            _barrelTexture = barrelTexture;
            IsAI = isAI;

            _turretOffset = new Vector2(0, -_bodyTexture.Height / 2f);

            Inventory = new List<InventoryItem>();
            var defaultWeapon = new Weapon("Standard Shell", "Basic projectile", 20f, 20f, 1, true);
            Inventory.Add(defaultWeapon);
            CurrentWeapon = defaultWeapon;
        }

        public override void Update(GameTime gameTime)
        {
            if (_dialogueTimer > 0)
                _dialogueTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
                _currentDialogue = null;
        }

        public void Update(GameTime gameTime, Terrain terrain)
        {
            Update(gameTime);
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float gravity = Constants.Gravity;

            var vel = Velocity;
            vel.Y += gravity * deltaTime;
            Velocity = vel;
            Position += Velocity * deltaTime;

            // Ground Check
            if (Position.X >= 0 && Position.X < terrain.Width)
            {
                int groundHeight = terrain.GetHeight((int)Position.X);

                if (Position.Y > groundHeight)
                {
                    // Fall damage check using Constants
                    if (Velocity.Y > Constants.FallDamageThreshold)
                    {
                        var parachute = GetItem<Item>("Parachute");
                        if (parachute != null && parachute.Count > 0)
                        {
                            parachute.Count--;
                            if (parachute.Count <= 0 && !parachute.IsInfinite) Inventory.Remove(parachute);
                        }
                        else
                        {
                            float damage = (Velocity.Y - Constants.FallDamageThreshold) * Constants.FallDamageMultiplier;
                            TakeDamage(damage);
                        }
                    }

                    // Snap to ground
                    Position = new Vector2(Position.X, groundHeight);
                    Velocity = new Vector2(Velocity.X, 0);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (_bodyTexture != null && _barrelTexture != null)
            {
                Vector2 barrelOrigin = new Vector2(0, _barrelTexture.Height / 2f);
                Vector2 barrelPosition = Position + _turretOffset;
                spriteBatch.Draw(_barrelTexture, barrelPosition, null, Color, -TurretAngle, barrelOrigin, 1f, SpriteEffects.None, 0f);

                Vector2 bodyOrigin = new Vector2(_bodyTexture.Width / 2f, _bodyTexture.Height);
                spriteBatch.Draw(_bodyTexture, Position, null, Color, 0f, bodyOrigin, 1f, SpriteEffects.None, 0f);
            }

            if (!string.IsNullOrEmpty(_currentDialogue))
            {
                var pos = Position - new Vector2(0, _bodyTexture.Height + 20);
                float alpha = 1f;
                if (_dialogueTimer < 1f) alpha = _dialogueTimer;

                spriteBatch.DrawString(font, _currentDialogue, pos, Color.Yellow * alpha);
            }
        }

        public Projectile Fire(Texture2D projectileTexture)
        {
            Vector2 barrelPosition = Position + _turretOffset;
            float barrelLength = _barrelTexture.Width;
            Vector2 offset = new Vector2((float)Math.Cos(-TurretAngle), (float)Math.Sin(-TurretAngle)) * barrelLength;
            Vector2 spawnPos = barrelPosition + offset;

            // Using Constant for Power Multiplier
            float speed = Power * Constants.PowerMultiplier;
            Vector2 velocity = new Vector2((float)Math.Cos(-TurretAngle), (float)Math.Sin(-TurretAngle)) * speed;

            float damage = CurrentWeapon.Damage * DamageMultiplier;
            float radius = CurrentWeapon.ExplosionRadius;

            if (!CurrentWeapon.IsInfinite)
            {
                CurrentWeapon.Count--;
                if (CurrentWeapon.Count <= 0)
                {
                    Inventory.Remove(CurrentWeapon);
                    SelectNextWeapon();
                }
            }

            var phrase = _dialogueManager?.GetRandomShootPhrase(PlayerIndex);
            if (phrase != null) ShowDialogue(phrase);

            return new Projectile(spawnPos, velocity, projectileTexture) { Damage = damage, ExplosionRadius = radius };
        }

        public void TakeDamage(float amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                IsActive = false;
                var phrase = _dialogueManager?.GetRandomHitPhrase(PlayerIndex);
                if (phrase != null) ShowDialogue(phrase);
            }
            if (Power > Health) Power = Health;
        }

        public void AdjustAim(float delta)
        {
            TurretAngle += delta;
            if (TurretAngle < 0) TurretAngle = 0;
            if (TurretAngle > MathHelper.Pi) TurretAngle = MathHelper.Pi;
        }

        public void AdjustPower(float delta)
        {
            Power += delta;
            if (Power < 0) Power = 0;
            if (Power > Health) Power = Health;
        }

        public void AddItem(InventoryItem item)
        {
            var existing = Inventory.FirstOrDefault(i => i.Name == item.Name);
            if (existing != null)
            {
                existing.Count += item.Count;
            }
            else
            {
                Inventory.Add(item);
            }
        }

        public T GetItem<T>(string name) where T : InventoryItem
        {
            return Inventory.FirstOrDefault(i => i.Name == name && i is T) as T;
        }

        public void SelectNextWeapon()
        {
            var weapons = Inventory.OfType<Weapon>().ToList();
            if (weapons.Count == 0) return;

            int currentIndex = weapons.IndexOf(CurrentWeapon);
            int nextIndex = (currentIndex + 1) % weapons.Count;
            CurrentWeapon = weapons[nextIndex];
        }
    }
}