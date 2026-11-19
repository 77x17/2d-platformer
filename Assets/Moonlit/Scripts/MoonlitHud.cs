using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Moonlit
{
    public sealed class MoonlitHud : MonoBehaviour
    {
        MoonlitGame game;
        RectTransform root;
        GameObject home, hud, pause, victory, toast, tutorialScreen;
        Text timer, motes, zone, abilityState, prompt, toastTitle, toastBody, result, mute;
        Image abilityFill, progressFill, fade;
        Image tokenIcon;
        float tokenFlash;
        Image[] hearts;
        CanvasGroup toastGroup;
        float toastTimer;
        Text tutorialTitle, tutorialBody, tutorialAction, tutorialProgress, tutorialSubject, tutorialContinueText;
        Button tutorialContinue;
        CanvasGroup tutorialGroup;
        readonly Color white = new Color(.89f, .95f, .92f);
        readonly Color muted = new Color(.50f, .67f, .68f);
        readonly Color panel = new Color(.025f, .065f, .10f, .91f);

        public void Build(MoonlitGame owner)
        {
            game = owner;
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            root = GetComponent<RectTransform>();
            if (!FindFirstObjectByType<EventSystem>()) { var es = new GameObject("UI EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<InputSystemUIInputModule>(); }
            BuildHome(); BuildHud(); BuildPause(); BuildVictory(); BuildTutorial();
            toast = Panel("Chapter message", root, new Vector2(470, 120), new Vector2(660, 95), new Color(.025f, .07f, .10f, .9f)).gameObject;
            Bar(toast.transform, 0, 0, 3, 95, Visuals.Gold);
            toastTitle = Label(toast.transform, "", 24, 16, 620, 30, 20, Visuals.Gold, FontStyle.Bold);
            toastBody = Label(toast.transform, "", 24, 50, 615, 35, 17, white);
            toastGroup = toast.AddComponent<CanvasGroup>(); toastGroup.blocksRaycasts = false;
            fade = Panel("Respawn fade", root, Vector2.zero, new Vector2(1600, 900), new Color(.01f, .035f, .07f, 0)); Stretch(fade.rectTransform); fade.raycastTarget = false;
            RefreshState();
        }

        void BuildHome()
        {
            home = Panel("Home", root, Vector2.zero, new Vector2(1600, 900), new Color(.015f, .045f, .075f, .48f)).gameObject; Stretch(home.GetComponent<RectTransform>());
            var p = Panel("Title panel", home.transform, new Vector2(68, 88), new Vector2(655, 680), new Color(.018f, .055f, .085f, .91f));
            Bar(p.transform, 0, 0, 3, 680, new Color(.53f, .82f, .7f, .55f));
            Label(p.transform, "MỘT CHUYẾN ĐI QUA MIỀN CỔ TÍCH", 36, 35, 580, 26, 15, Visuals.Gold, FontStyle.Bold);
            Label(p.transform, "CUỘI", 27, 86, 590, 152, 120, white, FontStyle.Bold);
            Label(p.transform, "ĐƯỜNG VỀ TRĂNG", 36, 238, 580, 52, 31, Visuals.Mint);
            Bar(p.transform, 36, 316, 80, 2, Visuals.Gold);
            Label(p.transform, "Một ngọn đèn nhỏ.\nMột khu rừng đang thức giấc.\nMột mảnh trăng dẫn lối về nhà.", 36, 345, 575, 116, 24, new Color(.66f, .78f, .77f));
            Button(p.transform, "BẮT ĐẦU HÀNH TRÌNH    →", new Vector2(36, 500), new Vector2(470, 65), game.StartRun, true);
            Label(p.transform, "MÀN 01  /  RỪNG ĐA", 36, 600, 420, 26, 16, muted, FontStyle.Bold);
            var moon = Panel("Moon emblem", home.transform, new Vector2(1005, 220), new Vector2(330, 330), Color.white); moon.sprite = game.art.moon; moon.preserveAspect = true;
            Label(home.transform, "01 — 04", 1070, 580, 230, 35, 22, Visuals.Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(home.transform, "NHẶT ÁNH TRĂNG\nTÌM ĐƯỜNG VỀ", 950, 630, 450, 80, 21, white, FontStyle.Normal, TextAnchor.MiddleCenter);
            Label(home.transform, "A  D   DI CHUYỂN     /     SPACE   NHẢY     /     F   BỪNG SÁNG     /     E   TƯƠNG TÁC", 88, 820, 1300, 30, 16, new Color(.70f, .81f, .79f));
            Label(home.transform, "ENTER  ĐỂ BẮT ĐẦU", 1260, 820, 265, 30, 13, Visuals.Gold, FontStyle.Bold, TextAnchor.MiddleRight);
            Button(home.transform, "THOÁT GAME", new Vector2(1375, 44), new Vector2(180, 44), () => Application.Quit(), false);
        }

        void BuildHud()
        {
            hud = new GameObject("In-game HUD", typeof(RectTransform)); hud.transform.SetParent(root, false); Stretch(hud.GetComponent<RectTransform>());
            var chapter = Panel("Chapter", hud.transform, new Vector2(36, 28), new Vector2(320, 86), panel);
            Bar(chapter.transform, 0, 0, 3, 86, Visuals.Mint);
            Label(chapter.transform, "01   /   RỪNG ĐA", 20, 13, 280, 28, 20, white, FontStyle.Bold);
            zone = Label(chapter.transform, "Lối mòn đom đóm", 20, 48, 280, 25, 15, muted);
            hearts = new Image[3];
            for (int i = 0; i < 3; i++) { hearts[i] = Bar(hud.transform, 43 + i * 27, 132, 13, 13, Visuals.Gold); hearts[i].rectTransform.localRotation = Quaternion.Euler(0, 0, 45); }
            Label(hud.transform, "SINH LỰC", 132, 127, 130, 24, 12, muted, FontStyle.Bold);
            var quest = Panel("Moon objective", hud.transform, new Vector2(644, 28), new Vector2(312, 60), panel);
            var icon = Panel("Moon icon", quest.transform, new Vector2(14, 6), new Vector2(43, 43), Color.white); icon.sprite = game.art.moon; icon.preserveAspect = true;
            Label(quest.transform, "TÌM MẢNH TRĂNG", 69, 7, 230, 23, 15, white, FontStyle.Bold);
            Label(quest.transform, "0 / 4    •    Theo lối sáng", 69, 31, 230, 22, 13, muted);
            var stats = Panel("Statistics", hud.transform, new Vector2(1254, 28), new Vector2(218, 86), panel);
            tokenIcon = Panel("Moon token counter icon", stats.transform, new Vector2(15, 12), new Vector2(30, 30), Color.white);
            tokenIcon.sprite = game.art.moonToken ? game.art.moonToken : game.art.moon; tokenIcon.preserveAspect = true;
            motes = Label(stats.transform, "00 / 40", 58, 8, 145, 26, 19, Visuals.Gold, FontStyle.Bold);
            Label(stats.transform, "ĐỒNG TRĂNG", 58, 34, 145, 18, 10, muted, FontStyle.Bold);
            timer = Label(stats.transform, "00:00", 18, 58, 190, 22, 14, muted);
            Button(hud.transform, "Ⅱ", new Vector2(1490, 28), new Vector2(66, 70), game.TogglePause, false);
            Bar(hud.transform, 36, 871, 1528, 2, new Color(.4f, .7f, .65f, .18f));
            progressFill = Bar(hud.transform, 36, 871, 1, 2, Visuals.Mint);
            var ability = Panel("Lantern ability", hud.transform, new Vector2(36, 767), new Vector2(305, 79), panel);
            var key = Panel("F key", ability.transform, new Vector2(13, 13), new Vector2(50, 50), new Color(.22f, .27f, .22f));
            Label(key.transform, "F", 0, 0, 50, 50, 26, Visuals.Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(ability.transform, "BỪNG SÁNG", 78, 9, 215, 23, 16, Visuals.Gold, FontStyle.Bold);
            abilityState = Label(ability.transform, "Sẵn sàng", 78, 32, 215, 22, 13, muted);
            Bar(ability.transform, 78, 61, 209, 3, new Color(.28f, .34f, .31f));
            abilityFill = Bar(ability.transform, 78, 61, 209, 3, Visuals.Gold);
            prompt = Label(hud.transform, "", 460, 729, 680, 58, 22, Visuals.Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(hud.transform, "A D  Di chuyển     SPACE  Nhảy     E  Tương tác     ESC  Dừng", 750, 807, 814, 30, 15, new Color(.74f, .83f, .8f), FontStyle.Normal, TextAnchor.MiddleRight);
        }

        void BuildPause()
        {
            pause = Panel("Pause overlay", root, Vector2.zero, new Vector2(1600, 900), new Color(.01f, .035f, .06f, .86f)).gameObject; Stretch(pause.GetComponent<RectTransform>());
            Label(pause.transform, "NGHỈ DƯỚI TÁN ĐA", 400, 185, 800, 70, 46, white, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(pause.transform, "Ngọn đèn vẫn chờ bạn.", 400, 273, 800, 40, 21, muted, FontStyle.Normal, TextAnchor.MiddleCenter);
            Button(pause.transform, "TIẾP TỤC", new Vector2(565, 357), new Vector2(470, 63), game.TogglePause, true);
            Button(pause.transform, "CHƠI LẠI MÀN", new Vector2(565, 438), new Vector2(470, 63), game.StartRun, false);
            Button(pause.transform, "VỀ TRANG CHỦ", new Vector2(565, 519), new Vector2(470, 63), game.Home, false);
            var sound = Button(pause.transform, "ÂM THANH: BẬT   [M]", new Vector2(565, 616), new Vector2(470, 48), () => game.audioFx.Toggle(), false); mute = sound.GetComponentInChildren<Text>();
            Label(pause.transform, "R  Hồi sinh tại đèn gần nhất     •     Giữ / nhả Space để đổi độ cao cú nhảy", 300, 745, 1000, 32, 17, muted, FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        void BuildVictory()
        {
            victory = Panel("Completion", root, Vector2.zero, new Vector2(1600, 900), new Color(.018f, .055f, .08f, .92f)).gameObject; Stretch(victory.GetComponent<RectTransform>());
            var icon = Panel("Recovered moon fragment", victory.transform, new Vector2(705, 65), new Vector2(190, 190), Color.white); icon.sprite = game.art.moon; icon.preserveAspect = true;
            Label(victory.transform, "MẢNH TRĂNG ĐẦU TIÊN", 250, 277, 1100, 70, 49, white, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(victory.transform, "1 / 4    —    MỘT CHÚT ÁNH SÁNG ĐÃ TRỞ VỀ", 300, 367, 1000, 38, 19, Visuals.Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            result = Label(victory.transform, "", 300, 430, 1000, 48, 23, Visuals.Mint, FontStyle.Normal, TextAnchor.MiddleCenter);
            var reward = Panel("Next chapter reward", victory.transform, new Vector2(480, 521), new Vector2(640, 95), new Color(.1f, .21f, .23f));
            Label(reward.transform, "ĐÃ MỞ KHÓA: BÁM VÁCH", 20, 15, 600, 30, 21, Visuals.Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(reward.transform, "Kỹ năng dành cho màn 2 • Chuyến đi sẽ còn tiếp tục", 20, 53, 600, 26, 16, muted, FontStyle.Normal, TextAnchor.MiddleCenter);
            Button(victory.transform, "CHƠI LẠI MÀN 1", new Vector2(565, 670), new Vector2(470, 63), game.StartRun, true);
            Button(victory.transform, "VỀ TRANG CHỦ", new Vector2(565, 751), new Vector2(470, 53), game.Home, false);
        }

        void BuildTutorial()
        {
            tutorialScreen = Panel("Focused tutorial", root, Vector2.zero, new Vector2(1600, 900), Color.clear).gameObject;
            Stretch(tutorialScreen.GetComponent<RectTransform>());
            tutorialScreen.GetComponent<Image>().raycastTarget = true;
            tutorialGroup = tutorialScreen.AddComponent<CanvasGroup>();
            tutorialProgress = Label(tutorialScreen.transform, "", 64, 45, 800, 32, 17, Visuals.Gold, FontStyle.Bold);
            Label(tutorialScreen.transform, "ĐANG TẠM DỪNG • ĐỌC THEO NHỊP CỦA BẠN", 805, 45, 730, 32, 15, muted, FontStyle.Normal, TextAnchor.MiddleRight);
            Bar(tutorialScreen.transform, 64, 97, 1472, 2, new Color(.3f, .47f, .43f, .6f));
            var card = Panel("Instruction card", tutorialScreen.transform, new Vector2(920, 224), new Vector2(616, 476), new Color(.018f, .055f, .065f, .98f));
            Bar(card.transform, 0, 0, 4, 476, Visuals.Gold);
            tutorialTitle = Label(card.transform, "", 32, 28, 552, 85, 29, white, FontStyle.Bold);
            tutorialBody = Label(card.transform, "", 32, 126, 552, 192, 22, new Color(.72f, .83f, .80f));
            Bar(card.transform, 32, 335, 552, 1, new Color(.25f, .39f, .35f));
            tutorialAction = Label(card.transform, "", 32, 354, 552, 36, 17, Visuals.Gold, FontStyle.Bold);
            tutorialContinue = Button(card.transform, "ĐÃ HIỂU — TIẾP TỤC   [ENTER]", new Vector2(32, 405), new Vector2(552, 49), () => game.tutorials.Continue(), true);
            tutorialContinueText = tutorialContinue.GetComponentInChildren<Text>();
            tutorialSubject = Label(tutorialScreen.transform, "", 238, 728, 590, 38, 20, Visuals.Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        void Update()
        {
            if (!game) return;
            int seconds = (int)game.Elapsed;
            timer.text = $"{seconds / 60:00}:{seconds % 60:00}   /   HÀNH TRÌNH";
            motes.text = $"{game.Motes:00} / {game.TotalMotes}";
            tokenFlash = Mathf.Max(0, tokenFlash - Time.deltaTime * 3);
            motes.color = Color.Lerp(Visuals.Gold, Color.white, tokenFlash);
            tokenIcon.rectTransform.localScale = Vector3.one * (1 + tokenFlash * .18f);
            float x = game.player.transform.position.x;
            zone.text = x < 79 ? "Lối mòn đom đóm" : x < 147 ? "Qua dòng sương bạc" : x < 274 ? "Cầu rễ cổ thụ" : "Ngọn cây giữ trăng";
            for (int i = 0; i < 3; i++) hearts[i].color = i < game.Health ? Visuals.Gold : new Color(.28f, .34f, .34f);
            abilityFill.rectTransform.sizeDelta = new Vector2(209 * game.player.PulseReady, 3);
            abilityState.text = game.player.Cooldown <= 0 ? "Sẵn sàng  •  Làm choáng" : $"Đang hồi sáng  {game.player.Cooldown:0.0}s";
            progressFill.rectTransform.sizeDelta = new Vector2(1528 * Mathf.Clamp01(game.Farthest / MoonlitLevel.GoalX), 2);
            prompt.text = game.level.lanternSwitch.Near && !game.level.lanternSwitch.Activated ? "[ E ]   THẮP ĐÈN • ĐÁNH THỨC CÂY CẦU" : "";
            if (game.tutorials && game.tutorials.Active)
            {
                var lesson = game.tutorials;
                tutorialTitle.text = lesson.Title; tutorialBody.text = lesson.Description;
                tutorialAction.text = lesson.Action; tutorialSubject.text = lesson.Subject;
                tutorialSubject.rectTransform.anchoredPosition = new Vector2(238, lesson.CurrentId == 2 ? -828 : -728);
                tutorialProgress.text = $"RỪNG ĐA   /   HƯỚNG DẪN {lesson.CurrentId + 1:00} / {MoonlitTutorial.Total:00}";
                tutorialGroup.alpha = lesson.CardOpacity; tutorialContinue.interactable = lesson.CanContinue;
                tutorialContinueText.text = lesson.CanContinue ? "ĐÃ HIỂU — TIẾP TỤC   [ENTER]" : "QUAN SÁT ĐỐI TƯỢNG...";
            }
            if (mute) mute.text = game.audioFx.Muted ? "ÂM THANH: TẮT   [M]" : "ÂM THANH: BẬT   [M]";
            toastTimer -= Time.deltaTime;
            toastGroup.alpha = game.State == GameState.Playing ? Mathf.Clamp01(toastTimer) * Mathf.Clamp01((6 - toastTimer) * 3) : 0;
        }

        public void RefreshState()
        {
            home.SetActive(game.State == GameState.Home); hud.SetActive(game.State == GameState.Playing || game.State == GameState.Respawning);
            pause.SetActive(game.State == GameState.Paused); victory.SetActive(game.State == GameState.Complete);
            tutorialScreen.SetActive(game.State == GameState.Tutorial);
            if (game.Motes == 0) tokenFlash = 0;
            if (game.State == GameState.Complete) result.text = $"{(int)game.Elapsed / 60:00}:{(int)game.Elapsed % 60:00}     •     {game.Motes} / {game.TotalMotes} đồng trăng     •     {game.Deaths} lần hồi sinh";
        }
        public void TokenCollected() { tokenFlash = 1; }
        public void ShowToast(string title, string description) { toastTitle.text = title; toastBody.text = description; toastTimer = 6; }
        public void SetFade(float opacity) { fade.color = new Color(.01f, .035f, .07f, opacity); }

        Image Panel(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(pos.x, -pos.y); rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
        }
        Image Bar(Transform p, float x, float y, float w, float h, Color c) => Panel("Accent", p, new Vector2(x, y), new Vector2(w, h), c);
        Text Label(Transform parent, string value, float x, float y, float w, float h, int size, Color color, FontStyle style = FontStyle.Normal, TextAnchor align = TextAnchor.UpperLeft)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
            var t = go.GetComponent<Text>(); t.font = game.art.uiFont; t.text = value; t.fontSize = size; t.color = color; t.fontStyle = style; t.alignment = align; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; return t;
        }
        Button Button(Transform parent, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action, bool primary)
        {
            var im = Panel(label, parent, pos, size, primary ? Visuals.Mint : new Color(.11f, .21f, .24f)); im.raycastTarget = true;
            var b = im.gameObject.AddComponent<Button>(); b.targetGraphic = im;
            var colors = b.colors; colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f); colors.pressedColor = new Color(.72f, .85f, .83f); colors.fadeDuration = .12f; b.colors = colors;
            b.onClick.AddListener(action);
            Label(im.transform, label, 5, 0, size.x - 10, size.y, 18, primary ? new Color(.035f, .12f, .15f) : white, FontStyle.Bold, TextAnchor.MiddleCenter);
            return b;
        }
        static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    }
}
