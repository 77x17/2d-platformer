using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitHud
    {
        GameObject settings;
        TMP_Text settingsHint, soundSetting;
        Button[,] bindingButtons;
        TMP_Text[,] bindingLabels;
        Button listeningButton;
        GameAction listeningAction;
        int listeningSlot, captureFrame, restoreNavigationFrame = -1;
        bool navigationBeforeCapture;

        void BuildSettings()
        {
            settings = Curtain("Settings");
            var sheet = Card(settings.transform, 294, 41, 1012, 818).transform;
            Label(sheet, "Cài đặt", 44, 30, 450, 70, 51, cream, false, TextAlignmentOptions.MidlineLeft, true);
            var sound = Button(sheet, "Toggle Sound", "", 684, 45, 280, 48, ToggleSettingsSound, false);
            soundSetting = sound.GetComponentInChildren<TMP_Text>();
            Label(sheet, "BÀN PHÍM", 46, 120, 295, 28, 17, gold, true);
            settingsHint = Label(sheet, "", 46, 151, 918, 35, 17, paper);
            Bar(sheet, 46, 202, 918, 1, rule);
            int count = game.Controls.Count;
            bindingButtons = new Button[count, 3];
            bindingLabels = new TMP_Text[count, 3];
            for (int i = 0; i < count; i++)
            {
                var action = (GameAction)i;
                int row = action == GameAction.Down ? 3 : i <= (int)GameAction.Jump ? i : i + 1;
                int y = 213 + row * 44;
                Label(sheet, MoonlitControls.Name(action), 47, y, 335, 39, 19, cream);
                for (int j = 0; j < game.Controls.Slots(action); j++)
                {
                    int slot = j;
                    var button = Button(sheet, $"Binding {action} {slot + 1}", "", 386 + slot * 196, y, 186, 38,
                        () => ListenForKey(action, slot), false);
                    bindingButtons[i,slot] = button;
                    bindingLabels[i,slot] = button.GetComponentInChildren<TMP_Text>();
                    bindingLabels[i,slot].fontSize = 17;
                }
            }
            Bar(sheet, 46, 713, 918, 1, rule);
            Button(sheet, "Back from Settings", "QUAY LẠI", 46, 737, 190, 52, CloseSettings, true);
            Button(sheet, "Restore Default Keys", "PHÍM MẶC ĐỊNH", 478, 737, 293, 52, RestoreKeys, false);
            settingsCredits = Button(sheet, "Copyright and Sources", "BẢN QUYỀN", 786, 737, 178, 52,
                () => { StopListening(); OpenCredits(); }, false);
            settings.SetActive(false);
        }

        void OpenSettings()
        {
            if (game.State == GameState.Paused) pause.SetActive(false);
            else homeContent.SetActive(false);
            settings.SetActive(true);
            RefreshBindings();
            RefreshSoundSetting();
            settingsHint.text = "Chọn ô phím muốn đổi, rồi nhấn phím mới.";
            Select(bindingButtons[0,0]);
        }

        void CloseSettings()
        {
            StopListening();
            buttonActivationFrame = Time.frameCount;
            settings.SetActive(false);
            if (game.State == GameState.Paused) { pause.SetActive(true); Select(pauseSettings); }
            else { homeContent.SetActive(true); Select(homeSettings); }
        }

        void RefreshBindings()
        {
            for (int i = 0; i < game.Controls.Count; i++)
            for (int j = 0; j < game.Controls.Slots((GameAction)i); j++)
                bindingLabels[i,j].text = game.Controls.Label((GameAction)i,j);
        }

        void ListenForKey(GameAction action, int slot)
        {
            if (listeningButton) RefreshBindings();
            listeningAction = action;
            listeningSlot = slot;
            listeningButton = bindingButtons[(int)action,slot];
            bindingLabels[(int)action,slot].text = "…";
            settingsHint.text = $"Nhấn phím mới cho “{MoonlitControls.Name(action)}”. Esc để hủy.";
            captureFrame = Time.frameCount;
            var events = EventSystem.current;
            if (events && events.sendNavigationEvents)
            {
                navigationBeforeCapture = true;
                events.sendNavigationEvents = false;
            }
            restoreNavigationFrame = -1;
        }

        void StopListening()
        {
            if (!listeningButton) return;
            bindingLabels[(int)listeningAction,listeningSlot].text = game.Controls.Label(listeningAction,listeningSlot);
            listeningButton = null;
            if (navigationBeforeCapture) restoreNavigationFrame = Time.frameCount + 1;
        }

        void RestoreMenuNavigation()
        {
            if (restoreNavigationFrame < 0 || Time.frameCount < restoreNavigationFrame) return;
            if (EventSystem.current) EventSystem.current.sendNavigationEvents = true;
            restoreNavigationFrame = -1;
            navigationBeforeCapture = false;
        }

        void UpdateSettingsInput()
        {
            var kb = Keyboard.current;
            if (kb == null || ButtonActivatedThisFrame) return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                buttonActivationFrame = Time.frameCount;
                if (!listeningButton) { CloseSettings(); return; }
                StopListening();
                settingsHint.text = "Đã hủy đổi phím.";
                return;
            }
            if (!listeningButton || captureFrame == Time.frameCount) return;
            foreach (var button in kb.allKeys)
            {
                if (button == null || !button.wasPressedThisFrame) continue;
                bool changed = game.Controls.Rebind(listeningAction, listeningSlot, button.keyCode, out var message);
                settingsHint.text = message;
                if (changed) StopListening();
                buttonActivationFrame = Time.frameCount;
                return;
            }
        }

        void RestoreKeys()
        {
            StopListening();
            game.Controls.RestoreDefaults();
            RefreshBindings();
            settingsHint.text = "Đã khôi phục các phím mặc định.";
        }

        void ToggleSettingsSound()
        {
            StopListening();
            game.audioFx.Toggle();
            settingsHint.text = "Chọn ô phím muốn đổi, rồi nhấn phím mới.";
            RefreshSoundSetting();
        }

        void RefreshSoundSetting() => soundSetting.text = game.audioFx.Muted ? "ÂM THANH   ·   TẮT" : "ÂM THANH   ·   BẬT";
    }
}
