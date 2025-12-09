using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.GUI;

namespace Test25.Managers
{
    public class GuiManager
    {
        private List<GuiElement> _elements;

        public GuiManager()
        {
            _elements = new List<GuiElement>();
        }

        public void AddElement(GuiElement element)
        {
            _elements.Add(element);
        }

        public void RemoveElement(GuiElement element)
        {
            _elements.Remove(element);
        }

        public void Clear()
        {
            _elements.Clear();
        }

        public void Update(GameTime gameTime)
        {
            // Update in reverse order so top-most elements handle input first if we implemented blocking
            // For now, standard update
            for (int i = _elements.Count - 1; i >= 0; i--)
            {
                _elements[i].Update(gameTime);
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
