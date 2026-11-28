using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Moonlit.Editor
{
    // Batch-only regression for pressing Play from any chapter in the Project window.
    [InitializeOnLoad]
    public static class ChapterEntryReview
    {
        const string Key = "Moonlit.VerifyChapterEntry.";
        const string Progress = "Moonlit.Campaign.ShardsRestored";
        const string Output = "QA/FinalPass/editor-direct-entry.json";

        [Serializable] class Row
        {
            public int chapter, banks, platforms, checkpoints, cameras, inputSystems, listeners, sessions, runtimeErrors;
            public string scene, initialState, previewCompletion;
            public bool worldInScene, servicesPersistent, progressUnchanged, passed;
            public float walkingDistance;
        }
        [Serializable] class Report
        {
            public string unity, error;
            public bool passed;
            public Row home;
            public List<Row> chapters = new List<Row>();
        }

        static ChapterEntryReview()
        {
            if (!Application.isBatchMode || !Environment.GetCommandLineArgs().Contains("-checkChapterEntry")) return;
            Application.logMessageReceived += (message,stack,type) => {
                if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
                if (!SessionState.GetBool(Key+"Active",false)) return;
                if (System.Text.RegularExpressions.Regex.IsMatch(stack, @"\bMoonlit\.(?!Editor\.)"))
                    SessionState.SetInt(Key+"Errors", SessionState.GetInt(Key+"Errors",0)+1);
            };
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key+"Active",false))
                    EditorApplication.delayCall += OpenNext;
            };
        }

        public static void Run()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Output));
            var report = new Report { unity = Application.unityVersion };
            File.WriteAllText(Output, JsonUtility.ToJson(report, true));
            SessionState.SetInt(Key+"Index", 0);
            SessionState.SetInt(Key+"Prefs", PlayerPrefs.GetInt(Progress,int.MinValue));
            SessionState.SetBool(Key+"Active", true);
            OpenNext();
        }

        static Report Read() => JsonUtility.FromJson<Report>(File.ReadAllText(Output));
        static void Finish(Report report, string error = null)
        {
            SessionState.SetBool(Key+"Active", false);
            report.error = error;
            report.passed = error == null && report.home != null && report.home.passed && report.chapters.Count == 4 && report.chapters.All(c => c.passed);
            File.WriteAllText(Output, JsonUtility.ToJson(report, true));
            Debug.Log("CHAPTER_ENTRY_DONE " + report.passed + " " + error);
            EditorApplication.Exit(report.passed ? 0 : 1);
        }

        static void OpenNext()
        {
            int i = SessionState.GetInt(Key+"Index",0);
            if (i > MoonlitChapterScene.SceneNames.Length) { Finish(Read()); return; }
            try {
                var name = i == 0 ? MoonlitHomeScene.SceneName : MoonlitChapterScene.SceneNames[i-1];
                var path = "Assets/Moonlit/Scenes/" + name + ".unity";
                SessionState.SetInt(Key+"Errors", 0);
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                SessionState.SetBool(Key+"Exiting", false);
                SessionState.SetBool(Key+"Walking", false);
                SessionState.SetInt(Key+"HomeStep", 0);
                SessionState.SetFloat(Key+"Timeout", (float)EditorApplication.timeSinceStartup + 100);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception e) { Finish(Read(), e.ToString()); }
        }

        static void Tick()
        {
            if (!SessionState.GetBool(Key+"Active", false)) return;
            try {
                if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Key+"Timeout",float.MaxValue))
                    throw new TimeoutException("Timed out at direct chapter " + (SessionState.GetInt(Key+"Index",0)+1));
                if (!EditorApplication.isPlaying || SessionState.GetBool(Key+"Exiting",false)) return;
                EditorApplication.QueuePlayerLoopUpdate();
                var game = MoonlitGame.Instance;
                if (!game || !game.hud) return;
                int number = SessionState.GetInt(Key+"Index",0);
                if (number == 0) { CheckHome(game); return; }
                if (!SessionState.GetBool(Key+"Walking",false))
                {
                    SessionState.SetString(Key+"State",game.State.ToString());
                    game.tutorials.Enabled = false;
                    SessionState.SetFloat(Key+"StartX",game.player.Body.position.x);
                    game.player.TestInput = true; game.player.TestMove = 1;
                    SessionState.SetBool(Key+"Walking",true);
                    return;
                }
                float delta = game.player.Body.position.x - SessionState.GetFloat(Key+"StartX",0);
                if (delta < .3f) return;
                game.player.TestMove = 0;
                var scene = SceneManager.GetActiveScene();
                var persistent = game.gameObject.scene;
                var source = game.ChapterSource;
                var row = new Row {
                    chapter=number, scene=scene.path, initialState=SessionState.GetString(Key+"State",""),
                    walkingDistance=delta, banks=source.banks.Length, platforms=source.platforms.Length,
                    checkpoints=source.checkpoints.Length,
                    sessions = Count<MoonlitGame>(scene,persistent), cameras=Count<Camera>(scene,persistent),
                    listeners=Count<AudioListener>(scene,persistent), inputSystems=Count<EventSystem>(scene,persistent),
                    worldInScene=source.gameObject.scene==scene && source.chapter==number && game.level.gameObject.scene==scene &&
                        source.banks.All(b=>b && b.gameObject.scene==scene) && source.platforms.All(b=>b && b.gameObject.scene==scene) &&
                        source.checkpoints.All(t=>t && t.gameObject.scene==scene) && game.level.transform.IsChildOf(source.transform),
                    servicesPersistent=persistent!=scene && game.cameraRig.gameObject.scene==persistent &&
                        EventSystem.current && EventSystem.current.gameObject.scene==persistent &&
                        game.hud.gameObject.scene==persistent && game.player.gameObject.scene==persistent
                };
                bool playable = game.State==GameState.Playing && game.Chapter==number && game.Health==3 && game.Shards==number-1;
                game.Complete();
                row.previewCompletion = game.Boss ? "Guardian active" : game.State.ToString();
                row.progressUnchanged = PlayerPrefs.GetInt(Progress,int.MinValue) == SessionState.GetInt(Key+"Prefs",int.MinValue);
                row.runtimeErrors = SessionState.GetInt(Key+"Errors",0);
                row.passed = row.runtimeErrors == 0 && playable && row.worldInScene && row.servicesPersistent && row.progressUnchanged &&
                    row.cameras==1 && row.sessions==1 && row.listeners==1 && row.inputSystems==1 &&
                    row.checkpoints==6 && row.banks>0 && row.platforms>0 &&
                    row.initialState=="Playing" && scene.name==MoonlitChapterScene.SceneNames[number-1] &&
                    (number==4 ? game.Boss && game.Boss.Active : game.State==GameState.Complete);
                var report=Read(); report.chapters.Add(row);
                File.WriteAllText(Output,JsonUtility.ToJson(report,true));
                Debug.Log("CHAPTER_ENTRY " + scene.name + " " + row.passed);
                SessionState.SetInt(Key+"Index",number+1);
                SessionState.SetBool(Key+"Exiting",true);
                EditorApplication.ExitPlaymode();
            }
            catch (Exception e) { Finish(Read(),e.ToString()); }
        }

        static void CheckHome(MoonlitGame game)
        {
            if (game.IsLoadingChapter) return;
            int phase=SessionState.GetInt(Key+"HomeStep",0);
            var scene=SceneManager.GetActiveScene();
            if (phase==1)
            {
                if (scene.name!=MoonlitChapterScene.SceneNames[0] || game.State!=GameState.Prologue || !game.player || !game.level)
                    throw new InvalidOperationException("Starting from Home did not enter the first scene with the prologue.");
                game.EndStory();
                if (game.State!=GameState.Playing) throw new InvalidOperationException("Prologue did not hand control to the player.");
                SessionState.SetInt(Key+"HomeStep",2);
                game.Home();
                return;
            }
            var persistent=game.gameObject.scene;
            var home=UnityEngine.Object.FindFirstObjectByType<MoonlitHomeScene>();
            bool noGameplay=!game.player && !game.level && !game.ChapterSource && !game.Boss && !game.wind &&
                !game.effects && !game.darkness && !game.cameraRig && Count<CuoiController>(scene,persistent)==0 &&
                Count<MoonlitLevel>(scene,persistent)==0;
            bool nativeTitle=home && home.gameObject.scene==scene && home.backdrop && home.backdrop.sprite && home.backdrop.enabled &&
                home.backdrop.sprite.texture && home.backdrop.sprite.texture.name=="VillageUnderBrokenMoon";
            if (scene.name!=MoonlitHomeScene.SceneName || game.State!=GameState.Home || !noGameplay || !nativeTitle)
                throw new InvalidOperationException("Home is missing its native title image or still owns chapter actors.");
            if (phase==0)
            {
                SessionState.SetInt(Key+"HomeStep",1);
                game.StartRun();
                return;
            }
            var report=Read();
            var row = new Row {chapter=0,scene=scene.path,initialState="Home",previewCompletion="Home → Prologue → Playing → Home",
                cameras=Count<Camera>(scene,persistent), sessions=Count<MoonlitGame>(scene,persistent),
                listeners=Count<AudioListener>(scene,persistent),inputSystems=Count<EventSystem>(scene,persistent),
                worldInScene=nativeTitle,servicesPersistent=persistent!=scene && game.ViewCamera.gameObject.scene==persistent &&
                    EventSystem.current && EventSystem.current.gameObject.scene==persistent,
                progressUnchanged=PlayerPrefs.GetInt(Progress,int.MinValue)==SessionState.GetInt(Key+"Prefs",int.MinValue),
                runtimeErrors=SessionState.GetInt(Key+"Errors",0)};
            row.passed=noGameplay && row.servicesPersistent && row.progressUnchanged && row.cameras==1 && row.sessions==1 &&
                row.listeners==1 && row.inputSystems==1 && row.runtimeErrors==0;
            report.home=row;
            File.WriteAllText(Output, JsonUtility.ToJson(report,true));
            Debug.Log("CHAPTER_ENTRY " + scene.name + " " + row.passed);
            SessionState.SetInt(Key+"Index",1);
            SessionState.SetBool(Key+"Exiting",true);
            EditorApplication.ExitPlaymode();
        }

        static int Count<T>(Scene active, Scene persistent) where T : Behaviour =>
            UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                .Count(c => c.enabled && (c.gameObject.scene==active || c.gameObject.scene==persistent));
    }
}
