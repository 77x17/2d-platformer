using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Moonlit
{
    public sealed partial class MoonlitHud
    {
        GameObject story;
        readonly Image[] lunar = new Image[4];
        readonly Image[] moteDust=new Image[16];
        Material storyBark;
        Image storyTree, storyArm, rider, candle, moonGlow;
        TMP_Text legend, storyHeading;
        Button skipStory;
        float reelTime;
        int lastCue;
        public float StoryTime => reelTime;
        static readonly Vector2 MoonCentre=new Vector2(811,305);
        static readonly Vector2[] Lands={new Vector2(220,689),new Vector2(594,654),new Vector2(1058,717),new Vector2(1425,624)};
        Image Actor(Transform parent,string name,Sprite art)
        {
            var image=Panel(name,parent,0,0,1,1,Color.white);
            image.sprite=art; image.preserveAspect=true; image.rectTransform.pivot=Vector2.one*.5f;
            return image;
        }
        static void Pose(Image im,Vector2 centre,Vector2 size,float degrees=0)
        {
            im.rectTransform.anchoredPosition=new Vector2(centre.x,-centre.y);
            im.rectTransform.sizeDelta=size;
            im.rectTransform.localRotation=Quaternion.Euler(0,0,degrees);
        }
        void BuildStory()
        {
            story=Layer("Moon Story");
            var p=story.transform;
            Art(p,"FourLandsAtNight");
            moonGlow=Actor(p,"Moon Halo",Visuals.Glow);
            for(int i=0;i<4;i++) lunar[i]=Actor(p,"Moon Fragment "+(i+1),FourMoonPieces.Get(i));
            for(int i=0;i<moteDust.Length;i++) moteDust[i]=Actor(p,"Falling Moon Mote",Visuals.MoonMote);
            storyTree=Actor(p,"Banyan Tree",Resources.Load<Sprite>("BanyanGuardian"));
            storyBark=new Material(Resources.Load<Shader>("BanyanStory"));
            storyTree.material=storyBark;
            storyArm=Actor(p,"Striking Root",Resources.Load<Sprite>("BanyanRootArm"));
            rider=Actor(p,"Cuoi on Branch",game.art.Hero(0,0));
            candle=Actor(p,"Cuoi's Lantern",Resources.Load<Sprite>("StarLantern"));
            Panel("Subtitle Backplate",p,0,770,1600,130,new Color(.026f,.043f,.11f,.92f));
            storyHeading=Label(p,"",65,789,295,54,25,gold,true,TextAlignmentOptions.MidlineLeft,true);
            legend=Label(p,"",359,783,995,82,27,cream,false,TextAlignmentOptions.MidlineLeft);
            skipStory=Button(p,"Skip Story","BỎ QUA",1391,803,156,52,game.EndStory,false);
            story.SetActive(false);
        }
        void StartStory()
        {
            reelTime=0;lastCue=-1;
            bool intro=game.State==GameState.Prologue;
            storyHeading.text=intro?"Đêm trăng vỡ":"Về cung trăng";
            Select(skipStory);
            DrawStory(intro);
        }
        void UpdateStory(float dt)
        {
            reelTime+=dt;
            bool intro=game.State==GameState.Prologue;
            DrawStory(intro);
            var kb=Keyboard.current;
            if(reelTime >= (intro?9f:8.2f) || (reelTime>.35f && kb!=null &&
                (kb.escapeKey.wasPressedThisFrame || game.Controls.Pressed(GameAction.Jump, kb)))) game.EndStory();
        }
        static float Blend(float start,float end,float t) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(start,end,t));
        void DrawStory(bool intro)
        {
            float t=reelTime;
            float shatter=Blend(2.4f,3.55f,t), fall=Blend(3.75f,7.1f,t), returnTrip=Blend(.65f,4.75f,t);
            Vector2 centre=MoonCentre;
            if(intro && t>2.38f && t<2.72f) centre.x+=Mathf.Sin((t-2.38f)*92)*8*(1-Blend(2.38f,2.72f,t));
            float glow=intro?1-fall:returnTrip;
            Pose(moonGlow,centre,Vector2.one*(490+70*glow));
            moonGlow.color=new Color(1,.81f,.42f,.07f+.38f*glow);
            for(int i=0;i<4;i++){
                Vector2 offset=FourMoonPieces.Offset(i);offset.y=-offset.y;
                Vector2 assembled=centre+offset*3.42f;
                Vector2 apart=assembled+offset.normalized*(intro?65:42);
                float local=intro?Blend(3.72f+i*.10f,6.67f+i*.15f,t):Blend(.55f+i*.15f,4.2f+i*.15f,t);
                Vector2 pos=intro?Vector2.Lerp(Vector2.Lerp(assembled,apart,shatter),Lands[i],local):
                    Vector2.Lerp(Lands[i],Vector2.Lerp(apart,assembled,returnTrip),local);
                pos.y-=Mathf.Sin(local*Mathf.PI)*(intro?48:130);
                float spin=(i%2==0?-1:1)*(intro?shatter*15+local*49:(1-local)*64);
                float scale=intro?Mathf.Lerp(3.42f,.78f,local):Mathf.Lerp(.78f,3.42f,local);
                Pose(lunar[i],pos,lunar[i].sprite.rect.size*scale,spin);
            }
            for(int i=0;i<moteDust.Length;i++) {
                float q=Blend(2.45f+(i%4)*.10f,6.8f+(i%3)*.18f,t);
                var mote=moteDust[i]; mote.enabled=intro && t>2.43f && t<8.85f;
                if(!mote.enabled)continue;
                Vector2 to=Lands[i%4]+new Vector2(((i/4)-1.5f)*49,Mathf.Sin(i*2.1f)*18);
                Vector2 point=Vector2.Lerp(centre,to,q);
                point.y-=Mathf.Sin(q*Mathf.PI)*(74+(i%3)*31);
                Pose(mote,point,Vector2.one*(9+3*(i%3)),i*23+q*50);
                mote.color=new Color(1,.8f,.44f,.80f+.2f*Mathf.Sin(t*6+i));
            }
            float rise=Blend(4.4f,7.65f,t);
            Vector2 treeAt=intro?new Vector2(798-460*fall,139+495*fall):Vector2.Lerp(new Vector2(1400,620),new Vector2(798,139),rise);
            float size=intro?Mathf.Lerp(248,89,fall):Mathf.Lerp(95,248,rise);
            float wind=intro?Blend(.85f,2.12f,t)-Blend(2.12f,2.43f,t):0;
            storyBark.SetVector(BanyanGuardian.GuardianPose,new Vector4(t,1-.30f*wind,0,0));
            Pose(storyTree,treeAt,Vector2.one*size,wind*9);
            bool strike=intro && t<3.25f;
            storyArm.enabled=strike;
            Pose(storyArm,treeAt+new Vector2(57+wind*32,66-wind*48),new Vector2(153,47),-52+wind*103);
            rider.enabled=candle.enabled=!intro;
            Pose(rider,treeAt+new Vector2(-.27f*size,.08f*size),Vector2.one*(size*.32f));
            Pose(candle,treeAt+new Vector2(-.18f*size,.15f*size),Vector2.one*(size*.12f));
            int cue=intro?(t<2.4f?0:t<6.9f?1:2):(t<.65f?0:t<4.7f?1:2);
            if(cue==lastCue)return;lastCue=cue;
            legend.text=intro ? (cue==0?"Đêm rằm ấy, cây đa trên trăng trở mình.":cue==1?
                "Một chiếc rễ quật xuống. Mặt trăng vỡ làm bốn mảnh, rơi tứ phía.":
                "Cuội xách đèn, chạy ra bờ tre.") :
                (cue==0?"Rễ đa nằm im.":
                cue==1?"Cuội ngẩng lên. Bốn mảnh trăng đã liền lại.":
                "“Về thôi, đa ơi!” Cuội trèo lên cành, ôm đèn vào lòng.");
            if(cue==1)game.audioFx.Play(intro?"rootImpact":"bloom");
            if(cue==2)game.audioFx.Play(intro?"signal1":"signal3");
        }
    }
}
