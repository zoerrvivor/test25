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
        public AIManager AIManager { get; private set; }
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

        private bool _turnInProgress;

        public GameManager(Terrain terrain, Texture2D projectileTexture, Texture2D tankBodyTexture,
            Texture2D tankBarrelTexture, List<Texture2D> decorationTextures)
        {
            Terrain = terrain;
            _projectileTexture = projectileTexture;
            _tankBodyTexture = tankBodyTexture;
            _tankBarrelTexture = tankBarrelTexture;

            Players = new List<Tank>();

            // Initialize Sub-Managers
            ExplosionManager = new ExplosionManager(terrain.GraphicsDevice);
            DecorationManager = new DecorationManager(decorationTextures);
            AIManager = new AIManager();
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
                    pSetup.IsAI));
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
            AIManager.ResetTurn();
            // TurnManager reset is handled by StartGame/StartNextRound for game over flags

            Terrain.Generate(Environment.TickCount);
            DecorationManager.GenerateDecorations(Terrain);
            _turnInProgress = false;
        }

        public void UpdateAi(GameTime gameTime)
        {
            AIManager.UpdateAi(gameTime, Players, CurrentPlayerIndex, IsProjectileInAir, IsGameOver, Fire);
        }

        public void NextTurn()
        {
            if (TurnManager.NextTurn(Players))
            {
                AIManager.ResetTurn();
                _turnInProgress = false;
            }
        }

        public void Fire()
        {
            if (IsProjectileInAir || IsGameOver) return;

            var activeTank = Players[CurrentPlayerIndex];
            if (!activeTank.IsActive) return; // Dead tanks can't shoot

            var projectile = activeTank.Fire(_projectileTexture);
            ProjectileManager.AddProjectile(projectile);
            _turnInProgress = true;
        }

        public void Update(GameTime gameTime)
        {
            Terrain.Update(gameTime);

            if (IsGameOver) return;

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

            // --- Update Projectiles ---
            ProjectileManager.Update(gameTime, this); // Passing 'this' because Projectile.OnHit needs it

            if (_turnInProgress && !IsProjectileInAir)
            {
                NextTurn();
            }

            // --- Update Explosions ---
            ExplosionManager.Update(gameTime);
        }

        public void AddExplosion(Vector2 position, float radius, Color? color = null)
        {
            ExplosionManager.AddExplosion(position, radius, color);
        }

        public void HandleTankDeath(Tank tank)
        {
            // 1. Initial random explosion
            float baseExplosionRadius = 40f + Rng.Range(0f, 60f);
            AddExplosion(tank.Position, baseExplosionRadius, Color.OrangeRed);
            Terrain.Destruct((int)tank.Position.X, (int)tank.Position.Y, (int)baseExplosionRadius);

            // 2. Ammo Cook-off check (20% chance)
            if (Rng.Instance.NextDouble() < 0.20)
            {
                // Cook-off triggered!
                int debrisCount = Rng.Range(5, 7); // 5 or 6

                for (int i = 0; i < debrisCount; i++)
                {
                    // Debris properties
                    float angle = (float)(Rng.Instance.NextDouble() * Math.PI * 2); // all angles
                    float speed = 200f + Rng.Range(0f, 300f); // Random speed
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                    // Explosion strength: Small - Medium (Normal projectile to Small Nuke)
                    float strength = 20f + Rng.Range(0f, 40f);
                    float damage = strength; // Damage roughly equal to radius for now

                    DebrisProjectile debris =
                        new DebrisProjectile(tank.Position, velocity, _projectileTexture, damage, strength);
                    ProjectileManager.AddProjectile(debris);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // 0. Draw Decorations (BEHIND Terrain)


            // 1. Draw Terrain 
            // NOTE: This call will interrupt the current SpriteBatch to draw 3D geometry
            Terrain.Draw(spriteBatch);

            // 2. Draw Entities (Tanks, Projectiles)
            // SpriteBatch is active again here
            foreach (var player in Players)
            {
                player.Draw(spriteBatch, font);
            }

            ProjectileManager.Draw(spriteBatch, font);
            ExplosionManager.Draw(spriteBatch);

            // 3. Draw Water (Semi-transparent overlay)
            Terrain.DrawWater(spriteBatch);

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
            }
        }
    }
}