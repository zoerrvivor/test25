using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Core.Services;
using Test25.Core.UI;
using Test25.Core.UI.Controls;

namespace Test25.UI.Screens
{
    public class OptionsScreen
    {
        private GuiManager _guiManager;
        private GraphicsDeviceManager _graphicsManager;
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;

        private Point? _pendingResolution;
        private bool? _pendingFullScreen;

        private List<Point> _availableResolutions;
        public bool IsBackRequested { get; set; }

        // Changed to simple notification, Game1 handles the logic/reloading
        public event System.Action OnResolutionChangeRequested;

        public OptionsScreen(GraphicsDeviceManager graphicsManager, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _graphicsManager = graphicsManager;
            _graphicsDevice = graphicsDevice;
            _font = font;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            _guiManager = new GuiManager();

            _availableResolutions = new List<Point>
            {
                new Point(800, 600),
                new Point(1024, 768),
                new Point(1280, 720),
                new Point(1366, 768),
                new Point(1600, 900)
            };

            RebuildGui();
        }

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

            // Refresh Dimensions
            _screenWidth = _graphicsDevice.Viewport.Width;
            _screenHeight = _graphicsDevice.Viewport.Height;

            // Background Panel
            int panelWidth = 500;
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
            Label title = new Label("Options", _font, new Vector2(panelRect.X + 210, panelRect.Y + 20));
            _guiManager.AddElement(title);

            int startY = panelRect.Y + 80;
            int labelX = panelRect.X + 50;
            int sliderX = panelRect.X + 200;
            int sliderWidth = 200;
            int sliderHeight = 20;
            int spacing = 60;

            // --- Master Volume ---
            Label masterLabel = new Label("Master Volume:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(masterLabel);

            Slider masterSlider = new Slider(_graphicsDevice, new Rectangle(sliderX, startY, sliderWidth, sliderHeight),
                SoundManager.MasterVolume);
            masterSlider.OnValueChanged += (val) => SoundManager.MasterVolume = val;
            _guiManager.AddElement(masterSlider);

            // --- Music Volume ---
            startY += spacing;
            Label musicLabel = new Label("Music Volume:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(musicLabel);

            Slider musicSlider = new Slider(_graphicsDevice, new Rectangle(sliderX, startY, sliderWidth, sliderHeight),
                SoundManager.MusicVolume);
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

            Slider sfxSlider = new Slider(_graphicsDevice, new Rectangle(sliderX, startY, sliderWidth, sliderHeight),
                SoundManager.SfxVolume);
            sfxSlider.OnValueChanged += (val) => SoundManager.SfxVolume = val;
            _guiManager.AddElement(sfxSlider);

            // --- Resolution ---
            startY += spacing;
            Label resLabel = new Label("Resolution:", _font, new Vector2(labelX, startY));
            _guiManager.AddElement(resLabel);

            // Use pending resolution for display if set, otherwise current settings
            int displayW = _pendingResolution.HasValue ? _pendingResolution.Value.X : SettingsManager.ResolutionWidth;
            int displayH = _pendingResolution.HasValue ? _pendingResolution.Value.Y : SettingsManager.ResolutionHeight;

            string resText = $"{displayW}x{displayH}";
            Button resBtn = new Button(_graphicsDevice, new Rectangle(sliderX, startY - 5, 200, 30), resText, _font);
            resBtn.OnClick += (e) => { CycleResolution(); };
            _guiManager.AddElement(resBtn);

            // --- Fullscreen ---
            startY += spacing;
            Checkbox fullScreenCheck =
                new Checkbox(_graphicsDevice, new Rectangle(labelX, startY, 20, 20), "Fullscreen", _font);

            // Use pending fullscreen state if set
            fullScreenCheck.IsChecked =
                _pendingFullScreen.HasValue ? _pendingFullScreen.Value : SettingsManager.IsFullScreen;

            fullScreenCheck.OnClick += (e) =>
            {
                // Defer change
                _pendingFullScreen = fullScreenCheck.IsChecked;
            };
            _guiManager.AddElement(fullScreenCheck);


            // --- Back Button ---
            Button backBtn = new Button(_graphicsDevice,
                new Rectangle(panelRect.X + 200, panelRect.Bottom - 60, 100, 40), "Back", _font);
            backBtn.OnClick += (e) =>
            {
                SettingsManager.Save();
                IsBackRequested = true;
            };
            _guiManager.AddElement(backBtn);
        }

        private void CycleResolution()
        {
            // Calculate based on current pending or actual settings
            int currentWidth = _pendingResolution.HasValue
                ? _pendingResolution.Value.X
                : SettingsManager.ResolutionWidth;
            int currentHeight = _pendingResolution.HasValue
                ? _pendingResolution.Value.Y
                : SettingsManager.ResolutionHeight;

            int index = _availableResolutions.FindIndex(r => r.X == currentWidth && r.Y == currentHeight);

            index++;
            if (index >= _availableResolutions.Count) index = 0;

            // Set pending, do NOT apply immediately
            _pendingResolution = _availableResolutions[index];

            // Rebuild GUI immediately to reflect the new text/state without applying graphics changes yet
            // This is safe because we are just rebuilding the UI list, not resetting the device
            RebuildGui();
        }

        public void Update(GameTime gameTime)
        {
            _guiManager.Update(gameTime);

            // Check for pending changes AFTER UI update is complete
            if (_pendingResolution.HasValue || _pendingFullScreen.HasValue)
            {
                // Commit pending changes to SettingsManager, then request application
                if (_pendingResolution.HasValue)
                {
                    SettingsManager.ResolutionWidth = _pendingResolution.Value.X;
                    SettingsManager.ResolutionHeight = _pendingResolution.Value.Y;
                    _pendingResolution = null;
                }

                if (_pendingFullScreen.HasValue)
                {
                    SettingsManager.IsFullScreen = _pendingFullScreen.Value;
                    _pendingFullScreen = null;
                }

                // Notify Game1 to take over
                OnResolutionChangeRequested?.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _guiManager.Draw(spriteBatch);
        }
    }
}

