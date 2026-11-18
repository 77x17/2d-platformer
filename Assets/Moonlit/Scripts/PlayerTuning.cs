using UnityEngine;

namespace Moonlit
{
    [CreateAssetMenu(menuName = "Moonlit/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [Header("Movement (world units / seconds)")]
        public float runSpeed = 6.4f;
        public float groundAcceleration = 65f;
        public float airAcceleration = 38f;
        public float jumpSpeed = 13.5f;
        public float gravity = 36f;
        public float fallMultiplier = 1.25f;
        public float maxFallSpeed = 23f;
        [Header("Forgiveness")]
        public float coyoteTime = .13f;
        public float jumpBuffer = .14f;
        [Header("Lantern")]
        public float pulseRadius = 4.3f;
        public float pulseCooldown = 3f;
        public float stunDuration = 2.6f;
    }
}
