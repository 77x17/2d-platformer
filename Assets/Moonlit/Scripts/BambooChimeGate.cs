using UnityEngine;

namespace Moonlit
{
    public sealed class BambooChimeGate : MonoBehaviour
    {
        readonly BoxCollider2D[] platforms = new BoxCollider2D[2];
        readonly SpriteRenderer[] plates = new SpriteRenderer[2];
        readonly bool[] lit = new bool[2];
        SpriteRenderer[] signals;
        static Sprite bellArt;
        readonly SpriteRenderer[] bells = new SpriteRenderer[2];
        readonly float[] strikes = { -2f, -2f };
        BoxCollider2D blocker;
        const float PlateWidth = 1.85f;
        float travel;
        public int Struck { get; private set; }
        public bool WestLit => lit[0];
        public bool SeenLocked { get; private set; }
        public bool Open => travel >= 1;

        public void Build(BoxCollider2D west, BoxCollider2D east, BoxCollider2D door, SpriteRenderer[] lights)
        {
            if (!bellArt) bellArt = MakeBell();
            platforms[0] = west; platforms[1] = east; blocker = door; signals = lights;
            for (int i = 0; i < platforms.Length; i++)
            {
                var at = (Vector2)platforms[i].transform.position;
                var deck = platforms[i].transform;
                Visuals.Box("Bronze bell housing", at + Vector2.down*.19f, new Vector2(2.12f,.32f), new Color(.39f,.24f,.14f), deck, 6);
                plates[i] = Visuals.Box("Bell treadle", at, new Vector2(PlateWidth,.15f), new Color(.94f,.69f,.36f), deck, 7);
                for (int j = 0; j <= i; j++)
                    Visuals.Box("Chime mark", at + new Vector2((j-i*.5f)*.27f,-.22f), new Vector2(.09f,.20f), Visuals.Gold, deck, 8);
                signals[i].color = new Color(.41f,.35f,.32f);
                var hanger = at + new Vector2((i == 0 ? -1 : 1)*platforms[i].size.x*.32f,-.49f);
                Visuals.Box("Chime hanger",hanger + Vector2.down*.16f,new Vector2(.075f,.40f),new Color(.64f,.47f,.25f),deck,6);
                bells[i] = Visuals.Sprite("Hanging bronze chime",bellArt,hanger + Vector2.down*.32f,deck,7);
            }
        }

        static Sprite MakeBell()
        {
            string[] rows = { ".....ddd.....", ".....dgd.....", "....dghgd....", "...dghhhbd...", "...dhgbgbd...", "...dhggbbd...", "...dhggbbd...", "..dhggggbbd..", "..dhggggbbd..", ".dhhhhhggbbd.", "dhggggggbbbbd", "dgggggggbbbbd", ".ddddddddddd.", ".....dbd.....", ".....dgd.....", "......d......" };
            int w = rows[0].Length, h = rows.Length;
            var pixels = new Color32[w*h];
            for (int y=0; y<h; y++) for (int x=0; x<w; x++)
            {
                char c=rows[y][x];
                if (c == '.') continue;
                pixels[(h-y-1)*w+x] = c == 'd' ? new Color32(65,42,38,255) :
                    c == 'b' ? new Color32(130,76,47,255) : c == 'g' ? new Color32(196,132,65,255) : new Color32(255,208,124,255);
            }
            var texture = new Texture2D(w,h,TextureFormat.RGBA32,false) { name="Canopy bronze chime",filterMode=FilterMode.Point };
            texture.SetPixels32(pixels); texture.Apply(false,true);
            return Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.96f),18,0,SpriteMeshType.FullRect);
        }

        void Update()
        {
            for (int i=0;i<bells.Length;i++)
            {
                float t = Time.time-strikes[i];
                if (!bells[i] || t > .85f) continue;
                bells[i].transform.localRotation = Quaternion.Euler(0,0,Mathf.Sin(t*23)*14*Mathf.Clamp01(1-t/.78f));
            }
        }

        void FixedUpdate()
        {
            var game = MoonlitGame.Instance;
            if (game.State != GameState.Playing || Open) return;
            var feet = game.player.Body.position;
            if (!SeenLocked && Struck < 2 && feet.x > transform.position.x-2 && feet.x < transform.position.x &&
                feet.y < blocker.bounds.min.y + 2.5f)
            {
                SeenLocked = true;
                game.Toast("CỔNG CÒN KHÓA", Struck == 0 ? "Hai đèn đều tắt. Phải lên giàn tre gióng chuông thôi." : "Mới sáng một đèn. Còn một chuông trên giàn tre.");
            }

            if (Struck < 2)
            {
                if (!game.player.Grounded) return;
                for (int i=0; i<platforms.Length; i++)
                {
                    var at = (Vector2)platforms[i].transform.position;
                    if (lit[i] || Mathf.Abs(feet.x-at.x) > PlateWidth*.5f || Mathf.Abs(feet.y-at.y) > .18f) continue;
                    lit[i] = true; Struck++; strikes[i] = Time.time;
                    plates[i].transform.position += Vector3.down * .10f;
                    plates[i].color = signals[i].color = Visuals.Gold;
                    Visuals.Halo("Lit gate lantern", signals[i].transform.position, 1.85f, new Color(1,.61f,.23f,.52f), signals[i].transform.parent, 6);
                    game.effects.Ring(at, Visuals.Gold, 2f);
                    game.audioFx.Play(i == 0 ? "signal1" : "signal2");
                    game.Toast(Struck == 1 ? "MỘT TIẾNG CHUÔNG" : "CỔNG TRÚC ĐÃ MỞ",
                        Struck == 2 ? "Xuống cầu tre rồi đi tiếp sang phải." : i == 0 ? "Còn chuông ở ngọn trúc bên phải." : "Còn chuông ở ngọn trúc bên trái.");
                }
                if (Struck < 2) return;
            }
            travel = Mathf.MoveTowards(travel,1,Time.fixedDeltaTime/.8f);
            transform.localRotation = Quaternion.Euler(0,0,Mathf.SmoothStep(0,88,travel));
            if (Open) { blocker.enabled = false; game.audioFx.Play("woodLatch"); }
        }
    }
}
