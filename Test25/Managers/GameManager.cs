// Version: 0.7 (Refactored to Sub-Managers)

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Test25.Entities;
using Test25.World;
using Test25.Utilities;

namespace Test25.Managers
{
    public class GameManager
    {
        // Managers
        public ExplosionManager ExplosionManager { get; private set; }
        public DecorationManager DecorationManager { get; private set; }
        public AiManager AiManager { get; private set; }
        public ProjectileManager ProjectileManager { get; private set; }
        public TurnManager TurnManager { get; private set; }

        // State held by GameManager acting as coordinator
        public List<Tank> Players { get; private set; }
        public Terrain Terrain { get; private set; }

        // Pass-through properties for Game1 compatibility
        public int CurrentPlayerIndex => TurnManager.CurrentPlayerIndex;
        public float Wind => TurnManager.Wind;
        public bool IsGameOver => TurnManager.IsGameOver;
        public bool IsMatchOver => TurnManager.IsMatchOver;
        public string GameOverMessage => TurnManager.GameOverMessage;
        public int CurrentRound => TurnManager.CurrentRound;
        public int TotalRounds => TurnManager.TotalRounds;
        public MatchSettings Settings => TurnManager.Settings; // or store copy? TurnManager has it.

        public bool IsProjectileInAir => ProjectileManager.IsProjectileInAir;

        // Assets
        private Texture2D _projectileTexture;
        private Texture2D _tankBodyTexture;
        private Texture2D _tankBarrelTexture;

        private Tank _hoveredTank;

        private bool _turnInProgress;

        private Camera _camera;

        public GameManager(Terrain terrain, Texture2D projectileTexture, Texture2D tankBodyTexture,
            Texture2D tankBarrelTexture, List<Texture2D> decorationTextures, Camera camera)
        {
            Terrain = terrain;
            _projectileTexture = projectileTexture;
            _tankBodyTexture = tankBodyTexture;
            _tankBarrelTexture = tankBarrelTexture;
            _camera = camera;

            Players = new List<Tank>();

            // Initialize Sub-Managers
            ExplosionManager = new ExplosionManager(terrain.GraphicsDevice);
            DecorationManager = new DecorationManager(decorationTextures);
            AiManager = new AiManager();
            ProjectileManager = new ProjectileManager();
            TurnManager = new TurnManager();

            _turnInProgress = false;
        }

        public void AddPlayer(Tank tank)
        {
            Players.Add(tank);
        }

        // Keep Spawn Logic here as it relates to Terrain and Players setup
        private Vector2 FindSpawnPosition(int playerIndex, int totalPlayers)
        {
            int x;
            int y;
            int attempts = 0;
            do
            {
                // Distribute players evenly across the map width
                x = 100 + (playerIndex * (Terrain.Width - 200) / (totalPlayers > 1 ? totalPlayers - 1 : 1));

                // If starting in water, try to find land nearby
                if (Terrain.GetHeight(x) >= Terrain.WaterLevel)
                {
                    bool found = false;
                    for (int offset = 10; offset < 200; offset += 10)
                    {
                        if (x + offset < Terrain.Width && Terrain.GetHeight(x + offset) < Terrain.WaterLevel)
                        {
                            x += offset;
                            found = true;
                            break;
                        }

                        if (x - offset > 0 && Terrain.GetHeight(x - offset) < Terrain.WaterLevel)
                        {
                            x -= offset;
                            found = true;
                            break;
                        }
                    }

                    // If still no land, pick random spot
                    if (!found) x = 100 + Rng.Range(0, Terrain.Width - 200);
                }

                y = Terrain.GetHeight(x) - 10; // Spawn slightly above ground
                attempts++;
            } while (y >= Terrain.WaterLevel && attempts < 10); // Try to avoid spawning underwater

            return new Vector2(x, y);
        }

        public void StartGame(MatchSettings settings)
        {
            TurnManager.StartGame(settings);
            Players.Clear();
            Reset();

            for (int i = 0; i < settings.Players.Count; i++)
            {
                var pSetup = settings.Players[i];
                Vector2 spawnPos = FindSpawnPosition(i, settings.Players.Count);
                AddPlayer(new Tank(i, pSetup.Name, spawnPos, pSetup.Color, _tankBodyTexture, _tankBarrelTexture,
                    pSetup.IsAi, pSetup.Personality));
            }

            NextTurn();
        }

        public void StartNextRound()
        {
            TurnManager.StartNextRound(); // update counters
            Reset();

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                p.IsActive = true;
                p.Health = 100;
                p.Position = FindSpawnPosition(i, Players.Count);
            }

            NextTurn();
        }

        public void Reset()
        {
            ProjectileManager.Reset();
            ExplosionManager.Reset();
            AiManager.ResetTurn();
            // TurnManager reset is handled by StartGame/StartNextRound for game over flags

            Terrain.Generate(Environment.TickCount);
            DecorationManager.GenerateDecorations(Terrain);
            _turnInProgress = false;
        }

