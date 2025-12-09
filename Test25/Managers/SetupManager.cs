// Version: 0.2

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Test25.GUI;

namespace Test25.Managers
{
    public class SetupManager
    {
        public MatchSettings Settings { get; private set; }
        private GuiManager _guiManager;
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;

        // State Flags
        public bool IsStartGameRequested { get; set; }

        public SetupManager(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            Settings = new MatchSettings();
            _graphicsDevice = graphicsDevice;
            _font = font;
            _guiManager = new GuiManager();

            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            RebuildGui();
        }

        private void RebuildGui()
        {
            _guiManager.Clear();

            // Background Panel
            int panelWidth = 600;
            int panelHeight = 500;
            Rectangle panelRect = new Rectangle(
                (_screenWidth - panelWidth) / 2,
                (_screenHeight - panelHeight) / 2,
                panelWidth,
                panelHeight
            );
            Panel bgPanel = new Panel(_graphicsDevice, panelRect);
            _guiManager.AddElement(bgPanel);

            // Title
            Label title = new Label("Match Setup", _font, new Vector2(panelRect.X + 240, panelRect.Y + 20));
            _guiManager.AddElement(title);

            // --- Settings Controls ---
            int startY = panelRect.Y + 60;
            int labelX = panelRect.X + 50;
            int valueX = panelRect.X + 250;
            int rowHeight = 40;

            // Wall Type
            Label wallLabel = new Label("Wall Type:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(wallLabel);

            Button prevWall = new Button(_graphicsDevice, new Rectangle(valueX, startY, 30, 30), "<", _font);
            prevWall.OnClick += (e) => CycleWallType(-1);
            _guiManager.AddElement(prevWall);

            Label wallValue = new Label(Settings.WallType.ToString(), _font, new Vector2(valueX + 40, startY));
            _guiManager.AddElement(wallValue);

            Button nextWall = new Button(_graphicsDevice, new Rectangle(valueX + 150, startY, 30, 30), ">", _font);
            nextWall.OnClick += (e) => CycleWallType(1);
            _guiManager.AddElement(nextWall);

            // Rounds
            startY += rowHeight;
            Label roundsLabel = new Label("Rounds:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(roundsLabel);

            Button prevRound = new Button(_graphicsDevice, new Rectangle(valueX, startY, 30, 30), "<", _font);
            prevRound.OnClick += (e) => ChangeRounds(-1);
            _guiManager.AddElement(prevRound);

            Label roundsValue = new Label(Settings.NumRounds.ToString(), _font, new Vector2(valueX + 40, startY));
            _guiManager.AddElement(roundsValue);

            Button nextRound = new Button(_graphicsDevice, new Rectangle(valueX + 150, startY, 30, 30), ">", _font);
            nextRound.OnClick += (e) => ChangeRounds(1);
            _guiManager.AddElement(nextRound);

            // Player List Header
            startY += rowHeight + 10;
            Label playersHeader = new Label("Players:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(playersHeader);

            // Dynamic Player Rows
            startY += 30;
            for (int i = 0; i < Settings.Players.Count; i++)
            {
                int playerIndex = i; // Local copy for closure
                PlayerSetup p = Settings.Players[i];

                Label nameLabel = new Label(p.Name, _font, new Vector2(labelX, startY));
                _guiManager.AddElement(nameLabel);

                // Color Box (Click to cycle)
                Button colorBtn = new Button(_graphicsDevice, new Rectangle(valueX, startY, 20, 20), "", _font);
                colorBtn.BackgroundColor = p.Color;
                colorBtn.HoverColor = p.Color; // No hover change for now
                colorBtn.OnClick += (e) => CyclePlayerColor(playerIndex);
                _guiManager.AddElement(colorBtn);

                // AI Checkbox
                Checkbox aiCheck = new Checkbox(_graphicsDevice, new Rectangle(valueX + 40, startY, 20, 20), "CPU",
                    _font);
                aiCheck.IsChecked = p.IsAI;
                aiCheck.OnClick += (e) => { p.IsAI = aiCheck.IsChecked; }; // Update directly
                _guiManager.AddElement(aiCheck);

                // Remove Button
                if (Settings.Players.Count > 2)
                {
                    Button removeBtn = new Button(_graphicsDevice, new Rectangle(valueX + 120, startY, 60, 25),
                        "Remove", _font);
                    removeBtn.OnClick += (e) => RemovePlayer(playerIndex);
                    _guiManager.AddElement(removeBtn);
                }

                startY += 30;
            }

            // Add Player Button
            if (Settings.Players.Count < 8)
            {
                Button addPlayerBtn = new Button(_graphicsDevice, new Rectangle(labelX, startY + 10, 100, 30),
                    "Add Player", _font);
                addPlayerBtn.OnClick += (e) => AddPlayer();
                _guiManager.AddElement(addPlayerBtn);
            }

            // Start Game Button (Bottom Right)
            Button startBtn = new Button(_graphicsDevice,
                new Rectangle(panelRect.Right - 150, panelRect.Bottom - 50, 120, 40), "Start Game", _font);
            startBtn.BackgroundColor = Color.Green;
            startBtn.OnClick += (e) => IsStartGameRequested = true;
            _guiManager.AddElement(startBtn);
        }

        private void CycleWallType(int dir)
        {
            int newType = (int)Settings.WallType + dir;
            if (newType < 0) newType = 2; // Assuming 3 types
            if (newType > 2) newType = 0;
            Settings.WallType = (WallType)newType;
            RebuildGui();
        }

        private void ChangeRounds(int amount)
        {
            Settings.NumRounds += amount;
            if (Settings.NumRounds < 1) Settings.NumRounds = 1;
            if (Settings.NumRounds > 99) Settings.NumRounds = 99;
            RebuildGui();
        }

        private void AddPlayer()
        {
            Color[] colors =
            {
                Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Cyan, Color.Violet, Color.HotPink, Color.Orange,
                Color.White
            };
            Color newColor = colors[Settings.Players.Count % colors.Length];
            Settings.Players.Add(new PlayerSetup($"Player {Settings.Players.Count + 1}", newColor));
            RebuildGui();
        }

        private void RemovePlayer(int index)
        {
            if (index >= 0 && index < Settings.Players.Count)
            {
                Settings.Players.RemoveAt(index);
                RebuildGui();
            }
        }

        private void CyclePlayerColor(int index)
        {
            Color[] colors =
            {
                Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Cyan, Color.Violet, Color.HotPink, Color.Orange,
                Color.White
            };
            int curIdx = Array.IndexOf(colors, Settings.Players[index].Color);
            if (curIdx == -1) curIdx = 0;
            curIdx = (curIdx + 1) % colors.Length;
            Settings.Players[index].Color = colors[curIdx];
            RebuildGui();
        }

        public void Update(GameTime gameTime)
        {
            _guiManager.Update(gameTime);
        }

        public bool IsStartSelected()
        {
            if (IsStartGameRequested)
            {
                IsStartGameRequested = false;
                return true;
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _guiManager.Draw(spriteBatch);
        }
    }
}