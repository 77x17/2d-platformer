using UnityEngine;

namespace Moonlit
{
    public static class FourMoonPieces
    {
        const int Size = 88;
        static readonly Sprite[] sprites = new Sprite[4];
        public static Sprite Get(int i)
        {
            i = Mathf.Clamp(i,0,3);
            if (!sprites[i]) Build();
            return sprites[i];
        }
        public static Vector2 Offset(int i) => Get(i).rect.center - Vector2.one*(Size*.5f);
        static void Build()
        {
            var textures = new Color32[4][];
            var low = new Vector2Int[4]; var high = new Vector2Int[4];
            for(int i=0;i<4;i++){textures[i]=new Color32[Size*Size]; low[i]=new Vector2Int(Size,Size);}
            for (int y=0;y<Size;y++) for(int x=0;x<Size;x++) {
                var p=new Vector2(x+.5f-Size*.5f,y+.5f-Size*.5f);
                float r=p.magnitude;
                if(r>42.6f) continue;
                // Two stepped fault lines partition a single disc. Every lunar pixel belongs to one piece.
                int vertical=(int)(p.y/5)%3*2;
                int horizontal=(int)(p.x/7)%3*2;
                int i=(p.x<vertical?0:1)+(p.y>horizontal?0:2);
                float rock=Mathf.PerlinNoise(x*.065f,y*.07f)+.24f*Mathf.PerlinNoise(x*.18f,y*.18f);
                Color32 c=r>41.4f?new Color32(206,147,80,255):r>39.9f?new Color32(255,223,149,255):
                    rock>.76f?new Color32(226,183,114,255):rock>.63f?new Color32(246,209,139,255):new Color32(255,235,178,255);
                textures[i][y*Size+x]=c;
                low[i]=Vector2Int.Min(low[i],new Vector2Int(x,y)); high[i]=Vector2Int.Max(high[i],new Vector2Int(x,y));
            }
            for(int i=0;i<4;i++){
                var t=new Texture2D(Size,Size,TextureFormat.RGBA32,false){name="Moon Fragment "+(i+1),filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
                t.SetPixels32(textures[i]);t.Apply(false,true);
                sprites[i]=Sprite.Create(t,new Rect(low[i].x,low[i].y,high[i].x-low[i].x+1,high[i].y-low[i].y+1),Vector2.one*.5f,32,0,SpriteMeshType.FullRect);
            }
        }
    }
}
