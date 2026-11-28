using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        IEnumerator ReviewBanyanAndLanternBridge()
        {
            game.StartChapter(1, true);
            while (game.IsLoadingChapter) yield return null;
            yield return null;
            var treeImage = Array.Find(game.hud.GetComponentsInChildren<Image>(true), image => image.name == "Banyan Tree");
            var opening = treeImage.material;
            int pose = BanyanGuardian.GuardianPose;
            Vector4 first = opening.GetVector(pose);
            yield return new WaitForSecondsRealtime(.2f);
            Vector4 current = opening.GetVector(pose);
            bool runs = game.State == GameState.Prologue && current.x > first.x && current.y > .8f;
            // The opening is authored at 1600 x 900. Both captures use this native resolution.
            int moving = CompareBanyanFrames("opening-banyan", new Rect(674, 637, 248, 248),
                value => opening.SetVector(pose, value), current, out int feet);
            Record("opening_tree_gathers_branches", runs && moving > 70 && feet <= 4,
                $"changed body samples={moving}, changed root-foot samples={feet}, pose clock={current.x:F2}");
            game.EndStory();

            for (int chapter = 2; chapter <= 4; chapter++)
            {
                game.Complete();
                game.NextChapter();
                while (game.IsLoadingChapter) yield return null;
                if (chapter >= 3) yield return ReviewProcessionRise(chapter);
            }
            game.player.Teleport(game.level.BossFoot + new Vector2(-7, .05f));
            game.cameraRig.Snap();
            game.Complete();
            var boss = game.Boss;
            yield return null;
            var tree = boss.transform.Find("Guardian Trunk/Carved Banyan Face").GetComponent<SpriteRenderer>();
            var block = new MaterialPropertyBlock();
            tree.GetPropertyBlock(block);
            first = block.GetVector(pose);
            yield return new WaitForSeconds(.22f);
            tree.GetPropertyBlock(block);
            current = block.GetVector(pose);
            runs = current.x > first.x && current.y > .8f;
            var camera = game.ViewCamera;
            var lower = camera.WorldToViewportPoint(tree.bounds.min);
            var upper = camera.WorldToViewportPoint(tree.bounds.max);
            Rect body = Rect.MinMaxRect(lower.x * 1600, lower.y * 900, upper.x * 1600, upper.y * 900);
            moving = CompareBanyanFrames("arena-banyan", body, value => {
                block.SetVector(pose, value);
                tree.SetPropertyBlock(block);
            }, current, out feet);
            Record("arena_tree_gathers_branches", runs && moving > 70 && feet <= 4,
                $"changed body samples={moving}, changed root-foot samples={feet}, pose clock={current.x:F2}");
            yield return new WaitUntil(() => boss.Warning);
            tree.GetPropertyBlock(block);
            Record("guardian_windup_readable", block.GetVector(pose).y < .6f &&
                boss.transform.Find("Bound Attack Root").GetComponent<SpriteRenderer>().enabled, boss.Hint);
        }

        // Render the same game frame with two pose keys so wind, framing and moving
        // scenery cannot be counted as part of the guardian's performance.
        int CompareBanyanFrames(string shot, Rect bounds, Action<Vector4> setPose, Vector4 restore, out int feet)
        {
            var frames = new Texture2D[2];
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    setPose(new Vector4(i * .91f, 1, 0, 0));
                    string file = shot + (i == 0 ? "-rest" : "-reach");
                    Capture(game, file);
                    frames[i] = new Texture2D(2, 2);
                    frames[i].LoadImage(File.ReadAllBytes(Path.Combine(directory, file + ".png")));
                }
                var a = frames[0].GetPixels32();
                var b = frames[1].GetPixels32();
                feet = 0;
                int moving = 0;
                for (int y = 1; y < 92; y++)
                    for (int x = 4; x < 96; x++)
                    {
                        int px = Mathf.Clamp(Mathf.RoundToInt(bounds.x + x * bounds.width / 100), 0, frames[0].width - 1);
                        int py = Mathf.Clamp(Mathf.RoundToInt(bounds.y + y * bounds.height / 100), 0, frames[0].height - 1);
                        var p = a[py * frames[0].width + px];
                        var q = b[py * frames[0].width + px];
                        if (Math.Abs(p.r-q.r) + Math.Abs(p.g-q.g) + Math.Abs(p.b-q.b) < 26) continue;
                        if (y < 13) feet++;
                        if (y > 24 && (x < 42 || x > 58)) moving++;
                    }
                return moving;
            }
            finally
            {
                setPose(restore);
                foreach (var frame in frames) if (frame) Destroy(frame);
            }
        }

        IEnumerator ReviewProcessionRise(int chapter)
        {
            var trail = game.level.Procession;
            var bridge = Array.Find(game.level.GetComponentsInChildren<RisingLanternBridge>(true),
                span => span.name == "Lantern Procession Bridge");
            var decks = bridge.GetComponentsInChildren<BoxCollider2D>(true);
            bool cold = !bridge.gameObject.activeSelf && Array.TrueForAll(decks, deck => !deck.enabled);
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitUntil(() => game.player.Cooldown <= 0);
                game.player.Teleport(trail.NextLight - Vector2.up * .9f);
                game.player.Pulse();
                cold &= i == 2 || (!bridge.gameObject.activeSelf && trail.LitCount == i + 1);
                if (i < 2) game.player.Teleport(game.RespawnPoint);
            }
            var shore = bridge.transform.position + new Vector3(-9.6f, .15f, 0);
            game.player.Teleport(shore);
            game.cameraRig.Snap();
            float floor = bridge.transform.position.y;
            bool started = trail.Complete && !trail.BridgeOpen && bridge.gameObject.activeSelf &&
                Array.TrueForAll(decks, deck => deck.transform.position.y < floor - 3f && !deck.enabled);
            float until = Time.time + 4;
            while (trail.Rise < .44f && Time.time < until) yield return new WaitForFixedUpdate();
            bool rippled = !trail.BridgeOpen && decks[0].transform.position.y > decks[decks.Length - 1].transform.position.y + .3f &&
                Array.Exists(decks, deck => Quaternion.Angle(deck.transform.rotation, Quaternion.identity) > .25f);
            if (chapter == 3) Capture(game, "procession-bridge-rising");
            while (!trail.BridgeOpen && Time.time < until) yield return new WaitForFixedUpdate();
            bool level = trail.BridgeOpen && Array.TrueForAll(decks, deck => deck.enabled &&
                Mathf.Abs(deck.bounds.max.y - floor) < .03f && Quaternion.Angle(deck.transform.rotation, Quaternion.identity) < .01f);
            trail.ResetLights();
            bool reset = !bridge.gameObject.activeSelf && trail.LitCount == 0 && !trail.BridgeOpen &&
                Array.TrueForAll(decks, deck => !deck.enabled);
            Record("three_lantern_bridge_" + chapter, cold && started && rippled && level && reset,
                $"locked for first two lights={cold}, submerged={started}, staggered and rocking={rippled}, solid at bank={level}, reset={reset}");
        }
    }
}
