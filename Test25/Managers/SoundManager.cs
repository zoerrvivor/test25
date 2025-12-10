using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace Test25.Managers
{
    public static class SoundManager
    {
        private static Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();
        private static Dictionary<string, Song> _songs = new Dictionary<string, Song>();

        private static string _currentSongName;

        public static float MasterVolume { get; set; } = 1.0f;
        public static float SfxVolume { get; set; } = 1.0f;
        public static float MusicVolume { get; set; } = 0.5f;

        public static void LoadContent(ContentManager content)
        {
            // Load Sound Effects
            string sfxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, "Sounds");
            if (Directory.Exists(sfxPath))
            {
                var files = Directory.GetFiles(sfxPath, "*.xnb");
                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    try
                    {
                        var potentialSfx = content.Load<SoundEffect>($"Sounds/{fileName}");
                        if (potentialSfx != null)
                        {
                            _soundEffects[fileName] = potentialSfx;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore load failures (e.g. if it's not a valid sound effect XNB)
                    }
                }
            }
            
            // Load Music (Songs)
            string musicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, "Music");
            if (Directory.Exists(musicPath))
            {
                var files = Directory.GetFiles(musicPath, "*.xnb"); // Songs are also .xnb in MonoGame pipeline usually, or .mp3/.ogg if raw
                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    try
                    {
                        var potentialSong = content.Load<Song>($"Music/{fileName}");
                        if (potentialSong != null)
                        {
                            _songs[fileName] = potentialSong;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore failure
                    }
                }
            }
        }

        public static void PlaySound(string name, float pitch = 0.0f, float pan = 0.0f)
        {
            if (_soundEffects.TryGetValue(name, out var sfx))
            {
                sfx.Play(SfxVolume * MasterVolume, pitch, pan);
            }
        }

        public static void PlayMusic(string name, bool isRepeating = true)
        {
            // Don't restart if already playing the same song
            if (_currentSongName == name && MediaPlayer.State == MediaState.Playing)
                return;

            if (_songs.TryGetValue(name, out var song))
            {
                _currentSongName = name;
                MediaPlayer.Volume = MusicVolume * MasterVolume;
                MediaPlayer.IsRepeating = isRepeating;
                MediaPlayer.Play(song);
            }
        }

        public static void StopMusic()
        {
            MediaPlayer.Stop();
            _currentSongName = null;
        }
    }
}
