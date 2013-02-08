using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


namespace GravWalker
{
    public static class AudioController
    {
        public static float sfxvolume = 1f;
        public static float musicvolume = 0.1f;

        public static Random randomNumber = new Random();

        public static Dictionary<string, SoundEffect> effects;

        public static Song musicInstance;

        public static string currentlyPlaying = "";

        public static void LoadContent(ContentManager content)
        {
            effects = new Dictionary<string, SoundEffect>();

            effects.Add("explode", content.Load<SoundEffect>("sfx/explode"));
            effects.Add("hit", content.Load<SoundEffect>("sfx/hit"));
            effects.Add("smg", content.Load<SoundEffect>("sfx/smg"));
            effects.Add("machinegun", content.Load<SoundEffect>("sfx/machinegun"));
            effects.Add("mortar", content.Load<SoundEffect>("sfx/mortar"));
            effects.Add("pistol", content.Load<SoundEffect>("sfx/pistol"));
            effects.Add("shotgun", content.Load<SoundEffect>("sfx/shotgun"));
            effects.Add("sniper", content.Load<SoundEffect>("sfx/sniper"));

        }

        public static void LoadMusic(string piece, ContentManager content)
        {
            if (currentlyPlaying.ToLower() == piece.ToLower()) return;
            currentlyPlaying = piece;

            if (!MediaPlayer.GameHasControl) return;

            if (MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();
            //if (musicInstance != null)
            //{
            //    musicInstance.Dispose();
            //}

            musicInstance = content.Load<Song>("audio/music/" + piece);
            MediaPlayer.IsRepeating = true;
            // MediaPlayer.Volume = musicvolume;
            MediaPlayer.Play(musicInstance);

            //if (!OptionsMenuScreen.music) MediaPlayer.Pause();
        }

        public static void StopMusic()
        {
            if (!MediaPlayer.GameHasControl) return;

            MediaPlayer.Stop();
        }

        public static void ToggleMusic()
        {
            if (!MediaPlayer.GameHasControl) return;

            //if (OptionsMenuScreen.music)
            //{
            //    MediaPlayer.Resume();
            //}
            //else
            //    MediaPlayer.Pause();
        }

        public static void PlaySFX(string name)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, 0f, 0f);
        }
        public static void PlaySFX(string name, float pitch)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, pitch, 0f);
        }
        public static void PlaySFX(string name, float volume, float pitch, float pan)
        {
           // if (OptionsMenuScreen.sfx)
            if (pan < -1f || pan > 1f) return;
            volume = MathHelper.Clamp(volume, 0f, 1f);
            effects[name].Play(volume * sfxvolume, pitch, pan);
        }
        public static void PlaySFX(string name, float minpitch, float maxpitch)
        {
           // if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), 0f);
        }

        internal static void PlaySFX(string name, float volume, float minpitch, float maxpitch, Vector2 Position)
        {
            Vector2 screenPos = Vector2.Transform(Position, GameManager.Camera.CameraMatrix);
            float pan = MathHelper.Clamp((screenPos.X - (GameManager.Camera.Width / 2)) / (GameManager.Camera.Width/2), -1f,1f);
            effects[name].Play(volume * sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), pan);
        }


        public static void Update(GameTime gameTime)
        {
            //if (!MediaPlayer.GameHasControl) return;

            // if (MediaPlayer.Volume > musicvolume) MediaPlayer.Volume = musicvolume;
        }

        public static void Unload()
        {

        }



       
    }
}
