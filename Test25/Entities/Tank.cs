// Version: 0.4 (Fixed)

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
        public int PlayerIndex { get; }
        public string Name { get; private set; }
        public float Health { get; set; } = 100f;

        public float Power { get; set; } = 50f;
        public float TurretAngle { get; set; } = MathHelper.PiOver2;
        public Color Color { get; set; }
        public int Money { get; set; } = 0;
        public int Score { get; set; } = 0;
        public int Kills { get; set; } = 0;
        public float DamageMultiplier { get; set; } = 1.0f;

        public List<InventoryItem> Inventory { get; }
        public Weapon CurrentWeapon { get; private set; }

        public Rectangle BoundingBox => new Rectangle((int)(Position.X - _bodyTexture.Width / 2f),
            (int)(Position.Y - _bodyTexture.Height), _bodyTexture.Width, _bodyTexture.Height);

        private readonly Texture2D _bodyTexture;
        private readonly Texture2D _barrelTexture;
        private readonly Vector2 _turretOffset;

        public bool IsAi { get; set; }
        public AiPersonality Personality { get; set; }

        private float _dialogueTimer;
        private string _currentDialogue;
        private static Managers.DialogueManager _dialogueManager;

        public static void SetDialogueManager(Managers.DialogueManager manager) => _dialogueManager = manager;

        private void ShowDialogue(string text)
        {
            _currentDialogue = text;
            _dialogueTimer = 3f;
        }

        public Tank(int playerIndex, string name, Vector2 startPosition, Color color, Texture2D bodyTexture,
            Texture2D barrelTexture, bool isAi = false, AiPersonality personality = null)
        {
            PlayerIndex = playerIndex;
            Name = name;
            Position = startPosition;
            Color = color;
            _bodyTexture = bodyTexture;
            _barrelTexture = barrelTexture;
            IsAi = isAi;

            if (IsAi)
            {
                if (personality != null && personality.Name != "Random")
                {
                    Personality = personality;
                }
                else
                {
                    Personality = AiPersonality.GetRandom();
                }

                // optional: debug name
                // Name += $" ({Personality.Name})";
            }

            _turretOffset = new Vector2(0, -_bodyTexture.Height / 2f);

            Inventory = new List<InventoryItem>();
            var defaultWeapon = new Weapon("Standard Shell", "Basic projectile", 20f, 20f, 1, true);
            Inventory.Add(defaultWeapon);

            // 5 Big Bombs (Nuke)
            var nukeWeapon = new Weapon("Nuke", "Massive damage area", 60f, 80f, 5, false);
            Inventory.Add(nukeWeapon);

            // 1 Parachute
            var parachute = new Item("Parachute", "Prevents fall damage", ItemType.Passive, null, 1);
            Inventory.Add(parachute);

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
            if (!IsActive) return;
            Update(gameTime);
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float gravity = Constants.Gravity;

            var vel = Velocity;
            vel.Y += gravity * deltaTime;
            Velocity = vel;
            Position += Velocity * deltaTime;

            // Collision Check (Pixel Perfect)
            if (Position.X >= 0 && Position.X < terrain.Width)
            {
                // We want to stand ON the pixel, not IN it.
                // Position is bottom-center. 
                // We check a point slightly UP (e.g. 1 pixel or epsilon) to see if we are embedded.
                int checkX = (int)Position.X;
                int checkY = (int)(Position.Y - 0.5f); // Check slighty inside the body

                if (terrain.IsPixelSolid(checkX, checkY))
                {
                    // Fall damage check using Constants happens only if we were falling fast
                    if (vel.Y > Constants.FallDamageThreshold)
                    {
                        var parachute = GetItem<InventoryItem>("Parachute");
                        if (parachute is { Count: > 0 })
                        {
                            parachute.Count--;
                            if (parachute.Count <= 0 && !parachute.IsInfinite) Inventory.Remove(parachute);
                        }
                        else
                        {
                            float damage = (vel.Y - Constants.FallDamageThreshold) * Constants.FallDamageMultiplier;
                            TakeDamage(damage);
                        }
                    }

                    // Move up until not solid (simple resolution)
                    for (int i = 0; i < 20; i++)
                    {
                        // Check upward
                        if (!terrain.IsPixelSolid(checkX, checkY - i))
                        {
                            // Found air!
                            // Snap to the boundary. 
                            // If pixel (y-i) is air, we want to be at bottom of (y-i), which is (y-i)+1? 
                            // No, if pixel Y is solid, and Y-1 is air. We want Pos.Y = Y.0.
                            // If checkY-i is air. That implies pixel `checkY-i` is empty.
                            // So we can stand at `checkY-i + 1`.
                            Position = new Vector2(Position.X, checkY - i + 1);
                            Velocity = new Vector2(Velocity.X, 0); // Stop falling
                            break;
                        }
                    }
                }

                // Grounding Check (Stop micro-gravity accumulation)
                // If the pixel immediately below us is solid, and we are not moving up...
                // Check pixel at (int)(Position.Y + 0.5f)
                if (Velocity.Y >= 0) // Only if not jumping
                {
                    if (terrain.IsPixelSolid(checkX, (int)(Position.Y + 0.5f)))
                    {
                        Velocity = new Vector2(Velocity.X, 0);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (IsActive && _bodyTexture != null && _barrelTexture != null)
            {
                Vector2 barrelOrigin = new Vector2(0, _barrelTexture.Height / 2f);
                Vector2 barrelPosition = Position + _turretOffset;
                spriteBatch.Draw(_barrelTexture, barrelPosition, null, Color, -TurretAngle, barrelOrigin, 1f,
                    SpriteEffects.None, 0f);

                Vector2 bodyOrigin = new Vector2(_bodyTexture.Width / 2f, _bodyTexture.Height);
                spriteBatch.Draw(_bodyTexture, Position, null, Color, 0f, bodyOrigin, 1f, SpriteEffects.None, 0f);
            }

            if (IsActive && !string.IsNullOrEmpty(_currentDialogue))
            {
                if (_bodyTexture != null)
                {
                    var pos = Position - new Vector2(0, _bodyTexture.Height + 20);
                    float alpha = 1f;
                    if (_dialogueTimer < 1f) alpha = _dialogueTimer;

                    spriteBatch.DrawString(font, _currentDialogue, pos, Color.Yellow * alpha);
                }
            }
        }

        public Projectile Fire(Texture2D projectileTexture)
        {
            Vector2 barrelPosition = Position + _turretOffset;
            float barrelLength = _barrelTexture.Width;
            Vector2 offset = new Vector2((float)Math.Cos(-TurretAngle), (float)Math.Sin(-TurretAngle)) * barrelLength;
            Vector2 spawnPos = barrelPosition + offset;

            // Using Constant for Power Multiplier
            // Fixed: Removed Max Power override for Laser
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

            Managers.SoundManager.PlaySound("fire");

            Projectile p;
            switch (CurrentWeapon.Type)
            {
                case ProjectileType.Mirv:
                    p = new MirvProjectile(spawnPos, velocity, projectileTexture, CurrentWeapon.SplitCount);
                    break;
                case ProjectileType.Dirt:
                    p = new DirtProjectile(spawnPos, velocity, projectileTexture);
                    break;
                case ProjectileType.Roller:
                    p = new RollerProjectile(spawnPos, velocity, projectileTexture);
                    break;
                case ProjectileType.Laser:
                    p = new LaserProjectile(spawnPos, velocity, projectileTexture);
                    break;
                case ProjectileType.Drone:
                    p = new DroneProjectile(spawnPos, velocity, projectileTexture);
                    break;
                case ProjectileType.Standard:
                default:
                    p = new ExplosiveProjectile(spawnPos, velocity, projectileTexture);
                    break;
            }

            p.Owner = this;

            p.Damage = damage;
            p.ExplosionRadius = radius;
            return p;
        }

        public bool TakeDamage(float amount)
        {
            if (!IsActive) return false;

            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                IsActive = false;
                _currentDialogue = null; // Clear any active dialogue
                return true; // Tank died
            }

            if (Power > Health) Power = Health;
            return false; // Tank survived
        }

        private float _accumulatedAngleChange;

        public void AdjustAim(float delta)
        {
            float oldAngle = TurretAngle;
            TurretAngle = MathHelper.Clamp(TurretAngle + delta, 0, MathHelper.Pi);

            float change = Math.Abs(TurretAngle - oldAngle);
            if (change > 0)
            {
                _accumulatedAngleChange += change;
                float threshold = MathHelper.ToRadians(1); // 1 degree

                while (_accumulatedAngleChange >= threshold)
                {
                    _accumulatedAngleChange -= threshold;
                    Managers.SoundManager.PlaySound("angle_tick");
                }
            }
        }

        public void AdjustPower(float delta)
        {
            Power = MathHelper.Clamp(Power + delta, 0, Health);
        }

        public void AddItem(InventoryItem item)
        {
            var existing = GetItem<InventoryItem>(item.Name);
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

        public void SetWeapon(Weapon weapon)
        {
            if (Inventory.Contains(weapon))
            {
                CurrentWeapon = weapon;
            }
        }
    }
}