using UnityEngine;

namespace Moonlit
{
    public enum WetlandSpecies { Fish, Crow }
    public enum WetlandPhase { Resting, Warning, Attacking, Returning, Stunned, Recovering }

    public sealed class RiverAndSkyCreature : MonoBehaviour
    {
        public WetlandSpecies Kind { get; private set; }
        public WetlandPhase Phase { get; private set; }
        public Vector2 BodyCenter { get; private set; }
        public Vector2 Anchor { get; private set; }
        public bool Warning => Phase == WetlandPhase.Warning;
        public bool IsStunned => Phase == WetlandPhase.Stunned;
        public int AttackCount { get; private set; }
        public int StunCount { get; private set; }
        public int HitCount { get; private set; }

        MoonlitGame game;
        WetlandCreatureArt art;
        Camera view;
        Vector2 lockedTarget, phaseOrigin, previousPlayer;
        float floor, reach, height, travel, direction, idleTime, remaining, duration, rhythm;
        bool resolvedContact, resetOnExit;
        bool Carp => Kind == WetlandSpecies.Fish;
        Vector2 PerchCenter => Anchor + Vector2.up * .47f;

        public static RiverAndSkyCreature Fish(MoonlitGame game, Transform parent, Vector2 surface, float leapHeight = 2.6f, float horizontalTravel = 1.6f)
        {
            var fish = Create(game, parent, WetlandSpecies.Fish, surface);
            fish.reach = 3.6f; fish.floor = surface.y; fish.height = leapHeight; fish.travel = horizontalTravel;
            fish.ResetCreature();
            return fish;
        }

        public static RiverAndSkyCreature Crow(MoonlitGame game, Transform parent, Vector2 perch, float floorY, float patrolRadius = 4f)
        {
            var crow = Create(game, parent, WetlandSpecies.Crow, perch);
            crow.reach = patrolRadius; crow.floor = floorY;
            crow.ResetCreature();
            return crow;
        }

        static RiverAndSkyCreature Create(MoonlitGame game, Transform parent, WetlandSpecies species, Vector2 anchor)
        {
            var node = new GameObject(species == WetlandSpecies.Fish ? "River Carp" : "Perched Crow");
            node.transform.SetParent(parent, false);
            node.transform.position = anchor;
            var animal = node.AddComponent<RiverAndSkyCreature>();
            animal.game = game; animal.Kind = species; animal.Anchor = anchor;
            animal.view = game.cameraRig.GetComponent<Camera>();
            animal.art = new WetlandCreatureArt(species, node.transform);
            game.player.LanternPulsed += animal.Pulsed;
            return animal;
        }

        public void ResetCreature()
        {
            direction = 1; idleTime = .45f; resolvedContact = false;
            previousPlayer = game.player.Body.position;
            BodyCenter = Carp ? Anchor + Vector2.down * .20f : PerchCenter;
            Change(WetlandPhase.Resting, 0);
            Pose();
        }

        void Change(WetlandPhase next, float seconds)
        {
            Phase = next; remaining = duration = seconds; phaseOrigin = BodyCenter;
        }

        void Update()
        {
            if (game.State != GameState.Playing)
            {
                if ((game.State == GameState.Defeat || game.State == GameState.Respawning) && !resetOnExit)
                {
                    ResetCreature(); resetOnExit = true;
                }
                return;
            }
            resetOnExit = false;
            Vector2 player = game.player.Body.position;
            if ((player - previousPlayer).sqrMagnitude > 144 && Phase != WetlandPhase.Resting) ResetCreature();
            previousPlayer = player;
            rhythm += Time.deltaTime;
            remaining = Mathf.Max(0, remaining - Time.deltaTime);
            float p = duration > 0 ? 1 - remaining / duration : 0;
            switch (Phase)
            {
                case WetlandPhase.Resting:
                    idleTime = Mathf.Max(0, idleTime - Time.deltaTime);
                    BodyCenter = Carp ? Anchor + new Vector2(.22f * Mathf.Sin(rhythm * 1.5f), -.2f + .04f * Mathf.Sin(rhythm * 2.4f)) : PerchCenter;
                    if (idleTime <= 0 && CanNotice(player)) BeginWarning(player);
                    break;
                case WetlandPhase.Warning:
                    if (Carp) BodyCenter = Vector2.Lerp(phaseOrigin, Takeoff(), p);
                    if (remaining <= 0) BeginAttack();
                    break;
                case WetlandPhase.Attacking:
                    BodyCenter = Carp ? FishArc(p) : CrowSwoop(p);
                    if (p > .07f && p < .93f && (!Carp || BodyCenter.y > floor + .16f)) TryContact(player);
                    if (remaining <= 0) BeginReturn();
                    break;
                case WetlandPhase.Returning:
                    BodyCenter = Vector2.Lerp(phaseOrigin, Carp ? Anchor + Vector2.down * .20f : PerchCenter, Mathf.SmoothStep(0, 1, p));
                    BodyCenter += Vector2.up * Mathf.Sin(p * Mathf.PI) * (Carp ? -.26f : .85f);
                    if (remaining <= 0) Change(WetlandPhase.Recovering, Carp ? 3.7f : 4.0f);
                    break;
                case WetlandPhase.Stunned:
                    float settle = Mathf.SmoothStep(0, 1, Mathf.Min(1, (duration - remaining) / .55f));
                    Vector2 safeSpot = Carp ? new Vector2(phaseOrigin.x, floor + .04f) :
                        Vector2.Distance(phaseOrigin, PerchCenter) < .50f ? PerchCenter : new Vector2(phaseOrigin.x, floor + .46f);
                    BodyCenter = Vector2.Lerp(phaseOrigin, safeSpot, settle);
                    if (remaining <= 0) BeginReturn();
                    break;
                case WetlandPhase.Recovering:
                    if (remaining <= 0) { Change(WetlandPhase.Resting, 0); idleTime = 1.35f; }
                    break;
            }
            Pose();
        }

