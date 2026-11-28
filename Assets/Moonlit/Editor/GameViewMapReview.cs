using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Moonlit.Editor
{
    // Read the Game tab's overlay pixels to catch empty charts as well as leaked routes and pickups.
    [InitializeOnLoad]
    public static class GameViewMapReview
    {
        const string Key = "Moonlit.EditorMapReview";
        const string Folder = "QA/MapVisibility";
        static readonly Vector2Int Size = new Vector2Int(3840, 2160);
        static int step;
        static double next, deadline;
        static bool passed = true;

        static GameViewMapReview()
        {
            if (Application.isBatchMode && Environment.GetCommandLineArgs().Contains("-inspectMapInEditor"))
                EditorApplication.update += Tick;
        }

        public static void Run()
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, "map.csv"), "view,section,terrainPixels,playerPixels,motePixels,passed\n");
            EditorSceneManager.OpenScene("Assets/Moonlit/Scenes/01_Bamboo_Village.unity");
            SessionState.SetBool(Key, true);
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying) return;
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (deadline == 0) deadline = now + 120;
                if (now > deadline) throw new TimeoutException("Game View map capture timed out.");
                EditorApplication.QueuePlayerLoopUpdate();
                var game = MoonlitGame.Instance;
                if (!game || !game.player) return;
                GameViewHealthReview.SetSize(Size);
                if (now < next || Screen.width != Size.x || Screen.height != Size.y) return;
                switch (step++)
                {
                    case 0:
                        game.tutorials.Enabled = false;
                        break;
                    case 1:
                        Capture(game, "village-pocket", 0, 1, 1);
                        VerifyFrontier(game);
                        game.ToggleMap();
                        break;
                    case 2:
                        Capture(game, "village-atlas", 0, 3, 1);
                        game.ToggleMap();
                        var perch = game.ChapterSource.platforms.First(s => s.name == "Irrigation Lookout 02");
                        game.player.Teleport(perch.Top + Vector2.up * .10f);
                        break;
                    case 3:
                        Capture(game, "village-pocket-mote", 0, 1, 1, 1);
                        game.player.Teleport((Vector2)LookoutMote(game).transform.position - Vector2.up * .72f);
                        break;
                    case 4:
                        passed &= LookoutMote(game).Collected;
                        Capture(game, "village-pocket-collected", 0, 1, 1, 0);
                        game.player.Teleport(game.ChapterSource.checkpoints[3].position);
                        break;
                    case 5:
                        game.ToggleMap();
                        break;
                    case 6:
                        int section = Mathf.Clamp((int)(3 * game.player.Body.position.x / game.level.RouteLength), 0, 2);
                        passed &= section > 0;
                        var survey = game.level.GetComponent<MoonlitMapDiscovery>();
                        float checkpoint = game.ChapterSource.checkpoints[3].position.x;
                        passed &= !survey.KnownSpan(80,checkpoint - 20,out _,out _) && survey.CanIdentify(LookoutMote(game).transform.position);
                        Capture(game, "village-atlas-reopened", section, 3, 1 | (1 << section));
                        game.ToggleMap();
                        game.RestartChapter();
                        break;
                    case 7:
                        passed &= !game.level.GetComponent<MoonlitMapDiscovery>().CanIdentify(LookoutMote(game).transform.position);
                        Finish(passed ? 0 : 1);
                        break;
                }
                next = now + .6;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Finish(2);
            }
        }

        static ForestPickup LookoutMote(MoonlitGame game) => game.level.pickups.First(p =>
            p.kind == PickupKind.Mote && p.name == "Top of the Irrigation Lookout");

        static void VerifyFrontier(MoonlitGame game)
        {
            var survey = game.level.GetComponent<MoonlitMapDiscovery>();
            var mask = survey.Mask;
            Vector2 at = game.player.Body.position + Vector2.up;
            float Fog(int horizontal, int vertical) => mask.GetPixel(Mathf.RoundToInt(at.x) + horizontal,
                Mathf.RoundToInt(at.y) - MoonlitMapDiscovery.Bottom + vertical).a;
            passed &= Fog(0, 0) < .01f && Fog(12, 0) > .05f && Fog(12, 0) < .9f &&
                Fog(21, 0) > .99f && Fog(0, 10) > .99f && !survey.CanIdentify(LookoutMote(game).transform.position);
        }

        static void Capture(MoonlitGame game, string view, int playerSection, int count, int knownSections, int expectedMotes = -1)
        {
            if (game.hud.GetComponent<Canvas>().renderMode != RenderMode.ScreenSpaceOverlay ||
                Screen.width != Size.x || Screen.height != Size.y) throw new InvalidOperationException($"Unexpected Game View {Screen.width} × {Screen.height}, canvas {game.hud.GetComponent<Canvas>().renderMode}");
            var charts = game.hud.GetComponentsInChildren<MoonlitMapStrip>().OrderByDescending(c => c.transform.position.y).ToArray();
            if (charts.Length != count) throw new InvalidOperationException("Incorrect number of charts in " + view);
            var pixels = GameViewHealthReview.Render(Size, view, Folder);
            for (int i = 0; i < charts.Length; i++)
            {
                var corners = new Vector3[4];
                charts[i].rectTransform.GetWorldCorners(corners);
                int terrain = 0, player = 0, motes = 0;
                for (int y = Mathf.CeilToInt(corners[0].y) + 1; y < Mathf.FloorToInt(corners[2].y) - 1; y++)
                for (int x = Mathf.CeilToInt(corners[0].x) + 1; x < Mathf.FloorToInt(corners[2].x) - 1; x++)
                {
                    var pixel = pixels[y * Size.x + x];
                    if (pixel.g >= 65 && pixel.g > pixel.r * 1.25f && pixel.g > pixel.b) terrain++;
                    // Count the star's opaque core all the way to the brightest pulse frame (255, 217, 115).
                    if (pixel.r == 255 && pixel.g >= 178 && pixel.g <= 218 && pixel.b >= 67 && pixel.b <= 116) player++;
                    if (pixel.r > 172 && pixel.b > 209 && pixel.b > pixel.r + 18 && pixel.r > pixel.g + 17) motes++;
                }
                bool explored = (knownSections & (1 << i)) != 0;
                bool visible = (explored ? terrain > 100 : terrain == 0 && motes == 0) &&
                    (i == playerSection ? player > 20 : player == 0) &&
                    (expectedMotes < 0 || (expectedMotes == 0 ? motes == 0 : motes > 12));
                passed &= visible;
                File.AppendAllText(Path.Combine(Folder, "map.csv"), $"{view},{i+1},{terrain},{player},{motes},{visible}\n");
            }
        }

        static void Finish(int code)
        {
            SessionState.SetBool(Key, false);
            Debug.Log("GAME_VIEW_MAP_REVIEW " + code);
            EditorApplication.Exit(code);
        }
    }
}
