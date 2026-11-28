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
        public Vector2 StartPoint => game.ChapterSource.spawn.position;
        public Vector2 BossFoot => game.ChapterSource.guardian ? (Vector2)game.ChapterSource.guardian.position : Vector2.zero;
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
        public const int MotesPerChapter = 10;
        readonly List<BoxCollider2D> chimeStations = new List<BoxCollider2D>(2);

        public void Build(MoonlitGame owner)
        {
            game = owner;
            var source = owner.ChapterSource;
            stage = source.chapter;
            RouteLength = source.boundary.position.x;
            EndX = source.moon.position.x;
            EndFloor = source.moon.position.y - 1.6f;
            terrain = Group("Terrain and platforms"); decoration = Group("Village and forest"); encounters = Group("Encounters and pickups");
            foreach (var bank in source.banks)
            {
                Vector2 top = bank.Top, size = bank.Size;
                Ground(top.x-size.x*.5f, top.x+size.x*.5f, top.y, size.y);
            }
            foreach (var slot in source.platforms)
            {
                Vector2 top = slot.Top;
                Vector2? travel = slot.travel == Vector2.zero ? (Vector2?)null : slot.travel;
                if (slot.kind == ChapterSurfaceKind.CanopyPerch || slot.kind == ChapterSurfaceKind.ChimePlatform)
                {
                    var perch = BambooPlatform(top.x, top.y, slot.Size.x, suspended: true);
                    perch.name = slot.name;
                    perch.GetComponent<PlatformSurface>().overhead = true;
                    var foot = new Vector2(top.x, -7);
                    var joint = top - Vector2.up * .43f;
                    Beam("Forked bamboo support", foot, joint, .21f, Timber, perch.transform, -4);
                    Beam("Bamboo fork", joint - Vector2.up * .95f, joint + Vector2.right * slot.Size.x * .36f, .15f, Timber, perch.transform, -4);
                    Beam("Bamboo fork", joint - Vector2.up * .95f, joint - Vector2.right * slot.Size.x * .36f, .15f, Timber, perch.transform, -4);
                    if (slot.kind == ChapterSurfaceKind.ChimePlatform) chimeStations.Add(perch.GetComponent<BoxCollider2D>());
                }
                else if (slot.kind == ChapterSurfaceKind.ExplorationDeck || slot.kind == ChapterSurfaceKind.ExplorationShelf)
                {
                    GameObject landing;
                    if (slot.kind == ChapterSurfaceKind.ExplorationDeck)
                        landing = BambooPlatform(top.x, top.y, slot.Size.x);
                    else
                        landing = Platform(top.x, top.y, slot.Size.x);
                    landing.name = slot.name;
                    landing.GetComponent<PlatformSurface>().overhead = true;
                }
                else if (slot.kind == ChapterSurfaceKind.BambooDeck)
                    BambooPlatform(top.x, top.y, slot.Size.x, travel);
                else
                    Platform(top.x, top.y, slot.Size.x, travel, slot.kind == ChapterSurfaceKind.WeatheredTimber);
            }
            for (int i = 0; i < source.checkpoints.Length; i++)
            {
                var point = source.checkpoints[i].position;
                Checkpoint(point.x, point.y, i+1);
            }
            switch (stage)
            {
                case 1: Village(); break;
                case 2: Bamboo(); break;
                case 3: River(); break;
                case 4: Hill(); break;
                default: throw new System.ArgumentOutOfRangeException(nameof(owner.Chapter));
            }
            BuildCrossing();
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
            if (source.hiddenMotes == null || source.hiddenMotes.Length != MotesPerChapter)
                throw new System.InvalidOperationException(source.name + " must define ten moon mote locations.");
            foreach (var marker in source.hiddenMotes) Mote(marker);
            game.TotalMotes = source.hiddenMotes.Length;
            Wall(LeftPlayerLimit - .575f, 4); Wall(RouteLength + .55f, 5);
            Moon(EndX, EndFloor + 1.6f);
        }

        void Village()
        {
            firstJumpTarget = land[1].transform;
            var courtyard = land[4];
            Enemy(false, 72, 68, 76, courtyard);
            var alley = land[6];
            Landmark("VillageCottage", 110, 0, 5f, new Color(.58f, .62f, .68f));
            Enemy(true, 110, 107, 113, alley, 3.7f);
            Crate(118, 0);
            var bridgeHead = land[9];
            Enemy(false, 147, 144, 151, bridgeHead);
            Enemy(true, 152, 149, 154, bridgeHead, 3);
            VillageJourney();
            Landmark("VillageGate", 8, 0, 5.3f, new Color(.85f, .89f, .89f));
            Landmark("VillageCottage", 17, -.12f, 3.2f, new Color(.48f, .56f, .66f));
            Landmark("VillageCottage", 68, -.12f, 4.3f, new Color(.51f, .58f, .66f));
            Landmark("VillageGate", 83, -.1f, 3.55f, new Color(.50f, .58f, .62f));
            Landmark("YoungBanyan", 174, -.12f, 8.2f, new Color(.53f, .68f, .66f));
            Water(44, 58, -1.25f); Water(153, 166, -1.6f);
            Foliage(0, 0); Foliage(19, 0); Foliage(38, 0); Foliage(57, 0); Foliage(88, 0);
            Foliage(103, 0); Foliage(134, 1.1f); Foliage(170, 0);
            HangLantern(10, 5); HangLantern(63, 3.6f); HangLantern(138, 4.4f);
        }

        void Bamboo()
        {

            firstJumpTarget = land[1].transform;    // First double jump has a safe landing below.
            var glade = land[2]; Enemy(false, 54, 51, 56, glade);
            var arch = land[3]; Enemy(true, 89, 86, 92, arch, 2.8f);
            BambooJourney();
            for (int i = 0; i < 18; i++)
            {
                float x = i * 8.8f; float y = x < 19 ? 0 : x < 59 ? 3.8f : x < 82 ? 5.2f : x < 108 ? 2.6f : x < 141 ? 4.8f : 2.8f;
                Foliage(x, y, true);
            }
            HangLantern(8, 5); HangLantern(48, 9.2f); HangLantern(102, 7); HangLantern(149, 7.8f);
        }

        void River()
        {

            firstJumpTarget = land[1].transform;
            VineWall(24, 0, 6.4f);
            var jetty = land[2];
            Enemy(true, 73, 70, 75, jetty, 2.75f); VineWall(77, 1.8f, 6.35f);
            BuildSwitchBridge(108, 3.1f, 113, 129);
            var farBank = land[5]; Enemy(false, 136, 132, 139, farBank);
            RiverJourney();
            Water(-4, 41.8f, -.95f);
            Water(41.8f, 95.8f, .22f);
            Water(95.8f, 159, .46f);
            for (int i = 0; i < 12; i++) FloatingLantern(2 + i * 12.8f, -.9f, i);

            MooredSampan(57, 3f, new Color(.66f, .74f, .78f));
            MooredSampan(121, 3.2f, new Color(.64f, .72f, .77f));
            Landmark("VillageCottage", 100, 3f, 3.8f, new Color(.53f, .59f, .68f));
            Foliage(4, 0); Foliage(37, 6.4f); Foliage(81, 8.15f); Foliage(143, EndFloor);
            HangLantern(26, 10.2f); HangLantern(70, 6.1f); HangLantern(108, 7); HangLantern(148, 5.3f);
        }

        void Hill()
        {

            firstJumpTarget = land[0].transform;
            VineWall(41, .1f, 6.2f);
            var ridge = land[3]; Enemy(false, 74, 72, 77, ridge);
            HillJourney();
            Landmark("YoungBanyan", 34, 0, 6.7f, new Color(.39f, .50f, .59f));
            Landmark("YoungBanyan", 76, 5.1f, 7.8f, new Color(.39f, .49f, .59f));
            Landmark("YoungBanyan", 120, 7f, 8f, new Color(.37f, .45f, .55f));
            Foliage(6, 0); Foliage(45, 6.3f); Foliage(99, 5.5f); Foliage(132, 4.4f);
            HangLantern(12, 4.1f); HangLantern(39, 4.4f); HangLantern(82, 9.5f);
        }

        public string AreaName(float x)
        {
            if (x >= ExtensionStart) return JourneyArea(x);
            switch (stage)
            {
                case 1: return x < 20 ? "Cổng làng" : x < 40 ? "Bờ ruộng" : x < 58 ? "Mương đầu làng" : x < 85 ? "Sân đình" : x < 112 ? "Ngõ tối" : x < 138 ? "Tường đất" : x < 162 ? "Cầu tre" : "Gốc đa làng";
                case 2: return x < 30 ? "Lối vào rừng" : x < 60 ? "Vòm trúc" : x < 96 ? "Lối trên ngọn tre" : x < 133 ? "Cầu gỗ mục" : "Miếu trong rừng";
                case 3: return x < 24 ? "Bến nước" : x < 45 ? "Bờ kè" : x < 77 ? "Khúc sông thả đèn" : x < 96 ? "Vách đá phủ rễ" : x < 129 ? "Miếu bến đò" : "Bờ bên kia";
                default: return x < 41 ? "Khe gió" : x < 70 ? "Sườn đồi rễ đa" : x < 106 ? "Sống đồi" : x < 134 ? "Bậc đá cổ" : "Bìa rừng đa";
            }
        }

        public string HintAt(float x)
        {

            string puzzle = CrossingHint(x);
            if (puzzle != null) return puzzle;
            if (stage == 2 && x > 146 && x < 156) return "Nghe quạ kêu thì chạy khỏi chỗ đang đứng.";
            if (x >= ExtensionStart) return JourneyHint(x);
            switch (stage)
            {
                case 1:
                    if (x < 9) return $"Dùng {game.Controls.Label(GameAction.Left)}, {game.Controls.Label(GameAction.Right)} để chạy. Theo lối tre ra khỏi làng.";
                    if (x > 17 && x < 24) return $"Giữ {game.Controls.Label(GameAction.Jump)} để nhảy cao.";
                    if (x > 37.6f && x < 39.8f && game.player.Grounded && game.player.Body.position.y > 1.5f)
                        return $"Bấm {game.Controls.Label(GameAction.Down)} để thả mình từ giàn tre xuống bờ ruộng.";
                    if (x > 41 && x < 49) return "Nhảy theo những bó tre để qua mương.";
                    if (x > 58 && x < 68) return $"Khi chó tới gần, bấm {game.Controls.Label(GameAction.Lantern)} để rọi đèn.";
                    if (x > 104 && x < 109) return "Coi chừng! Con mèo sắp chồm xuống từ mái nhà.";
                    if (x > 113 && x < 124) return "Đẩy thùng sát tường rồi nhảy lên thùng để vượt qua.";
                    if (x > 151 && x < 159) return "Chờ bè cập bến rồi nhảy lên.";
                    break;
                case 2:
                    if (x < 20) return $"Bấm {game.Controls.Label(GameAction.Jump)} lần nữa khi đang trên không để nhảy đôi.";
                    if (x > 104 && x < 113) return "Ván rung rồi! Nhảy sang tấm kế tiếp.";
                    break;
                case 3:
                    if (x > 54 && x < 55.6f && game.player.Grounded && game.player.Body.position.y > 4)
                        return $"Bấm {game.Controls.Label(GameAction.Down)} để xuống sàn gỗ sát mặt nước.";
                    if ((x > 14 && x < 25) || (x > 72 && x < 78)) return $"Áp sát vách, giữ phím hướng về vách và {game.Controls.Label(GameAction.Jump)} để leo.";
                    if (x > 102 && x < 114 && lanternSwitch && !lanternSwitch.Activated) return $"Đến bên miếu, bấm {game.Controls.Label(GameAction.Interact)} hoặc rọi đèn để mở cầu.";
                    break;
                case 4:
                    if (x < 18 || x > 80 && x < 86) return $"Nhảy đôi rồi bấm {game.Controls.Label(GameAction.Dash)} để lướt qua khe.";
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

        BoxCollider2D Ground(float start, float end, float top, float depth = 12)
        {
            var go = new GameObject($"Ground [{start}, {end}] top {top}"); go.layer = 8; go.transform.SetParent(terrain); go.transform.position = new Vector2((start + end) * .5f, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(end - start, depth); c.offset = new Vector2(0, -depth * .5f);
            go.AddComponent<PlatformSurface>();
            land.Add(c);
            return c;
        }

        GameObject Platform(float x, float top, float width, Vector2? travel = null, bool crumble = false, Transform parent = null)
        {
            var go = new GameObject(crumble ? "Weathered timber crossing" : travel.HasValue ? "Banyan suspended wooden lift" : "Rooted moss ledge");
            go.layer = 8; go.transform.SetParent(parent ? parent : terrain); go.transform.position = new Vector2(x, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(width, .55f); c.offset = new Vector2(0, -.275f);
            MakeOneWay(c);
            bool timber = crumble || travel.HasValue;
            go.AddComponent<PlatformSurface>().timber = timber;
            var deck = TerrainArt.DressLedge(game, stage, go, x, top, width, timber);
            if (travel.HasValue)
            {
                var m = go.AddComponent<MovingPlatform>(); m.travel = travel.Value; m.period = 4.8f; moving.Add(m);
                SupportLift(x, top, width, deck);
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
                var branch = Prop(14, new Vector2(x, y + perchHeight + .70f), .92f, -2);
                branch.transform.localScale = Vector3.one * (3.25f / branch.sprite.bounds.size.x);
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

        void Mote(Transform marker)
        {
            var go = new GameObject(marker.name); go.transform.SetParent(encounters); go.transform.position = marker.position;
            go.AddComponent<MoonTokenVisual>().Initialize();
            var hit = go.AddComponent<CircleCollider2D>(); hit.isTrigger = true; hit.radius = .55f;
            var p = go.AddComponent<ForestPickup>(); p.kind = PickupKind.Mote; pickups.Add(p);
        }

        void Moon(float x, float y)
        {
            var go = new GameObject("Moon fragment " + stage + " of 4"); go.transform.SetParent(encounters); go.transform.position = new Vector2(x, y);
            var s = Visuals.Sprite("Moon fragment", FourMoonPieces.Get(stage-1), new Vector2(x, y), go.transform, 16);
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
                var p = BambooPlatform(center, top, width + .04f, suspended: true);
                p.transform.SetParent(group.transform, true);
                Visuals.Halo("Awakened lantern", new Vector2(center, top - .28f), 2.8f, new Color(1, .64f, .24f, .55f), p.transform, 4);
            }
            group.SetActive(false);
            var shrine = new GameObject("River shrine"); shrine.transform.SetParent(encounters); shrine.transform.position = new Vector2(x, top);
            Prop(12, new Vector2(x, top + 2.8f), 2.8f, 2, shrine.transform);
            lanternSwitch = shrine.AddComponent<LanternSwitch>();
            lanternSwitch.Build(group, top + 1.8f);
        }

        void Crate(float x, float floor) => crate = CreateCrate(x,floor);
        PushableCrate CreateCrate(float x, float floor)
        {
            var go = new GameObject("Pushable wooden crate"); go.layer = 8; go.transform.SetParent(terrain);
            go.transform.position = new Vector2(x, floor + .675f);
            var box = go.AddComponent<PushableCrate>();
            var sr = Visuals.Sprite("Bamboo crate", game.art.forestExtras[4], new Vector2(x, floor + 1.34f), go.transform, 5);
            sr.transform.localScale = Vector3.one * (1.37f / sr.sprite.bounds.size.y);
            go.AddComponent<PlatformSurface>().timber = true;
            return box;
        }

        GameObject BambooPlatform(float x, float top, float width, Vector2? travel = null, bool suspended = false)
        {
            var go = new GameObject(travel.HasValue ? "Floating bamboo raft" : "Lashed bamboo platform"); go.layer = 8;
            go.transform.SetParent(terrain); go.transform.position = new Vector2(x, top);
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(width, .28f); c.offset = new Vector2(0, -.14f);
            MakeOneWay(c);
            go.AddComponent<PlatformSurface>().timber = true;
            // Preserve the braided lashings and round cane cross-sections in the painted raft sprite.
            var bundle = game.art.forestExtras[1];
            const float bodyHeight = .76f;
            Visuals.Band("Modular Bamboo Raft", bundle, new Vector2(x, top + .025f - bodyHeight * .5f),
                width, bodyHeight, 27, go.transform, 4);
            if (!travel.HasValue && !suspended) SupportDeck(x, top, width, false, go.transform);
            if (travel.HasValue)
            {
                var mover = go.AddComponent<MovingPlatform>(); mover.travel = travel.Value; mover.period = 4.65f; moving.Add(mover);
            }
            return go;
        }

        static void MakeOneWay(BoxCollider2D deck)
        {
            deck.usedByEffector = true;
            var effector = deck.gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 155;
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

        readonly List<Rect> waterways = new List<Rect>();

        void Water(float start, float end, float y)
        {
            if (!riverMaterial)
                riverMaterial = new Material(Resources.Load<Shader>("RiverSurface")) { name = "River Reflections" };
            waterways.Add(Rect.MinMaxRect(start, y - 8, end, y));
            // The water tints the submerged ends of pier piles. Its level also anchors fish and boats.
            const float depth = 8, crest = .12f;
            var water = Visuals.Box("River Surface", new Vector2((start + end) * .5f, y + (crest - depth) * .5f),
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
            foreach (var reach in waterways)
            {
                if (x < reach.xMin || x > reach.xMax) continue;
                y = reach.yMax;
                break;
            }
            if (!OpenWater(x, y, .35f)) return;
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

        public void ResetLevel(bool full)
        {
            foreach (var platform in falling) platform.ResetPlatform();
            foreach (var platform in moving) platform.ResetPlatform();
            foreach (var enemy in game.enemies) enemy.ResetCreature();
            foreach (var animal in Wildlife) animal.ResetCreature();
            if (crate && (full || game.RespawnPoint.x < 124)) crate.ResetCrate();
            if (Weight && (!Weight.Open || full)) Weight.ResetPuzzle();
            if (Ferry) Ferry.ResetLock();
            if (!full) return;
            foreach (var pickup in pickups) pickup.ResetPickup();
            if (lanternSwitch) lanternSwitch.ResetSwitch();
            if (Procession) Procession.ResetLights();
        }
    }
}
