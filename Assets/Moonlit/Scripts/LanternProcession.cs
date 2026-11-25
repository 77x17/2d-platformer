using UnityEngine;

namespace Moonlit
{
    // A trail of festival lights restores a crossing. Distances between the posts
    // give the hand lantern time to recover while the player is traversing.
    public sealed class LanternProcession : MonoBehaviour
    {
        static readonly Color Bamboo = new Color(.49f, .32f, .17f);
        static readonly Color Edge = new Color(.89f, .66f, .33f);
        static readonly Color Silk = new Color(.39f, .21f, .20f);
        public int LitCount { get; private set; }
        public int Count => lamps.Length;
        public bool Complete => LitCount == Count;
        public Vector2 NextLight => Complete ? exitBell : centers[LitCount];

        MoonlitGame game;
        GameObject bridge;
        Vector2[] centers;
        SpriteRenderer[] lamps, glows;
        Transform[] hangers;
        SpriteRenderer bell;
        Vector2 exitBell;
        float rhythm, lastStrike;
        bool atBell, nearLamp;

        public static LanternProcession Create(MoonlitGame game, Transform parent, Vector2[] lampCenters, GameObject bridge)
        {
            var go = new GameObject("Lễ rước đèn");
            go.transform.SetParent(parent, false);
            var trail = go.AddComponent<LanternProcession>();
            trail.game = game;
            trail.bridge = bridge;
            trail.centers = lampCenters;
            trail.Build();
            game.player.LanternPulsed += trail.ReceiveLight;
            return trail;
        }

        void Build()
        {
            lamps = new SpriteRenderer[centers.Length];
            glows = new SpriteRenderer[centers.Length];
            hangers = new Transform[centers.Length];
            var star = Resources.Load<Sprite>("StarLantern");
            // Query before deactivating the crossing. Its parent can be at (0,0)
            // even when all of its children are hundreds of units along the route.
            bridge.SetActive(true);
            Physics2D.SyncTransforms();
            float left = float.PositiveInfinity;
            foreach (var collider in bridge.GetComponentsInChildren<Collider2D>(true))
            {
                if (collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger && collider.bounds.min.x < left)
                {
                    left = collider.bounds.min.x;
                    exitBell = new Vector2(left - 1.2f, collider.bounds.max.y + 1.15f);
                }
            }
            if (float.IsPositiveInfinity(left))
                exitBell = centers[centers.Length - 1] + new Vector2(3, 0);
            bridge.SetActive(false);
            for (int i = 0; i < Count; i++)
            {
                Vector2 center = centers[i];
                // Put the footing beneath the actual upright. A post next to a
                // terrace edge can stand over a different surface than its lamp.
                Vector2 basePoint = FindFooting(center + Vector2.left * .58f);
                Transform post = Group("Cột rước đèn " + (i + 1));
                Vector2 tip = center + Vector2.up * .9f;
                BuildPost(post, basePoint, tip);
                hangers[i] = new GameObject("Móc treo").transform;
                hangers[i].SetParent(post, false);
                hangers[i].position = tip;
                Rod("Dây đèn", tip, center + Vector2.up * .34f, .033f, Edge, hangers[i], 6);
                lamps[i] = Visuals.Sprite("Đèn giấy", star, center, hangers[i], 7);
                lamps[i].transform.localScale = Vector3.one * (.72f / star.bounds.size.y);
                lamps[i].color = Silk;
                glows[i] = Visuals.Halo("Ánh đèn", center, 2.1f, Color.clear, hangers[i], 8, lightScenery: true);
                for (int k = 0; k <= i; k++)
                {
                    Vector2 knot = new Vector2(basePoint.x - .11f, basePoint.y + .63f + k * .13f);
                    Rod("Vạch chuông", knot, knot + Vector2.right * .24f, .046f, Edge, post, 6);
                }
            }
            var bellBase = FindFooting(exitBell + Vector2.left * .40f);
            exitBell = new Vector2(exitBell.x, bellBase.y + 1.25f);
            var b = Group("Chuông đầu cầu");
            Vector2 hook = exitBell + Vector2.up * .46f;
            BuildPost(b, bellBase, hook);
            Rod("Dây chuông", hook, exitBell + Vector2.up * .16f, .035f, Edge, b, 6);
            bell = Visuals.Sprite("Chuông đồng", Visuals.MoonMote, exitBell, b, 7);
            bell.transform.localScale = new Vector3(.65f, .45f, 1);
            bell.color = Bamboo;
        }

        Transform Group(string title)
        {
            var node = new GameObject(title).transform; node.SetParent(transform, false);
            return node;
        }

        Vector2 FindFooting(Vector2 at)
        {
            var hit = Physics2D.Raycast(at, Vector2.down, 20, 1 << 8);
            if (IsFooting(hit.collider) && hit.normal.y > .6f) return hit.point;
            // A lamp may overhang a bank after a layout edit. Mount its cantilever
            // on the nearest actual shore instead of inventing a floor in the air.
            Vector2 nearest = at;
            float distance = float.PositiveInfinity;
            foreach (var surface in game.level.GetComponentsInChildren<PlatformSurface>())
            {
                var collider = surface.GetComponent<Collider2D>();
                if (!IsFooting(collider) || collider.bounds.max.y >= at.y) continue;
                var box = collider.bounds;
                Vector2 point = new Vector2(Mathf.Clamp(at.x, box.min.x + .15f, box.max.x - .15f), box.max.y);
                float offset = (point - at).sqrMagnitude;
                if (offset < distance) { nearest = point; distance = offset; }
            }
            return nearest;
        }

