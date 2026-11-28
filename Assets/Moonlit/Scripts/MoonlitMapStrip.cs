using UnityEngine;
using UnityEngine.UI;

namespace Moonlit
{
    // One horizontal section of the chapter, shared by the pocket map and the atlas.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MoonlitMapStrip : MaskableGraphic
    {
        public static readonly Color32 PlayerInk = new Color32(255, 198, 89, 255);
        public static readonly Color32 LampInk = new Color32(145, 218, 202, 255);
        public static readonly Color32 MoteInk = new Color32(212, 176, 252, 255);
        MoonlitGame game;
        BoxCollider2D[] terrain;
        MoonlitMapDiscovery survey;
        float west, east;
        Image marker;
        RawImage shroud;
        static readonly Color32 Grid = new Color32(43, 69, 73, 255);
        static readonly Color32 Bank = new Color32(47, 87, 84, 255);
        static readonly Color32 Grass = new Color32(117, 166, 144, 255);
        static readonly Color32 Deck = new Color32(217, 175, 116, 255);

        public MoonlitMapStrip()
        {
            useLegacyMeshGeneration = false;
        }

        public void Show(MoonlitGame owner, BoxCollider2D[] surfaces, MoonlitMapDiscovery discovery,
            float from, float to, bool nearby)
        {
            game = owner;
            terrain = surfaces;
            survey = discovery;
            west = from;
            east = to;
            if (!shroud)
            {
                shroud = new GameObject("Unexplored country", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
                var rect = shroud.rectTransform;
                rect.SetParent(rectTransform, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                shroud.color = new Color32(12, 24, 33, 255);
                shroud.raycastTarget = false;
            }
            shroud.texture = survey.Mask;
            shroud.uvRect = survey.Uv(west, east);
            if (!marker)
            {
                marker = new GameObject("Cuoi's map star", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                marker.rectTransform.SetParent(rectTransform, false);
                marker.rectTransform.anchorMin = marker.rectTransform.anchorMax = Vector2.zero;
                marker.rectTransform.pivot = new Vector2(.5f, .5f);
                marker.sprite = Resources.Load<Sprite>("EmblemStar");
                marker.preserveAspect = true;
                marker.raycastTarget = false;
                var outline = marker.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(.07f,.11f,.13f,.95f);
                outline.effectDistance = nearby ? Vector2.one : new Vector2(1.5f, 1.5f);
            }
            Vector2 feet = game.player.Body.position;
            bool inView = feet.x >= west && (feet.x < east || east >= game.level.RouteLength);
            marker.gameObject.SetActive(inView);
            if (inView)
            {
                float diameter = nearby ? 14 : 22, inset = diameter * .58f;
                var size = rectTransform.rect.size;
                marker.rectTransform.sizeDelta = Vector2.one * diameter;
                marker.rectTransform.anchoredPosition = new Vector2(
                    Mathf.Clamp((feet.x - west) * size.x / (east - west), inset, size.x - inset),
                    Mathf.Clamp((feet.y - Bottom) * size.y / (Top - Bottom) + inset, inset, size.y - inset));
                Update();
            }
            SetVerticesDirty();
        }

        const float Bottom = MoonlitMapDiscovery.Bottom, Top = MoonlitMapDiscovery.Top;

        void Update()
        {
            if (!marker || !marker.gameObject.activeSelf) return;
            float glow = .5f + .5f * Mathf.Cos(Time.unscaledTime * (2 * Mathf.PI / 1.35f));
            marker.color = Color.Lerp(new Color32(255, 179, 68, 255), new Color32(255, 217, 115, 255), glow);
            marker.rectTransform.localScale = Vector3.one * (.86f + .25f * glow);
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (!game || !game.player || !survey || terrain == null || east <= west) return;
            Rect r = rectTransform.rect;
            float sx = r.width / (east - west);
            // Exaggerate relief slightly on the local chart so stacked platforms remain distinct.
            float sy = r.height / (Top - Bottom);
            float X(float world) => r.xMin + (world - west) * sx;
            float Y(float world) => r.yMin + (world - Bottom) * sy;
            void Box(float left, float low, float right, float high, Color32 color)
            {
                left = Mathf.Max(left, r.xMin); right = Mathf.Min(right, r.xMax);
                low = Mathf.Max(low, r.yMin); high = Mathf.Min(high, r.yMax);
                if (left >= right || low >= high) return;
                Quad(mesh, new Vector2(left, low), new Vector2(left, high),
                    new Vector2(right, high), new Vector2(right, low), color);
            }

            for (int tick = Mathf.CeilToInt(west / 10) * 10; tick < east; tick += 10)
                Box(X(tick), r.yMin, X(tick) + 1, r.yMax, Grid);
            for (int height = 0; height < Top; height += 5)
                Box(r.xMin, Y(height), r.xMax, Y(height) + 1, Grid);

            foreach (var surface in terrain)
            {
                if (!surface || !surface.isActiveAndEnabled) continue;
                Bounds b = surface.bounds;
                if (b.max.x < west || b.min.x > east) continue;
                float ledge = Y(b.max.y);
                if (b.size.y >= 3)
                {
                    Box(X(b.min.x), Y(b.min.y), X(b.max.x), ledge, Bank);
                    Box(X(b.min.x), ledge - 2, X(b.max.x), ledge, Grass);
                }
                else Box(X(b.min.x), ledge - 2.5f, X(b.max.x), ledge, Deck);
            }

            foreach (var lamp in game.ChapterSource.checkpoints)
            {
                Vector3 p = lamp.position;
                if (p.x < west || p.x >= east || !survey.CanIdentify(p)) continue;
                float x = X(p.x), y = Y(p.y);
                Box(x - 1, y, x + 1, y + 8, LampInk);
                Box(x - 3, y + 6, x + 3, y + 11, LampInk);
            }

            foreach (var pickup in game.level.pickups)
            {
                if (pickup.kind != PickupKind.Mote || pickup.Collected) continue;
                Vector2 p = pickup.transform.position;
                if (p.x < west || p.x >= east || !survey.CanIdentify(p)) continue;
                float radius = Mathf.Min(4.5f, r.height * .06f);
                Vector2 center = new Vector2(Mathf.Clamp(X(p.x), r.xMin + radius, r.xMax - radius),
                    Mathf.Clamp(Y(p.y), r.yMin + radius, r.yMax - radius));
                Quad(mesh, center + Vector2.left * radius, center + Vector2.up * radius,
                    center + Vector2.right * radius, center + Vector2.down * radius, MoteInk);
            }
        }

        static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 color)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(a, color, Vector2.zero); mesh.AddVert(b, color, Vector2.zero);
            mesh.AddVert(c, color, Vector2.zero); mesh.AddVert(d, color, Vector2.zero);
            mesh.AddTriangle(first, first + 1, first + 2);
            mesh.AddTriangle(first, first + 2, first + 3);
        }
    }
}
