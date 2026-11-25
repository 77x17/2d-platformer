using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitHud : MonoBehaviour
    {
        MoonlitGame game;
        RectTransform root;
        TMP_FontAsset displayFont, interfaceFont;
        GameObject home, hud, pause, victory, defeat, ending, tutorialScreen, picker, splash, credits;
        Image matte, fade, progressFill, abilityFill;
        Image promptBoard, bossBoard, introBoard;
        readonly Image[] bossBuds = new Image[6];
        readonly Image[] hearts = new Image[3];
        readonly Vector3[] iconCorners = new Vector3[4];
        Sprite heartFull, heartEmpty;
        RectTransform lifeCard;
        Canvas mainCanvas;
        TMP_Text healthCount;
        MoonlitEmblem lantern;
        TMP_Text lifeChapter, zone, shardCount, motes, abilityState, currentSkill, prompt, bossCount, bossHint, introName;
        TMP_Text toastTitle, toastBody, tutorialTitle, tutorialBody, tutorialAction, tutorialProgress, tutorialSubject;
        CanvasGroup toastGroup, introGroup, tutorialGroup, splashGroup, victoryGroup, defeatGroup;
        Button tutorialContinue;
        GameState? lastState;
        int buttonActivationFrame = -1, previousHealth = 3, previousShards = -1;
        int shownMotes = -1, shownTotal = -1, shownCooldown = -1;
        float toastTimer, tokenFlash, heartFlash, shardFlash, lanternFlash, splashTime, panelTime;
        bool showingSplash = true;
        public bool ButtonActivatedThisFrame => buttonActivationFrame == Time.frameCount;
        public bool BlocksDefaultSubmit => showingSplash || (picker && picker.activeSelf) || (credits && credits.activeSelf);
        public bool HeartDisplayVisible => lifeCard && lifeCard.gameObject.activeInHierarchy;
        public int VisibleHeartCount
        {
            get
            {
                int count = 0;
                if (HeartDisplayVisible && heartFull)
                    foreach (var heart in hearts)
                        if (heart && heart.sprite == heartFull && heart.isActiveAndEnabled) count++;
                return count;
            }
        }

        public Rect HeartScreenRect(int index)
        {
            if (index < 0 || index >= hearts.Length || !HeartDisplayVisible) return Rect.zero;
            hearts[index].rectTransform.GetWorldCorners(iconCorners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, iconCorners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, iconCorners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        static readonly Color ink = new Color(.029f,.072f,.105f,.97f);
        static readonly Color cream = new Color(1,.925f,.775f);
        static readonly Color paper = new Color(.84f,.84f,.78f);
        static readonly Color gold = new Color(1,.711f,.314f);
        static readonly Color dim = new Color(.62f,.71f,.71f);
        static readonly Color vermilion = new Color(.72f,.247f,.145f);
        static readonly Color rule = new Color(.78f,.61f,.37f,.57f);

        public void Build(MoonlitGame owner)
        {
            game = owner;
            mainCanvas = gameObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<GraphicRaycaster>();
            RectTransform fullScreen = GetComponent<RectTransform>();
            matte = Panel("Menu letterbox", fullScreen, 0, 0, 1600, 900, ink);
            Stretch(matte.rectTransform);
            root = new GameObject("16 by 9 safe region", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(fullScreen, false);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(.5f, .5f);
            root.sizeDelta = new Vector2(1600, 900);
            if (!FindFirstObjectByType<EventSystem>())
            {
                var es = new GameObject("Menu input");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }
            interfaceFont = TMP_Settings.defaultFontAsset;
            displayFont = TMP_FontAsset.CreateFontAsset("Constantia", "Regular", 90);
            if (displayFont && interfaceFont)
                displayFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { interfaceFont };
            if (!displayFont) displayFont = interfaceFont;
            BuildHome();
            BuildHud();
            BuildPause();
            BuildVictory();
            BuildDefeat();
            BuildEnding();
            BuildTutorial();
            BuildCredits();
            var note = Panel("Checkpoint notification", root, 480, 110, 640, 84, ink);
            Bar(note.transform, 0, 0, 3, 84, gold);
            toastTitle = Label(note.transform, "", 18, 8, 602, 31, 19, gold, true);
            toastBody = Label(note.transform, "", 18, 38, 600, 40, 18, paper);
            toastGroup = note.gameObject.AddComponent<CanvasGroup>();
            toastGroup.blocksRaycasts = false;
            BuildSplash();
            fade = Panel("Respawn fade", fullScreen, 0, 0, 1600, 900, Color.clear);
            Stretch(fade.rectTransform);
            RefreshState();
        }

        void BuildHud()
        {
            hud = Layer("In-game HUD");
            var life = Panel("Health and lantern", mainCanvas.transform, 26, 22, 330, 80, ink);
            lifeCard = life.rectTransform;
            var vitalsOverlay = life.gameObject.AddComponent<Canvas>();
            vitalsOverlay.overrideSorting = true;
            vitalsOverlay.sortingOrder = 30;
            Bar(life.transform, 0, 0, 3, 80, vermilion);
            heartFull = Resources.Load<Sprite>("HeartFull");
            heartEmpty = Resources.Load<Sprite>("HeartEmpty");
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i] = Panel($"Heart {i + 1}", life.transform, 13 + i * 41, 10, 38, 31, Color.white);
                hearts[i].sprite = heartFull;
                hearts[i].preserveAspect = true;
            }
            healthCount = Label(life.transform, "3/3", 137, 10, 43, 31, 19, cream, true);
            lifeChapter = Label(life.transform, "", 15, 47, 167, 24, 17, cream, true);
            Bar(life.transform, 181, 13, 1, 54, rule);
            lantern = Emblem(life.transform, MoonlitEmblem.Shape.Star, 194, 12, 32, 32, gold);
            Label(life.transform, "J / X", 234, 9, 87, 29, 17, paper, true);
            abilityState = Label(life.transform, "", 196, 45, 130, 22, 14, dim);
            Bar(life.transform, 196, 67, 117, 3, new Color(.29f,.33f,.32f));
            abilityFill = Bar(life.transform, 196, 67, 117, 3, gold);
            var stats = Panel("Moon counters", hud.transform, 1270, 22, 234, 80, ink);
            var shard = Panel("Shard", stats.transform, 12, 8, 53, 60, Color.white);
            shard.sprite = game.art.moon;
            shard.preserveAspect = true;
            shardCount = Label(stats.transform, "", 70, 8, 158, 32, 21, cream, true);
            motes = Label(stats.transform, "", 70, 43, 161, 22, 15, dim);
            var pauseControl = Button(hud.transform, "", 1515, 22, 59, 65, game.TogglePause, false);
            pauseControl.name = "Tạm dừng";
            Bar(pauseControl.transform, 21,22,6,22,cream);
            Bar(pauseControl.transform, 33,22,6,22,cream);
            zone = Label(life.transform, "", 3, 82, 540, 30, 16, cream, true);
            AddShadow(zone);
            currentSkill = Label(hud.transform, "", 30, 827, 445, 31, 16, cream, true);
            AddShadow(currentSkill);
            Bar(hud.transform, 29, 865, 1541, 3, new Color(.22f,.31f,.34f,.8f));
            progressFill = Bar(hud.transform, 29, 865, 1, 3, gold);

            promptBoard = Panel("Nearby sign", hud.transform, 400, 769, 800, 62, ink);
            Emblem(promptBoard.transform, MoonlitEmblem.Shape.Spark, 15, 20, 21, 21, gold);
            prompt = Label(promptBoard.transform, "", 47, 3, 734, 55, 20, cream, false, TextAlignmentOptions.Center);
            bossBoard = Panel("Banyan encounter", hud.transform, 365, 736, 870, 103, ink);
            Bar(bossBoard.transform, 0, 0, 4, 103, vermilion);
            bossCount = Label(bossBoard.transform, "", 24, 9, 819, 29, 18, gold, true, TextAlignmentOptions.Center);
            bossHint = Label(bossBoard.transform, "", 22, 41, 826, 50, 20, cream, false, TextAlignmentOptions.Center);
            for (int i = 0; i < bossBuds.Length; i++)
                bossBuds[i] = Bar(bossBoard.transform, 322 + i * 37 + (i / 2) * 4, 90, 30, 4, rule);
            introBoard = Panel("Chapter opening", hud.transform, 491, 95, 618, 99, new Color(.025f,.07f,.105f,.91f));
            Bar(introBoard.transform, 162, 91, 294, 1, rule);
            introName = Label(introBoard.transform, "", 20, 4, 578, 81, 38, cream, false, TextAlignmentOptions.Center, true);
            introGroup = introBoard.gameObject.AddComponent<CanvasGroup>();
            introGroup.blocksRaycasts = false;
        }

        void Update()
        {
            if (!game) return;
            float dt = Time.unscaledDeltaTime;
            if (credits.activeSelf)
            {
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) CloseCredits();
            }
            else if (game.State == GameState.Home)
            {
                UpdateHomeInput(dt);
                UpdateGlints();
            }
            if (game.State == GameState.Complete)
            {
                panelTime += dt;
                victoryGroup.alpha = Mathf.SmoothStep(0, 1, (panelTime - .12f) / .7f);
                fragment.rectTransform.anchoredPosition = new Vector2(260, -30 + Mathf.Sin(panelTime * 2) * 7);
            }
            if (game.State == GameState.Defeat)
            {
                panelTime += dt;
                defeatGroup.alpha = Mathf.SmoothStep(0, 1, panelTime / .85f);
            }
            if (HeartDisplayVisible) UpdateHearts(dt);
            if (game.State == GameState.Playing || game.State == GameState.Respawning) UpdateHud(dt);
            if (game.State == GameState.Tutorial && game.tutorials && game.tutorials.Active)
            {
                var lesson = game.tutorials;
                tutorialTitle.text = lesson.Title;
                tutorialBody.text = lesson.Description;
                tutorialAction.text = lesson.Action;
                tutorialSubject.text = lesson.Subject;
                tutorialProgress.text = $"HƯỚNG DẪN   ·   {lesson.CurrentId + 1} / {MoonlitTutorial.Total}";
                tutorialGroup.alpha = lesson.CardOpacity;
                tutorialContinue.interactable = lesson.CanContinue;
            }
            if (game.State == GameState.Paused)
                soundLabel.text = game.audioFx.Muted ? "ÂM THANH TẮT   [M]" : "ÂM THANH BẬT   [M]";
            toastTimer = Mathf.Max(0, toastTimer - Time.deltaTime);
            toastGroup.alpha = game.State == GameState.Playing
                ? Mathf.Min(Mathf.Clamp01(toastTimer * 3), Mathf.Clamp01((4 - toastTimer) * 4)) : 0;
        }

        void UpdateHud(float dt)
        {
            var level = game.level;
            var player = game.player;
            float x = player.transform.position.x;
            zone.text = level.AreaName(x);
            if (game.Shards != previousShards)
            {
                shardFlash = previousShards < 0 ? 0 : 1;
                previousShards = game.Shards;
                shardCount.text = $"{game.Shards} / 4 mảnh";
            }
            shardFlash = Mathf.Max(0, shardFlash - dt * 2);
            shardCount.rectTransform.localScale = Vector3.one * (1 + shardFlash * .10f);
            if (shownMotes != game.Motes || shownTotal != game.TotalMotes)
            {
                shownMotes = game.Motes; shownTotal = game.TotalMotes;
                motes.text = $"{shownMotes} / {shownTotal} hạt trăng";
            }
            tokenFlash = Mathf.Max(0, tokenFlash - dt * 2.5f);
            motes.color = Color.Lerp(dim, gold, tokenFlash);
            abilityFill.rectTransform.sizeDelta = new Vector2(117 * player.PulseReady, 3);
            int cooldown = Mathf.CeilToInt(player.Cooldown * 10);
            if (cooldown != shownCooldown)
            {
                if (cooldown == 0 && shownCooldown > 0) lanternFlash = 1;
                shownCooldown = cooldown;
                abilityState.text = cooldown == 0 ? "Sẵn sàng" : $"Hồi {cooldown * .1f:0.0} s";
            }
            lanternFlash = Mathf.Max(0, lanternFlash - dt * 2.4f);
            lantern.color = Color.Lerp(Color.Lerp(dim, gold, player.PulseReady), Color.white, lanternFlash);
            currentSkill.text = game.Chapter == 2 ? "SPACE ×2   Nhảy đôi" : game.Chapter == 3 ? "GIỮ HƯỚNG + SPACE   Leo vách"
                : game.Chapter == 4 ? player.DashReady >= .99f ? "SHIFT / K   Lướt" : "Lướt đang hồi…" : "";
            progressFill.rectTransform.sizeDelta = new Vector2(1541 * Mathf.Clamp01(game.Farthest / Mathf.Max(1, level.EndX)), 3);
            var boss = game.Boss;
            bool inEncounter = boss && boss.Active;
            bossBoard.gameObject.SetActive(inEncounter);
            if (inEncounter)
            {
                bossCount.text = $"CÂY ĐA   ·   {boss.CalmedRoots} / {boss.RootCount} RỄ ĐÃ DỊU";
                bossHint.text = boss.Hint;
                for (int i = 0; i < bossBuds.Length; i++)
                    bossBuds[i].color = i < boss.LitBuds ? gold : i == boss.LitBuds && boss.TargetOpen ? cream : rule;
            }
            string hint = inEncounter ? "" : level.HintAt(x);
            promptBoard.gameObject.SetActive(!string.IsNullOrEmpty(hint));
            prompt.text = hint;
            introGroup.alpha = Mathf.Clamp01((game.Elapsed - .2f) * 3) * Mathf.Clamp01((3.3f - game.Elapsed) * 2);
            introBoard.gameObject.SetActive(introGroup.alpha > .001f && toastTimer <= 0);
        }

        void UpdateHearts(float dt)
        {
            int health = Mathf.Clamp(game.Health, 0, hearts.Length);
            if (previousHealth != health)
            {
                if (health < previousHealth) heartFlash = 1;
                previousHealth = health;
            }
            healthCount.SetText("{0}/3", health);
            heartFlash = Mathf.Max(0, heartFlash - dt * 3.3f);
            for (int i = 0; i < hearts.Length; i++)
            {
                var icon = hearts[i];
                icon.sprite = i < health ? heartFull : heartEmpty;
                icon.rectTransform.localScale = Vector3.one * (i == health ? 1 + .15f * heartFlash : 1);
            }
        }

        void LateUpdate()
        {
            if (HeartDisplayVisible) AlignVitals();
        }

        void AlignVitals()
        {
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            Rect safe = Screen.safeArea;
            if (safe.width < 1 || safe.height < 1) safe = new Rect(0, 0, w, h);
            Vector2 corner = new Vector2(safe.xMin / w, safe.yMax / h);
            lifeCard.anchorMin = lifeCard.anchorMax = corner;
            // The card follows the actual Game View corner. A docked Editor view keeps a
            // readable ~24x19px sprite even when menus have scaled to much smaller sizes.
            float canvasScale = Mathf.Max(.01f, mainCanvas.scaleFactor);
            float physicalScale = Mathf.Clamp(Mathf.Min(safe.width / 1600, safe.height / 900), .64f, 2.5f);
            float scale = physicalScale / canvasScale;
            lifeCard.localScale = new Vector3(scale, scale, 1);
            lifeCard.anchoredPosition = new Vector2(Mathf.Round(24 * physicalScale) / canvasScale,
                -Mathf.Round(20 * physicalScale) / canvasScale);
        }

        public void RefreshState()
        {
            GameState state = game.State;
            var original = lastState;
            lastState = state;
            bool changed = original != state;
            if (changed) credits.SetActive(false);
            if (state != GameState.Playing) toastGroup.alpha = 0;
            if (state != GameState.Home) showingSplash = false;
            matte.gameObject.SetActive(state == GameState.Home || state == GameState.Ending);
            home.SetActive(state == GameState.Home);
            hud.SetActive(state == GameState.Playing || state == GameState.Respawning);
            bool showVitals = state == GameState.Playing || state == GameState.Respawning
                || state == GameState.Tutorial || state == GameState.Paused || state == GameState.Defeat;
            lifeCard.gameObject.SetActive(showVitals);
            zone.gameObject.SetActive(state == GameState.Playing || state == GameState.Respawning);
            if (showVitals)
            {
                AlignVitals();
                UpdateHearts(0);
            }
            pause.SetActive(state == GameState.Paused);
            victory.SetActive(state == GameState.Complete);
            defeat.SetActive(state == GameState.Defeat);
            ending.SetActive(state == GameState.Ending);
            tutorialScreen.SetActive(state == GameState.Tutorial);
            splash.SetActive(showingSplash && state == GameState.Home);
            if (!changed && !(state == GameState.Playing && game.Elapsed == 0f)) return;
            if (state == GameState.Home)
            {
                picker.SetActive(false);
                homeContent.SetActive(true);
                RefreshHome();
                Select(showingSplash ? null : homePlay);
            }
            else if (state == GameState.Playing)
            {
                if (game.Elapsed < .1f)
                {
                    introName.text = game.LevelTitle;
                    lifeChapter.text = $"MÀN {game.Chapter:00} / 04";
                    previousHealth = game.Health;
                    previousShards = shownMotes = shownCooldown = -1;
                    toastTimer = tokenFlash = heartFlash = shardFlash = lanternFlash = 0;
                }
                Select(null);
            }
            else if (state == GameState.Paused)
            {
                pauseInfo.text = $"{game.LevelTitle}    ·    {TimeText()}";
                Select(resume);
            }
            else if (state == GameState.Complete)
            {
                panelTime = 0;
                victoryGroup.alpha = 0;
                RefreshVictory();
                Select(next);
            }
            else if (state == GameState.Defeat)
            {
                panelTime = 0;
                defeatGroup.alpha = 0;
                Select(retry);
            }
            else if (state == GameState.Ending) Select(endingHome);
            else if (state == GameState.Tutorial) Select(tutorialContinue);
        }

        public void TokenCollected() => tokenFlash = 1;
        public void ShowToast(string title, string description)
        {
            toastTitle.text = title;
            toastBody.text = description;
            toastTimer = 4;
        }
        public void SetFade(float opacity) => fade.color = new Color(.014f,.026f,.057f,opacity);

        string TimeText()
        {
            int seconds = Mathf.Max(0, Mathf.FloorToInt(game.Elapsed));
            return $"{seconds / 60:00}:{seconds % 60:00}";
        }

        GameObject Layer(string name)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(root, false);
            Stretch(r);
            return r.gameObject;
        }
        static void Place(RectTransform rect, Transform parent, float x, float y, float w, float h)
        {
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = new Vector2(x,-y);
            rect.sizeDelta = new Vector2(w,h);
        }
        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        Image Panel(string name, Transform p, float x, float y, float w, float h, Color c)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            Place(image.rectTransform,p,x,y,w,h);
            image.color = c;
            image.raycastTarget = false;
            return image;
        }
        Image Bar(Transform p, float x, float y, float w, float h, Color c) => Panel("Inlay", p,x,y,w,h,c);
        MoonlitEmblem Emblem(Transform p, MoonlitEmblem.Shape shape, float x, float y, float w, float h, Color c)
        {
            var emblem = new GameObject(shape.ToString(), typeof(RectTransform), typeof(MoonlitEmblem)).GetComponent<MoonlitEmblem>();
            Place(emblem.rectTransform,p,x,y,w,h);
            emblem.Configure(shape);
            emblem.color = c;
            return emblem;
        }
        TMP_Text Label(Transform parent, string value, float x, float y, float w, float h, int size, Color c,
            bool bold = false, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, bool display = false)
        {
            var t = new GameObject(value.Length == 0 ? "Live text" : value, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            Place(t.rectTransform,parent,x,y,w,h);
            t.font = display ? displayFont : interfaceFont;
            t.text = value;
            t.fontSize = size;
            t.color = c;
            t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            t.richText = false;
            return t;
        }
        static void AddShadow(TMP_Text t)
        {
            t.outlineColor = new Color32(8,16,26,255);
            t.outlineWidth = .2f;
        }
        Button Button(Transform parent, string title, float x, float y, float w, float h,
            UnityEngine.Events.UnityAction clicked, bool primary)
        {
            var face = Panel(title, parent,x,y,w,h,Color.white);
            face.raycastTarget = true;
            var b = face.gameObject.AddComponent<Button>();
            b.targetGraphic = face;
            var palette = b.colors;
            palette.normalColor = primary ? vermilion : new Color(.082f,.16f,.19f,.97f);
            palette.selectedColor = palette.highlightedColor = primary ? new Color(.9f,.365f,.183f) : new Color(.2f,.31f,.32f);
            palette.pressedColor = new Color(.46f,.24f,.13f);
            palette.disabledColor = new Color(.085f,.115f,.135f);
            palette.fadeDuration = .10f;
            b.colors = palette;
            if (primary) Bar(face.transform, 0,h-3,w,3,gold);
            Label(face.transform,title,8,0,w-16,h,19,cream,true,TextAlignmentOptions.Center);
            var focus = face.gameObject.AddComponent<MoonlitMenuFocus>();
            focus.Configure(game, b, Bar(face.transform, 0,0,4,h,gold));
            b.onClick.AddListener(() =>
            {
                buttonActivationFrame = Time.frameCount;
                game.audioFx.PlayInteraction();
                clicked();
            });
            return b;
        }
        static void Select(Button button)
        {
            var events = EventSystem.current;
            if (events) events.SetSelectedGameObject(button ? button.gameObject : null);
        }
        void OnDestroy()
        {
            if (displayFont && displayFont != interfaceFont) Destroy(displayFont);
        }
    }
}
