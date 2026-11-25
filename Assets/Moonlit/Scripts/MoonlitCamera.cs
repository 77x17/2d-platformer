using UnityEngine;

namespace Moonlit
{
    public sealed class MoonlitCamera : MonoBehaviour
    {
        // A unit of camera travel moves terrain by one unit on screen.
        public static readonly float[] ScrollRates = { .04f, .33f, .63f, 1.16f };
        static readonly float[] PlaneWidths = { 39, 33, 29.25f, 22.9f };
        static readonly float[] VerticalRates = { .035f, .16f, .44f, .98f };
        // Bottoms are below the 13.5-unit view even in the lowest section of every route.
        static readonly float[] BaseAtRest = { -11.1f, -8.12f, -9.2f, -7.06f };
        static readonly int[] SortOrder = { -120, -86, -53, 24 };
        static readonly int[] MiddleBottom = { 487, 453, 469, 469 };
        static readonly int[] BankTop = { 512, 502, 497, 531 };
        static readonly int[] EdgeTop = { 0, 238, 474, 704 };
        static readonly int[] EdgeBottom = { 225, 454, 690, 931 };
        // The village courtyard comes into view at the first dog, the river landing at the shrine.
        static readonly float[,] ChapterPhase = {
            { 0, -36, 5, -7 }, { 4, -15, 7, 2 }, { -3, -28, 4, -8 }, { 4, -7, 3, -3 }
        };
        const int TileCount = 5;
        static readonly int BankTexture = Shader.PropertyToID("_MoonlitBankTexture");
        static readonly int BankRect = Shader.PropertyToID("_MoonlitBankRect");
        static readonly int BankPlane = Shader.PropertyToID("_MoonlitBankPlane");

        public CuoiController target;
        public float BackgroundOffset { get; private set; }
        readonly SpriteRenderer[,] tiles = new SpriteRenderer[4, TileCount];
        readonly Sprite[] planes = new Sprite[4];
        readonly float[] offsets = new float[4];
        public float LayerOffset(int layer) => offsets[layer];
        // Positive values mean the continuous foot of the illustrated bank is below the viewport.
        public float BankGroundClearance(int layer) =>
            transform.position.y - view.orthographicSize - tiles[layer, TileCount / 2].transform.position.y;

        Camera view;
        Sprite fallback;
        Transform scenery;
        Vector3 follow, smooth;
        float shakeTime, shakeStrength;
        int activeChapter;

        public void Initialize(Sprite forest)
        {
            fallback = forest; view = GetComponent<Camera>();
            follow = new Vector3(10, 3.6f, -10); transform.position = follow;
        }

        public void SetChapter(int chapter)
        {
            if (activeChapter == chapter) return;
            activeChapter = chapter;
            if (scenery) { scenery.gameObject.SetActive(false); Destroy(scenery.gameObject); }
            ReleaseSprites();
            scenery = new GameObject("Parallax • " + MoonlitGame.ChapterName(chapter)).transform;
            scenery.SetParent(transform, false);

            var horizon = Resources.Load<Texture2D>("Scenery" + chapter);
            if (!horizon && fallback) horizon = fallback.texture;
            var landscape = Resources.Load<Texture2D>("Embankment" + chapter);
            var edge = Resources.Load<Texture2D>("EdgeHabitat");
            int c = chapter - 1;
            // Y coordinates are measured down from the top of the 941-pixel art master.
            planes[0] = Slice(horizon, 0, 940, "Chân trời");
            planes[1] = Slice(landscape, 0, MiddleBottom[c], "Bờ xa và công trình trên nền");
            planes[2] = Slice(landscape, BankTop[c], 940, "Bờ gần");
            planes[3] = Slice(edge, EdgeTop[c], EdgeBottom[c], "Mép đất trước ống kính");
            if (planes[1])
            {
                // Water samples the same illustrated bank as the camera, including the alpha skyline.
                Rect rect = planes[1].rect;
                Shader.SetGlobalTexture(BankTexture, landscape);
                Shader.SetGlobalVector(BankRect, new Vector4(rect.x / landscape.width, rect.y / landscape.height,
                    rect.width / landscape.width, rect.height / landscape.height));
            }

            for (int band = 0; band < planes.Length; band++)
            {
                var sprite = planes[band];
                if (!sprite) continue;
                for (int i = 0; i < TileCount; i++)
                {
                    if (band == 0 && i != TileCount / 2) continue;
                    var piece = Visuals.Sprite(sprite.name, sprite, Vector2.zero, scenery, SortOrder[band]);
                    float width = PlaneWidths[band] + .034f;
                    float scale = width / sprite.bounds.size.x;
                    piece.transform.localScale = Vector3.one * scale;
                    piece.color = band == 0 ? new Color(.81f,.84f,.93f) :
                        band == 1 ? new Color(.97f,.97f,1f) :
                        band == 2 ? new Color(.90f,.93f,.97f) : new Color(.67f,.72f,.81f);
                    tiles[band,i] = piece;
                }
            }
            PositionScenery();
        }

