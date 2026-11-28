using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonlit
{
    [DefaultExecutionOrder(20)]
    public sealed class MoonlitTutorial : MonoBehaviour
    {
        public const int Total = 3;
        public bool Enabled { get; set; } = true;
        public bool Active => current >= 0;
        public int CurrentId => current;
        public int SeenCount { get; private set; }
        public float FocusAmount { get; private set; }
        public float FocusRadius { get; private set; }
        public Vector2 FocusPosition => focus ? (Vector2)focus.position + focusOffset : lastFocus;
        public bool CanContinue => Active && !closing && Time.unscaledTime - openedAt >= .85f && FocusAmount > .98f;
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Action { get; private set; }
        public string Subject { get; private set; }
        public float CardOpacity => Mathf.SmoothStep(0, 1, FocusAmount);

        MoonlitGame game;
        readonly bool[] seen = new bool[Total];
        int current = -1;
        float openedAt;
        bool closing;
        Transform focus;
        Vector2 focusOffset, lastFocus;

        public void Initialize(MoonlitGame owner) { game = owner; }

        void Update()
        {
            if (Active)
            {
                FocusAmount = Mathf.MoveTowards(FocusAmount, closing ? 0 : 1, Time.unscaledDeltaTime / (closing ? .45f : .55f));
                if (closing && FocusAmount <= 0)
                {
                    current = -1; closing = false;
                    game.EndTutorial();
                }
                else if (CanContinue)
                {
                    var kb = Keyboard.current;
                    if (kb != null && game.Controls.Pressed(GameAction.Jump, kb)) Continue();
                }
                return;
            }
            if (!Enabled || game.State != GameState.Playing || !game.player.Grounded) return;
            if (game.Chapter > 1 && !seen[game.Chapter - 2] && game.Elapsed > 1.4f && game.player.Body.position.x < 15)
                Open(game.Chapter - 2);
        }

        public bool Open(int id)
        {
            if (Active || id < 0 || id >= Total || game.State != GameState.Playing) return false;
            current = id;
            if (!seen[id]) { seen[id] = true; SeenCount++; }
            FocusAmount = 0; openedAt = Time.unscaledTime; closing = false;
            focus = game.player.transform; focusOffset = Vector2.up; FocusRadius = 3.4f;
            Subject = "MẢNH TRĂNG " + (id + 1);
            switch (id)
            {
                case 0:
                    Title = "Nhảy đôi";
                    Description = "Bậc tre cao quá một lần nhảy. Khi Cuội gần lên tới đỉnh, bấm nhảy thêm lần nữa.";
                    Action = $"{game.Controls.Label(GameAction.Jump)} hai lần"; break;
                case 1:
                    Title = "Leo vách";
                    Description = "Nhảy sát bờ kè. Giữ phím hướng vào vách cùng phím nhảy để Cuội leo lên. Buông phím nhảy để bám tại chỗ.";
                    Action = $"Giữ hướng vào vách + {game.Controls.Label(GameAction.Jump)}"; break;
                default:
                    Title = "Lướt";
                    Description = "Nhảy đôi từ mép đá rồi lướt qua khe. Mỗi lần chạm đất, Cuội lại có thể lướt thêm một lần trên không.";
                    Action = $"{game.Controls.Label(GameAction.Dash)} để lướt"; break;
            }
            lastFocus = FocusPosition;
            game.BeginTutorial(); return true;
        }

        public void Continue() { if (CanContinue) { closing = true; game.player.ClearInput(.25f); } }
        public void Cancel()
        {
            current = -1; FocusAmount = 0; closing = false;
        }
        public void ResetForRun() { Cancel(); System.Array.Clear(seen, 0, seen.Length); SeenCount = 0; }
    }
}


