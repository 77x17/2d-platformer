using UnityEngine;

namespace Moonlit
{
    public sealed class MoonTokenVisual : MonoBehaviour
    {
        public const float Size = .36f;
        SpriteRenderer face, halo, sparkle;
        float phase;
        public void Initialize()
        {
            phase = transform.position.x * .71f;
            face = Visuals.Sprite("Moon mote diamond", Visuals.MoonMote, transform.position, transform, 18, Size);
            halo = Visuals.Halo("Moon mote halo", transform.position, 1.05f, new Color(1, .65f, .22f, .35f), transform, 16);
            sparkle = Visuals.Sprite("Moon mote sparkle", Visuals.Square, transform.position, transform, 19, .045f);
            sparkle.transform.localRotation = Quaternion.Euler(0, 0, 45);
        }
        void Update()
        {
            if (!face) return;
            float clock = Time.time * 2.2f + phase;
            float wave = Mathf.Sin(clock);
            float pulse = (wave + 1) * .5f;
            var offset = Vector3.up * (wave * .085f);
            face.transform.localPosition = halo.transform.localPosition = offset;
            face.transform.localScale = Vector3.one * (Size * Mathf.Lerp(.95f, 1.10f, pulse));
            halo.color = new Color(1, .65f, .22f, Mathf.Lerp(.30f, .52f, pulse));

            float angle = clock * 1.25f;
            sparkle.transform.localPosition = offset + new Vector3(Mathf.Cos(angle) * .19f, Mathf.Sin(angle) * .22f, 0);
            float sparkleAlpha = Mathf.Pow(Mathf.Max(0, wave), 6);
            sparkle.color = new Color(1, .97f, .76f, sparkleAlpha);
        }
    }

    public sealed class TokenCollectFeedback : MonoBehaviour
    {
        SpriteRenderer sprite;
        Vector3 origin;
        float age;
        public void Initialize(Vector2 at)
        {
            origin = at;
            sprite = Visuals.Sprite("Collected moon mote", Visuals.MoonMote, at, transform, 32, MoonTokenVisual.Size);
        }
        void Update()
        {
            age += Time.deltaTime; float progress = age / .38f;
            if (progress >= 1) { Destroy(gameObject); return; }
            sprite.transform.position = origin + Vector3.up * (.65f * (1 - Mathf.Pow(1 - progress, 2)));
            sprite.transform.localScale = Vector3.one * (MoonTokenVisual.Size * (1 + Mathf.Sin(progress * Mathf.PI) * .45f));
            sprite.color = new Color(1, 1, 1, 1 - progress * progress);
        }
    }
}
