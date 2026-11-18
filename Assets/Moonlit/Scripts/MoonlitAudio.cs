using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed class MoonlitAudio : MonoBehaviour
    {
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource source, ambience;
        public bool Muted { get; private set; }
        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>(); source.volume = .23f;
            clips["jump"] = Tone(340, 680, .16f);
            clips["land"] = Tone(110, 55, .10f);
            clips["pulse"] = Tone(220, 880, .55f);
            clips["collect"] = Tone(740, 1100, .24f);
            clips["checkpoint"] = Tone(440, 880, .8f);
            clips["hurt"] = Tone(180, 60, .35f);
            clips["win"] = Tone(520, 1040, 1.6f);
            ambience = gameObject.AddComponent<AudioSource>(); ambience.clip = Ambient(); ambience.volume = .055f; ambience.loop = true; ambience.Play();
        }
        public void Play(string sound) { if (!Muted && clips.TryGetValue(sound, out var c)) source.PlayOneShot(c); }
        public void Toggle() { Muted = !Muted; ambience.mute = Muted; source.mute = Muted; }
        AudioClip Tone(float low, float high, float duration)
        {
            const int rate = 22050; int count = (int)(rate * duration); var data = new float[count]; float phase = 0;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count; phase += Mathf.Lerp(low, high, t) * Mathf.PI * 2 / rate;
                data[i] = (Mathf.Sin(phase) + .18f * Mathf.Sin(phase * 2)) * Mathf.Sin(Mathf.PI * Mathf.Min(t * 10, 1) * .5f) * Mathf.Pow(1 - t, 2) * .5f;
            }
            var clip = AudioClip.Create("Synth " + low, count, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        AudioClip Ambient()
        {
            const int rate = 22050, duration = 16; var data = new float[rate * duration];
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)rate;
                data[i] = (Mathf.Sin(t * Mathf.PI * 2 * 110) * .34f + Mathf.Sin(t * Mathf.PI * 2 * 165) * .2f + Mathf.Sin(t * Mathf.PI * 2 * 220) * .1f)
                    * (.6f + .4f * Mathf.Sin(t * Mathf.PI * 2 / duration));
            }
            var clip = AudioClip.Create("Forest drone - synthesized", data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
    }
}
