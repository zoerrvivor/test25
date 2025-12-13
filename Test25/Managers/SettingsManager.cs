using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Test25.Managers
{
    public class GameSettings
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 0.5f;
        public float SfxVolume { get; set; } = 1.0f;
    }

    public static class SettingsManager
    {
        private static string _filePath = "settings.json";

        public static void Save()
        {
            var settings = new GameSettings
            {
                MasterVolume = SoundManager.MasterVolume,
                MusicVolume = SoundManager.MusicVolume,
                SfxVolume = SoundManager.SfxVolume
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
                        
                        // Apply immediately (Volume reset)
                        Microsoft.Xna.Framework.Media.MediaPlayer.Volume = SoundManager.MusicVolume * SoundManager.MasterVolume;
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
