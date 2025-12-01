// Version: 0.1
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Test25.Managers
{
    public class DialogueManager
    {
        private readonly Dictionary<int, TankDialogue> _dialogues = new();
        private readonly Random _rand = new();
        private readonly string _basePath;

        public DialogueManager(string basePath)
        {
            _basePath = basePath; // e.g. Content/Dialogues
            LoadAll();
        }

        private void LoadAll()
        {
            // Load shoot lines
            foreach (var file in Directory.GetFiles(_basePath, "tank_*_shoot.txt"))
            {
                var id = ParseId(file);
                var lines = File.ReadAllLines(file);
                GetOrCreate(id).ShootLines.AddRange(lines);
            }
            // Load hit lines
            foreach (var file in Directory.GetFiles(_basePath, "tank_*_hit.txt"))
            {
                var id = ParseId(file);
                var lines = File.ReadAllLines(file);
                GetOrCreate(id).HitLines.AddRange(lines);
            }
        }

        private int ParseId(string path)
        {
            // Expected format: tank_{id}_shoot.txt or tank_{id}_hit.txt
            var name = Path.GetFileNameWithoutExtension(path);
            var parts = name.Split('_');
            if (parts.Length < 3) return -1;
            return int.Parse(parts[1]);
        }

        private TankDialogue GetOrCreate(int id)
        {
            if (!_dialogues.ContainsKey(id))
                _dialogues[id] = new TankDialogue();
            return _dialogues[id];
        }

        public string GetRandomShootPhrase(int tankId)
        {
            if (_dialogues.TryGetValue(tankId, out var dlg) && dlg.ShootLines.Count > 0)
                return dlg.ShootLines[_rand.Next(dlg.ShootLines.Count)];
            return null;
        }

        public string GetRandomHitPhrase(int tankId)
        {
            if (_dialogues.TryGetValue(tankId, out var dlg) && dlg.HitLines.Count > 0)
                return dlg.HitLines[_rand.Next(dlg.HitLines.Count)];
            return null;
        }
    }

    internal class TankDialogue
    {
        public List<string> ShootLines { get; } = new();
        public List<string> HitLines { get; } = new();
    }
}
