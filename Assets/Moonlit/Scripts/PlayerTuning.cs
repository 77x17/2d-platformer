using UnityEngine;

namespace Moonlit
{
    [CreateAssetMenu(menuName = "Moonlit/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [Header("Movement (world units / seconds)")]
        public float runSpeed = 6.4f;
        public float groundAcceleration = 65f;
        public float groundDeceleration = 110f;
        [Tooltip("Visual feet sit inside moss; colliders keep their physical surface.")]
        public float feetInset = .065f;
        public float airAcceleration = 38f;
        public float jumpSpeed = 13.5f;
        public float gravity = 36f;
        public float fallMultiplier = 1.25f;
        public float maxFallSpeed = 23f;
        [Header("Forgiveness")]
        [Tooltip("Seconds after walking off a ledge during which a jump is still accepted.")]
        public float coyoteTime = .10f;
        public float jumpBuffer = .14f;
        [Header("Lantern")]
        public float pulseRadius = 4.3f;
        public float pulseCooldown = 3f;
        public float stunDuration = 2.6f;
        [Header("Lantern darkness shader (world units)")]
        public float lightRadius = 6.4f;
        public float lightSoftEdge = 2.3f;
        public float pulseLightRadius = 9f;
        [Range(0, 1), Tooltip("World brightness remaining outside the lantern. Tutorials use full darkness.")]
        public float ambientVisibility = .30f;
    }
}
