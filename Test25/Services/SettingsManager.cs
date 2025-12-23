using System.IO;
using System.Text.Json;

namespace Test25.Services
{
    public class GameSettings
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 0.5f;
        public float SfxVolume { get; set; } = 1.0f;

        public int ResolutionWidth { get; set; } = 800;
        public int ResolutionHeight { get; set; } = 600;
        public bool IsFullScreen { get; set; }
    }

    public static class SettingsManager
    {
        private static string _filePath = "settings.json";

        // Static properties for easy access
        public static int ResolutionWidth { get; set; } = 800;
        public static int ResolutionHeight { get; set; } = 600;
        public static bool IsFullScreen { get; set; }

        public static void Save()
        {
            var settings = new GameSettings
            {
                MasterVolume = SoundManager.MasterVolume,
                MusicVolume = SoundManager.MusicVolume,
                SfxVolume = SoundManager.SfxVolume,
                ResolutionWidth = ResolutionWidth,
                ResolutionHeight = ResolutionHeight,
                IsFullScreen = IsFullScreen
            };

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public static void Load()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    GameSettings settings = JsonSerializer.Deserialize<GameSettings>(json);

                    if (settings != null)
                    {
                        SoundManager.MasterVolume = settings.MasterVolume;
                        SoundManager.MusicVolume = settings.MusicVolume;
                        SoundManager.SfxVolume = settings.SfxVolume;

                        ResolutionWidth = settings.ResolutionWidth;
                        ResolutionHeight = settings.ResolutionHeight;
                        IsFullScreen = settings.IsFullScreen;

                        // Apply immediately (Volume reset)
                        Microsoft.Xna.Framework.Media.MediaPlayer.Volume =
                            SoundManager.MusicVolume * SoundManager.MasterVolume;
                    }
                }
                catch
                {
                    // Ignore errors, start with defaults
                }
            }
        }
    }
}
