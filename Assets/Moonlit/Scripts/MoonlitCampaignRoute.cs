using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Moonlit
{
    // Drives the same jump buffer, motion input, lantern and dash used by the keyboard.
    // Geometry guides takeoff timing. Movement, collisions, damage and checkpoints remain live.
    public static class MoonlitCampaignRoute
    {
        public static IEnumerator Play(MoonlitGame game, Action<bool, string> done, int firstChapter = 1)
        {
            game.StartChapter(firstChapter);
            while (game.IsLoadingChapter) yield return null;
            var player = game.player;
            player.TestInput = true;
            int chapters = firstChapter - 1;
            string summary = "";
            for (int chapter = firstChapter; chapter <= 4; chapter++)
            {
                var route = new Driver(game);
                bool captured = false, capturedScenery = false, capturedProcession = false, homeward = false;
                float sceneryAt = chapter == 1 ? 320 : chapter == 2 ? 263 : chapter == 3 ? 412 : 190;
                float started = Time.unscaledTime;
                int lastCheckpoint = 0, lastGateStep = 0;
                float advancedAt = 0, previousX = 0;
                while (Time.unscaledTime - started < 240)
                {
                    if (game.State == GameState.Reunion && !homeward && game.hud.StoryTime>5.9f)
                    {
                        MoonlitCampaignPlaytest.Capture(game,"story-reunion"); homeward=true;
                    }
                    if (game.State == GameState.Tutorial)
                    {
                        if (game.tutorials.CanContinue) game.tutorials.Continue();
                    }
                    else if (game.State == GameState.Defeat)
                    {
                        player.TestMove = 0;
                        done(false, "Defeat " + route.Status()); yield break;
                    }
                    else if (game.State == GameState.Respawning)
                    {
                        route.ResetFlight(); previousX = game.RespawnPoint.x - 2; advancedAt = game.Elapsed;
                    }
                    else if (game.State == GameState.Playing)
                    {
                        if (game.Boss && game.Boss.Active) route.PlayGuardian();
                        else
                        {
                            route.Advance();
                            if (player.Body.position.x >= previousX + .25f || route.GateStep != lastGateStep)
                            {
                                previousX = player.Body.position.x; advancedAt = game.Elapsed; lastGateStep = route.GateStep;
                            }
                            if (game.Elapsed - advancedAt > 11)
                            {
                                MoonlitCampaignPlaytest.Capture(game, "route-stall");
                                string missing = string.Join(", ",game.level.pickups.Where(item=>item.kind==PickupKind.Mote && !item.Collected).Select(item=>item.name));
                                done(false, "No forward progress " + route.Status() + " missing: " + missing); yield break;
                            }
                        }
                        if (!capturedScenery && player.Body.position.x >= sceneryAt)
                        {
                            MoonlitCampaignPlaytest.Capture(game, "scenery-" + chapter);
                            capturedScenery = true;
                        }
                        if (!capturedProcession && chapter == 3 && game.level.Procession && game.level.Procession.LitCount == 2 &&
                            player.Body.position.x >= 367)
                        {
                            MoonlitCampaignPlaytest.Capture(game, "route-procession");
                            capturedProcession = true;
                        }
                        if (game.CheckpointIndex > lastCheckpoint)
                        {
                            lastCheckpoint = game.CheckpointIndex;
                            Debug.Log("CAMPAIGN_ROUTE checkpoint=" + lastCheckpoint + " " + route.Status());
                        }
                        if (!captured && lastCheckpoint > 0)
                        {
                            MoonlitCampaignPlaytest.Capture(game, "route-" + chapter);
                            captured = true;
                        }
                    }
                    else if (game.State == GameState.Complete || game.State == GameState.Ending)
                    {
                        if (game.Shards != chapter || (chapter == 4 && game.State != GameState.Ending))
                        {
                            done(false, "Unexpected chapter result " + route.Status()); yield break;
                        }
                        if (chapter == 4) MoonlitCampaignPlaytest.Capture(game, "route-ending");
                        Debug.Log("CAMPAIGN_ROUTE cleared " + route.Status());
                        summary += $" L{chapter}({game.Elapsed:F1}s,motes={game.Motes},cp={game.CheckpointIndex},falls={game.Deaths},jumps={player.Jumps},air={player.AirJumps},climb={player.ClimbDistance:F1},dash={player.Dashes})";
                        chapters++;
                        break;
                    }
                    yield return null;
                }
                player.TestMove = 0;
                if (chapters != chapter)
                {
                    done(false, "Route timeout " + route.Status()); yield break;
                }
                if (chapter < 4) game.NextChapter();
                while (game.IsLoadingChapter) yield return null;
                yield return null;
            }
            done(true, summary.TrimStart());
        }

        public static IEnumerator CheckCrossings(MoonlitGame game, Action<string,bool,string> check)
        {
            for(int chapter=1;chapter<=4;chapter++) {
                game.StartChapter(chapter,false);
                while (game.IsLoadingChapter) yield return null;
                game.tutorials.Enabled=false;
                var p=game.player; p.TestInput=true;
                Vector2[] starts={new Vector2(315,2.75f),new Vector2(371,6.45f),new Vector2(474,4.15f),new Vector2(320.7f,2.07f)};
                float[] exits={344,407,506,347};
                p.Teleport(starts[chapter-1]); game.cameraRig.Snap();
                yield return new WaitForSeconds(.35f);
                if(chapter==1) {
                    var pit=game.level.GetComponentInChildren<BambooPit>();
                    p.Teleport(pit.transform.position); yield return new WaitForSeconds(.85f);
                    check("bamboo_spikes_respawn",game.Health==2 && game.State==GameState.Playing && p.Body.position.x<10,"hearts="+game.Health);
                    p.Teleport(starts[0]); game.cameraRig.Snap();
                }
                var level=game.level;
                if(level.Weight) check("weight_starts_locked_"+chapter,!level.Weight.Activated && !level.Weight.bridgeCollider.enabled,"new entry");
                if(level.ChimeGate) check("chime_gate_starts_closed",level.ChimeGate.Struck==0 && !level.ChimeGate.Open,"new entry");
                if(level.Ferry) {
                    p.Pulse(); check("sluice_needs_crew",level.Ferry.State==SluiceFerry.LockState.AtLeft,"shore pulse");
                }
                var driver=new Driver(game);
                int previousDeaths=game.Deaths;
                float began=Time.time, lastLog=began, sailingSince=-1;
                int outboundFrames=0, unstableFrames=0;
                float minSeat=float.PositiveInfinity, maxSeat=float.NegativeInfinity, outboundGap=0;
                while(Time.time-began<(chapter == 2 ? 52 : 24) && (p.Body.position.x<exits[chapter-1] || !p.Grounded) && game.State==GameState.Playing) {
                    driver.Advance();
                    if (level.Ferry && level.Ferry.State == SluiceFerry.LockState.GoingRight)
                    {
                        if (sailingSince < 0) sailingSince=Time.time;
                        if (Time.time-sailingSince > .26f)
                        {
                            var local = p.transform.position - level.Ferry.Float.transform.position;
                            minSeat=Mathf.Min(minSeat,local.x); maxSeat=Mathf.Max(maxSeat,local.x);
                            outboundGap=Mathf.Max(outboundGap,Mathf.Abs(local.y)); outboundFrames++;
                            if (!p.Grounded || p.AnimationRow==1) unstableFrames++;
                        }
                    }
                    if(Time.time-lastLog>3){lastLog=Time.time; Debug.Log("CROSSING "+driver.Status());}
                    yield return null;
                }
                p.TestMove=0;
                bool solved=(!level.Weight || level.Weight.Open) && (!level.ChimeGate || level.ChimeGate.Open) &&
                    (!level.Ferry || level.Ferry.State==SluiceFerry.LockState.AtRight);
                check("crossing_"+chapter,solved && p.Body.position.x>=exits[chapter-1] && game.Deaths==previousDeaths,driver.Status());
                if (chapter == 2) check("gate_first_then_backtrack_for_both_chimes", driver.GateStep == 10 && level.ChimeGate.SeenLocked, "route step="+driver.GateStep);
                if (chapter == 3 && level.Ferry.State == SluiceFerry.LockState.AtRight)
                {
                    check("outbound_ferry_rider_stable",outboundFrames>9 && unstableFrames==0 && maxSeat-minSeat<.10f && outboundGap<.08f,
                        $"samples={outboundFrames} unstable={unstableFrames} horizontal drift={maxSeat-minSeat:F3}u vertical gap={outboundGap:F3}u");
                    p.TestMove=-1;
                    float beganBoarding=Time.time;
                    while ((p.Body.position.x>502.2f || !level.Ferry.Boarded(p.Body.position)) && Time.time-beganBoarding<3) yield return null;
                    p.TestMove=0; yield return new WaitForSeconds(.25f); game.Interact();
                    int samples=0, airborne=0, running=0; float largestGap=0; minSeat=float.PositiveInfinity; maxSeat=float.NegativeInfinity;
                    while(level.Ferry.State==SluiceFerry.LockState.GoingLeft && samples<900)
                    {
                        yield return null; samples++;
                        if(!p.Grounded) airborne++;
                        if(p.AnimationRow==1) running++;
                        var local = p.transform.position - level.Ferry.Float.transform.position;
                        largestGap=Mathf.Max(largestGap,Mathf.Abs(local.y)); minSeat=Mathf.Min(minSeat,local.x); maxSeat=Mathf.Max(maxSeat,local.x);
                    }
                    check("return_ferry_rider_stable",level.Ferry.State==SluiceFerry.LockState.AtLeft && samples>9 && airborne==0 && running==0 && largestGap<.08f && maxSeat-minSeat<.10f,
                        $"samples={samples} airborne={airborne} running={running} max visual gap={largestGap:F3}u horizontal drift={maxSeat-minSeat:F3}u");
                }
                if (chapter < 4) game.Complete();
            }
        }

        sealed class Driver
        {
            readonly MoonlitGame game;
            readonly CuoiController p;
            readonly BoxCollider2D[] route;
            int current = -1, target = -1;
            bool secondJump, dash, capturedGuardian, crossingCaptured;
            float lastJump = -20, gap, rise;

            public Driver(MoonlitGame owner)
            {
                game = owner; p = owner.player;
                route = game.level.GetComponentsInChildren<BoxCollider2D>(true)
                    .Where(s => !s.isTrigger && s.gameObject.layer == 8 && s.size.y < 20 && !(s.TryGetComponent<PlatformSurface>(out var skin) && skin.overhead) && !s.GetComponent<PushableCrate>() && !s.GetComponent<BambooChimeGate>() && !s.GetComponentInParent<BambooPit>())
                    .OrderBy(s => s.transform.position.x).ToArray();
            }

            public string Status() => $"L{game.Chapter} {game.State} pos={p.Body.position:F2} vel={p.Body.linearVelocity:F2} hp={game.Health} cp={game.CheckpointIndex} support={current} target={target} elapsed={game.Elapsed:F1} ground={p.Grounded} crate={game.level.crate?.Body.position}";

            public void ResetFlight() { target = -1; current = -1; }

            public int GateStep { get; private set; }
            static readonly Vector2[] ForestTour = {
                new(404.4f,5.35f), new(387.8f,5.35f), new(387.8f,8.05f), new(383.6f,10.65f),
                new(380.2f,12.25f), new(376.7f,14.25f), new(380.2f,12.25f), new(383.6f,10.65f),
                new(386.9f,12.85f), new(392.1f,14.15f)
            };

            bool VisitCanopy(Vector2 feet)
            {
                var gate = game.level.ChimeGate;
                if (!gate || feet.x < 369 || feet.x > 408) return false;
                if (GateStep >= ForestTour.Length)
                {
                    p.TestMove = 1; return true;
                }
                var goal = ForestTour[GateStep];
                float dx = goal.x - feet.x, dy = goal.y - feet.y;
                p.TestMove = Mathf.Clamp(dx*4,-1,1);
                if (Mathf.Abs(dx)<.34f && Mathf.Abs(dy)<.17f && p.Grounded)
                {
                    if (GateStep == 0 && !gate.SeenLocked || GateStep == 5 && !gate.WestLit ||
                        GateStep == 9 && !gate.Open) return true;
                    if (GateStep == 0) MoonlitCampaignPlaytest.Capture(game,"forest-locked-gate");
                    if (GateStep == 5) MoonlitCampaignPlaytest.Capture(game,"forest-west-chime");
                    if (GateStep == 9) MoonlitCampaignPlaytest.Capture(game,"forest-east-chime");
                    GateStep++; secondJump=false; return true;
                }
                if (GateStep < 2) return true;
                if (p.Grounded && dy > .3f && Time.time-lastJump>.16f) { Jump(); secondJump=false; }
                else if (!p.Grounded && !secondJump && dy > -.5f && p.Body.linearVelocity.y < 1.2f && Time.time-lastJump>.22f)
                { p.QueueJump(); secondJump=true; }
                return true;
            }

            public void Advance()
            {
                Vector2 feet = p.Body.position;
                if (game.level.lanternSwitch && game.level.lanternSwitch.Near) game.Interact();
                var procession = game.level.Procession;
                if (procession && !procession.Complete && Vector2.Distance(feet + Vector2.up * .9f, procession.NextLight) < 3.5f) p.Pulse();
                foreach (var enemy in game.enemies)
                    if (!enemy.IsStunned && Vector2.Distance(feet + Vector2.up * .9f, (Vector2)enemy.transform.position + Vector2.up * .4f) < 4.35f)
                    { p.Pulse(); break; }
                foreach (var animal in game.level.Wildlife)
                {
                    if ((animal.Warning || animal.Phase == WetlandPhase.Attacking) && Vector2.Distance(feet + Vector2.up * .9f, animal.BodyCenter) < 4.7f)
                    { p.Pulse(); break; }
                }

                if (VisitCanopy(feet) || PlayCrossing(feet)) return;

                // Push the box against the actual wall, jump onto it, then jump to the top.
                if (game.Chapter == 1 && feet.x > 114.7f && feet.x < 124.9f)
                {
                    var box = game.level.crate;
                    ResetFlight(); p.TestMove = 1;
                    if (box.Body.position.x >= 122.95f && feet.y < 3.25f)
                    {
                        if (p.Grounded && Time.time - lastJump > .20f) Jump();
                        if (feet.y < 1.22f && feet.x > 123.3f) p.TestMove = 0;
                    }
                    return;
                }

                if (p.Grounded && Time.time - lastJump > .14f)
                {
                    current = Support(feet);
                    target = -1;
                    if (current < 0 || current == route.Length - 1)
                    {
                        p.TestMove = feet.x < game.level.EndX ? 1 : 0; return;
                    }
                    var now = route[current].bounds;
                    var next = route[current + 1];
                    // The shrine is on the current bank and is activated before this point.
                    if (!next.enabled || !next.gameObject.activeInHierarchy) { p.TestMove = feet.x < now.max.x - 1 ? 1 : 0; return; }
                    var landing = next.bounds;
                    gap = landing.min.x - now.max.x; rise = landing.max.y - now.max.y;
                    p.TestMove = 1;
                    if (gap < .12f && rise < .18f) return;
                    // Leave a running approach for gaps. Solid step risers can be jumped near their face.
                    if (feet.x < now.max.x - .60f) return;
                    target = current + 1; secondJump = dash = false;
                    Jump(); return;
                }

                if (target < 0) { p.TestMove = 1; return; }
                var dest = route[target].bounds;
                float aim = Mathf.Min(dest.center.x, dest.min.x + 1.5f);
                p.TestMove = feet.x < aim - .18f ? 1 : feet.x > aim + .35f ? -1 : 0;
                bool tallWall = game.Chapter >= 3 && rise > 5.5f && gap < .4f;
                if (tallWall && feet.y < dest.max.y + .15f) p.TestMove = 1;
                if (game.Chapter < 2 || tallWall || Time.time - lastJump < .20f || p.IsDashing) return;
                bool needsLift = feet.x < dest.min.x - .65f || (rise > 2.55f && feet.y < dest.max.y);
                if (!secondJump && Time.time - lastJump < .65f && p.Body.linearVelocity.y <= 1.25f && p.Body.linearVelocity.y > -3.5f && needsLift)
                {
                    p.QueueJump(); secondJump = true;
                }
                else if (game.Chapter == 4 && gap > 8 && secondJump && !dash && p.Body.linearVelocity.y < 2.2f)
                {
                    dash = p.Dash();
                }
            }

            bool PlayCrossing(Vector2 feet)
            {
                var level = game.level;
                if(game.Chapter==1 && level.Weight.Open && feet.x>329 && feet.x<340) CaptureCrossing("village-counterweight");
                var weight = level.Weight;
                if (weight && feet.x > weight.plateCenter.x-10 && feet.x < weight.plateCenter.x+.98f) {
                    ResetFlight(); p.TestMove=1;
                    if (weight.Activated && p.Grounded && feet.y < weight.floorY+.5f && Time.time-lastJump>.28f) Jump();
                    if (weight.Open && game.Chapter==1) CaptureCrossing("village-counterweight");
                    return true;
                }
                if (game.Chapter == 4 && weight && weight.Open && feet.x > 333.4f && feet.x < 342)
                    CaptureCrossing("hill-drawbridge");
                var ferry=level.Ferry;
                if(ferry && feet.x>473 && feet.x<505.3f) {
                    ResetFlight();
                    switch(ferry.State) {
                        case SluiceFerry.LockState.AtLeft:
                            p.TestMove=feet.x<ferry.Float.PhysicsPosition.x-.20f?1:0;
                            if(p.TestMove==0 && ferry.Boarded(feet)) game.Interact();
                            break;
                        case SluiceFerry.LockState.GoingRight:
                            p.TestMove=0;
                            if(ferry.Float.PhysicsPosition.x>487) CaptureCrossing("river-sluice");
                            break;
                        default: p.TestMove=1; break;
                    }
                    return true;
                }
                return false;
            }
            void CaptureCrossing(string name)
            {
                if(crossingCaptured)return;
                crossingCaptured=true; MoonlitCampaignPlaytest.Capture(game,name);
            }

            int Support(Vector2 pos)
            {
                // A capsule can stand slightly beyond the rectangle. Prefer the new surface at seams.
                for (int i = route.Length - 1; i >= 0; i--)
                {
                    var surface = route[i];
                    if (!surface.enabled || !surface.gameObject.activeInHierarchy) continue;
                    var bounds = surface.bounds;
                    if (Mathf.Abs(pos.y - bounds.max.y) < .27f && pos.x >= bounds.min.x - .29f && pos.x <= bounds.max.x + .29f) return i;
                }
                return -1;
            }

            void Jump() { p.QueueJump(); lastJump = Time.time; }

            public void PlayGuardian()
            {
                var boss = game.Boss;
                var feet = p.Body.position;
                var bud = boss.TargetPosition;
                float dx = bud.x - feet.x;
                float aim = bud.x;
                if (boss.Warning || boss.Attacking)
                {
                    switch (boss.Attack)
                    {
                        case BanyanGuardian.Move.Charge:
                            float approach = (feet.x - boss.ContactX) * boss.Direction;
                            if (boss.Warning) aim = boss.BarkX + boss.Direction * 7.4f;
                            else if (approach > -.8f && approach < 5.4f)
                            {
                                aim = feet.x - boss.Direction * 3;
                                if (approach < 2.65f && !p.IsDashing) p.Dash();
                            }
                            break;
                        case BanyanGuardian.Move.LowSweep:
                            if (boss.Warning && boss.Progress > .86f && p.Grounded &&
                                Mathf.Abs(feet.x - boss.BarkX) < 9 && Time.time - lastJump > .6f) Jump();
                            break;
                        case BanyanGuardian.Move.Slam:
                            float away = boss.MarkX < game.level.BossFoot.x - 8 ? 1 : -1;
                            aim = boss.MarkX + away * 5;
                            float distance = Mathf.Abs(feet.x - boss.MarkX) - boss.ShockDistance;
                            if (boss.Attacking && boss.Progress > .40f && distance < 3.4f && distance > -.12f &&
                                p.Grounded && Time.time - lastJump > .5f) Jump();
                            break;
                    }
                }
                float steer = aim - feet.x;
                p.TestMove = Mathf.Abs(steer) > .32f ? Mathf.Sign(steer) : 0;
                if (!capturedGuardian && boss.TargetOpen && boss.CalmedRoots >= 1)
                {
                    MoonlitCampaignPlaytest.Capture(game, "route-guardian");
                    capturedGuardian = true;
                }
                if (!boss.TargetOpen || boss.Progress < .105f || p.Cooldown > 0) return;
                if (Vector2.Distance(feet + Vector2.up * .9f, bud) < game.tuning.pulseRadius - .16f) p.Pulse();
                else if (Mathf.Abs(dx) < 3.0f && p.Grounded && Time.time - lastJump > .28f) Jump();
            }
        }
    }
}
