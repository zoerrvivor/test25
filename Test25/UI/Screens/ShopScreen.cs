using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Gameplay.Entities;
using Test25.Gameplay.Managers;
using Test25.UI.Controls;


namespace Test25.UI.Screens
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

    public class ShopScreen
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

        public ShopScreen(GameManager gameManager, GraphicsDevice graphicsDevice, SpriteFont font)
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
                    (t) =>
                    {
                        t.AddItem(new Weapon("Heavy Shell", "High damage projectile", 40f, 30f, 5, hasTrail: true));
                    }),
                new ShopItem("Nuke", 500, "Huge Explosion (1 shot)",
                    (t) => { t.AddItem(new Weapon("Nuke", "Big Boom", 80f, 60f, 1, hasTrail: true)); }),
                new ShopItem("MIRV", 300, "Splits in air (3 shots)",
                    (t) =>
                    {
                        t.AddItem(new Weapon("MIRV", "Splits in air", 20f, 20f, 3, false, ProjectileType.Mirv, 3,
                            hasTrail: true));
                    }),
                new ShopItem("Dirt Clod", 100, "Adds terrain (5 shots)",
                    (t) =>
                    {
                        t.AddItem(new Weapon("Dirt Clod", "Adds terrain", 10f, 30f, 5, false, ProjectileType.Dirt, 0,
                            false));
                    }),
                new ShopItem("Roller", 200, "Rolls on ground (5 shots)",
                    (t) =>
                    {
                        t.AddItem(new Weapon("Roller", "Rolls on ground", 30f, 30f, 5, false, ProjectileType.Roller, 0,
                            true));
                    }),
                new ShopItem("Laser", 400, "Destroys terrain (3 shots)",
                    (t) =>
                    {
                        t.AddItem(new Weapon("Laser", "Destroys terrain", 50f, 5.0f, 3, false, ProjectileType.Laser, 0,
                            false));
                    }),
                new ShopItem("Drone", 300, "Homing Attack (3 shots)",
                    (t) =>
                    {
                        t.AddItem(new Weapon("Drone", "Homing Attack", 40f, 40f, 3, false, ProjectileType.Drone, 0,
                            true));
                    }),
                new ShopItem("Power Booster", 200, "+10% Damage", (t) => { t.DamageMultiplier += 0.1f; })
            };
        }

        public void StartShop()
        {
            _currentPlayerIndex = 0;
            _allPlayersReady = false;
            RebuildGui();
        }

        public void OnResize(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _font = font;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            RebuildGui();
        }

        private void RebuildGui()
        {
            _guiManager.Clear();
            if (_allPlayersReady) return;

            var currentPlayer = _gameManager.Players[_currentPlayerIndex];

            // AI Check
            if (currentPlayer.IsAi)
            {
                HandleAiPurchase(currentPlayer);
                // After purchase, move to next player
                NextPlayer();
                return;
            }

            // Background Panel
            int panelWidth = 700;
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
                new Vector2(panelRect.X + 20, panelRect.Y + 20));
            title.TextColor = currentPlayer.Color;
            _guiManager.AddElement(title);

            Label moneyLabel = new Label($"Money: ${currentPlayer.Money}", _font,
                new Vector2(panelRect.X + 20, panelRect.Y + 45));
            _guiManager.AddElement(moneyLabel);

            // Items - Grid Layout (2 Columns)
            int startX = panelRect.X + 20;
            int startY = panelRect.Y + 80;
            int colWidth = 330;
            int rowHeight = 60;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                int row = i / 2;
                int col = i % 2;

                int itemX = startX + (col * colWidth);
                int itemY = startY + (row * rowHeight);

                // Item Name & Cost
                string nameCost = $"{item.Name} (${item.Cost})";
                Label nameLabel = new Label(nameCost, _font, new Vector2(itemX, itemY));
                nameLabel.TextColor = Constants.UiLabelHeaderColor;
                _guiManager.AddElement(nameLabel);

                // Description
                Label descLabel = new Label(item.Description, _font, new Vector2(itemX, itemY + 20));
                descLabel.Scale =
                    0.8f; // Smaller text for description? Or just normal. Let's keep normal for now but maybe lighter color?
                _guiManager.AddElement(descLabel);

                // Buy Button
                // Position button to the right of the column slot
                Button buyBtn = new Button(_graphicsDevice, new Rectangle(itemX + 230, itemY, 80, 25),
                    "Buy", _font);

                if (currentPlayer.Money >= item.Cost)
                {
                    buyBtn.OnClick += (e) => BuyItem(item, currentPlayer);
                }
                else
                {
                    buyBtn.BackgroundColor = Constants.UiButtonDisabledColor;
                    buyBtn.HoverColor = Constants.UiButtonDisabledColor;
                    buyBtn.Text = "Poor"; // Short text
                }

                _guiManager.AddElement(buyBtn);
            }

            // Next / Ready Button
            Button nextBtn = new Button(_graphicsDevice,
                new Rectangle(panelRect.Right - 150, panelRect.Bottom - 60, 120, 40), "Ready", _font);
            nextBtn.BackgroundColor = Constants.UiButtonActionColor;
            nextBtn.OnClick += (e) => NextPlayer();
            _guiManager.AddElement(nextBtn);
        }

        private void HandleAiPurchase(Tank aiTank)
        {
            // 1. Health Critical? (Below 60)
            if (aiTank.Health < 60)
            {
                var repair = _items.Find(i => i.Name == "Repair Kit");
                if (repair != null && aiTank.Money >= repair.Cost)
                {
                    BuyItem(repair, aiTank);
                    if (aiTank.Health >= 90) return; // Healthy enough, stop spending unless rich
                }
            }

            // 2. Personality Preference
            if (aiTank.Personality != null)
            {
                // Primary preference
                if (aiTank.Personality.WeaponPreference == WeaponPreference.Aggressive)
                {
                    // Aggressive likes Nuke, Laser, Damage Boost
                    if (TryBuyItem(aiTank, "Nuke")) return;
                    if (TryBuyItem(aiTank, "Laser")) return;
                    if (TryBuyItem(aiTank, "Power Booster")) return;
                }
                else if (aiTank.Personality.WeaponPreference == WeaponPreference.Chaos)
                {
                    // Chaos likes MIRV, Roller, Drone
                    if (TryBuyItem(aiTank, "MIRV")) return;
                    if (TryBuyItem(aiTank, "Drone")) return;
                    if (TryBuyItem(aiTank, "Roller")) return;
                }

                // Sniper Logic (Weakest Target Pref) -> Likes Drone (Homing)
                if (aiTank.Personality.TargetPreference == TargetPreference.Weakest)
                {
                    if (TryBuyItem(aiTank, "Drone")) return;
                }

                // Balanced/Conservative Logic
                if (aiTank.Personality.WeaponPreference == WeaponPreference.Balanced ||
                    aiTank.Personality.WeaponPreference == WeaponPreference.Conservative)
                {
                    if (TryBuyItem(aiTank, "Heavy Shell")) return;
                    if (TryBuyItem(aiTank, "Repair Kit")) return;
                }
            }

            // 3. Excess Wealth Spending (If money > 600, just buy something fun)
            if (aiTank.Money > 600)
            {
                var options = _items.FindAll(i => i.Cost <= aiTank.Money);
                if (options.Count > 0)
                {
                    var randomItem = options[Utilities.Rng.Range(0, options.Count)];
                    BuyItem(randomItem, aiTank);
                }
            }
        }

        private bool TryBuyItem(Tank aiTank, string itemName)
        {
            var item = _items.Find(i => i.Name == itemName);
            if (item != null && aiTank.Money >= item.Cost)
            {
                BuyItem(item, aiTank);
                return true;
            }

            return false;
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

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_allPlayersReady)
            {
                _guiManager.Draw(spriteBatch);
            }
        }
    }
}
