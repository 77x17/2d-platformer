using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Moonlit
{
    // Opt-in integration checks run only with -moonlitQA. No debug shortcuts in normal gameplay.
    public sealed class MoonlitPlaytest : MonoBehaviour
    {
        [Serializable] public class Check { public string name; public bool passed; public string detail; }
        [Serializable] public class Report { public List<Check> checks = new List<Check>(); public bool passed; public string version; }
        readonly Report report = new Report();
        MoonlitGame g;
        string output;
        Keyboard testKeyboard;
        void CheckResult(string name, bool passed, string detail)
        {
            report.checks.Add(new Check { name = name, passed = passed, detail = detail });
            Debug.Log($"MOONLIT_QA {name}: {(passed ? "PASS" : "FAIL")} {detail}");
            File.WriteAllText(Path.Combine(output, "playtest-results.json"), JsonUtility.ToJson(report, true));
        }
        IEnumerator Start()
        {
            g = MoonlitGame.Instance; output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../QA")); Directory.CreateDirectory(output);
            g.tutorials.Enabled = false; // Isolated physics fixtures; full route exercises actual lessons.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            testKeyboard = InputSystem.AddDevice<Keyboard>("Moonlit QA keyboard"); testKeyboard.MakeCurrent();
            report.version = Application.unityVersion;
            yield return new WaitForSeconds(2);
            MoonlitCapture.Save(Path.Combine(output, "01-home.png"));
            CheckResult("art_and_actors", g.art.cuoi.Length == 16 && g.art.dog.Length == 16 && g.art.cat.Length == 16 && g.enemies.Count == 6, "16 frames per actor, 3 dog + 3 cat encounters");
            yield return Press(Key.Enter);
            CheckResult("keyboard_start", g.State == GameState.Playing, "Enter starts from the home screen through the Input System");
            if (g.State != GameState.Playing) g.StartRun();
            yield return new WaitForSeconds(.5f);
            CheckResult("spawn_grounded", g.player.Grounded, g.player.transform.position.ToString());
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(Key.D));
            yield return new WaitForSeconds(.5f); InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
            yield return null; g.player.TestInput = true;
            CheckResult("run_acceleration", g.player.transform.position.x > 6, "Player x=" + g.player.transform.position.x);
            yield return JumpMeasure(false);
            float shortJump = apex;
            yield return JumpMeasure(true);
            CheckResult("variable_jump_height", apex > shortJump + .6f, $"tap={shortJump:F2}, hold={apex:F2}");
            g.player.Teleport(new Vector2(19.2f, .03f)); g.player.TestMove = 1;
            float end = Time.time + 1;
            while (g.player.transform.position.x < 20.4f && Time.time < end) yield return null;
            g.player.QueueJump(); yield return new WaitForSeconds(.12f);
            CheckResult("coyote_jump", g.player.Body.linearVelocity.y > 1, "Vertical speed=" + g.player.Body.linearVelocity.y);
            g.player.TestMove = 0; g.player.Teleport(new Vector2(10, 1.5f));
            end = Time.time + 1.5f;
            while (g.player.transform.position.y > .18f && Time.time < end) yield return null;
            int jumpsBefore = g.player.Jumps; g.player.QueueJump(); yield return new WaitForSeconds(.2f);
            CheckResult("jump_buffer", g.player.Jumps > jumpsBefore, "Queued just before landing");
            g.enemies[0].ResetCreature();
            g.player.Teleport((Vector2)g.enemies[0].transform.position + new Vector2(-2.3f, .05f)); yield return new WaitForSeconds(.2f);
            bool pulse = g.player.Pulse(); bool rejected = !g.player.Pulse(); yield return new WaitForSeconds(.15f);
            CheckResult("lantern_stun_cooldown", pulse && rejected && g.enemies[0].IsStunned, $"pulse={pulse}, cooldownRejected={rejected}, stunned={g.enemies[0].IsStunned}");
            MoonlitCapture.Save(Path.Combine(output, "02-lantern.png"));
            yield return new WaitForSeconds(2.8f);
            CheckResult("stun_recovers", !g.enemies[0].IsStunned, "Creature resumes after stun duration");
            int deaths = g.Deaths; g.player.Invulnerable = 0;
            g.player.Teleport(g.enemies[0].transform.position); yield return new WaitForSeconds(.9f);
            CheckResult("contact_respawn", g.Deaths > deaths && Vector2.Distance(g.player.transform.position, g.RespawnPoint) < 1, "Contact respawns at the active lamp");
            g.player.Teleport(new Vector2(69, .3f)); yield return new WaitForSeconds(.25f);
            CheckResult("checkpoint", g.CheckpointIndex == 1 && g.Health == 3, "Checkpoint 1 activated and health restored");
            g.player.Teleport(new Vector2(80, -8)); yield return new WaitForSeconds(.9f);
            CheckResult("fall_respawn", Vector2.Distance(g.player.transform.position, g.RespawnPoint) < 1, "Fall returns to checkpoint");
            var platform = g.level.moving[0];
            g.player.Teleport((Vector2)platform.transform.position + Vector2.up * .08f); yield return new WaitForSeconds(.8f);
            CheckResult("moving_platform_carry", Mathf.Abs(g.player.transform.position.y - platform.transform.position.y) < .35f, $"Player y={g.player.transform.position.y:F2}, platform y={platform.transform.position.y:F2}");
            MoonlitCapture.Save(Path.Combine(output, "03-river.png"));
            var plank = g.level.falling[0]; g.player.Teleport((Vector2)plank.transform.position + Vector2.up * .08f);
            yield return new WaitForSeconds(.85f);
            CheckResult("crumbling_platform", plank.CollapseCount > 0, "Standing on old planks triggers collapse");
            g.player.Teleport(new Vector2(137, 3.5f)); yield return new WaitForSeconds(.2f);
            g.player.Teleport(new Vector2(172, .6f)); yield return new WaitForSeconds(.2f); g.Interact();
            CheckResult("environment_interaction", g.level.lanternSwitch.Activated && g.level.lanternSwitch.bridge.activeSelf, "E wakes bridge with colliders");
            yield return Press(Key.Escape); Vector3 paused = g.player.transform.position; yield return new WaitForSecondsRealtime(.3f);
            CheckResult("pause", g.State == GameState.Paused && g.player.transform.position == paused, "Escape pauses through the Input System"); yield return Press(Key.Escape);
            g.player.Teleport(new Vector2(MoonlitLevel.GoalX - 1.3f, MoonlitLevel.GoalFloor + .1f)); g.player.TestMove = 1;
            yield return new WaitForSeconds(.65f); g.player.TestMove = 0;
            CheckResult("moon_completion", g.State == GameState.Complete && g.WallClimbUnlocked, "Moon pickup ends level and unlocks next chapter reward flag");
            yield return new WaitForSeconds(.8f); MoonlitCapture.Save(Path.Combine(output, "04-complete.png"));
            g.StartRun(); yield return new WaitForSeconds(.3f);
            CheckResult("restart", g.Motes == 0 && g.CheckpointIndex == 0 && !g.level.lanternSwitch.Activated && g.State == GameState.Playing, "Full restart resets progression and mechanisms");
            yield return new WaitForSeconds(.2f); MoonlitCapture.Save(Path.Combine(output, "05-gameplay.png"));
            yield return new WaitForSeconds(.5f);
            report.passed = report.checks.TrueForAll(c => c.passed); File.WriteAllText(Path.Combine(output, "playtest-results.json"), JsonUtility.ToJson(report, true));
            InputSystem.RemoveDevice(testKeyboard);
            Debug.Log("MOONLIT_QA_DONE " + report.passed); Application.Quit(report.passed ? 0 : 1);
        }
        float apex;
        IEnumerator Press(Key key)
        {
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(key)); yield return null; yield return null;
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState()); yield return null;
        }
        IEnumerator JumpMeasure(bool hold)
        {
            g.player.TestMove = 0; g.player.Teleport(new Vector2(10, .03f)); yield return new WaitForSeconds(.15f);
            apex = 0; g.player.QueueJump(); float start = Time.time;
            while (Time.time - start < 1)
            {
                if (!hold && Time.time - start > .06f) g.player.ReleaseJump();
                apex = Mathf.Max(apex, g.player.transform.position.y); yield return null;
            }
        }
    }
}
