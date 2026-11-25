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
        public int AnimationFrame { get; private set; }
        public float MotionCycle => locomotion.Cycle;
        public bool HasPlantedFoot => (AnimationRow == 1 || AnimationRow == 5) && locomotion.Planted;
        public Vector2 PlantPosition => locomotion.Plant;
        public Vector2 GripPosition => locomotion.Grip;
        public Vector2 WallContact { get; private set; }
        public bool IsWallHolding { get; private set; }
        public float PulseFlash => Mathf.Clamp01(pulsePose / .45f);
        public Vector2 LanternPosition => lanternGlow.transform.position;
        public float InputSuppressedUntil { get; private set; }
        public bool IsClimbing { get; private set; }
        public bool IsDashing => dashTime > 0;
        public float DashReady => usedAirDash && !Grounded ? 0 : 1 - Mathf.Clamp01(dashRecharge / tuning.dashCooldown);
        public int AirJumps { get; private set; }
        public int Dashes { get; private set; }
        public float ClimbDistance { get; private set; }
        public event System.Action<Vector2, float> LanternPulsed;

        MoonlitGame game;
        Rigidbody2D rb;
        float move, coyote, buffer, pulsePose, squash, airTime, detachTimer, previousClimbY, wallTop, mantleSettle;
        bool jumpHeld, wasGrounded, cutJump, usedAirJump, usedAirDash;
        float dashTime, dashRecharge, dashDirection, knockbackTime, trailTimer, pushBeat;
        CuoiLocomotion locomotion;
        CuoiFootShadow footShadow;
        Material rimMaterial;
        CuoiHandLantern handLantern;
        MovingPlatform support;
        bool standingOnTimber;
        readonly RaycastHit2D[] hits = new RaycastHit2D[12];

        public void Initialize(MoonlitGame owner)
        {
            game = owner; tuning = game.tuning; rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0; rb.freezeRotation = true; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            // The contact capsule sits inside the artwork and clears the corners of bamboo ledges.
            var collider = GetComponent<CapsuleCollider2D>(); collider.size = new Vector2(.7f, 1.55f); collider.offset = new Vector2(0, .78f);
            collider.sharedMaterial = new PhysicsMaterial2D("Cuoi frictionless") { friction = 0, bounciness = 0 };
            footShadow = new CuoiFootShadow(transform);
            var rim = Resources.Load<Shader>("CuoiRim");
            if (rim && rim.isSupported)
            {
                rimMaterial = new Material(rim) { name = "Cuội • moon rim and amber key" };
                body.sharedMaterial = rimMaterial;
            }
            lanternGlow.sortingOrder = body.sortingOrder - 1;
            handLantern = new CuoiHandLantern(body);
            locomotion = new CuoiLocomotion(body, rimMaterial);
        }

        void Update()
        {
            var kb = Keyboard.current;
            bool playing = game.State == GameState.Playing;
            if (game.State == GameState.Paused || game.State == GameState.Tutorial) return;
            move = 0;
            if (playing && Time.unscaledTime >= InputSuppressedUntil)
            {
                if (TestInput) move = TestMove;
                else if (kb != null)
                {
                    move = ((kb.dKey.isPressed || kb.rightArrowKey.isPressed) ? 1 : 0) - ((kb.aKey.isPressed || kb.leftArrowKey.isPressed) ? 1 : 0);
                    bool holding = kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed;
                    if (jumpHeld && !holding) ReleaseJump();
                    jumpHeld = holding;
                    if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) QueueJump();
                    if (kb.fKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame) Pulse();
                    if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame || kb.kKey.wasPressedThisFrame) Dash();
                    if (kb.eKey.wasPressedThisFrame) game.Interact();
                    if (kb.rKey.wasPressedThisFrame) game.Hurt(true);
                }
            }
            if (playing)
            {
                float before = Cooldown;
                Cooldown = Mathf.Max(0, Cooldown - Time.deltaTime);
                if (before > 0 && Cooldown <= 0) game.audioFx.Play("ready");
                Invulnerable = Mathf.Max(0, Invulnerable - Time.deltaTime);
            }
            if (!IsDashing && Mathf.Abs(move) > .01f)
                FacingRight = Grounded && Mathf.Abs(rb.linearVelocity.x) > .3f ? rb.linearVelocity.x > 0 : move > 0;
            pulsePose = Mathf.Max(0, pulsePose - Time.deltaTime);
            squash = Mathf.MoveTowards(squash, 0, Time.deltaTime * 4);
            int row = 0, frame = 0;
            if (playing && !Grounded) { row = 2; frame = rb.linearVelocity.y > 4 ? 1 : rb.linearVelocity.y > -3 ? 2 : 3; }
            else if (playing && Mathf.Abs(rb.linearVelocity.x) > .15f) row = 1;
            if (pulsePose > 0) { row = 3; frame = Mathf.Clamp((int)((.45f - pulsePose) * 10), 0, 3); }
            if (playing && IsClimbing) row = 4;
            if (playing && mantleSettle > 0 && Grounded) row = 5;
            if (playing && IsDashing) { row = 2; frame = 2; }
            AnimationRow = row;
            float inset = Grounded ? (standingOnTimber ? .012f : tuning.feetInset) : 0;
            body.color = Invulnerable > 0 && (int)(Time.time * 14) % 2 == 0 ? new Color(.75f, 1, 1, .45f) : Color.white;
            if (row == 1 || row == 4 || row == 5)
            {
                if (row == 1) locomotion.Run(transform.position, FacingRight, support ? support.transform : null, Mathf.Min(.032f, inset));
                else if (row == 5) locomotion.Mantle(transform.position, FacingRight, WallContact.x, wallTop, 3);
                else locomotion.Climb(transform.position, FacingRight, WallContact.x, wallTop, IsWallHolding);
                frame = locomotion.Frame;
                lanternGlow.transform.position = handLantern.MotionPose(locomotion.Lantern, FacingRight, PulseFlash, MotionCycle, row >= 4, IsWallHolding);
                if (locomotion.Strike)
                {
                    var at = row == 4 ? GripPosition : PlantPosition;
                    game.effects.Burst(at, new Color(.53f, .54f, .38f, .58f), row == 4 ? 2 : 3, .6f);
                    game.audioFx.Play(row == 4 ? "climb" : standingOnTimber ? "woodStep" : "step");
                }
            }
            else
            {
                locomotion.Reset();
                body.sprite = game.art.Hero(row, frame); body.flipX = !FacingRight;
                float breath = row == 0 ? Mathf.Sin(Time.time * 1.6f) * .003f : 0;
                body.transform.localScale = new Vector3(1 + squash * .07f + (IsDashing ? .12f : 0), 1 - squash * .06f + breath - (IsDashing ? .10f : 0), 1);
                body.transform.localPosition = new Vector3(0, -inset, 0);
                lanternGlow.transform.position = handLantern.Pose(row, frame, FacingRight, PulseFlash, Mathf.Abs(rb.linearVelocity.x));
            }
            AnimationFrame = frame;
            float glow = 5.3f + Mathf.Sin(Time.time * 2.8f) * .035f + pulsePose * 8;
            lanternGlow.transform.localScale = Vector3.one * glow;
        }

        void LateUpdate() { footShadow.Update(transform.position, FacingRight, HasPlantedFoot ? PlantPosition.x : transform.position.x); }
        void OnDestroy() { if (rimMaterial) Destroy(rimMaterial); }

        void FixedUpdate()
        {
            if (game.State != GameState.Playing) { rb.linearVelocity = Vector2.zero; return; }
            float dt = Time.fixedDeltaTime;
            mantleSettle = Mathf.Max(0, mantleSettle - dt);
            detachTimer = Mathf.Max(0, detachTimer - dt);
            knockbackTime = Mathf.Max(0, knockbackTime - dt);
            dashRecharge = Mathf.Max(0, dashRecharge - dt);
            pushBeat = Mathf.Max(0, pushBeat - dt);
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
                usedAirJump = usedAirDash = false;
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
            if (knockbackTime <= 0) velocity.x = Mathf.MoveTowards(velocity.x, move * tuning.runSpeed, acceleration * dt);
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
            bool wasClimbing = IsClimbing;
            if (wasClimbing) ClimbDistance += Mathf.Max(0, rb.position.y - previousClimbY);
            previousClimbY = rb.position.y;
            IsClimbing = IsWallHolding = false;
            if (Mathf.Abs(move) > .01f && knockbackTime <= 0 && !IsDashing)
            {
                var direction = Vector2.right * Mathf.Sign(move);
                var wall = Physics2D.Raycast(rb.position + Vector2.up * .20f, direction, .46f, 1 << 8);
                if (!wall.collider || Mathf.Abs(wall.normal.x) < .65f)
                    wall = Physics2D.Raycast(rb.position + Vector2.up * .80f, direction, .46f, 1 << 8);
                if (wall.collider && Mathf.Abs(wall.normal.x) >= .65f)
                {
                    var crate = wall.collider.GetComponent<PushableCrate>();
                    if (crate && Grounded && !crate.Docked)
                    {
                        crate.Push(direction.x); velocity.x = direction.x * 2;
                        if (pushBeat <= 0) { game.audioFx.Play("crate"); pushBeat = .34f; }
                    }
                    else if (game.Chapter >= 3 && !Grounded && (jumpHeld || wasClimbing) && !crate && !wall.collider.usedByEffector &&
                        wall.collider.bounds.size.y >= 2 && wall.collider.GetComponent<PlatformSurface>())
                    {
                        IsClimbing = true; IsWallHolding = !jumpHeld;
                        WallContact = wall.point; wallTop = wall.collider.bounds.max.y; buffer = 0; cutJump = false;
                        velocity.x = 0;
                        float stroke = Mathf.Sin(MotionCycle * Mathf.PI * 4 - .55f);
                        velocity.y = IsWallHolding ? 0 : tuning.climbSpeed * (.86f + .20f * stroke);
                        if (!IsWallHolding && wallTop - rb.position.y < .34f)
                        {
                            // Commit the knee-over only when the far side supports a full
                            // capsule and there is headroom. Low ceilings keep the wall hold.
                            var landing = new Vector2(wall.point.x + direction.x * .44f, wallTop + .012f);
                            var top = Physics2D.Raycast(landing + Vector2.up * .17f, Vector2.down, .27f, 1 << 8);
                            bool space = !Physics2D.OverlapCapsule(landing + Vector2.up * .80f, new Vector2(.69f, 1.54f), CapsuleDirection2D.Vertical, 0, 1 << 8);
                            if (top.collider && top.normal.y > .85f && space)
                            {
                                rb.position = landing; velocity = Vector2.zero; IsClimbing = false; Grounded = true;
                                mantleSettle = .13f; airTime = 0; usedAirJump = usedAirDash = false;
                            }
                        }
                    }
                }
            }
            bool airJump = !Grounded && coyote <= 0;
            if (buffer > 0 && !IsClimbing && !IsDashing && knockbackTime <= 0 &&
                (!airJump || (game.Chapter >= 2 && !usedAirJump)))
            {
                velocity.y = airJump ? tuning.airJumpSpeed : tuning.jumpSpeed;
                if (airJump)
                {
                    usedAirJump = true; AirJumps++;
                    game.effects.Ring(rb.position + Vector2.up * .1f, Visuals.Mint, 1.05f);
                }
                buffer = 0; coyote = 0; Grounded = false; support = null;
                detachTimer = .12f; squash = -.6f; Jumps++;
                game.effects.Burst(transform.position, Visuals.Mint, airJump ? 13 : 7, 1.6f);
                game.audioFx.Play(airJump ? "airJump" : "jump");
                cutJump = !jumpHeld;
            }
            if (cutJump && velocity.y > 0) velocity.y *= .5f;
            cutJump = false;
            float gravity = tuning.gravity * (velocity.y < 0 ? tuning.fallMultiplier : 1);
            if (!IsClimbing) velocity.y = Mathf.Max(velocity.y - gravity * dt, -tuning.maxFallSpeed);
            if (Grounded && velocity.y < 0) velocity.y = -1.2f;
            if (IsDashing)
            {
                velocity = new Vector2(dashDirection * tuning.dashSpeed, 0);
                dashTime = Mathf.Max(0, dashTime - dt); trailTimer -= dt;
                if (trailTimer <= 0)
                {
                    game.effects.Burst(rb.position + Vector2.up * .75f - Vector2.right * dashDirection * .45f, Visuals.Mint, 6, 1.3f);
                    trailTimer = .04f;
                }
                if (dashTime <= 0) velocity.x = dashDirection * tuning.runSpeed;
            }
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
        public void ReleaseJump() { if (jumpHeld) cutJump = true; jumpHeld = false; }
        public bool Dash()
        {
            if (game.State != GameState.Playing || game.Chapter < 4 || IsDashing || dashRecharge > 0 || knockbackTime > 0 || (usedAirDash && !Grounded)) return false;
            dashDirection = Mathf.Abs(move) > .01f ? Mathf.Sign(move) : FacingRight ? 1 : -1;
            FacingRight = dashDirection > 0; usedAirDash = !Grounded;
            dashTime = tuning.dashDuration; dashRecharge = tuning.dashCooldown;
            buffer = 0; trailTimer = 0; Dashes++; IsClimbing = false;
            game.audioFx.Play("dash"); return true;
        }
        public void ApplyKnockback(float direction)
        {
            dashTime = 0; IsClimbing = false; Grounded = false; coyote = 0; buffer = 0; cutJump = false;
            knockbackTime = .25f; detachTimer = .12f; Invulnerable = 1;
            rb.linearVelocity = new Vector2(Mathf.Sign(direction) * 5, 6);
        }
        public void ClearInput(float suppressSeconds = 0)
        {
            buffer = 0; move = 0; jumpHeld = false; cutJump = false;
            InputSuppressedUntil = Time.unscaledTime + suppressSeconds;
        }

        public bool Pulse()
        {
            if (game.State != GameState.Playing || Cooldown > 0) return false;
            Cooldown = tuning.pulseCooldown; pulsePose = .45f; Pulses++;
            var at = (Vector2)transform.position + Vector2.up * .9f;
            game.effects.Ring(at, Visuals.Gold, tuning.pulseRadius); game.effects.Burst(at, Visuals.Gold, 30, 4);
            foreach (var e in game.enemies) if (Vector2.Distance(at, (Vector2)e.transform.position + Vector2.up * .4f) < tuning.pulseRadius) e.Stun(tuning.stunDuration);
            game.level.PulseAt(at);
            LanternPulsed?.Invoke(at, tuning.pulseRadius);
            game.cameraRig.Shake(.09f, .2f); game.audioFx.Play("pulse"); return true;
        }

        public void Teleport(Vector2 at)
        {
            rb.position = at; transform.position = at; rb.linearVelocity = Vector2.zero;
            buffer = coyote = detachTimer = airTime = dashTime = dashRecharge = knockbackTime = mantleSettle = 0;
            wasGrounded = Grounded = IsClimbing = IsWallHolding = usedAirJump = usedAirDash = cutJump = false; support = null;
            locomotion.Reset();
            Physics2D.SyncTransforms();
        }
        public void ResetRun() { Cooldown = 0; Jumps = Pulses = AirJumps = Dashes = 0; ClimbDistance = 0; Invulnerable = 0; ClearInput(); }
    }
}
