using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Moonlit
{
    // Standalone-only command-line fixture. The route driver uses the same controller and collision world as a player.
    public sealed partial class MoonlitCampaignPlaytest : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, detail; public bool passed; }
        [Serializable] sealed class Report { public string build; public bool passed; public List<Check> checks = new List<Check>(); }
        static string directory;
        readonly Report report = new Report();
        MoonlitGame game;
        Keyboard keyboard;
        bool done;
        float deadline;

        public static void Capture(MoonlitGame game,string name)
        {
            MoonlitCapture.Save(Path.Combine(directory,name + ".png"));
        }
        void Record(string name,bool ok,string detail)
        {
            report.checks.Add(new Check { name = name, detail = detail, passed = ok });
            Debug.Log($"CAMPAIGN_QA {name} {(ok ? "PASS" : "FAIL")} {detail}");
            WriteReport();
        }
        void WriteReport() => File.WriteAllText(Path.Combine(directory,"results.json"), JsonUtility.ToJson(report,true));
        void Logged(string condition,string stack,LogType type)
        {
            if (done || (type != LogType.Exception && type != LogType.Error && type != LogType.Assert)) return;
            done = true;
            Record("runtime",false,condition + "\n" + stack);
            Application.Quit(1);
        }
        void Update()
        {
            if (!done && deadline > 0 && Time.realtimeSinceStartup > deadline)
            {
                done = true; Record("watchdog",false,"Test timed out in " + game.State + " chapter " + game.Chapter);
                Application.Quit(1);
            }
        }
        IEnumerator Press(Key key)
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            yield return null;
        }
        IEnumerator Start()
        {
            game = MoonlitGame.Instance;
            var args = Environment.GetCommandLineArgs();
            bool guardianOnly = Array.IndexOf(args, "-moonlitGuardianQA") >= 0;
            bool motionOnly = Array.IndexOf(args, "-moonlitMotionQA") >= 0;
            directory = Path.GetFullPath(Path.Combine(Application.dataPath, guardianOnly ? "../../../QA/Guardian" : motionOnly ? "../../../QA/Motion" : "../../../QA/Campaign"));
            Directory.CreateDirectory(directory);
            report.build = Application.version + " / Unity " + Application.unityVersion;
            Application.logMessageReceived += Logged;
            deadline = Time.realtimeSinceStartup + 960;
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60;
            Application.runInBackground = true;
            Time.captureDeltaTime = 0;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard = InputSystem.AddDevice<Keyboard>("Campaign QA");
            keyboard.MakeCurrent();
            yield return new WaitForSeconds(2.3f);
            if (guardianOnly) yield return ReviewGuardian(Array.IndexOf(args, "-guardianFilm") >= 0);
            else if (motionOnly)
            {
                game.tutorials.Enabled = false; game.StartRun();
                game.Complete(); game.NextChapter(); game.Complete();
                yield return ObserveRun(null);
                yield return ObserveClimb(null);
            }
            else
            {
                bool routeOnly = Array.IndexOf(args,"-moonlitRouteQA") >= 0;
                if (!routeOnly) yield return Fixtures();
                if (Array.IndexOf(args,"-fixturesOnly") < 0)
                {
                    game.Home(); game.tutorials.Enabled = true;
                    yield return MoonlitCampaignRoute.Play(game,(ok,detail) => Record("continuous_four_chapter_route",ok,detail));
                }
            }
            report.passed = report.checks.Count > 0 && report.checks.TrueForAll(c => c.passed);
            WriteReport(); done = true; Debug.Log("CAMPAIGN_QA_DONE " + report.passed);
            InputSystem.RemoveDevice(keyboard);
            Application.Quit(report.passed ? 0 : 1);
        }

        IEnumerator Fixtures()
        {
            game.tutorials.Enabled = false;
            Capture(game,"home");
            yield return Press(Key.Enter);
            Record("home_enter",game.State == GameState.Playing && game.Chapter == 1, game.State.ToString());
            if (game.State != GameState.Playing) game.StartRun();
            yield return new WaitForSeconds(.3f);
            var p = game.player;
            CheckHeartImages("full", 3);
            Capture(game, "health-full");
            float startX = p.Body.position.x;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));
            yield return new WaitForSeconds(.65f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            yield return null;
            Record("movement",p.Body.position.x > startX + 2 && p.Grounded,p.Body.position.ToString());
            yield return Press(Key.Escape);
            Vector2 pos = p.Body.position;
            yield return null; yield return null;
            Record("pause",game.State == GameState.Paused && Time.timeScale == 0 && p.Body.position == pos,game.State.ToString());
            yield return Press(Key.Escape);
            p.TestInput = true;
            p.QueueJump(); yield return new WaitForSeconds(.18f);
            p.QueueJump(); yield return new WaitForSeconds(.1f);
            Record("chapter1_ability_gates",p.Jumps == 1 && p.AirJumps == 0 && !p.Dash(),"jumps=" + p.Jumps);
            p.Teleport(new Vector2(60,.15f)); yield return new WaitForSeconds(.25f);
            Record("lamp",game.CheckpointIndex == 1,game.RespawnPoint.ToString());
            p.Teleport(new Vector2(93,-9)); yield return new WaitForSeconds(1f);
            Record("fall_returns_to_lamp",game.Health == 2 && Vector2.Distance(p.Body.position,game.RespawnPoint) < 1,p.Body.position.ToString());
            CheckHeartImages("after_fall", 2);
            var enemy = game.enemies[0]; p.Teleport((Vector2)enemy.transform.position + Vector2.left * 2);
            bool flashed = p.Pulse();
            Record("pulse_stun_and_cooldown",flashed && !p.Pulse() && enemy.IsStunned,"cooldown=" + p.Cooldown);
            p.TestMove = 0;
            game.Hurt(true); yield return new WaitForSeconds(.85f); CheckHeartImages("one_left", 1); game.Hurt(true);
            Record("last_heart",game.State == GameState.Defeat && game.Health == 0,game.State.ToString());
            game.RetryCheckpoint(); yield return new WaitForSeconds(.35f);
            Record("checkpoint_retry",game.State == GameState.Playing && game.Health == 3 && game.CheckpointIndex == 1,game.State.ToString());
            CheckHeartImages("restored", 3);
            for (int chapter = 1; chapter <= 4; chapter++)
            {
                p.Teleport(new Vector2(14,.04f)); game.cameraRig.Snap();
                float x = p.Body.position.x;
                float previousFar = game.cameraRig.LayerOffset(0), previousMiddle = game.cameraRig.LayerOffset(1);
                float previousNear = game.cameraRig.LayerOffset(2), previousRim = game.cameraRig.LayerOffset(3);
                p.Teleport(new Vector2(x+5,.04f)); game.cameraRig.Snap();
                float far = game.cameraRig.LayerOffset(0)-previousFar, middle = game.cameraRig.LayerOffset(1)-previousMiddle;
                float near = game.cameraRig.LayerOffset(2)-previousNear, rim = game.cameraRig.LayerOffset(3)-previousRim;
                Record("parallax_chapter_" + chapter,far > 0 && far < middle && middle < near && near < rim,
                    $"camera +5: {far:F3}, {middle:F3}, {near:F3}, {rim:F3}");
                CheckPanorama(chapter);
                bool banksGrounded = true;
                float minClearance = float.PositiveInfinity;
                foreach (float height in new[] { .1f,6.4f,11f })
                {
                    p.Teleport(new Vector2(19,height)); game.cameraRig.Snap();
                    for (int layer = 1; layer <= 3; layer++)
                    {
                        float clearance = game.cameraRig.BankGroundClearance(layer);
                        minClearance = Mathf.Min(minClearance,clearance);
                        banksGrounded &= clearance > 0;
                    }
                }
                Record("parallax_ground_contact_" + chapter,banksGrounded,"minimum bank depth below frame " + minClearance.ToString("F2") + "u");
                bool anchored = game.level.ValidatePlacements(out var anchorDetail);
                Record("world_footings_" + chapter,anchored,anchorDetail);
                CheckHabitats(chapter);
                CheckTerrain(chapter);
                Vector2[] terrainSamples = { new Vector2(399, .65f), new Vector2(575, 2), new Vector2(430, 1.8f), new Vector2(249, 2.8f) };
                p.Teleport(terrainSamples[chapter - 1] + Vector2.up * .04f); game.cameraRig.Snap();
                yield return new WaitForSeconds(.18f); Capture(game,"tread-" + chapter);
                if (chapter == 2)
                {
                    p.Teleport(new Vector2(8,.05f)); yield return new WaitForSeconds(.35f);
                    p.QueueJump(); yield return new WaitForSeconds(.32f); p.QueueJump(); yield return new WaitForSeconds(.22f);
                    Record("double_jump",p.AirJumps == 1 && p.Body.position.y > 3,"apex sample " + p.Body.position);
                }
                if (chapter == 3)
                {
                    p.Teleport(new Vector2(23.5f,.05f)); yield return new WaitForSeconds(.3f);
                    p.TestMove = 1; p.QueueJump(); yield return new WaitForSeconds(1.7f);
                    Record("wall_climb",p.ClimbDistance > 1 && p.Body.position.y > 5,p.Body.position.ToString());
                    p.TestMove = 0; p.ReleaseJump();
                    p.Teleport(new Vector2(108,3.2f)); game.Interact();
                    Record("shrine_bridge",game.level.lanternSwitch.Activated,"lit");
                }
                if (chapter == 4)
                {
                    p.Teleport(new Vector2(8,.05f)); yield return new WaitForSeconds(.3f);
                    bool dash = p.Dash(); bool repeat = p.Dash();
                    Record("dash",dash && !repeat && p.Dashes == 1,"cooldown=" + p.DashReady);
                }
                if (chapter == 2 || chapter == 3) yield return CheckWildlife(chapter);
                p.Teleport(new Vector2(game.level.EndX,game.level.EndFloor+.2f)); game.cameraRig.Snap();
                yield return new WaitForSeconds(.3f);
                bool reached = game.Shards == chapter && (chapter < 4 ? game.State == GameState.Complete : game.Boss && game.Boss.Active);
                Record("shard_" + chapter,reached,game.State.ToString());
                if (!reached) yield break;
                if (chapter < 4) { if (chapter == 1) { yield return new WaitForSeconds(1f); Capture(game,"chapter-clear"); } game.NextChapter(); yield return new WaitForSeconds(.3f); }
                else Capture(game,"guardian");
            }
            game.Home(); game.ContinueJourney();
            Record("continue_unlocked_chapter",game.State == GameState.Playing && game.Chapter == 4 && game.Shards == 3,game.LevelTitle);
            game.Home(); game.StartRun();
            Record("replay_resets_chapter",game.Chapter == 1 && game.Shards == 0 && game.CheckpointIndex == 0 && game.FurthestChapter == 4,game.LevelTitle);
            yield return CheckController();
            game.RestartChapter();
            yield return new WaitForSeconds(2.2f);
            bool film = Array.IndexOf(Environment.GetCommandLineArgs(),"-parallaxClip") >= 0;
            string frames = Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Temp/ParallaxFrames"));
            if (film)
            {
                Directory.CreateDirectory(frames);
                // A separate visual sample; these positions and timing never feed the measured route.
                float[] starts = { 6,14,20,10 };
                for (int chapter=1; chapter<=4; chapter++)
                {
                    game.StartChapter(chapter);
                    p.TestMove = 0; p.Teleport(new Vector2(starts[chapter-1],.06f)); game.cameraRig.Snap();
                    yield return new WaitForSeconds(.45f);
                    string section = Path.Combine(frames,"chapter-" + chapter);
                    Directory.CreateDirectory(section);
                    p.TestMove=1;
                    Time.captureDeltaTime=1f/30;
                    for (int i=0; i<120; i++)
                    {
                        if ((chapter==1 && (i==73 || i==103)) || (chapter==2 && (i==18 || i==32 || i==77)) ||
                            (chapter==3 && i==14) || (chapter==4 && (i==25 || i==40))) p.QueueJump();
                        if (chapter==4 && i==53) p.Dash();
                        yield return null;
                        MoonlitCapture.Save(Path.Combine(section,i.ToString("D4")+".png"),960,540);
                    }
                    p.TestMove=0; Time.captureDeltaTime=0;
                }
                yield return FilmWildlife(frames, 2);
                yield return FilmWildlife(frames, 3);
            }
            yield return ObserveRun(film ? Path.Combine(frames, "run") : null);
            yield return ObserveClimb(film ? Path.Combine(frames, "climb") : null);
        }
        IEnumerator CheckController()
        {
            var p = game.player;
            p.TestMove = 0; p.TestInput = false;
            p.Teleport(new Vector2(10,.05f));
            yield return new WaitForSeconds(.4f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A,Key.D));
            yield return new WaitForSeconds(.22f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return null;
            Record("opposing_keys_cancel",Mathf.Abs(p.Body.position.x-10) < .05f,p.Body.position.ToString());
            p.TestInput = true;
            float[] apex = new float[2];
            for (int i=0; i<2; i++)
            {
                p.Teleport(new Vector2(10,.04f)); yield return new WaitForSeconds(.15f);
                p.QueueJump(); float started = Time.time;
                while (Time.time-started < 1.1f)
                {
                    if (i == 0 && Time.time-started >= .065f) p.ReleaseJump();
                    apex[i] = Mathf.Max(apex[i],p.Body.position.y);
                    yield return null;
                }
            }
            Record("variable_jump",apex[1] > apex[0]+.75f,$"tap={apex[0]:F2}u hold={apex[1]:F2}u");
            p.Teleport(new Vector2(32.3f,2.04f)); yield return new WaitForSeconds(.15f);
            p.TestMove=1;
            float limit = Time.time+.6f;
            while (p.Grounded && Time.time<limit) yield return null;
            p.QueueJump(); yield return new WaitForSeconds(.08f);
            Record("coyote_at_paddy_edge",p.Body.linearVelocity.y>3,p.Body.linearVelocity.ToString());
            p.TestMove=0; p.ReleaseJump();
            p.Teleport(new Vector2(10,1.15f));
            limit=Time.time+1f;
            while ((p.Body.position.y > .30f || p.Body.linearVelocity.y>=0) && Time.time<limit) yield return null;
            int before=p.Jumps;
            p.QueueJump(); yield return new WaitForSeconds(.21f);
            Record("buffered_landing_jump",p.Jumps>before && p.Body.linearVelocity.y>0,p.Body.linearVelocity.ToString());
            var raft=game.level.moving[0];
            p.Teleport((Vector2)raft.transform.position+Vector2.up*.08f); yield return new WaitForSeconds(.25f);
            Vector2 raftStart=raft.transform.position, riderStart=p.Body.position;
            yield return new WaitForSeconds(.75f);
            Vector2 travel=(Vector2)raft.transform.position-raftStart;
            var relativeError=(p.Body.position-riderStart-travel).magnitude;
            Record("raft_carries_feet",p.Grounded && relativeError<.32f,"rider drift "+relativeError.ToString("F3")+"u");
            var dog=game.enemies[0];
            p.Teleport(dog.transform.position); game.cameraRig.Snap(); yield return new WaitForSeconds(.12f);
            int hearts=game.Health;
            bool hit=hearts==2 && p.Invulnerable>0;
            game.Hurt(false);
            Record("contact_knockback_and_immunity",hit && game.Health==hearts && game.State==GameState.Playing,"hearts="+hearts+" invulnerability="+p.Invulnerable.ToString("F2"));
            Capture(game, "health-damaged");
            p.Teleport(new Vector2(3,.05f)); p.TestMove=-1; yield return new WaitForSeconds(.8f);
            Record("left_boundary",p.Body.position.x>=MoonlitLevel.LeftPlayerLimit-.05f && game.State==GameState.Playing,p.Body.position.ToString());
            p.TestMove=0;
        }
        void OnDestroy() { Application.logMessageReceived -= Logged; }
    }
}
