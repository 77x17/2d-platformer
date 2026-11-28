using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        readonly HashSet<string> poses = new HashSet<string>();
        readonly List<string> filmAudio = new List<string>();
        bool reviewGuardian, filmGuardian;
        int filmTick, minimumBossHearts = 3, litInReview;
        float bossStarted;
        string framesPath;

        IEnumerator ReviewGuardian(bool film)
        {
            game.tutorials.Enabled = false;
            game.StartChapter(1,false);
            while (game.IsLoadingChapter) yield return null;
            for (int chapter = 1; chapter < 4; chapter++)
            {
                game.Complete(); game.NextChapter();
                while (game.IsLoadingChapter) yield return null;
            }
            game.player.Teleport(game.level.BossFoot + new Vector2(-12, .05f));
            game.Complete();
            var boss = game.Boss;
            var player = game.player;
            yield return new WaitUntil(() => boss.Warning);
            player.Pulse();
            Record("closed_bark_ignores_lantern", boss.LitBuds == 0, boss.Hint);
            float timer = boss.Progress;
            game.TogglePause();
            yield return new WaitForSecondsRealtime(.27f);
            Record("windup_freezes_on_pause", Mathf.Abs(timer - boss.Progress) < .001f && boss.Warning, boss.Progress.ToString("F2"));
            game.TogglePause();
            yield return new WaitUntil(() => boss.TargetOpen);
            Record("charge_hits_once", game.Health == 2, "stationary hearts=" + game.Health);
            yield return new WaitUntil(() => boss.Warning);
            Record("missed_opening_rearms", boss.LitBuds == 0 && boss.Attack == BanyanGuardian.Move.Charge, boss.Hint);
            while (game.State == GameState.Playing) game.Hurt(true);
            while (game.State == GameState.Respawning) yield return null;
            if (game.State == GameState.Playing) game.Hurt(true);
            if (game.State == GameState.Defeat) game.RetryCheckpoint();
            yield return null;
            Record("arena_checkpoint_rearms", game.Health == 3 && game.State == GameState.Playing &&
                !boss.Attacking && Vector2.Distance(player.Body.position, game.RespawnPoint) < .5f, "hearts=" + game.Health);

            filmGuardian = film;
            reviewGuardian = true;
            if (film) {
                framesPath = Path.Combine(directory, "Frames");
                Directory.CreateDirectory(framesPath);
                game.audioFx.SoundPlayed += NoteAudio;
            }
            yield return MoonlitCampaignRoute.Play(game, (ok, detail) => Record("entire_hill_to_ending", ok, detail), 4);
            reviewGuardian = false;
            game.audioFx.SoundPlayed -= NoteAudio;
            Time.captureDeltaTime = 0;
            if (film) {
                File.WriteAllLines(Path.Combine(directory, "sound-events.csv"), filmAudio);
                File.WriteAllText(Path.Combine(directory, "frame-count.txt"), filmTick.ToString());
            }
            Record("all_attacks_shown", poses.Contains("strike-Charge") && poses.Contains("strike-Slam") &&
                poses.Contains("strike-LowSweep") && poses.Contains("strike-HighSweep"), string.Join(", ", poses));
            Record("six_buds_and_ending", game.State == GameState.Ending && game.Boss.LitBuds == 6 &&
                game.Boss.CalmedRoots == 3, "buds=" + game.Boss.LitBuds + ", minimum hearts=" + minimumBossHearts);
        }

        void NoteAudio(string sound)
        {
            if (filmTick > 0)
                filmAudio.Add(((filmTick - 1) / 30f).ToString("F3", CultureInfo.InvariantCulture) + "," + sound);
        }

        void LateUpdate()
        {
            ObserveTale();
            if (!reviewGuardian || !game.Boss || !game.Boss.Active || game.State != GameState.Playing) return;
            var boss = game.Boss;
            if (filmTick == 0) {
                bossStarted = game.Elapsed;
                if (filmGuardian) Time.captureDeltaTime = 1f / 30;
            }
            if (game.Health < minimumBossHearts)
                Debug.Log($"GUARDIAN_REVIEW contact={boss.Attack} phase={boss.Progress:F2} player={game.player.Body.position:F2} root={boss.ContactX:F1}");
            minimumBossHearts = Mathf.Min(minimumBossHearts, game.Health);
            if (boss.LitBuds != litInReview) {
                litInReview = boss.LitBuds;
                Debug.Log($"GUARDIAN_REVIEW blossom={litInReview} hearts={game.Health} t={game.Elapsed-bossStarted:F1}");
            }
            string pose = null;
            if (boss.Warning && boss.Progress > .64f) pose = "tell-" + boss.Attack;
            if (boss.Attacking && boss.Progress > (boss.Attack == BanyanGuardian.Move.Slam ? .55f : .43f)) pose = "strike-" + boss.Attack;
            if (boss.TargetOpen) pose = "open";
            if (pose != null && poses.Add(pose)) Capture(game, pose);
            if (filmGuardian)
                MoonlitCapture.Save(Path.Combine(framesPath, filmTick.ToString("D4") + ".png"), 960, 540);
            filmTick++;
        }
    }
}
