using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonlit
{
    // One owner per chapter releases its generated terrain meshes when the chapter unloads.
    public sealed class TerrainArt : MonoBehaviour
    {
        static readonly string[] Textures = { "VillageEarth", "BambooLoam", "RiverStone", "BanyanSlate" };
        // Rows of the painted tread atlas at the plane on which the character stands.
        static readonly float[] CrestRows = { 150, 370, 571, 806 };
        static readonly Material[] Materials = new Material[4];
        static readonly float[] DeepRows = { -.78f, 0, .20f, .43f, .76f, 1.20f, 1.82f, 2.58f, 3.54f, 4.68f, 6.02f, 7.58f, 9.42f, 12 };
        static readonly float[] ShelfRows = { -.78f, 0, .16f, .34f, .54f, .72f };
        static readonly int Lamp = Shader.PropertyToID("_CuoiTerrainLight");
        readonly List<Mesh> meshes = new List<Mesh>();
        readonly List<Vector3> rootContacts = new List<Vector3>();

        public static void AddRootContact(GameObject tree, Vector2 basePoint, float radius)
        {
            Owner(tree).rootContacts.Add(new Vector3(basePoint.x, basePoint.y, radius));
        }

        // Baked onto the bank mesh so the root shadow can only appear on actual painted ground.
        float RootLight(float x, float top, float depth)
        {
            if (depth > .54f) return 1;
            float occlusion = 0;
            foreach (var root in rootContacts)
            {
                if (Mathf.Abs(top - root.y) > .02f) continue;
                float w = Mathf.Clamp01(1 - Mathf.Abs(x - root.x) / root.z);
                occlusion = Mathf.Max(occlusion, Mathf.SmoothStep(0, 1, w));
            }
            return 1 - .37f * occlusion * (1 - Mathf.SmoothStep(0, 1, Mathf.Max(0, depth) / .54f));
        }

        public int MeshCount => meshes.Count;
        public int JoinedEdges { get; private set; }
        public int ExposedEdges { get; private set; }

        public static void LightAt(Vector2 at, float flare) => Shader.SetGlobalVector(Lamp, new Vector4(at.x, at.y, flare, 1));

        public static void DressBanks(int stage, List<BoxCollider2D> banks)
        {
            Physics2D.SyncTransforms();
            var bounds = new Bounds[banks.Count];
            for (int i = 0; i < banks.Count; i++) bounds[i] = banks[i].bounds;
            for (int index = 0; index < banks.Count; index++)
            {
                Bounds box = bounds[index];
                float left = box.size.y, right = box.size.y;
                for (int neighbor = 0; neighbor < bounds.Length; neighbor++)
                {
                    if (neighbor == index) continue;
                    Bounds next = bounds[neighbor];
                    float drop = Mathf.Clamp(box.max.y - next.max.y, 0, box.size.y);
                    if (Mathf.Abs(next.max.x - box.min.x) < .025f) left = Mathf.Min(left, drop);
                    if (Mathf.Abs(next.min.x - box.max.x) < .025f) right = Mathf.Min(right, drop);
                }
                DressFace(stage, banks[index].gameObject, box.min.x, box.max.x, box.max.y, left, right, false);
            }
        }

        public static SpriteRenderer DressLedge(MoonlitGame game, int stage, GameObject ledge, float centerX, float top, float width, bool wood)
        {
            if (wood)
            {
                const float thickness = .85f;
                return Visuals.Band("Timber deck with end brackets", game.art.forestExtras[11],
                    new Vector2(centerX, top + .12f - thickness * .5f), width, thickness, 19, ledge.transform, 3);
            }
            DressFace(stage, ledge, centerX - width / 2, centerX + width / 2, top, .72f, .72f, true);
            return null;
        }

        static TerrainArt Owner(GameObject ground)
        {
            var level = ground.GetComponentInParent<MoonlitLevel>();
            if (!level.TryGetComponent<TerrainArt>(out var owner)) owner = level.gameObject.AddComponent<TerrainArt>();
            return owner;
        }

        static Material SurfaceMaterial(int stage)
        {
            int index = stage - 1;
            if (Materials[index]) return Materials[index];
            var material = new Material(Resources.Load<Shader>("TerrainRelief")) { name = Textures[index] };
            material.mainTexture = Resources.Load<Texture2D>("Terrain/" + Textures[index]);
            material.SetTexture("_Crest", Resources.Load<Texture2D>("Terrain/SurfaceCrests"));
            // Source pixels per vertical world unit. Horizontal and vertical sampling use the same original pixel density.
            material.SetVector("_CrestData", new Vector4(CrestRows[index], 137, 941, index));
            material.SetFloat("_WetStone", stage == 3 ? 1 : 0);
            Materials[index] = material;
            return material;
        }

        static float SideInset(float depth, float exposure, float seed, bool shelf, int stage)
        {
            if (exposure <= .025f || depth <= 0) return 0;
            float strength = Mathf.SmoothStep(0, 1, depth / .95f);
            if (!shelf && exposure < 11.99f) strength *= Mathf.Clamp01((exposure - depth) / .32f);
            float rock = Mathf.PerlinNoise(seed, depth * 1.24f + .81f);
            float shape = shelf ? .48f : stage >= 3 ? .29f : .60f;
            return Mathf.Round(strength * (.04f + shape * rock) * 36) / 36;
        }

        static float EdgeLight(float depth, float exposure, bool shelf)
        {
            if (exposure <= .025f) return 0;
            return shelf || exposure >= 11.99f ? 1 : Mathf.Clamp01((exposure - depth) / .2f);
        }

        static float JointDepth(float distance, float drop, float width)
        {
            // Adjacent banks meet on the lower bank's geological datum. The taller
            // bank returns gradually to its own strata, instead of slicing them vertically.
            if (drop <= .025f || drop >= 11.99f) return 0;
            float blendWidth = Mathf.Min(width * .48f, Mathf.Clamp(drop * 1.36f, 1.3f, 5.4f));
            return drop * (1 - Mathf.SmoothStep(0, 1, distance / blendWidth));
        }

        static void DressFace(int stage, GameObject parent, float start, float end, float top, float leftDrop, float rightDrop, bool shelf)
        {
            var owner = Owner(parent);
            float width = end - start;
            float middle = (start + end) * .5f;
            float[] depths = shelf ? ShelfRows : DeepRows;
            // Small subdivisions let both the end profile and the underside bend without altering collision.
            int columns = Mathf.Max(3, Mathf.CeilToInt(width / .55f) + 1);
            var vertices = new Vector3[columns * depths.Length];
            var pigment = new Vector2[vertices.Length];
            var bevel = new Vector2[vertices.Length];
            var strata = new Vector2[vertices.Length];
            var exposure = new Color32[vertices.Length];
            var triangles = new int[(columns - 1) * (depths.Length - 1) * 6];
            float seed = (stage * 37.7f + Mathf.Abs(start * .573f + top * .83f)) % 419f;
            int k = 0;
            for (int row = 0; row < depths.Length; row++)
            {
                float d = depths[row];
                float left = SideInset(d, leftDrop, seed, shelf, stage);
                float right = width - SideInset(d, rightDrop, seed + 13.4f, shelf, stage);
                var edgeColor = new Color(EdgeLight(d, leftDrop, shelf), EdgeLight(d, rightDrop, shelf), shelf ? 1 : 0, 1);
                for (int col = 0; col < columns; col++)
                {
                    int id = row * columns + col;
                    float x = Mathf.Lerp(left, right, col / (float)(columns - 1));
                    float depth = d;
                    if (shelf && d > .34f)
                    {
                        float endTaper = Mathf.Clamp01(1 - Mathf.Min(x, width - x) / .68f);
                        float rough = Mathf.PerlinNoise((start + x) * 2.3f, seed) - .5f;
                        depth += (rough * .36f - endTaper * .23f) * Mathf.InverseLerp(.34f, .72f, d);
                    }
                    vertices[id] = new Vector3(x - width * .5f, -depth, 0);
                    pigment[id] = new Vector2(start + x, depth);
                    bevel[id] = new Vector2(x - left, right - x);
                    float leftDatum = shelf ? 0 : JointDepth(x, leftDrop, width);
                    float rightDatum = shelf ? 0 : JointDepth(width-x, rightDrop, width);
                    float datum = Mathf.Max(leftDatum, rightDatum);
                    float drop = leftDatum > rightDatum ? leftDrop : rightDrop;
                    strata[id] = new Vector2(datum, datum > 0 ? drop : 0);
                    edgeColor.a = shelf ? 1 : owner.RootLight(start + x, top, depth);
                    exposure[id] = edgeColor;
                    if (row == 0 || col == 0) continue;
                    int upper = id - columns;
                    triangles[k++] = upper - 1; triangles[k++] = upper; triangles[k++] = id - 1;
                    triangles[k++] = upper; triangles[k++] = id; triangles[k++] = id - 1;
                }
            }
            var mesh = new Mesh { name = "Terrain strata - " + Textures[stage - 1] };
            mesh.vertices = vertices; mesh.uv = pigment; mesh.uv2 = bevel; mesh.uv3 = strata;
            mesh.colors32 = exposure; mesh.triangles = triangles;
            mesh.RecalculateBounds();
            owner.meshes.Add(mesh);
            if (leftDrop > .025f) owner.ExposedEdges++; else owner.JoinedEdges++;
            if (rightDrop > .025f) owner.ExposedEdges++; else owner.JoinedEdges++;

            var go = new GameObject(mesh.name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = new Vector3(middle, top, 0);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SurfaceMaterial(stage);
            renderer.sortingOrder = 2;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
        }

        void OnDestroy()
        {
            foreach (var mesh in meshes) if (mesh) Destroy(mesh);
        }
    }
}
