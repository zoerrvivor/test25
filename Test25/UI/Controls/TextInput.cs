using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Test25.Utilities;
using Test25.Services;

namespace Test25.UI.Controls
{
    public class TextInput : GuiElement
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private Texture2D _backgroundTexture => GuiResources.WhiteTexture;

        public string Text { get; set; } = "";
        public int MaxLength { get; set; } = 15;
        public Color BackgroundColor { get; set; } = Color.White;
        public Color TextColor { get; set; } = Color.Black;
        public Color BorderColor { get; set; } = Color.Gray;
        public Color FocusedBorderColor { get; set; } = Color.Blue;

        public event Action<string> OnTextChanged;

        public TextInput(GraphicsDevice graphicsDevice, Rectangle bounds, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            Bounds = bounds;
            _font = font;
        }

        public override void HandleTextInput(char character, Keys key)
        {
            if (!IsActive || !IsVisible || !IsFocused) return;

            if (key == Keys.Back)
            {
                if (Text.Length > 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);
                    OnTextChanged?.Invoke(Text);
                }
            }
            else if (key == Keys.Enter)
            {
                // Optional: Handle Enter (maybe lose focus?)
            }
            else
            {
                // Basic filtering: letters, digits, space, punctuation
                if (!char.IsControl(character) && Text.Length < MaxLength && _font.Characters.Contains(character))
                {
                    Text += character;
                    OnTextChanged?.Invoke(Text);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            Color currentBorder = IsFocused ? FocusedBorderColor : BorderColor;

            // Draw Border (slightly larger)
            spriteBatch.Draw(_backgroundTexture,
                new Rectangle(Bounds.X - 2, Bounds.Y - 2, Bounds.Width + 4, Bounds.Height + 4), currentBorder);

            // Draw Background
            spriteBatch.Draw(_backgroundTexture, Bounds, BackgroundColor);

            // Draw Text
            string display = Text + (IsFocused && (DateTime.Now.Millisecond % 1000 < 500) ? "|" : ""); // Cursor blink
            Vector2 size = _font.MeasureString(display);

            // Clip if too long? For now, we rely on MaxLength to keep it fitting
            Vector2 textPos = new Vector2(Bounds.X + 5, Bounds.Y + (Bounds.Height - size.Y) / 2);
            spriteBatch.DrawString(_font, display, textPos, TextColor);
        }
    }
}
