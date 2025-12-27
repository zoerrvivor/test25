using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Core;
using Test25.Core.Gameplay;
using Test25.Core.Gameplay.Managers;
using Test25.Core.Gameplay.World;
using Test25.Core.Services;
using Test25.Core.UI;
using Test25.Core.UI.Controls;
using Test25.Core.Utilities;

namespace Test25.Editor.Editor;

public class EditorScreen
{
    private GuiManager _guiManager;
    private GraphicsDevice _graphicsDevice;

    // Panels
    private Panel _sidebarPanel;
    private Panel _propertiesPanel;
    private Panel _terrainPanel;
    private Panel _optionsPanel;

    // Game Components
    private GameManager _gameManager;
    private Terrain _terrain;
    private Camera _camera;
    private Texture2D _dummyTexture;

    private int _currentSeed = 12345;
    private WallType _wallType = WallType.Solid;
    private string _currentTool = "Tank";
    private string _levelName = "New Level";

    public EditorScreen(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _guiManager = new GuiManager();

        InitializeLayout();
        InitializeGameWorld();
    }

    private void InitializeLayout()
    {
        int screenWidth = _graphicsDevice.Viewport.Width;
        int screenHeight = _graphicsDevice.Viewport.Height;

        // Layout Constants
        int SidebarWidth = 250;
        int PropertiesWidth = 250;

        // Sidebar (Items)
        _sidebarPanel = new Panel(_graphicsDevice, new Rectangle(0, 0, SidebarWidth, screenHeight / 2));
        _sidebarPanel.BackgroundColor = new Color(50, 50, 50);
        _sidebarPanel.BorderThickness = 2;
        _guiManager.AddElement(_sidebarPanel);

        // Terrain Panel (Below Sidebar)
        _terrainPanel = new Panel(_graphicsDevice, new Rectangle(0, screenHeight / 2, SidebarWidth, screenHeight / 4));
        _terrainPanel.BackgroundColor = new Color(60, 60, 60);
        _terrainPanel.BorderThickness = 2;
        _guiManager.AddElement(_terrainPanel);

        // Options Panel (Below Terrain)
        _optionsPanel = new Panel(_graphicsDevice,
            new Rectangle(0, screenHeight * 3 / 4, SidebarWidth, screenHeight / 4));
        _optionsPanel.BackgroundColor = new Color(55, 55, 55);
        _optionsPanel.BorderThickness = 2;
        _guiManager.AddElement(_optionsPanel);

        // Properties Panel (Right)
        _propertiesPanel = new Panel(_graphicsDevice,
            new Rectangle(screenWidth - PropertiesWidth, 0, PropertiesWidth, screenHeight));
        _propertiesPanel.BackgroundColor = new Color(50, 50, 60);
        _propertiesPanel.BorderThickness = 2;
        _guiManager.AddElement(_propertiesPanel);
    }

    private void InitializeGameWorld()
    {
        int sw = _graphicsDevice.Viewport.Width;
        int sh = _graphicsDevice.Viewport.Height;

        _camera = new Camera(sw, sh);
        _terrain = new Terrain(_graphicsDevice, sw, sh);

        // Note: GameManager needs textures. We will load them in LoadContent.
    }

