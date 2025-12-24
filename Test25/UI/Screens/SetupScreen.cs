using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Test25.UI.Controls;
using Test25.Gameplay.Entities;
using Test25.Services;

namespace Test25.UI.Screens
{
    public class SetupScreen
    {
        public MatchSettings Settings { get; private set; }
        private GuiManager _guiManager;
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;

        // State Flags
        public bool IsStartGameRequested { get; set; }

        public SetupScreen(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            Settings = new MatchSettings();
            _graphicsDevice = graphicsDevice;
            _font = font;
            _guiManager = new GuiManager();

            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            RebuildGui();
        }

        private Label _wallValueLabel;
        private Label _roundsValueLabel;

        public void OnResize(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _font = font;
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

            _wallValueLabel = new Label(Settings.WallType.ToString(), _font, new Vector2(valueX + 40, startY));
            _guiManager.AddElement(_wallValueLabel);

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

            _roundsValueLabel = new Label(Settings.NumRounds.ToString(), _font, new Vector2(valueX + 40, startY));
            _guiManager.AddElement(_roundsValueLabel);

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

                TextInput nameInput = new TextInput(_graphicsDevice, new Rectangle(labelX, startY, 150, 25), _font);
                nameInput.Text = p.Name;
                nameInput.OnTextChanged += (newName) => p.Name = newName;
                _guiManager.AddElement(nameInput);

                // Color Box (Click to cycle)
                Button colorBtn = new Button(_graphicsDevice, new Rectangle(valueX, startY, 20, 20), "", _font);
                colorBtn.BackgroundColor = p.Color;
                colorBtn.HoverColor = p.Color; // No hover change for now
                colorBtn.OnClick += (e) =>
                {
                    CyclePlayerColor(playerIndex);
                    // Update Button Color immediately (requires closure capture or refetch)
                    // We can just rebuild for player properties for now, as they are less spammy than WallType
                    // RebuildGui(); 
                    // Actually, let's just Rebuild because color change is visually complex to partial update without ref
                    RebuildGui();
                };
                _guiManager.AddElement(colorBtn);

                // AI Checkbox
                Checkbox aiCheck = new Checkbox(_graphicsDevice, new Rectangle(valueX + 40, startY, 20, 20), "CPU",
                    _font);
                aiCheck.IsChecked = p.IsAi;
                aiCheck.OnClick += (e) =>
                {
                    p.IsAi = aiCheck.IsChecked;
                    RebuildGui(); // Rebuild to show/hide personality button
                };
                _guiManager.AddElement(aiCheck);

                // Personality Button (Only if AI)
                if (p.IsAi)
                {
                    string pName = p.Personality?.Name ?? "Random";
                    Button pBtn = new Button(_graphicsDevice, new Rectangle(valueX + 90, startY, 80, 25), pName, _font);
                    pBtn.OnClick += (e) => CyclePersonality(playerIndex);
                    _guiManager.AddElement(pBtn);
                }

                // Remove Button
                int removeX = p.IsAi ? valueX + 180 : valueX + 120;

                if (Settings.Players.Count > 2)
                {
                    Button removeBtn = new Button(_graphicsDevice, new Rectangle(removeX, startY, 60, 25),
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

            // In-place update
            if (_wallValueLabel != null) _wallValueLabel.Text = Settings.WallType.ToString();
        }

        private void ChangeRounds(int amount)
        {
            Settings.NumRounds += amount;
            if (Settings.NumRounds < 1) Settings.NumRounds = 1;
            if (Settings.NumRounds > 99) Settings.NumRounds = 99;

            // In-place update
            if (_roundsValueLabel != null) _roundsValueLabel.Text = Settings.NumRounds.ToString();
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
            // Rebuild required to update button color unless we refactor button ref too
            RebuildGui();
        }

        private void CyclePersonality(int index)
        {
            var player = Settings.Players[index];
            var options = AiPersonality.All;

            int curIdx = 0;
            if (player.Personality != null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (options[i].Name == player.Personality.Name)
                    {
                        curIdx = i;
                        break;
                    }
                }
            }

            curIdx = (curIdx + 1) % options.Count;
            player.Personality = options[curIdx];
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