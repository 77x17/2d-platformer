using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitLevel
    {
        public CratePressureBridge Weight { get; private set; }
        public BambooChimeGate ChimeGate { get; private set; }
        public SluiceFerry Ferry { get; private set; }
        static Sprite stake;

        void BuildCrossing()
        {
            if (stage == 1) {
                const float center = 334.5f, floor = 2.7f, width = 11.06f;
                var deck = BambooPlatform(center, floor, width, suspended: true);
                var bamboo = deck.GetComponentInChildren<SpriteRenderer>();
                SupportLift(center, floor, width, bamboo, clearance: 5.7f);
                WeightPlate(318.2f, 326.5f, floor, deck);
                StakeBed(33.05f,34.95f,-.53f);
                StakeBed(329.2f,339.85f,.10f);
            }
            else if (stage == 2) {
                BuildChimePass();
            }
            else if (stage == 3) {
                Vector2 dock = new Vector2(480.2f,2.85f), exit = new Vector2(501.5f,4.35f);
                var raft = BambooPlatform(dock.x,dock.y,5,Vector2.zero);
                var lamp = Visuals.Sprite("Ferry Signal Lantern",Resources.Load<Sprite>("StarLantern"),
                    dock + new Vector2(1.48f,1.45f), raft.transform, 7);
                lamp.transform.localScale = Vector3.one * (.79f/lamp.sprite.bounds.size.y);
                Beam("Ferry Lantern Post",dock+new Vector2(1.78f,-.05f),dock+new Vector2(1.78f,2f),.12f,Timber,raft.transform,5);
                Vector2 westEye = new Vector2(476.8f, 9.4f), eastEye = new Vector2(505, 10.65f);
                Rig(475, 4.1f, westEye.x, westEye.y);
                Rig(506.3f, 4.35f, eastEye.x, eastEye.y);
                var westBell = CallBell(new Vector2(476.65f,4.1f));
                var eastBell = CallBell(new Vector2(505.65f,4.35f));
                Ferry = new GameObject("Sluice Cable Ferry").AddComponent<SluiceFerry>();
                Ferry.transform.SetParent(encounters,false);
                Ferry.Build(raft.GetComponent<MovingPlatform>(), dock, exit, lamp, westEye, eastEye, westBell, eastBell);
            }
            else {
                BuildHillDrawbridge();
                StakeBed(84.05f,94.4f,2.8f);
                StakeBed(333.1f,339.75f,-.3f);
            }
        }
        SpriteRenderer CallBell(Vector2 foot)
        {
            Beam("Bank call post", foot - Vector2.up * .12f, foot + Vector2.up * 1.7f, .16f, Timber, decoration, 5);
            Beam("Call bell arm", foot + Vector2.up * 1.67f, foot + new Vector2(.47f,1.67f), .11f, Timber, decoration, 5);
            var lamp = Visuals.Sprite("Bank call lantern", Resources.Load<Sprite>("StarLantern"),
                foot + new Vector2(.41f,1.23f), decoration, 6);
            lamp.transform.localScale = Vector3.one * (.75f / lamp.sprite.bounds.size.y);
            return lamp;
        }

        void Rig(float baseX, float floor, float hookX, float top)
        {
            float sign = Mathf.Sign(hookX-baseX);
            Vector2 baseAt = new Vector2(baseX,floor-.12f), elbow = new Vector2(baseX + sign*.18f,top);
            Vector2 hook = new Vector2(hookX,top);
            Beam("Bamboo Upright",baseAt,elbow,.27f,Timber,decoration,3);
            Beam("Bamboo Lintel",elbow+Vector2.left*sign*.24f,hook+Vector2.right*sign*.3f,.24f,Timber,decoration,3);
            Beam("Diagonal Brace",Vector2.Lerp(baseAt,elbow,.53f),Vector2.Lerp(elbow,hook,.82f),.20f,Timber,decoration,3);
            for(int i=0;i<4;i++) {
                Vector2 at=Vector2.Lerp(baseAt,elbow,(i+1)/5f);
                Beam("Bamboo Joint",at-Vector2.right*.16f,at+Vector2.right*.16f,.065f,new Color(.68f,.64f,.40f),decoration,4);
            }
            Visuals.Box("Red Lashing",hook,new Vector2(.37f,.32f),new Color(.77f,.31f,.18f),decoration,5);
        }
        void BuildChimePass()
        {
            chimeStations.Sort((a,b) => a.bounds.center.x.CompareTo(b.bounds.center.x));
            const float gateX = 405f, floor = 5.35f, height = 8.1f;
            Rig(gateX+1.05f,floor,gateX,floor+height);
            var shutter = new GameObject("Chime Gate");
            shutter.transform.SetParent(encounters,false);
            shutter.transform.position = new Vector2(gateX,floor+height);
            shutter.layer = 8;
            var blocker = shutter.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(.55f,height);
            blocker.offset = Vector2.down*(height*.5f);
            var hinge = (Vector2)shutter.transform.position;
            for (int i=0; i<3; i++) {
                Vector2 top = hinge + Vector2.right*((i-1)*.21f);
                Beam("Bamboo Gate Slat",top,top+Vector2.down*height,.16f,Timber,shutter.transform,4);
            }
            for (int i=1;i<=4;i++) {
                Vector2 band = hinge + Vector2.down*(i*height/5);
                Beam("Copper Binding",band+Vector2.left*.32f,band+Vector2.right*.32f,.12f,
                    new Color(.82f,.53f,.25f),shutter.transform,5);
            }
            ChimeGate = shutter.AddComponent<BambooChimeGate>();
            var rail = new Vector2(gateX + .9f, floor + 3.8f);
            Beam("Signal lantern rail", rail + Vector2.right*.35f, rail + Vector2.left*2.65f, .17f, Timber, decoration, 5);
            Beam("Signal rail brace", rail + Vector2.down*.9f, rail + Vector2.left*1.1f, .13f, Timber, decoration, 4);
            var lamps = new SpriteRenderer[2];
            for (int i = 0; i < lamps.Length; i++)
            {
                var tie = rail + Vector2.left*(2.25f-i*.96f);
                Beam("Signal lantern cord", tie, tie + Vector2.down*.47f, .045f, Timber, decoration, 5);
                var lamp = Visuals.Sprite("Gate signal lantern", Resources.Load<Sprite>("StarLantern"), tie + Vector2.down*.80f, decoration, 7);
                lamp.transform.localScale = Vector3.one*(.91f/lamp.sprite.bounds.size.y);
                lamps[i] = lamp;
            }
            ChimeGate.Build(chimeStations[0], chimeStations[1], blocker, lamps);
        }

        void BuildHillDrawbridge()
        {
            const float from = 332.8f, to = 340.12f, floor = 2;
            var deck = BambooPlatform((from+to)*.5f,floor,to-from,null,true);
            var hinge = new GameObject("Drawbridge Axle").transform;
            hinge.SetParent(terrain,false);
            hinge.position = new Vector2(from,floor);
            deck.transform.SetParent(hinge,true);
            Ground(339.85f,342.7f,floor);
            Ground(342.7f,345.55f,5.1f);

            var axle = (Vector2)hinge.position;
            Beam("Axle Bearing",axle+new Vector2(-.48f,-.48f),axle,.25f,Timber,terrain,5);
            Beam("Counterweight Link",new Vector2(328.8f,floor+.10f),axle,.11f,Timber,terrain,3);
            Visuals.Box("Axle Pin", axle, Vector2.one*.29f, Visuals.Gold, terrain, 6);
            WeightPlate(323.5f,328.8f,floor,deck);
            Weight.hinge = hinge;
        }

        void WeightPlate(float boxX,float plateX,float floor,GameObject deck)
        {
            var plate = Visuals.Box("Counterweight Plate",new Vector2(plateX,floor+.06f),
                new Vector2(1.76f,.14f),new Color(.76f,.56f,.30f),terrain,6);
            Visuals.Box("Crate Guide",new Vector2(plateX,floor+.15f),new Vector2(.88f,.05f),Visuals.Mint,terrain,7);
            var pulley = new GameObject("Bamboo Counterweight"); pulley.transform.SetParent(terrain,false);
            Weight = pulley.AddComponent<CratePressureBridge>();
            Weight.crate = CreateCrate(boxX,floor);
            Weight.plateCenter = new Vector2(plateX,floor+.11f); Weight.plate=plate; Weight.floorY=floor;
            Weight.bridge=deck.transform;
            Weight.bridgeCollider=deck.GetComponent<BoxCollider2D>();
        }
        void StakeBed(float from,float to,float top)
        {
            int count=Mathf.Max(3,Mathf.FloorToInt((to-from)/.44f));
            var bed = new GameObject("Bamboo Stake Bed");
            bed.transform.SetParent(encounters,false);
            bed.transform.position=new Vector2((from+to)*.5f,top-.32f);
            var box=bed.AddComponent<BoxCollider2D>(); box.isTrigger=true; box.size=new Vector2(to-from,.64f);
            bed.AddComponent<BambooPit>();
            Ground(from,to,top-.67f).transform.SetParent(bed.transform,true);
            if (!stake) stake=MakeStake();
            for(int i=0;i<count;i++) {
                float x=Mathf.Lerp(from+.13f,to-.13f,i/(count-1f));
                var s=Visuals.Sprite("Bamboo Stake",stake,new Vector2(x,top-.32f),bed.transform,5);
                s.transform.localScale=Vector3.one * (.96f+.10f*Mathf.Sin(i*2.4f));
                s.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(i*1.8f)*8);
            }
        }
        static Sprite MakeStake()
        {
            const int width=9,height=27;
            var t=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
            var pixels=new Color32[width*height];
            for(int y=0;y<height;y++) for(int x=1;x<=7;x++) {
                if(y>18+(x-1)*1.35f) continue;
                pixels[y*width+x]=y>16+x*.65f ? new Color32(245,211,142,255) :
                    y==8 || y==9 ? new Color32(168,137,69,255) :
                    x==1 || x==7 ? new Color32(68,68,44,255) : x<4 ? new Color32(172,168,89,255) : new Color32(105,121,66,255);
            }
            t.SetPixels32(pixels); t.Apply();
            return Sprite.Create(t,new Rect(0,0,width,height),Vector2.one*.5f,25);
        }
        string CrossingHint(float x)
        {
            if (Weight && !Weight.Activated && Mathf.Abs(x-Weight.plateCenter.x)<14)
                return stage==1 ? "Đẩy thùng lên bàn cân để hạ cầu qua hố chông." : "Đẩy thùng lên bàn cân. Nhịp cầu sẽ hạ xuống bờ bên kia.";
            if (stage==4 && Weight && Weight.Open && x>330 && x<345) return "Qua cầu rồi nhảy đôi để lên bậc đá.";
            if (ChimeGate && x > 374 && x < 405.5f && !ChimeGate.Open)
            {
                if (ChimeGate.Struck == 1) return ChimeGate.WestLit ? "Còn chuông bên phải tán trúc." : "Còn chuông bên trái tán trúc.";
                if (ChimeGate.SeenLocked) return "Quay lại giàn tre. Trên cao có hai bàn đạp chuông.";
            }
            if (Ferry && x > 471 && x < 509) {
                switch(Ferry.State) {
                    case SluiceFerry.LockState.AtLeft: return x > 500 ? $"Bấm {game.Controls.Label(GameAction.Interact)} ở bến để gọi bè về." : $"Bè đã cập bến. Lên bè rồi bấm {game.Controls.Label(GameAction.Interact)} để sang phải.";
                    case SluiceFerry.LockState.AtRight: return x < 480 ? $"Bấm {game.Controls.Label(GameAction.Interact)} ở bến để gọi bè về." : $"Lên bờ để đi tiếp, hoặc bấm {game.Controls.Label(GameAction.Interact)} trên bè để về bến trái.";
                    default: return null;
                }
            }
            return null;
        }
    }
    public sealed class BambooPit : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D hit)
        {
            if (hit.GetComponent<CuoiController>()) MoonlitGame.Instance.Hurt(true);
        }
    }
}
