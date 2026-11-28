using UnityEngine;

namespace Moonlit
{
    public sealed class CuoiFootShadow
    {
        readonly SpriteRenderer cast, contact;
        readonly RaycastHit2D[] hits = new RaycastHit2D[6];
        readonly ContactFilter2D terrain = new ContactFilter2D { useLayerMask = true, layerMask = 1 << 8, useTriggers = false };

        public CuoiFootShadow(Transform actor)
        {
            cast = Visuals.Halo("Lantern Cast Shadow", actor.position, 1, Color.clear, actor, 7);
            contact = Visuals.Halo("Foot Contact Shadow", actor.position, 1, Color.clear, actor, 8);
        }

        public void Update(Vector2 feet, bool right, float contactX)
        {
            const float reach = 5.6f;
            int count = Physics2D.Raycast(feet + Vector2.up * .24f, Vector2.down, terrain, hits, reach + .24f);
            Collider2D surface = null;
            float floor = float.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.normal.y < .65f || hit.collider.bounds.max.y > feet.y + .15f || hit.point.y <= floor) continue;
                surface = hit.collider; floor = hit.point.y;
            }
            cast.enabled = contact.enabled = surface;
            if (!surface) return;
            float height = Mathf.Max(0, feet.y - floor);
            float p = Mathf.SmoothStep(0, 1, height / reach);
            float away = right ? -1 : 1;
            float shift = away * Mathf.Lerp(.15f, .28f, p);
            Stamp(cast, surface.bounds, feet.x + shift, floor + .025f, Mathf.Lerp(1.65f, .62f, p),
                Mathf.Lerp(.22f,.12f,p), new Color(.055f,.048f,.058f, (1 - p) * .85f));
            Stamp(contact, surface.bounds, contactX, floor + .033f, Mathf.Lerp(.80f,.46f,p), .13f,
                new Color(.035f,.031f,.035f, .90f * Mathf.Clamp01(1 - height / 1.9f)));
        }

        static void Stamp(SpriteRenderer shadow, Bounds support, float x, float y, float width, float thickness, Color color)
        {
            // Taper at platform edges rather than painting an ellipse over the void.
            float left = Mathf.Max(support.min.x, x - width * .5f);
            float right = Mathf.Min(support.max.x, x + width * .5f);
            shadow.transform.position = new Vector3((left + right) * .5f, y, 0);
            shadow.transform.localScale = new Vector3(Mathf.Max(0, right - left), thickness, 1);
            shadow.color = color;
        }
    }
}
