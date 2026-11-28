using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Moonlit
{
    public enum GameState { Home, Playing, Paused, Respawning, Complete, Tutorial, Defeat, Ending, Prologue, Reunion, Loading, Map }

    public sealed class MoonlitGame : MonoBehaviour
    {
        const string ProgressKey = "Moonlit.Campaign.ShardsRestored";
        static readonly string[] Names = { "Bờ Tre Làng Cuội", "Rừng Trúc", "Sông Đèn", "Đồi Gốc Đa" };
        public static MoonlitGame Instance { get; private set; }
        public MoonlitChapterScene ChapterSource { get; private set; }
        public bool IsLoadingChapter => State == GameState.Loading;
        public MoonlitControls Controls { get; private set; }
        public Camera ViewCamera { get; private set; }
        GameObject gameplayActors;
        public MoonlitArt art;
        public PlayerTuning tuning;
        public GameState State { get; private set; } = GameState.Home;
        public int Chapter { get; private set; } = 1;
        public int Shards { get; private set; }
        public int FurthestChapter => Mathf.Min(4, best + 1);
        public bool FinishedCampaign => best == 4;
        public static string ChapterName(int chapter) => Names[Mathf.Clamp(chapter, 1, 4) - 1];
        public string LevelTitle => ChapterName(Chapter);
        public int Health { get; private set; } = 3;
        public int Deaths { get; private set; }
        public int CheckpointIndex { get; private set; }
        public int Motes { get; private set; }
        readonly int[] bestMotes = new int[4];
        public int BestMotes(int chapter) => bestMotes[chapter - 1];
        static string MoteRecordKey(int chapter) => "Moonlit.Campaign.BestMotes." + chapter;
        public int TotalMotes { get; set; }
        public float Elapsed { get; private set; }
        public float Farthest { get; private set; }
        public bool WallClimbUnlocked => Chapter >= 3;
        public CuoiController player { get; private set; }
        public MoonlitCamera cameraRig { get; private set; }
        public MoonlitLevel level { get; private set; }
        public MoonlitEffects effects { get; private set; }
        public MoonlitAudio audioFx { get; private set; }
        public MoonlitHud hud { get; private set; }
        public ForestWind wind { get; private set; }
        public LanternDarkness darkness { get; private set; }
        public MoonlitTutorial tutorials { get; private set; }
        public BanyanGuardian Boss { get; private set; }
        public readonly List<ForestCreature> enemies = new List<ForestCreature>();
        public Vector2 RespawnPoint { get; private set; }
        int best;
        int storyExitFrame = -1;
        public bool InStory => State == GameState.Prologue || State == GameState.Reunion;
        float respawnTimer;
        bool movedToCheckpoint, testing;

        bool showRequestedOpening, editorPreview;

        public static void EnterHomeScene(MoonlitHomeScene home)
        {
            var game = GetSession(home.art, home.tuning);
            game.ReleaseGameplay();
            game.ChapterSource = null;
            if (game.editorPreview && !game.testing)
                game.best = Mathf.Clamp(PlayerPrefs.GetInt(ProgressKey, 0), 0, 4);
            game.editorPreview = false;
            game.Chapter = 1;
            game.Shards = game.Motes = game.CheckpointIndex = 0;
            game.Health = 3;
            game.State = GameState.Home;
            game.ViewCamera.transform.position = new Vector3(0,0,-10);
            game.ViewCamera.orthographicSize = 6.75f;
            game.hud.SetFade(0); game.hud.RefreshState();
        }

        public static void EnterScene(MoonlitChapterScene source)
        {
            bool directEntry = !Instance;
            var game = GetSession(source.art, source.tuning);
            game.ChapterSource = source;
            if (directEntry && Application.isEditor)
            {
                game.editorPreview = true;
                game.best = Mathf.Max(game.best, source.chapter - 1);
            }
            game.BeginChapter(!directEntry && game.showRequestedOpening);
        }

        static MoonlitGame GetSession(MoonlitArt art, PlayerTuning tuning)
        {
            if (Instance) return Instance;
            var root = new GameObject("Campaign Session");
            var game = root.AddComponent<MoonlitGame>();
            Instance = game;
            DontDestroyOnLoad(root);
            game.art = art;
            game.tuning = tuning;
            game.InitializeSession();
            return game;
        }

        GameObject SessionObject(string label, Transform parent = null)
        {
            var node = new GameObject(label);
            node.transform.SetParent(parent ? parent : transform, false);
            return node;
        }

        void InitializeSession()
        {
            Time.timeScale = 1;
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
#if UNITY_STANDALONE || UNITY_EDITOR
            var args = Environment.GetCommandLineArgs();
            testing = Array.Exists(args, a => a.StartsWith("-moonlit", StringComparison.OrdinalIgnoreCase) && a.EndsWith("QA", StringComparison.OrdinalIgnoreCase));
#endif
            if (!testing)
            {
                best = Mathf.Clamp(PlayerPrefs.GetInt(ProgressKey, 0), 0, 4);
                for (int i = 0; i < bestMotes.Length; i++)
                    bestMotes[i] = Mathf.Clamp(PlayerPrefs.GetInt(MoteRecordKey(i + 1), 0), 0, MoonlitLevel.MotesPerChapter);
            }
            Controls = new MoonlitControls(!testing && !Application.isBatchMode);
            Visuals.Init(art.spriteMaterial);
            audioFx = gameObject.AddComponent<MoonlitAudio>();
            var cam = SessionObject("Main Camera");
            cam.tag = "MainCamera";
            ViewCamera = cam.AddComponent<Camera>();
            ViewCamera.orthographic = true; ViewCamera.orthographicSize = 6.75f;
            ViewCamera.clearFlags = CameraClearFlags.SolidColor;
            ViewCamera.backgroundColor = new Color(.018f,.04f,.075f);
            ViewCamera.nearClipPlane = .1f; ViewCamera.farClipPlane = 100;
            ViewCamera.transform.position = new Vector3(0,0,-10);
            cam.AddComponent<AudioListener>();
            tutorials = gameObject.AddComponent<MoonlitTutorial>();
            tutorials.Initialize(this);
            hud = SessionObject("Game Interface").AddComponent<MoonlitHud>();
            hud.Build(this);
            var input = UnityEngine.EventSystems.EventSystem.current;
            if (!input) input = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (input) input.transform.SetParent(transform, false);
#if UNITY_STANDALONE || UNITY_EDITOR
            if (testing) gameObject.AddComponent<MoonlitCampaignPlaytest>();
#endif
        }

        void PrepareGameplay()
        {
            if (gameplayActors) return;
            gameplayActors = SessionObject("Gameplay Actors");
            Transform owner = gameplayActors.transform;
            effects = SessionObject("Fireflies and Lantern Sparks",owner).AddComponent<MoonlitEffects>();
            effects.Populate();
            var p = SessionObject("Cuoi",owner);
            p.layer = 9;
            player = p.AddComponent<CuoiController>();
            player.body = Visuals.Sprite("Cuoi Sprite", art.Hero(0,0), Vector2.zero, p.transform, 10);
            player.lanternGlow = Visuals.Halo("Star Lantern", new Vector2(.55f,.85f), 5.4f, new Color(1,.62f,.22f,.52f), p.transform,14);
            player.Initialize(this);
            p.AddComponent<LanternRecoveryMeter>().Initialize(this);
            cameraRig = ViewCamera.gameObject.AddComponent<MoonlitCamera>();
            cameraRig.target = player; cameraRig.Initialize(art.forest);
            wind = SessionObject("Wind and Leaves",owner).AddComponent<ForestWind>();
            wind.Initialize(this);
            darkness = SessionObject("Lantern Lighting",owner).AddComponent<LanternDarkness>();
            darkness.Initialize(this);
        }

        void ReleaseGameplay()
        {
            if (gameplayActors)
            {
                gameplayActors.SetActive(false);
                Destroy(gameplayActors);
            }
            if (cameraRig) { cameraRig.enabled = false; Destroy(cameraRig); }
            enemies.Clear();
            level = null; Boss = null; player = null; wind = null;
            effects = null; darkness = null; gameplayActors = null; cameraRig = null;
        }

        void CreateLevel()
        {
            if (level)
            {
                level.gameObject.SetActive(false);
                Destroy(level.gameObject);
            }
            Boss = null;
            enemies.Clear();
            TotalMotes = 0;
            var world = new GameObject("Chapter World");
            world.transform.SetParent(ChapterSource.transform, false);
            level = world.AddComponent<MoonlitLevel>();
            level.Build(this);
            RespawnPoint = level.StartPoint;
            cameraRig.SetChapter(Chapter);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && !hud.CapturesMenuInput && !hud.ButtonActivatedThisFrame)
            {
                if (Controls.Pressed(GameAction.Pause, kb) || ((State == GameState.Map || State == GameState.Paused) && kb.escapeKey.wasPressedThisFrame))
                {
                    if (State == GameState.Map) ToggleMap();
                    else TogglePause();
                }
                else if (Controls.Pressed(GameAction.Map, kb)) ToggleMap();
                if (Controls.Pressed(GameAction.Sound, kb)) audioFx.Toggle();
            }
            if (State == GameState.Playing)
            {
                Elapsed += Time.deltaTime;
                Farthest = Mathf.Max(Farthest, player.Body.position.x);
            }
            else if (State == GameState.Respawning)
            {
                respawnTimer -= Time.deltaTime;
                hud.SetFade(Mathf.Sin(Mathf.Clamp01(1 - respawnTimer / .65f) * Mathf.PI) * .87f);
                if (!movedToCheckpoint && respawnTimer <= .325f)
                {
                    movedToCheckpoint = true;
                    ReturnToCheckpoint();
                }
                if (respawnTimer <= 0) { State = GameState.Playing; hud.SetFade(0); hud.RefreshState(); }
            }
        }

        void LateUpdate()
        {
            if (player) TerrainArt.LightAt(player.LanternPosition, player.PulseFlash);
            var kb = Keyboard.current;
            if (InStory || Time.frameCount == storyExitFrame || kb == null || hud.ButtonActivatedThisFrame || hud.BlocksDefaultSubmit ||
                !(kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)) return;
            if (State == GameState.Home) hud.OpenPicker();
            else if (State == GameState.Complete) NextChapter();
            else if (State == GameState.Defeat) RetryCheckpoint();
            else if (State == GameState.Ending) Home();
            else if (State == GameState.Paused) TogglePause();
            else if (State == GameState.Tutorial) tutorials.Continue();
        }

        public void StartRun() => StartChapter(1);
        public void ContinueJourney() => StartChapter(FurthestChapter);
        public void RestartChapter() => StartChapter(Chapter,false);
        public void NextChapter() { if (State == GameState.Complete && Chapter < 4) StartChapter(Chapter + 1); }

        public void StartChapter(int chapter, bool showOpening = true)
        {
            if (IsLoadingChapter || chapter < 1 || chapter > FurthestChapter) return;
            if (!ChapterSource || chapter != ChapterSource.chapter)
            {
                showRequestedOpening = showOpening;
                Load(MoonlitChapterScene.SceneNames[chapter - 1]);
                return;
            }
            BeginChapter(showOpening);
        }

        void Load(string sceneName)
        {
            tutorials.Cancel();
            Time.timeScale = 1;
            State = GameState.Loading;
            if (player) player.ClearInput();
            if (cameraRig) cameraRig.enabled = false;
            int next = Array.IndexOf(MoonlitChapterScene.SceneNames, sceneName) + 1;
            hud.SetFade(0); hud.ShowLoading(next); hud.RefreshState();
            StartCoroutine(LoadWithScreen(sceneName));
        }

        System.Collections.IEnumerator LoadWithScreen(string sceneName)
        {
            // Submit the overlay before any asset or scene activation work can block this frame.
            yield return null;
            var request = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            request.allowSceneActivation = false;
            float shown = Time.unscaledTime;
            while (request.progress < .9f || Time.unscaledTime - shown < .32f)
            {
                hud.SetLoadingProgress(request.progress / .9f);
                yield return null;
            }
            hud.SetLoadingProgress(1);
            yield return null;
            request.allowSceneActivation = true;
        }

        void BeginChapter(bool showOpening)
        {
            int chapter = ChapterSource.chapter;
            tutorials.ResetForRun();
            Time.timeScale = 1;
            Chapter = chapter;
            Shards = Chapter - 1;
            State = chapter == 1 && showOpening ? GameState.Prologue : GameState.Playing;
            Health = 3;
            Deaths = Motes = CheckpointIndex = 0;
            Elapsed = Farthest = 0;
            PrepareGameplay();
            cameraRig.enabled = true;
            CreateLevel();
            player.ResetRun(); player.Teleport(RespawnPoint); player.ClearInput(.18f);
            cameraRig.Snap(); wind.ResetWind();
            hud.SetFade(0); hud.RefreshState();
        }

        public void ToggleMap()
        {
            if (State != GameState.Playing && State != GameState.Map) return;
            bool opening = State == GameState.Playing;
            State = opening ? GameState.Map : GameState.Playing;
            Time.timeScale = opening ? 0 : 1;
            player.ClearInput(opening ? 0 : .1f);
            hud.RefreshState();
        }

        public void TogglePause()
        {
            if (State == GameState.Playing) { State = GameState.Paused; Time.timeScale = 0; }
            else if (State == GameState.Paused) { State = GameState.Playing; Time.timeScale = 1; player.ClearInput(.1f); }
            else return;
            hud.RefreshState();
        }
        public void Home()
        {
            if (!IsLoadingChapter && State != GameState.Home) Load(MoonlitHomeScene.SceneName);
        }
        public void BeginTutorial()
        {
            State = GameState.Tutorial; Time.timeScale = 0; player.ClearInput();
            player.Body.linearVelocity = Vector2.zero; hud.RefreshState();
        }
        public void EndTutorial()
        {
            if (State != GameState.Tutorial) return;
            State = GameState.Playing; Time.timeScale = 1; player.ClearInput(.16f);
            player.Invulnerable = Mathf.Max(player.Invulnerable, .65f); hud.RefreshState();
        }
        public void Hurt(bool falling = false, float direction = 0)
        {
            if (State != GameState.Playing || (!falling && player.Invulnerable > 0)) return;
            Health--;
            audioFx.Play("hurt"); effects.Burst(player.transform.position + Vector3.up, new Color(1, .5f, .35f), 22, 3);
            cameraRig.Shake(.18f, .3f);
            if (falling || Health == 0) Deaths++;
            if (Health == 0)
            {
                State = GameState.Defeat; audioFx.Play("lose"); hud.RefreshState();
            }
            else if (falling)
            {
                State = GameState.Respawning; respawnTimer = .65f; movedToCheckpoint = false; hud.RefreshState();
            }
            else player.ApplyKnockback(direction == 0 ? (player.FacingRight ? -1 : 1) : direction);
        }

        void ReturnToCheckpoint()
        {
            level.ResetLevel(false);
            if (Boss) Boss.Regroup();
            player.Teleport(RespawnPoint); player.Invulnerable = 1.2f; player.ClearInput(.1f);
            cameraRig.Snap();
        }

        public void RetryCheckpoint()
        {
            if (State != GameState.Defeat) return;
            Health = 3; State = GameState.Playing; Time.timeScale = 1;
            ReturnToCheckpoint(); hud.SetFade(0); hud.RefreshState();
        }
        public void SetCheckpoint(int index, Vector2 point)
        {
            if (index <= CheckpointIndex) return;
            CheckpointIndex = index; RespawnPoint = point; audioFx.Play("checkpoint");
            Toast("ĐÈN ĐÃ THẮP", "Cuội sẽ trở lại cột đèn này nếu ngã.");
        }
        public void Interact()
        {
            if (State != GameState.Playing) return;
            if (level.Ferry && level.Ferry.Interact(player.Body.position)) return;
            if (level.lanternSwitch) level.lanternSwitch.Activate();
        }

        public void CollectMote()
        {
            Motes++;
            if (!editorPreview && Motes > BestMotes(Chapter))
            {
                bestMotes[Chapter - 1] = Motes;
                if (!testing) SaveValue(MoteRecordKey(Chapter), Motes);
            }
            if (Motes == TotalMotes) Toast("ĐỦ MƯỜI HẠT TRĂNG", $"Cuội đã tìm thấy cả mười hạt ở {LevelTitle}.");
        }

        public bool Complete()
        {
            if (State != GameState.Playing || Shards >= Chapter) return false;
            Shards = Chapter;
            effects.Ring(player.transform.position + Vector3.up, Visuals.Mint, 9);
            effects.Burst(player.transform.position + Vector3.up * 2, Visuals.Gold, 64, 6);
            audioFx.Play("win");
            if (Chapter == 4)
            {
                Health = 3;
                RespawnPoint = new Vector2(level.BossFoot.x - 17f, level.BossFoot.y + .1f);
                Boss = BanyanGuardian.Begin(this, level.BossFoot, FinishCampaign);
                Toast("ĐỦ BỐN MẢNH TRĂNG", "Chờ nụ trên thân cây mở rồi đến gần và bùng đèn.");
            }
            else { State = GameState.Complete; SaveProgress(Chapter); }
            hud.RefreshState();
            return true;
        }
        void FinishCampaign()
        {
            if (State != GameState.Playing || Chapter != 4 || Shards != 4) return;
            State = GameState.Reunion; SaveProgress(4); audioFx.Play("win");
            effects.Burst(player.transform.position + Vector3.up * 3, Visuals.Gold, 86, 6); hud.RefreshState();
        }
        public void EndStory()
        {
            if (!InStory) return;
            storyExitFrame = Time.frameCount;
            State = State == GameState.Prologue ? GameState.Playing : GameState.Ending;
            player.ClearInput(.18f);
            hud.RefreshState();
        }
        void SaveProgress(int completed)
        {
            if (completed <= best) return;
            best = completed;
            if (testing || editorPreview) return;
            SaveValue(ProgressKey, best);
        }

        static void SaveValue(string key, int value)
        {
            try { PlayerPrefs.SetInt(key, value); PlayerPrefs.Save(); }
            catch (PlayerPrefsException) { Debug.LogWarning("Moonlit: progress is available for this session only."); }
        }
        public void Toast(string title, string description) { if (hud) hud.ShowToast(title, description); }
        void OnDestroy() { Time.timeScale = 1; if (Instance == this) Instance = null; }
    }
}