    public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
    {
        SpriteFont font = content.Load<SpriteFont>("Font");

        // --- Items Sidebar ---
        int itemBtnY = 40;
        int itemBtnGap = 45;
        _guiManager.AddElement(new Label("ITEMS", font, new Vector2(10, 10)) { TextColor = Color.White });

        string[] items = { "Tank", "Crate", "Tree", "Building" };
        foreach (var item in items)
        {
            Button btn = new Button(_graphicsDevice, new Rectangle(10, itemBtnY, 230, 40), item, font);
            btn.OnClick += (e) => { _currentTool = item; };
            _guiManager.AddElement(btn);
            itemBtnY += itemBtnGap;
        }

        // --- Terrain Panel ---
        int ty = _terrainPanel.Bounds.Y;
        _guiManager.AddElement(new Label("TERRAIN", font, new Vector2(10, ty + 10)) { TextColor = Color.White });

        TextInput seedInput = new TextInput(_graphicsDevice, new Rectangle(10, ty + 40, 150, 30), font)
            { Text = _currentSeed.ToString() };
        seedInput.OnTextChanged += (text) => { int.TryParse(text, out _currentSeed); };
        _guiManager.AddElement(seedInput);

        Button btnRandom = new Button(_graphicsDevice, new Rectangle(170, ty + 40, 70, 30), "RAND", font);
        btnRandom.OnClick += (e) =>
        {
            _currentSeed = new System.Random().Next(1000000);
            seedInput.Text = _currentSeed.ToString();
        };
        _guiManager.AddElement(btnRandom);

        Button btnGenerate = new Button(_graphicsDevice, new Rectangle(10, ty + 80, 230, 40), "GENERATE", font);
        btnGenerate.OnClick += (e) => { _terrain.Generate(_currentSeed); };
        _guiManager.AddElement(btnGenerate);

        // --- Options Panel ---
        int oy = _optionsPanel.Bounds.Y;
        TextInput nameInput = new TextInput(_graphicsDevice, new Rectangle(10, oy + 60, 230, 30), font)
            { Text = _levelName };
        nameInput.OnTextChanged += (text) => { _levelName = text; };
        _guiManager.AddElement(nameInput);

        Button btnSave = new Button(_graphicsDevice, new Rectangle(10, oy + 100, 110, 30), "SAVE", font);
        btnSave.OnClick += (e) => { SaveLevel(); };
        _guiManager.AddElement(btnSave);

        Button btnLoad = new Button(_graphicsDevice, new Rectangle(130, oy + 100, 110, 30), "LOAD", font);
        btnLoad.OnClick += (e) => { LoadLevel(); };
        _guiManager.AddElement(btnLoad);

        Button btnWallType = new Button(_graphicsDevice, new Rectangle(10, oy + 140, 230, 40), $"Walls: {_wallType}",
            font);
        btnWallType.OnClick += (e) =>
        {
            _wallType = (WallType)(((int)_wallType + 1) % 3);
            btnWallType.Text = $"Walls: {_wallType}";
        };
        _guiManager.AddElement(btnWallType);

        // --- Properties Panel ---
        _guiManager.AddElement(new Label("PROPERTIES", font, new Vector2(_propertiesPanel.Bounds.X + 10, 10))
            { TextColor = Color.White });
        _guiManager.AddElement(new Label("No selection", font, new Vector2(_propertiesPanel.Bounds.X + 10, 40))
            { Scale = 0.8f, TextColor = Color.Gray });

        // Load Game Resources
        var tankBody = content.Load<Texture2D>("Images/tank_body");
        var tankBarrel = content.Load<Texture2D>("Images/tank_gun_barrel");
        _dummyTexture = TextureGenerator.CreateSolidColorTexture(_graphicsDevice, 4, 4, Color.White);

        var decorationTextures = new List<Texture2D>();
        try
        {
            decorationTextures.Add(content.Load<Texture2D>("Images/building_ruins"));
        }
        catch
        {
        }

        _gameManager = new GameManager(_terrain, _dummyTexture, tankBody, tankBarrel,
            decorationTextures, _camera);
        _gameManager.LoadContent(content);

        // Start a dummy game or just init terrain
        _gameManager.StartGame(new MatchSettings());
    }

    public void Update(GameTime gameTime)
    {
        _guiManager.Update(gameTime);
        _camera.Update(gameTime);

        UpdatePlacement();

        _gameManager?.Update(gameTime);
    }

    private void UpdatePlacement()
    {
        if (InputManager.IsMouseClicked())
        {
            var mousePos = InputManager.GetMousePosition();

            // Check if mouse is in preview area (roughly outside sidebars)
            // Layout: Sidebar (250px left), Properties (250px right)
            if (mousePos.X > 250 && mousePos.X < _graphicsDevice.Viewport.Width - 250)
            {
                // Convert screen mouse to world mouse
                var worldMouse = _camera.ScreenToWorld(mousePos.ToVector2());
                _gameManager.SpawnEntity(_currentTool, worldMouse);
            }
        }
    }

    private void SaveLevel()
    {
        var data = new LevelData
        {
            Name = _levelName,
            Seed = _currentSeed,
            WallType = _wallType,
            TerrainRoughness = Constants.TerrainRoughness, // Default for now
            TerrainDisplacement = Constants.TerrainDisplacement,
            Entities = new List<PlacedEntity>()
        };

        foreach (var tank in _gameManager.Players)
        {
            data.Entities.Add(new PlacedEntity { Type = "Tank", Position = tank.Position });
        }
        // TODO: Add decorations if they are tracked.
        // For now, let's just save tanks.

        LevelService.SaveLevel(data);
    }

    private void LoadLevel()
    {
        var data = LevelService.LoadLevel(_levelName);
        if (data != null)
        {
            _levelName = data.Name;
            _currentSeed = data.Seed;
            _wallType = data.WallType;

            _gameManager.ClearWorld();
            _terrain.Generate(_currentSeed);

            foreach (var ent in data.Entities)
            {
                _gameManager.SpawnEntity(ent.Type, ent.Position);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        // 1. Draw Game World (Preview)
        // We want to restrict drawing to the "Preview Area" eventally using ScissorRectangle.
        // For now, draw full screen, then UI covers it.
        var viewMatrix = _camera.GetViewMatrix();
        // End the UI spritebatch from EditorGame.Draw (wait, EditorGame calls Begin/End around EditorScreen.Draw)
        // We need to manage batches here.
        // EditorGame.Draw starts a batch. We should end it, draw world (with camera transform), then start UI batch.

        spriteBatch.End(); // End UI batch temporarily

        spriteBatch.Begin(transformMatrix: viewMatrix);
        _gameManager?.DrawWorld(spriteBatch, font, viewMatrix);
        spriteBatch.End();

        spriteBatch.Begin(); // Restart UI batch

        _guiManager.Draw(spriteBatch);
    }
}
