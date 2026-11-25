using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed class MoonlitAudio : MonoBehaviour
    {
        public const float InteractionDuration = .10f;
        const string MutePreference = "Moonlit.AudioMuted";
        static readonly string[] ChapterMusic = { "mus_home", "mus_village", "mus_bamboo", "mus_river", "mus_hill" };
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly AudioSource[] music = new AudioSource[2];
        AudioSource effects;
        int current, interactionFrame = -1;
        string theme;
        internal event System.Action<string> SoundPlayed;
        public bool Muted { get; private set; }

        void Awake()
        {
            foreach (var clip in Resources.LoadAll<AudioClip>("MoonAudio")) clips[clip.name] = clip;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.volume = .53f;
            for (int i = 0; i < music.Length; i++)
            {
                music[i] = gameObject.AddComponent<AudioSource>();
                music[i].playOnAwake = false;
                music[i].loop = true;
                music[i].volume = 0;
            }
            SetMute(PlayerPrefs.GetInt(MutePreference, 0) == 1);
        }

        void Update()
        {
            var game = MoonlitGame.Instance;
            if (!game) return;
            string next = game.State == GameState.Home ? ChapterMusic[0] :
                game.Boss && game.Boss.Active ? "mus_guardian" : ChapterMusic[Mathf.Clamp(game.Chapter, 1, 4)];
            if (theme != next && clips.TryGetValue(next, out var nextClip))
            {
                current = 1 - current;
                music[current].clip = nextClip;
                music[current].volume = 0;
                music[current].Play();
                theme = next;
            }
            float volume = game.State == GameState.Home || game.State == GameState.Playing ? .35f :
                game.State == GameState.Paused || game.State == GameState.Tutorial ? .14f : .045f;
            for (int i = 0; i < music.Length; i++)
            {
                var source = music[i];
                source.volume = Mathf.MoveTowards(source.volume, current == i ? volume : 0, Time.unscaledDeltaTime * .55f);
                if (i != current && source.volume <= 0 && source.isPlaying) source.Stop();
            }
        }

        public void Play(string sound)
        {
            if (Muted) return;
            string prefix = sound == "win" || sound == "lose" ? "jingle_" : "sfx_";
            if (clips.TryGetValue(prefix + sound, out var clip))
            {
                effects.PlayOneShot(clip);
                SoundPlayed?.Invoke(sound);
            }
        }

        public void PlayInteraction()
        {
            if (Muted || interactionFrame == Time.frameCount) return;
            interactionFrame = Time.frameCount;
            Play("uiClick");
        }

        public void Toggle()
        {
            SetMute(!Muted);
            try { PlayerPrefs.SetInt(MutePreference, Muted ? 1 : 0); PlayerPrefs.Save(); }
            catch (PlayerPrefsException) { Debug.LogWarning("Could not save audio preference for this user."); }
        }

        void SetMute(bool value)
        {
            Muted = value;
            effects.mute = value;
            foreach (var source in music) source.mute = value;
        }
    }
}
