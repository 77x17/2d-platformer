using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Moonlit.Editor
{
    // Runs only in a dedicated batch Editor. The render call is the one used by Unity 6000.3's Game tab.
    // Keep ScreenSpaceOverlay active throughout so a camera capture cannot hide canvas/layout regressions.
    [InitializeOnLoad]
    public static class GameViewHealthReview
    {
        const string SessionKey = "Moonlit.EditorHealthReview.Running";
        static readonly Vector2Int[] Sizes = {
            new Vector2Int(1600,900), new Vector2Int(720,480), new Vector2Int(512,288), new Vector2Int(480,800)
        };
        static readonly string Folder = Path.GetFullPath("QA/EditorGameView");
        const BindingFlags Bind = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly List<string> Rows = new List<string>();
        static readonly Dictionary<Vector2Int, RenderTexture> Targets = new Dictionary<Vector2Int, RenderTexture>();
        static int sizeIndex, step, failed;
        static double next, deadline;
        static EditorWindow gameView;
        static Vector2Int configured;
        static readonly HashSet<Sprite> RunPoses = new HashSet<Sprite>(), ClimbPoses = new HashSet<Sprite>();
        static double movingSince;
        static int motion;

        static GameViewHealthReview()
        {
            if (Application.isBatchMode && Environment.GetCommandLineArgs().Contains("-inspectHealthInEditor"))
                EditorApplication.update += Tick;
        }

        public static void Run()
        {
            Directory.CreateDirectory(Folder);
            EditorSceneManager.OpenScene("Assets/Moonlit/Scenes/Level01_BanyanForest.unity");
            SessionState.SetBool(SessionKey, true);
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!SessionState.GetBool(SessionKey, false) || !EditorApplication.isPlaying) return;
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (deadline == 0) deadline = now + 130;
                if (now > deadline) throw new TimeoutException("Editor Play Mode did not finish the health review.");
                EditorApplication.QueuePlayerLoopUpdate();
                if (now < next) return;
                var game = MoonlitGame.Instance;
                if (!game || !game.hud) return;
                if (sizeIndex == Sizes.Length) { ObserveMotion(game, now); return; }
                var size = Sizes[sizeIndex];
                SetSize(size);
                switch (step++)
                {
                    case 0:
                        game.StartRun();
                        next = now + 1.7;
                        break;
                    case 1:
                        Capture(game, size, "playing", 3);
                        game.tutorials.Open(0);
                        next = now + .90;
                        break;
                    case 2:
                        Capture(game, size, "tutorial", 3);
                        game.tutorials.Cancel();
                        game.EndTutorial();
                        next = now + 1.45;
                        break;
                    case 3:
                        game.Hurt();
                        next = now + .22;
                        break;
                    case 4:
                        Capture(game, size, "after-hit", 2);
                        game.Hurt(true);
                        next = now + .08;
                        break;
                    case 5:
                        Capture(game, size, "respawn", 1);
                        ++sizeIndex;
                        step = 0;
                        next = now + .9;
                        break;
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(Folder, "error.txt"), ex.ToString());
                Debug.LogException(ex);
                SessionState.SetBool(SessionKey, false);
                EditorApplication.Exit(2);
            }
        }

        static void SetSize(Vector2Int size)
        {
            Vector2 pixels = size;
            // These are the Editor's per-display and Game View targets, independent of the desktop window.
            if (!gameView)
            {
                Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView", true);
                gameView = EditorWindow.GetWindow(type, false, "Game", false);
                gameView.Show();
            }
            const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
            if (configured != size)
            {
                // Set a fixed-size selection in this process, without saving a resize preset to the user's Editor.
                var assembly = typeof(EditorWindow).Assembly;
                var settingsType = assembly.GetType("UnityEditor.GameViewSizes", true);
                var settings = settingsType.BaseType.GetProperty("instance", Bind).GetValue(null);
                var group = settingsType.GetProperty("currentGroup", instance | BindingFlags.Public).GetValue(settings);
                var groupType = group.GetType();
                int index = (int)groupType.GetMethod("GetTotalCount", instance | BindingFlags.Public).Invoke(group, null);
                var ctor = assembly.GetType("UnityEditor.GameViewSize", true);
                var kind = Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType", true), "FixedResolution");
                object preset = Activator.CreateInstance(ctor, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new object[] { kind, size.x, size.y, "Moonlit QA " + size }, null);
                groupType.GetMethod("AddCustomSize", instance | BindingFlags.Public).Invoke(group, new[] { preset });
                gameView.GetType().GetProperty("selectedSizeIndex", instance | BindingFlags.Public).SetValue(gameView, index);
                gameView.GetType().GetMethod("UpdateZoomAreaAndParent", instance | BindingFlags.Public).Invoke(gameView, null);
                gameView.GetType().BaseType.GetProperty("targetSize", instance | BindingFlags.Public).SetValue(gameView, pixels);
                configured = size;
            }
            typeof(EditorWindow).GetMethod("SetDisplayViewSize", instance).Invoke(gameView, new object[] { 0, pixels });
            typeof(EditorWindow).GetMethod("SetMainPlayModeViewSize", instance).Invoke(gameView, new object[] { pixels });
            typeof(EditorWindow).GetMethod("SetPlayModeViewSize", instance).Invoke(gameView, new object[] { pixels });
        }

        static Color32[] Render(Vector2Int size, string name)
        {
            if (!Targets.TryGetValue(size, out var target))
            {
                // The Editor caches its last target for the next frame; keep each target for the full review.
                target = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
                target.Create();
                Targets.Add(size, target);
            }
            var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            var old = RenderTexture.active;
            try
            {
                Canvas.ForceUpdateCanvases();
                var draw = typeof(EditorGUIUtility).GetMethod("RenderPlayModeViewCamerasInternal", Bind);
                draw.Invoke(null, new object[] { target, 0, Vector2.zero, false, true });
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0,0,size.x,size.y), 0,0);
                image.Apply();
                var colors = image.GetPixels32();
                // On D3D the Game tab flips the render target when it blits it to the window.
                if (SystemInfo.graphicsUVStartsAtTop)
                {
                    var upright = new Color32[colors.Length];
                    for (int row = 0; row < size.y; row++)
                        Array.Copy(colors, row * size.x, upright, (size.y - 1 - row) * size.x, size.x);
                    colors = upright;
                    image.SetPixels32(colors);
                    image.Apply();
                }
                File.WriteAllBytes(Path.Combine(Folder, name+".png"), image.EncodeToPNG());
                return colors;
            }
            finally
            {
                RenderTexture.active = old;
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        static void Capture(MoonlitGame game, Vector2Int size, string state, int expected)
        {
            string label = $"{size.x}x{size.y}-{state}";
            var canvas = game.hud.GetComponent<Canvas>();
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) throw new InvalidOperationException("Expected normal overlay canvas.");
            var pixels = Render(size, label);
            var icons = game.hud.GetComponentsInChildren<Graphic>(true)
                .Where(g => (g is Image && (g.name == "Heart 1" || g.name == "Heart 2" || g.name == "Heart 3")) ||
                           (g is MoonlitEmblem e && e.shape == MoonlitEmblem.Shape.Heart))
                .OrderBy(g=>g.rectTransform.position.x).ToArray();
            var counts = new List<int>();
            bool pass = game.Health == expected && icons.Length == 3 && Screen.width == size.x && Screen.height == size.y;
            if (state == "tutorial") pass &= game.State == GameState.Tutorial;
            if (state == "respawn") pass &= game.State == GameState.Respawning;
            for (int i = 0; i < icons.Length; i++)
            {
                var points = new Vector3[4];
                icons[i].rectTransform.GetWorldCorners(points);
                Vector2 lo = RectTransformUtility.WorldToScreenPoint(null, points[0]);
                Vector2 hi = RectTransformUtility.WorldToScreenPoint(null, points[2]);
                int count = 0;
                for (int y = Mathf.Max(0,Mathf.CeilToInt(lo.y)); y < Mathf.Min(size.y,Mathf.FloorToInt(hi.y)); y++)
                for (int x = Mathf.Max(0,Mathf.CeilToInt(lo.x)); x < Mathf.Min(size.x,Mathf.FloorToInt(hi.x)); x++)
                {
                    Color32 c = pixels[y*size.x+x];
                    if (c.r >= 96 && c.r > c.g*1.48f && c.r > c.b*1.60f) count++;
                }
                counts.Add(count);
                bool expectedFull = i < expected;
                pass &= icons[i].isActiveAndEnabled && lo.x >= 0 && hi.y <= size.y + 1 &&
                        hi.x <= size.x + 1 && lo.y >= 0 && hi.x-lo.x >= 13 && hi.y-lo.y >= 11 &&
                        (expectedFull ? count >= 20 : count < 12);
            }
            if (!pass) failed++;
            string row = $"{size.x},{size.y},{Screen.width},{Screen.height},{state},{game.State},{game.Health},{icons.Length},\"{string.Join(" / ",counts)}\",{pass}";
            Rows.Add(row);
            Debug.Log("EDITOR_HEART_QA " + row);
        }

        static void Finish()
        {
            File.WriteAllLines(Path.Combine(Folder, "health.csv"),
                new[] {"width,height,screenWidth,screenHeight,scenario,gameState,health,icons,redPixelsByHeart,passed"}.Concat(Rows));
            SessionState.SetBool(SessionKey, false);
            Debug.Log("EDITOR_HEART_QA_DONE failures=" + failed);
            EditorApplication.Exit(failed == 0 ? 0 : 1);
        }

        static void ObserveMotion(MoonlitGame game, double now)
        {
            var viewSize = new Vector2Int(960,540);
            var p = game.player;
            if (motion == 0)
            {
                SetSize(viewSize);
                game.StartRun();
                game.tutorials.Enabled = false;
                p.Teleport(new Vector2(176,.04f));
                p.TestInput = true; p.TestMove = 1;
                game.cameraRig.Snap();
                movingSince = now; motion++;
            }
            else if (motion == 1)
            {
                if (p.body.sprite.texture.name == "CuoiRun" && RunPoses.Add(p.body.sprite) && RunPoses.Count == 4)
                    Render(viewSize, "editor-run");
                if (now - movingSince < 2.4) return;
                // Unlock only the in-memory test instance. This QA never changes saved player progression.
                typeof(MoonlitGame).GetField("best",BindingFlags.Instance | BindingFlags.NonPublic).SetValue(game,2);
                game.StartChapter(3);
                game.tutorials.Enabled = false;
                p.Teleport(new Vector2(20,.04f)); p.TestInput = true; p.TestMove = 1;
                game.cameraRig.Snap(); movingSince = now; motion++;
            }
            else
            {
                if (motion == 2 && p.Body.position.x >= 22.45f) { p.QueueJump(); motion++; }
                var texture = p.body.sprite.texture.name;
                if (p.IsClimbing && texture.StartsWith("CuoiClimb", StringComparison.Ordinal) &&
                    ClimbPoses.Add(p.body.sprite) && ClimbPoses.Count == 3) Render(viewSize, "editor-climb");
                bool crested = p.Grounded && p.Body.position.y >= 6.3f && p.Body.position.x > 24;
                if (!crested && now - movingSince < 10) return;
                int scores = Resources.LoadAll<AudioClip>("MoonAudio").Count(a => a.name.StartsWith("mus_",StringComparison.Ordinal) && a.channels == 2 && a.frequency == 32000);
                bool pass = RunPoses.Count >= 5 && ClimbPoses.Count >= 4 && crested && scores == 6;
                File.WriteAllText(Path.Combine(Folder,"motion-and-piano.txt"),
                    $"Run sprites seen: {RunPoses.Count}\nClimb sprites seen: {ClimbPoses.Count}\nReached wall top: {crested}\nStereo piano themes at 32 kHz: {scores}\nPassed: {pass}\n");
                if (!pass) failed++;
                Finish();
            }
        }
    }
}
