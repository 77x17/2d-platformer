using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        IEnumerator ReviewLanternUpdate()
        {
            game.tutorials.Enabled = false;
            game.StartChapter(1, false);
            while (game.IsLoadingChapter) yield return null;
            yield return new WaitForSeconds(.4f);
            var player = game.player;
            var meter = player.transform.Find("Lantern recharge");
            var fill = meter.Find("Lantern charge");
            bool hiddenAtRest = !meter.gameObject.activeSelf;
            yield return Press(Key.J);
            yield return new WaitForSeconds(.25f);
            bool aboveTrack = meter.gameObject.activeInHierarchy && meter.position.y < player.transform.position.y;
            float initial = fill.localScale.x;
            yield return Press(Key.Space);
            yield return new WaitForSeconds(.18f);
            bool anchored = !player.Grounded && Mathf.Abs((meter.position.y - player.transform.position.y) + .21f) < .01f &&
                Mathf.Abs(meter.position.x - player.transform.position.x) < .01f && fill.localScale.x > initial;
            Capture(game, "lantern-recharging");
            yield return Press(Key.Escape);
            bool paused = game.State == GameState.Paused && !meter.gameObject.activeInHierarchy;
            yield return Press(Key.Escape);
            yield return new WaitForSeconds(game.tuning.pulseCooldown + .1f);
            Record("lantern_meter_under_feet", hiddenAtRest && aboveTrack && anchored && paused && !meter.gameObject.activeInHierarchy,
                $"ready hidden={hiddenAtRest}, below feet while charging={aboveTrack}, follows jump={anchored}, hidden in pause={paused}");
            yield return ReviewDownControls();
            yield return ReviewBanyanAndLanternBridge();
        }
    }
}
