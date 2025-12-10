// Version: 0.1

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Test25.Entities;
using Test25.GUI;

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
        private GameManager _gameManager;
        private int _currentPlayerIndex;
        private bool _allPlayersReady;

        private GuiManager _guiManager;
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;

        public bool IsFinished => _allPlayersReady;

        public ShopManager(GameManager gameManager, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _gameManager = gameManager;
            _graphicsDevice = graphicsDevice;
            _font = font;
            _guiManager = new GuiManager();

            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            _items = new List<ShopItem>
            {
                new ShopItem("Repair Kit", 100, "Restores 50 Health", (t) =>
                {
                    t.Health += 50;
                    if (t.Health > 100) t.Health = 100;
                }),
                new ShopItem("Parachute", 100, "Prevents fall damage (1 use)",
                    (t) => { t.AddItem(new Item("Parachute", "Prevents fall damage", ItemType.Passive, null)); }),
                new ShopItem("Heavy Shell", 150, "High Damage (5 shots)",
                    (t) => { t.AddItem(new Weapon("Heavy Shell", "High damage projectile", 40f, 30f, 5)); }),
                new ShopItem("Power Booster", 200, "+10% Damage", (t) => { t.DamageMultiplier += 0.1f; })
            };
        }

        public void StartShop()
        {
            _currentPlayerIndex = 0;
            _allPlayersReady = false;
            RebuildGui();
        }

        private void RebuildGui()
        {
            _guiManager.Clear();
            if (_allPlayersReady) return;

            var currentPlayer = _gameManager.Players[_currentPlayerIndex];

            // Background Panel
            int panelWidth = 600;
            int panelHeight = 500;
            Rectangle panelRect = new Rectangle(
                (_screenWidth - panelWidth) / 2,
                (_screenHeight - panelHeight) / 2,
                panelWidth,
                panelHeight
            );
            Panel bgPanel = new Panel(_graphicsDevice, panelRect);
            _guiManager.AddElement(bgPanel);

            // Title / Player Info
            Label title = new Label($"SHOP - {currentPlayer.Name}", _font,
                new Vector2(panelRect.X + 240, panelRect.Y + 20));
            title.TextColor = currentPlayer.Color;
            _guiManager.AddElement(title);

            Label moneyLabel = new Label($"Money: ${currentPlayer.Money}", _font,
                new Vector2(panelRect.X + 240, panelRect.Y + 50));
            _guiManager.AddElement(moneyLabel);

            // Items
            int startY = panelRect.Y + 100;
            int itemX = panelRect.X + 50;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                // Description Label
                string itemText = $"{item.Name} (${item.Cost}) - {item.Description}";
                Label itemLabel = new Label(itemText, _font, new Vector2(itemX, startY));
                _guiManager.AddElement(itemLabel);

                // Buy Button
                Button buyBtn = new Button(_graphicsDevice, new Rectangle(panelRect.Right - 150, startY, 100, 30),
                    "Buy", _font);

                if (currentPlayer.Money >= item.Cost)
                {
                    buyBtn.OnClick += (e) => BuyItem(item, currentPlayer);
                }
                else
                {
                    buyBtn.BackgroundColor = Color.Gray;
                    buyBtn.HoverColor = Color.Gray;
                    buyBtn.Text = "No Funds";
                }

                _guiManager.AddElement(buyBtn);

                startY += 50;
            }

            // Next / Ready Button
            Button nextBtn = new Button(_graphicsDevice,
                new Rectangle(panelRect.Right - 150, panelRect.Bottom - 60, 120, 40), "Ready", _font);
            nextBtn.BackgroundColor = Color.Green;
            nextBtn.OnClick += (e) => NextPlayer();
            _guiManager.AddElement(nextBtn);
        }

        private void BuyItem(ShopItem item, Tank player)
        {
            if (player.Money >= item.Cost)
            {
                player.Money -= item.Cost;
                item.OnPurchase(player);
                RebuildGui(); // Update GUI to reflect new money balance
            }
        }

        private void NextPlayer()
        {
            _currentPlayerIndex++;
            if (_currentPlayerIndex >= _gameManager.Players.Count)
            {
                _allPlayersReady = true;
            }
            else
            {
                RebuildGui();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!_allPlayersReady)
            {
                _guiManager.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight)
        {
            // Signature matches old interface to minimize easy breakage, but we ignore width/height args
            // properly we should update Game1 to call Draw(SpriteBatch) only.
            if (!_allPlayersReady)
            {
                _guiManager.Draw(spriteBatch);
            }
        }
    }
}
