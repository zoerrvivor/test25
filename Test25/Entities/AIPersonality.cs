using System;
using System.Collections.Generic;
using Test25.Utilities;

namespace Test25.Entities
{
    public enum TargetPreference
    {
        Closest,
        Weakest,
        Random,
        Strongest
    }

    public enum WeaponPreference
    {
        Balanced,
        Aggressive, // Loves big damage (Nuke)
        Chaos,      // Loves weird stuff (MIRV, Roller)
        Conservative // Sticks to standard/infinite mostly
    }

    public class AIPersonality
    {
        public string Name { get; set; }
        public float AimError { get; set; } // Radians
        public float PowerError { get; set; }
        public TargetPreference TargetPreference { get; set; }
        public WeaponPreference WeaponPreference { get; set; }

        public static AIPersonality Sniper => new AIPersonality
        {
            Name = "Sniper",
            AimError = 0.02f,
            PowerError = 2.0f,
            TargetPreference = TargetPreference.Weakest,
            WeaponPreference = WeaponPreference.Balanced
        };

        public static AIPersonality Aggressive => new AIPersonality
        {
            Name = "Aggressive",
            AimError = 0.1f,
            PowerError = 8.0f,
            TargetPreference = TargetPreference.Closest,
            WeaponPreference = WeaponPreference.Aggressive
        };

        public static AIPersonality Chaotic => new AIPersonality
        {
            Name = "Chaotic",
            AimError = 0.2f,
            PowerError = 15.0f,
            TargetPreference = TargetPreference.Random,
            WeaponPreference = WeaponPreference.Chaos
        };

        public static AIPersonality Average => new AIPersonality
        {
            Name = "Average",
            AimError = 0.08f,
            PowerError = 5.0f,
            TargetPreference = TargetPreference.Closest,
            WeaponPreference = WeaponPreference.Balanced
        };

        public static AIPersonality Random => new AIPersonality
        {
            Name = "Random",
            // Other properties don't matter as this is a placeholder
        };

        public static List<AIPersonality> All => new List<AIPersonality>
        {
            Random,
            Sniper,
            Aggressive,
            Chaotic,
            Average
        };

        public static AIPersonality GetRandom()
        {
            int roll = Rng.Instance.Next(0, 4);
            switch (roll)
            {
                case 0: return Sniper;
                case 1: return Aggressive;
                case 2: return Chaotic;
                default: return Average;
            }
        }
    }
}
