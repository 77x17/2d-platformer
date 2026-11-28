using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    // Rendering metadata only. The collider remains at the level designer's walking height.
    public sealed class PlatformSurface : MonoBehaviour
    {
        public bool timber;
        public bool overhead;
        static readonly Dictionary<Sprite, Sprite> lips = new Dictionary<Sprite, Sprite>();

        public static void AddMossLip(SpriteRenderer surface)
        {
            var source = surface.sprite;
            if (!lips.TryGetValue(source, out var lip) || !lip)
            {
                Rect r = source.rect;
                float walk = r.y + source.pivot.y;
                float bottom = Mathf.Max(r.y, Mathf.Floor(walk - r.height * .12f));
                var strip = new Rect(r.x, bottom, r.width, r.yMax - bottom);
                lip = Sprite.Create(source.texture, strip,
                    new Vector2(source.pivot.x / r.width, (walk - bottom) / strip.height),
                    source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                lip.name = source.name + "_foreground_moss"; lips[source] = lip;
            }
            var front = Visuals.Sprite("Thin foreground moss - feet only", lip, surface.transform.position, surface.transform.parent, 11);
            front.transform.localScale = surface.transform.localScale;
            front.flipX = surface.flipX; front.color = surface.color;
        }
    }
}
