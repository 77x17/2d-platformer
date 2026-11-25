using System;
using UnityEngine;

namespace Moonlit
{
    // Registration marks are in pixels from the original PNG's top left.
    // Knuckle, sole and belt points stay meaningful across trimming, sprite import and flipping.
    internal sealed class CuoiLocomotion
    {
        const float RunStride = 3.04f, ClimbStride = 1.60f;
        static readonly Vector2[] RunToes = {
            new(338,417), new(662,418), new(1052,422), new(1443,385),
            new(336,866), new(665,868), new(1046,867), new(1455,833) };
        static readonly Vector2[] RunGrips = {
            new(355,233), new(774,230), new(1215,213), new(1654,192),
            new(339,651), new(782,652), new(1220,651), new(1668,642) };
        static readonly Vector2[] WallHands = {
            new(488,55), new(1016,133), new(475,894), new(780,177),
            new(1023,687), new(349,74), new(783,586), new(1667,177) };
        static readonly Vector2[] WallToes = {
            new(463,571), new(1010,379), new(419,1158), new(725,312),
            new(991,987), new(301,316), new(744,751), new(1640,297) };
        static readonly Vector2[] WallBelts = {
            new(252,324), new(814,295), new(218,1108), new(623,228),
            new(761,964), new(184,236), new(620,683), new(1510,221) };
        static readonly Vector2[] MantleHands = {new(398,248), new(921,328), new(348,1004), new(1083,1063)};
        static readonly Vector2[] MantleSoles = {new(376,444), new(854,528), new(274,1004), new(852,1063)};
        static readonly Vector2[] MantleBelts = {new(223,354), new(851,310), new(246,882), new(791,961)};

        readonly SpriteRenderer actor;
        readonly Sprite[] run, climb, mantle;
        readonly Material surface;
        readonly int pinA = Shader.PropertyToID("_PinA"), pinB = Shader.PropertyToID("_PinB");
        readonly int bendA = Shader.PropertyToID("_BendA"), bendB = Shader.PropertyToID("_BendB");
        readonly int uvScale = Shader.PropertyToID("_PoseUvScale");
        float travel, lastPosition;
        int lastStep = -1, mode;
        bool lastRight;
        Transform carrier;
        public int Frame { get; private set; }
        public float Cycle { get; private set; }
        public Vector2 Plant { get; private set; }
        public Vector2 Grip { get; private set; }
        public bool Planted { get; private set; }
        public bool Strike { get; private set; }
        public Vector2 Lantern { get; private set; }

        public CuoiLocomotion(SpriteRenderer actor, Material material)
        {
            this.actor = actor; surface = material;
            run = Load("CuoiRun", 8);
            var brace = Load("CuoiClimb", 4);
            var pull = Load("CuoiClimbPull", 4);
            climb = new[] { pull[0], pull[1], pull[2], brace[0], pull[3], brace[1], brace[2], brace[3] };
            mantle = Load("CuoiMantle", 4);
        }

        static Sprite[] Load(string name, int count)
        {
            var sheet = Resources.LoadAll<Sprite>(name);
            Array.Sort(sheet, (a,b) => string.CompareOrdinal(a.name, b.name));
            if (sheet.Length != count) throw new InvalidOperationException(name + " requires " + count + " imported poses.");
            return sheet;
        }

        public void Reset()
        {
            if (mode != 0 && surface)
            {
                surface.SetVector(bendA, Vector4.zero);
                surface.SetVector(bendB, Vector4.zero);
            }
            mode = 0; travel = Cycle = 0; Frame = 0; Planted = Strike = false; carrier = null;
        }

        void Advance(int next, float position, float length, bool right, Transform support, bool held)
        {
            bool entering = mode != next || lastRight != right;
            if (entering) { travel = 0; lastStep = 0; }
            else if (carrier == support && !held)
            {
                float delta = position - lastPosition;
                travel += next == 1 ? Mathf.Abs(delta) : Mathf.Max(0, delta);
            }
            int step = Mathf.FloorToInt(travel * 2 / length);
            Strike = !entering && step != lastStep;
            lastStep = step; lastRight = right; lastPosition = position; mode = next; carrier = support;
            Cycle = Mathf.Repeat(travel / length, 1);
            Frame = Mathf.Min(7, Mathf.FloorToInt(Cycle * 8));
        }