        static Sprite Slice(Texture2D sheet, int top, int bottom, string label)
        {
            if (!sheet) { Debug.LogError("Missing landscape for " + label); return null; }
            int yTop = Mathf.RoundToInt(top * sheet.height / 941f);
            int yBottom = Mathf.RoundToInt((bottom + 1) * sheet.height / 941f);
            Rect region = new Rect(0, sheet.height - yBottom, sheet.width, yBottom - yTop);
            var sprite = Sprite.Create(sheet, region, new Vector2(.5f, 0), 100, 0, SpriteMeshType.FullRect);
            sprite.name = label;
            return sprite;
        }

        void ReleaseSprites()
        {
            for (int band = 0; band < planes.Length; band++)
                if (planes[band]) { Destroy(planes[band]); planes[band] = null; }
        }
        void OnDestroy() => ReleaseSprites();

        Vector3 Desired(float look)
        {
            float length = MoonlitGame.Instance.level.RouteLength;
            float halfWidth = view.orthographicSize * view.aspect;
            var boss = MoonlitGame.Instance.Boss;
            if (boss && boss.Active)
            {
                float playerX = target.transform.position.x;
                float margin = Mathf.Max(.5f, halfWidth - 2.2f);
                float focus = Mathf.Clamp((playerX + boss.BarkX) * .5f, playerX - margin, playerX + margin);
                return new Vector3(focus, Mathf.Max(boss.FloorY + 3.85f, target.transform.position.y + 1.45f), -10);
            }
            return new Vector3(Mathf.Clamp(target.transform.position.x + look, halfWidth - .9f, length - halfWidth + .9f),
                Mathf.Clamp(target.transform.position.y + 3.15f, 3.5f, 15), -10);
        }
        void LateUpdate()
        {
            if (!target || !scenery) return;
            var desired = Desired(target.FacingRight ? 3.1f : -1.6f);
            var tutorial = MoonlitGame.Instance.tutorials;
            bool focusing = tutorial && tutorial.Active;
            if (focusing)
            {
                shakeTime = 0;
                var focus = tutorial.FocusPosition;
                desired = Vector3.Lerp(desired, new Vector3(focus.x + 2.3f, focus.y + 1.25f, -10), Mathf.SmoothStep(0,1,tutorial.FocusAmount));
            }
            follow = Vector3.SmoothDamp(follow, desired, ref smooth, focusing ? .14f : .24f, Mathf.Infinity, focusing ? Time.unscaledDeltaTime : Time.deltaTime);
            view.orthographicSize = Mathf.Lerp(6.75f, 5.8f, focusing ? tutorial.FocusAmount : 0);
            shakeTime = Mathf.Max(0, shakeTime - Time.deltaTime);
            Vector2 shake = shakeTime > 0 ? Random.insideUnitCircle * shakeStrength * Mathf.Min(1, shakeTime * 10) : Vector2.zero;
            transform.position = follow + (Vector3)shake;
            PositionScenery();
        }
        void PositionScenery()
        {
            // One sunset, one skyline. The painting extends beyond both ends of the camera's route.
            var horizon = tiles[0, TileCount / 2];
            float routeLength = MoonlitGame.Instance.level.RouteLength;
            offsets[0] = Mathf.Clamp(follow.x, 0, routeLength) * ScrollRates[0];
            if (horizon)
            {
                float skyTravel = routeLength * ScrollRates[0];
                float width = Mathf.Max(PlaneWidths[0], 13.5f * view.aspect + skyTravel + 1.25f);
                float scale = width / planes[0].bounds.size.x;
                float addedHeight = (width - PlaneWidths[0]) * planes[0].bounds.size.y / planes[0].bounds.size.x;
                horizon.transform.localScale = new Vector3(scale, scale, 1);
                float y = follow.y + BaseAtRest[0] - addedHeight * .53f - (follow.y - 3.5f) * VerticalRates[0];
                horizon.transform.position = new Vector3(follow.x + skyTravel * .5f - offsets[0], y, 10);
            }
            for (int band = 1; band < ScrollRates.Length; band++)
            {
                offsets[band] = follow.x * ScrollRates[band];
                float distance = offsets[band] + ChapterPhase[activeChapter - 1,band];
                float width = PlaneWidths[band];
                int center = Mathf.FloorToInt(distance / width + .5f);
                float baseY = activeChapter == 2 && band == 2 ? -7.25f : BaseAtRest[band];
                float y = follow.y + baseY - (follow.y - 3.5f) * VerticalRates[band];
                for (int i = 0; i < TileCount; i++)
                {
                    int index = center + i - TileCount / 2;
                    var piece = tiles[band,i];
                    if (!piece) continue;
                    piece.transform.position = new Vector3(follow.x + index * width - distance, y, 10 - band);
                }
                if (band == 1 && planes[1])
                {
                    float height = planes[1].bounds.size.y * tiles[1, TileCount / 2].transform.lossyScale.y;
                    Shader.SetGlobalVector(BankPlane, new Vector4(follow.x + center * width - distance - width * .5f,
                        y, 1f / width, 1f / height));
                }
            }
            BackgroundOffset = offsets[0];
        }

        public void Snap()
        {
            smooth = Vector3.zero; shakeTime = 0; view.orthographicSize = 6.75f;
            follow = Desired(3.1f); transform.position = follow; PositionScenery();
        }
        public void Shake(float strength,float duration) { shakeStrength = strength; shakeTime = duration; }
    }
}
