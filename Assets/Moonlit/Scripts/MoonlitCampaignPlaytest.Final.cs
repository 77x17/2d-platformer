using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        IEnumerator ReviewFinalPass()
        {
            while (game.IsLoadingChapter) yield return null;
            Record("home_is_separate_initial_scene", HomeHasNoChapter(), SceneManager.GetActiveScene().path);
            if (!HomeHasNoChapter()) yield break;
            Capture(game, "home-title");
            yield return ReviewCredits("home", true);
            game.tutorials.Enabled = false;
            game.StartChapter(1, false);
            yield return null;
            var loading = game.hud.transform.Find("Scene loading");
            Record("illustrated_scene_loading", game.IsLoadingChapter && loading && loading.gameObject.activeInHierarchy &&
                loading.GetComponentsInChildren<RawImage>().Any(im=>im.texture), "Home to Bamboo Village");
            if (game.IsLoadingChapter) Capture(game,"loading-village");
            while (game.IsLoadingChapter) yield return null;
            game.player.TestInput = true;

            int[] targets = { 0, 4, 2, 0 };
            for (int chapter = 1; chapter <= 4; chapter++)
            {
                while (game.IsLoadingChapter) yield return null;
                yield return null;
                var active = SceneManager.GetActiveScene();
                var data = game.ChapterSource;
                bool ownsLevel = data && data.chapter == chapter && data.gameObject.scene == active &&
                    game.level.gameObject.scene == active && game.level.transform.IsChildOf(data.transform);
                bool oneScene = FindObjectsByType<MoonlitChapterScene>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length == 1;
                bool correct = active.name == MoonlitChapterScene.SceneNames[chapter-1] && oneScene && ownsLevel &&
                    data.spawn && data.moon && data.boundary && data.checkpoints.Length == 6;
                Record("active_chapter_scene_" + chapter, correct, active.path + ", chapter world owned by scene=" + ownsLevel);
                if (!correct) yield break;
                bool grounded = game.level.ValidatePlacements(out var contacts);
                Record("painted_ground_and_lift_anchors_" + chapter, grounded, contacts);

                if (chapter == 1)
                {
                    var tree = game.level.GetComponentsInChildren<SpriteRenderer>()
                        .Where(s => s.name == "YoungBanyan").OrderBy(s => Mathf.Abs(s.transform.position.x - 174)).FirstOrDefault();
                    Record("village_banyan_visible", tree && tree.enabled, tree ? tree.transform.position.ToString() : "missing");
                    if (tree) yield return PhotographAt(new Vector2(tree.transform.position.x-3,.10f), "village-root-contact");
                    yield return ReviewVillageHoist();
                }
                else if (chapter == 2)
                {
                    var gate = game.level.ChimeGate;
                    yield return PhotographAt(new Vector2(392.1f,14.25f), "forest-east-first");
                    bool first = gate.Struck == 1 && !gate.WestLit && !gate.Open;
                    game.player.QueueJump(); yield return new WaitForSeconds(1.2f);
                    bool held = gate.Struck == 1 && !gate.Open;
                    yield return PhotographAt(new Vector2(376.7f,14.35f), "forest-west-second");
                    yield return new WaitForSeconds(.9f);
                    Record("chimes_either_order",first && held && gate.WestLit && gate.Open,"east first="+first+", repeat stays locked="+held+", open="+gate.Open);
                    var lift = game.level.GetComponentsInChildren<LiftSuspension>()
                        .OrderBy(s => Mathf.Abs(s.transform.position.x-62.3f)).FirstOrDefault();
                    if (lift)
                    {
                        yield return PhotographAt((Vector2)lift.transform.position + Vector2.up*.08f, "forest-lift-attachments");
                        Vector2 from = lift.transform.position;
                        yield return new WaitForSeconds(.85f);
                        bool fixedEyes = lift.CheckAttachments(out var why);
                        Record("suspension_remains_attached_in_motion", Vector2.Distance(from,lift.transform.position) > .10f &&
                            fixedEyes, why ?? $"platform {from:F2} to {lift.transform.position:F2}");
                    }
                    else Record("suspension_remains_attached_in_motion",false,"Missing working forest hoist");
                }
                else if (chapter == 3) yield return PhotographAt(new Vector2(475,4.20f), "river-lock");

                if (chapter == 2 || chapter == 3)
                {
                    string clipName = chapter == 2 ? "amb_canopy" : "amb_river";
                    var source = game.audioFx.GetComponents<AudioSource>()
                        .FirstOrDefault(a => a.clip && a.clip.name == clipName);
                    bool mixed = source && source.loop && source.clip.channels == 2 &&
                        source.volume > .015f && source.mute == game.audioFx.Muted;
                    Record("environment_mix_" + chapter, mixed, source
                        ? $"{clipName}, stereo channels={source.clip.channels}, gain={source.volume:F3}, playing={source.isPlaying}" : "Missing loop");
                }

                yield return ReviewMap(chapter);
                int target = targets[chapter - 1];
                yield return GatherMotes(game, game.TotalMotes - target);
                Record("optional_motes_" + chapter, game.Motes == target && game.TotalMotes == 10 &&
                    game.BestMotes(chapter) == target, $"run={game.Motes}, best={game.BestMotes(chapter)}");
                game.player.Teleport(new Vector2(data.moon.position.x,game.level.EndFloor+.18f));
                game.cameraRig.Snap();
                yield return new WaitForSeconds(.35f);
                bool collected = game.Shards == chapter && (chapter<4 ? game.State==GameState.Complete : game.Boss && game.Boss.Active);
                Record("chapter_fragment_" + chapter, collected, "shards=" + game.Shards);
                if (!collected) yield break;
                if (chapter < 4) game.NextChapter();
            }

            var boss = game.Boss;
            // Position the lantern by each open bud to exercise the natural ending trigger.
            // The combat and avoidance playthrough lives in the independent campaign route.
            game.player.Invulnerable = 180;
            float began = Time.time, previousStep = Time.captureDeltaTime;
            Time.captureDeltaTime = .04f;
            while (game.State == GameState.Playing && Time.time-began < 120)
            {
                if (boss.TargetOpen && game.player.Cooldown <= 0) {
                    game.player.Teleport(boss.TargetPosition - Vector2.up*.9f);
                    game.player.Pulse();
                }
                yield return null;
            }
            Time.captureDeltaTime = previousStep;
            Record("four_fragments_reunion", game.State == GameState.Reunion && boss.LitBuds == 6, "buds=" + boss.LitBuds);
            if (game.State != GameState.Reunion) yield break;
            game.EndStory();
            yield return null;
            yield return ReviewCredits("ending", false);

            var toHome = VisibleButton("Return Home");
            if (!toHome) { Record("ending_returns_to_home", false, "Return Home button missing"); yield break; }
            var formerLevel = game.level;
            var formerPlayer = game.player;
            toHome.onClick.Invoke();
            while (game.IsLoadingChapter) yield return null;
            yield return null;
            bool unloaded = !SceneManager.GetSceneByName(MoonlitChapterScene.SceneNames[3]).isLoaded;
            bool backAtHome = HomeHasNoChapter() && unloaded && !formerLevel && !formerPlayer;
            Record("ending_returns_to_home", backAtHome, SceneManager.GetActiveScene().path + ", Banyan Hill unloaded=" + unloaded);
            if (!backAtHome) yield break;

            var picker = VisibleButton("Select Chapter");
            if (picker) picker.onClick.Invoke();
            yield return null;
            var forest = VisibleButton("Chapter 2");
            if (!forest) { Record("home_selects_unlocked_forest", false, "Unlocked forest entry missing"); yield break; }
            bool matching = true;
            for (int i = 0; i < targets.Length; i++)
                matching &= VisibleButton("Chapter " + (i + 1)).transform.Find("Mote record").GetComponent<TMP_Text>().text
                    == $"{targets[i]}/10 hạt trăng";
            Record("chapter_picker_independent_records", matching, string.Join(", ",targets));
            Capture(game, "home-chapter-selection");
            forest.onClick.Invoke();
            while (game.IsLoadingChapter) yield return null;
            var selected = SceneManager.GetActiveScene();
            bool forestLoaded = selected.name == MoonlitChapterScene.SceneNames[1] && game.level &&
                game.level.gameObject.scene == selected && game.player && game.Shards == 1 && game.FurthestChapter == 4 &&
                !SceneManager.GetSceneByName(MoonlitHomeScene.SceneName).isLoaded;
            Record("home_selects_unlocked_forest", forestLoaded, selected.path + ", unlocked chapter=" + game.FurthestChapter);
            bool fresh = game.Motes == 0;
            yield return GatherMotes(game, 8);
            bool retained = fresh && game.Motes == 2 && game.BestMotes(2) == 4;
            game.RestartChapter();
            yield return null;
            yield return GatherMotes(game, 3);
            game.Home();
            while (game.IsLoadingChapter) yield return null;
            VisibleButton("Select Chapter").onClick.Invoke();
            var record = VisibleButton("Chapter 2").transform.Find("Mote record").GetComponent<TMP_Text>();
            Record("replay_keeps_best_single_run", retained && game.BestMotes(2) == 7 && game.BestMotes(3) == 2 &&
                record.text == "7/10 hạt trăng", "forest runs 4, 2, 7 (left early), picker=" + record.text);
            Capture(game, "replay-records");
        }

        IEnumerator ReviewMap(int chapter)
        {
            var pocket = game.hud.transform.Find("Pocket map");
            var atlas = game.hud.transform.Find("Chapter atlas");
            var open = VisibleButton("Open Chapter Map");
            var pauseButton = VisibleButton("Pause Game");
            bool compact = pocket.gameObject.activeInHierarchy && open && pauseButton &&
                pauseButton.transform.position.y < open.transform.position.y &&
                pocket.GetComponentsInChildren<TMP_Text>().Any(t => t.text.Contains("hạt trăng") && t.transform.position.y < open.transform.position.y);
            if (chapter == 2) Capture(game, "forest-pocket-map");
            if (chapter % 2 == 0 && open) open.onClick.Invoke();
            else yield return Press(Key.Tab);
            var position = game.player.Body.position;
            float clock = game.Elapsed;
            yield return new WaitForSecondsRealtime(.20f);
            bool mapped = game.State == GameState.Map && Time.timeScale == 0 && clock == game.Elapsed &&
                game.player.Body.position == position && atlas.gameObject.activeInHierarchy &&
                atlas.GetComponentsInChildren<MoonlitMapStrip>().Length == 3 &&
                atlas.GetComponentsInChildren<TMP_Text>().Any(t => t.text == game.LevelTitle);
            if (chapter == 2 && mapped) Capture(game, "forest-chapter-map");
            if (chapter == 3 && VisibleButton("Close Chapter Map")) VisibleButton("Close Chapter Map").onClick.Invoke();
            else yield return Press(chapter == 2 ? Key.Escape : Key.Tab);
            bool resumed = game.State == GameState.Playing && Time.timeScale == 1 && !atlas.gameObject.activeInHierarchy;
            if (pauseButton) pauseButton.onClick.Invoke();
            bool paused = game.State == GameState.Paused;
            yield return Press(Key.Escape);
            Record("pocket_map_and_controls_" + chapter, compact && mapped && resumed && paused && game.State == GameState.Playing,
                $"compact={compact}, frozen map={mapped}, resumed={resumed}, pause button={paused}");
        }

        IEnumerator ReviewVillageHoist()
        {
            var weight = game.level.Weight;
            var hoist = weight.bridge.GetComponent<LiftSuspension>();
            if (!hoist)
            {
                Record("village_hoist_attachments", false, "No suspension on the lowering deck");
                yield break;
            }

            game.player.Teleport(new Vector2(weight.plateCenter.x + 1.85f, weight.floorY + .08f));
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.25f);
            var afterRender = new WaitForEndOfFrame();
            yield return afterRender;
            float raised = weight.bridge.position.y;
            bool attached = hoist.CheckAttachments(out string detail);
            Capture(game, "village-hoist-raised");

            // The crate trips the actual pressure plate. Sample the cables after LateUpdate draws each pose.
            weight.crate.Body.position = new Vector2(weight.plateCenter.x, weight.floorY + .68f);
            int movingFrames = 0;
            bool capturedMidway = false;
            float start = Time.time;
            while (!weight.Open && Time.time - start < 3f)
            {
                yield return afterRender;
                if (!hoist.CheckAttachments(out var gap)) { attached = false; detail = gap; }
                if (weight.Progress <= 0 || weight.Open) continue;
                movingFrames++;
                if (!capturedMidway && weight.Progress >= .45f)
                {
                    Capture(game, "village-hoist-lowering");
                    capturedMidway = true;
                }
            }
            float lowered = weight.bridge.position.y;
            Capture(game, "village-hoist-lowered");
            bool landed = weight.Open && weight.bridgeCollider.enabled && Mathf.Abs(lowered - weight.floorY) < .02f;
            weight.ResetPuzzle();
            yield return afterRender;
            bool reset = !weight.bridgeCollider.enabled && Mathf.Abs(weight.bridge.position.y - raised) < .02f;
            if (!hoist.CheckAttachments(out var resetGap)) { attached = false; detail = resetGap; }
            Record("village_hoist_attachments", attached && landed && reset && capturedMidway && movingFrames >= 2 &&
                raised - lowered > 3f, detail ?? $"{movingFrames} moving frames, height {raised:F2} to {lowered:F2}, landed={landed}, reset={reset}");
        }

        bool HomeHasNoChapter()
        {
            return SceneManager.GetActiveScene().name == MoonlitHomeScene.SceneName && game.State == GameState.Home &&
                !game.ChapterSource && !game.level && !game.player && !game.cameraRig &&
                FindObjectsByType<MoonlitChapterScene>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length == 0;
        }

        IEnumerator PhotographAt(Vector2 feet, string file)
        {
            game.player.Teleport(feet);
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.42f);
            Capture(game, file);
        }

        Button VisibleButton(string label)
        {
            return game.hud.GetComponentsInChildren<Button>()
                .FirstOrDefault(b => b.name == label && b.isActiveAndEnabled && b.IsInteractable());
        }

        IEnumerator ReviewCredits(string from, bool keyboardSubmit)
        {
            if (from == "home")
            {
                var settings = VisibleButton("Home Settings");
                if (!settings) { Record("home_settings_button", false, "Settings button missing"); yield break; }
                settings.onClick.Invoke();
                yield return null;
            }
            var entry = VisibleButton("Copyright and Sources");
            Record(from + "_copyright_button", entry != null, game.State.ToString());
            if (!entry) yield break;
            var originalState = game.State;
            EventSystem.current.SetSelectedGameObject(entry.gameObject);
            if (keyboardSubmit) yield return Press(Key.Enter);
            else { entry.onClick.Invoke(); yield return null; }
            var back = VisibleButton("Back");
            bool focus = back && EventSystem.current.currentSelectedGameObject == back.gameObject;
            Record(from + "_copyright_opens", focus && game.State == originalState, "focused back button=" + focus);
            if (!back) yield break;
            var title = game.hud.GetComponentsInChildren<TMP_Text>().FirstOrDefault(t=>t.text == "Bản quyền và nguồn tư liệu");
            int accents = 0, missing = 0;
            if (title)
            {
                var section = title.transform.parent;
                foreach (var label in section.GetComponentsInChildren<TMP_Text>())
                {
                    label.ForceMeshUpdate();
                    var text = label.textInfo;
                    for (int i=0; i<text.characterCount; i++)
                    {
                        var glyph = text.characterInfo[i];
                        if (glyph.index >= label.text.Length) continue;
                        char specified = label.text[glyph.index];
                        if (specified <= 127 || !char.IsLetter(specified)) continue;
                        accents++;
                        if (!glyph.isVisible || glyph.textElement == null || glyph.textElement.unicode != specified) missing++;
                    }
                }
            }
            Record(from + "_copyright_vietnamese_glyphs", title && accents > 20 && missing == 0, $"accented letters={accents}, missing glyphs={missing}");
            Capture(game, from + "-copyright");
            if (keyboardSubmit) yield return Press(Key.Escape);
            else { back.onClick.Invoke(); yield return null; }
            bool returned = VisibleButton("Copyright and Sources") == entry;
            Record(from + "_copyright_returns", returned && game.State == originalState, game.State.ToString());
            if (from == "home" && returned)
            {
                VisibleButton("Back from Settings").onClick.Invoke();
                yield return null;
            }
        }
    }
}
