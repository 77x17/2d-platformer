using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Moonlit
{
    // Opt-in regression fixtures. Exercise real physics/controller updates in the player.
    public sealed class MoonlitPolishPlaytest : MonoBehaviour
    {
        [Serializable] class Check { public string name; public bool passed; public string detail; }
        [Serializable] class Report { public bool passed; public List<Check> checks = new List<Check>(); }
        readonly Report report = new Report();
        MoonlitGame g;
        string output;
        void CheckResult(string name, bool pass, string detail)
        {
            report.checks.Add(new Check { name = name, passed = pass, detail = detail });
            Debug.Log($"POLISH_QA {name}: {(pass ? "PASS" : "FAIL")} {detail}");
            File.WriteAllText(Path.Combine(output, "polish-results.json"), JsonUtility.ToJson(report, true));
        }

        IEnumerator Start()
        {
            g = MoonlitGame.Instance;
            g.tutorials.Enabled = false;
            output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../QA")); Directory.CreateDirectory(output);
            yield return new WaitForSeconds(1);
            g.StartRun(); g.player.TestInput = true;
            yield return new WaitForSeconds(.5f);
            CheckResult("coyote_setting", Mathf.Abs(g.tuning.coyoteTime - .10f) < .001f, "Grace window = " + g.tuning.coyoteTime);
            yield return CoyoteTrial(2, true);
            yield return CoyoteTrial(9, false);
            g.player.Teleport(new Vector2(10, .02f)); g.player.TestMove = 0; yield return new WaitForSeconds(.2f);
            int jumps = g.player.Jumps; g.player.QueueJump(); yield return new WaitForSeconds(.08f); g.player.QueueJump();
            yield return new WaitForSeconds(.12f);
            CheckResult("no_air_double_jump", g.player.Jumps == jumps + 1, "Second press after takeoff must not refresh grace");

            var platform = g.level.moving[0]; platform.ResetPlatform();
            g.player.Teleport((Vector2)platform.transform.position + Vector2.up * .04f);
            yield return new WaitForSeconds(.25f);
            bool ascending = platform.Velocity.y > .1f && g.player.Grounded;
            jumps = g.player.Jumps; g.player.QueueJump();
            yield return new WaitForSeconds(.20f);
            bool airborne = !g.player.Grounded && g.player.AnimationRow == 2 && g.player.Jumps == jumps + 1;
            yield return new WaitForSeconds(1.5f);
            CheckResult("jump_from_ascending_platform", ascending && airborne && g.player.Grounded && g.player.AnimationRow != 2,
                $"ascendingGrounded={ascending}, airborne={airborne}, landed={g.player.Grounded}, row={g.player.AnimationRow}");
            // Two full platform periods catch stale animation and support drift on both directions.
            float until = Time.time + platform.period * 2 + .1f; int unstable = 0; float worstGap = 0;
            while (Time.time < until)
            {
                if (!g.player.Grounded || g.player.AnimationRow == 2) unstable++;
                worstGap = Mathf.Max(worstGap, Mathf.Abs(g.player.Body.position.y - platform.GetComponent<Collider2D>().bounds.max.y));
                yield return new WaitForFixedUpdate();
            }
            CheckResult("ride_two_full_cycles", unstable == 0 && worstGap < .16f, $"unstableFrames={unstable}, largestFeetGap={worstGap:F3}");
            until = Time.time + 4.2f;
            while (platform.Velocity.y > -.3f && Time.time < until) yield return null;
            jumps = g.player.Jumps; g.player.QueueJump(); yield return new WaitForSeconds(.2f);
            bool downJump = g.player.Jumps == jumps + 1 && !g.player.Grounded;
            yield return new WaitForSeconds(1.6f);
            CheckResult("jump_from_descending_platform", downJump && g.player.Grounded && g.player.AnimationRow != 2, "Jump and land on a descending support");
            MoonlitCapture.Save(Path.Combine(output, "polish-moving-platform.png"));

            g.player.Teleport(new Vector2(10, .02f)); yield return new WaitForSeconds(.3f);
            g.cameraRig.Snap(); yield return null;
            float before = g.cameraRig.BackgroundOffset; yield return new WaitForSeconds(.75f);
            CheckResult("background_scrolls_while_idle", Mathf.Abs(before - g.cameraRig.BackgroundOffset) > .05f, "Parallax tile offset changes without player movement");
            g.wind.BeginGust(); yield return new WaitForSeconds(3.4f);
            CheckResult("wind_gust_ramps", g.wind.Strength > .9f && g.wind.GustCount > 0, "Wind strength=" + g.wind.Strength);
            MoonlitCapture.Save(Path.Combine(output, "polish-wind.png"));
            yield return new WaitForSeconds(4);
            CheckResult("wind_gust_settles", g.wind.Strength < .05f, "Gust returns to calm after seven seconds");

            MoonlitCapture.Save(Path.Combine(output, "polish-darkness.png"));
            var shader = g.darkness.MaskMaterial.shader;
            CheckResult("darkness_shader_loaded", shader.isSupported && g.darkness.CurrentRadius > 6, shader.name);
            float radius = g.darkness.CurrentRadius; g.player.Pulse(); yield return null; yield return null;
            CheckResult("pulse_expands_visibility", g.darkness.CurrentRadius > radius + 1.7f, "Normal=" + radius + ", pulse=" + g.darkness.CurrentRadius);
            MoonlitCapture.Save(Path.Combine(output, "polish-light-pulse.png"));

            foreach (var e in new[] { g.enemies[0], g.enemies[1] })
            {
                e.ResetCreature();
                g.player.Teleport(new Vector2(e.transform.position.x + 2.7f, e.floor + .04f)); g.player.Invulnerable = 4;
                until = Time.time + 4;
                while (!e.IsWarning && Time.time < until) yield return null;
                yield return new WaitForSeconds(.1f);
                CheckResult(e.isCat ? "cat_attack_telegraph" : "dog_attack_telegraph", e.IsWarning && e.WarningVisible, "Large exclamation visible during anticipation");
                g.cameraRig.Snap(); yield return null;
                MoonlitCapture.Save(Path.Combine(output, e.isCat ? "polish-cat-warning.png" : "polish-dog-warning.png"));
                e.Stun(1); yield return null;
                CheckResult(e.isCat ? "cat_stun_cancels_warning" : "dog_stun_cancels_warning", !e.WarningVisible, "Stun cancels wind-up and its marker");
                yield return AttackTrial(e);
            }

            CheckResult("crate_mechanic_removed", FindObjectsByType<PushableCrate>(FindObjectsSortMode.None).Length == 0, "No crate or plate in the playable scene");
            g.StartRun(); yield return new WaitForSeconds(.2f);
            CheckResult("restart_resets_wind", g.wind.GustCount == 0, "Restart restores event state");
            report.passed = report.checks.TrueForAll(c => c.passed);
            File.WriteAllText(Path.Combine(output, "polish-results.json"), JsonUtility.ToJson(report, true));
            Debug.Log("POLISH_QA_DONE " + report.passed); Application.Quit(report.passed ? 0 : 1);
        }

        IEnumerator CoyoteTrial(int delayTicks, bool expected)
        {
            g.player.Teleport(new Vector2(19.4f, .025f)); g.player.TestMove = 1;
            yield return new WaitForSeconds(.05f);
            float until = Time.time + 1;
            while (g.player.Grounded && Time.time < until) yield return new WaitForFixedUpdate();
            g.player.TestMove = 0;
            for (int i = 0; i < delayTicks; i++) yield return new WaitForFixedUpdate();
            int before = g.player.Jumps; g.player.QueueJump();
            yield return new WaitForSeconds(.04f);
            bool jumped = g.player.Jumps > before;
            CheckResult(expected ? "coyote_inside_window" : "coyote_expired_window", jumped == expected, $"delayTicks={delayTicks}, accepted={jumped}");
        }

        IEnumerator AttackTrial(ForestCreature enemy)
        {
            enemy.ResetCreature();
            // Near the detection limit, and beyond the patrol's left edge on the same ground.
            float distance = enemy.isCat ? 5.2f : 4.5f;
            g.player.Teleport(new Vector2(enemy.transform.position.x - distance, enemy.floor + .04f));
            g.player.Invulnerable = 0;
            int deaths = g.Deaths;
            float start = Time.time;
            while (!enemy.WarningVisible && Time.time < start + .15f) yield return null;
            bool warnedOnEntry = enemy.WarningVisible;
            float hitDeadline = start + (enemy.isCat ? 1.28f : 1.12f);
            while (g.Deaths == deaths && Time.time < hitDeadline) yield return null;
            bool hit = g.Deaths == deaths + 1;
            CheckResult(enemy.isCat ? "cat_pounce_reaches_player" : "dog_lunge_reaches_player",
                warnedOnEntry && hit, $"Immediate warning={warnedOnEntry}, hit={hit}, elapsed={Time.time - start:F2}s");
            while (g.State == GameState.Respawning) yield return null;
        }
    }
}
