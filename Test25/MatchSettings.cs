// Version: 0.1
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Test25
{
    public enum WallType
    {
        Solid,
        Rubber, // Bouncy
        Wrap
    }

    public class PlayerSetup
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public bool IsAI { get; set; }

        public PlayerSetup(string name, Color color, bool isAI = false)
        {
            Name = name;
            Color = color;
            IsAI = isAI;
        }
    }

    public class MatchSettings
    {
        // Global Physics Constants
        public const float Gravity = 150f;

        public WallType WallType { get; set; } = WallType.Solid;
        public int NumRounds { get; set; } = 10;
        public List<PlayerSetup> Players { get; set; } = new List<PlayerSetup>();

        public MatchSettings()
        {
            // Defaults
            Players.Add(new PlayerSetup("Player 1", Color.Red));
            Players.Add(new PlayerSetup("Player 2", Color.Blue));
        }
    }
}