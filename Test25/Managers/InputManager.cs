// Version: 0.1

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Test25.Managers
{
    public static class InputManager
    {
        private static KeyboardState _currentKeyboardState;
        private static KeyboardState _previousKeyboardState;

        private static MouseState _currentMouseState;
        private static MouseState _previousMouseState;

        public static void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }

        public static bool IsKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        public static bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }

        // Mouse Helpers
        public static bool IsMouseClicked()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed &&
                   _previousMouseState.LeftButton == ButtonState.Released;
        }

        public static bool IsMouseDown()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public static Point GetMousePosition()
        {
            return _currentMouseState.Position;
        }

        // Example helper for tank controls
        public static float GetTurretMovement()
        {
            if (_currentKeyboardState.IsKeyDown(Keys.Left)) return -1f;
            if (_currentKeyboardState.IsKeyDown(Keys.Right)) return 1f;
            return 0f;
        }

        public static float GetPowerChange()
        {
            if (_currentKeyboardState.IsKeyDown(Keys.Up)) return 1f;
            if (_currentKeyboardState.IsKeyDown(Keys.Down)) return -1f;
            return 0f;
        }
    }
}
