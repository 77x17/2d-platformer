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
        readonly AudioSource[] ambience = new AudioSource[2];
        AudioSource effects;
        int current, interactionFrame = -1;
        string theme;
        float warningUntil;
        internal event System.Action<string> SoundPlayed;
        public bool Muted { get; private set; }

        void Awake()
        {
            foreach (var clip in Resources.LoadAll<AudioClip>("MoonAudio")) clips[clip.name] = clip;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.volume = .53f;
            effects.priority = 48;
            for (int i = 0; i < music.Length; i++)
            {
                music[i] = gameObject.AddComponent<AudioSource>();
                music[i].playOnAwake = false;
                music[i].loop = true;
                music[i].volume = 0;
                var bed = ambience[i] = gameObject.AddComponent<AudioSource>();
                bed.playOnAwake = false;
                bed.loop = true;
                bed.priority = 180;
                bed.volume = 0;
                if (clips.TryGetValue(i == 0 ? "amb_canopy" : "amb_river", out var bedClip))
                    bed.clip = bedClip;
            }
            SetMute(PlayerPrefs.GetInt(MutePreference, 0) == 1);
        }

        void Update()
        {
            var game = MoonlitGame.Instance;
            if (!game) return;
            bool story = game.State == GameState.Home || game.InStory || game.State == GameState.Ending;
            string next = story ? ChapterMusic[0] :
                game.Boss && game.Boss.Active ? "mus_guardian" : ChapterMusic[Mathf.Clamp(game.Chapter, 1, 4)];
            if (theme != next && clips.TryGetValue(next, out var nextClip))
            {
                current = 1 - current;
                if (!music[current].isPlaying || music[current].clip != nextClip)
                {
                    music[current].clip = nextClip;
                    music[current].volume = 0;
                    music[current].Play();
                }
                theme = next;
            }
            float volume = story || game.State == GameState.Playing ? .35f :
                game.State == GameState.Paused || game.State == GameState.Map || game.State == GameState.Tutorial ? .14f : .045f;
            bool warning = Time.unscaledTime < warningUntil && !story;
            if (warning) volume *= .68f;
            for (int i = 0; i < music.Length; i++)
            {
                var source = music[i];
                source.volume = Mathf.MoveTowards(source.volume, current == i ? volume : 0, Time.unscaledDeltaTime * .55f);
                if (i != current && source.volume <= 0 && source.isPlaying) source.Stop();
                var bed = ambience[i];
                float ambientLevel = game.Chapter == i + 2 && game.State == GameState.Playing ? (warning ? .095f : .16f) : 0;
                if (ambientLevel > 0 && !bed.isPlaying && bed.clip) bed.Play();
                bed.volume = Mathf.MoveTowards(bed.volume, ambientLevel, Time.unscaledDeltaTime * .25f);
                if (ambientLevel == 0 && bed.volume == 0 && bed.isPlaying) bed.Stop();
            }
        }

        public void Play(string sound)
        {
            if (Muted) return;
            string prefix = sound == "win" || sound == "lose" ? "jingle_" : "sfx_";
            if (clips.TryGetValue(prefix + sound, out var clip))
            {
                float gain = 1;
                switch (sound)
                {
                    case "rootCoil": case "rootDrum": case "warn": case "crowCall": case "carpBubbles":
                        warningUntil = Time.unscaledTime + .65f;
                        break;
                    case "step": case "woodStep": case "climb": gain = .68f; break;
                    case "uiMove": case "ready": gain = .78f; break;
                }
                effects.PlayOneShot(clip, gain);
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
            foreach (var source in ambience) source.mute = value;
        }
    }
}
