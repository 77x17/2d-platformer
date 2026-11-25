using UnityEngine;

namespace Moonlit
{
    public sealed class ForestCreature : MonoBehaviour
    {
        public bool isCat;
        public float left, right, floor, perchHeight;
        public SpriteRenderer body;
        public bool IsStunned => stunned > 0;
        public int StunCount { get; private set; }
        MoonlitGame game;
        Rigidbody2D rb;
        float direction = 1, recovery, stunned, leapTime = -1, leapDuration, leapStart, leapEnd, notice;
        float originX, attackLeft, attackRight, leapFromY;
        bool perched;
        readonly Transform[] stunStars = new Transform[3];
        static Sprite star;
        int animationRow, attackFrame;
        Transform warning;
        SpriteRenderer warningFace;
        float NoticeRange => isCat ? (perched ? 4.4f : 3f) : 5f;
        float NoticeDuration => isCat ? .45f : .5f;
        public bool IsWarning => notice > 0 && stunned <= 0;
        public bool WarningVisible => warning && warning.gameObject.activeSelf;
        public bool TutorialPreview { get; set; }

        public void Initialize(MoonlitGame owner, BoxCollider2D ground)
        {
            game = owner; originX = transform.position.x; perched = perchHeight > 0; direction = -1;
            float halfWidth = ground.size.x * .5f;
            attackLeft = ground.transform.position.x - halfWidth;
            attackRight = ground.transform.position.x + halfWidth;
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

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
            if (!star)
            {
                string[] rows = { "....#....", "....#....", "...###...", "#########", ".#######.", "..#####..", "..#####..", ".##...##.", ".#.....#." };
                var texture = new Texture2D(9, 9, TextureFormat.RGBA32, false) { name = "Lantern stun star", filterMode = FilterMode.Point };
                var pixels = new Color32[81];
                for (int row = 0; row < 9; row++)
                    for (int col = 0; col < 9; col++)
                        if (rows[row][col] == '#') pixels[(8 - row) * 9 + col] = new Color32(255, 231, 142, 255);
                texture.SetPixels32(pixels); texture.Apply();
                star = Sprite.Create(texture, new Rect(0, 0, 9, 9), Vector2.one * .5f, 9);
            }
            for (int i = 0; i < stunStars.Length; i++)
            {
                stunStars[i] = Visuals.Sprite("Stun star", star, transform.position, transform, 18, .22f).transform;
                stunStars[i].gameObject.SetActive(false);
            }
        }

        void FixedUpdate()
        {
            if (game.State != GameState.Playing)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float dt = Time.fixedDeltaTime;
            stunned = Mathf.Max(0, stunned - dt);
            Vector2 pos = rb.position;
            Vector2 target = game.player.Body.position;
            float dx = target.x - pos.x;
            int row = 0;
            if (stunned > 0) { row = 3; pos.y = Mathf.MoveTowards(pos.y, floor, dt * 8); }
            else if (leapTime >= 0)
            {
                row = 2; leapTime += dt;
                float p = Mathf.Clamp01(leapTime / leapDuration);
                attackFrame = Mathf.Min((int)(p * 4), 3);
                pos.x = Mathf.Lerp(leapStart, leapEnd, p);
                pos.y = Mathf.Lerp(leapFromY, floor, p) + Mathf.Sin(p * Mathf.PI) * (isCat ? 1.55f : .10f);
                if (p >= 1) { leapTime = -1; recovery = isCat ? 1.5f : .8f; pos.y = floor; }
            }
            else if (notice > 0)
            {
                notice -= dt; row = 2;
                if (notice <= 0)
                {
                    leapStart = pos.x; leapFromY = pos.y;
                    leapDuration = isCat ? .6f : Mathf.Max(.22f, Mathf.Abs(leapEnd - leapStart) / 8);
                    leapTime = 0; perched = false;
                }
            }
            else if (recovery > 0) recovery -= dt;
            else if (target.x >= attackLeft && target.x <= attackRight &&
                     Mathf.Abs(dx) <= NoticeRange && (perched || Mathf.Abs(target.y - floor) < 1.7f) &&
                     (isCat || dx * direction >= 0))
            {
                direction = dx < 0 ? -1 : 1;
                float aim = isCat ? target.x : pos.x + direction * 9.6f;
                leapEnd = Mathf.Clamp(aim, attackLeft + .5f, attackRight - .5f);
                notice = NoticeDuration; row = 2; attackFrame = 0;
                game.audioFx.Play(isCat ? "cat" : "dog");
            }
            else if (!perched)
            {
                row = 1;
                if (pos.x <= left) direction = 1;
                else if (pos.x >= right) direction = -1;
                pos.x = Mathf.MoveTowards(pos.x, direction < 0 ? left : right, dt * (isCat ? 1.5f : 2f));
            }
            animationRow = row;
            rb.MovePosition(pos);
        }

        void Update()
        {
            int frame = animationRow == 2 ? attackFrame : (int)(Time.time * (animationRow == 1 ? 11 : 5)) % 4;
            body.sprite = game.art.Creature(isCat, animationRow, frame); body.flipX = direction < 0;
            body.color = stunned > 0 ? new Color(.65f, .92f, 1) : Color.white;
            warning.gameObject.SetActive(TutorialPreview || IsWarning);
            for (int i = 0; i < stunStars.Length; i++)
            {
                var spark = stunStars[i];
                if (spark.gameObject.activeSelf != IsStunned) spark.gameObject.SetActive(IsStunned);
                if (!IsStunned) continue;
                float phase = Time.time * 5.5f + i * Mathf.PI * 2 / 3;
                spark.localPosition = new Vector2(Mathf.Cos(phase) * .50f, 1.24f + Mathf.Sin(phase) * .17f);
                spark.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(phase) * 18);
            }
            if (IsWarning || TutorialPreview)
            {
                float age = IsWarning ? NoticeDuration - notice : 0;
                float pop = 1 + Mathf.Sin(Mathf.Clamp01(age / .2f) * Mathf.PI) * .35f;
                warning.localScale = Vector3.one * pop;
                warning.localPosition = new Vector3(0, 2.1f + Mathf.Sin(age * 15) * .05f, 0);
                warningFace.color = Color.Lerp(new Color(.97f, .38f, .18f), new Color(1, .64f, .15f), (Mathf.Sin(age * 23) + 1) * .3f);
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!IsStunned && game.State == GameState.Playing && other.GetComponent<CuoiController>()) game.Hurt(false, Mathf.Sign(other.transform.position.x - transform.position.x));
        }
        public void Stun(float seconds)
        {
            perched = false; game.audioFx.Play("stun"); stunned = seconds; leapTime = -1; notice = 0; recovery = .35f; animationRow = 3; StunCount++;
            game.effects.Burst(transform.position + Vector3.up, Visuals.Gold, 12, 2);
        }
        public void ResetCreature()
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = new Vector2(originX, floor + perchHeight);
            transform.position = rb.position;
            stunned = notice = recovery = 0; leapTime = -1; direction = -1; animationRow = 0; perched = perchHeight > 0;
        }
    }
}
