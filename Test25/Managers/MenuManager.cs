using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Test25.Managers
{
    public class MenuManager
    {
        private List<string> _menuItems;
        public int SelectedIndex { get; private set; }
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        private Texture2D _background;

        public MenuManager(Texture2D background)
        {
            _background = background;
            _menuItems = new List<string> { "Start New Game", "Options", "Exit" };
            SelectedIndex = 0;
        }

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            if (IsKeyPressed(Keys.Down))
            {
                SelectedIndex++;
                if (SelectedIndex >= _menuItems.Count)
                    SelectedIndex = 0;
            }

            if (IsKeyPressed(Keys.Up))
            {
                SelectedIndex--;
                if (SelectedIndex < 0)
                    SelectedIndex = _menuItems.Count - 1;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight)
        {
            // Draw Background
            if (_background != null)
            {
                spriteBatch.Draw(_background, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            }

            Vector2 position = new Vector2(screenWidth / 2, screenHeight / 2 - (_menuItems.Count * 30) / 2 + 100); // Shift menu down a bit

            for (int i = 0; i < _menuItems.Count; i++)
            {
                Color color = (i == SelectedIndex) ? Color.Yellow : Color.White;
                Vector2 size = font.MeasureString(_menuItems[i]);
                Vector2 origin = size / 2;

                spriteBatch.DrawString(font, _menuItems[i], position + new Vector2(0, i * 40), color, 0f, origin, 1f, SpriteEffects.None, 0f);
            }
        }

        public string GetSelectedItem()
        {
            return _menuItems[SelectedIndex];
        }

        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key);
        }
    }
}
