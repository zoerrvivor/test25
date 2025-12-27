using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Test25.Core.Gameplay

{
    public static class LevelService
    {
        private static string SavePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels");

        public static void SaveLevel(LevelData data)
        {
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            string filePath = Path.Combine(SavePath, $"{data.Name}.json");
            File.WriteAllText(filePath, json);
        }

        public static LevelData LoadLevel(string name)
        {
            string filePath = Path.Combine(SavePath, $"{name}.json");
            if (!File.Exists(filePath)) return null;

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<LevelData>(json);
        }

        public static List<string> GetLevelList()
        {
            if (!Directory.Exists(SavePath)) return new List<string>();

            var files = Directory.GetFiles(SavePath, "*.json");
            var names = new List<string>();
            foreach (var f in files)
            {
                names.Add(Path.GetFileNameWithoutExtension(f));
            }

            return names;
        }
    }
}
