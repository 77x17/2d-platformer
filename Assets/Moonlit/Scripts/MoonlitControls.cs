using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonlit
{
    public enum GameAction { Left, Right, Jump, Lantern, Interact, Dash, Map, Pause, Sound, Respawn, Down }

    public sealed class MoonlitControls
    {
        static readonly Key[][] Defaults = {
            new[] { Key.A, Key.LeftArrow }, new[] { Key.D, Key.RightArrow },
            new[] { Key.Space, Key.W, Key.UpArrow }, new[] { Key.J, Key.X, Key.F },
            new[] { Key.E }, new[] { Key.LeftShift, Key.K, Key.RightShift },
            new[] { Key.Tab }, new[] { Key.Escape }, new[] { Key.M }, new[] { Key.R },
            new[] { Key.S, Key.DownArrow }
        };
        static readonly string[] Names = {
            "Đi sang trái", "Đi sang phải", "Nhảy / leo vách", "Rọi đèn", "Tương tác",
            "Lướt", "Bản đồ", "Tạm dừng", "Bật / tắt âm thanh", "Về cột đèn (mất một tim)", "Cúi / thả xuống bệ dưới"
        };
        readonly Key[][] keys = new Key[Defaults.Length][];
        readonly string[][] labels = new string[Defaults.Length][];
        readonly bool usePreferences;
        public int Count => keys.Length;
        public int Slots(GameAction action) => keys[(int)action].Length;
        public static string Name(GameAction action) => Names[(int)action];
        public string Label(GameAction action, int slot = 0) => labels[(int)action][slot];

        public MoonlitControls(bool usePreferences)
        {
            this.usePreferences = usePreferences;
            var used = new HashSet<Key>();
            bool valid = true;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = (Key[])Defaults[i].Clone();
                labels[i] = new string[keys[i].Length];
                for (int j = 0; j < keys[i].Length; j++)
                {
                    var action = (GameAction)i;
                    var key = usePreferences ? (Key)PlayerPrefs.GetInt(Pref(action,j), (int)Defaults[i][j]) : Defaults[i][j];
                    // Keep an older layout which already assigned S or Down Arrow.
                    if (action == GameAction.Down && used.Contains(key) && usePreferences && !PlayerPrefs.HasKey(Pref(action, j)))
                    {
                        Key other = (Key)PlayerPrefs.GetInt(Pref(action, 1-j), (int)Defaults[i][1-j]);
                        for (int k = (int)Key.A; k <= (int)Key.Z; k++)
                            if (!used.Contains((Key)k) && (Key)k != other) { key = (Key)k; break; }
                        PlayerPrefs.SetInt(Pref(action,j), (int)key);
                        PlayerPrefs.Save();
                    }
                    if (!Allowed(action, key) || !used.Add(key)) valid = false;
                    else Put(action, j, key);
                }
            }
            if (!valid) RestoreDefaults();
        }

        static string Pref(GameAction action, int slot) => $"Moonlit.Keys.{action}.{slot}";
        static bool Allowed(GameAction action, Key key) =>
            (((int)key >= (int)Key.Space && (int)key <= (int)Key.OEM5) ||
             ((int)key >= (int)Key.F13 && (int)key <= (int)Key.MediaForward)) &&
            key != Key.Enter && key != Key.NumpadEnter && (key != Key.Escape || action == GameAction.Pause);

        static string DisplayName(Key key)
        {
            switch (key)
            {
                case Key.Space: return "Phím cách";
                case Key.Escape: return "Esc";
                case Key.LeftShift: return "Shift trái";
                case Key.RightShift: return "Shift phải";
                case Key.LeftAlt: return "Alt trái";
                case Key.RightAlt: return "Alt phải";
                case Key.LeftCtrl: return "Ctrl trái";
                case Key.RightCtrl: return "Ctrl phải";
                case Key.LeftArrow: return "←";
                case Key.RightArrow: return "→";
                case Key.UpArrow: return "↑";
                case Key.DownArrow: return "↓";
                case Key.Tab: return "Tab";
                default:
                    string label = Keyboard.current?[key].displayName ?? key.ToString();
                    return label.Length == 1 ? label.ToUpperInvariant() : label;
            }
        }

        void Put(GameAction action, int slot, Key key)
        {
            keys[(int)action][slot] = key;
            labels[(int)action][slot] = DisplayName(key);
        }

        public bool Rebind(GameAction action, int slot, Key key, out string message)
        {
            if (!Allowed(action, key))
            {
                message = "Phím này dành cho hệ thống hoặc menu. Hãy chọn phím khác.";
                return false;
            }
            for (int i = 0; i < keys.Length; i++)
            for (int j = 0; j < keys[i].Length; j++)
                if (keys[i][j] == key && (i != (int)action || j != slot))
                {
                    message = $"{DisplayName(key)} đang dùng cho “{Names[i]}”. Hãy chọn phím khác.";
                    return false;
                }
            Put(action, slot, key);
            if (usePreferences) { PlayerPrefs.SetInt(Pref(action, slot), (int)key); PlayerPrefs.Save(); }
            message = $"{Name(action)}  ·  {Label(action, slot)}";
            return true;
        }

        public void RestoreDefaults()
        {
            for (int i = 0; i < keys.Length; i++)
            for (int j = 0; j < keys[i].Length; j++)
            {
                var action = (GameAction)i;
                Put(action, j, Defaults[i][j]);
                if (usePreferences) PlayerPrefs.DeleteKey(Pref(action, j));
            }
            if (usePreferences) PlayerPrefs.Save();
        }

        public bool Held(GameAction action, Keyboard keyboard)
        {
            foreach (var key in keys[(int)action]) if (keyboard[key].isPressed) return true;
            return false;
        }

        public bool Pressed(GameAction action, Keyboard keyboard)
        {
            foreach (var key in keys[(int)action]) if (keyboard[key].wasPressedThisFrame) return true;
            return false;
        }
    }
}
