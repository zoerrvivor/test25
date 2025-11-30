using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Entities;
using Test25.World;
using System;

namespace Test25.Managers
{
    public class GameManager
    {
        public List<Tank> Players { get; private set; }
        public int CurrentPlayerIndex { get; private set; }
        public float Wind { get; private set; }
        public Terrain Terrain { get; private set; }

        public List<Projectile> Projectiles { get; private set; }
        public bool IsProjectileInAir => Projectiles.Count > 0;
        private Texture2D _projectileTexture;
        private Texture2D _tankBodyTexture;
        private Texture2D _tankBarrelTexture;

        public MatchSettings Settings { get; private set; }

        public bool IsGameOver { get; private set; }
        public bool IsMatchOver { get; private set; }
        public string GameOverMessage { get; private set; }
        public int CurrentRound { get; private set; }
        public int TotalRounds { get; private set; }

        public GameManager(Terrain terrain, Texture2D projectileTexture, Texture2D tankBodyTexture, Texture2D tankBarrelTexture)
        {
            Terrain = terrain;
            _projectileTexture = projectileTexture;
            _tankBodyTexture = tankBodyTexture;
            _tankBarrelTexture = tankBarrelTexture;
            Players = new List<Tank>();
            Projectiles = new List<Projectile>();
            CurrentPlayerIndex = 0;
            Wind = 0;
        }

        public void AddPlayer(Tank tank)
        {
            Players.Add(tank);
        }

        public void StartGame(MatchSettings settings)
        {
            Settings = settings;
            TotalRounds = settings.NumRounds;
            CurrentRound = 1;
            Players.Clear();
            Reset();

            // Add Players from Settings
            for (int i = 0; i < Settings.Players.Count; i++)
            {
                var pSetup = Settings.Players[i];
                // Randomize X position
                int x = 0;
                int y = 0;
                int attempts = 0;
                do
                {
                    x = 100 + (i * (Terrain.Width - 200) / (Settings.Players.Count > 1 ? Settings.Players.Count - 1 : 1));
                    // Add some random jitter if retrying or just generally? Let's keep it simple first.
                    // If we need to retry, we should pick a new random spot? 
                    // The original logic was deterministic based on index.
                    // Let's try to find a safe spot near the target x.
                    if (Terrain.GetHeight(x) >= Terrain.WaterLevel)
                    {
                        // Underwater, try to find a better spot
                        // Simple search: look left/right
                        bool found = false;
                        for (int offset = 10; offset < 200; offset += 10)
                        {
                            if (x + offset < Terrain.Width && Terrain.GetHeight(x + offset) < Terrain.WaterLevel) { x += offset; found = true; break; }
                            if (x - offset > 0 && Terrain.GetHeight(x - offset) < Terrain.WaterLevel) { x -= offset; found = true; break; }
                        }
                        if (!found) x = 100 + new System.Random().Next(Terrain.Width - 200); // Fallback to random
                    }

                    y = Terrain.GetHeight(x) - 10;
                    attempts++;
                } while (y >= Terrain.WaterLevel && attempts < 10); // Safety break

                AddPlayer(new Tank(i, pSetup.Name, new Vector2(x, y), pSetup.Color, _tankBodyTexture, _tankBarrelTexture, pSetup.IsAI));
            }

            // Initialize game state
            NextTurn();
        }

        public void StartNextRound()
        {
            CurrentRound++;
            Reset();

            // Re-position existing players
            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                p.IsActive = true;
                p.Health = (p.Health <= 0) ? 100 : p.Health; // Revive dead players with full health, others keep health? Or full reset? Let's do full reset for fairness but keep upgrades.
                // Actually, standard worms style: Dead players revive with base health, survivors keep health? 
                // Let's just revive everyone to 100 for now to keep it simple, but they keep items/money.
                p.Health = 100;
                p.Fuel = 100;

                // Randomize X position
                int x = 0;
                int y = 0;
                int attempts = 0;
                do
                {
                    x = 100 + (i * (Terrain.Width - 200) / (Players.Count > 1 ? Players.Count - 1 : 1));
                    if (Terrain.GetHeight(x) >= Terrain.WaterLevel)
                    {
                        bool found = false;
                        for (int offset = 10; offset < 200; offset += 10)
                        {
                            if (x + offset < Terrain.Width && Terrain.GetHeight(x + offset) < Terrain.WaterLevel) { x += offset; found = true; break; }
                            if (x - offset > 0 && Terrain.GetHeight(x - offset) < Terrain.WaterLevel) { x -= offset; found = true; break; }
                        }
                        if (!found) x = 100 + new System.Random().Next(Terrain.Width - 200);
                    }
                    y = Terrain.GetHeight(x) - 10;
                    attempts++;
                } while (y >= Terrain.WaterLevel && attempts < 10);

                p.Position = new Vector2(x, y);
            }
            NextTurn();
        }

