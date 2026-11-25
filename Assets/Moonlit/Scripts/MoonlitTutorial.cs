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
                    if (kb != null && kb.spaceKey.wasPressedThisFrame) Continue();
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
                    Description = "Nhảy lên, rồi bấm nhảy lần nữa gần đỉnh quỹ đạo để tới bậc trúc cao. Hai lượt nhảy hồi lại khi chân chạm đất.";
                    Action = "SPACE   ·   SPACE"; break;
                case 1:
                    Title = "Leo tường";
                    Description = "Nhảy sát bờ kè có dây leo. Giữ hướng vào vách và giữ phím nhảy cho tới khi Cuội trèo qua mép.";
                    Action = "GIỮ HƯỚNG VÀO VÁCH + SPACE"; break;
                default:
                    Title = "Lướt";
                    Description = "Chạy lấy đà và nhảy đôi ở mép khe. Bấm lướt khi còn trên không để tới bờ bên kia. Cuội lại lướt được sau khi tiếp đất.";
                    Action = "SHIFT hoặc K   ·   LƯỚT"; break;
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


