using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed class MoonlitLevel : MonoBehaviour
    {
        public const float Length = 370;
        public const float GoalX = 362;
        public const float GoalFloor = 9.1f;
        public const float LeftPlayerLimit = 1.25f;
        public readonly List<FallingPlatform> falling = new List<FallingPlatform>();
        public readonly List<MovingPlatform> moving = new List<MovingPlatform>();
        public readonly List<ForestPickup> pickups = new List<ForestPickup>();
        public LanternSwitch lanternSwitch;
        public Transform firstJumpTarget;
        MoonlitGame game;
        Transform terrain, decoration, encounters;
        int tile;

        public void Build(MoonlitGame owner)
        {
            game = owner;
            terrain = Group("01 - Terrain and platforms"); decoration = Group("02 - Animated forest dressing"); encounters = Group("03 - Encounters and pickups");
            // 1. Learn the lantern: generous landing spaces, first dog and a short ascent.
            Ground(-4, 20, 0); Ground(28, 43, .5f); Ground(65, 79, .2f);
            firstJumpTarget = Platform(23.3f, .9f, 3.8f).transform;
            Platform(46.3f, 1.6f, 4.2f); Platform(52.4f, 2.7f, 4.6f); Platform(59.2f, 1.6f, 4.3f);
            Enemy(false, 36, .5f, 33, 41);
            Checkpoint(69, .2f, 1);
            // 2. River of fireflies: predictable moving platforms and the first pouncing cat.
            Platform(82.5f, 1.3f, 3.8f, new Vector2(0, .75f));
            Platform(88.8f, 2.1f, 4.1f);
            Platform(96, 2.8f, 4.3f);
            Ground(101, 114, 1); Enemy(true, 109, 1, 106, 112);
            // 3. Old bridge: falling planks telegraph before dropping and return after 3.5 s.
            Platform(118, 1.8f, 3.6f, null, true);
            Platform(123.5f, 2.8f, 3.6f, null, true);
            Platform(129, 3.8f, 3.6f, null, true);
            Ground(134, 147, 3.4f); Checkpoint(137, 3.4f, 2);
            Platform(151, 2.1f, 4);
            Ground(156, 176, .5f); Enemy(false, 163, .5f, 158, 169);
            BuildSwitchBridge();
            // 4. Moon altar: the final rhythm combines the earlier lessons.
            Ground(190, 206, 1); Checkpoint(193, 1, 3); Enemy(true, 201, 1, 197, 204.5f);
            Platform(209.7f, 2.4f, 3.5f);
            Platform(215.3f, 3.7f, 3.5f, new Vector2(0, .55f));
            Platform(221, 4.9f, 3.7f);
            Ground(226, 247, 6);
            // Upper roots extend the route without introducing another control or skill.
            Platform(251, 5, 4); Platform(257, 4.1f, 4);
            Ground(262, 275, 3); Checkpoint(264, 3, 4); Enemy(false, 270, 3, 267, 273);
            Platform(279, 4, 4.1f); Platform(285, 5.1f, 4.2f);
            Ground(290, 303, 4.5f); Enemy(true, 297, 4.5f, 292, 301);
            Platform(307, 5.5f, 3.8f, null, true); Platform(312.5f, 6.6f, 3.8f, null, true);
            Ground(318, 332, 5); Checkpoint(321, 5, 5);
            Platform(336, 6, 3.8f); Platform(342, 7, 4); Platform(348, 8.1f, 4);
            Ground(353, 373, GoalFloor); Moon(GoalX, GoalFloor + 2);
            // Glimmers guide jumps; none are required to finish.
            float[,] trail = { {12,1.8f},{18,2},{23.3f,2.4f},{30,2},{40,2},{46.3f,3.1f},{52.4f,4.2f},{59.2f,3.1f},
                {74,2},{82.5f,3.1f},{88.8f,3.9f},{96,4.6f},{109,3},{118,3.3f},{123.5f,4.3f},{129,5.3f},
                {143,5},{151,3.7f},{166,2.5f},{180,2.6f},{185,2.6f},{200,3},{209.7f,4},{215.3f,5.4f},{221,6.5f},{232,7.8f},
                {243,7.8f},{251,6.6f},{257,5.7f},{270,5},{279,5.6f},{285,6.7f},{296,6.4f},{307,7.1f},{312.5f,8.2f},
                {326,6.8f},{336,7.6f},{342,8.6f},{348,9.7f},{358,10.9f} };
            for (int i = 0; i < trail.GetLength(0); i++) Mote(trail[i, 0], trail[i, 1]);
            game.TotalMotes = trail.GetLength(0);
            // Invisible end stops stop leaving the level without obscuring the art.
            Wall(LeftPlayerLimit - .575f, 4); Wall(372, 14);
            var startRock = Prop(6, new Vector2(-.65f, 1.45f), 1.6f, 4);
            startRock.color = new Color(.68f, .79f, .74f);
            Prop(15, new Vector2(-.4f, 5.8f), 6.2f, 1);
            Dress();
        }

        Transform Group(string name) { var go = new GameObject(name); go.transform.SetParent(transform); return go.transform; }

        void Wall(float x, float y)
        {
            var go = new GameObject("Level boundary"); go.layer = 8; go.transform.SetParent(terrain); go.transform.position = new Vector2(x, y);
            go.AddComponent<BoxCollider2D>().size = new Vector2(.5f, 30);
        }

        void Ground(float start, float end, float top)
        {
            var go = new GameObject($"Ground [{start}, {end}] top {top}"); go.layer = 8; go.transform.SetParent(terrain); go.transform.position = new Vector2((start + end) * .5f, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(end - start, 12); c.offset = new Vector2(0, -6);
            go.AddComponent<PlatformSurface>();
            Visuals.Box("Deep soil", new Vector2((start + end) * .5f, top - 6), new Vector2(end - start, 12), new Color(.025f, .065f, .10f), go.transform, 0);
            int count = Mathf.CeilToInt((end - start) / 2.7f); float width = (end - start) / count;
            for (int i = 0; i < count; i++) SurfaceTile(go.transform, start + (i + .5f) * width, top, width + .035f, false);
            for (int i = 0; i < count; i++)
            {
                var roots = Prop(15, new Vector2(start + (i + .5f) * width, top - 1.5f), 2.6f, -1, go.transform);
                roots.color = new Color(.35f, .49f, .55f, .8f);
            }
        }

        GameObject Platform(float x, float top, float width, Vector2? travel = null, bool crumble = false, Transform parent = null)
        {
            var go = new GameObject(crumble ? "Weathered timber crossing" : travel.HasValue ? "Banyan suspended wooden lift" : "Rooted moss ledge");
            go.layer = 8; go.transform.SetParent(parent ? parent : terrain); go.transform.position = new Vector2(x, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(width, .55f); c.offset = new Vector2(0, -.275f);
            bool timber = crumble || travel.HasValue;
            go.AddComponent<PlatformSurface>().timber = timber;
            int count = timber ? 1 : Mathf.CeilToInt(width / 2.8f);
            for (int i = 0; i < count; i++) SurfaceTile(go.transform, x - width * .5f + width / count * (i + .5f), top, width / count + .03f, timber);
            if (travel.HasValue)
            {
                var m = go.AddComponent<MovingPlatform>(); m.travel = travel.Value; m.period = 4.8f; moving.Add(m);
                var branch = Prop(14, new Vector2(x - 1.6f, top + 5.1f), 1.7f, -3);
                branch.transform.localScale = new Vector3((width + 2.6f) / branch.sprite.bounds.size.x, 1.7f / branch.sprite.bounds.size.y, 1);
                var roots = Prop(15, new Vector2(x - width * .62f, top + 4.7f), 9.5f, -4);
                roots.color = new Color(.36f, .51f, .48f);
                go.AddComponent<LiftSuspension>().Initialize(new Vector2(x, top + 3.7f), width);
            }
            else if (!crumble)
            {
                var roots = Prop(15, new Vector2(x, top - .65f), 3.4f, -1, go.transform);
                roots.transform.localScale = new Vector3(width * .8f / roots.sprite.bounds.size.x, 3.4f / roots.sprite.bounds.size.y, 1);
                roots.color = new Color(.55f, .68f, .62f);
            }
            if (crumble) falling.Add(go.AddComponent<FallingPlatform>());
            return go;
        }

        void SurfaceTile(Transform parent, float x, float y, float width, bool wood)
        {
            int variant = tile++;
            bool registered = !wood && game.art.mossPlatforms != null && game.art.mossPlatforms.Length == 4;
            var sprite = wood ? game.art.environment[4] : registered ? game.art.mossPlatforms[(variant + variant / 4) % 4] : game.art.environment[variant % 4];
            var sr = Visuals.Sprite(wood ? "Weathered bridge planks" : "Registered moss and stone " + variant % 4, sprite,
                new Vector2(x, y + (wood ? .65f : registered ? 0 : .35f)), parent, 3);
            float height = wood ? 1.1f : registered ? width / 3f : 1.45f;
            sr.transform.localScale = new Vector3(width / sr.sprite.bounds.size.x, height / sr.sprite.bounds.size.y, 1);
            if (registered)
            {
                sr.flipX = variant % 3 == 1;
                // Small material variation within the original forest palette.
                sr.color = variant % 5 == 0 ? new Color(.91f, .97f, 1) : variant % 5 == 2 ? new Color(1, .97f, .90f) : Color.white;
                PlatformSurface.AddMossLip(sr);
            }
        }

        SpriteRenderer Prop(int index, Vector2 pos, float height, int order = 1, Transform parent = null)
        {
            var s = Visuals.Sprite("Forest prop " + index, game.art.environment[index], pos, parent ? parent : decoration, order);
            s.transform.localScale = Vector3.one * (height / s.sprite.bounds.size.y); return s;
        }

        void Enemy(bool cat, float x, float y, float min, float max)
        {
            var go = new GameObject(cat ? "Moon cat - telegraphed pounce" : "Forest dog - patrol and dash"); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            var e = go.AddComponent<ForestCreature>(); e.isCat = cat; e.left = min; e.right = max; e.floor = y;
            e.body = Visuals.Sprite("Animated sprite", game.art.Creature(cat, 0, 0), go.transform.position, go.transform, 10, cat ? .86f : 1);
            var hit = go.AddComponent<BoxCollider2D>(); hit.isTrigger = true; hit.size = new Vector2(cat ? 1.05f : 1.25f, .76f); hit.offset = new Vector2(0, .4f);
            e.Initialize(game); game.enemies.Add(e);
        }

        void Checkpoint(float x, float y, int index)
        {
            var go = new GameObject("Lantern checkpoint " + index); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            // Bottom pivot props are positioned with their top at y + height.
            Prop(12, new Vector2(x, y + 2.8f), 2.8f, 2, go.transform);
            var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true; c.size = new Vector2(2, 3); c.offset = new Vector2(0, 1.4f);
            var p = go.AddComponent<ForestPickup>(); p.kind = PickupKind.Checkpoint; p.index = index; p.respawn = new Vector2(x + 1.5f, y + .12f); pickups.Add(p);
            var halo = Visuals.Halo("Checkpoint breathing light", new Vector2(x, y + 1.2f), 4, new Color(1, .7f, .2f, .4f), go.transform, 5);
            var a = halo.gameObject.AddComponent<AmbientMotion>(); a.flicker = true; a.sway = 0; a.speed = 1.4f;
        }

        void Mote(float x, float y)
        {
            var go = new GameObject("Optional moon token"); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            go.AddComponent<MoonTokenVisual>().Initialize(game.art.moonToken ? game.art.moonToken : game.art.moon);
            go.AddComponent<CircleCollider2D>().isTrigger = true; go.GetComponent<CircleCollider2D>().radius = .42f;
            var p = go.AddComponent<ForestPickup>(); p.kind = PickupKind.Mote; pickups.Add(p);
        }

        void Moon(float x, float y)
        {
            var go = new GameObject("Moon fragment 1 of 4"); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            var s = Visuals.Sprite("The first moon fragment", game.art.moon, new Vector2(x, y), go.transform, 16);
            s.transform.localScale = Vector3.one * (1.85f / s.sprite.bounds.size.y);
            Visuals.Halo("Moon radiance", new Vector2(x, y), 7, new Color(.5f, 1, .85f, .7f), go.transform, 14);
            var a = go.AddComponent<AmbientMotion>(); a.sway = 6; a.bob = .17f; a.speed = 1.6f;
            var c = go.AddComponent<CircleCollider2D>(); c.radius = .9f; c.isTrigger = true;
            var p = go.AddComponent<ForestPickup>(); p.kind = PickupKind.Moon; pickups.Add(p);
            Prop(5, new Vector2(x, y - 1), 1, 2);
        }

        void BuildSwitchBridge()
        {
            var group = new GameObject("Light bridge - activated with E"); group.transform.SetParent(terrain);
            for (int i = 0; i < 3; i++)
            {
                var p = Platform(179 + i * 3.6f, 1, 3.7f, null, false, group.transform);
                Visuals.Halo("Moonstone light", p.transform.position, 3, new Color(.2f, .8f, 1, .4f), p.transform, 4);
            }
            group.SetActive(false);
            var shrine = new GameObject("Interact - awaken the bridge"); shrine.transform.SetParent(encounters); shrine.transform.position = new Vector2(172.5f, .5f);
            Prop(12, new Vector2(172.5f, 3.3f), 2.8f, 2, shrine.transform);
            lanternSwitch = shrine.AddComponent<LanternSwitch>(); lanternSwitch.bridge = group;
        }

        void Dress()
        {
            float[,] places = { {3,0},{7,0},{15,0},{31,.5f},{39,.5f},{67,.2f},{75,.2f},{103,1},{111,1},{136,3.4f},{144,3.4f},{159,.5f},{169,.5f},{192,1},{205,1},{229,6},{244,6},
                {264,3},{273,3},{292,4.5f},{301,4.5f},{320,5},{330,5},{356,9.1f},{369,9.1f} };
            for (int i = 0; i < places.GetLength(0); i++)
            {
                float x = places[i, 0], y = places[i, 1];
                var fern = Prop(i % 3 == 0 ? 10 : 7, new Vector2(x, y + .65f), .8f, 2);
                var motion = fern.gameObject.AddComponent<AmbientMotion>(); motion.sway = 3; motion.speed = 1.6f; motion.phase = x;
                motion.windAffected = true;
                if (i % 2 == 0)
                {
                    var bamboo = Prop(9, new Vector2(x - 1.3f, y + 5.5f), 5.5f, -7); bamboo.color = new Color(.28f, .52f, .51f, .7f);
                    var m = bamboo.gameObject.AddComponent<AmbientMotion>(); m.sway = 1.3f; m.speed = .7f; m.phase = x;
                    m.windAffected = true;
                }
                var vine = Prop(8, new Vector2(x + 3, y + 11), 4.3f, 21); vine.color = new Color(.23f, .47f, .43f, .8f);
                var v = vine.gameObject.AddComponent<AmbientMotion>(); v.sway = 2; v.phase = x; v.speed = .75f;
                v.windAffected = true;
            }
            foreach (float x in new[] { 8f, 37f, 72f, 110f, 141f, 169f, 195f, 231f })
            {
                float floor = x > 225 ? 6 : x > 133 && x < 147 ? 3.4f : x > 190 ? 1 : .3f;
                var lamp = Prop(11, new Vector2(x, floor + 4.5f), 1.5f, 2);
                var motion = lamp.gameObject.AddComponent<AmbientMotion>(); motion.sway = 4; motion.speed = 1.2f; motion.phase = x;
                var halo = Visuals.Halo("Warm hanging lantern pool", new Vector2(x, floor + 3.5f), 5, new Color(1, .6f, .18f, .22f), decoration, 4);
                var a = halo.gameObject.AddComponent<AmbientMotion>(); a.flicker = true; a.sway = 0; a.phase = x;
            }
        }

        public void ResetLevel(bool full)
        {
            foreach (var p in falling) p.ResetPlatform();
            foreach (var p in moving) p.ResetPlatform();
            foreach (var e in game.enemies) e.ResetCreature();
            if (!full) return;
            foreach (var p in pickups) p.ResetPickup(); lanternSwitch.ResetSwitch();
        }
    }
}
