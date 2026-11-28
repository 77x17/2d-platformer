using UnityEngine;

namespace Moonlit
{
    public sealed class LiftSuspension : MonoBehaviour
    {
        static Sprite socketArt;
        readonly Transform[] heads = new Transform[2], feet = new Transform[2];
        readonly LineRenderer[] cables = new LineRenderer[2], braids = new LineRenderer[2];
        SpriteRenderer lintel, deck;

        public void Initialize(SpriteRenderer crossbar, SpriteRenderer floor, float width, float floorY)
        {
            lintel = crossbar; deck = floor;
            if (!socketArt) socketArt = MakeSocket();
            for (int i = 0; i < 2; i++)
            {
                float x = transform.position.x + (i == 0 ? -.40f : .40f) * width;
                heads[i] = Fastening("Overhead iron eye", new Vector2(x, lintel.bounds.center.y - .18f), lintel, true);
                feet[i] = Fastening("Deck strap and eye", new Vector2(x, floorY + .14f), deck, false);
                cables[i] = Cable("Hemp cable outline", .101f, new Color(.18f,.12f,.09f), 4);
                braids[i] = Cable("Waxed hemp braid", .057f, new Color(.82f,.64f,.36f), 5);
            }
            LateUpdate();
        }

        Transform Fastening(string label, Vector2 point, SpriteRenderer mountingSurface, bool inverted)
        {
            var socket = Visuals.Sprite(label, socketArt, point, mountingSurface.transform, 6);
            socket.transform.localScale = Vector3.one / mountingSurface.transform.lossyScale.x;
            socket.flipY = inverted;
            return socket.transform;
        }

        LineRenderer Cable(string label, float width, Color tint, int order)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2; line.sharedMaterial = Visuals.Material;
            line.sortingOrder = order;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = tint;
            return line;
        }

        void LateUpdate()
        {
            for (int i = 0; i < 2; i++)
            {
                var a = heads[i].position; var b = feet[i].position;
                cables[i].SetPosition(0, a); cables[i].SetPosition(1, b);
                braids[i].SetPosition(0, a); braids[i].SetPosition(1, b);
            }
        }

        // Blackened iron and the brass rivet are drawn at the final pixel proportions.
        // The pivot is inside the aperture: the cable passes through it at every lift height.
        static Sprite MakeSocket()
        {
            string[] rows = {
                "....dddd.....", "...dggggd....", "..dghhhggd...", "..dgh..bgd...",
                "..dgh..bgd...", "..dgg..bgd...", "...dgbbgd....", "....dggd.....",
                "...dghbgd....", ".dddgbbgddd..", ".dgghhhbbgd..", ".dbbggggbbd..",
                "...dghbgd....", "...dghbgd....", "...dghbgd....", "...dggggd....",
                "...dgyrgd....", "...dgrrgd....", "...dgbbgd....", "....dddd....."
            };
            int w = rows[0].Length, h = rows.Length;
            var pixels = new Color32[w*h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color32 pixel;
                switch (rows[y][x])
                {
                    case 'd': pixel = new Color32(45,38,36,255); break;
                    case 'b': pixel = new Color32(83,66,53,255); break;
                    case 'g': pixel = new Color32(129,106,75,255); break;
                    case 'h': pixel = new Color32(197,168,110,255); break;
                    case 'r': pixel = new Color32(170,117,65,255); break;
                    case 'y': pixel = new Color32(232,181,98,255); break;
                    default: continue;
                }
                pixels[(h-1-y)*w+x] = pixel;
            }
            var texture = new Texture2D(w,h,TextureFormat.RGBA32,false)
            {
                name = "Lift strap and iron eye", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0,0,w,h), new Vector2(6.5f/w, (h-4.5f)/h), 45, 0, SpriteMeshType.FullRect);
        }

        public bool CheckAttachments(out string detail)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 top = heads[i].position, bottom = feet[i].position;
                // Sample the iron rivet centers inside the painted wood, in addition to the actual line ends.
                var beamRivet = top + Vector3.up * .26f;
                var boardRivet = bottom - Vector3.up * .26f;
                if (!lintel.bounds.Contains(beamRivet) || !deck.bounds.Contains(boardRivet) ||
                    Vector2.Distance(cables[i].GetPosition(0),top) > .035f ||
                    Vector2.Distance(cables[i].GetPosition(1),bottom) > .075f ||
                    Vector2.Distance(braids[i].GetPosition(0),top) > .035f ||
                    Vector2.Distance(braids[i].GetPosition(1),bottom) > .075f)
                {
                    detail = $"Lift at {transform.position:F2}: cable {i+1} or its rivet lost contact.";
                    return false;
                }
            }
            detail = null; return true;
        }
    }
}
