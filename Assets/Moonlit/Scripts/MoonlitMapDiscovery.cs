using UnityEngine;

namespace Moonlit
{
    // Lives on a chapter's runtime world. Returning to a checkpoint keeps the same survey.
    public sealed class MoonlitMapDiscovery : MonoBehaviour
    {
        public const int Bottom = -5, Top = 22;
        const float HorizontalSight = 15f, VerticalSight = 6.5f;
        const byte KnownAlpha = 76; // Up to 30% remaining fog is clear enough to identify a landmark.
        const int Rows = Top - Bottom + 1;
        Color32[] fog;
        bool[] knownColumns;
        int columns;
        public Texture2D Mask { get; private set; }

        public void Begin(float length)
        {
            columns = Mathf.CeilToInt(length) + 1;
            fog = new Color32[columns * Rows];
            knownColumns = new bool[columns];
            for (int i = 0; i < fog.Length; i++) fog[i] = new Color32(255, 255, 255, 255);
            Mask = new Texture2D(columns, Rows, TextureFormat.RGBA32, false, true)
            {
                name = "Chapter exploration mask", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp
            };
            Upload();
        }

        public void Visit(Vector2 feet)
        {
            Vector2 center = feet + Vector2.up;
            int xmin = Mathf.Max(0, Mathf.FloorToInt(center.x - HorizontalSight));
            int xmax = Mathf.Min(columns - 1, Mathf.CeilToInt(center.x + HorizontalSight));
            int ymin = Mathf.Max(Bottom, Mathf.FloorToInt(center.y - VerticalSight));
            int ymax = Mathf.Min(Top, Mathf.CeilToInt(center.y + VerticalSight));
            bool changed = false;
            for (int y = ymin; y <= ymax; y++)
            {
                float dy = (y - center.y) / VerticalSight;
                int row = (y - Bottom) * columns;
                for (int x = xmin; x <= xmax; x++)
                {
                    if (fog[row + x].a == 0) continue;
                    float dx = (x - center.x) / HorizontalSight;
                    float radius2 = dx * dx + dy * dy;
                    if (radius2 >= 1) continue;
                    // Clear inside the lantern's reach, then feather out across the outer third.
                    float edge = Mathf.InverseLerp(.36f, 1f, radius2);
                    byte alpha = (byte)Mathf.RoundToInt(255 * Mathf.SmoothStep(0, 1, edge));
                    if (alpha >= fog[row + x].a) continue;
                    fog[row + x].a = alpha;
                    if (alpha <= KnownAlpha) knownColumns[x] = true;
                    changed = true;
                }
            }
            if (changed) Upload();
        }

        void Upload()
        {
            Mask.SetPixels32(fog);
            Mask.Apply(false, false);
        }

        public bool CanIdentify(Vector2 at)
        {
            int x = Mathf.RoundToInt(at.x), row = Mathf.RoundToInt(at.y) - Bottom;
            return x >= 0 && x < columns && row >= 0 && row < Rows && fog[row * columns + x].a <= KnownAlpha;
        }

        public bool KnownSpan(float west, float east, out int first, out int last)
        {
            int end = Mathf.Min(columns - 1, Mathf.FloorToInt(east));
            first = Mathf.Max(0, Mathf.CeilToInt(west));
            while (first <= end && !knownColumns[first]) first++;
            last = end;
            while (last > first && !knownColumns[last]) last--;
            return first <= end;
        }

        public Rect Uv(float west, float east) =>
            new Rect((west + .5f) / columns, .5f / Rows, (east - west) / columns, (Top - Bottom) / (float)Rows);

        void OnDestroy()
        {
            if (Mask) Destroy(Mask);
        }
    }
}
