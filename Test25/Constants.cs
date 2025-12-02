// Version: 0.3
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
        public const int DefaultCloudCount = 5;
        public const float CloudMinSpeed = 10f;
        public const float CloudMaxSpeed = 40f;
        public const float CloudMinScale = 0.5f;
        public const float CloudMaxScale = 2.5f;
        public const int CloudSpawnHeightDivisor = 3;
    }
}