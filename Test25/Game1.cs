// Version: 0.1
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    private Texture2D _tankBodyTexture;
    private Texture2D _tankBarrelTexture;
    private Texture2D _projectileTexture;

    // Removed static SpriteFont property
    private SpriteFont _font;

    private DebugManager _debugManager;
    private GameState _gameState;
    private MenuManager _menuManager;
    private SetupManager _setupManager;
    private ShopManager _shopManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("Font");

        _tankBodyTexture = Content.Load<Texture2D>("Images/tank_body");
        _tankBarrelTexture = Content.Load<Texture2D>("Images/tank_gun_barrel");

        _projectileTexture = new Texture2D(GraphicsDevice, 4, 4);
        Color[] projData = new Color[4 * 4];
        for (int i = 0; i < projData.Length; i++) projData[i] = Color.White;
        _projectileTexture.SetData(projData);

        _terrain = new Terrain(GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        _gameManager = new GameManager(_terrain, _projectileTexture, _tankBodyTexture, _tankBarrelTexture);
        _debugManager = new DebugManager(_gameManager);

        Texture2D titleScreen = Content.Load<Texture2D>("Images/title_screen");
        _menuManager = new MenuManager(titleScreen);
        _setupManager = new SetupManager();
        _shopManager = new ShopManager(_gameManager);

        _dialogueManager = new DialogueManager(Path.Combine(Content.RootDirectory, "Dialogues"));
        Tank.SetDialogueManager(_dialogueManager);
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        switch (_gameState)
        {
            case GameState.Menu:
                _menuManager.Update();
                if (InputManager.IsKeyPressed(Keys.Enter))
                {
                    string selected = _menuManager.GetSelectedItem();
                    if (selected == "Start New Game")
                    {
                        _gameState = GameState.Setup;
                    }
                    else if (selected == "Options")
                    {
                        _gameState = GameState.Options;
                    }
                    else if (selected == "Exit")
                    {
                        Exit();
                    }
                }
                break;

            case GameState.Setup:
                _setupManager.Update();
                if (_setupManager.IsStartSelected())
                {
                    _gameManager.StartGame(_setupManager.Settings);
                    _gameState = GameState.Playing;
                }
                if (InputManager.IsKeyPressed(Keys.Escape))
                {
                    _gameState = GameState.Menu;
                }
                break;

            case GameState.Playing:
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || InputManager.IsKeyPressed(Keys.Escape))
                {
                    _gameState = GameState.Menu;
                }

                if (!_gameManager.IsGameOver)
                {
                    var activeTank = _gameManager.Players[_gameManager.CurrentPlayerIndex];

                    if (activeTank.IsAI)
                    {
                        _gameManager.UpdateAI(gameTime);
                    }
                    else
                    {
                        float aimDelta = InputManager.GetTurretMovement() * (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
                        if (aimDelta != 0) activeTank.AdjustAim(aimDelta);

                        float powerDelta = InputManager.GetPowerChange() * (float)gameTime.ElapsedGameTime.TotalSeconds * 50f;
                        if (powerDelta != 0) activeTank.AdjustPower(powerDelta);

                        if (InputManager.IsKeyPressed(Keys.Space))
                        {
                            _gameManager.Fire();
                        }
                    }
                }

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
                break;

            case GameState.Shop:
                _shopManager.Update();
                if (_shopManager.IsFinished)
                {
                    _gameManager.StartNextRound();
                    _gameState = GameState.Playing;
                }
                break;

            case GameState.Options:
                if (InputManager.IsKeyPressed(Keys.Escape))
                {
                    _gameState = GameState.Menu;
                }
                break;
        }

        _debugManager.Update();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        switch (_gameState)
        {
            case GameState.Menu:
                _menuManager.Draw(_spriteBatch, _font, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                break;

            case GameState.Setup:
                _setupManager.Draw(_spriteBatch, _font, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                break;

            case GameState.Playing:
                _gameManager.Draw(_spriteBatch, _font);
                break;

            case GameState.Shop:
                _shopManager.Draw(_spriteBatch, _font, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                break;

            case GameState.Options:
                _spriteBatch.DrawString(_font, "Options Placeholder", new Vector2(100, 100), Color.White);
                _spriteBatch.DrawString(_font, "Press Escape to Return", new Vector2(100, 130), Color.White);
                break;
        }

        _debugManager.Draw(_spriteBatch, _font);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}