        public void Reset()
        {
            // Don't clear players list, just reset state if needed. 
            // Actually StartGame calls Reset then adds players. StartNextRound calls Reset then repositions.
            // We need to differentiate.
            // Let's make Reset ONLY clear projectiles and terrain.
            Projectiles.Clear();
            //Projectiles.Clear();
            IsGameOver = false;
            IsMatchOver = false;
            CurrentPlayerIndex = -1;
            Wind = 0;
            Terrain.Generate(System.Environment.TickCount);
        }

        // AI State
        private float _aiTimer = 0f;
        private bool _aiHasFired = false;
        private bool _aiAiming = false;
        private float _aiTargetAngle = 0f;
        private float _aiTargetPower = 0f;

        public void UpdateAI(GameTime gameTime)
        {
            if (IsProjectileInAir || IsGameOver) return;

            var activeTank = Players[CurrentPlayerIndex];
            _aiTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Wait a bit before doing anything
            if (_aiTimer < 1.0f) return;

            if (!_aiAiming)
            {
                // Find nearest enemy
                Tank target = null;
                float minDist = float.MaxValue;
                foreach (var p in Players)
                {
                    if (p != activeTank && p.IsActive)
                    {
                        float dist = Vector2.Distance(activeTank.Position, p.Position);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            target = p;
                        }
                    }
                }

                if (target != null)
                {
                    // Calculate angle and power
                    // Simple AI: Aim directly at target with some randomness
                    Vector2 diff = target.Position - activeTank.Position;
                    float angle = (float)Math.Atan2(-diff.Y, diff.X); // Invert Y because screen Y is down

                    // Clamp angle to 0-Pi (upwards)
                    if (angle < 0) angle += MathHelper.TwoPi;
                    if (angle > MathHelper.Pi) angle = MathHelper.Pi; // Don't aim down
                    if (angle < 0) angle = 0;

                    // Adjust for wind? Maybe later.
                    // Adjust for gravity? 
                    // Simple ballistic approximation: 
                    // We need to hit (dx, dy). 
                    // v^2 * sin(2*theta) / g = range (on flat ground)
                    // Let's just pick a random high angle and calculate power, or pick max power and calculate angle.

                    // Let's try a fixed power strategy for now, or random.
                    // Let's just aim 45 degrees towards them and adjust power?
                    // Or just aim directly at them (which won't work with gravity).

                    // Let's use a simple heuristic:
                    // Aim 45 degrees (Pi/4) or 135 degrees (3*Pi/4) depending on direction.
                    bool targetIsRight = target.Position.X > activeTank.Position.X;
                    _aiTargetAngle = targetIsRight ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4;

                    // Calculate required velocity for 45 degree launch?
                    // Range R = v^2 / g
                    // v = Sqrt(R * g)
                    // Power = v / 10f (since speed = Power * 10f)
                    float g = 98f; // Gravity used in projectile update
                    float range = Math.Abs(diff.X);
                    float v = (float)Math.Sqrt(range * g);
                    _aiTargetPower = v / 10f;

                    // Add some randomness
                    System.Random rand = new System.Random();
                    _aiTargetAngle += (float)(rand.NextDouble() * 0.2 - 0.1);
                    _aiTargetPower += (float)(rand.NextDouble() * 10 - 5);

                    // Clamp
                    if (_aiTargetPower > activeTank.Health) _aiTargetPower = activeTank.Health; // Cap at max power (health)
                    if (_aiTargetPower < 0) _aiTargetPower = 0;

                    _aiAiming = true;
                }
            }
            else
            {
                // Move aim and power towards target
                float aimSpeed = 2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float powerSpeed = 50f * (float)gameTime.ElapsedGameTime.TotalSeconds;

                bool aimed = false;
                bool powered = false;

                if (Math.Abs(activeTank.TurretAngle - _aiTargetAngle) < aimSpeed)
                {
                    activeTank.TurretAngle = _aiTargetAngle;
                    aimed = true;
                }
                else
                {
                    if (activeTank.TurretAngle < _aiTargetAngle) activeTank.TurretAngle += aimSpeed;
                    else activeTank.TurretAngle -= aimSpeed;
                }

                if (Math.Abs(activeTank.Power - _aiTargetPower) < powerSpeed)
                {
                    activeTank.Power = _aiTargetPower;
                    powered = true;
                }
                else
                {
                    if (activeTank.Power < _aiTargetPower) activeTank.Power += powerSpeed;
                    else activeTank.Power -= powerSpeed;
                }

                if (aimed && powered && !_aiHasFired)
                {
                    Fire();
                    _aiHasFired = true;
                }
            }
        }

        public void NextTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            // Randomize wind
            System.Random rand = new System.Random();
            Wind = (float)(rand.NextDouble() * 20 - 10);

            // Reset AI state
            _aiTimer = 0;
            _aiHasFired = false;
            _aiAiming = false;
        }

        public void Fire()
        {
            if (IsProjectileInAir || IsGameOver) return;

            var activeTank = Players[CurrentPlayerIndex];
            var projectile = activeTank.Fire(_projectileTexture);
            Projectiles.Add(projectile);
        }

        public void Update(GameTime gameTime)
        {
            Terrain.Update(gameTime);

            if (IsGameOver) return;

            // Weapon Switching
            if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Tab))
            {
                Players[CurrentPlayerIndex].SelectNextWeapon();
            }

            foreach (var player in Players)
            {
                player.Update(gameTime, Terrain);

                // Check if tank fell off world
                // Check if tank fell off world OR into water
                if (player.Position.Y > Terrain.Height || player.Position.Y > Terrain.WaterLevel)
                {
                    player.TakeDamage(1000); // Instant kill
                }

                if (!player.IsActive)
                {
                    // Check if only one player left
                    int activeCount = 0;
                    Tank lastSurvivor = null;
                    foreach (var p in Players)
                    {
                        if (p.IsActive)
                        {
                            activeCount++;
                            lastSurvivor = p;
                        }
                    }

                    if (activeCount <= 1)
                    {
                        IsGameOver = true;
                        if (lastSurvivor != null)
                        {
                            GameOverMessage = $"{lastSurvivor.Name} Wins Round {CurrentRound}!";
                            lastSurvivor.Score++;
                            lastSurvivor.Money += 500; // Winner bonus
                        }
                        else
                        {
                            GameOverMessage = "Draw!";
                        }

                        // Participation award
                        foreach (var p in Players) p.Money += 100;

                        if (CurrentRound >= TotalRounds)
                        {
                            IsMatchOver = true;
                            GameOverMessage += "\nMATCH OVER!";
                        }
                    }
                }
            }

            // Update Projectiles
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                p.UpdatePhysics(gameTime, Wind, 98f); // Gravity 98

                // Handle Wall Physics
                if (Settings.WallType == WallType.Wrap)
                {
                    if (p.Position.X < 0) p.Position = new Vector2(Terrain.Width - 1, p.Position.Y);
                    else if (p.Position.X >= Terrain.Width) p.Position = new Vector2(0, p.Position.Y);
                }
                else if (Settings.WallType == WallType.Rubber)
                {
                    if (p.Position.X < 0)
                    {
                        p.Position = new Vector2(0, p.Position.Y);
                        p.Velocity = new Vector2(-p.Velocity.X, p.Velocity.Y);
                    }
                    else if (p.Position.X >= Terrain.Width)
                    {
                        p.Position = new Vector2(Terrain.Width - 1, p.Position.Y);
                        p.Velocity = new Vector2(-p.Velocity.X, p.Velocity.Y);
                    }
                }

                bool hit = false;

                // Check Tank Collision
                foreach (var player in Players)
                {
                    if (player.IsActive && player.BoundingBox.Contains(p.Position))
                    {
                        player.TakeDamage(p.Damage);
                        hit = true;
                        break;
                    }
                }

                if (!hit && p.CheckCollision(Terrain, Settings.WallType))
                {
                    hit = true;
                }

                if (hit)
                {
                    Terrain.Destruct((int)p.Position.X, (int)p.Position.Y, (int)p.ExplosionRadius);
                    Projectiles.RemoveAt(i);
                    NextTurn();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            Terrain.Draw(spriteBatch);
            foreach (var player in Players)
            {
                player.Draw(spriteBatch);
            }
            foreach (var p in Projectiles)
            {
                p.Draw(spriteBatch);
            }

            Terrain.DrawWater(spriteBatch);

            // HUD
            if (IsGameOver)
            {
                spriteBatch.DrawString(font, GameOverMessage, new Vector2(300, 300), Color.Red);
            }
            else
            {
                var activeTank = Players[CurrentPlayerIndex];
                spriteBatch.DrawString(font, $"Player: {CurrentPlayerIndex + 1} ({activeTank.Color})", new Vector2(10, 10), activeTank.Color);
                spriteBatch.DrawString(font, $"Health: {activeTank.Health}", new Vector2(10, 30), Color.White);
                spriteBatch.DrawString(font, $"Power: {(int)activeTank.Power}", new Vector2(10, 50), Color.White);
                spriteBatch.DrawString(font, $"Angle: {(int)MathHelper.ToDegrees(activeTank.TurretAngle)}", new Vector2(10, 70), Color.White);
                spriteBatch.DrawString(font, $"Wind: {(int)Wind}", new Vector2(10, 90), Color.White);

                string weaponInfo = $"{activeTank.CurrentWeapon.Name}";
                if (!activeTank.CurrentWeapon.IsInfinite) weaponInfo += $" ({activeTank.CurrentWeapon.Count})";
                spriteBatch.DrawString(font, $"Weapon: {weaponInfo}", new Vector2(10, 110), Color.Yellow);

                spriteBatch.DrawString(font, $"Round: {CurrentRound}/{TotalRounds}", new Vector2(300, 10), Color.White);
            }
        }
    }
}
