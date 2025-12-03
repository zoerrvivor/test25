// Version: 0.5 (Fixed & Refactored)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Entities;
using Test25.World;
using System;

namespace Test25.Managers
{
    public class GameManager(Terrain terrain, Texture2D projectileTexture, Texture2D tankBodyTexture, Texture2D tankBarrelTexture)
    {
        public List<Tank> Players { get; private set; } = [];
        public int CurrentPlayerIndex { get; private set; }
        public float Wind { get; private set; }
        public Terrain Terrain { get; private set; } = terrain;

        public List<Projectile> Projectiles { get; private set; } = [];
        public bool IsProjectileInAir => Projectiles.Count > 0;
        private readonly Texture2D _projectileTexture = projectileTexture;
        private readonly Texture2D _tankBodyTexture = tankBodyTexture;
        private readonly Texture2D _tankBarrelTexture = tankBarrelTexture;

        public MatchSettings Settings { get; private set; }

        public bool IsGameOver { get; private set; }
        public bool IsMatchOver { get; private set; }
        public string GameOverMessage { get; private set; }
        public int CurrentRound { get; private set; }
        public int TotalRounds { get; private set; }

        private bool _turnInProgress;

        public void AddPlayer(Tank tank)
        {
            Players.Add(tank);
        }

        private Vector2 FindSpawnPosition(int playerIndex, int totalPlayers)
        {
            int attempts = 0;
            int x;
            int y;
            do
            {
                x = 100 + (playerIndex * (Terrain.Width - 200) / (totalPlayers > 1 ? totalPlayers - 1 : 1));

                if (Terrain.GetHeight(x) >= Terrain.WaterLevel)
                {
                    bool found = false;
                    for (int offset = 10; offset < 200; offset += 10)
                    {
                        if (x + offset < Terrain.Width && Terrain.GetHeight(x + offset) < Terrain.WaterLevel) { x += offset; found = true; break; }
                        if (x - offset > 0 && Terrain.GetHeight(x - offset) < Terrain.WaterLevel) { x -= offset; found = true; break; }
                    }
                    if (!found)
                    {
                        x = 100 + new Random().Next(Terrain.Width - 200);
                    }
                }

                y = Terrain.GetHeight(x) - 10;
                attempts++;
            } while (y >= Terrain.WaterLevel && attempts < 10);

            return new Vector2(x, y);
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
                PlayerSetup pSetup = Settings.Players[i];
                Vector2 spawnPos = FindSpawnPosition(i, Settings.Players.Count);
                AddPlayer(new Tank(i, pSetup.Name, spawnPos, pSetup.Color, _tankBodyTexture, _tankBarrelTexture, pSetup.IsAI));
            }

            NextTurn();
        }

        public void StartNextRound()
        {
            CurrentRound++;
            Reset();

            for (int i = 0; i < Players.Count; i++)
            {
                Tank p = Players[i];
                p.IsActive = true;
                p.Health = 100;

                p.Position = FindSpawnPosition(i, Players.Count);
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
            Terrain.Generate(Environment.TickCount);
            _turnInProgress = false;
        }

        private float _aiTimer;
        private bool _aiHasFired;
        private bool _aiAiming;
        private float _aiTargetAngle;
        private float _aiTargetPower;

        public void UpdateAI(GameTime gameTime)
        {
            if (IsProjectileInAir || IsGameOver)
            {
                return;
            }

            Tank activeTank = Players[CurrentPlayerIndex];
            _aiTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_aiTimer < 1.0f)
            {
                return;
            }

            if (!_aiAiming)
            {
                Tank target = null;
                float minDist = float.MaxValue;
                foreach (Tank p in Players)
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

                    float g = Constants.Gravity;
                    float range = Math.Abs(diff.X);
                    // R = v^2 / g => v = Sqrt(R*g)
                    float v = (float)Math.Sqrt(range * g);

                    _aiTargetPower = v / Constants.PowerMultiplier;

                    Random rand = new();
                    _aiTargetAngle += (float)((rand.NextDouble() * 0.2) - 0.1);
                    _aiTargetPower += (float)((rand.NextDouble() * 10) - 5);

                    if (_aiTargetPower > activeTank.Health)
                    {
                        _aiTargetPower = activeTank.Health;
                    }

                    if (_aiTargetPower < 0)
                    {
                        _aiTargetPower = 0;
                    }

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
                    if (activeTank.TurretAngle < _aiTargetAngle)
                    {
                        activeTank.TurretAngle += aimSpeed;
                    }
                    else
                    {
                        activeTank.TurretAngle -= aimSpeed;
                    }
                }

                if (Math.Abs(activeTank.Power - _aiTargetPower) < powerSpeed)
                {
                    activeTank.Power = _aiTargetPower;
                    powered = true;
                }
                else
                {
                    if (activeTank.Power < _aiTargetPower)
                    {
                        activeTank.Power += powerSpeed;
                    }
                    else
                    {
                        activeTank.Power -= powerSpeed;
                    }
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
            Random rand = new();
            Wind = (float)((rand.NextDouble() * 20) - 10);

            _aiTimer = 0;
            _aiHasFired = false;
            _aiAiming = false;
            _turnInProgress = false;
        }

        public void Fire()
        {
            if (IsProjectileInAir || IsGameOver)
            {
                return;
            }

            Tank activeTank = Players[CurrentPlayerIndex];
            Projectile projectile = activeTank.Fire(_projectileTexture);
            Projectiles.Add(projectile);
            _turnInProgress = true;
        }

        public void Update(GameTime gameTime)
        {
            Terrain.Update(gameTime);

            if (IsGameOver)
            {
                return;
            }

            if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Tab))
            {
                Players[CurrentPlayerIndex].SelectNextWeapon();
            }