        static bool IsFooting(Collider2D candidate) =>
            candidate && candidate.enabled && !candidate.attachedRigidbody && !candidate.GetComponent<FallingPlatform>();

        void BuildPost(Transform parent, Vector2 ground, Vector2 hook)
        {
            float armY = hook.y + .07f;
            Vector2 foot = ground - Vector2.up * .09f;
            Vector2 top = new Vector2(foot.x + .08f, armY);
            Rod("Thân tre", foot, top, .14f, Bamboo, parent, 4);
            Rod("Vệt nắng trên tre", foot + Vector2.left * .039f, top + Vector2.left * .039f, .025f, Edge, parent, 5);
            Rod("Tay đỡ đèn", top + Vector2.left * .10f, hook + Vector2.right * .10f, .10f, Bamboo, parent, 5);
            Rod("Thanh chống", top + Vector2.down * .41f, hook + Vector2.left * .09f, .065f, Bamboo, parent, 4);
            Rod("Dây buộc", top + new Vector2(-.09f, -.14f), top + new Vector2(.11f, -.08f), .055f, Edge, parent, 6);
            var stone = game.art.environment[6];
            var baseStone = Visuals.Sprite("Chân đá", stone, ground, parent, 3);
            var size = new Vector3(.65f / stone.bounds.size.x, .30f / stone.bounds.size.y, 1);
            baseStone.transform.localScale = size;
            baseStone.transform.position = ground + Vector2.up * (-.08f - stone.bounds.min.y * size.y);
            baseStone.color = new Color(.76f, .74f, .64f);
        }

        static void Rod(string name, Vector2 a, Vector2 b, float width, Color tint, Transform parent, int order)
        {
            var slat = Visuals.Box(name, (a + b) * .5f, new Vector2(width, Vector2.Distance(a,b)), tint, parent, order);
            slat.transform.rotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }

        void ReceiveLight(Vector2 at, float radius)
        {
            if (!isActiveAndEnabled || Complete || game.State != GameState.Playing || Vector2.Distance(at, NextLight) > radius) return;
            int index = LitCount++;
            lastStrike = rhythm;
            game.audioFx.Play("signal" + LitCount);
            game.effects.Ring(centers[index], Visuals.Gold, 1.45f);
            game.effects.Burst(centers[index], Visuals.Gold, 17, 2);
            if (Complete)
            {
                bridge.SetActive(true);
                bell.color = Color.white;
                game.effects.Ring(exitBell, Visuals.Mint, 4);
                foreach (var floor in bridge.GetComponentsInChildren<PlatformSurface>())
                    game.effects.Burst(floor.transform.position + Vector3.up * .15f, Visuals.Mint, 12, 1.8f);
                game.Toast("CẦU RƯỚC ĐÈN ĐÃ MỞ", "Ba ngọn đèn cùng sáng. Cuội có thể sang bờ bên kia.");
            }
            else game.Toast("ĐÈN RƯỚC  " + LitCount + " / " + Count, "Đèn kế tiếp đang chờ ở phía trước.");
            nearLamp = false;
        }

        void Update()
        {
            if (game.State != GameState.Playing) return;
            rhythm += Time.deltaTime;
            for (int i = 0; i < Count; i++)
            {
                bool lit = i < LitCount, next = i == LitCount;
                float flicker = .5f + .5f * Mathf.Sin(rhythm * (lit ? 3.3f : 2.5f) + i);
                hangers[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(rhythm * 1.8f + i * 1.4f) * (lit && rhythm - lastStrike < 1 ? 6 : 2));
                lamps[i].color = lit ? Color.Lerp(new Color(1,.86f,.70f), Color.white, flicker) :
                    next ? Color.Lerp(new Color(.67f,.46f,.34f), new Color(.87f,.68f,.50f), flicker) : Silk;
                glows[i].color = new Color(1,.65f,.21f, lit ? .47f + flicker * .12f : next ? .18f + flicker * .12f : .04f);
            }
            if (Complete) return;
            Vector2 player = game.player.Body.position + Vector2.up * .9f;
            bool byLamp = Vector2.Distance(player, NextLight) < game.tuning.pulseRadius + .35f;
            if (byLamp && !nearLamp) game.Toast("ĐÈN RƯỚC  " + (LitCount + 1) + " / " + Count, "Đến bên đèn ông sao rồi bấm J để thắp.");
            nearLamp = byLamp;
            bool byBell = Vector2.Distance(player, exitBell) < 3;
            if (byBell && !atBell)
                game.Toast("CẦU CHƯA SÁNG", "Quay lại thắp đèn thứ " + (LitCount + 1) + ", cách đây " + Mathf.RoundToInt(Mathf.Abs(NextLight.x - exitBell.x)) + " ô.");
            atBell = byBell;
        }

        void OnEnable()
        {
            // On the first activation Create has not assigned the owner yet.
            if (game && game.player) game.player.LanternPulsed += ReceiveLight;
        }

        void OnDisable()
        {
            if (game && game.player) game.player.LanternPulsed -= ReceiveLight;
        }
    }
}
