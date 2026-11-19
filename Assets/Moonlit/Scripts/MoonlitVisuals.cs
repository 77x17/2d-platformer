using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public static class Visuals
    {
        public static readonly Color Mint = new Color(.61f, .94f, .82f);
        public static readonly Color Gold = new Color(1f, .73f, .34f);
        public static Sprite Square { get; private set; }
        public static Sprite Glow { get; private set; }
        public static Material Material;

        public static void Init(Material material)
        {
            Material = material;
            if (Square) return;
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, Color.white); t.Apply();
            Square = UnityEngine.Sprite.Create(t, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
            var g = new Texture2D(96, 96, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(47.5f, 47.5f)) / 47.5f;
                float a = Mathf.Pow(Mathf.Clamp01(1 - d), 2.4f);
                g.SetPixel(x, y, new Color(1, 1, 1, a));
            }
            g.Apply(); Glow = UnityEngine.Sprite.Create(g, new Rect(0, 0, 96, 96), Vector2.one * .5f, 96);
        }

        public static SpriteRenderer Sprite(string name, Sprite sprite, Vector2 position, Transform parent, int order = 0, float scale = 1)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = order;
            if (Material) sr.sharedMaterial = Material;
            return sr;
        }

        public static SpriteRenderer Box(string name, Vector2 pos, Vector2 size, Color color, Transform parent, int order)
        {
            var s = Sprite(name, Square, pos, parent, order); s.transform.localScale = new Vector3(size.x, size.y, 1); s.color = color; return s;
        }

        public static SpriteRenderer Halo(string name, Vector2 pos, float size, Color color, Transform parent, int order = 15)
        {
            var s = Sprite(name, Glow, pos, parent, order, size); s.color = color; return s;
        }
    }

    public sealed class AmbientMotion : MonoBehaviour
    {
        public float sway = 3, bob, speed = 1, phase;
        public bool flicker;
        public bool windAffected;
        Vector3 origin;
        SpriteRenderer sr;
        Color tint;
        void Start() { origin = transform.localPosition; sr = GetComponent<SpriteRenderer>(); if (sr) tint = sr.color; }
        void Update()
        {
            float t = Time.time * speed + phase;
            transform.localPosition = origin + Vector3.up * Mathf.Sin(t) * bob;
            float wind = windAffected && MoonlitGame.Instance && MoonlitGame.Instance.wind ? MoonlitGame.Instance.wind.Strength : 0;
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t) * sway + wind * (6 + Mathf.Sin(t * 3.1f) * 3));
            if (flicker && sr) { var c = tint; c.a *= .77f + .23f * Mathf.Sin(t * 2.3f); sr.color = c; }
        }
    }

    public sealed class MoonlitEffects : MonoBehaviour
    {
        sealed class Mote
        {
            public SpriteRenderer sr;
            public Vector2 origin, velocity;
            public float age, life, size, phase;
            public Color color;
            public bool ambient;
        }
        readonly List<Mote> motes = new List<Mote>();
        readonly Queue<SpriteRenderer> pool = new Queue<SpriteRenderer>();
        public int BurstCount { get; private set; }

        public void Populate(float length)
        {
            var rng = new System.Random(48);
            for (int i = 0; i < 260; i++)
            {
                var pos = new Vector2((float)rng.NextDouble() * length, (float)rng.NextDouble() * 10 - 1);
                var c = new Color(.45f, .75f, .61f); // Ambient insects stay muted green; collectibles are solid gold.
                var sr = Visuals.Sprite("Drifting firefly", Visuals.Square, pos, transform, i % 2 == 0 ? -5 : 20, .035f + (float)rng.NextDouble() * .035f);
                motes.Add(new Mote { sr = sr, origin = pos, ambient = true, phase = (float)rng.NextDouble() * 8, color = c });
            }
        }

        public void Burst(Vector2 at, Color color, int count = 12, float strength = 2.8f)
        {
            BurstCount++;
            for (int i = 0; i < count; i++)
            {
                var sr = pool.Count > 0 ? pool.Dequeue() : Visuals.Sprite("Spark", Visuals.Square, at, transform, 30);
                sr.gameObject.SetActive(true); sr.transform.position = at;
                float size = Random.Range(.035f, .09f);
                sr.transform.localScale = Vector3.one * size;
                motes.Add(new Mote { sr = sr, origin = at, velocity = Random.insideUnitCircle * strength + Vector2.up * .6f,
                    life = Random.Range(.3f, .8f), color = color, size = size });
            }
        }

        public void CollectToken(Vector2 at, Sprite sprite)
        {
            var feedback = new GameObject("Moon token pickup feedback"); feedback.transform.SetParent(transform);
            feedback.AddComponent<TokenCollectFeedback>().Initialize(sprite, at);
            Ring(at, Visuals.Gold, .65f); Burst(at, Visuals.Gold, 6, 1.2f);
        }

        public void Ring(Vector2 at, Color color, float radius = 4.3f)
        {
            var g = new GameObject("Lantern ripple"); g.transform.SetParent(transform); g.transform.position = at;
            var line = g.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.loop = true; line.positionCount = 64;
            line.sharedMaterial = Visuals.Material; line.sortingOrder = 26;
            line.startColor = color; line.endColor = color;
            g.AddComponent<ExpandingRing>().Initialize(line, radius, color);
        }

        void Update()
        {
            for (int i = motes.Count - 1; i >= 0; i--)
            {
                var m = motes[i];
                if (m.ambient)
                {
                    float t = Time.time * .6f + m.phase;
                    m.sr.transform.position = m.origin + new Vector2(Mathf.Sin(t) * .7f, Mathf.Cos(t * .7f) * .45f);
                    var c = m.color; c.a = .12f + .30f * Mathf.Pow((Mathf.Sin(t * 2) + 1) * .5f, 3); m.sr.color = c;
                    continue;
                }
                m.age += Time.deltaTime;
                if (m.age >= m.life) { m.sr.gameObject.SetActive(false); pool.Enqueue(m.sr); motes.RemoveAt(i); continue; }
                m.velocity += Vector2.down * Time.deltaTime * 2;
                m.sr.transform.position += (Vector3)m.velocity * Time.deltaTime;
                var tint = m.color; tint.a *= 1 - m.age / m.life; m.sr.color = tint;
                m.sr.transform.Rotate(0, 0, 80 * Time.deltaTime);
            }
        }
    }

    public sealed class ExpandingRing : MonoBehaviour
    {
        LineRenderer line; float radius, age; Color color;
        public void Initialize(LineRenderer lr, float r, Color c) { line = lr; radius = r; color = c; }
        void Update()
        {
            age += Time.deltaTime; float p = age / .55f;
            if (p >= 1) { Destroy(gameObject); return; }
            float r = Mathf.Lerp(.3f, radius, 1 - Mathf.Pow(1 - p, 3));
            for (int i = 0; i < 64; i++) { float a = i * Mathf.PI * 2 / 64; line.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * r); }
            line.startWidth = line.endWidth = .065f * (1 - p);
            var c = color; c.a = 1 - p; line.startColor = line.endColor = c;
        }
    }
}
