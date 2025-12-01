// Version: 0.1
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

            for (int i = 0; i < Settings.Players.Count; i++)
            {
                var pSetup = Settings.Players[i];
                int x = 0;
                int y = 0;
                int attempts = 0;
                do
                {
                    x = 100 + (i * (Terrain.Width - 200) / (Settings.Players.Count > 1 ? Settings.Players.Count - 1 : 1));
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

                AddPlayer(new Tank(i, pSetup.Name, new Vector2(x, y), pSetup.Color, _tankBodyTexture, _tankBarrelTexture, pSetup.IsAI));
            }

            NextTurn();
        }

        public void StartNextRound()
        {
            CurrentRound++;
            Reset();

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                p.IsActive = true;
                p.Health = 100;
                p.Fuel = 100;

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
            Projectiles.Clear();
            IsGameOver = false;
            IsMatchOver = false;
            CurrentPlayerIndex = -1;
            Wind = 0;
            Terrain.Generate(System.Environment.TickCount);
        }

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

            if (_aiTimer < 1.0f) return;

            if (!_aiAiming)
            {
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
                    Vector2 diff = target.Position - activeTank.Position;
                    bool targetIsRight = target.Position.X > activeTank.Position.X;
                    _aiTargetAngle = targetIsRight ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4;

                    float g = MatchSettings.Gravity;
                    float range = Math.Abs(diff.X);
                    // R = v^2 / g => v = Sqrt(R*g)
                    // This assumes 45 degree angle for max range logic
                    float v = (float)Math.Sqrt(range * g);
                    _aiTargetPower = v / 10f; // Since speed = Power * 10f

                    System.Random rand = new System.Random();
                    _aiTargetAngle += (float)(rand.NextDouble() * 0.2 - 0.1);
                    _aiTargetPower += (float)(rand.NextDouble() * 10 - 5);

                    if (_aiTargetPower > activeTank.Health) _aiTargetPower = activeTank.Health;
                    if (_aiTargetPower < 0) _aiTargetPower = 0;

                    _aiAiming = true;
                }
            }
            else
            {
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
            System.Random rand = new System.Random();
            Wind = (float)(rand.NextDouble() * 20 - 10);

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

            if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Tab))
            {
                Players[CurrentPlayerIndex].SelectNextWeapon();
            }

            foreach (var player in Players)
            {
                player.Update(gameTime, Terrain);

                if (player.Position.Y > Terrain.Height || player.Position.Y > Terrain.WaterLevel)
                {
                    player.TakeDamage(1000);
                }

                if (!player.IsActive)
                {
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
                            lastSurvivor.Money += 500;
                        }
                        else
                        {
                            GameOverMessage = "Draw!";
                        }

                        foreach (var p in Players) p.Money += 100;

                        if (CurrentRound >= TotalRounds)
                        {
                            IsMatchOver = true;
                            GameOverMessage += "\nMATCH OVER!";
                        }
                    }
                }
            }

            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                // Use Global Gravity
                p.UpdatePhysics(gameTime, Wind, MatchSettings.Gravity);

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
                // Passing the font down
                player.Draw(spriteBatch, font);
            }
            foreach (var p in Projectiles)
            {
                // Passing the font down
                p.Draw(spriteBatch, font);
            }

            Terrain.DrawWater(spriteBatch);

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