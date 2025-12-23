using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.UI.Controls;
using Test25.Services;

namespace Test25.UI
{
    public class GuiManager
    {
        private List<GuiElement> _elements;

        public GuiElement FocusedElement { get; private set; }

        public GuiManager()
        {
            _elements = new List<GuiElement>();
            InputManager.OnTextInput += HandleTextInput;
        }

        private void HandleTextInput(char character, Microsoft.Xna.Framework.Input.Keys key)
        {
            FocusedElement?.HandleTextInput(character, key);
        }

        public void AddElement(GuiElement element)
        {
            _elements.Add(element);
        }

        public void RemoveElement(GuiElement element)
        {
            _elements.Remove(element);
            if (FocusedElement == element) FocusedElement = null; // Prevent leak
        }

        public void Clear()
        {
            _elements.Clear();
            FocusedElement = null;
        }

        public void Update(GameTime gameTime)
        {
            bool clickedThisFrame = InputManager.IsMouseClicked();

            // Update in reverse order so top-most elements handle input first if we implemented blocking
            // For now, standard update
            for (int i = _elements.Count - 1; i >= 0; i--)
            {
                var element = _elements[i];
                element.Update(gameTime);

                if (clickedThisFrame && element.Bounds.Contains(InputManager.GetMousePosition()) && element.IsVisible &&
                    element.IsActive)
                {
                    // Unfocus old
                    if (FocusedElement != null) FocusedElement.IsFocused = false;

                    // Focus new
                    FocusedElement = element;
                    FocusedElement.IsFocused = true;

                    clickedThisFrame = false; // Consume click
                    break;
                }
            }

            // Clicked outside any element? (Rough check, assumes full screen coverage or similar)
            // A more robust UI system would block clicks on elements and if no element handled click, clear focus.
            // For simple usage: if clicked and no element claimed it? 
            // Actually, individual elements handle their own click logic in Update via InputManager. 
            // We can just rely on setting focus above. If user clicks "nothing", we might want to defocus?
            // Let's keep it simple: clicking an element focuses it. Clicking empty space doesn't necessarily defocus unless we check.
            if (clickedThisFrame)
            {
                bool hitAny = false;
                foreach (var el in _elements)
                {
                    if (el.IsVisible && el.IsActive && el.Bounds.Contains(InputManager.GetMousePosition()))
                    {
                        hitAny = true;
                        break;
                    }
                }

                if (!hitAny && FocusedElement != null)
                {
                    FocusedElement.IsFocused = false;
                    FocusedElement = null;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var element in _elements)
            {
                element.Draw(spriteBatch);
            }
        }
    }
}
