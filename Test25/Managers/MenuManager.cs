// Version: 0.1

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Test25.GUI;

namespace Test25.Managers
{
    public class MenuManager
    {
        private GuiManager _guiManager;
        private Texture2D _background;
        private SpriteFont _font;

        // State for Game1 to read
        public bool IsStartGameSelected { get; set; }
        public bool IsOptionsSelected { get; set; }
        public bool IsExitSelected { get; set; }

        public MenuManager(Texture2D background, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _background = background;
            _font = font;
            _guiManager = new GuiManager();
            InitializeGui(graphicsDevice, font);
        }

        public void OnResize(GraphicsDevice graphicsDevice)
        {
            InitializeGui(graphicsDevice, null); // Font stays same? Or we need to store it?
        }


        private void InitializeGui(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;

            // Main Panel
            int panelWidth = 300;
            int panelHeight = 250;
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

            Button btnExit = new Button(graphicsDevice,
                new Rectangle(startX, startY + (btnHeight + gap) * 2, btnWidth, btnHeight), "Exit", _font);
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
