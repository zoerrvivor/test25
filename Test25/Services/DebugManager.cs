// Version: 0.1

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Test25.Gameplay.Entities;
using Test25.Gameplay.Managers;

namespace Test25.Services
{
    public class DebugAction
    {
        public string Name { get; set; }
        public System.Action<GameManager> Action { get; set; }

        public DebugAction(string name, System.Action<GameManager> action)
        {
            Name = name;
            Action = action;
        }
    }

    public class DebugManager
    {
        private GameManager _gameManager;
        private List<DebugAction> _actions;
        private int _selectedIndex;
        public bool IsActive { get; private set; }

        public DebugManager(GameManager gameManager)
        {
            _gameManager = gameManager;
            _actions = new List<DebugAction>
            {
                new DebugAction("Kill Active Tank", (gm) =>
                {
                    if (gm.Players.Count > 0)
                    {
                        gm.Players[gm.CurrentPlayerIndex].Health = 0;
                    }
                }),
                new DebugAction("Give $1000", (gm) =>
                {
                    if (gm.Players.Count > 0)
                    {
                        gm.Players[gm.CurrentPlayerIndex].Money += 1000;
                    }
                }),
                new DebugAction("Give Heavy Shells (5)", (gm) =>
                {
                    if (gm.Players.Count > 0)
                    {
                        gm.Players[gm.CurrentPlayerIndex]
                            .AddItem(new Weapon("Heavy Shell", "High damage projectile", 40f, 30f, 5));
                    }
                }),
                new DebugAction("Give Repair Kit", (gm) =>
                {
                    if (gm.Players.Count > 0)
                    {
                        gm.Players[gm.CurrentPlayerIndex].AddItem(new Item("Repair Kit", "Restores 50 Health",
                            ItemType.Active, (t) =>
                            {
                                t.Health += 50;
                                if (t.Health > 100) t.Health = 100;
                            }));
                    }
                }),
                new DebugAction("Give Parachute", (gm) =>
                {
                    if (gm.Players.Count > 0)
                    {
                        gm.Players[gm.CurrentPlayerIndex]
                            .AddItem(new Item("Parachute", "Prevents fall damage", ItemType.Passive, null));
                    }
                }),
                new DebugAction("Give All Weapons", (gm) =>
                {
                    if (gm.Players.Count > 0)
                    {
                        var player = gm.Players[gm.CurrentPlayerIndex];
                        player.AddItem(new Weapon("Nuke", "Big Boom", 80f, 60f, 5));
                        player.AddItem(new Weapon("MIRV", "Splits in air", 20f, 20f, 5, false, ProjectileType.Mirv, 3));
                        player.AddItem(new Weapon("Dirt Clod", "Adds terrain", 10f, 30f, 5, false,
                            ProjectileType.Dirt));
                        player.AddItem(new Weapon("Roller", "Rolls on ground", 30f, 30f, 5, false,
                            ProjectileType.Roller));
                        player.AddItem(new Weapon("Laser", "Destroys terrain", 50f, 5.0f, 5, false,
                            ProjectileType.Laser));
                    }
                }),
                new DebugAction("Next Turn", (gm) => { gm.NextTurn(); })
            };
        }

        public void Update()
        {
            // Toggle Debug Menu with F1
            if (InputManager.IsKeyPressed(Keys.F1))
            {
                IsActive = !IsActive;
            }

            if (!IsActive) return;

            if (InputManager.IsKeyPressed(Keys.Down))
            {
                _selectedIndex++;
                if (_selectedIndex >= _actions.Count) _selectedIndex = 0;
            }

            if (InputManager.IsKeyPressed(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0) _selectedIndex = _actions.Count - 1;
            }

            if (InputManager.IsKeyPressed(Keys.Enter))
            {
                _actions[_selectedIndex].Action(_gameManager);
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsActive) return;

            Vector2 position = new Vector2(10, 200);

            // Draw background (optional, but good for readability)
            // For now just text

            spriteBatch.DrawString(font, "--- DEBUG MENU ---", position, Color.Red);
            position.Y += 20;

            for (int i = 0; i < _actions.Count; i++)
            {
                Color color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, _actions[i].Name, position, color);
                position.Y += 20;
            }
        }
    }
}
