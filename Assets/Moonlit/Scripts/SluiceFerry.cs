using UnityEngine;

namespace Moonlit
{
    public sealed class SluiceFerry : MonoBehaviour
    {
        public enum LockState { AtLeft, GoingRight, AtRight, GoingLeft }
        public LockState State { get; private set; }
        public MovingPlatform Float { get; private set; }
        Vector2 left, right;
        SpriteRenderer beacon, westBell, eastBell;
        public LineRenderer Cable { get; private set; }
        public LineRenderer Hanger { get; private set; }
        Vector2 westEye, eastEye;
        Transform trolley;
        float t;
        const int CablePoints = 37;
        const float Sag = .67f;

        public void Build(MovingPlatform deck, Vector2 dock, Vector2 exit, SpriteRenderer light,
            Vector2 west, Vector2 east, SpriteRenderer westControl, SpriteRenderer eastControl)
        {
            Float = deck; left = dock; right = exit; beacon = light;
            westEye = west; eastEye = east; westBell = westControl; eastBell = eastControl;
            Cable = Rope("Fixed bank-to-bank cable", CablePoints, .105f, 3);
            for (int i = 0; i < CablePoints; i++)
                Cable.SetPosition(i, CablePoint(Mathf.Lerp(westEye.x, eastEye.x, i / (CablePoints - 1f))));
            Hanger = Rope("Ferry suspension line", 2, .082f, 5);
            trolley = new GameObject("Cable traveller").transform;
            trolley.SetParent(transform, false);
            var shoes = new Color(.30f, .27f, .19f);
            foreach (float x in new[] { -.16f, .16f })
            {
                Visuals.Box("Trolley roller", new Vector2(x, .05f), new Vector2(.19f,.16f), shoes, trolley, 6);
                Visuals.Box("Roller axle", new Vector2(x,.052f), Vector2.one*.075f, Visuals.Gold, trolley, 7);
            }
            Visuals.Box("Trolley saddle", Vector2.down * .095f, new Vector2(.51f,.11f), Visuals.Gold, trolley, 7);
            Visuals.Box("Raft lifting eye", dock + new Vector2(0,.21f), new Vector2(.29f,.3f), shoes, Float.transform, 6);
            Float.Ferry = this;
            UpdateCable();
        }

        LineRenderer Rope(string label, int count, float width, int order)
        {
            var node = new GameObject(label);
            node.transform.SetParent(transform, false);
            var line = node.AddComponent<LineRenderer>();
            line.sharedMaterial = Visuals.Material;
            line.useWorldSpace = true; line.positionCount = count; line.sortingOrder = order;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = new Color(.74f,.61f,.37f);
            return line;
        }

        Vector2 CablePoint(float x)
        {
            float u = Mathf.InverseLerp(westEye.x, eastEye.x, x);
            return new Vector2(x, Mathf.Lerp(westEye.y, eastEye.y, u) - 4 * Sag * u * (1 - u));
        }

        public bool Boarded(Vector2 feet)
        {
            Vector2 at = Float.PhysicsPosition;
            return Mathf.Abs(feet.x-at.x) < 2.5f && Mathf.Abs(feet.y-at.y) < .24f;
        }

        public bool Interact(Vector2 feet)
        {
            if (State != LockState.AtLeft && State != LockState.AtRight) return false;
            bool atLeft = State == LockState.AtLeft;
            var game = MoonlitGame.Instance;
            bool aboard = game.player.Grounded && Boarded(feet);
            bool AtPost(SpriteRenderer bell)
            {
                Vector2 post = bell.transform.position;
                return Mathf.Abs(feet.x - post.x) < 1.85f && Mathf.Abs(feet.y + 1.23f - post.y) < .8f;
            }
            bool onBank = game.player.Grounded && (AtPost(westBell) || AtPost(eastBell));
            if (!aboard && !onBank) return false;
            State = atLeft ? LockState.GoingRight : LockState.GoingLeft;
            t = 0; game.audioFx.Play("signal2");
            game.effects.Ring(beacon.transform.position, Visuals.Mint, 2);
            return true;
        }

        public Vector2 Step(float dt)
        {
            if (State == LockState.AtLeft) return left;
            if (State == LockState.AtRight) return right;
            bool outbound = State == LockState.GoingRight;
            t = Mathf.Min(1,t + dt/4.5f);
            float phase = Mathf.SmoothStep(0,1,t);
            float q = outbound ? phase : 1-phase;
            if (t >= 1) { State = outbound ? LockState.AtRight : LockState.AtLeft; MoonlitGame.Instance.audioFx.Play("deckSettle"); }
            return Vector2.Lerp(left,right,q) + Vector2.up*(.45f*Mathf.Sin(q*Mathf.PI));
        }

        public void ResetLock() { State = LockState.AtLeft; t = 0; }
        void LateUpdate()
        {
            bool docked = State == LockState.AtLeft || State == LockState.AtRight;
            beacon.color = docked ? Visuals.Gold : Visuals.Mint;
            westBell.color = State == LockState.AtRight ? Visuals.Gold : Visuals.Mint;
            eastBell.color = State == LockState.AtLeft ? Visuals.Gold : Visuals.Mint;
            UpdateCable();
        }

        void UpdateCable()
        {
            Vector2 foot = (Vector2)Float.transform.position + Vector2.up * .26f;
            var truck = CablePoint(foot.x);
            trolley.position = truck;
            Hanger.SetPosition(0, truck - Vector2.up * .09f);
            Hanger.SetPosition(1, foot);
        }
    }
}
