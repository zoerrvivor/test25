using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.GUI;

namespace Test25.Managers
{
    public class OptionsManager
    {
        private GuiManager _guiManager;
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;

        public bool IsBackRequested { get; set; }

        public OptionsManager(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _font = font;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            _guiManager = new GuiManager();

            RebuildGui();
        }

        private void RebuildGui()
        {
            _guiManager.Clear();

            // Background Panel
            int panelWidth = 500;
            int panelHeight = 400;
            Rectangle panelRect = new Rectangle(
                (_screenWidth - panelWidth) / 2,
                (_screenHeight - panelHeight) / 2,
                panelWidth,
                panelHeight
            );
            Panel bgPanel = new Panel(_graphicsDevice, panelRect);
            _guiManager.AddElement(bgPanel);

            // Title
            Label title = new Label("Options", _font, new Vector2(panelRect.X + 210, panelRect.Y + 20));
            _guiManager.AddElement(title);

            int startY = panelRect.Y + 80;
            int labelX = panelRect.X + 50;
            int sliderX = panelRect.X + 200;
            int sliderWidth = 200;
            int sliderHeight = 20; // Hitbox height (visual track is thinner, Handle is 20)
            int spacing = 50;

            // --- Master Volume ---
            Label masterLabel = new Label("Master Volume:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(masterLabel);

            Slider masterSlider = new Slider(_graphicsDevice, new Rectangle(sliderX, startY, sliderWidth, sliderHeight), SoundManager.MasterVolume);
            masterSlider.OnValueChanged += (val) => SoundManager.MasterVolume = val;
            _guiManager.AddElement(masterSlider);

            // --- Music Volume ---
            startY += spacing;
            Label musicLabel = new Label("Music Volume:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(musicLabel);

            Slider musicSlider = new Slider(_graphicsDevice, new Rectangle(sliderX, startY, sliderWidth, sliderHeight), SoundManager.MusicVolume);
            musicSlider.OnValueChanged += (val) => 
            {
                SoundManager.MusicVolume = val;
                Microsoft.Xna.Framework.Media.MediaPlayer.Volume = SoundManager.MusicVolume * SoundManager.MasterVolume;
            };
            _guiManager.AddElement(musicSlider);

            // --- SFX Volume ---
            startY += spacing;
            Label sfxLabel = new Label("SFX Volume:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(sfxLabel);

            Slider sfxSlider = new Slider(_graphicsDevice, new Rectangle(sliderX, startY, sliderWidth, sliderHeight), SoundManager.SfxVolume);
            sfxSlider.OnValueChanged += (val) => SoundManager.SfxVolume = val;
            _guiManager.AddElement(sfxSlider);


            // --- Back Button ---
            Button backBtn = new Button(_graphicsDevice, new Rectangle(panelRect.X + 200, panelRect.Bottom - 60, 100, 40), "Back", _font);
            backBtn.OnClick += (e) => IsBackRequested = true;
            _guiManager.AddElement(backBtn);
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
