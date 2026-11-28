using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitHud
    {
        GameObject homeContent;
        Button homePlay, homeSettings, pauseSettings, resume, next, retry, endingHome;
        Button settingsCredits, endCredits, creditsBack;
        readonly Button[] chapterRows = new Button[4];
        readonly TMP_Text[] chapterLabels = new TMP_Text[4];
        readonly TMP_Text[] chapterRecords = new TMP_Text[4];
        readonly RawImage[] chapterPreviews = new RawImage[4];
        readonly MoonlitEmblem[] glints = new MoonlitEmblem[11];
        TMP_Text pauseInfo, clearTitle, clearCount, clearMetrics, prizeTitle, prizeHint, nextName;
        Image fragment;

        void BuildHome()
        {
            home = Layer("Title screen");
            homeContent = Panel("Home choices", home.transform, 0, 0, 1600, 900, Color.clear).gameObject;
            Transform p = homeContent.transform;
            for (int i = 0; i < glints.Length; i++)
                glints[i] = Emblem(p, MoonlitEmblem.Shape.Spark, 0, 0, i % 3 + 4, i % 3 + 4, gold);
            TitleSeal(p, 93, 63);
            Label(p, "NHÓM 05", 151, 65, 268, 44, 18, cream, true);
            Label(p, "Cuội", 85, 163, 585, 128, 114, cream, false, TextAlignmentOptions.MidlineLeft, true);
            Label(p, "Tìm Trăng", 85, 288, 645, 133, 102, gold, false, TextAlignmentOptions.MidlineLeft, true);
            Bar(p, 95, 440, 103, 2, gold);
            Emblem(p, MoonlitEmblem.Shape.Spark, 207, 433, 16, 16, gold);
            homePlay = Button(p, "Select Chapter", "CHƠI", 95, 484, 350, 70, OpenPicker, true);
            homeSettings = Button(p, "Home Settings", "CÀI ĐẶT", 95, 570, 350, 58, OpenSettings, false);
#if !UNITY_WEBGL || UNITY_EDITOR
            Button(p, "Quit Game", "THOÁT", 95, 641, 350, 58, () => StartCoroutine(QuitAfterSound()), false);
#endif
            BuildPicker();
        }

        void TitleSeal(Transform p, float x, float y)
        {
            var stamp = Panel("Lantern seal", p, x, y, 44, 44, vermilion);
            Bar(stamp.transform, 3, 3, 38, 1, gold);
            Bar(stamp.transform, 3, 40, 38, 1, gold);
            Emblem(stamp.transform, MoonlitEmblem.Shape.Star, 5, 5, 34, 34, cream);
        }

        void Art(Transform p, string resource)
        {
            var raw = new GameObject(resource, typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
            Place(raw.rectTransform, p, 0, 0, 1600, 900);
            var image = Resources.Load<Texture2D>(resource);
            raw.texture = image;
            if (image) {
                float aspect = image.width / (float)image.height;
                const float frame = 1600f / 900f;
                float w = Mathf.Min(1, frame / aspect), h = Mathf.Min(1, aspect / frame);
                raw.uvRect = new Rect((1-w)*.5f, (1-h)*.5f, w, h);
            }
            raw.raycastTarget = false;
        }

        void BuildPicker()
        {
            picker = Panel("Choose chapter", home.transform, 54, 49, 697, 799, ink).gameObject;
            var p = picker.transform;
            Bar(p, 0, 0, 697, 4, vermilion);
            Label(p, "Chọn màn", 44, 34, 605, 82, 58, cream, false, TextAlignmentOptions.MidlineLeft, true);
            Label(p, "Kỷ lục hạt trăng tính theo lượt nhặt được nhiều nhất.", 46, 103, 606, 29, 17, dim);
            Bar(p, 44, 137, 607, 1, rule);
            for (int i = 0; i < chapterRows.Length; i++)
            {
                int chapter = i + 1;
                var row = Button(p, "Chapter " + chapter, "", 44, 162 + i * 112, 607, 94, () => game.StartChapter(chapter), false);
                chapterRows[i] = row;
                var preview = new GameObject("Chapter landscape",typeof(RectTransform),typeof(RawImage)).GetComponent<RawImage>();
                Place(preview.rectTransform, row.transform, 9, 7, 153, 80);
                preview.texture = Resources.Load<Texture2D>("Scenery" + chapter);
                preview.raycastTarget = false;
                chapterPreviews[i] = preview;
                var number = Panel("Chapter number", row.transform, 9, 57, 36, 30, ink);
                Label(number.transform, chapter.ToString("00"), 0, 0, 36, 30, 16, gold, true, TextAlignmentOptions.Center);
                Label(row.transform, MoonlitGame.ChapterName(chapter), 182, 8, 405, 45, 27, cream, false, TextAlignmentOptions.MidlineLeft, true);
                chapterLabels[i] = Label(row.transform, "", 182, 53, 213, 28, 16, dim);
                chapterRecords[i] = Label(row.transform, "", 400, 53, 187, 28, 18, cream, false, TextAlignmentOptions.MidlineRight);
                chapterRecords[i].name = "Mote record";
            }
            Button(p, "Back", "QUAY LẠI", 44, 665, 225, 54, ClosePicker, false);
            Label(p, "ESC  Quay lại", 320, 672, 325, 35, 16, dim, false, TextAlignmentOptions.Right);
        }
        void RefreshHome()
        {
            for (int i = 0; i < chapterRows.Length; i++)
            {
                int chapter = i + 1;
                bool available = chapter <= game.FurthestChapter;
                chapterRows[i].interactable = available;
                string status = !available ? "Chưa mở" : game.FinishedCampaign || chapter < game.FurthestChapter ? "Đã lấy mảnh trăng" : "Đã mở";
                chapterLabels[i].text = status;
                chapterRecords[i].text = $"{game.BestMotes(chapter)}/{MoonlitLevel.MotesPerChapter} hạt trăng";
                chapterLabels[i].color = available ? gold : dim;
                chapterPreviews[i].color = available ? Color.white : new Color(.52f,.57f,.60f);
            }
        }
        public void OpenPicker()
        {
            picker.SetActive(true);
            homeContent.SetActive(false);
            Select(chapterRows[Mathf.Clamp(game.FurthestChapter - 1, 0, 3)]);
        }
        void ClosePicker()
        {
            buttonActivationFrame = Time.frameCount;
            picker.SetActive(false);
            homeContent.SetActive(true);
            Select(homePlay);
        }
        IEnumerator QuitAfterSound()
        {
            if (!game.audioFx.Muted) yield return new WaitForSecondsRealtime(MoonlitAudio.InteractionDuration);
            Application.Quit();
        }
        void BuildSplash()
        {
            splash = Panel("Team splash", root, 0, 0, 1600, 900, new Color(.018f,.044f,.069f)).gameObject;
            var ray = splash.GetComponent<Image>();
            ray.raycastTarget = true;
            splashGroup = splash.AddComponent<CanvasGroup>();
            var stamp = Panel("Studio lantern", splash.transform, 746, 304, 108, 108, vermilion);
            Emblem(stamp.transform, MoonlitEmblem.Shape.Star, 13, 13, 82, 82, cream);
            Label(splash.transform, "Nhóm 05", 500, 449, 600, 87, 57, cream, false, TextAlignmentOptions.Center, true);
        }
        void UpdateHomeInput(float dt)
        {
            var kb = Keyboard.current;
            if (showingSplash)
            {
                splashTime += dt;
                bool skip = (kb != null && kb.anyKey.wasPressedThisFrame) ||
                    (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                    (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
                if (splashTime >= 2 || skip)
                {
                    showingSplash = false;
                    splash.SetActive(false);
                    buttonActivationFrame = Time.frameCount;
                    StartCoroutine(FocusHomeAfterSplash());
                }
                else splashGroup.alpha = Mathf.Clamp01((2 - splashTime) * 3);
            }
            else if (picker.activeSelf && kb != null && kb.escapeKey.wasPressedThisFrame) ClosePicker();
        }
        IEnumerator FocusHomeAfterSplash()
        {
            yield return null;
            if (game.State == GameState.Home && !picker.activeSelf && !CapturesMenuInput) Select(homePlay);
        }
        void UpdateGlints()
        {
            if (!homeContent.activeInHierarchy || showingSplash) return;
            float t = Time.unscaledTime;
            for (int i = 0; i < glints.Length; i++)
            {
                int hash = (i * 383 + 41) % 863;
                float x = 659 + hash + 14 * Mathf.Sin(t * .48f + i);
                float y = 110 + (i * 179 % 644) + 9 * Mathf.Sin(t * .7f + i * 2.2f);
                glints[i].rectTransform.anchoredPosition = new Vector2(x, -y);
                Color tint = gold;
                tint.a = .23f + .67f * Mathf.Pow(.5f + .5f * Mathf.Sin(t * 1.5f + i * 2.9f), 2);
                glints[i].color = tint;
            }
        }

        GameObject Curtain(string name)
        {
            var overlay = Layer(name);
            var wash = Panel("Dimmed scene", overlay.transform, 0, 0, 1600, 900, new Color(.014f,.039f,.064f,.77f));
            wash.raycastTarget = true;
            return overlay;
        }
        Image Card(Transform p, float x, float y, float w, float h)
        {
            var c = Panel("Lacquered wood panel", p, x,y,w,h,ink);
            Bar(c.transform,0,0,w,5,vermilion);
            Bar(c.transform,13,15,w-26,1,rule);
            Bar(c.transform,13,h-16,w-26,1,rule);
            return c;
        }
        void BuildPause()
        {
            pause = Curtain("Pause");
            var c = Card(pause.transform, 463, 86, 674, 726).transform;
            Label(c, "Tạm dừng", 35, 35, 604, 96, 60, cream, false, TextAlignmentOptions.Center, true);
            pauseInfo = Label(c, "", 35, 133, 604, 35, 20, gold, false, TextAlignmentOptions.Center);
            resume = Button(c, "Resume", "TIẾP TỤC", 93, 215, 488, 64, game.TogglePause, true);
            pauseSettings = Button(c, "Pause Settings", "CÀI ĐẶT", 93, 302, 488, 60, OpenSettings, false);
            Button(c, "Restart Chapter", "CHƠI LẠI MÀN", 93, 380, 488, 60, game.RestartChapter, false);
            Button(c, "Return Home", "VỀ MÀN HÌNH CHÍNH", 93, 458, 488, 60, game.Home, false);
        }
        void BuildDefeat()
        {
            defeat = Curtain("Lantern extinguished");
            defeatGroup = defeat.AddComponent<CanvasGroup>();
            var c = Card(defeat.transform, 460, 157, 680, 581).transform;
            Emblem(c, MoonlitEmblem.Shape.Star, 310, 42, 60, 60, dim);
            Label(c, "Đèn đã tắt…", 24, 121, 632, 90, 59, cream, false, TextAlignmentOptions.Center, true);
            retryAt = Label(c, "", 45, 223, 590, 89, 22, paper, false, TextAlignmentOptions.Center);
            retry = Button(c, "Retry Checkpoint", "THỬ LẠI", 82, 351, 516, 64, game.RetryCheckpoint, true);
            Button(c, "Return Home", "VỀ MÀN HÌNH CHÍNH", 82, 436, 516, 54, game.Home, false);
        }
        void BuildVictory()
        {
            victory = Curtain("Chapter complete");
            victoryGroup = victory.AddComponent<CanvasGroup>();
            var c = Card(victory.transform, 433, 54, 734, 790).transform;
            fragment = Panel("Collected moon shard", c, 260, 30, 214, 130, Color.white);
            fragment.sprite = game.art.moon; fragment.preserveAspect = true;
            clearCount = Label(c, "", 35, 171, 664, 33, 17, gold, true, TextAlignmentOptions.Center);
            clearTitle = Label(c, "", 20, 202, 694, 76, 47, cream, false, TextAlignmentOptions.Center, true);
            clearMetrics = Label(c, "", 34, 282, 666, 42, 20, dim, false, TextAlignmentOptions.Center);
            var reward = Panel("New ability", c, 63, 347, 608, 145, new Color(.102f,.181f,.191f));
            Bar(reward.transform, 0, 0, 3, 145, gold);
            Label(reward.transform, "KỸ NĂNG MỚI", 22, 12, 562, 23, 14, gold, true, TextAlignmentOptions.Center);
            prizeTitle = Label(reward.transform, "", 20, 38, 566, 49, 34, cream, false, TextAlignmentOptions.Center, true);
            prizeHint = Label(reward.transform, "", 20, 88, 566, 45, 18, paper, false, TextAlignmentOptions.Center);
            next = Button(c, "Next Chapter", "", 93, 535, 548, 65, game.NextChapter, true);
            nextName = next.GetComponentInChildren<TMP_Text>();
            Button(c, "Restart Chapter", "CHƠI LẠI MÀN", 93, 624, 260, 53, game.RestartChapter, false);
            Button(c, "Return Home", "VỀ MÀN HÌNH CHÍNH", 367, 624, 274, 53, game.Home, false);
        }
        void RefreshVictory()
        {
            int chapter = game.Chapter;
            fragment.sprite = FourMoonPieces.Get(chapter-1);
            clearCount.text = $"ĐÃ TÌM ĐƯỢC {game.Shards}/4 MẢNH TRĂNG";
            clearTitle.text = game.LevelTitle;
            clearMetrics.text = $"{TimeText()}     ·     {game.Motes}/{game.TotalMotes} hạt trăng     ·     {game.Deaths} lần ngã";
            prizeTitle.text = chapter == 1 ? "Nhảy đôi" : chapter == 2 ? "Leo vách" : "Lướt";
            prizeHint.text = chapter == 1 ? "Bấm nhảy lần nữa khi Cuội còn trên không." : chapter == 2
                ? "Nhảy sát vách rồi giữ phím hướng vào vách cùng phím nhảy." : $"Bấm {game.Controls.Label(GameAction.Dash)} để lướt về phía trước.";
            nextName.text = $"MÀN {chapter + 1:00}    ·    {MoonlitGame.ChapterName(chapter + 1).ToUpperInvariant()}";
        }
        void BuildEnding()
        {
            ending = Layer("Return to the moon");
            var p = ending.transform;
            Art(p,"ReturnToMoon");
            TitleSeal(p, 86, 70);
            Label(p, "RẰM THÁNG TÁM", 146, 70, 520, 43, 17, gold, true);
            Label(p, "Trăng tròn\ntrở lại", 80, 167, 636, 244, 91, cream, false, TextAlignmentOptions.MidlineLeft, true);
            Bar(p, 88, 442, 103, 2, gold);
            Label(p, "Dưới làng, lũ trẻ lại rước đèn quanh sân đình. Ngước lên mà xem, Cuội vẫn ngồi trên cành đa đấy.", 88, 469, 607, 136, 23, cream);
            endingHome = Button(p, "Return Home", "VỀ MÀN HÌNH CHÍNH", 88, 668, 348, 64, game.Home, true);
            endCredits = Button(p, "Copyright and Sources", "BẢN QUYỀN", 451, 668, 158, 64, OpenCredits, false);
            Label(p, "Hết", 89, 800, 260, 51, 30, gold, false, TextAlignmentOptions.MidlineLeft, true);
        }
        void BuildCredits()
        {
            credits = Curtain("Copyright and Sources");
            var p = Card(credits.transform, 420, 76, 760, 748).transform;
            Label(p, "Bản quyền và nguồn tư liệu", 36, 30, 688, 92, 40, cream, false, TextAlignmentOptions.Center, true);
            Label(p, "CUỘI TÌM TRĂNG  ·  NHÓM 05", 36, 131, 688, 34, 18, gold, true, TextAlignmentOptions.Center);
            Bar(p, 60, 188, 640, 1, rule);
            Label(p, "CỐT TRUYỆN", 64, 211, 624, 29, 16, gold, true);
            Label(p, "Lấy cảm hứng từ chuyện dân gian về chú Cuội và cây đa trên cung trăng.", 64, 249, 630, 69, 21, paper);
            Label(p, "MỸ THUẬT VÀ ÂM THANH", 64, 341, 624, 29, 16, gold, true);
            Label(p, "Tranh và hình nhân vật được tạo bằng OpenAI ImageGen. Nhạc piano phối từ Trống quân và Đi cấy, dùng mẫu đàn của Alexander Holm (CC BY 3.0).", 64, 380, 630, 96, 20, paper);
            Label(p, "CHỮ VÀ GIAO DIỆN", 64, 491, 624, 29, 16, gold, true);
            Label(p, "TextMesh Pro · Unity Technologies. Liberation Sans · Google và Red Hat (SIL OFL). Phông tiêu đề dùng Constantia của Windows khi có sẵn.", 64, 530, 630, 85, 19, paper);
            creditsBack = Button(p, "Back", "QUAY LẠI", 221, 645, 318, 55, CloseCredits, true);
        }
        void OpenCredits()
        {
            if (game.State == GameState.Ending) ending.SetActive(false);
            else settings.SetActive(false);
            credits.SetActive(true);
            Select(creditsBack);
        }
        void CloseCredits()
        {
            buttonActivationFrame = Time.frameCount;
            credits.SetActive(false);
            if (game.State == GameState.Ending) { ending.SetActive(true); Select(endCredits); }
            else { settings.SetActive(true); Select(settingsCredits); }
        }
        void BuildTutorial()
        {
            tutorialScreen = Layer("Skill lesson");
            var c = Card(tutorialScreen.transform, 873, 175, 664, 546).transform;
            tutorialProgress = Label(c, "", 34, 28, 588, 31, 16, gold, true);
            tutorialTitle = Label(c, "", 34, 75, 590, 86, 33, cream, false, TextAlignmentOptions.MidlineLeft, true);
            tutorialBody = Label(c, "", 34, 168, 589, 168, 21, paper);
            Bar(c, 34, 353, 592, 1, rule);
            tutorialAction = Label(c, "", 34, 366, 590, 44, 18, gold, true);
            tutorialContinue = Button(c, "Continue Lesson", "CHƠI TIẾP    ·    ENTER", 34, 435, 592, 60, () => game.tutorials.Continue(), true);
            tutorialSubject = Label(tutorialScreen.transform, "", 86, 797, 710, 44, 20, cream, true, TextAlignmentOptions.Center);
            AddShadow(tutorialSubject);
            tutorialGroup = tutorialScreen.AddComponent<CanvasGroup>();
        }
    }
}
