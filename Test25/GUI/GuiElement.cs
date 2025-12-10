using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Test25.Managers;

namespace Test25.GUI
{
    public abstract class GuiElement
    {
        public Rectangle Bounds { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public GuiElement Parent { get; set; }

        public event Action<GuiElement> OnClick;
        public event Action<GuiElement> OnMouseEnter;
        public event Action<GuiElement> OnMouseLeave;

        protected bool _isHovered;

        public bool IsFocused { get; set; }

        public virtual void HandleTextInput(char character, Microsoft.Xna.Framework.Input.Keys key)
        {
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!IsActive || !IsVisible) return;

            Point mousePos = InputManager.GetMousePosition();
            bool currentlyHovered = Bounds.Contains(mousePos);

            if (currentlyHovered && !_isHovered)
            {
                _isHovered = true;
                OnMouseEnter?.Invoke(this);
            }
            else if (!currentlyHovered && _isHovered)
            {
                _isHovered = false;
                OnMouseLeave?.Invoke(this);
            }

            if (InputManager.IsMouseClicked())
            {
                if (_isHovered)
                {
                    OnClick?.Invoke(this);
                }
            }
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
