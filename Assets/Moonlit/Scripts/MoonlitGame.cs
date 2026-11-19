using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonlit
{
    public enum GameState { Home, Playing, Paused, Respawning, Complete, Tutorial }

    public sealed class MoonlitGame : MonoBehaviour
    {
        public static MoonlitGame Instance { get; private set; }
        public MoonlitArt art;
        public PlayerTuning tuning;
        public GameState State { get; private set; } = GameState.Home;
        public int Health { get; private set; } = 3;
        public int Deaths { get; private set; }
        public int CheckpointIndex { get; private set; }
        public int Motes { get; set; }
        public int TotalMotes { get; set; }
        public float Elapsed { get; private set; }
        public float Farthest { get; private set; }
        public bool WallClimbUnlocked { get; private set; }
        public CuoiController player { get; private set; }
        public MoonlitCamera cameraRig { get; private set; }
        public MoonlitLevel level { get; private set; }
        public MoonlitEffects effects { get; private set; }
        public MoonlitAudio audioFx { get; private set; }
        public MoonlitHud hud { get; private set; }
        public ForestWind wind { get; private set; }
        public LanternDarkness darkness { get; private set; }
        public MoonlitTutorial tutorials { get; private set; }
        public readonly List<ForestCreature> enemies = new List<ForestCreature>();
        public Vector2 RespawnPoint { get; private set; } = new Vector2(4, .12f);
        float respawnTimer;

        void Awake()
        {
            Instance = this; Time.timeScale = 1;
            Application.targetFrameRate = 120; QualitySettings.vSyncCount = 1;
            Visuals.Init(art.spriteMaterial);
            audioFx = gameObject.AddComponent<MoonlitAudio>();
            effects = new GameObject("Ambient fireflies and feedback").AddComponent<MoonlitEffects>(); effects.Populate(MoonlitLevel.Length);
            var p = new GameObject("Cuoi - player"); p.layer = 9; p.transform.position = RespawnPoint;
            player = p.AddComponent<CuoiController>();
            player.body = Visuals.Sprite("Cuoi animated body", art.Hero(0, 0), RespawnPoint, p.transform, 10);
            player.lanternGlow = Visuals.Halo("Lantern circular light", RespawnPoint + new Vector2(.55f, .85f), 5.4f, new Color(1, .62f, .22f, .52f), p.transform, 14);
            player.Initialize(this);
            var cam = new GameObject("Main Camera"); cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>(); c.orthographic = true; c.orthographicSize = 6.75f; c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(.018f, .04f, .075f); c.nearClipPlane = .1f; c.farClipPlane = 100;
            cam.AddComponent<AudioListener>(); cameraRig = cam.AddComponent<MoonlitCamera>(); cameraRig.target = player; cameraRig.Initialize(art.forest);
            level = new GameObject("LEVEL 01 - Banyan forest").AddComponent<MoonlitLevel>(); level.Build(this);
            hud = new GameObject("Moonlit UI").AddComponent<MoonlitHud>(); hud.Build(this);
            wind = new GameObject("Forest wind and horizontal leaves").AddComponent<ForestWind>(); wind.Initialize(this);
            darkness = new GameObject("Lantern darkness shader mask").AddComponent<LanternDarkness>(); darkness.Initialize(this);
            tutorials = gameObject.AddComponent<MoonlitTutorial>(); tutorials.Initialize(this);
            if (System.Array.Exists(System.Environment.GetCommandLineArgs(), a => a == "-moonlitQA")) gameObject.AddComponent<MoonlitPlaytest>();
            if (System.Array.Exists(System.Environment.GetCommandLineArgs(), a => a == "-moonlitRouteQA")) gameObject.AddComponent<MoonlitRoutePlaytest>();
            if (System.Array.Exists(System.Environment.GetCommandLineArgs(), a => a == "-moonlitPolishQA")) gameObject.AddComponent<MoonlitPolishPlaytest>();
            if (System.Array.Exists(System.Environment.GetCommandLineArgs(), a => a == "-moonlitRevisionQA")) gameObject.AddComponent<MoonlitRevisionPlaytest>();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.escapeKey.wasPressedThisFrame) TogglePause();
                if (kb.mKey.wasPressedThisFrame) audioFx.Toggle();
                if (kb.enterKey.wasPressedThisFrame)
                {
                    if (State == GameState.Home || State == GameState.Complete) StartRun();
                    else if (State == GameState.Paused) TogglePause();
                }
            }
            if (State == GameState.Playing)
            {
                Elapsed += Time.deltaTime; Farthest = Mathf.Max(Farthest, player.transform.position.x);
            }
            else if (State == GameState.Respawning)
            {
                respawnTimer -= Time.deltaTime;
                hud.SetFade(Mathf.Sin(Mathf.Clamp01(1 - respawnTimer / .65f) * Mathf.PI) * .8f);
                if (respawnTimer < .32f && respawnTimer + Time.deltaTime >= .32f)
                {
                    level.ResetLevel(false); player.Teleport(RespawnPoint); cameraRig.Snap();
                }
                if (respawnTimer <= 0) { State = GameState.Playing; player.Invulnerable = 1.5f; hud.SetFade(0); }
            }
        }

        public void StartRun()
        {
            tutorials.ResetForRun(); Time.timeScale = 1; State = GameState.Playing; Health = 3; Deaths = Motes = CheckpointIndex = 0;
            Elapsed = Farthest = 0; RespawnPoint = new Vector2(4, .12f);
            level.ResetLevel(true); player.ResetRun(); player.Teleport(RespawnPoint); cameraRig.Snap();
            wind.ResetWind();
            hud.RefreshState();
        }
        public void TogglePause()
        {
            if (State == GameState.Playing) { State = GameState.Paused; Time.timeScale = 0; }
            else if (State == GameState.Paused) { State = GameState.Playing; Time.timeScale = 1; }
            hud.RefreshState();
        }
        public void Home()
        {
            tutorials.Cancel();
            Time.timeScale = 1; State = GameState.Home; player.Teleport(new Vector2(4, .12f)); cameraRig.Snap(); hud.RefreshState();
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
        public void Hurt(bool falling = false)
        {
            if (State != GameState.Playing || (!falling && player.Invulnerable > 0)) return;
            Deaths++; Health--; if (Health <= 0) Health = 3;
            audioFx.Play("hurt"); effects.Burst(player.transform.position + Vector3.up, new Color(1, .5f, .35f), 22, 3);
            cameraRig.Shake(.18f, .3f); State = GameState.Respawning; respawnTimer = .65f;
        }
        public void SetCheckpoint(int index, Vector2 point)
        {
            CheckpointIndex = index; RespawnPoint = point; Health = 3; audioFx.Play("checkpoint");
            Toast("ĐÃ THẮP ĐÈN", "Điểm hồi sinh được lưu. Cuội hồi đầy sinh lực.");
        }
        public void Interact() { if (State == GameState.Playing) level.lanternSwitch.Activate(); }
        public void Complete()
        {
            if (State != GameState.Playing) return;
            State = GameState.Complete; effects.Ring(player.transform.position + Vector3.up, Visuals.Mint, 9);
            effects.Burst(player.transform.position + Vector3.up * 2, Visuals.Gold, 90, 6); audioFx.Play("win");
            WallClimbUnlocked = true; hud.RefreshState();
            try
            {
                PlayerPrefs.SetInt("Moonlit.Level1Complete", 1); PlayerPrefs.SetInt("Moonlit.WallClimbUnlocked", 1); PlayerPrefs.Save();
            }
            catch (PlayerPrefsException)
            {
                Debug.LogWarning("Moonlit: progress could not be saved on this account; the level is still complete for this session.");
            }
        }
        public void Toast(string title, string description) { hud.ShowToast(title, description); }
        void OnDestroy() { Time.timeScale = 1; if (Instance == this) Instance = null; }
    }
}
