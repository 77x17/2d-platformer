using UnityEngine;

namespace Moonlit
{
    public sealed class ForestWind : MonoBehaviour
    {
        public float Strength { get; private set; }
        public int GustCount { get; private set; }
        public float Drift { get; private set; }
        MoonlitGame game;
        float gustAge = -1, nextGust = 28;
        bool firstZone, upperZone;
        readonly SpriteRenderer[] leaves = new SpriteRenderer[34];
        readonly Vector2[] offsets = new Vector2[34];
        readonly float[] phases = new float[34];

        public void Initialize(MoonlitGame owner)
        {
            game = owner;
            var random = new System.Random(1842);
            for (int i = 0; i < leaves.Length; i++)
            {
                int index = i % 3 == 0 ? 6 : i % 3 == 1 ? 12 : 13;
                var s = Visuals.Sprite("Windborne banyan leaf", game.art.forestExtras[index], Vector2.zero, transform, i % 2 == 0 ? -15 : 22);
                float size = .14f + (float)random.NextDouble() * .18f;
                s.transform.localScale = Vector3.one * (size / s.sprite.bounds.size.x);
                s.color = new Color(.68f, .82f, .55f, .55f);
                leaves[i] = s; offsets[i] = new Vector2((float)random.NextDouble() * 30 - 15, (float)random.NextDouble() * 17 - 8.5f);
                phases[i] = (float)random.NextDouble() * 7;
            }
        }

        void Update()
        {
            if (!game) return;
            float dt = game.State == GameState.Playing || game.State == GameState.Home ? Time.deltaTime : 0;
            if (game.State == GameState.Playing)
            {
                float x = game.player.transform.position.x;
                if (!firstZone && x > 44) { firstZone = true; BeginGust(); }
                else if (!upperZone && x > 226) { upperZone = true; BeginGust(); }
                else if (game.Elapsed > nextGust && gustAge < 0) BeginGust();
            }
            if (gustAge >= 0)
            {
                gustAge += dt;
                Strength = Mathf.Sin(Mathf.Clamp01(gustAge / 7) * Mathf.PI);
                if (gustAge >= 7) { gustAge = -1; Strength = 0; }
            }
            Drift += dt * (.12f + Strength * .8f);
            for (int i = 0; i < leaves.Length; i++)
            {
                offsets[i].x += dt * (.30f + Strength * (3 + phases[i] * .45f));
                offsets[i].y -= dt * (.15f + Strength * .35f);
                if (offsets[i].x > 15) offsets[i].x -= 30;
                if (offsets[i].y < -8.5f) offsets[i].y += 17;
                leaves[i].enabled = i < 7 || Strength > .12f;
            }
        }

        void LateUpdate()
        {
            if (!game) return;
            Vector3 center = game.cameraRig.transform.position; center.z = 0;
            for (int i = 0; i < leaves.Length; i++)
            {
                leaves[i].transform.position = center + (Vector3)offsets[i] + Vector3.up * Mathf.Sin(Time.time * 1.4f + phases[i]) * .22f;
                leaves[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2 + phases[i]) * 40 + Strength * 50);
            }
        }

        public void BeginGust()
        {
            gustAge = 0; GustCount++; nextGust = game.Elapsed + 26;
            // Wind stays atmospheric; it must not interrupt a mechanic lesson or hide its text.
        }
        public void ResetWind() { gustAge = -1; Strength = 0; GustCount = 0; firstZone = upperZone = false; nextGust = 28; Drift = 0; }
    }
}
