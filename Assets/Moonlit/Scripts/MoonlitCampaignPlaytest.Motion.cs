using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        void CheckHeartImages(string state, int filled)
        {
            Canvas.ForceUpdateCanvases();
            bool visible = game.hud.HeartDisplayVisible && game.hud.VisibleHeartCount == filled && game.Health == filled;
            float previousEnd = -1;
            float smallest = float.PositiveInfinity;
            for (int i = 0; i < 3; i++)
            {
                Rect rect = game.hud.HeartScreenRect(i);
                visible &= rect.xMin >= previousEnd - 1 && rect.xMin >= 0 && rect.yMin >= 0 && rect.xMax <= Screen.width + 1 && rect.yMax <= Screen.height + 1;
                smallest = Mathf.Min(smallest, rect.width, rect.height);
                previousEnd = rect.xMax;
            }
            Record("heart_images_" + state, visible && smallest > 8, $"filled={game.hud.VisibleHeartCount}/{filled}, three slots ≥{smallest:F0}px, viewport={Screen.width}×{Screen.height}");
        }

        IEnumerator SetUpMotion(int chapter, Vector2 start, string folder)
        {
            game.StartChapter(chapter,false);
            while (game.IsLoadingChapter) yield return null;
            game.player.TestInput = false; game.player.TestMove = 0;
            keyboard.MakeCurrent(); InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            game.player.Teleport(start); game.cameraRig.Snap();
            if (folder != null) Directory.CreateDirectory(folder);
            yield return new WaitForSeconds(2.35f);
            Time.captureDeltaTime = 1f / 30;
        }

        void MotionFrame(string folder, int frame)
        {
            if (folder != null) MoonlitCapture.Save(Path.Combine(folder, frame.ToString("D4") + ".png"), 960, 540);
        }

        void EndMotion(string folder, List<string> marks)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            Time.captureDeltaTime = 0;
            if (folder != null) File.WriteAllLines(Path.Combine(folder, "motion.csv"), marks);
        }

        IEnumerator ObserveRun(string folder)
        {
            yield return SetUpMotion(1, new Vector2(177, .04f), folder);
            var p = game.player;
            var poses = new HashSet<Sprite>();
            var idlePoses = new HashSet<Sprite>();
            Vector2 idleStart = p.Body.position;
            bool solesStayed = true, checkedLantern = false;
            for (int i = 0; i < 228; i++)
            {
                yield return null;
                idlePoses.Add(p.body.sprite);
                if (i == 5) Capture(game, "motion-idle-breath");
                if (!checkedLantern && p.body.sprite == game.art.Hero(3, 2))
                {
                    checkedLantern = true;
                    Capture(game, "motion-idle-lantern");
                }
                solesStayed &= p.AnimationRow == 0 && p.Grounded && Vector2.Distance(p.Body.position, idleStart) < .025f;
            }
            Record("idle_breathes_on_planted_feet", idlePoses.Count == 7 && solesStayed && checkedLantern, $"{idlePoses.Count} standing poses, stable support={solesStayed}, raised lantern={checkedLantern}");
            var marks = new List<string> { "frame,event" };
            float previousX = p.Body.position.x, previousCycle = p.MotionCycle, right = 0, left = 0, cycleTravel = 0, stoppedTravel = 0, stoppedCycles = 0;
            bool floorContact = true, facing = true, photographed = false;
            int movingFrames = 0, toeSamples = 0;
            float toeGap = 0, stanceX = 0, toeSlide = 0;
            int stance = -1;
            for (int i = 0; i < 150; i++)
            {
                if (i == 10) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); marks.Add(i + ",run_right"); }
                if (i == 64 || i == 139) { InputSystem.QueueStateEvent(keyboard, new KeyboardState()); marks.Add(i + ",stop"); }
                if (i == 88) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A)); marks.Add(i + ",run_left"); }
                yield return null;
                float dx = p.Body.position.x - previousX;
                float phaseDelta = Mathf.Abs(Mathf.DeltaAngle(previousCycle * 360, p.MotionCycle * 360)) / 360;
                bool runningRight = i >= 20 && i < 63, runningLeft = i >= 99 && i < 138;
                if (runningRight || runningLeft)
                {
                    if (runningRight) right += dx; else left -= dx;
                    movingFrames++; cycleTravel += phaseDelta; poses.Add(p.body.sprite);
                    var hit = Physics2D.Raycast(p.Body.position + Vector2.up * .21f, Vector2.down, .45f, 1 << 8);
                    floorContact &= p.Grounded && hit.collider && Mathf.Abs(p.Body.position.y - hit.point.y) < .14f && p.AnimationRow == 1;
                    facing &= p.FacingRight == runningRight;
                    if (p.HasPlantedFoot)
                    {
                        var toe = p.PlantPosition;
                        int foot = p.AnimationFrame / 4 + (runningRight ? 0 : 2);
                        if (foot != stance) { stanceX = toe.x; stance = foot; }
                        toeSlide = Mathf.Max(toeSlide, Mathf.Abs(toe.x - stanceX));
                        var belowToe = Physics2D.Raycast(toe + Vector2.up * .36f, Vector2.down, .8f, 1 << 8);
                        if (belowToe.collider) { toeSamples++; toeGap = Mathf.Max(toeGap, Mathf.Abs(toe.y - belowToe.point.y)); }
                        else floorContact = false;
                        if (!photographed && i >= 32) { Capture(game, "motion-run"); photographed = true; }
                    }
                    else stance = -1;
                }
                if (i >= 77 && i < 86) { stoppedTravel += Mathf.Abs(dx); stoppedCycles += phaseDelta; }
                previousX = p.Body.position.x; previousCycle = p.MotionCycle;
                MotionFrame(folder, i);
            }
            EndMotion(folder, marks);
            Record("run_pose_and_reversal", right > 6 && left > 5.5f && poses.Count >= 4 && cycleTravel > 2 && floorContact && facing && photographed,
                $"right={right:F2}u, left={left:F2}u, {poses.Count} rendered poses, {cycleTravel:F2} strides, {movingFrames} grounded samples");
            Record("run_stops_with_feet", stoppedTravel < .02f && stoppedCycles < .025f, $"stopped drift={stoppedTravel:F3}u, phase drift={stoppedCycles:F3}");
            Record("run_planted_sole", toeSamples >= 20 && toeGap < .18f && toeSlide < .22f, $"{toeSamples} soles on terrain; max vertical gap={toeGap:F3}u, horizontal slide during stance={toeSlide:F3}u");
        }

        IEnumerator ObserveClimb(string folder)
        {
            yield return SetUpMotion(3, new Vector2(19.8f, .04f), folder);
            var p = game.player;
            var poses = new HashSet<Sprite>();
            var marks = new List<string> { "frame,event" };
            bool jumped = false, released = false, resumed = false, landed = false, photographed = false, wallMatched = true;
            var climbingLine = System.Array.Find(game.level.GetComponentsInChildren<SpriteRenderer>(), sr => sr.name == "Braided Wall Cord" && Mathf.Abs(sr.bounds.center.x - 24) < .1f);
            int holdFrame = -1, stableSamples = 0, wallSamples = 0, gripSamples = 0, crestFrame = -1;
            int missedWall = 0, missedGrip = 0, missedCord = 0, intactFrames = 0;
            bool rigidArt = true;
            int mantleMask = 0, crestContacts = 0;
            Vector2 crestSole = Vector2.zero;
            bool plantedOnCrest = false, capturedMantle = false;
            float gripGap = 0, gripSlide = 0, gripY = 0;
            int lastGrip = -1, beltSamples = 0, previousWallFrame = -1;
            Vector2 previousLantern = Vector2.zero;
            float beltStep = 0;
            float heldMin = float.PositiveInfinity, heldMax = float.NegativeInfinity, heldPhase = 0, previousCycle = p.MotionCycle;
            float climbedMin = float.PositiveInfinity, climbedMax = float.NegativeInfinity;
            for (int i = 0; i < 150; i++)
            {
                if (i == 8) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); marks.Add(i + ",approach"); }
                if (!jumped && p.Body.position.x >= 22.85f) { jumped = true; InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space)); marks.Add(i + ",jump"); }
                if (!released && p.IsClimbing && p.Body.position.y >= 2.35f) { released = true; InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); marks.Add(i + ",hold"); }
                if (holdFrame >= 0 && !resumed && i - holdFrame >= 22) { resumed = true; InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space)); marks.Add(i + ",resume"); }
                if (!landed && resumed && p.Grounded && p.Body.position.y >= 6.3f) { landed = true; crestFrame = i; marks.Add(i + ",crest"); }
                if (landed && i == crestFrame + 12) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                if (p.AnimationRow == 4 || p.AnimationRow == 5)
                {
                    intactFrames++;
                    var material = p.body.sharedMaterial;
                    rigidArt &= p.body.transform.localScale == Vector3.one &&
                        material.GetVector("_BendA").sqrMagnitude < .00001f && material.GetVector("_BendB").sqrMagnitude < .00001f;
                }
                if ((p.AnimationRow == 4 || p.AnimationRow == 5) && p.AnimationFrame >= 8 && p.AnimationFrame <= 11)
                {
                    mantleMask |= 1 << (p.AnimationFrame - 8);
                    if (p.AnimationRow == 4 && p.AnimationFrame == 10 && !capturedMantle) { Capture(game, "motion-mantle"); capturedMantle = true; }
                    if (p.AnimationRow == 5 && p.Grounded && p.HasPlantedFoot)
                    {
                        var foot = p.PlantPosition;
                        crestSole = foot; crestContacts++;
                        var top = Physics2D.Raycast(foot + Vector2.up * .3f, Vector2.down, .7f, 1 << 8);
                        plantedOnCrest |= top.collider && top.normal.y > .85f && Mathf.Abs(foot.y - top.point.y) < .18f;
                    }
                }
                if (p.IsClimbing && !p.IsWallHolding && p.AnimationRow == 4)
                {
                    poses.Add(p.body.sprite);
                    climbedMin = Mathf.Min(climbedMin,p.Body.position.y); climbedMax = Mathf.Max(climbedMax,p.Body.position.y);
                    // The controller raycasts before physics integrates its upward step.
                    // Sample the vertical face below the lip, then separately verify the crest landing.
                    if (p.Body.position.y < 6.24f)
                    {
                        var wall = Physics2D.Raycast(p.Body.position + Vector2.up * .5f, Vector2.right, .55f, 1 << 8);
                        if (!wall.collider) wall = Physics2D.Raycast(p.Body.position + Vector2.up * .05f, Vector2.right, .55f, 1 << 8);
                        if (!wall.collider || wall.normal.x > -.7f || Mathf.Abs(wall.point.x - p.WallContact.x) >= .13f) missedWall++;
                        wallSamples++;
                    }
                    var grip = p.GripPosition;
                    if (grip.y < 6.35f)
                    {
                        int hand = p.AnimationFrame / 4;
                        if (hand != lastGrip) { lastGrip = hand; gripY = grip.y; }
                        gripSlide = Mathf.Max(gripSlide, Mathf.Abs(grip.y - gripY));
                        var face = Physics2D.Raycast(grip - Vector2.right * .4f, Vector2.right, .85f, 1 << 8);
                        if (face.collider && face.normal.x < -.7f) { gripSamples++; gripGap = Mathf.Max(gripGap, Mathf.Abs(grip.x - face.point.x)); }
                        else missedGrip++;
                        if (!climbingLine || Mathf.Abs(climbingLine.bounds.center.x - grip.x) >= .13f || grip.y < climbingLine.bounds.min.y || grip.y > climbingLine.bounds.max.y) missedCord++;
                    }
                    if (!photographed && resumed && p.Body.position.y > 3.45f) { Capture(game, "motion-climb"); photographed = true; }
                }
                if (!resumed && p.IsWallHolding)
                {
                    if (holdFrame < 0) holdFrame = i;
                    if (i > holdFrame + 3)
                    {
                        heldMin = Mathf.Min(heldMin, p.Body.position.y); heldMax = Mathf.Max(heldMax, p.Body.position.y);
                        heldPhase += Mathf.Abs(Mathf.DeltaAngle(previousCycle * 360, p.MotionCycle * 360)) / 360;
                        stableSamples++;
                    }
                }
                bool stroking = p.AnimationRow == 4 && p.AnimationFrame < 8 && p.IsClimbing && !p.IsWallHolding;
                if (stroking)
                {
                    Vector2 relativeLantern = p.LanternPosition - (Vector2)p.transform.position;
                    if (previousWallFrame >= 0 && previousWallFrame != p.AnimationFrame)
                    {
                        beltSamples++;
                        beltStep = Mathf.Max(beltStep,Vector2.Distance(relativeLantern,previousLantern));
                    }
                    previousLantern = relativeLantern;
                }
                previousWallFrame = stroking ? p.AnimationFrame : -1;
                previousCycle = p.MotionCycle;
                MotionFrame(folder, i);
            }
            EndMotion(folder, marks);
            float ascent = climbedMax - climbedMin, hangDrift = heldMax - heldMin;
            wallMatched = missedWall == 0 && missedGrip == 0 && missedCord == 0;
            Record("climb_preserves_painted_silhouette", intactFrames >= 20 && rigidArt, $"{intactFrames} unscaled climb, hang and mantle samples; zero UV bending={rigidArt}");
            Record("climb_contact_and_crest", landed && photographed && wallMatched && wallSamples >= 10 && ascent > 3 && poses.Count >= 3,
                $"{wallSamples} wall samples, {poses.Count} climbing sprites, ascent={ascent:F2}u, crested={landed}, missed wall/grip/cord={missedWall}/{missedGrip}/{missedCord}");
            Record("climb_lantern_continuity", beltSamples >= 4 && beltStep < .12f,
                $"{beltSamples} sprite changes, largest relative lantern shift={beltStep:F3}u");
            Record("climb_grip_holds_pose", stableSamples >= 12 && hangDrift < .075f && heldPhase < .04f && resumed,
                $"{stableSamples} held samples, body drift={hangDrift:F3}u, phase drift={heldPhase:F3}, resumed={resumed}");
            Record("climb_hand_on_wall", gripSamples >= 10 && gripGap < .18f && gripSlide < .21f, $"{gripSamples} hands on wall; max horizontal gap={gripGap:F3}u, vertical grip slide={gripSlide:F3}u");
            Record("mantle_over_lip", mantleMask == 15 && plantedOnCrest && landed, $"observed pose mask={mantleMask}/15, {crestContacts} planted pose samples, sole={crestSole:F3}, touched upper bank={plantedOnCrest}");
        }
    }
}
