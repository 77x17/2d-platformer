using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        void ClickVisible(string name)
        {
            var button = VisibleButton(name);
            if (!button) throw new InvalidOperationException("Missing button: " + name);
            button.onClick.Invoke();
        }

        IEnumerator ReviewFestival()
        {
            game.tutorials.Enabled = false;
            var choices = game.hud.GetComponentsInChildren<Button>().Where(b => b.IsInteractable()).ToArray();
            Record("home_three_choices", choices.Length == 3 && VisibleButton("Select Chapter") && VisibleButton("Home Settings"),
                string.Join(", ", choices.Select(b => b.name)));
            Capture(game, "home");
            ClickVisible("Home Settings");
            yield return null;
            Capture(game, "controls");
            string originalJump = game.Controls.Label(GameAction.Jump);
            ClickVisible("Binding Jump 1");
            yield return null;
            yield return Press(Key.A);
            bool conflict = game.Controls.Label(GameAction.Jump) == originalJump &&
                game.hud.GetComponentsInChildren<TMP_Text>().Any(t => t.text.Contains("đang dùng"));
            yield return Press(Key.Escape);
            Record("cancel_conflicting_binding", conflict && VisibleButton("Back from Settings") &&
                game.Controls.Label(GameAction.Jump) == originalJump, "A belongs to movement; Esc cancels capture");
            ClickVisible("Binding Jump 1");
            yield return null;
            yield return Press(Key.Q);
            Record("bind_jump_in_home", game.Controls.Label(GameAction.Jump) == "Q", game.Controls.Label(GameAction.Jump));
            ClickVisible("Back from Settings");
            ClickVisible("Select Chapter");
            yield return null;
            Record("play_opens_picker", game.State == GameState.Home && VisibleButton("Chapter 1"), game.State.ToString());
            ClickVisible("Chapter 1");
            while (game.IsLoadingChapter) yield return null;
            if (game.InStory) game.EndStory();
            yield return new WaitForSeconds(.45f);
            int jumps = game.player.Jumps;
            yield return Press(Key.Space);
            bool oldKeyReleased = game.player.Jumps == jumps;
            yield return Press(Key.Q);
            yield return new WaitForSeconds(.10f);
            Record("rebound_key_jumps", oldKeyReleased && game.player.Jumps == jumps + 1, "jumps=" + game.player.Jumps);
            yield return Press(Key.Escape);
            ClickVisible("Pause Settings");
            yield return null;
            ClickVisible("Restore Default Keys");
            Record("pause_settings_reset_keys", game.Controls.Label(GameAction.Jump) == originalJump && Time.timeScale == 0,
                "restored " + game.Controls.Label(GameAction.Jump));
            ClickVisible("Back from Settings");
            yield return null;
            yield return Press(Key.Escape);
            yield return new WaitForSeconds(.65f);

            yield return MeasureStar(false);
            yield return Press(Key.Tab);
            yield return MeasureStar(true);
            yield return Press(Key.Tab);
            for (int chapter = 1; chapter <= 4; chapter++)
            {
                while (game.IsLoadingChapter) yield return null;
                var decks = game.ChapterSource.platforms.Where(s => s.kind == ChapterSurfaceKind.ExplorationDeck ||
                    s.kind == ChapterSurfaceKind.ExplorationShelf).ToArray();
                Record("alternate_routes_" + chapter, decks.Length >= 3 && game.TotalMotes == 10,
                    $"branch platforms={decks.Length}, optional motes={game.TotalMotes}");
                yield return ReviewBranch(chapter);
                if (chapter == 3)
                {
                    var card = game.hud.transform.Find("Health and lantern");
                    var labels = card.GetComponentsInChildren<TMP_Text>().Select(t => t.text).ToArray();
                    var lantern = card.Find("Cuoi's paper lantern").GetComponent<Image>();
                    Record("three_hearts_chapter_and_lantern", game.hud.VisibleHeartCount == 3 && labels.Contains("Màn 03") &&
                        labels.All(s => !s.Contains("/3") && !s.Contains("/ 04") && !s.Contains("Sẵn sàng")) &&
                        lantern.sprite.name == "StarLantern", string.Join(" · ",labels));
                    yield return ReviewRiverChanges();
                }
                if (chapter < 4)
                {
                    game.Complete();
                    game.NextChapter();
                }
                else
                {
                    game.Complete();
                    var villageTree = game.level.GetComponentsInChildren<SpriteRenderer>().First(s => s.name == "YoungBanyan");
                    var guardian = game.Boss.GetComponentsInChildren<SpriteRenderer>().First(s => s.name == "Carved Banyan Face");
                    Record("living_banyan_canopies", guardian.sharedMaterial.shader.name == "Moonlit/Banyan canopy" &&
                        guardian.sharedMaterial.shader.isSupported && villageTree.sharedMaterial == guardian.sharedMaterial,
                        "guardian and landmark canopies use " + guardian.sharedMaterial.shader.name);
                }
            }
        }

        IEnumerator ReviewBranch(int chapter)
        {
            // Start on the first landing, then traverse the rest with the normal movement and jump controller.
            string[][] names = {
                new[] { "Irrigation Lookout 01", "Irrigation Lookout Rack", "Irrigation Lookout 02", "Irrigation Lookout 03" },
                new[] { "Charcoal Kiln Eaves 01", "Charcoal Drying Rack", "Charcoal Kiln Eaves 02", "Charcoal Kiln Eaves 03" },
                new[] { "Lower Pontoon Walk 02", "Lower Pontoon Walk 03" },
                new[] { "Ravine Root Ledges 01", "Ravine Root Ledges 02" }
            };
            var route = names[chapter - 1].Select(name => game.ChapterSource.platforms.First(s => s.name == name)).ToArray();
            var prize = game.level.pickups.First(p => p.kind == PickupKind.Mote && Mathf.Abs(p.transform.position.x - route.Last().Top.x) < .4f &&
                Mathf.Abs(p.transform.position.y - route.Last().Top.y - 1) < 1);
            var player = game.player;
            player.TestInput = true;
            player.Teleport(route[0].Top + Vector2.up * .10f);
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.26f);
            int reached = 0;
            for (int i = 1; i < route.Length; i++)
            {
                var goal = route[i].Top;
                var from = route[i-1];
                float edge = from.Top.x + from.Size.x * .5f - .55f;
                float start = Time.time, takeoff = -1;
                bool boosted = false;
                while (Time.time - start < 3)
                {
                    var pos = player.Body.position;
                    float dx = goal.x - pos.x;
                    if (player.Grounded && Mathf.Abs(dx) < .65f && Mathf.Abs(pos.y - goal.y) < .2f) { reached++; break; }
                    player.TestMove = Mathf.Clamp(dx * 4, -1, 1);
                    if (takeoff < 0 && player.Grounded && pos.x >= edge)
                    {
                        player.QueueJump(); takeoff = Time.time;
                    }
                    if (chapter == 2 && !boosted && takeoff >= 0 && Time.time - takeoff > .34f &&
                        pos.y < goal.y + .65f && dx > 1)
                    {
                        player.QueueJump(); boosted = true;
                    }
                    yield return null;
                }
                if (reached != i) break;
            }
            player.TestMove = 0;
            player.TestInput = false;
            Record("exploration_jump_sequence_" + chapter, prize.Collected && reached == route.Length - 1,
                $"{route[0].name} to {route.Last().name}: landings={reached}/{route.Length-1}, mote={prize.Collected}, feet={player.Body.position:F2}");
            Capture(game, "side-route-" + chapter);
        }

        IEnumerator MeasureStar(bool atlas)
        {
            var strip = game.hud.GetComponentsInChildren<MoonlitMapStrip>()
                .First(s => s.transform.Find("Cuoi's map star") && s.transform.Find("Cuoi's map star").gameObject.activeInHierarchy);
            var star = strip.transform.Find("Cuoi's map star").GetComponent<Image>();
            float began = Time.unscaledTime, smallest = 2, largest = 0, elapsed = game.Elapsed;
            while (Time.unscaledTime - began < 1.55f)
            {
                smallest = Mathf.Min(smallest, star.rectTransform.localScale.x);
                largest = Mathf.Max(largest, star.rectTransform.localScale.x);
                yield return null;
            }
            Record(atlas ? "star_blinks_on_paused_map" : "star_blinks_on_minimap",
                star.isActiveAndEnabled && star.sprite.name == "EmblemStar" && largest - smallest > .20f &&
                (!atlas || Mathf.Approximately(elapsed,game.Elapsed)), $"scale {smallest:F2} to {largest:F2}");
        }

        IEnumerator ReviewRiverChanges()
        {
            var scene = game.level;
            var player = game.player;
            var sprites = scene.GetComponentsInChildren<SpriteRenderer>();
            var boats = sprites.Where(s => s.name == "Moored River Sampan").ToArray();
            var reaches = sprites.Where(s => s.name == "River Surface").ToArray();
            var water = new MaterialPropertyBlock();
            bool afloat = boats.Length > 0 && boats.All(boat => reaches.Any(reach => {
                reach.GetPropertyBlock(water);
                var bounds = boat.bounds;
                float surface = water.GetVector("_Waterline").x;
                return reach.bounds.min.x < bounds.min.x + .12f && reach.bounds.max.x > bounds.max.x - .12f &&
                    surface > bounds.min.y && surface < bounds.center.y && reach.sortingOrder > boat.sortingOrder;
            }));
            Record("sampans_meet_water", afloat, $"moored boats={boats.Length}, river reaches={reaches.Length}");
            var shrine = scene.lanternSwitch;
            player.Teleport((Vector2)shrine.transform.position + Vector2.up * .08f);
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.25f);
            yield return Press(Key.E);
            float began = Time.time;
            bool sawTravel = false;
            while (Time.time - began < 5 && !shrine.Open)
            {
                if (!sawTravel && shrine.Rise > .35f && shrine.Rise < .80f)
                {
                    sawTravel = true;
                    Capture(game, "bridge-rising");
                }
                yield return null;
            }
            Record("shrine_bridge_emerges", shrine.Open && sawTravel && Time.time - began > .55f,
                $"elapsed={Time.time-began:F2}, intermediate pose={sawTravel}");

            var ferry = scene.Ferry;
            var start = ferry.Float.PhysicsPosition;
            var token = scene.pickups.First(p => p.kind == PickupKind.Mote && Mathf.Abs(p.transform.position.x - 491) < 2);
            player.Teleport(start + Vector2.up * .1f);
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.35f);
            yield return Press(Key.E);
            bool outbound = ferry.State == SluiceFerry.LockState.GoingRight;
            bool jumped = false, photographed = false, landed = false;
            float maxCableGap = 0;
            began = Time.time;
            while (Time.time - began < 8 && ferry.State == SluiceFerry.LockState.GoingRight)
            {
                float toMote = token.transform.position.x - ferry.Float.PhysicsPosition.x;
                if (!jumped && toMote < 1.4f && player.Grounded)
                {
                    jumped = true;
                    player.TestInput = true;
                    player.TestMove = .75f;
                    player.QueueJump();
                }
                var attachment = ferry.Hanger.GetPosition(0);
                float gap = float.PositiveInfinity;
                for (int i = 1; i < ferry.Cable.positionCount; i++)
                {
                    Vector2 a = ferry.Cable.GetPosition(i - 1), b = ferry.Cable.GetPosition(i);
                    if (attachment.x < a.x || attachment.x > b.x) continue;
                    gap = Mathf.Abs(attachment.y - Mathf.Lerp(a.y, b.y, Mathf.InverseLerp(a.x, b.x, attachment.x)));
                    break;
                }
                maxCableGap = Mathf.Max(maxCableGap, gap);
                if (jumped)
                {
                    float error = ferry.Float.PhysicsPosition.x - player.Body.position.x;
                    player.TestMove = Mathf.Clamp(.8f + error*1.7f, -1f, 1f);
                    if (player.Grounded && ferry.Boarded(player.Body.position))
                    {
                        if (photographed) landed = true;
                        player.TestMove = 0;
                    }
                }
                if (!photographed && token.Collected && jumped && !player.Grounded)
                {
                    Capture(game, "ferry-moon-mote");
                    photographed = true;
                }
                yield return null;
            }
            player.TestMove = 0;
            player.TestInput = false;
            Record("jump_for_ferry_mote", outbound && jumped && token.Collected && landed,
                $"sailed={outbound}, jumped={jumped}, collected={token.Collected}, landed on raft={landed}, token={token.transform.position:F2}");
            Record("ferry_hanger_follows_cable", ferry.Cable.positionCount > 3 && maxCableGap < .15f, $"maximum gap={maxCableGap:F3}");
            while (ferry.State == SluiceFerry.LockState.GoingRight && Time.time - began < 9) yield return null;
            // Leave the boat on the far bank. Both shore levers must also operate an empty deck.
            player.Teleport(new Vector2(476.65f,4.18f));
            yield return new WaitForSeconds(.24f);
            yield return Press(Key.E);
            bool fromLeft = ferry.State == SluiceFerry.LockState.GoingLeft;
            string leftResult = $"{ferry.State} at {player.Body.position:F2}, grounded={player.Grounded}";
            began = Time.time;
            while (ferry.State == SluiceFerry.LockState.GoingLeft && Time.time - began < 7) yield return null;
            player.Teleport(new Vector2(505.5f,4.42f));
            yield return new WaitForSeconds(.24f);
            yield return Press(Key.E);
            bool fromRight = ferry.State == SluiceFerry.LockState.GoingRight;
            Record("call_empty_ferry_from_either_bank", fromLeft && fromRight, $"left lever={fromLeft} ({leftResult}), right lever={fromRight} ({ferry.State} at {player.Body.position:F2}, grounded={player.Grounded})");
        }
    }
}
