using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitHud
    {
        RectTransform pocket;
        GameObject atlas;
        Button mapClose;
        TMP_Text mapTitle, currentArea, pocketMapKey, atlasCloseKey;
        MoonlitMapStrip nearby;
        readonly MoonlitMapStrip[] sections = new MoonlitMapStrip[3];
        readonly TMP_Text[] sectionStarts = new TMP_Text[3], sectionEnds = new TMP_Text[3];
        readonly Image[] sectionTabs = new Image[3];
        MoonlitLevel mappedLevel;
        MoonlitMapDiscovery survey;
        BoxCollider2D[] mapSurfaces;
        float nextMapSample;

        void BuildChapterMap(RectTransform screen)
        {
            var frame = Panel("Pocket map", screen, 0, 0, 248, 139, ink);
            pocket = frame.rectTransform;
            pocket.anchorMin = pocket.anchorMax = pocket.pivot = Vector2.one;
            var open = Button(frame.transform, "Open Chapter Map", "", 0, 0, 248, 95, game.ToggleMap, false);
            Label(open.transform, "BẢN ĐỒ", 11, 2, 145, 24, 14, cream, true);
            pocketMapKey = Label(open.transform, "", 177, 2, 58, 24, 15, gold, true, TextAlignmentOptions.MidlineRight);
            nearby = Chart(open.transform, 9, 30, 230, 58);
            Emblem(frame.transform, MoonlitEmblem.Shape.Spark, 13, 109, 19, 19, gold);
            motes = Label(frame.transform, "", 42, 99, 146, 38, 17, cream, true);
            var pauseButton = Button(frame.transform, "Pause Game", "", 193, 98, 55, 41, game.TogglePause, false);
            Bar(pauseButton.transform, 20, 12, 5, 17, cream);
            Bar(pauseButton.transform, 30, 12, 5, 17, cream);

            var shade = Panel("Chapter atlas", screen, 0, 0, 0, 0, new Color(.016f,.045f,.064f,.88f));
            atlas = shade.gameObject;
            Stretch(shade.rectTransform);
            shade.raycastTarget = true;
            var sheet = Panel("Map folio", shade.transform, 0, 0, 1260, 751, ink);
            var sheetRect = sheet.rectTransform;
            sheetRect.anchorMin = sheetRect.anchorMax = sheetRect.pivot = new Vector2(.5f,.5f);
            sheetRect.anchoredPosition = Vector2.zero;
            var page = sheet.transform;
            Bar(page, 0, 0, 1260, 4, vermilion);
            Label(page, "BẢN ĐỒ", 48, 23, 212, 27, 17, gold, true);
            mapTitle = Label(page, "", 45, 49, 775, 62, 47, cream, false, TextAlignmentOptions.MidlineLeft, true);
            mapClose = Button(page, "Close Chapter Map", "ĐÓNG   [TAB]", 1005, 43, 208, 49, game.ToggleMap, false);
            atlasCloseKey = mapClose.GetComponentInChildren<TMP_Text>();
            currentArea = Label(page, "", 49, 115, 710, 32, 20, cream);
            Bar(page, 48, 158, 1164, 1, rule);

            for (int i = 0; i < sections.Length; i++)
            {
                int y = 177 + i * 163;
                sectionTabs[i] = Bar(page, 49, y + 3, 4, 21, rule);
                sectionStarts[i] = Label(page, "", 66, y, 536, 28, 17, paper, true);
                sectionEnds[i] = Label(page, "", 637, y, 575, 28, 17, dim, false, TextAlignmentOptions.MidlineRight);
                sections[i] = Chart(page, 49, y + 33, 1163, 110);
            }

            Emblem(page, MoonlitEmblem.Shape.Star, 45, 678, 21, 21, MoonlitMapStrip.PlayerInk);
            Label(page, "Cuội", 73, 670, 75, 37, 18, cream);
            Bar(page, 160, 689, 2, 9, MoonlitMapStrip.LampInk);
            Bar(page, 157, 683, 8, 7, MoonlitMapStrip.LampInk);
            Label(page, "Cột đèn", 176, 670, 135, 37, 18, cream);
            var mote = Bar(page, 305, 682, 12, 12, MoonlitMapStrip.MoteInk);
            mote.rectTransform.localRotation = Quaternion.Euler(0,0,45);
            Label(page, "Hạt trăng", 324, 670, 145, 37, 18, cream);
        }

        MoonlitMapStrip Chart(Transform parent, int x, int y, int w, int h)
        {
            var field = Panel("Terrain chart", parent, x, y, w, h, new Color(.052f,.114f,.135f));
            var plot = new GameObject("Mapped terrain", typeof(RectTransform), typeof(MoonlitMapStrip)).GetComponent<MoonlitMapStrip>();
            Place(plot.rectTransform, field.transform, 0, 0, w, h);
            plot.raycastTarget = false;
            return plot;
        }

        void BindMap()
        {
            if (mappedLevel == game.level && mapSurfaces != null) return;
            mappedLevel = game.level;
            survey = mappedLevel.gameObject.AddComponent<MoonlitMapDiscovery>();
            survey.Begin(mappedLevel.RouteLength);
            var colliders = new List<BoxCollider2D>();
            foreach (var surface in mappedLevel.GetComponentsInChildren<PlatformSurface>(true))
                if (surface.TryGetComponent<BoxCollider2D>(out var collider)) colliders.Add(collider);
            mapSurfaces = colliders.ToArray();
        }

        void SetMapVisibility(GameState state)
        {
            bool playing = state == GameState.Playing || state == GameState.Respawning;
            pocket.gameObject.SetActive(playing);
            atlas.SetActive(state == GameState.Map);
            nextMapSample = 0;
            if (state == GameState.Home || state == GameState.Loading)
            {
                mappedLevel = null;
                survey = null;
                mapSurfaces = null;
            }
            if (playing) pocketMapKey.text = game.Controls.Label(GameAction.Map);
            if (state != GameState.Map) return;
            atlasCloseKey.text = "ĐÓNG   [" + game.Controls.Label(GameAction.Map) + "]";
            BindMap();
            survey.Visit(game.player.Body.position);
            float length = mappedLevel.RouteLength;
            float x = game.player.Body.position.x;
            int here = Mathf.Clamp(Mathf.FloorToInt(x / length * sections.Length), 0, sections.Length - 1);
            mapTitle.text = game.LevelTitle;
            currentArea.text = "Cuội đang ở " + mappedLevel.AreaName(x);
            for (int i = 0; i < sections.Length; i++)
            {
                float from = length * i / sections.Length, to = length * (i + 1) / sections.Length;
                sectionTabs[i].color = i == here ? gold : rule;
                bool explored = survey.KnownSpan(from, to, out int first, out int last);
                string start = explored ? mappedLevel.AreaName(first) : "Chưa khám phá";
                string end = explored ? mappedLevel.AreaName(last) : "";
                sectionStarts[i].text = start;
                sectionEnds[i].text = start == end ? "" : end;
                sections[i].Show(game, mapSurfaces, survey, from, to, false);
            }
        }

        void RefreshLocalMap()
        {
            if (Time.unscaledTime < nextMapSample) return;
            nextMapSample = Time.unscaledTime + .05f;
            BindMap();
            if (game.State == GameState.Playing) survey.Visit(game.player.Body.position);
            float span = Mathf.Min(62, mappedLevel.RouteLength);
            float west = Mathf.Clamp(game.player.Body.position.x - span * .5f, 0, mappedLevel.RouteLength - span);
            nearby.Show(game, mapSurfaces, survey, west, west + span, true);
        }

        void AlignPocket()
        {
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            Rect safe = Screen.safeArea;
            if (safe.width < 1 || safe.height < 1) safe = new Rect(0, 0, w, h);
            pocket.anchorMin = pocket.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            pocket.anchoredPosition = new Vector2(-24, -20);
        }
    }
}
