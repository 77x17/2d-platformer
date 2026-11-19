using UnityEngine;

namespace Moonlit
{
    public sealed class MoonlitCamera : MonoBehaviour
    {
        public CuoiController target;
        Vector3 smooth;
        float shakeTime, shakeStrength;
        Vector3 follow;
        readonly Transform[] backgrounds = new Transform[3];
        const float TileWidth = 31;
        public float BackgroundOffset { get; private set; }
        public void Initialize(Sprite forest)
        {
            follow = new Vector3(10, 3.6f, -10); transform.position = follow;
            for (int i = 0; i < backgrounds.Length; i++)
            {
                var s = Visuals.Sprite("Scrolling forest tile " + i, forest, Vector2.zero, transform, -100);
                s.transform.localScale = Vector3.one * (TileWidth / forest.bounds.size.x);
                s.color = new Color(.68f, .79f, .88f); backgrounds[i] = s.transform;
            }
        }
        void LateUpdate()
        {
            if (!target) return;
            float look = target.FacingRight ? 3.1f : -1.6f;
            var desired = new Vector3(Mathf.Clamp(target.transform.position.x + look, 10, MoonlitLevel.Length - 9), Mathf.Clamp(target.transform.position.y + 3.5f, 3.6f, 13), -10);
            var tutorial = MoonlitGame.Instance.tutorials;
            bool focusing = tutorial && tutorial.Active;
            if (focusing) shakeTime = 0;
            float dt = focusing ? Time.unscaledDeltaTime : Time.deltaTime;
            if (focusing)
            {
                Vector2 focus = tutorial.FocusPosition;
                Vector3 focused = new Vector3(focus.x + 3.3f, focus.y + 1.1f, -10);
                desired = Vector3.Lerp(desired, focused, Mathf.SmoothStep(0, 1, tutorial.FocusAmount));
            }
            follow = Vector3.SmoothDamp(follow, desired, ref smooth, focusing ? .14f : .24f, Mathf.Infinity, dt);
            GetComponent<Camera>().orthographicSize = Mathf.Lerp(6.75f, 5.8f, focusing ? tutorial.FocusAmount : 0);
            shakeTime = Mathf.Max(0, shakeTime - Time.deltaTime);
            Vector2 shake = shakeTime > 0 ? Random.insideUnitCircle * shakeStrength * Mathf.Min(1, shakeTime * 10) : Vector2.zero;
            transform.position = follow + (Vector3)shake;
            // Continuous horizontal parallax plus autonomous wind drift, including while idle.
            float drift = MoonlitGame.Instance.wind ? MoonlitGame.Instance.wind.Drift : 0;
            BackgroundOffset = follow.x * .22f - drift;
            int section = Mathf.FloorToInt(BackgroundOffset / TileWidth);
            for (int i = 0; i < backgrounds.Length; i++)
            {
                int tile = section + i - 1;
                backgrounds[i].position = new Vector3(follow.x + tile * TileWidth - BackgroundOffset, follow.y + .7f, 20);
                backgrounds[i].GetComponent<SpriteRenderer>().flipX = (tile & 1) != 0;
            }
        }
        public void Snap() { smooth = Vector3.zero; shakeTime = 0; GetComponent<Camera>().orthographicSize = 6.75f; follow = new Vector3(Mathf.Clamp(target.transform.position.x + 3.1f, 10, MoonlitLevel.Length - 9), Mathf.Clamp(target.transform.position.y + 3.5f, 3.6f, 13), -10); transform.position = follow; }
        public void Shake(float strength, float duration) { shakeStrength = strength; shakeTime = duration; }
    }
}
