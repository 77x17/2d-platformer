using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moonlit.Editor
{
    public static class LevelOneBuilder
    {
        const string Root = "Assets/Moonlit/";

        [MenuItem("Moonlit/1 - Create or refresh level 1")]
        public static void Setup()
        {
            var art = AssetDatabase.LoadAssetAtPath<MoonlitArt>(Root + "Settings/ArtLibrary.asset");
            if (!art) { art = ScriptableObject.CreateInstance<MoonlitArt>(); AssetDatabase.CreateAsset(art, Root + "Settings/ArtLibrary.asset"); }
            art.cuoi = ImportSheet("Cuoi", true);
            art.dog = ImportSheet("Dog", true);
            art.cat = ImportSheet("Cat", true);
            art.environment = ImportSheet("Environment", false);
            art.forestExtras = ImportSheet("ForestExtras", false);
            art.forest = ImportSingle("Forest", false);
            art.moon = ImportSingle("MoonShard", true);
            art.uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "Settings/PixelSprite.mat");
            if (!material)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (!shader) shader = Shader.Find("Sprites/Default");
                material = new Material(shader); AssetDatabase.CreateAsset(material, Root + "Settings/PixelSprite.mat");
            }
            art.spriteMaterial = material; EditorUtility.SetDirty(art);
            var tuning = AssetDatabase.LoadAssetAtPath<PlayerTuning>(Root + "Settings/PlayerTuning.asset");
            if (!tuning) { tuning = ScriptableObject.CreateInstance<PlayerTuning>(); AssetDatabase.CreateAsset(tuning, Root + "Settings/PlayerTuning.asset"); }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("CUOI - Level 01 bootstrap (settings below)"); var game = root.AddComponent<MoonlitGame>(); game.art = art; game.tuning = tuning;
            EditorSceneManager.SaveScene(scene, Root + "Scenes/Level01_BanyanForest.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "Scenes/Level01_BanyanForest.unity", true) };
            PlayerSettings.companyName = "Moonlit Studio"; PlayerSettings.productName = "Cuoi - Duong Ve Trang";
            PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true; PlayerSettings.runInBackground = true;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            Time.fixedDeltaTime = 1f / 60;
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue = "Terrain";
            tags.FindProperty("layers").GetArrayElementAtIndex(9).stringValue = "Player";
            tags.ApplyModifiedProperties();
            Physics2D.IgnoreLayerCollision(9, 9);
            AssetDatabase.SaveAssets();
            Debug.Log("MOONLIT: Level 1 scene, sprites, settings and build list created.");
        }

        static void Configure(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.isReadable = true;
            importer.wrapMode = TextureWrapMode.Clamp;
        }

        static Sprite[] ImportSheet(string name, bool actor)
        {
            string path = Root + "Art/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); Configure(importer); importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.SaveAndReimport();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int cw = tex.width / 4, ch = tex.height / 4;
            var pixels = tex.GetPixels32();
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var existing = provider.GetSpriteRects();
            var frames = new SpriteRect[16];
            for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++)
            {
                int ox = col * cw, oy = (3 - row) * ch;
                int minX = cw - 1, maxX = 0, minY = ch - 1, maxY = 0;
                // Trim transparent atlas padding. The source PNG remains untouched.
                for (int y = 3; y < ch - 3; y++) for (int x = 3; x < cw - 3; x++)
                {
                    if (pixels[(oy + y) * tex.width + ox + x].a < 180) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
                if (maxX <= minX || maxY <= minY) throw new Exception("Empty sprite cell: " + name + " " + row + "," + col);
                Rect rect;
                Vector2 pivot;
                if (actor)
                {
                    // Keep horizontal registration fixed. Only the feet are aligned per frame.
                    rect = new Rect(ox + 2, oy + minY, cw - 4, Mathf.Min(ch - minY, maxY - minY + 3));
                    pivot = new Vector2(.5f, 0);
                }
                else { rect = new Rect(ox + minX, oy + minY, maxX - minX + 1, maxY - minY + 1); pivot = new Vector2(.5f, 1); }
                var frameName = name + "_" + (row * 4 + col).ToString("00");
                var old = existing.FirstOrDefault(s => s.name == frameName);
                frames[row * 4 + col] = new SpriteRect { name = frameName, rect = rect, alignment = SpriteAlignment.Custom, pivot = pivot, spriteID = old != null ? old.spriteID : GUID.Generate() };
            }
            importer.spritePixelsPerUnit = actor ? ch / (name == "Cuoi" ? 2.15f : 1.65f) : 100;
            provider.SetSpriteRects(frames);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(frames.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length != 16) throw new Exception(name + " expected 16 sprites, got " + sprites.Length);
            return sprites;
        }

        static Sprite ImportSingle(string name, bool alpha)
        {
            var path = Root + "Art/" + name + ".png"; var i = (TextureImporter)AssetImporter.GetAtPath(path);
            Configure(i); i.spriteImportMode = SpriteImportMode.Single; i.spritePixelsPerUnit = 100; i.alphaIsTransparency = alpha;
            i.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Trim and register new assets without changing the original actor/environment atlases.
        static Sprite[] ImportReadableSprites(string name, int columns, int rows, bool walkingSurface)
        {
            string path = Root + "Art/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Configure(importer); importer.spriteImportMode = SpriteImportMode.Multiple;
            var importSettings = new TextureImporterSettings(); importer.ReadTextureSettings(importSettings);
            importSettings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(importSettings);
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); var pixels = texture.GetPixels32();
            int cw = texture.width / columns, ch = texture.height / rows;
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects(); var frames = new SpriteRect[columns * rows];
            for (int row = 0; row < rows; row++) for (int col = 0; col < columns; col++)
            {
                int ox = col * cw, oy = (rows - 1 - row) * ch;
                int minX = cw, maxX = 0, minY = ch, maxY = 0;
                for (int y = 0; y < ch; y++) for (int x = 0; x < cw; x++)
                {
                    if (pixels[(oy + y) * texture.width + ox + x].a < 180) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
                if (maxX <= minX || maxY <= minY) throw new Exception("Empty readability sprite: " + name);
                int walkY = maxY;
                if (walkingSurface)
                {
                    int margin = Mathf.Max(2, (maxX - minX) / 10);
                    int left = minX + margin, right = maxX - margin;
                    // Ignore isolated grass tips: the first continuous opaque row is the walking plane.
                    for (int y = maxY; y > minY; y--)
                    {
                        int solid = 0;
                        for (int x = left; x <= right; x++) if (pixels[(oy + y) * texture.width + ox + x].a >= 200) solid++;
                        if (solid >= (right - left + 1) * .92f) { walkY = y; break; }
                    }
                    maxY = Mathf.Min(maxY, walkY + Mathf.Max(2, (walkY - minY) / 40));
                }
                var rect = new Rect(ox + minX, oy + minY, maxX - minX + 1, maxY - minY + 1);
                Vector2 pivot = walkingSurface ? new Vector2(.5f, (walkY - minY + .5f) / rect.height) : Vector2.one * .5f;
                string frameName = name + "_" + (row * columns + col).ToString("00");
                var old = previous.FirstOrDefault(s => s.name == frameName);
                frames[row * columns + col] = new SpriteRect { name = frameName, rect = rect, pivot = pivot,
                    alignment = SpriteAlignment.Custom, spriteID = old != null ? old.spriteID : GUID.Generate() };
            }
            provider.SetSpriteRects(frames);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(frames.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        }

        [MenuItem("Moonlit/4 - Import readable tokens and platform surfaces")]
        public static void PrepareReadabilityAssets()
        {
            var art = AssetDatabase.LoadAssetAtPath<MoonlitArt>(Root + "Settings/ArtLibrary.asset");
            if (!art) return;
            if (!art.moonToken) art.moonToken = ImportReadableSprites("MoonToken", 1, 1, false)[0];
            if (art.mossPlatforms == null || art.mossPlatforms.Length != 4 || art.mossPlatforms.Any(s => !s))
                art.mossPlatforms = ImportReadableSprites("MossPlatforms", 2, 2, true);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            Debug.Log("MOONLIT_READABILITY_READY: Moon token + four registered moss surfaces imported. No player build requested.");
        }

        [MenuItem("Moonlit/2 - Build Windows executable")]
        public static void Build()
        {
            if (!File.Exists(Root + "Scenes/Level01_BanyanForest.unity")) Setup();
            PreparePolishAssets();
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Builds/Windows/Cuoi.exe", target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            Directory.CreateDirectory("QA");
            File.WriteAllText("QA/build-result.txt", $"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nDuration: {report.summary.totalTime}\n");
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows build failed: " + report.summary.result);
            Debug.Log("MOONLIT_BUILD_OK: Builds/Windows/Cuoi.exe");
        }

        [MenuItem("Moonlit/3 - Import forest additions (preserve existing art)")]
        public static void PreparePolishAssets()
        {
            var art = AssetDatabase.LoadAssetAtPath<MoonlitArt>(Root + "Settings/ArtLibrary.asset");
            if (art.forestExtras == null || art.forestExtras.Length != 16)
            {
                art.forestExtras = ImportSheet("ForestExtras", false);
                EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            }
            PrepareReadabilityAssets();
        }
    }
}
