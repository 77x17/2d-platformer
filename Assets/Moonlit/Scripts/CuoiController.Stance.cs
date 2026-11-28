using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class CuoiController
    {
        const float StandingHeight = 1.55f, CrouchedHeight = 1.05f;
        CapsuleCollider2D capsule;
        readonly List<Collider2D> clearedPlatforms = new List<Collider2D>(2);
        readonly Collider2D[] headroom = new Collider2D[12];
        readonly ContactFilter2D groundFilter = new ContactFilter2D { useLayerMask = true, layerMask = 1 << 8, useTriggers = false };
        bool downHeld, downPressed;
        float crouchBlend, crouchBreath;
        public bool IsCrouching { get; private set; }
        public float ContactHeight => capsule.size.y;

        void SetCapsuleHeight(float height)
        {
            capsule.size = new Vector2(.7f, height);
            capsule.offset = Vector2.up * (.005f + height * .5f);
        }

        static bool IsOneWay(Collider2D floor) => floor.usedByEffector &&
            floor.TryGetComponent<PlatformEffector2D>(out var effector) && effector.isActiveAndEnabled && effector.useOneWay;

        bool CanStand()
        {
            var center = rb.position + Vector2.up * (.022f + StandingHeight * .5f);
            int count = Physics2D.OverlapCapsule(center, new Vector2(.69f, StandingHeight - .04f),
                CapsuleDirection2D.Vertical, 0, groundFilter, headroom);
            for (int i = 0; i < count; i++)
                if (headroom[i].bounds.max.y > rb.position.y + .07f && !IsOneWay(headroom[i])) return false;
            return true;
        }

        void UpdateStance(Collider2D floor, int hitCount)
        {
            bool pressed = downPressed;
            downPressed = false;
            bool canAct = Grounded && !IsDashing && knockbackTime <= 0;
            if (canAct && pressed && IsOneWay(floor))
            {
                // Seams between bridge planks can support one foot each. Only ignore
                // the contacting top plane, so a lower platform can still catch Cuội.
                float top = floor.bounds.max.y;
                for (int i = 0; i < hitCount; i++)
                {
                    var deck = hits[i].collider;
                    if (hits[i].normal.y < .65f || Mathf.Abs(top - deck.bounds.max.y) > .035f ||
                        !IsOneWay(deck) || clearedPlatforms.Contains(deck)) continue;
                    Physics2D.IgnoreCollision(capsule, deck, true);
                    clearedPlatforms.Add(deck);
                }
                float vertical = support ? support.Velocity.y : 0;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, vertical - 2.5f));
                // Keep horizontal momentum when stepping off a moving ferry.
                carriedX = 0;
                Grounded = canAct = false;
                support = null;
                coyote = buffer = mantleSettle = 0;
            }
            bool wantsLow = canAct && downHeld && buffer <= 0 && !IsOneWay(floor);
            if (wantsLow == IsCrouching || (!wantsLow && !CanStand())) return;
            IsCrouching = wantsLow;
            SetCapsuleHeight(wantsLow ? CrouchedHeight : StandingHeight);
        }

        void RestoreClearedPlatforms()
        {
            if (clearedPlatforms.Count == 0) return;
            var playerBounds = capsule.bounds;
            for (int i = clearedPlatforms.Count - 1; i >= 0; i--)
            {
                var deck = clearedPlatforms[i];
                if (deck && deck.enabled && deck.gameObject.activeInHierarchy)
                {
                    var bounds = deck.bounds;
                    bounds.Expand(.05f);
                    if (bounds.Intersects(playerBounds)) continue;
                    Physics2D.IgnoreCollision(capsule, deck, false);
                }
                clearedPlatforms.RemoveAt(i);
            }
        }

        void ResetStance()
        {
            foreach (var deck in clearedPlatforms)
                if (deck && deck.isActiveAndEnabled) Physics2D.IgnoreCollision(capsule, deck, false);
            clearedPlatforms.Clear();
            downHeld = downPressed = IsCrouching = false;
            crouchBlend = crouchBreath = 0;
            SetCapsuleHeight(StandingHeight);
        }
    }
}
