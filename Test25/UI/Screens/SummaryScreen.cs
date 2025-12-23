using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Test25.Gameplay.Entities;
using Test25.UI.Controls;

namespace Test25.UI.Screens
{
    public class SummaryScreen
    {
        private GuiManager _guiManager;
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;

        public bool IsFinished { get; set; } // Flag to signal Game1 to proceed

        public SummaryScreen(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = width;
            _screenHeight = height;
            _guiManager = new GuiManager();
        }

        public void OnResize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            // We don't have a RebuildGui here that persists state, but ShowRoundSummary / ShowMatchSummary will use new dims when called.
            // If summary is currently showing, we might want to refresh it, but we don't have the player list stored.
            // For now, next show call will be correct. If it was already showing, this might not resize strictly, but usually res change happens in menus before summary.
        }

        public void LoadContent(SpriteFont font)
        {
            _font = font;
        }

        public void Update(GameTime gameTime)
        {
            _guiManager.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _guiManager.Draw(spriteBatch);
        }

        public void ShowRoundSummary(List<Tank> players, int round)
        {
            SetupSummary(players, $"ROUND {round} SUMMARY", false);
        }

        public void ShowMatchSummary(List<Tank> players)
        {
            SetupSummary(players, "MATCH RESULTS", true);
        }

        private void SetupSummary(List<Tank> players, string title, bool isMatchOver)
        {
            IsFinished = false;
            _guiManager.Clear();

            // Background Panel
            int panelWidth = 600;
            int panelHeight = 400;
            Rectangle panelBounds = new Rectangle(
                (_screenWidth - panelWidth) / 2,
                (_screenHeight - panelHeight) / 2,
                panelWidth,
                panelHeight
            );

            Panel bgPanel = new Panel(_graphicsDevice, panelBounds);
            _guiManager.AddElement(bgPanel);

            // Title
            Label titleLabel = new Label(title, _font, new Vector2(panelBounds.X + 20, panelBounds.Y + 20));
            _guiManager.AddElement(titleLabel);

            // Table Headers
            // Rank | Name | Kills | Score | Money
            int startY = panelBounds.Y + 60;
            int rowHeight = 30;

            AddRow(panelBounds.X + 20, startY, "Rank", "Name", "Kills", "Score", "Money");

            // Sort Players
            // For Match Summary, likely sort by Score (or Kills?)
            // For Round Summary, traditionally sorted by Score or just list them.
            // Let's sort by Score descending.
            var sortedPlayers = players.OrderByDescending(p => p.Score).ThenByDescending(p => p.Kills).ToList();

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                var p = sortedPlayers[i];
                AddRow(panelBounds.X + 20, startY + (i + 1) * rowHeight,
                    $"#{i + 1}",
                    p.Name,
                    p.Kills.ToString(),
                    p.Score.ToString(),
                    "$" + p.Money.ToString());
            }

            // Continue Button
            string buttonText = isMatchOver ? "Back to Menu" : "Continue";
            int btnWidth = 200;
            int btnHeight = 40;
            Rectangle btnBounds = new Rectangle(
                (_screenWidth - btnWidth) / 2,
                panelBounds.Bottom - 60,
                btnWidth,
                btnHeight
            );

            Button continueBtn = new Button(_graphicsDevice, btnBounds, buttonText, _font);
            continueBtn.OnClick += (e) => { IsFinished = true; };
            _guiManager.AddElement(continueBtn);
        }

        private void AddRow(int x, int y, string rank, string name, string kills, string score, string money)
        {
            // Simple column spacing
            // int col1 = 0;
            int col2 = 60;
            int col3 = 250;
            int col4 = 350;
            int col5 = 450;

            _guiManager.AddElement(new Label(rank, _font, new Vector2(x, y)));
            _guiManager.AddElement(new Label(name, _font, new Vector2(x + col2, y)));
            _guiManager.AddElement(new Label(kills, _font, new Vector2(x + col3, y)));
            _guiManager.AddElement(new Label(score, _font, new Vector2(x + col4, y)));
            _guiManager.AddElement(new Label(money, _font, new Vector2(x + col5, y)));
        }
    }
}
