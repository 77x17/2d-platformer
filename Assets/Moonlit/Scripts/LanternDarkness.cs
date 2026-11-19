using UnityEngine;

namespace Moonlit
{
    // A single world-space mask renders after the forest/effects and before the HUD.
    // World distances preserve a circular light at any window aspect ratio.
    [DefaultExecutionOrder(100)]
    public sealed class LanternDarkness : MonoBehaviour
    {
        public float CurrentRadius { get; private set; }
        public Material MaskMaterial => material;
        MoonlitGame game;
        Camera view;
        SpriteRenderer mask;
        Material material;
        static readonly int Center = Shader.PropertyToID("_LightCenter");
        static readonly int Radius = Shader.PropertyToID("_Radius");
        static readonly int Softness = Shader.PropertyToID("_Softness");

        public void Initialize(MoonlitGame owner)
        {
            game = owner; view = owner.cameraRig.GetComponent<Camera>();
            var shader = Resources.Load<Shader>("LanternDarkness");
            if (!shader) throw new System.InvalidOperationException("Missing LanternDarkness shader.");
            material = new Material(shader) { name = "Lantern darkness (runtime)" };
            mask = Visuals.Box("Darkness outside the lantern", Vector2.zero, Vector2.one, Color.white, transform, 900);
            mask.sharedMaterial = material;
        }

        void LateUpdate()
        {
            if (!game) return;
            mask.enabled = game.State != GameState.Home;
            var p = view.transform.position;
            mask.transform.position = new Vector3(p.x, p.y, 0);
            mask.transform.localScale = new Vector3(view.orthographicSize * view.aspect * 2 + 4, view.orthographicSize * 2 + 4, 1);
            float flicker = Mathf.Sin(Time.time * 2.8f) * .015f;
            CurrentRadius = Mathf.Lerp(game.tuning.lightRadius, game.tuning.pulseLightRadius, game.player.PulseFlash) + flicker;
            Vector2 light = game.player.LanternPosition;
            material.SetVector(Center, new Vector4(light.x, light.y, 0, 0));
            material.SetFloat(Radius, CurrentRadius);
            material.SetFloat(Softness, game.tuning.lightSoftEdge);
            material.SetFloat("_Darkness", 1 - Mathf.Clamp01(game.tuning.ambientVisibility));
            var tutorial = game.tutorials;
            material.SetFloat("_FocusBlend", tutorial ? tutorial.FocusAmount : 0);
            if (tutorial && tutorial.Active)
            {
                Vector2 at = tutorial.FocusPosition;
                material.SetVector("_FocusCenter", new Vector4(at.x, at.y, 0, 0));
                material.SetFloat("_FocusRadius", tutorial.FocusRadius);
            }
        }
        void OnDestroy() { if (material) Destroy(material); }
    }
}
