using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Moonlit
{
    public sealed class MoonlitRevisionPlaytest : MonoBehaviour
    {
        [Serializable] class Check { public string name; public bool passed; public string detail; }
        [Serializable] class Report { public bool passed; public List<Check> checks = new List<Check>(); }
        readonly Report report = new Report();
        MoonlitGame g; string output;
        void CheckResult(string name, bool pass, string detail)
        {
            report.checks.Add(new Check { name = name, passed = pass, detail = detail });
            Debug.Log($"REVISION_QA {name}: {(pass ? "PASS" : "FAIL")} {detail}");
            File.WriteAllText(Path.Combine(output, "revision-results.json"), JsonUtility.ToJson(report, true));
        }
        IEnumerator Start()
        {
            g = MoonlitGame.Instance;
            output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../QA")); Directory.CreateDirectory(output);
            yield return new WaitForSeconds(1);
            g.StartRun(); g.player.TestInput = true;
            yield return new WaitForSecondsRealtime(1.4f);
            CheckResult("initial_tutorial", g.State == GameState.Tutorial && g.tutorials.CurrentId == 0, "First lesson opens on safe spawn ground");
            float clock = g.Elapsed; Vector2 playerAt = g.player.Body.position, enemyAt = g.enemies[0].transform.position;
            Vector2 platformAt = g.level.moving[0].transform.position;
            g.player.TestMove = 1; int deaths = g.Deaths; g.Hurt();
            yield return new WaitForSecondsRealtime(.7f);
            CheckResult("reading_freezes_gameplay", g.Elapsed == clock && g.player.Body.position == playerAt && (Vector2)g.enemies[0].transform.position == enemyAt && (Vector2)g.level.moving[0].transform.position == platformAt && deaths == g.Deaths,
                "Player, enemy, platform, timer and damage are frozen while reading");
            CheckResult("spotlight_shader", g.darkness.MaskMaterial.GetFloat("_FocusBlend") > .98f && g.tutorials.CanContinue, "Only focused object and instruction card stay visible");
            MoonlitCapture.Save(Path.Combine(output, "revision-tutorial-01.png"));
            // Real Input System Enter closes the lesson; it must not leak a jump.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            var keyboard = InputSystem.AddDevice<Keyboard>("Revision test keyboard"); keyboard.MakeCurrent();
            int jumps = g.player.Jumps; g.player.TestMove = 0;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Enter)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSecondsRealtime(.8f);
            CheckResult("tutorial_keyboard_resume", g.State == GameState.Playing && Time.timeScale == 1 && g.player.Jumps == jumps, "Enter resumes without jumping accidentally");
            InputSystem.RemoveDevice(keyboard);
            g.tutorials.Enabled = false;
            g.player.Teleport(new Vector2(10, .02f)); g.player.TestMove = 0;
            yield return new WaitForSeconds(.5f); g.cameraRig.Snap(); yield return new WaitForSeconds(.5f);
            Sprite idle = g.player.body.sprite; Vector2 resting = g.player.Body.position;
            float maxDrift = 0, minFoot = float.MaxValue, maxFoot = float.MinValue; bool samePose = true;
            float until = Time.time + 2.5f;
            while (Time.time < until)
            {
                samePose &= g.player.body.sprite == idle && g.player.AnimationRow == 0;
                maxDrift = Mathf.Max(maxDrift, Vector2.Distance(resting, g.player.Body.position));
                float y = g.player.body.transform.localPosition.y; minFoot = Mathf.Min(minFoot, y); maxFoot = Mathf.Max(maxFoot, y);
                yield return null;
            }
            CheckResult("idle_is_planted", samePose && maxDrift < .003f && maxFoot - minFoot < .001f, $"samePose={samePose}, physicsDrift={maxDrift:F5}, footDelta={maxFoot-minFoot:F5}");
            CheckResult("feet_inside_moss", minFoot < -.06f && minFoot > -.13f, "Visual feet offset=" + minFoot);
            MoonlitCapture.Save(Path.Combine(output, "revision-grounded-feet.png"));
            g.player.TestMove = 1; yield return new WaitForSeconds(.4f); g.player.TestMove = 0;
            float stoppedFrom = g.player.Body.position.x;
            yield return new WaitForSeconds(.15f);
            CheckResult("release_stops_sliding", Mathf.Abs(g.player.Body.linearVelocity.x) < .02f && g.player.Body.position.x - stoppedFrom < .26f, "Stop distance=" + (g.player.Body.position.x - stoppedFrom));
            g.player.Teleport(new Vector2(4, .02f)); g.player.TestMove = -1; deaths = g.Deaths; int health = g.Health;
            float leftMin = 4; until = Time.time + 2;
            while (Time.time < until) { leftMin = Mathf.Min(leftMin, g.player.Body.position.x); yield return null; }
            CheckResult("left_edge_blocks_without_death", g.Deaths == deaths && g.Health == health && leftMin >= MoonlitLevel.LeftPlayerLimit - .005f && g.player.Body.position.x < 1.7f,
                $"x={g.player.Body.position.x:F2}, minX={leftMin:F3}, deaths={g.Deaths-deaths}, health={g.Health}");
            g.player.TestMove = 0; g.cameraRig.Snap(); yield return null;
            MoonlitCapture.Save(Path.Combine(output, "revision-left-boundary.png"));
            CheckResult("coherent_lifts_no_crate", g.level.moving.Count == 2 && FindObjectsByType<LiftSuspension>(FindObjectsSortMode.None).Length == 2 && !FindFirstObjectByType<PushableCrate>(), "Two suspended wooden lifts; crate removed");

            float[] positions = { 4, 29.2f, 74.5f, 102.5f, 111.5f, 169.5f };
            float[] floors = { 0, .5f, .2f, 1, 1, .5f };
            for (int id = 1; id < MoonlitTutorial.Total; id++)
            {
                g.player.Teleport(new Vector2(positions[id], floors[id] + .02f)); g.player.Invulnerable = 4;
                yield return new WaitForSeconds(.15f); g.cameraRig.Snap();
                bool opened = g.tutorials.Open(id);
                g.tutorials.Continue(); // Guard should reject this same-frame continuation.
                yield return new WaitForSecondsRealtime(.2f);
                bool guarded = g.State == GameState.Tutorial;
                yield return new WaitForSecondsRealtime(.95f);
                var camera = g.cameraRig.GetComponent<Camera>();
                Vector3 focusScreen = camera.WorldToViewportPoint(g.tutorials.FocusPosition);
                CheckResult("tutorial_focus_" + id, opened && guarded && g.tutorials.CanContinue && focusScreen.x > .20f && focusScreen.x < .46f && focusScreen.y > .20f && focusScreen.y < .65f,
                    $"safeConfirm={guarded}, targetViewport={focusScreen}");
                MoonlitCapture.Save(Path.Combine(output, "revision-tutorial-" + (id + 1).ToString("00") + ".png"));
                g.tutorials.Continue(); yield return new WaitForSecondsRealtime(.7f);
                CheckResult("tutorial_resume_" + id, g.State == GameState.Playing && Time.timeScale == 1, "Gameplay and camera return after fade");
            }
            int seen = g.tutorials.SeenCount;
            g.Hurt(true); yield return new WaitForSeconds(.9f);
            CheckResult("death_keeps_lessons_seen", g.tutorials.SeenCount == seen, "Lessons do not repeat after a death");
            g.tutorials.Open(1); yield return new WaitForSecondsRealtime(.2f); g.Home(); yield return null;
            CheckResult("home_cancels_modal", g.State == GameState.Home && !g.tutorials.Active && Time.timeScale == 1, "No frozen state after leaving a tutorial");
            g.StartRun(); yield return new WaitForSeconds(.1f);
            CheckResult("restart_resets_lessons", g.tutorials.SeenCount == 0, "A fresh run can teach again");
            report.passed = report.checks.TrueForAll(c => c.passed);
            File.WriteAllText(Path.Combine(output, "revision-results.json"), JsonUtility.ToJson(report, true));
            Debug.Log("REVISION_QA_DONE " + report.passed); Application.Quit(report.passed ? 0 : 1);
        }
    }
}
