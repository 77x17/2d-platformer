using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonlit
{
    public enum GameState { Home, Playing, Paused, Respawning, Complete, Tutorial, Defeat, Ending }

    public sealed class MoonlitGame : MonoBehaviour
    {
        const string ProgressKey = "Moonlit.Campaign.ShardsRestored";
        static readonly string[] Names = { "Bờ Tre Làng Cuội", "Rừng Trúc", "Sông Đèn", "Đồi Gốc Đa" };
        public static MoonlitGame Instance { get; private set; }
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
        public int Motes { get; set; }
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
        float respawnTimer;
        bool movedToCheckpoint, testing;

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1;
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
            var args = Environment.GetCommandLineArgs();
            testing = Array.Exists(args, a => a.StartsWith("-moonlit", StringComparison.OrdinalIgnoreCase) && a.EndsWith("QA", StringComparison.OrdinalIgnoreCase));
            if (!testing) best = Mathf.Clamp(PlayerPrefs.GetInt(ProgressKey, 0), 0, 4);
            Visuals.Init(art.spriteMaterial);
            audioFx = gameObject.AddComponent<MoonlitAudio>();
            effects = new GameObject("Fireflies and lantern sparks").AddComponent<MoonlitEffects>();
            effects.Populate();
            var p = new GameObject("Cuội");
            p.layer = 9;
            player = p.AddComponent<CuoiController>();
            player.body = Visuals.Sprite("Cuội", art.Hero(0, 0), Vector2.zero, p.transform, 10);
            player.lanternGlow = Visuals.Halo("Đèn ông sao", new Vector2(.55f, .85f), 5.4f, new Color(1, .62f, .22f, .52f), p.transform, 14);
            player.Initialize(this);
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>();
            c.orthographic = true; c.orthographicSize = 6.75f; c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(.018f, .04f, .075f); c.nearClipPlane = .1f; c.farClipPlane = 100;
            cam.AddComponent<AudioListener>();
            cameraRig = cam.AddComponent<MoonlitCamera>();
            cameraRig.target = player;
            cameraRig.Initialize(art.forest);
            CreateLevel();
            player.Teleport(RespawnPoint);
            cameraRig.Snap();
            wind = new GameObject("Gió và lá").AddComponent<ForestWind>();
            wind.Initialize(this);
            darkness = new GameObject("Ánh đèn").AddComponent<LanternDarkness>();
            darkness.Initialize(this);
            tutorials = gameObject.AddComponent<MoonlitTutorial>();
            tutorials.Initialize(this);
            hud = new GameObject("Giao diện").AddComponent<MoonlitHud>();
            hud.Build(this);
            if (testing) gameObject.AddComponent<MoonlitCampaignPlaytest>();
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
            level = new GameObject(LevelTitle).AddComponent<MoonlitLevel>();
            level.Build(this);
            RespawnPoint = level.StartPoint;
            cameraRig.SetChapter(Chapter);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.escapeKey.wasPressedThisFrame) TogglePause();
                if (kb.mKey.wasPressedThisFrame) audioFx.Toggle();
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
            TerrainArt.LightAt(player.LanternPosition, player.PulseFlash);
            var kb = Keyboard.current;
            if (kb == null || hud.ButtonActivatedThisFrame || hud.BlocksDefaultSubmit ||
                !(kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)) return;
            if (State == GameState.Home) ContinueJourney();
            else if (State == GameState.Complete) NextChapter();
            else if (State == GameState.Defeat) RetryCheckpoint();
            else if (State == GameState.Ending) Home();
            else if (State == GameState.Paused) TogglePause();
            else if (State == GameState.Tutorial) tutorials.Continue();
        }

        public void StartRun() => StartChapter(1);
        public void ContinueJourney() => StartChapter(FurthestChapter);
        public void RestartChapter() => StartChapter(Chapter);
        public void NextChapter() { if (State == GameState.Complete && Chapter < 4) StartChapter(Chapter + 1); }

        public void StartChapter(int chapter)
        {
            if (chapter < 1 || chapter > FurthestChapter) return;
            tutorials.ResetForRun();
            Time.timeScale = 1;
            Chapter = chapter;
            Shards = Chapter - 1;
            State = GameState.Playing;
            Health = 3;
            Deaths = Motes = CheckpointIndex = 0;
            Elapsed = Farthest = 0;
            CreateLevel();
            player.ResetRun(); player.Teleport(RespawnPoint); player.ClearInput(.18f);
            cameraRig.Snap(); wind.ResetWind();
            hud.SetFade(0); hud.RefreshState();
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
            tutorials.Cancel(); Time.timeScale = 1; State = GameState.Home;
            if (Boss) { Boss.gameObject.SetActive(false); Destroy(Boss.gameObject); Boss = null; }
            player.Teleport(level.StartPoint); cameraRig.Snap(); hud.SetFade(0); hud.RefreshState();
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
        public void Interact() { if (State == GameState.Playing && level.lanternSwitch) level.lanternSwitch.Activate(); }

        public void Complete()
        {
            if (State != GameState.Playing || Shards >= Chapter) return;
            Shards = Chapter;
            effects.Ring(player.transform.position + Vector3.up, Visuals.Mint, 9);
            effects.Burst(player.transform.position + Vector3.up * 2, Visuals.Gold, 64, 6);
            audioFx.Play("win");
            if (Chapter == 4)
            {
                Health = 3;
                RespawnPoint = new Vector2(level.BossFoot.x - 17f, level.BossFoot.y + .1f);
                Boss = BanyanGuardian.Begin(this, level.BossFoot, FinishCampaign);
                Toast("BỐN MẢNH TRĂNG", "Mang ánh đèn đến bên những rễ đa đang thức.");
            }
            else { State = GameState.Complete; SaveProgress(Chapter); }
            hud.RefreshState();
        }
        void FinishCampaign()
        {
            if (State != GameState.Playing || Chapter != 4 || Shards != 4) return;
            State = GameState.Ending; SaveProgress(4); audioFx.Play("win");
            effects.Burst(player.transform.position + Vector3.up * 3, Visuals.Gold, 86, 6); hud.RefreshState();
        }
        void SaveProgress(int completed)
        {
            if (completed <= best) return;
            best = completed;
            if (testing) return;
            try { PlayerPrefs.SetInt(ProgressKey, best); PlayerPrefs.Save(); }
            catch (PlayerPrefsException) { Debug.LogWarning("Moonlit: progress is available for this session only."); }
        }
        public void Toast(string title, string description) { if (hud) hud.ShowToast(title, description); }
        void OnDestroy() { Time.timeScale = 1; if (Instance == this) Instance = null; }
    }
}