        bool CanNotice(Vector2 feet)
        {
            if (Mathf.Abs(feet.x - Anchor.x) > reach || game.player.Invulnerable > .1f) return false;
            if (feet.y < floor - .35f || feet.y > floor + (Carp ? height + 1.1f : 2.7f)) return false;
            // Test the emergence point, not a fish lying below an opaque river surface.
            Vector2 point = Carp ? Anchor + Vector2.up * Mathf.Min(height, .85f) : PerchCenter;
            return OnScreen(point);
        }

        bool OnScreen(Vector2 point)
        {
            Vector3 screen = view.WorldToViewportPoint(point);
            return screen.z > 0 && screen.x > .025f && screen.x < .975f && screen.y > .015f && screen.y < .985f;
        }

        Vector2 Takeoff() => Anchor + new Vector2(-direction * travel * .5f, -.15f);
        Vector2 FishArc(float p)
        {
            return Takeoff() + new Vector2(direction * travel * p, 4 * (height + .15f) * p * (1 - p));
        }
        Vector2 CrowSwoop(float p)
        {
            if (p <= .72f)
            {
                float q = p / .72f;
                return Vector2.Lerp(PerchCenter, lockedTarget, q) + Vector2.up * Mathf.Sin(q * Mathf.PI) * .30f;
            }
            return lockedTarget + new Vector2(direction * 1.3f, .66f) * ((p - .72f) / .28f);
        }

        void BeginWarning(Vector2 player)
        {
            direction = player.x < Anchor.x ? -1 : 1;
            lockedTarget = Carp ? Anchor : new Vector2(player.x, Mathf.Clamp(player.y + .80f, floor + .72f, floor + 1.75f));
            Change(WetlandPhase.Warning, Carp ? .90f : 1.05f);
            game.audioFx.Play(Carp ? "carpBubbles" : "crowCall");
        }

        void BeginAttack()
        {
            if (!OnScreen(Carp ? Anchor + Vector2.up * .7f : BodyCenter))
            {
                BeginReturn(); return;
            }
            resolvedContact = false; AttackCount++;
            Change(WetlandPhase.Attacking, Carp ? 1.10f : Mathf.Clamp(Vector2.Distance(PerchCenter, lockedTarget) / 7 + .28f,.78f,1.25f));
            game.audioFx.Play(Carp ? "carpLeap" : "crowFlight");
            if (Carp) art.Splash(Takeoff(), game);
        }

        void BeginReturn()
        {
            if (Carp && Phase == WetlandPhase.Attacking && OnScreen(BodyCenter))
            {
                art.Splash(new Vector2(BodyCenter.x, floor), game);
                game.audioFx.Play("carpSplash");
            }
            Change(WetlandPhase.Returning, Carp ? .72f : 1.18f);
        }

        void TryContact(Vector2 feet)
        {
            if (resolvedContact || game.player.Invulnerable > 0 || game.player.IsDashing || !OnScreen(BodyCenter)) return;
            // Ellipse against the vertical core of Cuội's capsule. Fins, wingtips,
            // tails and the tip of his hat stay outside the damaging contact.
            float dx = (feet.x - BodyCenter.x) / (Carp ? .68f : .69f);
            float y = Mathf.Clamp(BodyCenter.y, feet.y + .28f, feet.y + game.player.ContactHeight - .32f);
            float dy = (y - BodyCenter.y) / (Carp ? .49f : .48f);
            if (dx * dx + dy * dy > 1) return;
            resolvedContact = true;
            int before = game.Health;
            game.Hurt(false, Mathf.Abs(feet.x - BodyCenter.x) > .12f ? Mathf.Sign(feet.x - BodyCenter.x) : direction);
            if (game.Health < before) HitCount++;
        }

        void Pulsed(Vector2 center, float radius)
        {
            if (!isActiveAndEnabled || game.State != GameState.Playing || IsStunned || Vector2.Distance(center, BodyCenter) > radius) return;
            StunCount++; resolvedContact = true;
            Change(WetlandPhase.Stunned, game.tuning.stunDuration);
            game.effects.Burst(BodyCenter, Visuals.Mint, 12, 1.65f);
            game.audioFx.Play("stun");
            Pose();
        }

        void Pose()
        {
            float p = duration > 0 ? 1 - remaining / duration : 0;
            int frame = 0;
            if (Warning) frame = 1;
            else if (Phase == WetlandPhase.Attacking) frame = Carp && p > .52f ? 3 : 2;
            else if (Phase == WetlandPhase.Returning && !Carp) frame = (int)(rhythm * 7) % 2 + 2;
            float facing = !Carp && Phase == WetlandPhase.Returning ? Mathf.Sign(Anchor.x - phaseOrigin.x) : direction;
            art.Pose(BodyCenter, lockedTarget, Phase, frame, facing, rhythm, p);
        }

        void OnEnable() { if (game && game.player) game.player.LanternPulsed += Pulsed; }
        void OnDisable() { if (game && game.player) game.player.LanternPulsed -= Pulsed; }
    }
}
