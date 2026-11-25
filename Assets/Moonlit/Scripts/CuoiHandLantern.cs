using UnityEngine;

namespace Moonlit
{
    // Hand attachment points in the original 4x4 atlas (pixels from its top left).
    // Only the bowl below the original handle is masked; the hand remains animated.
    public sealed class CuoiHandLantern
    {
        static readonly Vector2[] Grip =
        {
            new Vector2(234,168), new Vector2(549,158), new Vector2(864,157), new Vector2(1174,168),
            new Vector2(243,464), new Vector2(553,462), new Vector2(871,450), new Vector2(1186,470),
            new Vector2(238,827), new Vector2(554,768), new Vector2(874,732), new Vector2(1180,822),
            new Vector2(236,1119), new Vector2(549,1083), new Vector2(873,1044), new Vector2(1188,1014)
        };
        readonly SpriteRenderer actor, paper;
        readonly Transform cutout, hook;
        float angle, angularSpeed;
        public CuoiHandLantern(SpriteRenderer actor)
        {
            this.actor = actor;
            var mask = new GameObject("Original lantern bowl mask").AddComponent<SpriteMask>();
            mask.sprite = Visuals.Square;
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = actor.sortingOrder + 1;
            mask.backSortingOrder = actor.sortingOrder - 1;
            cutout = mask.transform; cutout.SetParent(actor.transform, false);
            actor.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            hook = new GameObject("Bamboo handle").transform;
            hook.SetParent(actor.transform, false);
            var sprite = Resources.Load<Sprite>("StarLantern");
            paper = Visuals.Sprite("Đèn ông sao", sprite, Vector2.zero, hook, actor.sortingOrder + 2);
            const float height = .57f;
            paper.transform.localScale = Vector3.one * (height / sprite.bounds.size.y);
            paper.transform.localPosition = Vector3.down * (height * .47f);
        }

        public Vector2 Pose(int row, int frame, bool right, float pulse, float speed)
        {
            if (!cutout.gameObject.activeSelf)
            {
                cutout.gameObject.SetActive(true);
                actor.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }
            var s = actor.sprite;
            Vector2 grip = Grip[row * 4 + frame];
            // Sprite rects/pivots may be trimmed by the importer. Convert through
            // those rects rather than treating an animation cell as one world unit.
            grip = new Vector2(grip.x * s.texture.width, (1254 - grip.y) * s.texture.height) / 1254f;
            var anchor = (grip - s.rect.position - s.pivot) / s.pixelsPerUnit;
            float pixel = s.texture.width / (1254f * s.pixelsPerUnit);
            cutout.localScale = new Vector3(64 * pixel, 79 * pixel, 1);
            cutout.localPosition = new Vector2(right ? anchor.x : -anchor.x, anchor.y - 39.5f * pixel);
            return Hang(anchor, right, pulse, Mathf.Sin(Time.time * 9f) * Mathf.Clamp(speed, 0, 6f) * .75f, false);
        }

        public Vector2 MotionPose(Vector2 anchor, bool right, float pulse, float cycle, bool onWall, bool holding)
        {
            if (cutout.gameObject.activeSelf)
            {
                cutout.gameObject.SetActive(false);
                actor.maskInteraction = SpriteMaskInteraction.None;
            }
            float swing = holding ? 0 : Mathf.Sin(cycle * Mathf.PI * 4 - .75f) * (onWall ? 8 : 14);
            return Hang(anchor, right, pulse, swing, onWall);
        }

        Vector2 Hang(Vector2 anchor, bool right, float pulse, float targetAngle, bool onWall)
        {
            if (!right) { anchor.x = -anchor.x; targetAngle = -targetAngle; }
            hook.localPosition = anchor;
            angle = Mathf.SmoothDampAngle(angle, targetAngle, ref angularSpeed, .064f);
            hook.localRotation = Quaternion.Euler(0, 0, angle);
            paper.sortingOrder = actor.sortingOrder + (onWall ? -1 : 2);
            paper.color = Color.Lerp(new Color(1, .95f + .05f * Mathf.Sin(Time.time * 9), .94f, actor.color.a), Color.white, pulse);
            return paper.transform.position;
        }
    }
}
