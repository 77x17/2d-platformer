using UnityEngine;

namespace Moonlit
{
    public sealed class LanternRecoveryMeter : MonoBehaviour
    {
        const float Width = .92f, Height = .065f;
        MoonlitGame game;
        GameObject frame;
        Transform fill;

        public void Initialize(MoonlitGame owner)
        {
            game = owner;
            // Follow the actor's feet through jumps, drops and raft travel, independently of sprite poses.
            var anchor = new GameObject("Lantern recharge");
            anchor.transform.SetParent(transform, false);
            anchor.transform.localPosition = Vector2.down * .21f;
            var center = (Vector2)anchor.transform.position;
            Visuals.Box("Lantern meter rim", center, new Vector2(Width + .075f, Height + .065f),
                new Color(.51f,.42f,.28f), anchor.transform, 901);
            Visuals.Box("Lantern meter track", center, new Vector2(Width, Height),
                new Color(.055f,.10f,.12f), anchor.transform, 902);
            fill = Visuals.Box("Lantern charge", center, new Vector2(Width, Height), Visuals.Gold, anchor.transform, 903).transform;
            frame = anchor;
            frame.SetActive(false);
        }

        void LateUpdate()
        {
            var player = game.player;
            bool charging = game.State == GameState.Playing && player && player.Cooldown > 0;
            if (frame.activeSelf != charging) frame.SetActive(charging);
            if (!charging) return;
            float width = Width * Mathf.Clamp01(player.PulseReady);
            fill.localScale = new Vector3(width, Height, 1);
            fill.localPosition = new Vector2((width - Width) * .5f, 0);
        }
    }
}
