using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonlit
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class CuoiController : MonoBehaviour
    {
        public PlayerTuning tuning;
        public SpriteRenderer body;
        public SpriteRenderer lanternGlow;
        public bool Grounded { get; private set; }
        public bool FacingRight { get; private set; } = true;
        public float Cooldown { get; private set; }
        public float Invulnerable { get; set; }
        public int Jumps { get; private set; }
        public int Pulses { get; private set; }
        public Rigidbody2D Body => rb;
        public float TestMove { get; set; }
        public bool TestInput { get; set; }
        public float PulseReady => 1 - Cooldown / tuning.pulseCooldown;
        public int AnimationRow { get; private set; }
        public float PulseFlash => Mathf.Clamp01(pulsePose / .45f);
        public Vector2 LanternPosition => lanternGlow.transform.position;
        public float InputSuppressedUntil { get; private set; }

        MoonlitGame game;
        Rigidbody2D rb;
        float move, coyote, buffer, pulsePose, squash, stepTimer, airTime, detachTimer, stride;
        bool jumpHeld, wasGrounded;
        SpriteRenderer contactShadow;
        MovingPlatform support;
        bool standingOnTimber;
        readonly RaycastHit2D[] hits = new RaycastHit2D[12];

        public void Initialize(MoonlitGame owner)
        {
            game = owner; tuning = game.tuning; rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0; rb.freezeRotation = true; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = GetComponent<CapsuleCollider2D>(); collider.size = new Vector2(.65f, 1.45f); collider.offset = new Vector2(0, .73f);
            collider.sharedMaterial = new PhysicsMaterial2D("Cuoi frictionless") { friction = 0, bounciness = 0 };
            contactShadow = Visuals.Halo("Feet contact shadow", transform.position, 1, new Color(.01f, .025f, .02f, .65f), transform, 6);
            contactShadow.transform.localPosition = new Vector3(0, -.06f, 0);
            contactShadow.transform.localScale = new Vector3(.62f, .13f, 1);
        }

        void Update()
        {
            var kb = Keyboard.current;
            bool playing = game.State == GameState.Playing;
            move = 0;
            if (playing && Time.unscaledTime >= InputSuppressedUntil)
            {
                if (TestInput) move = TestMove;
                else if (kb != null)
                {
                    move = ((kb.dKey.isPressed || kb.rightArrowKey.isPressed) ? 1 : 0) - ((kb.aKey.isPressed || kb.leftArrowKey.isPressed) ? 1 : 0);
                    jumpHeld = kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed;
                    if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) QueueJump();
                    if (kb.fKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame) Pulse();
                    if (kb.eKey.wasPressedThisFrame) game.Interact();
                    if (kb.rKey.wasPressedThisFrame) game.Hurt(true);
                }
                Cooldown = Mathf.Max(0, Cooldown - Time.deltaTime);
                Invulnerable = Mathf.Max(0, Invulnerable - Time.deltaTime);
            }
            if (Mathf.Abs(move) > .01f) FacingRight = move > 0;
            pulsePose = Mathf.Max(0, pulsePose - Time.deltaTime);
            squash = Mathf.MoveTowards(squash, 0, Time.deltaTime * 4);
            // AI idle frames do not register consistently. Keep one planted pose;
            // breathing changes height very slightly around its fixed foot pivot.
            int row = 0, frame = 0;
            if (playing && !Grounded) { row = 2; frame = rb.linearVelocity.y > 4 ? 1 : rb.linearVelocity.y > -3 ? 2 : 3; }
            else if (playing && Mathf.Abs(rb.linearVelocity.x) > .15f)
            { stride += Mathf.Abs(rb.linearVelocity.x) * Time.deltaTime; row = 1; frame = (int)(stride / .58f) % 4; }
            if (pulsePose > 0) { row = 3; frame = Mathf.Clamp((int)((.45f - pulsePose) * 10), 0, 3); }
            AnimationRow = row;
            body.sprite = game.art.Hero(row, frame); body.flipX = !FacingRight;
            float breath = row == 0 ? Mathf.Sin(Time.time * 1.6f) * .003f : 0;
            body.transform.localScale = new Vector3(1 + squash * .12f, 1 - squash * .10f + breath, 1);
            // Moss covers only the lowest part of the feet; solid planks need almost no inset.
            float inset = Grounded ? (standingOnTimber ? .012f : tuning.feetInset) : 0;
            body.transform.localPosition = new Vector3(0, -inset, 0);
            contactShadow.enabled = Grounded || game.State == GameState.Home;
            body.color = Invulnerable > 0 && (int)(Time.time * 14) % 2 == 0 ? new Color(.75f, 1, 1, .45f) : Color.white;
            lanternGlow.transform.localPosition = new Vector3(FacingRight ? .55f : -.55f, pulsePose > 0 ? 1.32f : .85f, 0);
            float glow = 5.3f + Mathf.Sin(Time.time * 2.8f) * .035f + pulsePose * 8;
            lanternGlow.transform.localScale = Vector3.one * glow;
            if (playing && Grounded && Mathf.Abs(move) > .1f)
            {
                stepTimer += Time.deltaTime;
                if (stepTimer > .18f) { stepTimer = 0; game.effects.Burst(transform.position + Vector3.up * .06f, new Color(.36f, .63f, .58f, .65f), 3, .7f); }
            }
        }

        void FixedUpdate()
        {
            if (game.State != GameState.Playing) { rb.linearVelocity = Vector2.zero; return; }
            float dt = Time.fixedDeltaTime;
            detachTimer = Mathf.Max(0, detachTimer - dt);
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = 1 << 8, useTriggers = false };
            // Query the physics position, not the interpolated visual Transform.
            int count = Physics2D.BoxCast(rb.position + new Vector2(0, .20f), new Vector2(.52f, .10f), 0, Vector2.down, filter, hits, .34f);
            Grounded = false; support = null;
            Collider2D floor = null;
            if (detachTimer <= 0)
                for (int i = 0; i < count; i++)
                {
                    if (hits[i].normal.y < .65f) continue;
                    var candidate = hits[i].collider.GetComponent<MovingPlatform>();
                    float floorVelocity = candidate ? candidate.Velocity.y : hits[i].rigidbody ? hits[i].rigidbody.linearVelocity.y : 0;
                    float gap = rb.position.y - hits[i].collider.bounds.max.y;
                    // An ascending platform may give the body positive world Y velocity.
                    // Only velocity relative to the support determines whether we are landing.
                    if (gap < -.12f || gap > .13f + (candidate ? Mathf.Abs(candidate.Delta.y) : 0)) continue;
                    if (rb.linearVelocity.y - floorVelocity > .8f) continue;
                    Grounded = true; support = candidate; floor = hits[i].collider; break;
                }
            if (Grounded)
            {
                coyote = tuning.coyoteTime;
                if (!wasGrounded && airTime > .15f)
                {
                    squash = 1; game.effects.Burst(transform.position, Visuals.Mint * new Color(1, 1, 1, .65f), 9, 1.7f);
                    game.audioFx.Play("land"); game.cameraRig.Shake(.045f, .12f);
                }
                airTime = 0;
            }
            else { coyote = Mathf.Max(0, coyote - dt); airTime += dt; }
            Vector2 velocity = rb.linearVelocity;
            float acceleration = Grounded ? (Mathf.Abs(move) < .01f ? tuning.groundDeceleration : tuning.groundAcceleration) : tuning.airAcceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, move * tuning.runSpeed, acceleration * dt);
            if (Grounded && floor)
            {
                var surface = floor.GetComponent<PlatformSurface>(); standingOnTimber = surface && surface.timber;
                var falling = floor.GetComponent<FallingPlatform>();
                if (falling) falling.NotifyStanding();
                // Carry once, then anchor feet to this step's target surface. Never parent the body.
                Vector2 carry = support ? support.Delta : Vector2.zero;
                float targetFeet = floor.bounds.max.y + .005f + carry.y;
                // Let the static contact solver settle instead of teleporting a resting body every tick.
                if (support || Mathf.Abs(rb.position.y - targetFeet) > .025f)
                    rb.position = new Vector2(rb.position.x + carry.x, targetFeet);
                velocity.y = -1.2f;
            }
            if (buffer > 0 && coyote > 0)
            {
                velocity.y = tuning.jumpSpeed; buffer = 0; coyote = 0; Grounded = false; support = null;
                detachTimer = .12f; squash = -.6f; Jumps++;
                game.effects.Burst(transform.position, Visuals.Mint, 7, 1.6f); game.audioFx.Play("jump");
            }
            if (!jumpHeld && velocity.y > 5) velocity.y = Mathf.MoveTowards(velocity.y, 5, 65 * Time.fixedDeltaTime);
            float gravity = tuning.gravity * (velocity.y < 0 ? tuning.fallMultiplier : 1);
            velocity.y = Mathf.Max(velocity.y - gravity * Time.fixedDeltaTime, -tuning.maxFallSpeed);
            if (Grounded && velocity.y < 0) velocity.y = -1.2f;
            if (rb.position.x < MoonlitLevel.LeftPlayerLimit)
            {
                rb.position = new Vector2(MoonlitLevel.LeftPlayerLimit, rb.position.y);
                velocity.x = Mathf.Max(0, velocity.x);
            }
            else if (rb.position.x + velocity.x * dt < MoonlitLevel.LeftPlayerLimit)
                velocity.x = (MoonlitLevel.LeftPlayerLimit - rb.position.x) / dt;
            rb.linearVelocity = velocity; wasGrounded = Grounded;
            buffer = Mathf.Max(0, buffer - dt);
            if (transform.position.y < -7) game.Hurt(true);
        }

        public void QueueJump(bool hold = true) { buffer = tuning.jumpBuffer; if (TestInput) jumpHeld = hold; }
        public void ReleaseJump() { jumpHeld = false; }
        public void ClearInput(float suppressSeconds = 0)
        {
            buffer = 0; move = 0; jumpHeld = false;
            InputSuppressedUntil = Time.unscaledTime + suppressSeconds;
        }

        public bool Pulse()
        {
            if (game.State != GameState.Playing || Cooldown > 0) return false;
            Cooldown = tuning.pulseCooldown; pulsePose = .45f; Pulses++;
            var at = (Vector2)transform.position + Vector2.up * .9f;
            game.effects.Ring(at, Visuals.Gold, tuning.pulseRadius); game.effects.Burst(at, Visuals.Gold, 30, 4);
            foreach (var e in game.enemies) if (Vector2.Distance(at, (Vector2)e.transform.position + Vector2.up * .4f) < tuning.pulseRadius) e.Stun(tuning.stunDuration);
            game.cameraRig.Shake(.09f, .2f); game.audioFx.Play("pulse"); return true;
        }

        public void Teleport(Vector2 at)
        {
            rb.position = at; transform.position = at; rb.linearVelocity = Vector2.zero;
            buffer = coyote = detachTimer = airTime = 0; wasGrounded = false; Grounded = false; support = null;
            Physics2D.SyncTransforms();
        }
        public void ResetRun() { Cooldown = 0; Jumps = Pulses = 0; Invulnerable = 0; }
    }
}
