using UnityEngine;

namespace Moonlit
{
    // The collectible stays solid and readable; only its child artwork bobs.
    // Collider and route placement do not drift with the decoration.
    public sealed class MoonTokenVisual : MonoBehaviour
    {
        SpriteRenderer face, halo, glintA, glintB;
        Vector3 baseScale;
        float phase;
        public void Initialize(Sprite sprite)
        {
            phase = transform.position.x * .71f;
            face = Visuals.Sprite("Solid golden moon token", sprite, transform.position, transform, 18);
            baseScale = Vector3.one * (.54f / sprite.bounds.size.y); face.transform.localScale = baseScale;
            halo = Visuals.Halo("Small amber token halo", transform.position, .95f, new Color(1, .65f, .15f, .23f), transform, 16);
            glintA = Visuals.Box("Token rim glint vertical", transform.position, new Vector2(.025f, .16f), Color.white, transform, 19);
            glintB = Visuals.Box("Token rim glint horizontal", transform.position, new Vector2(.11f, .025f), Color.white, transform, 19);
        }
        void Update()
        {
            if (!face) return;
            float clock = Time.time * 1.7f + phase;
            float bob = Mathf.Sin(clock) * .075f;
            face.transform.localPosition = new Vector3(0, bob, 0);
            // A subtle tilt keeps the crescent readable; never flip to an invisible edge.
            face.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * .65f) * 5);
            halo.transform.localPosition = face.transform.localPosition;
            halo.color = new Color(1, .65f, .15f, .20f + .035f * Mathf.Sin(clock));
            float glint = Mathf.Pow(Mathf.Max(0, Mathf.Sin(Time.time * 2.1f + phase)), 12);
            glintA.color = glintB.color = new Color(1, .96f, .73f, glint);
            glintA.transform.localPosition = glintB.transform.localPosition = new Vector3(-.15f, bob + .16f, 0);
        }
    }

    public sealed class TokenCollectFeedback : MonoBehaviour
    {
        SpriteRenderer sprite;
        Vector3 origin, scale;
        float age;
        public void Initialize(Sprite image, Vector2 at)
        {
            origin = at; sprite = Visuals.Sprite("Collected moon token", image, at, transform, 32);
            scale = Vector3.one * (.54f / image.bounds.size.y); sprite.transform.localScale = scale;
        }
        void Update()
        {
            age += Time.deltaTime; float progress = age / .38f;
            if (progress >= 1) { Destroy(gameObject); return; }
            sprite.transform.position = origin + Vector3.up * (.65f * (1 - Mathf.Pow(1 - progress, 2)));
            sprite.transform.localScale = scale * (1 + Mathf.Sin(progress * Mathf.PI) * .45f);
            sprite.color = new Color(1, 1, 1, 1 - progress * progress);
        }
    }
}
