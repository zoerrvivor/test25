// Version: 0.2

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
        public bool IsAi { get; set; }
        public Entities.AiPersonality Personality { get; set; }

        public PlayerSetup(string name, Color color, bool isAI = false)
        {
            Name = name;
            Color = color;
            IsAi = isAI;
            Personality = Entities.AiPersonality.Random;
        }
    }

    public class MatchSettings
    {
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