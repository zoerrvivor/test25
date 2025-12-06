// Version: 0.1
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Test25.Entities;

namespace Test25.Managers
{
    public class ShopItem
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }
        public System.Action<Tank> OnPurchase { get; set; }

        public ShopItem(string name, int cost, string description, System.Action<Tank> onPurchase)
        {
            Name = name;
            Cost = cost;
            Description = description;
            OnPurchase = onPurchase;
        }
    }

    public class ShopManager
    {
        private List<ShopItem> _items;
        private int _selectedIndex;
        private GameManager _gameManager;
        private int _currentPlayerIndex;
        private bool _allPlayersReady;

        public bool IsFinished => _allPlayersReady;

        public ShopManager(GameManager gameManager)
        {
            _gameManager = gameManager;
            _items = new List<ShopItem>
            {
                new ShopItem("Repair Kit", 100, "Restores 50 Health", (t) => {
                    t.Health += 50;
                    if (t.Health > 100) t.Health = 100;
                }),
                new ShopItem("Parachute", 100, "Prevents fall damage (1 use)", (t) => {
                    t.AddItem(new Item("Parachute", "Prevents fall damage", ItemType.Passive, null));
                }),
                new ShopItem("Heavy Shell", 150, "High Damage (5 shots)", (t) => {
                    t.AddItem(new Weapon("Heavy Shell", "High damage projectile", 40f, 30f, 5));
                }),
                new ShopItem("Power Booster", 200, "+10% Damage", (t) => {
                    t.DamageMultiplier += 0.1f;
                })
            };
        }

        public void StartShop()
        {
            _currentPlayerIndex = 0;
            _allPlayersReady = false;
            _selectedIndex = 0;
        }

        public void Update()
        {
            if (_allPlayersReady) return;

            var currentPlayer = _gameManager.Players[_currentPlayerIndex];

            if (InputManager.IsKeyPressed(Keys.Down))
            {
                _selectedIndex++;
                if (_selectedIndex > _items.Count) _selectedIndex = 0; // +1 for "Ready" button
            }
            if (InputManager.IsKeyPressed(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0) _selectedIndex = _items.Count;
            }

            if (InputManager.IsKeyPressed(Keys.Enter))
            {
                if (_selectedIndex == _items.Count) // Ready
                {
                    _currentPlayerIndex++;
                    if (_currentPlayerIndex >= _gameManager.Players.Count)
                    {
                        _allPlayersReady = true;
                    }
                    else
                    {
                        _selectedIndex = 0; // Reset cursor for next player
                    }
                }
                else
                {
                    var item = _items[_selectedIndex];
                    if (currentPlayer.Money >= item.Cost)
                    {
                        currentPlayer.Money -= item.Cost;
                        item.OnPurchase(currentPlayer);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight)
        {
            if (_allPlayersReady) return;

            var currentPlayer = _gameManager.Players[_currentPlayerIndex];

            Vector2 position = new Vector2(100, 100);
            spriteBatch.DrawString(font, $"SHOP - {currentPlayer.Name}", position, currentPlayer.Color);
            position.Y += 30;
            spriteBatch.DrawString(font, $"Money: ${currentPlayer.Money}", position, Color.White);
            position.Y += 40;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                Color color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                if (currentPlayer.Money < item.Cost) color = Color.Gray;
                if (i == _selectedIndex && currentPlayer.Money < item.Cost) color = Color.DarkGoldenrod;

                spriteBatch.DrawString(font, $"{item.Name} (${item.Cost}) - {item.Description}", position, color);
                position.Y += 30;
            }

            position.Y += 20;
            Color readyColor = (_selectedIndex == _items.Count) ? Color.Yellow : Color.White;
            spriteBatch.DrawString(font, "Ready / Next Player", position, readyColor);
        }
    }
}
