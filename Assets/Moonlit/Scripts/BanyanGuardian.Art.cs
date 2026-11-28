using UnityEngine;

namespace Moonlit
{
    public sealed partial class BanyanGuardian
    {
        static readonly Color BarkDust = new Color(.88f, .65f, .36f);
        static readonly Color Danger = new Color(1f, .47f, .25f);
        static readonly Color Canopy = new Color(.56f, .90f, .86f);
        Transform trunk, flower;
        SpriteRenderer tree, shadow, arm, bloom, core;
        readonly SpriteRenderer[] petals = new SpriteRenderer[5], knots = new SpriteRenderer[3];
        readonly SpriteRenderer[] markers = new SpriteRenderer[9], shoots = new SpriteRenderer[6];
        readonly SpriteRenderer[] shockLight = new SpriteRenderer[2], shockTip = new SpriteRenderer[2];
        internal static readonly int GuardianPose = Shader.PropertyToID("_GuardianPose");
        MaterialPropertyBlock poseBlock;
        float bodyScale;
        float BodyHeight => 7.45f;

        void BuildArt()
        {
            trunk = new GameObject("Guardian Trunk").transform;
            trunk.SetParent(transform, false);
            tree = Visuals.Sprite("Carved Banyan Face", Resources.Load<Sprite>("BanyanGuardian"), foot, trunk, 4);
            Visuals.AnimateBanyan(tree);
            poseBlock = new MaterialPropertyBlock();
            tree.GetPropertyBlock(poseBlock);
            bodyScale = BodyHeight / tree.sprite.bounds.size.y;
            tree.transform.localScale = Vector3.one * bodyScale;
            tree.transform.localPosition = Vector3.up * (BodyHeight * .48f);
            shadow = Visuals.Halo("Canopy Shadow", foot, 1, Color.black, transform, 2);
            var wood = Resources.Load<Sprite>("BanyanRootArm");
            arm = Visuals.Sprite("Bound Attack Root", Visuals.TiledSprite(wood, 23), foot, transform, 3);
            for (int i = 0; i < shoots.Length; i++)
                shoots[i] = Visuals.Sprite("Rising Root", wood, foot, transform, 9);
            for (int i = 0; i < 2; i++) {
                shockLight[i] = Visuals.Halo("Root Shockwave Glow", foot, 2, Danger, transform, 7);
                shockTip[i] = Visuals.Sprite("Shockwave Tip", Visuals.MoonMote, foot, transform, 11, .26f);
            }
            for (int i = 0; i < markers.Length; i++)
                markers[i] = Visuals.Sprite("Attack Direction Marker", Visuals.MoonMote, foot, transform, 900);
            flower = new GameObject("Moon Bud").transform;
            flower.SetParent(transform, false);
            bloom = Visuals.Halo("Bud Glow", foot, 1, Visuals.Gold, flower, 900);
            bloom.transform.localPosition = Vector3.zero;
            core = Visuals.Sprite("Bud Core", Visuals.MoonMote, foot, flower, 903, .46f);
            core.transform.localPosition = Vector3.zero;
            for (int i = 0; i < petals.Length; i++)
                petals[i] = Visuals.Sprite("Bud Petal", Visuals.MoonMote, foot, flower, 902);
            for (int i = 0; i < knots.Length; i++)
                knots[i] = Visuals.Sprite("Restored Blossom", Visuals.MoonMote, foot, trunk, 6, .42f);
        }

