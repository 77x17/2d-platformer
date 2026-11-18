using UnityEngine;

namespace Moonlit
{
    public sealed class MoonlitCamera : MonoBehaviour
    {
        public CuoiController target;
        Vector3 smooth;
        float shakeTime, shakeStrength;
        Vector3 follow;
        Transform background;
        public void Initialize(Sprite forest)
        {
            follow = new Vector3(10, 3.6f, -10); transform.position = follow;
            background = Visuals.Sprite("Distant banyan forest - parallax", forest, Vector2.zero, null, -100).transform;
            var size = forest.bounds.size; background.localScale = Vector3.one * (31 / size.x);
            background.GetComponent<SpriteRenderer>().color = new Color(.68f, .79f, .88f);
        }
        void LateUpdate()
        {
            if (!target) return;
            float look = target.FacingRight ? 3.1f : -1.6f;
            var desired = new Vector3(Mathf.Clamp(target.transform.position.x + look, 10, MoonlitLevel.Length - 9), Mathf.Clamp(target.transform.position.y + 3.5f, 3.6f, 13), -10);
            follow = Vector3.SmoothDamp(follow, desired, ref smooth, .24f);
            shakeTime = Mathf.Max(0, shakeTime - Time.deltaTime);
            Vector2 shake = shakeTime > 0 ? Random.insideUnitCircle * shakeStrength * Mathf.Min(1, shakeTime * 10) : Vector2.zero;
            transform.position = follow + (Vector3)shake;
            // Broad plate slides gently, while placed vines/bamboo create the nearer depth layer.
            background.position = new Vector3(follow.x + Mathf.Sin(follow.x / 38) * 1.2f, follow.y + .7f + Mathf.Sin(follow.x / 65) * .5f, 20);
        }
        public void Snap() { smooth = Vector3.zero; follow = new Vector3(Mathf.Clamp(target.transform.position.x + 3.1f, 10, MoonlitLevel.Length - 9), Mathf.Clamp(target.transform.position.y + 3.5f, 3.6f, 13), -10); transform.position = follow; }
        public void Shake(float strength, float duration) { shakeStrength = strength; shakeTime = duration; }
    }
}
