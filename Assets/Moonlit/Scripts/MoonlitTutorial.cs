using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonlit
{
    [DefaultExecutionOrder(20)]
    public sealed class MoonlitTutorial : MonoBehaviour
    {
        public const int Total = 6;
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
        ForestCreature highlightedCreature;

        public void Initialize(MoonlitGame owner) { game = owner; }

        void Update()
        {
            if (Active)
            {
                FocusAmount = Mathf.MoveTowards(FocusAmount, closing ? 0 : 1, Time.unscaledDeltaTime / (closing ? .45f : .55f));
                if (closing && FocusAmount <= 0)
                {
                    current = -1; closing = false;
                    if (highlightedCreature) highlightedCreature.TutorialPreview = false;
                    highlightedCreature = null; game.EndTutorial();
                }
                else if (CanContinue)
                {
                    var kb = Keyboard.current;
                    if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) Continue();
                }
                return;
            }
            if (!Enabled || game.State != GameState.Playing || !game.player.Grounded) return;
            float x = game.player.Body.position.x;
            // Every lesson starts from a safe, grounded area before the actual challenge.
            if (!seen[0] && game.Elapsed > .20f) Open(0);
            else if (!seen[1] && x >= 28.8f && x < 43) Open(1);
            else if (!seen[2] && x >= 73.5f && x < 79) Open(2);
            else if (!seen[3] && x >= 101.5f && x < 111) Open(3);
            else if (!seen[4] && x >= 111 && x < 114) Open(4);
            else if (!seen[5] && x >= 169 && x < 176) Open(5);
        }

        public bool Open(int id)
        {
            if (Active || id < 0 || id >= Total || game.State != GameState.Playing) return false;
            current = id; if (!seen[id]) { seen[id] = true; SeenCount++; }
            FocusAmount = 0; openedAt = Time.unscaledTime; closing = false;
            highlightedCreature = null;
            switch (id)
            {
                case 0:
                    focus = game.player.transform; focusOffset = Vector2.up * .8f; FocusRadius = 2.1f;
                    Subject = "CUỘI & NGỌN ĐÈN"; Title = "BƯỚC VÀO RỪNG ĐA";
                    Description = "Đi sang phải theo vùng sáng. Đồng trăng vàng có thể nhặt; chấm xanh nhỏ chỉ là đom đóm.\n\nGiữ Space để nhảy cao; nhả sớm để đáp gần hơn.";
                    Action = "A / D  DI CHUYỂN     SPACE  NHẢY"; break;
                case 1:
                    highlightedCreature = game.enemies[0]; focus = highlightedCreature.transform; focusOffset = Vector2.up * 1.1f; FocusRadius = 2.4f;
                    Subject = "CHÓ RỪNG"; Title = "THẤY DẤU ! — CHUẨN BỊ NÉ";
                    Description = "Chó dừng lại và hiện dấu ! trước khi lao tới.\n\nĐến gần vừa đủ rồi bấm F để làm choáng, hoặc nhảy qua nó. Đèn cần 3 giây để hồi sáng.";
                    Action = "F  LÀM CHOÁNG     SPACE  NHẢY QUA"; break;
                case 2:
                    focus = game.level.moving[0].transform; focusOffset = Vector2.up * 1.5f; FocusRadius = 3.8f;
                    Subject = "BỆ GỖ TREO"; Title = "CHỜ BỆ — RỒI MỚI NHẢY";
                    Description = "Bệ gỗ được treo dưới rễ Đa và nâng lên, hạ xuống theo nhịp.\n\nChờ bệ ở gần độ cao của Cuội, nhảy lên giữa bệ. Bạn có thể đứng yên để bệ mang theo rồi nhảy tiếp.";
                    Action = "QUAN SÁT NHỊP     SPACE  LÊN BỆ"; break;
                case 3:
                    highlightedCreature = game.enemies[1]; focus = highlightedCreature.transform; focusOffset = Vector2.up * 1.05f; FocusRadius = 2.5f;
                    Subject = "MÈO RỪNG"; Title = "MÈO VỒ CAO VÀ XA HƠN";
                    Description = "Mèo có thể vồ lên cao. Dấu ! cho bạn một khoảng ngắn để phản ứng.\n\nDừng ngoài tầm vồ, đợi đèn sẵn sàng rồi bấm F khi mèo đến gần. Không cần đối đầu trực tiếp.";
                    Action = "GIỮ KHOẢNG CÁCH     F  LÀM CHOÁNG"; break;
                case 4:
                    focus = game.level.falling[0].transform; focusOffset = Vector2.up * .25f; FocusRadius = 2.8f;
                    Subject = "CẦU GỖ MỤC"; Title = "ĐÁP XUỐNG — NHẢY TIẾP";
                    Description = "Ván gỗ rung trước khi sụp. Bạn có 0,7 giây kể từ lúc đứng lên ván.\n\nNhảy liên tiếp qua ba nhịp cầu. Nếu lỡ rơi, Cuội trở về đèn đã thắp và cầu sẽ được đặt lại.";
                    Action = "GIỮ HƯỚNG PHẢI     SPACE  NHẢY TIẾP"; break;
                default:
                    focus = game.level.lanternSwitch.transform; focusOffset = Vector2.up * 1.3f; FocusRadius = 2.8f;
                    Subject = "MIẾU ĐÈN"; Title = "THẮP ĐÈN ĐỂ NỐI LỐI ĐI";
                    Description = "Cây cầu phía trước đang ngủ. Đứng cạnh miếu và nhấn E để đánh thức nó.\n\nMiếu giữ cây cầu mở trong phần còn lại của lượt chơi, kể cả sau khi hồi sinh.";
                    Action = "ĐẾN CẠNH MIẾU     E  THẮP ĐÈN"; break;
            }
            lastFocus = FocusPosition;
            if (highlightedCreature) highlightedCreature.TutorialPreview = true;
            game.BeginTutorial(); return true;
        }

        public void Continue() { if (CanContinue) { closing = true; game.player.ClearInput(.25f); } }
        public void Cancel()
        {
            if (highlightedCreature) highlightedCreature.TutorialPreview = false;
            highlightedCreature = null; current = -1; FocusAmount = 0; closing = false;
        }
        public void ResetForRun() { Cancel(); System.Array.Clear(seen, 0, seen.Length); SeenCount = 0; }
    }
}
