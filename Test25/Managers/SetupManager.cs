using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Test25.Managers
{
    public class SetupManager
    {
        public MatchSettings Settings { get; private set; }
        private int _selectedIndex = 0;

        // Colors to cycle through
        private readonly Color[] _availableColors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Cyan, Color.Violet, Color.HotPink, Color.Orange, Color.White };

        // Name editing state
        private bool _isEditingName = false;
        private int _editingPlayerIndex = -1;
        private string _nameBuffer = "";

        public SetupManager()
        {
            Settings = new MatchSettings();
        }

        public void Update()
        {
            // If editing a name, capture characters
            if (_isEditingName)
            {
                // Finish editing on Enter
                if (InputManager.IsKeyPressed(Keys.Enter))
                {
                    Settings.Players[_editingPlayerIndex].Name = _nameBuffer;
                    _isEditingName = false;
                    _editingPlayerIndex = -1;
                    _nameBuffer = "";
                    return;
                }

                // Backspace handling
                if (InputManager.IsKeyPressed(Keys.Back) && _nameBuffer.Length > 0)
                {
                    _nameBuffer = _nameBuffer.Substring(0, _nameBuffer.Length - 1);
                }

                // Capture alphanumeric keys (A-Z, Space)
                var state = Keyboard.GetState();
                foreach (var key in state.GetPressedKeys())
                {
                    if (key == Keys.Enter || key == Keys.Back || key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down) continue;
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        char c = (char)('A' + (key - Keys.A));
                        _nameBuffer += c;
                    }
                    else if (key == Keys.Space)
                    {
                        _nameBuffer += ' ';
                    }
                }
                // Skip other updates while editing
                return;
            }

            // Navigation
            if (InputManager.IsKeyPressed(Keys.Down)) _selectedIndex++;
            if (InputManager.IsKeyPressed(Keys.Up)) _selectedIndex--;

            // Total menu items (Start, Wall, Rounds, Add, Remove + each player)
            int totalItems = 5 + Settings.Players.Count;
            if (_selectedIndex < 0) _selectedIndex = totalItems - 1;
            if (_selectedIndex >= totalItems) _selectedIndex = 0;

            // Modify wall type or player color
            if (InputManager.IsKeyPressed(Keys.Right) || InputManager.IsKeyPressed(Keys.Left))
            {
                int direction = InputManager.IsKeyPressed(Keys.Right) ? 1 : -1;
                if (_selectedIndex == 1) // Wall type
                {
                    int newType = (int)Settings.WallType + direction;
                    if (newType < 0) newType = 2;
                    if (newType > 2) newType = 0;
                    Settings.WallType = (WallType)newType;
                }
                else if (_selectedIndex == 2) // Num Rounds
                {
                    Settings.NumRounds += direction;
                    if (Settings.NumRounds < 1) Settings.NumRounds = 1;
                    if (Settings.NumRounds > 99) Settings.NumRounds = 99;
                }
                else if (_selectedIndex >= 5) // Player color
                {
                    int playerIdx = _selectedIndex - 4;
                    if (playerIdx < Settings.Players.Count)
                    {
                        // Toggle Color or Type? Let's make Left/Right toggle Type if holding Shift? Or just cycle both?
                        // Let's make it simple: Left/Right changes Color.
                        // But we need to change Type too.
                        // Let's use A/D for Type and Left/Right for Color? Or just add another menu item?
                        // Or just cycle through: Human (Red) -> CPU (Red) -> Human (Blue) ... no that's annoying.
                        // Better: Left/Right changes Color. Space changes Type? Or Enter edits name?
                        // Let's use Tab to toggle Type?

                        int curIdx = Array.IndexOf(_availableColors, Settings.Players[playerIdx].Color);
                        if (curIdx == -1) curIdx = 0;
                        curIdx = (curIdx + direction + _availableColors.Length) % _availableColors.Length;
                        Settings.Players[playerIdx].Color = _availableColors[curIdx];
                    }
                }
            }

            // Toggle AI with Tab
            if (InputManager.IsKeyPressed(Keys.Tab))
            {
                if (_selectedIndex >= 5)
                {
                    int playerIdx = _selectedIndex - 5;
                    if (playerIdx < Settings.Players.Count)
                    {
                        Settings.Players[playerIdx].IsAI = !Settings.Players[playerIdx].IsAI;
                    }
                }
            }

            // Actions: Add, Remove, Edit name
            if (InputManager.IsKeyPressed(Keys.Enter))
            {
                if (_selectedIndex == 3) // Add player
                {
                    if (Settings.Players.Count < 8)
                    {
                        Settings.Players.Add(new PlayerSetup($"Player {Settings.Players.Count + 1}", _availableColors[Settings.Players.Count % _availableColors.Length]));
                    }
                }
                else if (_selectedIndex == 3) // Add player
                {
                    if (Settings.Players.Count < 8)
                    {
                        Settings.Players.Add(new PlayerSetup($"Player {Settings.Players.Count + 1}", _availableColors[Settings.Players.Count % _availableColors.Length]));
                    }
                }
                else if (_selectedIndex == 4) // Remove player
                {
                    if (Settings.Players.Count > 2)
                    {
                        Settings.Players.RemoveAt(Settings.Players.Count - 1);
                        if (_selectedIndex >= 5 + Settings.Players.Count)
                        {
                            _selectedIndex--;
                        }
                    }
                }
                else if (_selectedIndex >= 5) // Edit player name
                {
                    _editingPlayerIndex = _selectedIndex - 5;
                    _isEditingName = true;
                    _nameBuffer = Settings.Players[_editingPlayerIndex].Name;
                }
            }
        }

        public bool IsStartSelected()
        {
            return _selectedIndex == 0 && InputManager.IsKeyPressed(Keys.Enter);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight)
        {
            // Position menu at bottom of screen
            int totalItems = 5 + Settings.Players.Count;
            float startY = screenHeight - (totalItems * 30) - 20; // 20px margin from bottom
            Vector2 position = new Vector2(screenWidth / 2, startY);

            DrawMenuItem(spriteBatch, font, "Start Game", 0, position);
            position.Y += 30;
            DrawMenuItem(spriteBatch, font, $"Wall Type: {Settings.WallType}", 1, position);
            position.Y += 30;
            DrawMenuItem(spriteBatch, font, $"Rounds: {Settings.NumRounds}", 2, position);
            position.Y += 30;
            DrawMenuItem(spriteBatch, font, "Add Player", 3, position);
            position.Y += 30;
            DrawMenuItem(spriteBatch, font, "Remove Player", 4, position);
            position.Y += 40;
            DrawMenuItem(spriteBatch, font, "(Tab to toggle Human/CPU)", -1, new Vector2(screenWidth / 2, screenHeight - 50), Color.Gray);
            position.Y += 20;

            for (int i = 0; i < Settings.Players.Count; i++)
            {
                string displayName = Settings.Players[i].Name;
                if (_isEditingName && i == _editingPlayerIndex)
                {
                    displayName = _nameBuffer + "_"; // cursor placeholder
                }
                string type = Settings.Players[i].IsAI ? "CPU" : "Human";
                DrawMenuItem(spriteBatch, font, $"{displayName} < {GetColorName(Settings.Players[i].Color)} > [{type}]", 5 + i, position, Settings.Players[i].Color);
                position.Y += 30;
            }
        }

        private void DrawMenuItem(SpriteBatch spriteBatch, SpriteFont font, string text, int index, Vector2 position, Color? color = null)
        {
            Color drawColor = (index == _selectedIndex) ? Color.Yellow : (color ?? Color.White);
            Vector2 size = font.MeasureString(text);
            spriteBatch.DrawString(font, text, position - size / 2, drawColor);
        }

        private string GetColorName(Color c)
        {
            if (c == Color.Red) return "Red";
            if (c == Color.Blue) return "Blue";
            if (c == Color.Green) return "Green";
            if (c == Color.Yellow) return "Yellow";
            if (c == Color.Cyan) return "Cyan";
            if (c == Color.Violet) return "Violet";
            if (c == Color.HotPink) return "Pink";
            if (c == Color.Orange) return "Orange";
            if (c == Color.White) return "White";
            return "Custom";
        }
    }
}