        public void UpdateAi(GameTime gameTime)
        {
            AiManager.UpdateAi(gameTime, Players, CurrentPlayerIndex, ProjectileManager.IsProjectileInAir,
                IsGameOver, TurnManager.Wind, TurnManager.Settings.WallType, Terrain.Width, Fire, Terrain);
        }

        public void NextTurn()
        {
            if (TurnManager.NextTurn(Players))
            {
                AiManager.ResetTurn();
                _turnInProgress = false;
            }
        }

        public void Fire()
        {
            if (IsProjectileInAir || IsGameOver) return;

            var activeTank = Players[CurrentPlayerIndex];
            if (!activeTank.IsActive) return; // Dead tanks can't shoot

            var projectile = activeTank.Fire(_projectileTexture);

            if (projectile is DroneProjectile drone)
            {
                drone.SetTargets(Players);
                drone.SetOwner(activeTank);
            }

            ProjectileManager.AddProjectile(projectile);
            _turnInProgress = true;
        }

        public void Update(GameTime gameTime)
        {
            Terrain.Update(gameTime);

            if (!IsGameOver)
            {
                if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Tab))
                {
                    Players[CurrentPlayerIndex].SelectNextWeapon();
                }

                // --- Update Players ---
                for (int i = 0; i < Players.Count; i++)
                {
                    var player = Players[i];
                    player.Update(gameTime, Terrain);

                    // Check for drowning (if center of tank is below water level)
                    float tankCenterY = player.Position.Y - (player.BoundingBox.Height / 2f);

                    if (player.Position.Y > Terrain.Height || tankCenterY > Terrain.WaterLevel)
                    {
                        if (player.IsActive)
                        {
                            // Large splash/destruction
                            Terrain.Destruct((int)player.Position.X, (int)player.Position.Y, 60);
                            // Instant kill with death sequence
                            if (player.TakeDamage(player.Health + 1000))
                            {
                                HandleTankDeath(player);
                            }
                        }
                    }
                }