            // PERFORMANCE: For-Schleife statt Foreach
            for (int i = 0; i < Players.Count; i++)
            {
                Tank player = Players[i];
                player.Update(gameTime, Terrain);

                // Check for drowning (mostly submerged)
                // Position is bottom center. Center is Position.Y - Height/2.
                float tankCenterY = player.Position.Y - (player.BoundingBox.Height / 2f);

                if (player.Position.Y > Terrain.Height || tankCenterY > Terrain.WaterLevel)
                {
                    if (player.IsActive)
                    {
                        // Trigger large explosion (triple radius, approx 60)
                        Terrain.Destruct((int)player.Position.X, (int)player.Position.Y, 60);

                        // Kill tank immediately
                        player.TakeDamage(player.Health + 1000);
                    }
                }
            }

            CheckWinCondition();

            // PERFORMANCE: Rückwärts-Loop für Entfernung ist sicher und effizient
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                Projectile p = Projectiles[i];
                p.UpdatePhysics(gameTime, Wind, Constants.Gravity);

                // Handle MIRV splitting
                if (p is MirvProjectile mirv)
                {
                    if (mirv.NewProjectiles.Count > 0)
                    {
                        Projectiles.AddRange(mirv.NewProjectiles);
                        mirv.NewProjectiles.Clear();
                    }
                }

                if (p.IsDead)
                {
                    Projectiles.RemoveAt(i);
                    continue;
                }

                // Wall Types Logic
                if (Settings.WallType == WallType.Wrap)
                {
                    if (p.Position.X < 0)
                    {
                        p.Position = new Vector2(Terrain.Width - 1, p.Position.Y);
                    }
                    else if (p.Position.X >= Terrain.Width)
                    {
                        p.Position = new Vector2(0, p.Position.Y);
                    }
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

                // Kollision mit Spielern
                for (int j = 0; j < Players.Count; j++)
                {
                    Tank player = Players[j];
                    if (!player.IsActive)
                    {
                        continue;
                    }

                    // PERFORMANCE: Schneller BoundingBox Check vor genauer Prüfung
                    if (!player.BoundingBox.Contains(p.Position))
                    {
                        continue;
                    }
                    hit = true;
                    break;
                }

                // Kollision mit Terrain
                if (!hit)
                {
                    // PERFORMANCE: Kollision nur prüfen, wenn Projektil im Bild ist
                    if (p.Position.Y >= 0 && p.CheckCollision(Terrain, Settings.WallType))
                    {
                        hit = true;
                    }
                }

                if (hit)
                {
                    p.OnHit(Terrain, Players);
                    if (p.IsDead)
                    {
                        Projectiles.RemoveAt(i);
                    }
                }
            }

            if (_turnInProgress && Projectiles.Count == 0)
            {
                NextTurn();
            }
        }

        private void CheckWinCondition()
        {
            if (IsGameOver)
            {
                return;
            }

            int activeCount = 0;
            Tank lastSurvivor = null;
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].IsActive)
                {
                    activeCount++;
                    lastSurvivor = Players[i];
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

                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].Money += 100;
                }

                if (CurrentRound >= TotalRounds)
                {
                    IsMatchOver = true;
                    GameOverMessage += "\nMATCH OVER!";
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            Terrain.Draw(spriteBatch);
            foreach (Tank player in Players)
            {
                player.Draw(spriteBatch, font);
            }
            foreach (Projectile p in Projectiles)
            {
                p.Draw(spriteBatch, font);
            }

            Terrain.DrawWater(spriteBatch);

            if (IsGameOver)
            {
                spriteBatch.DrawString(font, GameOverMessage, new Vector2(300, 300), Color.Red);
            }
            else
            {
                Tank activeTank = Players[CurrentPlayerIndex];
                spriteBatch.DrawString(font, $"Player: {CurrentPlayerIndex + 1} ({activeTank.Color})", new Vector2(10, 10), activeTank.Color);
                spriteBatch.DrawString(font, $"Health: {activeTank.Health}", new Vector2(10, 30), Color.White);
                spriteBatch.DrawString(font, $"Power: {(int)activeTank.Power}", new Vector2(10, 50), Color.White);
                spriteBatch.DrawString(font, $"Angle: {(int)MathHelper.ToDegrees(activeTank.TurretAngle)}", new Vector2(10, 70), Color.White);
                spriteBatch.DrawString(font, $"Wind: {(int)Wind}", new Vector2(10, 90), Color.White);

                string weaponInfo = $"{activeTank.CurrentWeapon.Name}";
                if (!activeTank.CurrentWeapon.IsInfinite)
                {
                    weaponInfo += $" ({activeTank.CurrentWeapon.Count})";
                }

                spriteBatch.DrawString(font, $"Weapon: {weaponInfo}", new Vector2(10, 110), Color.Yellow);

                spriteBatch.DrawString(font, $"Round: {CurrentRound}/{TotalRounds}", new Vector2(300, 10), Color.White);
            }
        }
    }
}