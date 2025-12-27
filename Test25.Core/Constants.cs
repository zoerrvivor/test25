// Version: 0.4

using Microsoft.Xna.Framework;

namespace Test25.Core
{
    public static class Constants
    {
        // ------ Physics ------//
        public const float Gravity = 150f; // the gravity of the game
        public const float FallDamageThreshold = 100f; // the threshold of fall damage
        public const float FallDamageMultiplier = 0.5f; // the multiplier of fall damage


        // ------ Combat ------ //
        public const float MaxHealth = 100f; // the maximum health of a tank
        public const float PowerMultiplier = 10f; // the multiplier of power
        public const float RollerMaxLifetime = 3f; // the maximum lifetime of the roller
        public const int ProjectileTrailLength = 30; // Number of trail points to store
        public const float ProjectileTrailFrequency = 0.05f; // How often to record (seconds)

        // ------ Smoke Effect Settings ------ //
        public const float SmokeHealthThreshold = 1.0f / 3.0f; // emit smoke below 33% health
        public const float SmokeEmissionChance = 0.15f; // reduced chance for more natural puffs


        // ------ Cloud Settings ------ //
        public const float ReferenceWidth = 1280f; // Reference resolution for scaling
        public const float CloudAmbientSpeed = 20f; // Base speed without wind
        public const float CloudMinSpeed = 10f; // the minimum speed of the cloud
        public const float CloudMaxSpeed = 40f; // the maximum speed of the cloud
        public const float CloudMinScale = 1f; // the minimum scale of the cloud
        public const float CloudMaxScale = 3f; // the maximum scale of the cloud
        public const int CloudSpawnHeightDivisor = 3; // the height of the cloud


        // ------ Terrain Settings ------ //
        public const float WaterLevelRatio = 0.85f; // the ratio of the water level
        public const int TerrainGenerationSize = 1025; // the size of the terrain
        public const float TerrainDisplacement = 350f; // the displacement of the terrain
        public const float TerrainRoughness = 0.5f; // the roughness of the terrain
        public const int TerrainMinHeight = 50; // the minimum height of the terrain
        public const int TerrainMaxHeightOffset = 20; // the maximum height offset of the terrain
        public const float WaterMargin = 50f; // the margin of the water


        // ------ Death Settings ------- //
        public const float DeathExplosionRadiusMin = 40f; // the minimum radius of the explosion
        public const float DeathExplosionRadiusVariance = 60f; // the variance of the explosion radius
        public const float DeathCookOffChance = 0.20f; // the chance of the tank cooking off
        public const int DeathDebrisCountMin = 5; // the minimum count of the debris
        public const int DeathDebrisCountMax = 7; // the maximum count of the debris
        public const float DeathDebrisSpeedMin = 200f; // the minimum speed of the debris
        public const float DeathDebrisSpeedVariance = 300f; // the variance of the debris speed
        public const float DeathDebrisExplosionMin = 20f; // the minimum explosion of the debris
        public const float DeathDebrisExplosionVariance = 40f; // the variance of the debris explosion


        // ----- Economy ------ //
        public const int KillReward = 200; // the amount of money given when killing a tank


        // ----- UI Effects ------ //
        public const float DamageNumberLifetime = 1.5f;
        public const float DamageNumberSpeed = 50f;
        public const float DamageNumberFadeTime = 0.5f;


        // ----- UI Colors ------ //
        public static readonly Color UiButtonColor = Color.Gray;
        public static readonly Color UiButtonHoverColor = Color.LightGray;
        public static readonly Color UiButtonTextColor = Color.White;
        public static readonly Color UiButtonDisabledColor = Color.Gray; // Or DarkGray
        public static readonly Color UiButtonActionColor = Color.Green;
        public static readonly Color UiPanelColor = new(0, 0, 0, 150); // gray
        public static readonly Color UiLabelColor = Color.White;
        public static readonly Color UiLabelHeaderColor = Color.Yellow;
    }
}