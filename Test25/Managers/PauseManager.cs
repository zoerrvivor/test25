using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.GUI;

namespace Test25.Managers
{
    public class PauseManager
    {
        private GuiManager _guiManager;
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;

        public bool IsResumeSelected { get; set; }
        public bool IsMainMenuSelected { get; set; }

        public PauseManager(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _font = font;
            _guiManager = new GuiManager();
            
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            InitializeGui();
        }

        private void InitializeGui()
        {
            _guiManager.Clear();

            // Background Panel (Semitransparent overlay simulation?)
            // Since we can't easily do alpha blending with CreateSolidColorTexture without alpha support in that helper,
            // we'll just make a solid panel for the menu itself, not fullscreen covering.
            
            int panelWidth = 300;
            int panelHeight = 250;
            Rectangle panelRect = new Rectangle(
                (_screenWidth - panelWidth) / 2,
                (_screenHeight - panelHeight) / 2,
                panelWidth,
                panelHeight
            );

            Panel bgPanel = new Panel(_graphicsDevice, panelRect);
            // Optional: If we want it to look distinct, we could use a different color.
            // But default constant color is fine.
            _guiManager.AddElement(bgPanel);

            // Title
            Label titleLabel = new Label("PAUSED", _font, new Vector2(panelRect.X + 100, panelRect.Y + 20));
            titleLabel.TextColor = Color.Yellow;
            _guiManager.AddElement(titleLabel);

            // Resume Button
            Button resumeBtn = new Button(_graphicsDevice, new Rectangle(panelRect.X + 50, panelRect.Y + 80, 200, 40), "Resume Game", _font);
            resumeBtn.OnClick += (e) => IsResumeSelected = true;
            _guiManager.AddElement(resumeBtn);

            // Exit Button
            Button exitBtn = new Button(_graphicsDevice, new Rectangle(panelRect.X + 50, panelRect.Y + 150, 200, 40), "Exit to Menu", _font);
            exitBtn.OnClick += (e) => IsMainMenuSelected = true;
            exitBtn.BackgroundColor = Color.DarkRed;
            exitBtn.HoverColor = Color.Red;
            _guiManager.AddElement(exitBtn);
        }

        public void Update(GameTime gameTime)
        {
            _guiManager.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _guiManager.Draw(spriteBatch);
        }
    }
}