        public void Run(Vector2 feet, bool right, Transform support, float inset)
        {
            float x = feet.x - (support ? support.position.x : 0);
            Advance(1, x, RunStride, right, support, false);
            var sprite = actor.sprite = run[Frame];
            actor.flipX = !right;
            float within = Mathf.Repeat(Cycle * 2, 1);
            actor.transform.localScale = Vector3.one;
            float recoil = -.035f * Mathf.Sin(Mathf.Min(1, within * 2) * Mathf.PI);
            actor.transform.localPosition = Vector3.up * (recoil - inset);
            Vector2 sole = Local(sprite, RunToes[Frame]);
            Planted = Frame % 4 < 3;
            Vector2 goal = sole;
            if (Planted)
            {
                // A stance covers 3/8 of a stride. The supporting sole travels back
                // exactly as fast as the skeleton moves forward, including on rafts.
                goal.x = RunStride * (.1875f - within * .5f);
                goal.y = -recoil;
            }
            Pin(sprite, sole, goal, new Vector2(.58f, .50f), Vector2.zero, Vector2.zero, Vector2.one);
            Plant = World(goal, right); Grip = Lantern = Local(sprite, RunGrips[Frame]);
            Grip = World(Grip, right);
        }

        public void Climb(Vector2 feet, bool right, float faceX, float faceTop, bool holding)
        {
            Advance(4, feet.y, ClimbStride, right, null, holding);
            float lip = faceTop - feet.y;
            if (lip <= 1.42f)
            {
                int key = lip > 1.14f ? 0 : lip > .78f ? 1 : 2;
                Mantle(feet, right, faceX, faceTop, key);
                return;
            }
            var sprite = actor.sprite = climb[Frame];
            int sign = right ? 1 : -1;
            actor.flipX = !right; actor.transform.localScale = Vector3.one;
            var hand = Local(sprite, WallHands[Frame]);
            var toe = Local(sprite, WallToes[Frame]);
            // Sash/pelvis moves next to the wall; the supporting hand sets the true wall plane.
            actor.transform.localPosition = new Vector3(faceX - feet.x - hand.x * sign, 0, 0);
            float halfPhase = Mathf.Repeat(Cycle * 2, 1);
            var heldHand = new Vector2(hand.x, 1.85f - halfPhase * ClimbStride * .5f);
            // Weight transfers to the new foothold twice per stroke. Only the braced
            // leg is pinned; the lifted and trailing leg may swing away from the wall.
            var heldToe = new Vector2(hand.x - .012f, toe.y - (Mathf.Repeat(Cycle * 8, 1) - .5f) * ClimbStride / 8);
            Pin(sprite, hand, heldHand, new Vector2(.69f,.82f), toe, heldToe, new Vector2(.82f,.57f));
            Grip = World(heldHand, right); Plant = World(heldToe, right); Planted = false;
            Lantern = Local(sprite, WallBelts[Frame]) + Vector2.left * .08f;
        }

        public void Mantle(Vector2 feet, bool right, float faceX, float faceTop, int key)
        {
            Frame = 8 + key; Strike = false;
            var sprite = actor.sprite = mantle[key];
            var hand = Local(sprite, MantleHands[key]);
            var sole = Local(sprite, MantleSoles[key]);
            var direction = right ? 1 : -1;
            // The weight moves from the fingertips, over the palms, onto a knee
            // and eventually the sole. The same stone lip supports each contact.
            float reach = key == 0 ? .04f : key == 1 ? .12f : key == 2 ? .39f : .97f;
            actor.flipX = !right; actor.transform.localScale = Vector3.one;
            actor.transform.localPosition = Vector2.right * (faceX - feet.x + direction * (reach - hand.x));
            float gap = faceTop - feet.y;
            float rise = key == 2 ? Mathf.Clamp(gap - hand.y, -.25f, .10f) : 0;
            actor.transform.localPosition += Vector3.up * rise;
            var heldHand = new Vector2(hand.x, gap - rise + .015f);
            Vector2 heldSole = key >= 2 ? new Vector2(sole.x, heldHand.y) : sole;
            Pin(sprite, hand, heldHand, new Vector2(.68f,.73f), sole, heldSole, new Vector2(.55f,.62f));
            Grip = World(heldHand,right); Plant = World(heldSole,right); Planted = key == 3;
            Lantern = Local(sprite,MantleBelts[key]) + Vector2.left * .06f;
        }

        Vector2 World(Vector2 point, bool right)
        {
            if (!right) point.x = -point.x;
            return actor.transform.TransformPoint(point);
        }

        static Vector2 Local(Sprite sprite, Vector2 pixels)
        {
            pixels.y = sprite.texture.height - pixels.y;
            return (pixels - sprite.rect.position - sprite.pivot) / sprite.pixelsPerUnit;
        }

        void Pin(Sprite sprite, Vector2 a, Vector2 targetA, Vector2 radiusA, Vector2 b, Vector2 targetB, Vector2 radiusB)
        {
            if (!surface) return;
            surface.SetVector(uvScale, new Vector2(sprite.pixelsPerUnit / sprite.texture.width, sprite.pixelsPerUnit / sprite.texture.height));
            surface.SetVector(pinA, new Vector4(targetA.x,targetA.y,1/radiusA.x,1/radiusA.y));
            surface.SetVector(pinB, new Vector4(targetB.x,targetB.y,1/radiusB.x,1/radiusB.y));
            surface.SetVector(bendA, targetA-a); surface.SetVector(bendB, targetB-b);
        }
    }
}
