using UnityEngine;

namespace Moonlit
{
    [CreateAssetMenu(menuName = "Moonlit/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [Header("Movement (world units / seconds)")]
        public float runSpeed = 6f;
        public float groundAcceleration = 60f;
        public float groundDeceleration = 75f;
        [Tooltip("Visual feet sit inside moss; colliders keep their physical surface.")]
        public float feetInset = .065f;
        public float airAcceleration = 60f;
        public float jumpSpeed = 12f;
        public float gravity = 24f;
        public float fallMultiplier = 1.6f;
        public float maxFallSpeed = 18f;
        [Header("Forgiveness")]
        [Tooltip("Seconds after walking off a ledge during which a jump is still accepted.")]
        public float coyoteTime = .10f;
        public float jumpBuffer = .12f;
        [Header("Moon shard abilities")]
        public float airJumpSpeed = 11.5f;
        public float climbSpeed = 4f;
        public float dashSpeed = 16f;
        public float dashDuration = .22f;
        public float dashCooldown = .9f;
        [Header("Lantern")]
        public float pulseRadius = 5f;
        public float pulseCooldown = 4f;
        public float stunDuration = 2f;
        [Header("Lantern darkness shader (world units)")]
        public float lightRadius = 4.4f;
        public float lightSoftEdge = 1.4f;
        public float pulseLightRadius = 6.4f;
        [Range(0, 1), Tooltip("World brightness remaining outside the lantern. Tutorials use full darkness.")]
        public float ambientVisibility = .075f;
    }
}
