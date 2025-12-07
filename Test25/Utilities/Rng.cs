using System;

namespace Test25.Utilities
{
    public static class Rng
    {
        private static Random _instance = new Random();
        public static Random Instance => _instance;

        /// <summary>
        /// Returns a random float between min (inclusive) and max (exclusive).
        /// </summary>
        public static float Range(float min, float max)
        {
            return min + (float)_instance.NextDouble() * (max - min);
        }

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (exclusive).
        /// </summary>
        public static int Range(int min, int max)
        {
            return _instance.Next(min, max);
        }
    }
}
