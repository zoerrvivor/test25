// Version: 0.4

namespace Test25
{
    public static class Constants
    {
        // Physics
        public const float Gravity = 150f;
        public const float FallDamageThreshold = 100f; // Ab dieser Geschwindigkeit gibt es Schaden
        public const float FallDamageMultiplier = 0.5f;

        // Combat
        public const float PowerMultiplier = 10f; // Umrechnung von Power (0-100) in Geschwindigkeit

        // Cloud Settings
        public const float CloudMinSpeed = 10f;
        public const float CloudMaxSpeed = 40f;
        public const float CloudMinScale = 0.5f;
        public const float CloudMaxScale = 2.5f;
        public const int CloudSpawnHeightDivisor = 3;

        // Terrain Settings
        public const float WaterLevelRatio = 0.85f;
        public const int TerrainGenerationSize = 1025;
        public const float TerrainDisplacement = 350f;
        public const float TerrainRoughness = 0.5f;
        public const int TerrainMinHeight = 50;
        public const int TerrainMaxHeightOffset = 20;
        public const float WaterMargin = 50f;

        // Death Settings
        public const float DeathExplosionRadiusMin = 40f;

        // Let's define as Base + Variance or Min/Max. Use Min/Max for clarity.
        public const float DeathExplosionRadiusVariance = 60f;

        public const float DeathCookOffChance = 0.20f;
        public const int DeathDebrisCountMin = 5;
        public const int DeathDebrisCountMax = 7; // Exclusive?

        public const float DeathDebrisSpeedMin = 200f;
        public const float DeathDebrisSpeedVariance = 300f;

        public const float DeathDebrisExplosionMin = 20f;
        public const float DeathDebrisExplosionVariance = 40f;
    }
}