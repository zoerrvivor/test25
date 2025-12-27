using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Core.UI;
using Test25.Core.UI.Controls;

namespace Test25.UI.Screens
{
    public class MenuScreen
    {
        private GuiManager _guiManager;
        private Texture2D _background;
        private SpriteFont _font;

        // State for Game1 to read
        public bool IsStartGameSelected { get; set; }
        public bool IsOptionsSelected { get; set; }
        public bool IsEditorSelected { get; set; }
        public bool IsExitSelected { get; set; }

        public MenuScreen(Texture2D background, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _background = background;
            _font = font;
            _guiManager = new GuiManager();
            InitializeGui(graphicsDevice, font);
        }

        public void OnResize(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _font = font; // Update font reference
            InitializeGui(graphicsDevice, font);
        }


        private void InitializeGui(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _guiManager.Clear();
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;

            // Main Panel
            int panelWidth = 300;
            int panelHeight = 310; // Increased height for 4 buttons
            Rectangle panelRect = new Rectangle(
                (screenWidth - panelWidth) / 2,
                (screenHeight - panelHeight) / 2,
                panelWidth,
                panelHeight
            );

            Panel bgPanel = new Panel(graphicsDevice, panelRect);
            _guiManager.AddElement(bgPanel);

            // Title
            Label title = new Label("Main Menu", _font, new Vector2(panelRect.X + 100, panelRect.Y + 20));
            _guiManager.AddElement(title);

            // Buttons
            int btnWidth = 200;
            int btnHeight = 40;
            int startX = panelRect.X + (panelWidth - btnWidth) / 2;
            int startY = panelRect.Y + 70;
            int gap = 10;

            Button btnStart = new Button(graphicsDevice, new Rectangle(startX, startY, btnWidth, btnHeight),
                "Start New Game", _font);
            btnStart.OnClick += (e) => IsStartGameSelected = true;
            _guiManager.AddElement(btnStart);

            Button btnOptions = new Button(graphicsDevice,
                new Rectangle(startX, startY + btnHeight + gap, btnWidth, btnHeight), "Options", _font);
            btnOptions.OnClick += (e) => IsOptionsSelected = true;
            _guiManager.AddElement(btnOptions);

            Button btnEditor = new Button(graphicsDevice,
                new Rectangle(startX, startY + (btnHeight + gap) * 2, btnWidth, btnHeight), "Full Editor", _font);
            btnEditor.OnClick += (e) => IsEditorSelected = true;
            _guiManager.AddElement(btnEditor);

            Button btnExit = new Button(graphicsDevice,
                new Rectangle(startX, startY + (btnHeight + gap) * 3, btnWidth, btnHeight), "Exit", _font);
            btnExit.OnClick += (e) => IsExitSelected = true;
            _guiManager.AddElement(btnExit);
        }

        public void Update(GameTime gameTime)
        {
            // Reset flags each frame? Or let Game1 consume them?
            // Better pattern: Game1 checks flags then acts.
            _guiManager.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_background != null)
            {
                int width = spriteBatch.GraphicsDevice.Viewport.Width;
                int height = spriteBatch.GraphicsDevice.Viewport.Height;
                spriteBatch.Draw(_background, new Rectangle(0, 0, width, height), Color.White);
            }

            _guiManager.Draw(spriteBatch);
        }

        // Backward compatibility getters if needed, but we should update Game1 to use the properties
        public string GetSelectedItem()
        {
            if (IsStartGameSelected)
            {
                IsStartGameSelected = false;
                return "Start New Game";
            }

            if (IsOptionsSelected)
            {
                IsOptionsSelected = false;
                return "Options";
            }

            if (IsExitSelected)
            {
                IsExitSelected = false;
                return "Exit";
            }

            return "";
        }
    }
}
