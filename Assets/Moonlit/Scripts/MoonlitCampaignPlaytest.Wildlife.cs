using System.Collections;
using System.IO;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        void CheckPanorama(int chapter)
        {
            var camera = game.cameraRig.GetComponent<Camera>();
            var horizons = System.Array.FindAll(game.cameraRig.GetComponentsInChildren<SpriteRenderer>(), sr => sr.enabled && sr.sortingOrder == -120);
            var initialPosition = game.player.Body.position;
            bool covered = horizons.Length == 1;
            int samples = 0;
            foreach (float aspect in new[] { 4f / 3, 16f / 9, 21f / 9, 32f / 9 })
            {
                camera.aspect = aspect;
                foreach (float x in new[] { 3f, game.level.RouteLength * .5f, game.level.RouteLength - 2 })
                foreach (float y in new[] { .05f, 11f })
                {
                    game.player.Teleport(new Vector2(x,y)); game.cameraRig.Snap();
                    for (int i = 0; i <= 12; i++)
                    for (int row = 0; row < 3; row++)
                    {
                        Vector3 p = camera.ViewportToWorldPoint(new Vector3(i / 12f, row * .5f, 10));
                        bool inside = false;
                        foreach (var sky in horizons)
                        {
                            var box = sky.bounds;
                            if (p.x >= box.min.x - .001f && p.x <= box.max.x + .001f && p.y >= box.min.y - .001f && p.y <= box.max.y + .001f) inside = true;
                        }
                        covered &= inside;
                        samples++;
                    }
                }
            }
            camera.ResetAspect(); game.player.Teleport(initialPosition); game.cameraRig.Snap();
            Record("sky_coverage_" + chapter,covered,$"{horizons.Length} continuous horizon, {samples} viewport samples at 3 route positions, 2 heights, aspect 4:3 through 32:9");
        }

        // These observations use the actual water shader's waterline and the visible
        // roost branch. Physics checks sample the walkable escape space on each bank.
        void CheckHabitats(int chapter)
        {
            Physics2D.SyncTransforms();
            var scenery = game.level.GetComponentsInChildren<SpriteRenderer>();
            var block = new MaterialPropertyBlock();
            int fish = 0, crows = 0, placed = 0;
            foreach (var animal in game.level.Wildlife)
            {
                var at = animal.Anchor;
                bool supported = false;
                if (animal.Kind == WetlandSpecies.Fish)
                {
                    fish++;
                    foreach (var sprite in scenery)
                    {
                        if (sprite.name != "Dòng nước" || at.x < sprite.bounds.min.x || at.x > sprite.bounds.max.x) continue;
                        sprite.GetPropertyBlock(block);
                        if (Mathf.Abs(block.GetVector("_Waterline").x - at.y) > .09f) continue;
                        supported = !Physics2D.OverlapPoint(at, 1 << 8) &&
                            (FlatBank(at + new Vector2(-1.5f, 5)) || FlatBank(at + new Vector2(1.5f, 5)));
                        if (supported) break;
                    }
                }
                else
                {
                    crows++;
                    foreach (var sprite in scenery)
                    {
                        var bounds = sprite.bounds;
                        if (sprite.name != "Roost branch" || at.x < bounds.min.x || at.x > bounds.max.x || Mathf.Abs(bounds.max.y - at.y) > .18f) continue;
                        supported = FlatBank(at + Vector2.left * 3) && FlatBank(at + Vector2.right * 3);
                        if (supported) break;
                    }
                }
                if (supported) placed++;
            }
            int expected = chapter == 1 || chapter == 4 ? 1 : 2;
            bool composition = chapter == 1 || chapter == 3 ? fish == expected && crows == 0 : crows == expected && fish == 0;
            Record("wildlife_habitats_" + chapter, composition && placed == expected, $"{fish} fish, {crows} crows, {placed} matched visible water/branch and nearby firm bank");
        }

        static bool FlatBank(Vector2 from)
        {
            var hit = Physics2D.Raycast(from, Vector2.down, 8, 1 << 8);
            return hit && hit.normal.y > .95f;
        }

        void CheckTerrain(int chapter)
        {
            var terrain = game.level.GetComponent<TerrainArt>();
            bool valid = terrain && terrain.MeshCount > 0 && terrain.JoinedEdges > 0 && terrain.ExposedEdges > 0 &&
                terrain.ExposedEdges + terrain.JoinedEdges == 2 * terrain.MeshCount;
            int observed = 0;
            foreach (var filter in game.level.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = filter.sharedMesh;
                if (!mesh || !mesh.name.StartsWith("Địa tầng ")) continue;
                observed++;
                var bounds = mesh.bounds;
                valid &= bounds.size.x > .1f && bounds.size.y > .1f && mesh.vertexCount >= 12 && mesh.uv2.Length == mesh.vertexCount;
                foreach (var vertex in mesh.vertices)
                    valid &= !float.IsNaN(vertex.sqrMagnitude) && !float.IsInfinity(vertex.sqrMagnitude);
                var surface = filter.GetComponent<MeshRenderer>().sharedMaterial;
                valid &= surface && surface.shader.isSupported;
            }
            Record("terrain_treads_" + chapter, valid && terrain && observed == terrain.MeshCount,
                terrain ? $"{observed} tread meshes, {terrain.JoinedEdges} joined edges, {terrain.ExposedEdges} exposed edges" : "Missing terrain owner");
        }

        static Vector2 WildlifeStand(int chapter) => chapter == 2 ? new Vector2(153, 2.84f) : new Vector2(219.08f, 2.09f);
        static Vector2 WildlifeRefuge(int chapter) => WildlifeStand(chapter) + Vector2.right * (chapter == 2 ? -6f : 6f);

        IEnumerator CheckWildlife(int chapter)
        {
            var animal = game.level.Wildlife.Count > 0 ? game.level.Wildlife[0] : null;
            if (!animal) { Record("wildlife_encounter_" + chapter, false, "No encounter in authored level"); yield break; }
            string name = chapter == 2 ? "crow" : "fish";
            var p = game.player;
            var spot = WildlifeStand(chapter);
            p.TestInput = true; p.TestMove = 0; p.ReleaseJump();
            p.Teleport(WildlifeRefuge(chapter));
            while (p.Cooldown > 0) yield return null;
            p.Teleport(spot); game.cameraRig.Snap();
            float deadline = Time.time + 3;
            while (!animal.Warning && Time.time < deadline) yield return null;
            bool warned = animal.Warning;
            int hp = game.Health, stuns = animal.StunCount;
            yield return new WaitForSeconds(.15f);
            bool flashed = p.Pulse();
            yield return null;
            Record(name + "_warning_pulse", warned && flashed && animal.IsStunned && animal.StunCount == stuns + 1 && game.Health == hp,
                $"warned={warned}, flash={flashed}, stuns={animal.StunCount}, health={game.Health}");

            p.Teleport(WildlifeRefuge(chapter)); game.cameraRig.Snap();
            deadline = Time.time + 15;
            while ((animal.Phase != WetlandPhase.Resting || p.Cooldown > 0) && Time.time < deadline) yield return null;
            Record(name + "_recovers_from_light", animal.Phase == WetlandPhase.Resting, animal.Phase.ToString());
            p.Teleport(spot); game.cameraRig.Snap();
            int hits = animal.HitCount, attacks = animal.AttackCount;
            bool telegraph = false, active = false, recovered = false, photographed = false;
            float warningAt = -1, attackAt = -1;
            deadline = Time.time + 8;
            while (Time.time < deadline)
            {
                if (animal.Warning && !telegraph) { telegraph = true; warningAt = Time.time; }
                if (animal.Phase == WetlandPhase.Attacking)
                {
                    if (!active) { active = true; attackAt = Time.time; }
                    bool inFrame = chapter == 3 ? animal.BodyCenter.y > spot.y + .45f : animal.BodyCenter.y < spot.y + 2.3f;
                    if (!photographed && inFrame) { Capture(game, "encounter-" + name); photographed = true; }
                }
                if (active && animal.Phase != WetlandPhase.Attacking) { recovered = true; break; }
                yield return null;
            }
            yield return new WaitForSeconds(.35f);
            float lead = attackAt - warningAt;
            Record(name + "_attack_sequence", telegraph && active && recovered && photographed && lead >= .55f && animal.AttackCount == attacks + 1,
                $"lead={lead:F2}s, attacks={animal.AttackCount-attacks}, recovery={animal.Phase}, camera={photographed}");
            Record(name + "_contact_once", animal.HitCount == hits + 1 && game.Health == hp - 1,
                $"damage events={animal.HitCount-hits}, hearts={hp}→{game.Health}");
        }

        IEnumerator FilmWildlife(string frames, int chapter)
        {
            game.StartChapter(chapter);
            var p = game.player;
            var animal = game.level.Wildlife[0];
            p.TestInput = true; p.TestMove = 0;
            p.Teleport(WildlifeRefuge(chapter)); game.cameraRig.Snap();
            yield return new WaitForSeconds(2.3f);
            p.Teleport(WildlifeStand(chapter)); game.cameraRig.Snap();
            string section = Path.Combine(frames, chapter == 2 ? "crow" : "fish");
            Directory.CreateDirectory(section);
            int warningFrame = -1, attackFrame = -1, returnFrame = -1, initialHealth = game.Health;
            float retreatDirection = chapter == 2 ? -1f : 1f;
            Time.captureDeltaTime = 1f / 30;
            for (int i = 0; i < 150; i++)
            {
                int after = i - warningFrame;
                // One short sidestep after the warning. The strike plays out in full on camera.
                if (warningFrame >= 0 && after >= 15 && after < 25) p.TestMove = retreatDirection;
                else if (warningFrame >= 0 && after >= 92 && after < 95) p.TestMove = -retreatDirection;
                else p.TestMove = 0;
                yield return null;
                if (warningFrame < 0 && animal.Warning) warningFrame = i;
                if (attackFrame < 0 && animal.Phase == WetlandPhase.Attacking) attackFrame = i;
                if (returnFrame < 0 && animal.Phase == WetlandPhase.Returning) returnFrame = i;
                MoonlitCapture.Save(Path.Combine(section, i.ToString("D4") + ".png"), 960, 540);
            }
            p.TestMove = 0; Time.captureDeltaTime = 0;
            File.WriteAllText(Path.Combine(section, "cues.csv"),
                string.Format(System.Globalization.CultureInfo.InvariantCulture, "warningFrame,attackFrame\n{0},{1}\n", warningFrame, attackFrame));
            Record("escape_film_" + (chapter == 2 ? "crow" : "fish"), warningFrame >= 0 && attackFrame > warningFrame && returnFrame > attackFrame && game.Health == initialHealth,
                $"warning={warningFrame}, attack={attackFrame}, return={returnFrame}, hearts={game.Health}");
        }
    }
}
