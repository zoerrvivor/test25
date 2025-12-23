using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Test25.Gameplay.Entities;
using Test25.Gameplay.Managers;
using Test25.Gameplay.World;
using Test25.Services;
using Test25.UI;
using Test25.UI.Screens;

namespace Test25;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private GameManager _gameManager;
    private DialogueManager _dialogueManager;
    private Terrain _terrain;

    // Textures
    private Texture2D _tankBodyTexture;
    private Texture2D _tankBarrelTexture;
    private Texture2D _projectileTexture;
    private Texture2D _cloudTexture;

    private SpriteFont _font;

    // Managers / States
    private DebugManager _debugManager;
    private GameState _gameState;
    private GameState _lastGameState; // Track previous state for music transitions
    private MenuScreen _menuScreen;
    private SetupScreen _setupScreen;
    private ShopScreen _shopScreen;
    private OptionsScreen _optionsScreen;
    private PauseScreen _pauseScreen;
    private CloudManager _cloudManager;
    private SummaryScreen _summaryScreen;

    private Camera _camera;

    private bool _isTimeAccelerated;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Load Settings FIRST
        SettingsManager.Load();

        // Set resolution based on settings
        _graphics.PreferredBackBufferWidth = SettingsManager.ResolutionWidth;
        _graphics.PreferredBackBufferHeight = SettingsManager.ResolutionHeight;
        _graphics.IsFullScreen = SettingsManager.IsFullScreen;

        // Anti-aliasing helps the geometry terrain look smoother
        _graphics.PreferMultiSampling = true;

        Window.TextInput += (s, e) => InputManager.ReceiveTextInput(e.Character, e.Key);

        // Initialize state tracking
        _lastGameState = (GameState)(-1); // Force update on first frame
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load Assets
        _font = Content.Load<SpriteFont>("Font");

        // Initialize GUI Resources (Shared Textures)
        Test25.UI.Controls.GuiResources.Init(GraphicsDevice);

        // Initialize Sound Manager (Safe to call even with no sounds)
        SoundManager.LoadContent(Content);
        // SettingsManager.Load(); // Moved to constructor

        _tankBodyTexture = Content.Load<Texture2D>("Images/tank_body");
        _tankBarrelTexture = Content.Load<Texture2D>("Images/tank_gun_barrel");
        _cloudTexture = Content.Load<Texture2D>("Images/cloud");

        // Initialize Camera
        _camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);


        // Dynamic Decoration Loading
        List<Texture2D> decorationTextures = new List<Texture2D>();
        string imagesPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Content.RootDirectory, "Images");

        if (Directory.Exists(imagesPath))
        {
            var ruinFiles = Directory.GetFiles(imagesPath, "*ruins*.xnb");
            foreach (var file in ruinFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                // "Images/" prefix is needed because Content.RootDirectory is 'Content'
                decorationTextures.Add(Content.Load<Texture2D>($"Images/{fileName}"));
            }
        }

        // Fallback if none found (safety)
        if (decorationTextures.Count == 0)
        {
            // Try explicit load if dynamic failed
            try
            {
                decorationTextures.Add(Content.Load<Texture2D>("Images/building_ruins"));
            }
            catch
            {
                // ignored
            }
        }

        // Create a simple white texture using TextureGenerator
        _projectileTexture =
            Utilities.TextureGenerator.CreateSolidColorTexture(GraphicsDevice, 4, 4, Color.White);

        // Initialize World & Managers
        // Terrain is now mesh-based (VertexPositionTexture)
        _terrain = new Terrain(GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        _gameManager = new GameManager(_terrain, _projectileTexture, _tankBodyTexture, _tankBarrelTexture,
            decorationTextures, _camera);
        _debugManager = new DebugManager(_gameManager);

        _cloudManager = new CloudManager(_cloudTexture, _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight);

        _summaryScreen = new SummaryScreen(GraphicsDevice, _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight);
        _summaryScreen.LoadContent(_font);

        // Load GPU resources
        _terrain.LoadContent(Content);

        Texture2D titleScreen = Content.Load<Texture2D>("Images/title_screen");
        _menuScreen = new MenuScreen(titleScreen, GraphicsDevice, _font);
        _setupScreen = new SetupScreen(GraphicsDevice, _font);
        _shopScreen = new ShopScreen(_gameManager, GraphicsDevice, _font);
        _optionsScreen = new OptionsScreen(_graphics, GraphicsDevice, _font);
        _optionsScreen.OnResolutionChanged += HandleResolutionChange;
        _pauseScreen = new PauseScreen(GraphicsDevice, _font);


        _dialogueManager = new DialogueManager(Path.Combine(Content.RootDirectory, "Dialogues"));
        Tank.SetDialogueManager(_dialogueManager);
    }

    private void HandleResolutionChange(GraphicsDevice device)
    {
        int newWidth = device.Viewport.Width;
        int newHeight = device.Viewport.Height;

        _camera.Resize(newWidth, newHeight);

        _terrain = new Terrain(device, newWidth, newHeight);
        _terrain.LoadContent(Content);

        List<Texture2D> decorationTextures = new List<Texture2D>();
        try
        {
            string imagesPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Content.RootDirectory,
                "Images");
            if (Directory.Exists(imagesPath))
            {
                var ruinFiles = Directory.GetFiles(imagesPath, "*ruins*.xnb");
                foreach (var file in ruinFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    decorationTextures.Add(Content.Load<Texture2D>($"Images/{fileName}"));
                }
            }
        }
        catch
        {
            // ignored
        }

        if (decorationTextures.Count == 0)
        {
            try
            {
                decorationTextures.Add(Content.Load<Texture2D>("Images/building_ruins"));
            }
            catch
            {
                // ignored
            }
        }

        _gameManager = new GameManager(_terrain, _projectileTexture, _tankBodyTexture, _tankBarrelTexture,
            decorationTextures, _camera);
        _debugManager = new DebugManager(_gameManager);

        _shopScreen = new ShopScreen(_gameManager, device, _font);

        _cloudManager = new CloudManager(_cloudTexture, newWidth, newHeight);
        _summaryScreen = new SummaryScreen(device, newWidth, newHeight);
        _summaryScreen.LoadContent(_font);

        _menuScreen.OnResize(device);
        _setupScreen.OnResize(device);
        _shopScreen.OnResize(device);
        _pauseScreen = new PauseScreen(device, _font);
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        // Music Management
        // Music Management - Optimized
        if (_gameState != _lastGameState)
        {
            bool shouldPlayMusic = _gameState == GameState.Menu ||
                                   _gameState == GameState.Setup ||
                                   _gameState == GameState.Shop ||
                                   _gameState == GameState.Options ||
                                   _gameState == GameState.RoundOver ||
                                   _gameState == GameState.MatchOver;

            if (shouldPlayMusic)
            {
                // Play "menu_music" in all UI states (or switch based on state if we had more tracks)
                SoundManager.PlayMusic("menu_music");
            }
            else
            {
                // Playing, Paused -> Stop
                SoundManager.StopMusic();
            }

            _lastGameState = _gameState;
        }


        // Background elements (Clouds) update independently
        float currentWind = _gameManager != null ? _gameManager.Wind : 0f;
        _cloudManager?.Update(gameTime, currentWind);

        _camera?.Update(gameTime);

        switch (_gameState)
        {
            case GameState.Menu:
                _menuScreen.Update(gameTime);

                if (_menuScreen.IsStartGameSelected)
                {
                    _menuScreen.IsStartGameSelected = false; // Reset
                    _gameState = GameState.Setup;
                }
                else if (_menuScreen.IsOptionsSelected)
                {
                    _menuScreen.IsOptionsSelected = false; // Reset
                    _gameState = GameState.Options;
                }
                else if (_menuScreen.IsExitSelected)
                {
                    Exit();
                }

                break;

            case GameState.Setup:
                _setupScreen.Update(gameTime);
                if (_setupScreen.IsStartSelected())
                {
                    if (_gameManager != null) _gameManager.StartGame(_setupScreen.Settings);
                    _gameState = GameState.Playing;
                }

                if (InputManager.IsKeyPressed(Keys.Escape))
                {
                    _gameState = GameState.Menu;
                }

                break;

            case GameState.RoundOver:
                _summaryScreen.Update(gameTime);
                if (_summaryScreen.IsFinished)
                {
                    // Clean up explosions/projectiles before shop? 
                    // Should probably reset round state here or in StartShop?
                    _shopScreen.StartShop();
                    _gameState = GameState.Shop;
                }

                break;

            case GameState.MatchOver:
                _summaryScreen.Update(gameTime);
                if (_summaryScreen.IsFinished)
                {
                    _gameState = GameState.Menu;
                }

                break;

            case GameState.Playing:
                // Turbo Toggle
                if (InputManager.IsKeyPressed(Keys.T)) _isTimeAccelerated = !_isTimeAccelerated;

                int iterations = _isTimeAccelerated ? 4 : 1;

                for (int i = 0; i < iterations; i++)
                {
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                        InputManager.IsKeyPressed(Keys.Escape))
                    {
                        _gameState = GameState.Paused;
                        _pauseScreen.IsResumeSelected = false; // Reset flags on entry
                        _pauseScreen.IsMainMenuSelected = false;
                        _isTimeAccelerated = false; // Disable turbo on pause
                        break;
                    }

                    if (_gameManager != null && !_gameManager.IsGameOver)
                    {
                        var activeTank = _gameManager.Players[_gameManager.CurrentPlayerIndex];

                        if (activeTank.IsAi)
                        {
                            _gameManager.UpdateAi(gameTime);
                        }
                        else
                        {
                            // Input handling for human player
                            // Note: With Turbo, this input is applied multiple times per frame (Hyper-speed movement)
                            float aimDelta = InputManager.GetTurretMovement() *
                                             (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
                            if (aimDelta != 0) activeTank.AdjustAim(aimDelta);

                            float powerDelta = InputManager.GetPowerChange() *
                                               (float)gameTime.ElapsedGameTime.TotalSeconds * 50f;
                            if (powerDelta != 0) activeTank.AdjustPower(powerDelta);

                            if (InputManager.IsKeyPressed(Keys.Space))
                            {
                                _gameManager.Fire();
                            }
                        }
                    }

                    if (_gameManager != null)
                    {
                        _gameManager.Update(gameTime);

                        if (_gameManager.IsGameOver)
                        {
                            // Wait for all explosions and projectiles to finish
                            if (_gameManager.ExplosionManager.HasActiveExplosions || _gameManager.IsProjectileInAir)
                            {
                                // Do nothing, just let it update
                            }
                            else
                            {
                                _isTimeAccelerated = false; // Reset on game over

                                if (_gameManager.IsMatchOver)
                                {
                                    _summaryScreen.ShowMatchSummary(_gameManager.Players);
                                    _gameState = GameState.MatchOver;
                                }
                                else
                                {
                                    _summaryScreen.ShowRoundSummary(_gameManager.Players, _gameManager.CurrentRound);
                                    _gameState = GameState.RoundOver;
                                }

                                break;
                            }
                        }
                    }
                } // End Turbo Loop

                break;

            case GameState.Shop:
                _shopScreen.Update(gameTime);
                if (_shopScreen.IsFinished)
                {
                    if (_gameManager != null) _gameManager.StartNextRound();
                    _gameState = GameState.Playing;
                }

                break;

            case GameState.Options:
                _optionsScreen.Update(gameTime);
                if (_optionsScreen.IsBackRequested || InputManager.IsKeyPressed(Keys.Escape))
                {
                    _optionsScreen.IsBackRequested = false; // Reset
                    _gameState = GameState.Menu;
                }

                break;

            case GameState.Paused:
                _pauseScreen.Update(gameTime);
                if (_pauseScreen.IsResumeSelected)
                {
                    _gameState = GameState.Playing;
                    _pauseScreen.IsResumeSelected = false;
                }
                else if (_pauseScreen.IsMainMenuSelected)
                {
                    _gameState = GameState.Menu;
                    _pauseScreen.IsMainMenuSelected = false;
                    if (_gameManager != null)
                        _gameManager.Reset(); // Ensure we don't return to a half-finished game state weirdly
                    // Or ideally, we just go to menu. The next "Start New Game" will re-create or re-init GameManager logic.
                }

                break;
        }

        _debugManager.Update();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // --- DRAW WORLD (With Camera Shake) ---
        var viewMatrix = _camera.GetViewMatrix();
        _spriteBatch.Begin(transformMatrix: viewMatrix);

        // 1. Draw Clouds (Background) - Shaken
        _cloudManager?.Draw(_spriteBatch);

        switch (_gameState)
        {
            case GameState.Playing:
            case GameState.Paused: // Draw world behind pause menu
                // This draws Terrain (Mesh) + Tanks + Projectiles + Water (All Shaken)
                _gameManager.DrawWorld(_spriteBatch, _font, viewMatrix);
                break;

            case GameState.Shop:
                // Shop background? Or just UI?
                // Existing shop drew "Draw" which drew world if shop overlays?
                // No, ShopManager.Draw draws the UI.
                break;
        }

        _spriteBatch.End();

        // --- DRAW UI (Static) ---
        _spriteBatch.Begin();

        switch (_gameState)
        {
            case GameState.Menu:
                _menuScreen.Draw(_spriteBatch);
                break;

            case GameState.Setup:
                _setupScreen.Draw(_spriteBatch);
                break;

            case GameState.Playing:
                _gameManager.DrawUI(_spriteBatch, _font);

                if (_isTimeAccelerated)
                {
                    _spriteBatch.DrawString(_font, "TURBO >>", new Vector2(GraphicsDevice.Viewport.Width - 100, 10),
                        Color.Red);
                }

                break;

            case GameState.Shop:
                // Shop is full screen UI basically
                _shopScreen.Draw(_spriteBatch, _font, _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight);
                break;

            case GameState.RoundOver:
            case GameState.MatchOver:
                _summaryScreen.Draw(_spriteBatch);
                break;

            case GameState.Options:
                _optionsScreen.Draw(_spriteBatch);
                break;

            case GameState.Paused:
                // Draw game UI behind pause menu
                _gameManager.DrawUI(_spriteBatch, _font);
                // Draw pause menu overlay
                _pauseScreen.Draw(_spriteBatch);
                break;
        }

        _debugManager.Draw(_spriteBatch, _font);

        _spriteBatch.End();


        base.Draw(gameTime);
    }
}