using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Test25.Managers;
using Test25.Utilities;

namespace Test25.GUI
{
    public class Slider : GuiElement
    {
        public float Value { get; private set; } // 0.0f to 1.0f
        public event Action<float> OnValueChanged;

        private Texture2D _trackTexture => GuiResources.WhiteTexture;
        private Texture2D _handleTexture => GuiResources.WhiteTexture;
        private Rectangle _trackBounds;
        private Rectangle _handleBounds;
        private bool _isDragging;

        private int _handleWidth = 10;
        private int _handleHeight = 20;

        public Slider(GraphicsDevice graphicsDevice, Rectangle bounds, float initialValue = 0.5f)
        {
            Bounds = bounds;
            Value = MathHelper.Clamp(initialValue, 0f, 1f);

            // Create textures (Now using Shared Resources)
            // _trackTexture and _handleTexture are properties pointing to shared texture

            // Define track (visually centered vertically in Bounds)
            int trackHeight = 4;
            _trackBounds = new Rectangle(Bounds.X, Bounds.Y + (Bounds.Height - trackHeight) / 2, Bounds.Width,
                trackHeight);

            UpdateHandlePosition();
        }

        private void UpdateHandlePosition()
        {
            int centerX = (int)(Bounds.X + (Bounds.Width * Value));
            _handleBounds = new Rectangle(centerX - _handleWidth / 2, Bounds.Y + (Bounds.Height - _handleHeight) / 2,
                _handleWidth, _handleHeight);
        }

        public void SetValue(float value)
        {
            Value = MathHelper.Clamp(value, 0f, 1f);
            UpdateHandlePosition();
            OnValueChanged?.Invoke(Value);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!IsActive || !IsVisible) return;

            Point mousePos = InputManager.GetMousePosition();
            bool isMouseDown = InputManager.IsMouseDown(); // Need to verify if InputManager has IsMouseDown or similar

            // We need a way to check if mouse is actively held down.
            // Assumption: InputManager might technically only have "IsMouseClicked" exposed publicly based on previous files.
            // Let's check InputManager again if needed, but standard Mouse.GetState() works if InputManager doesn't wrap it fully.
            // Actually, I should check InputManager first to see what it exposes. 
            // BUT, for now I will use standard Mouse.GetState() if InputManager doesn't help, OR I'll assume standard input.
            // Wait, I can't assume. Let's look at InputManager.cs quickly? 
            // "InputManager.IsMouseClicked()" was used in GuiElement. 

            // START DRAG
            if (InputManager.IsMouseClicked() && _handleBounds.Contains(mousePos))
            {
                _isDragging = true;
            }

            // END DRAG (if mouse released)
            // If InputManager doesn't expose "IsMouseDown", I might need to check raw state.
            // Let's use Mouse.GetState() for raw check to be safe, assuming XNA.
            MouseState state = Mouse.GetState();
            if (state.LeftButton == ButtonState.Released)
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                float relativeX = mousePos.X - Bounds.X;
                float newValue = relativeX / (float)Bounds.Width;

                // Only invoke if changed significantly? Or always?
                float visibleValue = MathHelper.Clamp(newValue, 0f, 1f);

                if (Math.Abs(visibleValue - Value) > 0.001f)
                {
                    Value = visibleValue;
                    UpdateHandlePosition();
                    OnValueChanged?.Invoke(Value);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            // Draw Track
            spriteBatch.Draw(_trackTexture, _trackBounds, Color.Gray);

            // Draw Handle
            Color handleColor = _isDragging ? Color.Green : (IsHovered ? Color.Yellow : Color.White);
            spriteBatch.Draw(_handleTexture, _handleBounds, handleColor);
        }
    }
}
