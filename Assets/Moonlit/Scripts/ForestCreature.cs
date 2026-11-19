using UnityEngine;

namespace Moonlit
{
    public sealed class ForestCreature : MonoBehaviour
    {
        public bool isCat;
        public float left, right, floor;
        public SpriteRenderer body;
        public bool IsStunned => stunned > 0;
        public int StunCount { get; private set; }
        MoonlitGame game;
        float direction = 1, timer, stunned, leapTime = -1, leapStart, leapEnd, originX, notice;
        Transform warning;
        SpriteRenderer warningFace;
        public bool IsWarning => notice > 0 && stunned <= 0;
        public bool WarningVisible => warning && warning.gameObject.activeSelf;
        public bool TutorialPreview { get; set; }

        public void Initialize(MoonlitGame owner)
        {
            game = owner; originX = transform.position.x;
            warning = new GameObject("Attack telegraph - exclamation mark").transform;
            warning.SetParent(transform, false); warning.localPosition = new Vector3(0, 2.1f, 0);
            var at = (Vector2)warning.position;
            var border = Visuals.Box("Dark diamond outline", at, new Vector2(.76f, .76f), new Color(.035f, .025f, .02f), warning, 60);
            border.transform.localRotation = Quaternion.Euler(0, 0, 45);
            warningFace = Visuals.Box("Amber warning diamond", at, new Vector2(.64f, .64f), Visuals.Gold, warning, 61);
            warningFace.transform.localRotation = Quaternion.Euler(0, 0, 45);
            Visuals.Box("Exclamation stem outline", at + Vector2.up * .12f, new Vector2(.23f, .45f), Color.black, warning, 62);
            Visuals.Box("Exclamation stem", at + Vector2.up * .13f, new Vector2(.13f, .33f), Color.white, warning, 63);
            Visuals.Box("Exclamation dot outline", at - Vector2.up * .23f, new Vector2(.23f, .22f), Color.black, warning, 62);
            Visuals.Box("Exclamation dot", at - Vector2.up * .23f, new Vector2(.13f, .12f), Color.white, warning, 63);
            warning.gameObject.SetActive(false);
        }

        void Update()
        {
            float dt = game.State == GameState.Playing ? Time.deltaTime : 0;
            timer += dt; stunned = Mathf.Max(0, stunned - dt);
            Vector3 pos = transform.position;
            int row = 0;
            if (stunned > 0) { row = 3; pos.y = Mathf.MoveTowards(pos.y, floor, dt * 8); }
            else if (leapTime >= 0)
            {
                row = 2; leapTime += dt;
                float p = leapTime / (isCat ? .72f : .64f);
                pos.x = Mathf.Lerp(leapStart, leapEnd, p);
                pos.y = floor + Mathf.Sin(Mathf.Clamp01(p) * Mathf.PI) * (isCat ? 2.1f : .45f);
                if (p >= 1) { leapTime = -1; timer = -1.1f; pos.y = floor; }
            }
            else if (notice > 0)
            {
                notice -= dt; row = 2;
                if (notice <= 0)
                {
                    leapStart = pos.x; leapEnd = Mathf.Clamp(pos.x + direction * (isCat ? 4.4f : 3.1f), left, right); leapTime = 0;
                }
            }
            else
            {
                float dx = game.player.transform.position.x - pos.x;
                bool near = Mathf.Abs(dx) < (isCat ? 5.3f : 4.6f) && Mathf.Abs(game.player.transform.position.y - floor) < 2.5f;
                if (near && timer > 1.3f && dt > 0) { direction = Mathf.Sign(dx); notice = isCat ? .9f : .75f; timer = 0; }
                else if (timer > 0)
                {
                    row = 1; pos.x += direction * dt * (isCat ? 1.15f : 1.65f);
                    if (pos.x > right) { pos.x = right; direction = -1; timer = -.55f; }
                    if (pos.x < left) { pos.x = left; direction = 1; timer = -.55f; }
                }
            }
            transform.position = pos;
            body.sprite = game.art.Creature(isCat, row, (int)(Time.time * (row == 1 ? 9 : 5)) % 4); body.flipX = direction < 0;
            body.color = stunned > 0 ? new Color(.65f, .92f, 1) : Color.white;
            warning.gameObject.SetActive(TutorialPreview || notice > 0 && stunned <= 0);
            if (notice > 0)
            {
                float age = (isCat ? .9f : .75f) - notice;
                float pop = 1 + Mathf.Sin(Mathf.Clamp01(age / .2f) * Mathf.PI) * .35f;
                warning.localScale = Vector3.one * pop;
                warning.localPosition = new Vector3(0, 2.1f + Mathf.Sin(age * 15) * .05f, 0);
                warningFace.color = Color.Lerp(Visuals.Gold, new Color(1, .4f, .12f), (Mathf.Sin(age * 23) + 1) * .3f);
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!IsStunned && game.State == GameState.Playing && other.GetComponent<CuoiController>()) game.Hurt();
        }
        public void Stun(float seconds)
        {
            stunned = seconds; leapTime = -1; notice = 0; timer = -1; StunCount++;
            game.effects.Burst(transform.position + Vector3.up, Visuals.Gold, 12, 2);
        }
        public void ResetCreature()
        {
            transform.position = new Vector3(originX, floor); stunned = notice = timer = 0; leapTime = -1; direction = 1;
        }
    }
}
