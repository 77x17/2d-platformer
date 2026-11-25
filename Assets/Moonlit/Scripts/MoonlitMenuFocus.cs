using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed class MoonlitMenuFocus : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        MoonlitGame game;
        Button button;
        Image rail;
        bool selected, hovered;

        public void Configure(MoonlitGame owner, Button target, Image focusRail)
        {
            game = owner;
            button = target;
            rail = focusRail;
            rail.enabled = false;
        }
        public void OnSelect(BaseEventData e)
        {
            selected = true;
            if (e is AxisEventData && game) game.audioFx.Play("uiMove");
            Refresh();
        }
        public void OnDeselect(BaseEventData e) { selected = false; Refresh(); }
        public void OnPointerEnter(PointerEventData e)
        {
            hovered = true;
            if (button && button.interactable && game) game.audioFx.Play("uiMove");
            Refresh();
        }
        public void OnPointerExit(PointerEventData e) { hovered = false; Refresh(); }
        void OnDisable() { selected = hovered = false; Refresh(); }
        void Refresh() { if (rail) rail.enabled = button && button.interactable && (hovered || selected); }
    }
}