                TurnManager.CheckWinCondition(Players); // Update game over state
            }

            // --- Update Projectiles ---
            ProjectileManager.Update(gameTime, this); // Passing 'this' because Projectile.OnHit needs it

            if (!IsGameOver && _turnInProgress && !IsProjectileInAir)
            {
                NextTurn();
            }

            // --- Update Explosions ---
            ExplosionManager.Update(gameTime);

            // --- Tooltip Check ---
            _hoveredTank = null;
            if (!IsGameOver)
            {
                var mousePos = InputManager.GetMousePosition();
                var mouseRect = new Rectangle(mousePos.X, mousePos.Y, 1, 1);

                foreach (var p in Players)
                {
                    if (p.IsActive && p.BoundingBox.Contains(mouseRect))
                    {
                        _hoveredTank = p;
                        break;
                    }
                }
            }
        }

        public void AddExplosion(Vector2 position, float radius, Color? color = null)
        {
            ExplosionManager.AddExplosion(position, radius, color);
        }

        public void HandleTankDeath(Tank tank, Tank killer = null)
        {
            if (killer != null && killer != tank)
            {
                killer.Kills++;
                killer.Money += Constants.KillReward;
            }

            // 1. Initial random explosion
            float baseExplosionRadius = Constants.DeathExplosionRadiusMin +
                                        Rng.Range(0f, Constants.DeathExplosionRadiusVariance);
            AddExplosion(tank.Position, baseExplosionRadius, Color.OrangeRed);
            Terrain.Destruct((int)tank.Position.X, (int)tank.Position.Y, (int)baseExplosionRadius);

            // Trigger Shake
            _camera.Shake(1.0f); // Max trauma

            // 2. Ammo Cook-off check
            if (Rng.Instance.NextDouble() < Constants.DeathCookOffChance)
            {
                // Cook-off triggered!
                int debrisCount = Rng.Range(Constants.DeathDebrisCountMin, Constants.DeathDebrisCountMax);

                for (int i = 0; i < debrisCount; i++)
                {
                    // Debris properties
                    float angle = (float)(Rng.Instance.NextDouble() * Math.PI * 2); // all angles
                    float speed = Constants.DeathDebrisSpeedMin + Rng.Range(0f, Constants.DeathDebrisSpeedVariance);
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                    // Explosion strength
                    float strength = Constants.DeathDebrisExplosionMin +
                                     Rng.Range(0f, Constants.DeathDebrisExplosionVariance);
                    float damage = strength; // Damage roughly equal to radius for now

                    DebrisProjectile debris =
                        new DebrisProjectile(tank.Position, velocity, _projectileTexture, damage, strength);
                    ProjectileManager.AddProjectile(debris);
                }
            }
        }

        /// <summary>
        /// Draws the game world (Terrain, Tanks, Projectiles, Water).
        /// Expected to be called within a SpriteBatch.Begin(transformMatrix: camera.GetViewMatrix()).
        /// </summary>
        public void DrawWorld(SpriteBatch spriteBatch, Matrix viewMatrix)
        {
            // 1. Draw Terrain 
            // NOTE: This call will interrupt the current SpriteBatch to draw 3D geometry
            Terrain.Draw(spriteBatch);

            // 2. Draw Entities (Tanks, Projectiles)
            // SpriteBatch is active again here
            foreach (var player in Players)
            {
                player.Draw(spriteBatch,
                    null); // Pass null font if simple draw, wait, Tank.Draw uses font? Yes, for dialogue. It's ok to pass font if we have it, but here we might not?
                // Actually Tank.Draw uses font? Checking.
                // Tank.Draw(SpriteBatch, SpriteFont)
            }

            // To fix the font issue, we can pass null or we need to pass font to DrawWorld.
            // Let's assume we pass font to DrawWorld. Wait, I didn't change the signature in the plan to include font for DrawWorld properly or I missed it.
            // Checking Tank.Draw... it draws dialogue strings. So we need the font.
        }

        public void DrawWorld(SpriteBatch spriteBatch, SpriteFont font, Matrix viewMatrix)
        {
            // 1. Draw Terrain 
            Terrain.Draw(spriteBatch);

            // 2. Draw Entities
            foreach (var player in Players)
            {
                player.Draw(spriteBatch, font);
            }

            ProjectileManager.Draw(spriteBatch, font);
            ExplosionManager.Draw(spriteBatch);

            // 3. Draw Water (Semi-transparent overlay) with transform
            Terrain.DrawWater(spriteBatch, viewMatrix);
        }

        public void DrawUI(SpriteBatch spriteBatch, SpriteFont font)
        {
            // 4. Draw UI
            if (IsGameOver)
            {
                spriteBatch.DrawString(font, GameOverMessage, new Vector2(300, 300), Color.Red);
            }
            else
            {
                var activeTank = Players[CurrentPlayerIndex];
                spriteBatch.DrawString(font, $"Player: {CurrentPlayerIndex + 1} ({activeTank.Color})",
                    new Vector2(10, 10), activeTank.Color);
                spriteBatch.DrawString(font, $"Health: {activeTank.Health}", new Vector2(10, 30), Color.White);
                spriteBatch.DrawString(font, $"Power: {(int)activeTank.Power}", new Vector2(10, 50), Color.White);
                spriteBatch.DrawString(font, $"Angle: {(int)MathHelper.ToDegrees(activeTank.TurretAngle)}",
                    new Vector2(10, 70), Color.White);
                spriteBatch.DrawString(font, $"Wind: {(int)Wind}", new Vector2(10, 90), Color.White);

                string weaponInfo = $"{activeTank.CurrentWeapon.Name}";
                if (!activeTank.CurrentWeapon.IsInfinite) weaponInfo += $" ({activeTank.CurrentWeapon.Count})";
                spriteBatch.DrawString(font, $"Weapon: {weaponInfo}", new Vector2(10, 110), Color.Yellow);

                spriteBatch.DrawString(font, $"Round: {CurrentRound}/{TotalRounds}", new Vector2(300, 10), Color.White);

                // Tooltip
                if (_hoveredTank != null)
                {
                    DrawTooltip(spriteBatch, font);
                }
            }
        }

        private void DrawTooltip(SpriteBatch spriteBatch, SpriteFont font)
        {
            var mousePos = InputManager.GetMousePosition().ToVector2();
            Vector2 tooltipPos = mousePos + new Vector2(15, 15);

            string text = $"Name: {_hoveredTank.Name}\n" +
                          $"HP: {_hoveredTank.Health}\n" +
                          $"Power: {_hoveredTank.Power}\n" +
                          $"Weapon: {_hoveredTank.CurrentWeapon.Name}\n" +
                          $"Money: ${_hoveredTank.Money}";

            if (_hoveredTank.IsAi && _hoveredTank.Personality != null)
            {
                text += $"\nPersona: {_hoveredTank.Personality.Name}";
                text += $"\nAimErr: {_hoveredTank.Personality.AimError}";
                text += $"\nPref: {_hoveredTank.Personality.WeaponPreference}";
            }

            Vector2 size = font.MeasureString(text);
            Rectangle bgRect = new Rectangle((int)tooltipPos.X - 5, (int)tooltipPos.Y - 5, (int)size.X + 10,
                (int)size.Y + 10);

            // Constraint Logic
            int screenWidth = Terrain.Width;
            int screenHeight = Terrain.Height;

            int maxX = Math.Max(0, screenWidth - bgRect.Width);
            int maxY = Math.Max(0, screenHeight - bgRect.Height);

            bgRect.X = MathHelper.Clamp(bgRect.X, 0, maxX);
            bgRect.Y = MathHelper.Clamp(bgRect.Y, 0, maxY);

            // Recalculate text position based on new background rect
            Vector2 textPos = new Vector2(bgRect.X + 5, bgRect.Y + 5);

            // Draw Background (using projectile texture stretched)
            spriteBatch.Draw(_projectileTexture, bgRect, new Color(0, 0, 0, 200));
            spriteBatch.DrawString(font, text, textPos, Color.White);
        }
    }
}