        void Draw()
        {
            float t = Progress;
            bool fighting = Warning || Attacking;
            poseBlock.SetVector(GuardianPose, new Vector4(clock, fighting ? .48f : 1f, 0, 0));
            tree.SetPropertyBlock(poseBlock);
            float tension = Warning ? Mathf.SmoothStep(0, 1, t) : 0;
            float bend = -direction * tension * 5;
            if (Attacking && Attack == Move.Charge) bend = -direction * Mathf.Sin(t * Mathf.PI) * 7;
            if (Attacking && (Attack == Move.LowSweep || Attack == Move.HighSweep))
                bend = -direction * Mathf.Sin(t * Mathf.PI) * 8;
            float settle = Attack == Move.Slam && Attacking && landed
                ? .16f * Mathf.Clamp01(1 - (t - LandingFraction) * 7) : 0;
            trunk.position = new Vector2(BarkX, foot.y + lift - settle);
            trunk.localRotation = Quaternion.Euler(0, 0, bend);
            float calm = lights / 6f;
            tree.color = Color.Lerp(new Color(.85f,.89f,.96f), new Color(1,.99f,.88f), calm);
            shadow.transform.position = new Vector2(BarkX, foot.y + .035f);
            shadow.transform.localScale = new Vector3(5.4f - lift * .7f, .29f, 1);
            shadow.color = new Color(.05f,.035f,.08f,.65f - lift * .12f);
            for (int i = 0; i < knots.Length; i++) {
                knots[i].enabled = i < CalmedRoots;
                knots[i].transform.localPosition = new Vector2((i - 1) * .67f, 2.45f - Mathf.Abs(i - 1) * .19f);
            }

            arm.enabled = fighting;
            if (fighting) {
                float length = Attack == Move.Charge ? 3.35f :
                    Attack == Move.Slam ? 2.7f : Attacking ? SwingSpan(t) : 2f;
                float height = Attack == Move.Slam ? 1.7f + lift : Attack == Move.Charge ? 1.0f : ZoneHeight;
                float angle = Attack == Move.Slam ? 30 + 30 * tension : Warning ? 15 * tension : 0;
                OrientWood(arm, new Vector2(BarkX, foot.y + height), length, direction, angle, true);
            }
            for (int i = 0; i < shoots.Length; i++) {
                var shoot = shoots[i];
                bool show = Attacking && Attack == Move.Slam && landed;
                int side = i < 3 ? -1 : 1;
                float front = ShockDistance - (i % 3) * .44f;
                float x = endX + side * front;
                shoot.enabled = show && front >= 0 && x >= foot.x - 25 && x <= foot.x + 6;
                if (shoot.enabled) OrientWood(shoot, new Vector2(x, foot.y - .08f), 1.40f - .23f * (i % 3), side, 62, false);
                if (i % 3 == 0) {
                    int j = i / 3;
                    shockLight[j].enabled = shockTip[j].enabled = shoot.enabled;
                    if (shoot.enabled) {
                        shockLight[j].transform.position = new Vector2(x + side * .24f, foot.y + .49f);
                        shockLight[j].color = new Color(1,.55f,.20f,.52f);
                        shockTip[j].transform.position = new Vector2(x + side * .64f, foot.y + 1.10f);
                        shockTip[j].color = Visuals.Gold;
                    }
                }
            }
            DrawWarning(t);
            DrawBloom();
        }

        static void OrientWood(SpriteRenderer wood, Vector2 basePoint, float length, float face, float angle, bool articulated)
        {
            float radians = angle * Mathf.Deg2Rad;
            var axis = new Vector2(face * Mathf.Cos(radians), Mathf.Sin(radians));
            wood.transform.position = basePoint + axis * (length * .5f);
            wood.transform.rotation = Quaternion.Euler(0, 0, face * angle);
            float scale = length / (wood.sprite.rect.width / wood.sprite.pixelsPerUnit);
            wood.flipX = face < 0;
            if (articulated)
                Visuals.SizeBand(wood, length, wood.sprite.rect.height / wood.sprite.pixelsPerUnit * Mathf.Min(scale, .26f));
            else wood.transform.localScale = Vector3.one * scale;
        }

        void DrawWarning(float t)
        {
            for (int i = 0; i < markers.Length; i++) {
                var mark = markers[i];
                mark.enabled = Warning;
                if (!Warning) continue;
                float q = i / (markers.Length - 1f);
                bool slam = Attack == Move.Slam;
                float x = slam ? endX + (q - .5f) * 3.45f : Mathf.Lerp(startX, endX, q);
                float y = slam ? .13f + Mathf.Abs(q - .5f) * .42f :
                    Attack == Move.HighSweep ? ZoneHeight : Attack == Move.LowSweep ? ZoneHeight * .65f : .20f;
                mark.transform.position = new Vector2(x, foot.y + y);
                mark.transform.rotation = Quaternion.Euler(0, 0, slam ? 0 : -direction * 58);
                float blink = .77f + .23f * Mathf.Cos((q - t * 1.5f) * Mathf.PI * 4);
                mark.transform.localScale = Vector3.one * ((slam && i == 4 ? .54f : .36f) * blink);
                mark.color = Attack == Move.HighSweep ? Canopy : Danger;
            }
        }

        void DrawBloom()
        {
            bool visible = TargetOpen || beat == Beat.Resting || beat == Beat.Homecoming;
            flower.gameObject.SetActive(visible);
            if (!visible) return;
            flower.position = TargetPosition + Vector2.up * (.07f * Mathf.Sin(clock * 3));
            float radius = TargetOpen ? .34f : .18f;
            float glow = TargetOpen ? 3.6f : 1.6f;
            bloom.transform.localScale = Vector3.one * (glow + .18f * Mathf.Sin(clock * 4));
            bloom.color = TargetOpen ? new Color(1,.74f,.28f,.66f) : new Color(.60f,.92f,.82f,.42f);
            for (int i = 0; i < petals.Length; i++) {
                float a = i * Mathf.PI * 2 / petals.Length + clock * .23f;
                var petal = petals[i];
                petal.transform.localPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                petal.transform.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg - 90);
                petal.transform.localScale = Vector3.one * (TargetOpen ? .45f : .33f);
                petal.color = TargetOpen ? Visuals.Gold : Visuals.Mint;
            }
        }
    }
}
