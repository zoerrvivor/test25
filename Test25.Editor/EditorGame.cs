using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Test25.Core.Services;
using Test25.Core.UI.Controls;

namespace Test25.Editor;

public class EditorGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public EditorGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set higher resolution for Editor
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        Window.TextInput += (s, e) => InputManager.ReceiveTextInput(e.Character, e.Key);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Initialize Shared UI Resources
        GuiResources.Init(GraphicsDevice);

        _editorScreen = new Test25.Editor.Editor.EditorScreen(GraphicsDevice);
        _editorScreen.LoadContent(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _editorScreen.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Gray); // Different background for editor to distinguish

        _spriteBatch.Begin();

        // We need to pass the font to Draw for labels
        // EditorScreen.Draw signature was (SpriteBatch, SpriteFont)
        _editorScreen.Draw(_spriteBatch, Content.Load<SpriteFont>("Font"));

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Test25.Editor.Editor.EditorScreen _editorScreen;
}
