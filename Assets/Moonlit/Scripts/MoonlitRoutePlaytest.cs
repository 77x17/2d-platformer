using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Moonlit
{
    // End-to-end input driver: no teleport calls, no invincibility, normal jump and lantern cooldowns.
    public sealed class MoonlitRoutePlaytest : MonoBehaviour
    {
        [Serializable] class Result
        {
            public bool completed;
            public float seconds, farthest;
            public int deaths, jumps, pulses, reachedSurface, tutorialsSeen;
            public string lastPosition;
        }
        IEnumerator Start()
        {
            var g = MoonlitGame.Instance; yield return new WaitForSeconds(2);
            g.StartChapter(1,false); while (g.IsLoadingChapter) yield return null;
            g.player.TestInput = true;
            var surfaces = g.level.GetComponentsInChildren<BoxCollider2D>(true)
                .Where(c => !c.isTrigger && c.gameObject.layer == 8 && c.size.y < 20 && !c.GetComponent<PushableCrate>())
                .OrderBy(c => c.transform.position.x).ToArray();
            int current = 0, airTarget = -1, lastCheckpoint = 0;
            float jumpTime = -10;
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../QA")); Directory.CreateDirectory(dir);
            while (g.Elapsed < 150 && g.State != GameState.Complete)
            {
                if (g.State == GameState.Tutorial)
                {
                    if (g.tutorials.CanContinue) g.tutorials.Continue();
                    yield return null; continue;
                }
                if (g.State != GameState.Playing) { yield return null; continue; }
                var p = g.player; Vector2 pos = p.transform.position;
                if (g.level.lanternSwitch.Near) g.Interact();
                foreach (var e in g.enemies)
                    if (!e.IsStunned && Mathf.Abs(e.transform.position.x - pos.x) < 3.8f && Mathf.Abs(e.transform.position.y - pos.y) < 2) p.Pulse();
                if (p.Grounded && Time.time - jumpTime > .20f)
                {
                    for (int i = 0; i < surfaces.Length; i++)
                    {
                        var s = surfaces[i];
                        if (s.enabled && s.gameObject.activeInHierarchy && Mathf.Abs(pos.y - s.transform.position.y) < .4f && Mathf.Abs(pos.x - s.transform.position.x) < s.size.x * .5f + .3f)
                        { current = i; break; }
                    }
                    airTarget = -1;
                    if (current >= surfaces.Length - 1) p.TestMove = pos.x < MoonlitLevel.GoalX ? 1 : 0;
                    else
                    {
                        var now = surfaces[current]; var next = surfaces[current + 1];
                        float edge = now.transform.position.x + now.size.x * .5f;
                        if (pos.x < edge - .42f) p.TestMove = 1;
                        else
                        {
                            p.TestMove = 0;
                            Vector2 nextPos = next.transform.position;
                            var mover = next.GetComponent<MovingPlatform>();
                            if (mover) nextPos = mover.FuturePosition(.62f);
                            float dy = nextPos.y - pos.y;
                            float flight = .375f + Mathf.Sqrt(Mathf.Max(0, 2 * (2.4f - dy) / 45));
                            float left = nextPos.x - next.size.x * .5f;
                            if (next.gameObject.activeInHierarchy && dy < 2.26f && pos.x + 6.4f * flight > left + .32f)
                            { airTarget = current + 1; p.TestMove = 1; p.QueueJump(); jumpTime = Time.time; }
                        }
                    }
                }
                else if (airTarget >= 0)
                {
                    var next = surfaces[airTarget];
                    float target = next.transform.position.x;
                    // Brake over the destination rather than overshooting narrow platforms.
                    p.TestMove = pos.x < target - .2f ? 1 : pos.x > target + .55f ? -1 : 0;
                }
                if (g.CheckpointIndex > lastCheckpoint)
                {
                    lastCheckpoint = g.CheckpointIndex;
                    Debug.Log($"MOONLIT_ROUTE checkpoint={lastCheckpoint}, seconds={g.Elapsed:F1}, deaths={g.Deaths}");
                    MoonlitCapture.Save(Path.Combine(dir, "route-checkpoint-" + lastCheckpoint + ".png"));
                }
                yield return null;
            }
            var result = new Result { completed = g.State == GameState.Complete, seconds = g.Elapsed, farthest = g.Farthest, deaths = g.Deaths,
                jumps = g.player.Jumps, pulses = g.player.Pulses, reachedSurface = current, tutorialsSeen = g.tutorials.SeenCount, lastPosition = g.player.transform.position.ToString() };
            File.WriteAllText(Path.Combine(dir, "route-results.json"), JsonUtility.ToJson(result, true));
            MoonlitCapture.Save(Path.Combine(dir, "route-final.png"));
            Debug.Log("MOONLIT_ROUTE_DONE " + JsonUtility.ToJson(result));
            yield return new WaitForSeconds(.5f); Application.Quit(result.completed ? 0 : 1);
        }
    }
}
