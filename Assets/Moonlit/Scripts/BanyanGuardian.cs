using System;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class BanyanGuardian : MonoBehaviour
    {
        public enum Move { Charge, LowSweep, HighSweep, Slam }
        enum Beat { Waking, Windup, Strike, Flowering, Resting, Homecoming }

        // Each new blossom introduces one phrase. A missed opening repeats its phrase.
        static readonly Move[][] Phrases = {
            new[] { Move.Charge },
            new[] { Move.Slam },
            new[] { Move.LowSweep, Move.HighSweep },
            new[] { Move.Charge, Move.Slam },
            new[] { Move.LowSweep, Move.HighSweep, Move.LowSweep },
            new[] { Move.Charge, Move.Charge, Move.Slam }
        };
        const float SwingReach = 8.3f;
        const float LandingFraction = .44f;
        const float ShockReach = 14;
        MoonlitGame game;
        Action onCalmed;
        Vector2 foot;
        Beat beat;
        int lights, step;
        float clock, elapsed, duration, startX, endX, direction = -1, lift, travelDust;
        bool struck, landed, finished;
        public int RootCount => 3;
        public int LitBuds => lights;
        public int CalmedRoots => lights / 2;
        public bool Active => game && !finished;
        public bool TargetOpen => Active && beat == Beat.Flowering;
        public bool Warning => beat == Beat.Windup;
        public bool Attacking => beat == Beat.Strike;
        public Move Attack { get; private set; }
        public float Direction => direction;
        public float BarkX { get; private set; }
        public float MarkX => endX;
        public float FloorY => foot.y;
        public float Progress => Mathf.Clamp01(elapsed / duration);
        public float ShockDistance => ShockReach * Mathf.Clamp01((Progress - LandingFraction) / (1 - LandingFraction));
        public float ContactX => Attack == Move.Charge ? BarkX + direction * 1.85f : endX;
        public Vector2 TargetPosition => new Vector2(BarkX + (BarkX < foot.x - 8 ? 1 : -1) * 1.65f,
            foot.y + (lights < 2 ? 2.1f : lights < 4 ? 3.15f : 4.35f));
        float LeftAnchor => foot.x - 15;
        float RightAnchor => foot.x - 1;
        float ZoneHeight => Attack == Move.HighSweep ? 3.05f : .80f;

        public string Hint
        {
            get {
                switch (beat)
                {
                    case Beat.Waking: return "Chờ nụ vàng hé mở rồi đến gần rọi đèn.";
                    case Beat.Flowering: return $"Nụ đã mở! Đến gần, bấm {game.Controls.Label(GameAction.Lantern)} để rọi đèn.";
                    case Beat.Resting: return lights % 2 == 0 ? "Một chiếc rễ đã nằm yên." : "Nụ đã sáng lên.";
                    case Beat.Homecoming: return "Cây đa đã thôi vùng vẫy.";
                }
                switch (Attack)
                {
                    case Move.Charge: return "Cây sắp lao tới! Lướt qua hoặc nhảy tránh.";
                    case Move.HighSweep: return "Cành quét ngang đầu. Đứng dưới cành!";
                    case Move.LowSweep: return "Rễ quét sát đất. Nhảy qua!";
                    default: return "Tránh chỗ đất sáng, rồi nhảy qua đợt rễ trồi lên.";
                }
            }
        }

        public static BanyanGuardian Begin(MoonlitGame game, Vector2 treeFoot, Action onCalmed)
        {
            var node = new GameObject("Banyan Guardian");
            node.transform.SetParent(game.level.transform, false);
            node.transform.position = treeFoot;
            var boss = node.AddComponent<BanyanGuardian>();
            boss.game = game; boss.foot = treeFoot; boss.onCalmed = onCalmed;
            boss.BarkX = boss.RightAnchor;
            boss.BuildArt();
            game.player.LanternPulsed += boss.ReceiveLight;
            boss.Enter(Beat.Waking, 2.3f);
            return boss;
        }

        void Enter(Beat next, float seconds)
        {
            beat = next; elapsed = 0; duration = seconds;
            if (next == Beat.Flowering) {
                game.audioFx.Play("ready"); game.effects.Ring(TargetPosition, Visuals.Gold, 1.8f);
            }
            Draw();
        }

        void Prepare()
        {
            Attack = Phrases[lights][step];
            startX = BarkX;
            float playerX = game.player.Body.position.x;
            if (Attack == Move.Charge) endX = BarkX < (LeftAnchor + RightAnchor) * .5f ? RightAnchor : LeftAnchor;
            else if (Attack == Move.Slam) endX = Mathf.Clamp(playerX, LeftAnchor - 1.5f, RightAnchor + 1);
            else endX = BarkX + (playerX < BarkX ? -1 : 1) * SwingReach;
            direction = endX >= startX ? 1 : -1;
            landed = struck = false; travelDust = 0;
            float lead = Attack == Move.Slam ? 1.12f : Attack == Move.Charge ? .96f : .78f;
            if (step > 0) lead -= .15f;
            game.audioFx.Play(Attack == Move.Slam ? "rootDrum" : Attack == Move.Charge ? "rootCoil" : "warn");
            Enter(Beat.Windup, lead);
        }

        public void Regroup()
        {
            if (!Active || beat == Beat.Homecoming) return;
            // Retry preserves earned lights and restarts the warning with both feet on the ground.
            BarkX = Mathf.Clamp(BarkX, LeftAnchor, RightAnchor);
            lift = 0; step = 0;
            Enter(Beat.Waking, 1.35f);
        }

        void Update()
        {
            if (!Active || game.State != GameState.Playing) return;
            elapsed += Time.deltaTime; clock += Time.deltaTime;
            if (Attacking) Strike();
            Draw();
            if (elapsed < duration || game.State != GameState.Playing) return;
            switch (beat)
            {
                case Beat.Waking:
                case Beat.Resting:
                case Beat.Flowering: step = 0; Prepare(); break;
                case Beat.Windup:
                    game.audioFx.Play(Attack == Move.Charge ? "rootRush" : "sweep");
                    Enter(Beat.Strike, Attack == Move.Slam ? 1.95f : Attack == Move.Charge ? 1.22f : .76f);
                    break;
                case Beat.Strike:
                    lift = 0;
                    if (++step < Phrases[lights].Length) Prepare();
                    else Enter(Beat.Flowering, 5.25f);
                    break;
                case Beat.Homecoming:
                    finished = true; onCalmed?.Invoke(); break;
            }
        }

        void Strike()
        {
            float t = Progress;
            Vector2 player = game.player.Body.position;
            if (Attack == Move.Charge)
            {
                BarkX = Mathf.Lerp(startX, endX, Mathf.SmoothStep(0, 1, t));
                if (t > .07f && t < .96f && Mathf.Abs(player.x - ContactX) < 1.02f && player.y < foot.y + 1.62f)
                    Hit(direction);
                if (elapsed >= travelDust) {
                    travelDust = elapsed + .13f;
                    game.effects.Burst(new Vector2(BarkX - direction, foot.y + .12f), BarkDust, 4, 1.5f);
                }
            }
            else if (Attack == Move.Slam)
            {
                float flight = Mathf.Clamp01(t / LandingFraction);
                BarkX = Mathf.Lerp(startX, endX, Mathf.SmoothStep(0, 1, flight));
                lift = 2.55f * Mathf.Sin(Mathf.PI * flight);
                if (t < LandingFraction) return;
                if (!landed) {
                    landed = true;
                    game.audioFx.Play("rootImpact"); game.cameraRig.Shake(.21f, .22f);
                    game.effects.Burst(new Vector2(endX, foot.y + .22f), BarkDust, 28, 4);
                    game.effects.Ring(new Vector2(endX, foot.y + .12f), Visuals.Gold, 2.7f);
                }
                float offset = player.x - endX;
                if (Mathf.Abs(Mathf.Abs(offset) - ShockDistance) < .78f && player.y < foot.y + 1.02f)
                    Hit(offset < 0 ? -1 : 1);
            }
            else
            {
                float span = SwingSpan(t);
                float forward = (player.x - BarkX) * direction;
                float y = player.y - foot.y;
                if (t > .12f && t < .78f && forward >= -.12f && forward <= span + .2f &&
                    y < ZoneHeight + .46f && y + game.player.ContactHeight - .05f > ZoneHeight - .35f) Hit(direction);
            }
        }

        static float SwingSpan(float t) => Mathf.Lerp(1.7f, SwingReach, Mathf.Sin(Mathf.PI * Mathf.Clamp01(t)) );
        void Hit(float away)
        {
            if (struck || game.player.Invulnerable > 0) return;
            // Crossing a root while dashing clears this strike, including its trailing edge.
            struck = true;
            if (!game.player.IsDashing) game.Hurt(false, away);
        }

        void ReceiveLight(Vector2 at, float radius)
        {
            if (game.State != GameState.Playing || !TargetOpen || Vector2.Distance(at, TargetPosition) > radius) return;
            Vector2 blossom = TargetPosition;
            lights++;
            game.effects.Burst(blossom, Visuals.Gold, 30, 3.3f);
            game.effects.Ring(blossom, Visuals.Mint, 2);
            game.audioFx.Play("bloom");
            if (lights % 2 == 0)
            {
                var crown = new Vector2(BarkX, foot.y + 4.8f);
                game.effects.Ring(crown, Visuals.Mint, 3.6f);
                game.effects.Burst(crown, Visuals.Gold, 18, 2.6f);
                game.audioFx.Play("signal" + CalmedRoots);
            }
            if (lights == Phrases.Length)
            {
                game.Toast("SÁU NỤ ĐÃ NỞ", "Cuội hạ đèn. Cây đa đứng yên.");
                Enter(Beat.Homecoming, 2.25f);
            }
            else Enter(Beat.Resting, lights % 2 == 0 ? 2.05f : 1.0f);
        }

        void OnDestroy()
        {
            if (game && game.player) game.player.LanternPulsed -= ReceiveLight;
        }
    }
}
