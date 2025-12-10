using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Test25.Entities;
using Test25.Managers;
using Test25.World;

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
    private MenuManager _menuManager;
    private SetupManager _setupManager;
    private ShopManager _shopManager;
    private PauseManager _pauseManager;
    private CloudManager _cloudManager;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set resolution
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        // Anti-aliasing helps the geometry terrain look smoother
        _graphics.PreferMultiSampling = true;

        Window.TextInput += (s, e) => InputManager.ReceiveTextInput(e.Character, e.Key);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load Assets
        _font = Content.Load<SpriteFont>("Font");
        _tankBodyTexture = Content.Load<Texture2D>("Images/tank_body");
        _tankBarrelTexture = Content.Load<Texture2D>("Images/tank_gun_barrel");
        _cloudTexture = Content.Load<Texture2D>("Images/cloud");


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
            decorationTextures);
        _debugManager = new DebugManager(_gameManager);

        _cloudManager = new CloudManager(_cloudTexture, _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight);

        // Load GPU resources
        _terrain.LoadContent(Content);

        Texture2D titleScreen = Content.Load<Texture2D>("Images/title_screen");
        _menuManager = new MenuManager(titleScreen, GraphicsDevice, _font);
        _setupManager = new SetupManager(GraphicsDevice, _font);
        _shopManager = new ShopManager(_gameManager, GraphicsDevice, _font);
        _pauseManager = new PauseManager(GraphicsDevice, _font);


        _dialogueManager = new DialogueManager(Path.Combine(Content.RootDirectory, "Dialogues"));
        Tank.SetDialogueManager(_dialogueManager);
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();


        // Background elements (Clouds) update independently
        float currentWind = _gameManager != null ? _gameManager.Wind : 0f;
        _cloudManager?.Update(gameTime, currentWind);

        switch (_gameState)
        {
            case GameState.Menu:
                _menuManager.Update(gameTime);

                if (_menuManager.IsStartGameSelected)
                {
                    _menuManager.IsStartGameSelected = false; // Reset
                    _gameState = GameState.Setup;
                }
                else if (_menuManager.IsOptionsSelected)
                {
                    _menuManager.IsOptionsSelected = false; // Reset
                    _gameState = GameState.Options;
                }
                else if (_menuManager.IsExitSelected)
                {
                    Exit();
                }

                break;

            case GameState.Setup:
                _setupManager.Update(gameTime);
                if (_setupManager.IsStartSelected())
                {
                    if (_gameManager != null) _gameManager.StartGame(_setupManager.Settings);
                    _gameState = GameState.Playing;
                }

                if (InputManager.IsKeyPressed(Keys.Escape))
                {
                    _gameState = GameState.Menu;
                }

                break;

            case GameState.Playing:
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                    InputManager.IsKeyPressed(Keys.Escape))
                {
                    _gameState = GameState.Paused;
                    _pauseManager.IsResumeSelected = false; // Reset flags on entry
                    _pauseManager.IsMainMenuSelected = false;
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
                        if (InputManager.IsKeyPressed(Keys.Enter))
                        {
                            if (_gameManager.IsMatchOver)
                            {
                                _gameState = GameState.Menu;
                            }
                            else
                            {
                                _shopManager.StartShop();
                                _gameState = GameState.Shop;
                            }
                        }
                    }
                }

                break;

            case GameState.Shop:
                _shopManager.Update(gameTime);
                if (_shopManager.IsFinished)
                {
                    if (_gameManager != null) _gameManager.StartNextRound();
                    _gameState = GameState.Playing;
                }

                break;

            case GameState.Options:
                if (InputManager.IsKeyPressed(Keys.Escape))
                {
                    _gameState = GameState.Menu;
                }

                break;

            case GameState.Paused:
                _pauseManager.Update(gameTime);
                if (_pauseManager.IsResumeSelected)
                {
                    _gameState = GameState.Playing;
                    _pauseManager.IsResumeSelected = false;
                }
                else if (_pauseManager.IsMainMenuSelected)
                {
                    _gameState = GameState.Menu;
                    _pauseManager.IsMainMenuSelected = false;
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

        // Begin global SpriteBatch. 
        // NOTE: Terrain.Draw() inside GameManager will temporarily End() this batch 
        // to draw 3D geometry, and then Begin() it again.
        _spriteBatch.Begin();

        // 1. Draw Clouds (Background)
        _cloudManager?.Draw(_spriteBatch);

        switch (_gameState)
        {
            case GameState.Menu:
                _menuManager.Draw(_spriteBatch);
                break;

            case GameState.Setup:
                _setupManager.Draw(_spriteBatch);
                break;

            case GameState.Playing:
                // This draws Terrain (Mesh) + Tanks + Projectiles + Water + UI
                _gameManager.Draw(_spriteBatch, _font);
                break;

            case GameState.Shop:
                _shopManager.Draw(_spriteBatch, _font, _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight);
                break;

            case GameState.Options:
                _spriteBatch.DrawString(_font, "Options Placeholder", new Vector2(100, 100), Color.White);
                _spriteBatch.DrawString(_font, "Press Escape to Return", new Vector2(100, 130), Color.White);
                break;

            case GameState.Paused:
                // Draw game behind (frozen)
                _gameManager.Draw(_spriteBatch, _font);
                // Draw pause menu overlay
                _pauseManager.Draw(_spriteBatch);
                break;
        }

        _debugManager.Draw(_spriteBatch, _font);

        _spriteBatch.End();


        base.Draw(gameTime);
    }
}