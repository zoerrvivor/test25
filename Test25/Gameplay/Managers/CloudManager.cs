// Version: 0.2

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Test25.Gameplay.World;
using Test25.Gameplay.Entities;

namespace Test25.Gameplay.Managers
{
    /// <summary>
    /// Manages multiple clouds moving across the sky.
    /// </summary>
    public class CloudManager
    {
        private List<Cloud> _clouds = new();
        private Texture2D _cloudTexture;
        private int _screenWidth;
        private int _screenHeight;
        private Random _rand = new();

        public CloudManager(Texture2D cloudTexture, int screenWidth, int screenHeight, int cloudCount = 5)
        {
            _cloudTexture = cloudTexture;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            // Initialize clouds at random positions and speeds
            for (int i = 0; i < cloudCount; i++)
            {
                var startPos = new Vector2(_rand.Next(0, _screenWidth),
                    _rand.Next(0, _screenHeight / Constants.CloudSpawnHeightDivisor));
                float speed = (float)_rand.NextDouble() * (Constants.CloudMaxSpeed - Constants.CloudMinSpeed) +
                              Constants.CloudMinSpeed;
                float scale = (float)_rand.NextDouble() * (Constants.CloudMaxScale - Constants.CloudMinScale) +
                              Constants.CloudMinScale;
                _clouds.Add(new Cloud(_cloudTexture, startPos, speed, scale, _screenWidth));
            }
        }

        public void Update(GameTime gameTime, float wind)
        {
            foreach (var cloud in _clouds)
                cloud.Update(gameTime, wind);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var cloud in _clouds)
                cloud.Draw(spriteBatch);
        }
    }
}
