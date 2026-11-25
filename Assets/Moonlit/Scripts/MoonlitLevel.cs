using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitLevel : MonoBehaviour
    {
        public const float Length = 647, GoalX = 640, GoalFloor = 1, LeftPlayerLimit = 1.25f;
        public float RouteLength { get; private set; }
        public float EndX { get; private set; }
        public float EndFloor { get; private set; }
        public Vector2 StartPoint => new Vector2(3, .12f);
        public Vector2 BossFoot => new Vector2(409, 2);
        public readonly List<FallingPlatform> falling = new List<FallingPlatform>();
        public readonly List<MovingPlatform> moving = new List<MovingPlatform>();
        public readonly List<ForestPickup> pickups = new List<ForestPickup>();
        public LanternSwitch lanternSwitch;
        public PushableCrate crate;
        public LanternProcession Procession { get; private set; }
        readonly List<BoxCollider2D> land = new List<BoxCollider2D>();
        public Transform firstJumpTarget;
        MoonlitGame game;
        Transform terrain, decoration, encounters;
        int stage;
        int moteCount;

        public void Build(MoonlitGame owner)
        {
            game = owner; stage = owner.Chapter;
            terrain = Group("Terrain and platforms"); decoration = Group("Village and forest"); encounters = Group("Encounters and pickups");
            switch (stage)
            {
                case 1: Village(); break;
                case 2: Bamboo(); break;
                case 3: River(); break;
                case 4: Hill(); break;
                default: throw new System.ArgumentOutOfRangeException(nameof(owner.Chapter));
            }
            TerrainArt.DressBanks(stage, land);
            // Small painted waymarks repeat the same symbols used in the nearby control hints.
            switch (stage)
            {
                case 1: Sign(6, 0, 0); Sign(20, 0, 1); Sign(44, 0, 1); Sign(63, 0, 2); Sign(116, 0, 3); break;
                case 2: Sign(10, 0, 1); Sign(105, 2.6f, 1); break;
                case 3: Sign(18, 0, 4); Sign(105, 3.1f, 2); break;
                case 4: Sign(12, 0, 5); Sign(82, 5.2f, 5); break;
            }
            PopulateWildlife();
            game.TotalMotes = moteCount;
            Wall(LeftPlayerLimit - .575f, 4); Wall(RouteLength + .55f, 5);
            Moon(EndX, EndFloor + 1.6f);
        }

        void Village()
        {
            RouteLength = Length; EndX = GoalX; EndFloor = GoalFloor;
            Ground(-5, 22, 0); firstJumpTarget = Ground(22, 28, 1).transform;
            Ground(28, 33, 2); Ground(35, 46, 0);
            BambooPlatform(49, .85f, 2.8f); BambooPlatform(53.4f, 1.75f, 2.85f);
            var courtyard = Ground(56, 92, 0);
            Checkpoint(60, 0, 1); Enemy(false, 72, 68, 76, courtyard);
            Ground(95, 101, 0);
            var alley = Ground(104, 124, 0);
            Landmark("VillageCottage", 110, 0, 5f, new Color(.58f, .62f, .68f));
            Enemy(true, 110, 107, 113, alley, 3.7f);
            Crate(118, 0);
            Ground(124, 130, 3.65f); Ground(130, 136, 1.1f);
            var bridgeHead = Ground(136, 156, 0);
            Checkpoint(140, 0, 2); Enemy(false, 147, 144, 151, bridgeHead);
            Enemy(true, 152, 149, 154, bridgeHead, 3);
            BambooPlatform(159, .1f, 3.4f, new Vector2(1.15f, 0));
            Ground(162, 180, 0); VillageJourney();
            Landmark("VillageGate", 8, 0, 5.3f, new Color(.85f, .89f, .89f));
            Landmark("VillageCottage", 17, -.12f, 3.2f, new Color(.48f, .56f, .66f));
            Landmark("VillageCottage", 68, -.12f, 4.3f, new Color(.51f, .58f, .66f));
            Landmark("VillageGate", 83, -.1f, 3.55f, new Color(.50f, .58f, .62f));
            Landmark("YoungBanyan", 174, -.12f, 8.2f, new Color(.53f, .68f, .66f));
            Water(44, 58, -1.25f); Water(153, 166, -1.6f);
            Foliage(0, 0); Foliage(19, 0); Foliage(38, 0); Foliage(57, 0); Foliage(88, 0);
            Foliage(103, 0); Foliage(134, 1.1f); Foliage(170, 0);
            HangLantern(10, 5); HangLantern(63, 3.6f); HangLantern(138, 4.4f);
            Trail(11, 1.3f, 20, 1.6f, 24, 2.6f, 30, 3.6f, 34, 3.45f, 43, 1.5f, 49, 2.35f, 53.4f, 3.2f, 64, 1.4f, 80, 1.4f,
                  94, 2.1f, 102.6f, 2.1f, 115, 1.4f, 124.8f, 5.1f, 133, 2.6f, 144, 1.4f, 159, 1.8f, 168, 1.4f);
        }

        void Bamboo()
        {
            RouteLength = 637; EndX = 630; EndFloor = 6;
            Ground(-5, 19, 0);
            firstJumpTarget = Ground(19, 29, 3.8f).transform;    // First double jump has a safe landing below.
            BambooPlatform(34, 4.55f, 4.1f); BambooPlatform(41, 5.45f, 4.6f);
            var glade = Ground(46, 58, 4.6f); Checkpoint(48, 4.6f, 1); Enemy(false, 54, 51, 56, glade);
            Platform(62.3f, 5.9f, 3.6f, new Vector2(0, .7f)); Platform(69.4f, 7.2f, 4.3f);
            BambooPlatform(77.1f, 6.4f, 3.7f); var arch = Ground(83, 94, 4.1f); Enemy(true, 89, 86, 92, arch, 2.8f);
            Ground(96, 107, 2.6f); Checkpoint(102, 2.6f, 2);
            Platform(111.2f, 3.6f, 3.7f, null, true); Platform(117.6f, 4.8f, 3.7f, null, true);
            Ground(122, 132, 7.8f); BambooPlatform(136.3f, 6.2f, 4.4f);
            Ground(141, 157, 2.8f); BambooJourney();
            for (int i = 0; i < 18; i++)
            {
                float x = i * 8.8f; float y = x < 19 ? 0 : x < 59 ? 3.8f : x < 82 ? 5.2f : x < 108 ? 2.6f : x < 141 ? 4.8f : 2.8f;
                Foliage(x, y, true);
            }
            HangLantern(8, 5); HangLantern(48, 9.2f); HangLantern(102, 7); HangLantern(149, 7.8f);
            Trail(11, 1.4f, 18.1f, 2.4f, 19.7f, 5.1f, 28, 5.4f, 34, 6f, 41, 6.9f, 50, 6.2f, 62.3f, 8.15f, 69.4f, 8.9f,
                  77.1f, 8, 88, 6, 99, 4.1f, 111.2f, 5.3f, 117.6f, 6.55f, 124, 9.3f, 136.3f, 7.9f, 146, 4.4f);
        }

        void River()
        {
            RouteLength = 640; EndX = 632; EndFloor = 2.1f;
            Ground(-5, 24, 0); firstJumpTarget = Ground(24, 42, 6.4f).transform;
            VineWall(24, 0, 6.4f); Checkpoint(39, 6.4f, 1);
            BambooPlatform(46, 4.9f, 4); BambooPlatform(53.4f, 4.15f, 4.6f, new Vector2(1.1f, 0));
            BambooPlatform(61.8f, 3.1f, 4.8f, new Vector2(1.25f, 0)); var jetty = Ground(67, 77, 1.8f);
            Enemy(true, 73, 70, 75, jetty, 2.75f); Ground(77, 87, 8.15f); VineWall(77, 1.8f, 6.35f);
            BambooPlatform(91, 6.1f, 4);
            var dock = Ground(96, 113, 3.1f); Checkpoint(99, 3.1f, 2); BuildSwitchBridge(108, 3.1f, 113, 129);
            var farBank = Ground(129, 141, 3.1f); Enemy(false, 136, 132, 139, farBank);
            Ground(141, 156, 1.4f); RiverJourney();
            Water(-4, 159, -.95f);
            for (int i = 0; i < 12; i++) FloatingLantern(2 + i * 12.8f, -.9f, i);

            Landmark("RiverSampan", 57, -.65f, 3f, new Color(.66f, .74f, .78f));
            Landmark("RiverSampan", 121, -.03f, 3.2f, new Color(.64f, .72f, .77f));
            Landmark("VillageCottage", 100, 3f, 3.8f, new Color(.53f, .59f, .68f));
            Foliage(4, 0); Foliage(37, 6.4f); Foliage(81, 8.15f); Foliage(143, EndFloor);
            HangLantern(26, 10.2f); HangLantern(70, 6.1f); HangLantern(108, 7); HangLantern(148, 5.3f);
            Trail(13, 1.45f, 23.5f, 2.4f, 23.6f, 4.65f, 26, 7.9f, 39, 8f, 46, 6.55f, 53.4f, 5.8f, 61.8f, 4.75f,
                  70, 3.35f, 76.6f, 4.1f, 76.6f, 6.4f, 81, 9.7f, 91, 7.7f, 103, 4.8f, 117, 4.7f, 125, 4.7f, 145, 3.1f);
        }

        void Hill()
        {
            RouteLength = 414; EndX = 391; EndFloor = 2;
            firstJumpTarget = Ground(-5, 16, 0).transform; Ground(26.5f, 41, .1f); Checkpoint(37, .1f, 1);
            Ground(41, 51, 6.3f); VineWall(41, .1f, 6.2f);
            Platform(56.2f, 7, 4.4f, null, true); Platform(63.9f, 7.65f, 4.5f, null, true);
            var ridge = Ground(70, 84, 5.2f); Checkpoint(80, 5.2f, 2); Enemy(false, 74, 72, 77, ridge);
            Ground(94.5f, 106, 5.5f); Platform(111, 6.55f, 4.2f, new Vector2(0, .65f));
            Ground(117, 126, 7.1f); Ground(129, 134, 4.4f); Ground(134, 149, EndFloor); Checkpoint(136, EndFloor, 3); HillJourney();
            Landmark("YoungBanyan", 34, 0, 6.7f, new Color(.39f, .50f, .59f));
            Landmark("YoungBanyan", 76, 5.1f, 7.8f, new Color(.39f, .49f, .59f));
            Landmark("YoungBanyan", 120, 7f, 8f, new Color(.37f, .45f, .55f));
            Foliage(6, 0); Foliage(45, 6.3f); Foliage(99, 5.5f); Foliage(132, 4.4f);
            HangLantern(12, 4.1f); HangLantern(39, 4.4f); HangLantern(82, 9.5f);
            Trail(10, 1.4f, 15.4f, 2f, 19, 3.2f, 23, 3.1f, 28, 1.7f, 40.6f, 2.8f, 40.6f, 5f, 44, 7.8f, 56.2f, 8.6f,
                  63.9f, 9.2f, 78, 6.8f, 83.5f, 7f, 87, 8.2f, 91, 8.2f, 97, 7.1f, 111, 8.5f, 119, 8.6f, 132, 5.9f);
        }

        public string AreaName(float x)
        {
            if (x >= ExtensionStart) return JourneyArea(x);
            switch (stage)
            {
                case 1: return x < 20 ? "Cổng làng" : x < 40 ? "Bờ ruộng" : x < 58 ? "Mương tre" : x < 85 ? "Sân đình" : x < 112 ? "Ngõ tối" : x < 138 ? "Tường đất" : x < 162 ? "Cầu tre" : "Gốc đa làng";
                case 2: return x < 30 ? "Lối vào rừng" : x < 60 ? "Vòm trúc" : x < 96 ? "Tán tre cao" : x < 133 ? "Cầu gỗ mục" : "Miếu trong rừng";
                case 3: return x < 24 ? "Bến nước" : x < 45 ? "Bờ kè" : x < 77 ? "Dòng hoa đăng" : x < 96 ? "Vách dây leo" : x < 129 ? "Miếu bến đò" : "Bờ bên kia";
                default: return x < 41 ? "Khe gió" : x < 70 ? "Sườn rễ" : x < 106 ? "Sống đồi" : x < 134 ? "Bậc đá cổ" : "Tán Đa thần";
            }
        }

        public string HintAt(float x)
        {
            if (stage == 2 && x > 146 && x < 156) return "Quạ xòe cánh! Rời chỗ vừa đứng khi nghe tiếng kêu";
            if (x >= ExtensionStart) return JourneyHint(x);
            switch (stage)
            {
                case 1:
                    if (x < 9) return "A / D hoặc ← →   ·   Chạy";
                    if (x > 17 && x < 24) return "Giữ Space để nhảy cao";
                    if (x > 41 && x < 49) return "Nhảy lên những bó tre giữa mương";
                    if (x > 58 && x < 68) return "J / X   ·   Bùng sáng khi chó đến gần";
                    if (x > 104 && x < 109) return "Cẩn thận mèo trên mái!";
                    if (x > 113 && x < 124) return "Đẩy thùng đến chân tường rồi nhảy lên";
                    if (x > 151 && x < 159) return "Chờ bè tới gần rồi nhảy lên";
                    break;
                case 2:
                    if (x < 20) return "Space lần nữa trên không   ·   Nhảy đôi";
                    if (x > 104 && x < 113) return "Ván đang rung! Hãy nhảy tiếp";
                    break;
                case 3:
                    if ((x > 14 && x < 25) || (x > 72 && x < 78)) return "Giữ hướng vào vách và giữ Space để leo";
                    if (x > 102 && x < 114 && lanternSwitch && !lanternSwitch.Activated) return "Đến gần miếu và bấm J hoặc E để nối cầu";
                    break;
                case 4:
                    if (x < 18 || x > 80 && x < 86) return "Nhảy đôi rồi nhấn Shift / K để lướt qua khe";
                    break;
            }
            return "";
        }

        public void PulseAt(Vector2 at)
        {
            if (lanternSwitch && Vector2.Distance(at, (Vector2)lanternSwitch.transform.position + Vector2.up) < game.tuning.pulseRadius)
                lanternSwitch.Activate(true);
        }
        Transform Group(string name) { var go = new GameObject(name); go.transform.SetParent(transform); return go.transform; }

        void Wall(float x, float y)
        {
            var go = new GameObject("Level boundary"); go.layer = 8; go.transform.SetParent(terrain); go.transform.position = new Vector2(x, y);
            go.AddComponent<BoxCollider2D>().size = new Vector2(.5f, 30);
        }

        BoxCollider2D Ground(float start, float end, float top)
        {
            var go = new GameObject($"Ground [{start}, {end}] top {top}"); go.layer = 8; go.transform.SetParent(terrain); go.transform.position = new Vector2((start + end) * .5f, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(end - start, 12); c.offset = new Vector2(0, -6);
            go.AddComponent<PlatformSurface>();
            land.Add(c);
            return c;
        }

        GameObject Platform(float x, float top, float width, Vector2? travel = null, bool crumble = false, Transform parent = null)
        {
            var go = new GameObject(crumble ? "Weathered timber crossing" : travel.HasValue ? "Banyan suspended wooden lift" : "Rooted moss ledge");
            go.layer = 8; go.transform.SetParent(parent ? parent : terrain); go.transform.position = new Vector2(x, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(width, .55f); c.offset = new Vector2(0, -.275f);
            bool timber = crumble || travel.HasValue;
            go.AddComponent<PlatformSurface>().timber = timber;
            TerrainArt.DressLedge(game, stage, go, x, top, width, timber);
            if (travel.HasValue)
            {
                var m = go.AddComponent<MovingPlatform>(); m.travel = travel.Value; m.period = 4.8f; moving.Add(m);
                SupportLift(x, top, width);
                go.AddComponent<LiftSuspension>().Initialize(new Vector2(x, top + 3.7f), width);
            }
            else SupportDeck(x, top, width, crumble);
            if (crumble) falling.Add(go.AddComponent<FallingPlatform>());
            return go;
        }

        SpriteRenderer Prop(int index, Vector2 pos, float height, int order = 1, Transform parent = null)
        {
            var s = Visuals.Sprite("Forest prop " + index, game.art.environment[index], pos, parent ? parent : decoration, order);
            s.transform.localScale = Vector3.one * (height / s.sprite.bounds.size.y); return s;
        }

        void Enemy(bool cat, float x, float min, float max, BoxCollider2D ground, float perchHeight = 0)
        {
            float y = ground.transform.position.y;
            var go = new GameObject(cat ? "Moon cat - telegraphed pounce" : "Forest dog - patrol and dash"); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y + perchHeight);
            if (cat && perchHeight > 0 && !(stage == 1 && x < 120))
            {
                var branch = Prop(14, new Vector2(x, y + perchHeight + .12f), .92f, -2);
                branch.transform.localScale = new Vector3(3.25f / branch.sprite.bounds.size.x, .92f / branch.sprite.bounds.size.y, 1);
                var stalk = Prop(9, new Vector2(x + 1.5f, y + perchHeight + 1.1f), perchHeight + 1.1f, -6);
                stalk.color = new Color(.53f, .67f, .59f);
            }
            var e = go.AddComponent<ForestCreature>(); e.isCat = cat; e.left = min; e.right = max; e.floor = y; e.perchHeight = perchHeight;
            e.body = Visuals.Sprite("Animated sprite", game.art.Creature(cat, 0, 0), go.transform.position, go.transform, 10, cat ? .86f : 1);
            var hit = go.AddComponent<BoxCollider2D>(); hit.isTrigger = true; hit.size = new Vector2(cat ? 1.05f : 1.25f, .76f); hit.offset = new Vector2(0, .4f);
            e.Initialize(game, ground); game.enemies.Add(e);
        }

        void Checkpoint(float x, float y, int index)
        {
            var go = new GameObject("Lantern checkpoint " + index); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            // Bottom pivot props are positioned with their top at y + height.
            Prop(12, new Vector2(x, y + 2.8f), 2.8f, 2, go.transform);
            var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true; c.size = new Vector2(2, 3); c.offset = new Vector2(0, 1.4f);
            var p = go.AddComponent<ForestPickup>(); p.kind = PickupKind.Checkpoint; p.index = index; p.respawn = new Vector2(x + 1.5f, y + .12f); pickups.Add(p);
            var halo = Visuals.Halo("Checkpoint breathing light", new Vector2(x, y + 1.2f), 4, new Color(1, .7f, .2f, .4f), go.transform, 5, lightScenery: true);
            p.checkpointLight = halo; p.SetCheckpointLit(false);
        }

        void Mote(float x, float y)
        {
            var go = new GameObject("Optional moon token"); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            go.AddComponent<MoonTokenVisual>().Initialize();
            go.AddComponent<CircleCollider2D>().isTrigger = true; go.GetComponent<CircleCollider2D>().radius = .42f;
            var p = go.AddComponent<ForestPickup>(); p.kind = PickupKind.Mote; pickups.Add(p);
        }

        void Moon(float x, float y)
        {
            var go = new GameObject("Moon fragment " + stage + " of 4"); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            var s = Visuals.Sprite("Moon fragment", game.art.moon, new Vector2(x, y), go.transform, 16);
            s.transform.localScale = Vector3.one * (1.85f / s.sprite.bounds.size.y);
            Visuals.Halo("Moon radiance", new Vector2(x, y), 7, new Color(.5f, 1, .85f, .7f), go.transform, 14);
            var a = go.AddComponent<AmbientMotion>(); a.sway = 6; a.bob = .17f; a.speed = 1.6f;
            var c = go.AddComponent<CircleCollider2D>(); c.radius = .9f; c.isTrigger = true;
            var p = go.AddComponent<ForestPickup>(); p.kind = PickupKind.Moon; pickups.Add(p);
            Prop(5, new Vector2(x, y - 1), 1, 2);
        }


        void BuildSwitchBridge(float x, float top, float start, float end)
        {
            var group = new GameObject("Bridge of floating lanterns"); group.transform.SetParent(terrain);
            const int planks = 4;
            float width = (end - start) / planks;
            for (int i = 0; i < planks; i++)
            {
                float center = start + (i + .5f) * width;
                var p = BambooPlatform(center, top, width + .04f);
                p.transform.SetParent(group.transform, true);
                Visuals.Halo("Awakened lantern", new Vector2(center, top - .28f), 2.8f, new Color(1, .64f, .24f, .55f), p.transform, 4);
            }
            group.SetActive(false);
            var shrine = new GameObject("River shrine"); shrine.transform.SetParent(encounters); shrine.transform.position = new Vector2(x, top);
            Prop(12, new Vector2(x, top + 2.8f), 2.8f, 2, shrine.transform);
            lanternSwitch = shrine.AddComponent<LanternSwitch>(); lanternSwitch.bridge = group;
        }

        void Crate(float x, float floor)
        {
            var go = new GameObject("Pushable wooden crate"); go.layer = 8; go.transform.SetParent(terrain);
            go.transform.position = new Vector2(x, floor + .675f);
            crate = go.AddComponent<PushableCrate>();
            var sr = Visuals.Sprite("Bamboo crate", game.art.forestExtras[4], new Vector2(x, floor + 1.34f), go.transform, 5);
            sr.transform.localScale = new Vector3(1.37f / sr.sprite.bounds.size.x, 1.37f / sr.sprite.bounds.size.y, 1);
            go.AddComponent<PlatformSurface>().timber = true;
        }

        GameObject BambooPlatform(float x, float top, float width, Vector2? travel = null)
        {
            var go = new GameObject(travel.HasValue ? "Floating bamboo raft" : "Lashed bamboo platform"); go.layer = 8;
            go.transform.SetParent(terrain); go.transform.position = new Vector2(x, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(width, .28f); c.offset = new Vector2(0, -.14f);
            c.usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>(); effector.useOneWay = true; effector.surfaceArc = 155;
            go.AddComponent<PlatformSurface>().timber = true;
            // Preserve the braided lashings and round cane cross-sections in the painted raft sprite.
            var bundle = game.art.forestExtras[1];
            float bodyHeight = Mathf.Clamp(width * .2f, .48f, .92f);
            float verticalScale = bodyHeight / bundle.bounds.size.y;
            var bamboo = Visuals.Sprite("Woven bamboo", bundle, new Vector2(x, top - bundle.bounds.max.y * verticalScale + .025f), go.transform, 4);
            bamboo.transform.localScale = new Vector3(width / bundle.bounds.size.x, verticalScale, 1);
            if (!travel.HasValue) SupportDeck(x, top, width, false, go.transform);
            if (travel.HasValue)
            {
                var mover = go.AddComponent<MovingPlatform>(); mover.travel = travel.Value; mover.period = 4.65f; moving.Add(mover);
            }
            return go;
        }

        void VineWall(float x, float bottom, float height)
        {
            var roots = Prop(8, new Vector2(x + .1f, bottom + height + .05f), height + .1f, 5);
            roots.color = new Color(.80f, .94f, .71f);
            DressClimbCord(x, bottom, height);
        }

        static readonly Dictionary<int, Sprite> waymarks = new Dictionary<int, Sprite>();
        void Sign(float x, float y, int symbol)
        {
            if (!waymarks.TryGetValue(symbol, out Sprite mark))
            {
                const int width = 30, height = 32;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "Painted bamboo waymark", filterMode = FilterMode.Point };
                var pixels = new Color32[width * height];
                Color32 wood = new Color32(106, 65, 41, 255), side = new Color32(67, 45, 39, 255), light = new Color32(192, 136, 75, 255);
                Color32 ink = new Color32(255, 226, 158, 255);
                for (int row = 0; row < height; row++)
                    for (int col = 0; col < width; col++)
                    {
                        if (col >= 11 && col <= 15 && row < 17) pixels[row * width + col] = col == 12 ? light : side;
                        if (row < 12 || row > 29 || col < 1 || col > 27) continue;
                        if ((row == 12 || row == 29) && (col == 1 || col == 27)) continue;
                        pixels[row * width + col] = row == 29 || col == 1 ? light : row == 12 || col == 27 ? side : wood;
                        if ((row == 15 || row == 26) && (col == 4 || col == 24)) pixels[row * width + col] = side;
                    }
                // Nine-pixel folk pictograms for running, jumping, lantern, crate, climbing and dash.
                string[] glyphs ={
                    "........./...##..../....##.../.....##../.#######./.....##../....##.../...##..../.........",
                    "....#..../...###.../..#####../....#..../....#..../...##..../..##...../.##....../.........",
                    "....#..../....#..../...###.../#########/..#####../...###.../..##.##../..#...#../.........",
                    "..#####../..#...#../..##.##../..#.###../..##.##../..#...#../..#####../........./.........",
                    "......##./....####./..######./....#.##./....#.##./....#.##./....#.##./......##./.........",
                    ".....#.../.##...#../.....###./..######./.....###./.##...#../.....#.../........./........."};
                string[] lines = glyphs[symbol].Split('/');
                for (int row = 0; row < lines.Length; row++)
                    for (int col = 0; col < lines[row].Length; col++)
                        if (lines[row][col] == '#') pixels[(24 - row) * width + col + 10] = ink;
                texture.SetPixels32(pixels); texture.Apply();
                mark = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, 0), 20, 0, SpriteMeshType.FullRect);
                waymarks.Add(symbol, mark);
            }
            Visuals.Sprite("Painted sign", mark, new Vector2(x, y), decoration, 2);
        }

        static Material riverMaterial;
        static readonly int Waterline = Shader.PropertyToID("_Waterline");

        void Water(float start, float end, float y)
        {
            if (!riverMaterial)
                riverMaterial = new Material(Resources.Load<Shader>("RiverSurface")) { name = "Nước và bóng bờ" };
            // The water tints the submerged ends of pier piles. Its level also anchors fish and boats.
            const float depth = 8, crest = .12f;
            var water = Visuals.Box("Dòng nước", new Vector2((start + end) * .5f, y + (crest - depth) * .5f),
                new Vector2(end - start, depth + crest), Color.white, decoration, -2);
            water.sharedMaterial = riverMaterial;
            var properties = new MaterialPropertyBlock();
            water.GetPropertyBlock(properties);
            properties.SetVector(Waterline, new Vector4(y, stage, start * .37f, 0));
            water.SetPropertyBlock(properties);
            PlantWaterMargins(start, end, y);
        }

        void FloatingLantern(float x, float y, int seed)
        {
            var floatRoot = new GameObject("Floating flower lantern").transform; floatRoot.SetParent(decoration);
            floatRoot.position = new Vector2(x, y);
            Visuals.Box("Folded paper hull", new Vector2(x, y + .1f), new Vector2(.61f, .15f), new Color(.95f, .49f, .26f), floatRoot, -8);
            var candle = Visuals.Box("Candle", new Vector2(x, y + .24f), new Vector2(.14f, .2f), new Color(1, .90f, .52f), floatRoot, -7);
            for (int i = 0; i < 2; i++)
            {
                var petal = Visuals.Box("Lotus petal", new Vector2(x + (i == 0 ? -.22f : .22f), y + .23f), new Vector2(.21f, .29f), new Color(1, .55f, .49f), floatRoot, -7);
                petal.transform.rotation = Quaternion.Euler(0, 0, i == 0 ? 30 : -30);
            }
            Visuals.Halo("Candlelight", candle.transform.position, 2.3f, new Color(1, .56f, .21f, .6f), floatRoot, -9, lightScenery: true);
            var bob = floatRoot.gameObject.AddComponent<AmbientMotion>(); bob.bob = .06f; bob.sway = 2; bob.phase = seed; bob.speed = 1.25f;
        }

        void Trail(params float[] points)
        {
            for (int i = 0; i < points.Length; i += 2) Mote(points[i], points[i + 1]);
            moteCount += points.Length / 2;
        }

        public void ResetLevel(bool full)
        {
            foreach (var platform in falling) platform.ResetPlatform();
            foreach (var platform in moving) platform.ResetPlatform();
            foreach (var enemy in game.enemies) enemy.ResetCreature();
            foreach (var animal in Wildlife) animal.ResetCreature();
            if (crate && (full || game.RespawnPoint.x < 124)) crate.ResetCrate();
            if (!full) return;
            foreach (var pickup in pickups) pickup.ResetPickup();
            if (lanternSwitch) lanternSwitch.ResetSwitch();
        }
    }
}
