using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        IEnumerator ReviewDownControls()
        {
            var p = game.player;
            var capsule = p.GetComponent<CapsuleCollider2D>();
            p.TestInput = false;
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            p.Teleport(new Vector2(8, .06f));
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.24f);
            var start = p.Body.position;
            var poses = new HashSet<Sprite>();
            float longestHold = 0, changedAt = Time.time, end = Time.time + 3.95f;
            var last = p.body.sprite;
            while (Time.time < end)
            {
                yield return null;
                var sprite = p.body.sprite;
                poses.Add(sprite);
                if (last == sprite) continue;
                longestHold = Mathf.Max(longestHold, Time.time - changedAt);
                last = sprite; changedAt = Time.time;
            }
            Record("idle_tempo_and_contacts", poses.Count == 7 && longestHold < .47f && Vector2.Distance(start,p.Body.position) < .035f,
                $"poses={poses.Count}; longest uninterrupted pose={longestHold:F2}s; foot drift={Vector2.Distance(start,p.Body.position):F3}");

            float standing = capsule.size.y;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
            yield return new WaitForSeconds(.32f);
            bool crouched = p.IsCrouching && p.AnimationRow == 6 && p.AnimationFrame >= 2 &&
                p.body.sprite.name.StartsWith("CuoiCrouch") && capsule.size.y < standing - .35f && Mathf.Abs(start.y - p.Body.position.y) < .04f;
            Capture(game, "cuoi-ground-crouch");
            var ceiling = new GameObject("QA low clearance") { layer = 8 };
            ceiling.transform.position = p.Body.position + Vector2.up * 1.38f;
            ceiling.AddComponent<BoxCollider2D>().size = new Vector2(2, .2f);
            Physics2D.SyncTransforms();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(.18f);
            bool safe = p.IsCrouching && capsule.bounds.max.y < ceiling.GetComponent<BoxCollider2D>().bounds.min.y;
            ceiling.SetActive(false);
            Destroy(ceiling);
            yield return new WaitForSeconds(.25f);
            Record("crouch_s_under_low_ceiling", crouched && safe && !p.IsCrouching && Mathf.Approximately(standing,capsule.size.y),
                $"S lowers art and contact shape={crouched}; release under beam={safe}; stood after clearance={!p.IsCrouching}");

            bool rebound = game.Controls.Rebind(GameAction.Down, 0, Key.C, out _);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
            yield return new WaitForSeconds(.14f);
            bool oldReleased = !p.IsCrouching;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.C));
            yield return new WaitForSeconds(.20f);
            Record("rebound_down_reaches_controller", rebound && oldReleased && p.IsCrouching,
                $"rebound S to C={rebound}; S inactive={oldReleased}; C crouched={p.IsCrouching}");
            game.Controls.Rebind(GameAction.Down, 0, Key.S, out _);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());

            // Exercise a real authored side route, including the lower bamboo deck.
            p.Teleport(new Vector2(49, 5.79f));
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.24f);
            bool onLookout = p.Grounded && Mathf.Abs(p.Body.position.y - 5.75f) < .15f;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.DownArrow));
            yield return new WaitForSeconds(1.10f);
            bool caught = p.Grounded && Mathf.Abs(p.Body.position.y - .85f) < .15f;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(.12f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
            yield return new WaitForSeconds(.43f);
            bool leftLower = p.Body.position.y < .55f;
            Record("drop_to_next_platform_once", onLookout && caught && leftLower,
                $"on authored upper perch={onLookout}; held Down Arrow stopped at lower deck={caught}; fresh S left lower deck={leftLower}");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            p.Teleport(game.level.StartPoint);
            game.cameraRig.Snap();
            yield return new WaitForSeconds(.22f);
        }
    }
}
