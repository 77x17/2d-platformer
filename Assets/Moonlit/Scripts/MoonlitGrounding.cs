using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitLevel
    {
        static readonly Color VillageTint = new Color(.83f, .81f, .78f);
        static readonly Color BoatTint = new Color(.78f, .84f, .86f);
        static readonly Color TreeTint = new Color(.61f, .74f, .68f);
        static readonly Color Timber = new Color(.61f, .50f, .31f);
        static Sprite bambooGrain, climbingFibre;
        struct GroundContact
        {
            public SpriteRenderer art;
            public Vector2[] paintedFeet;
        }
        readonly List<GroundContact> contacts = new List<GroundContact>();

        public bool ValidatePlacements(out string detail)
        {
            Physics2D.SyncTransforms();
            int tested = 0;
            foreach (var contact in contacts)
            {
                foreach (var paint in contact.paintedFeet)
                {
                    var anchor = contact.art.transform.TransformPoint(paint);
                    var hit = Physics2D.Raycast((Vector2)anchor + Vector2.up * .38f, Vector2.down, .64f, 1 << 8);
                    if (!hit || hit.normal.y < .65f || anchor.y > hit.point.y + .04f)
                    {
                        detail = $"{contact.art.name} painted foot {anchor:F2} is clear of the soil";
                        return false;
                    }
                    tested++;
                }
            }
            var lifts = GetComponentsInChildren<LiftSuspension>(true);
            foreach (var lift in lifts)
                if (!lift.CheckAttachments(out detail)) return false;
            detail = $"{contacts.Count} planted props, {tested} painted contacts, {lifts.Length * 4} lift attachments";
            return tested > 0;
        }

        // Seat the full footprint on one continuous top. A marker may move slightly back
        // from a cliff edge, but a grove never grows out of empty air between colliders.
        bool FindFoot(float wantedX, float footprint, float maxShift, out Vector2 foot)
        {
            foot = Vector2.zero; float best = maxShift + .001f;
            foreach (var block in land)
            {
                float center = block.transform.position.x, half = block.size.x * .5f;
                float left = center - half + footprint * .5f, right = center + half - footprint * .5f;
                if (left > right) continue;
                float x = Mathf.Clamp(wantedX, left, right);
                float delta = Mathf.Abs(wantedX - x);
                if (delta >= best) continue;
                best = delta; foot = new Vector2(x, block.transform.position.y);
            }
            return best <= maxShift + .0001f;
        }

        // Coordinates in the source rectangle, independent of SpriteMeshType and the transparent margin.
        static Vector2 PaintedPoint(Sprite sprite, float x, float y)
        {
            var pixel = new Vector2(sprite.rect.width * x, sprite.rect.height * y);
            return (pixel - sprite.pivot) / sprite.pixelsPerUnit;
        }

        void MooredSampan(float x, float height, Color tint)
        {
            var sprite = Resources.Load<Sprite>("RiverSampan");
            float scale = height / sprite.bounds.size.y;
            float halfWidth = sprite.bounds.size.x * scale * .49f;
            foreach (var reach in waterways)
            {
                if (x - halfWidth < reach.xMin || x + halfWidth > reach.xMax || !OpenWater(x, reach.yMax, halfWidth)) continue;
                // The lower 12% of this painted hull sits below the surface. Let the water pass draw over it.
                float line = reach.yMax;
                float pivot = line - PaintedPoint(sprite, .5f, .12f).y * scale;
                var hull = Visuals.Sprite("Moored River Sampan", sprite, new Vector2(x, pivot), decoration, -8);
                hull.transform.localScale = Vector3.one * scale;
                hull.color = tint;
                return;
            }
            Debug.LogWarning($"No open berth for a sampan at x={x} in {game.LevelTitle}.");
        }

        void Landmark(string asset, float x, float floor, float height, Color tint)
        {
            var sprite = Resources.Load<Sprite>(asset);
            if (!sprite) return;
            float scale = height / sprite.bounds.size.y;
            float width = sprite.bounds.size.x * scale;
            Vector2 foot;
            if (!FindFoot(x, width * .93f, 2.85f, out foot)) return;
            bool tree = asset == "YoungBanyan", gate = asset == "VillageGate";
            // The outer roots end 5.7% above the image bottom; the cottage sill and each
            // separate gatepost also have transparent padding underneath their painted feet.
            float baseline = tree ? .061f : gate ? .042f : .049f;
            float inset = tree ? .10f : .05f;
            var pivot = foot - Vector2.up * (PaintedPoint(sprite, .5f, baseline).y * scale + inset);
            var s = Visuals.Sprite(asset, sprite, pivot, decoration, -8);
            s.transform.localScale = Vector3.one * scale; s.color = tint;
            var points = tree
                ? new[] { PaintedPoint(sprite,.12f,.055f), PaintedPoint(sprite,.5f,.048f), PaintedPoint(sprite,.88f,.057f) }
                : gate ? new[] { PaintedPoint(sprite,.12f,.036f), PaintedPoint(sprite,.88f,.040f) }
                : new[] { PaintedPoint(sprite,.20f,.049f), PaintedPoint(sprite,.80f,.035f) };
            contacts.Add(new GroundContact { art = s, paintedFeet = points });
            if (tree)
            {
                Visuals.AnimateBanyan(s);
                TerrainArt.AddRootContact(s.gameObject, foot, width * .36f);
            }
            if (gate || asset == "VillageCottage")
            {
                // Shallow masonry plinth and a two-step tread give the structure a contact shadow.
                float baseY = foot.y + .035f;
                var stone = game.art.forestExtras[2];
                const float stoneHeight = .38f;
                var plinth = Visuals.Band("Courtyard masonry plinth", stone, new Vector2(foot.x, baseY - .055f),
                    width, stoneHeight, 15, decoration, -7);
                plinth.color = new Color(.61f, .66f, .63f);
                Beam("Courtyard doorstep", new Vector2(foot.x - width * .21f, baseY + .035f), new Vector2(foot.x + width * .21f, baseY + .035f), .10f, new Color(.50f, .48f, .43f), decoration, -6);
            }
        }

        void Foliage(float x, float unusedHeight, bool grove = false)
        {
            if (FindFoot(x + 1.6f, .55f, 1f, out var fernFoot))
                Plant("Fern at soil line", 7, fernFoot, .78f, Color.white, 2, .9f, x);
            int stems = grove ? 3 : 1;
            for (int i = 0; i < stems; i++)
            {
                if (!FindFoot(x + i * 2.1f, 1.1f, .9f, out var root)) continue;
                float height = grove ? 5.8f + Mathf.Sin(x + i * 1.9f) * 1.7f : 4.7f;
                var tint = grove ? new Color(.36f + i * .035f, .61f - i * .045f, .54f) : new Color(.39f, .57f, .53f);
                Plant("Bamboo rooted in bank", 9, root, height, tint, -7 - i, 1.2f, x + i, (i & 1) == 1);
            }
        }

        void Plant(string label, int cell, Vector2 foot, float height, Color tint, int order, float amplitude, float phase, bool flip = false)
        {
            var root = new GameObject(label).transform; root.SetParent(decoration); root.position = foot - Vector2.up * .13f;
            var sprite = game.art.environment[cell];
            var foliage = Visuals.Sprite(label, sprite, foot, root, order);
            float scale = height / sprite.bounds.size.y;
            foliage.transform.localScale = Vector3.one * scale;
            foliage.transform.localPosition = new Vector3(0, -sprite.bounds.min.y * scale - .04f, 0);
            foliage.color = tint; foliage.flipX = flip;
            contacts.Add(new GroundContact { art = foliage, paintedFeet = new[] { PaintedPoint(sprite,.50f,cell == 9 ? .003f : .060f) } });
            var wind = root.gameObject.AddComponent<AmbientMotion>();
            wind.sway = amplitude; wind.speed = .8f; wind.phase = phase; wind.windAffected = true;
        }

        static Sprite BambooGrain()
        {
            if (bambooGrain) return bambooGrain;
            const int w = 72, h = 12;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = "Cut bamboo grain", filterMode = FilterMode.Point };
            var pixels = new Color32[w * h];
            var rng = new System.Random(4801);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if ((x == 0 || x == w - 1) && (y == 0 || y == h - 1)) continue;
                    int shade = (int)(48 * Mathf.Sin(Mathf.PI * (y + 1) / h)) + rng.Next(-9, 8);
                    int node = (x == 15 || x == 46) ? -37 : (x == 16 || x == 47) ? 24 : 0;
                    int streak = ((x / 7 + y * 3) % 9 == 0) ? 13 : 0;
                    int light = shade + node + streak;
                    pixels[y * w + x] = new Color32((byte)Mathf.Clamp(150 + light, 0, 255), (byte)Mathf.Clamp(132 + light, 0, 255), (byte)Mathf.Clamp(94 + light, 0, 255), 255);
                }
            texture.SetPixels32(pixels); texture.Apply();
            bambooGrain = Sprite.Create(texture, new Rect(0, 0, w, h), Vector2.one * .5f, 32, 0, SpriteMeshType.FullRect);
            return bambooGrain;
        }

        void DressClimbCord(float x, float bottom, float height)
        {
            // A continuous plait can be grasped between the leaves, including where
            // the visible cleft recedes behind the physical climbing line.
            if (!climbingFibre)
            {
                const int width = 20, rows = 48;
                var pixels = new Color32[width * rows];
                var bark = new Color32(61, 55, 36, 255);
                for (int y = 0; y < rows; y++)
                {
                    float cycle = y * 2 * Mathf.PI / rows;
                    float center = 9.5f + Mathf.Sin(cycle) * .7f;
                    float twist = Mathf.Sin(cycle * 3) * 2.1f;
                    float front = Mathf.Cos(cycle * 3) > 0 ? twist : -twist;
                    bool knot = y >= 21 && y <= 25;
                    for (int col = 0; col < width; col++)
                    {
                        float d = col - center;
                        if (Mathf.Min(Mathf.Abs(d - twist), Mathf.Abs(d + twist)) > (knot ? 4.3f : 3.1f)) continue;
                        var color = bark;
                        if (Mathf.Abs(d + front) < 2.8f) color = new Color32(105, 98, 62, 255);
                        if (Mathf.Abs(d - front) < 2.65f) color = new Color32(142, 130, 80, 255);
                        if (Mathf.Abs(d - front + .85f) < .72f) color = new Color32(184, 162, 100, 255);
                        if (knot && (y + col / 3) % 3 == 0) color = new Color32(88, 79, 48, 255);
                        pixels[y * width + col] = color;
                    }
                }
                var paper = new Texture2D(width, rows, TextureFormat.RGBA32, false) { name = "Braided wall fiber", filterMode = FilterMode.Point };
                paper.SetPixels32(pixels); paper.Apply();
                climbingFibre = Sprite.Create(paper, new Rect(0, 0, width, rows), Vector2.one * .5f, 55, 0, SpriteMeshType.FullRect);
            }
            var cord = Visuals.Sprite("Braided Wall Cord", climbingFibre, new Vector2(x, bottom + height * .5f), terrain, 7);
            cord.drawMode = SpriteDrawMode.Tiled;
            cord.size = new Vector2(climbingFibre.bounds.size.x, height + .055f);
            var crest = new Vector2(x, bottom + height);
            var root = new Color(.68f, .63f, .40f);
            Beam("Cliff edge grip roots", crest - Vector2.up * .07f, crest + new Vector2(.19f, .025f), .14f, root, terrain, 7);
            Beam("Cliff edge grip roots", crest + new Vector2(.19f, .025f), crest + new Vector2(.65f, -.065f), .105f, root, terrain, 7);
        }

        SpriteRenderer Beam(string name, Vector2 a, Vector2 b, float thickness, Color tint, Transform parent, int order)
        {
            Vector2 direction = b - a;
            var sr = Visuals.Band(name, BambooGrain(), (a + b) * .5f, direction.magnitude, thickness, 0, parent, order);
            sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            sr.color = tint;
            return sr;
        }

        void Fence(float from, float to)
        {
            if (!FindFoot((from + to) * .5f, to - from, .20f, out var foot)) return;
            var color = new Color(.59f, .56f, .43f);
            int bays = Mathf.Max(1, Mathf.RoundToInt((to - from) / 2f));
            float length = (to - from) / bays;
            for (int i = 0; i <= bays; i++)
            {
                float x = from + i * length;
                Beam("Fence stake", new Vector2(x, foot.y - .13f), new Vector2(x - .025f, foot.y + .88f), .13f, color, decoration, -4);
            }
            Beam("Fence rail", new Vector2(from - .08f, foot.y + .64f), new Vector2(to + .08f, foot.y + .64f), .12f, color, decoration, -3);
            Beam("Fence rail", new Vector2(from - .08f, foot.y + .30f), new Vector2(to + .08f, foot.y + .30f), .09f, color, decoration, -3);
        }

        void HangLantern(float x, float proposedHeight)
        {
            if (!FindFoot(x - 1.0f, .4f, 1f, out var foot)) return;
            float height = foot.y + Mathf.Clamp(proposedHeight - foot.y, 2.8f, 4.8f);
            var crown = new Vector2(foot.x, height + .2f);
            var hook = new Vector2(x, height + .2f);
            Beam("Lantern post", foot - Vector2.up * .20f, crown + Vector2.up * .15f, .16f, Timber, decoration, 0);
            Beam("Lantern bracket", crown - Vector2.right * .2f, hook + Vector2.right * .15f, .14f, Timber, decoration, 0);
            Beam("Bracket brace", crown - Vector2.up * .78f, hook - Vector2.right * .20f, .09f, Timber, decoration, 0);
            Beam("Lantern cord", hook, new Vector2(x, height - .08f), .038f, Color.gray, decoration, 0);
            var lamp = Prop(11, new Vector2(x, height - .04f), 1.14f, 1);
            var motion = lamp.gameObject.AddComponent<AmbientMotion>(); motion.sway = 4; motion.speed = 1.25f; motion.phase = x;
            var halo = Visuals.Halo("Paper lantern candle", new Vector2(x, height - .67f), 2.3f, new Color(1, .56f, .20f, .33f), decoration, 1, lightScenery: true);
            var glow = halo.gameObject.AddComponent<AmbientMotion>(); glow.sway = 0; glow.phase = x; glow.flicker = true;
        }

        void SupportDeck(float x, float top, float width, bool weathered, Transform owner = null)
        {
            var parent = owner ? owner : decoration;
            float left = x - width * .32f, right = x + width * .32f;
            float baseY = Mathf.Min(top - 5.5f, -8.5f);
            var tint = weathered ? new Color(.43f, .34f, .29f) : new Color(.46f, .47f, .33f);
            Beam("Bridge pile", new Vector2(left - .20f, baseY), new Vector2(left, top - .30f), .22f, tint, parent, -4);
            Beam("Bridge pile", new Vector2(right + .22f, baseY), new Vector2(right, top - .30f), .22f, tint, parent, -4);
            Beam("Deck crossbeam", new Vector2(left - .22f, top - .37f), new Vector2(right + .22f, top - .37f), .18f, Timber, parent, -3);
            Beam("Pier bracing", new Vector2(left - .04f, top - 2.25f), new Vector2(right, top - .46f), .13f, tint, parent, -3);
            if (!weathered) Beam("Pier bracing", new Vector2(right + .04f, top - 2.25f), new Vector2(left, top - .46f), .12f, tint, parent, -3);
        }

        void SupportLift(float x, float top, float width, SpriteRenderer deck, float clearance = 3.7f)
        {
            float left = x - width * .64f, right = x + width * .64f, height = top + clearance, baseY = Mathf.Min(-8.5f, top - 6);
            var tint = new Color(.49f, .46f, .36f);
            Beam("Hoist upright", new Vector2(left - .2f, baseY), new Vector2(left, height + .18f), .25f, tint, decoration, -6);
            Beam("Hoist upright", new Vector2(right + .2f, baseY), new Vector2(right, height + .18f), .25f, tint, decoration, -6);
            var lintel = Beam("Hoist lintel", new Vector2(left - .22f, height), new Vector2(right + .22f, height), .25f, Timber, decoration, -4);
            Beam("Hoist knee brace", new Vector2(left, height - 1.22f), new Vector2(left + 1.05f, height), .16f, tint, decoration, -5);
            Beam("Hoist knee brace", new Vector2(right, height - 1.22f), new Vector2(right - 1.05f, height), .16f, tint, decoration, -5);
            deck.transform.parent.gameObject.AddComponent<LiftSuspension>().Initialize(lintel, deck, width, top);
        }
        void Waterwheel(float x, float y, float radius)
        {
            var center = new Vector2(x, y); var wood = new Color(.60f, .47f, .31f);
            var baseY = y - radius - 2.7f;
            Beam("Waterwheel trestle", new Vector2(x - 1.6f, baseY), center, .27f, wood, decoration, -8);
            Beam("Waterwheel trestle", new Vector2(x + 1.6f, baseY), center, .27f, wood, decoration, -8);
            var wheel = new GameObject("Bamboo waterwheel").transform;
            wheel.SetParent(decoration); wheel.position = center;
            const int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments, b = (i + 1) * Mathf.PI * 2 / segments;
                Vector2 spoke = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Vector2 next = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                Beam("Rim joint", center + spoke * radius, center + next * radius, .16f, wood, wheel, -8);
                Beam("Second rim", center + spoke * radius * .83f, center + next * radius * .83f, .10f, wood, wheel, -8);
                if ((i & 1) == 0)
                {
                    Beam("Wheel spoke", center + spoke * .08f, center + spoke * radius, .13f, wood, wheel, -8);
                    var tangent = new Vector2(-spoke.y, spoke.x);
                    var bucket = center + spoke * radius * .94f;
                    Beam("Bamboo scoop", bucket - tangent * .28f - spoke * .12f, bucket + tangent * .30f + spoke * .025f, .24f, new Color(.72f, .54f, .32f), wheel, -7);
                }
            }
            var hub = Prop(6, center + Vector2.up * .3f, .52f, -6, wheel);
            hub.color = new Color(.60f, .55f, .50f);
            wheel.gameObject.AddComponent<WaterwheelMotion>();
        }
    }

    public sealed class WaterwheelMotion : MonoBehaviour
    {
        void Update() => transform.Rotate(0, 0, -9f * Time.deltaTime);
    }
}
