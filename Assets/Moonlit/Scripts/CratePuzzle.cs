using UnityEngine;

namespace Moonlit
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PushableCrate : MonoBehaviour
    {
        public Rigidbody2D Body { get; private set; }
        public float DistancePushed { get; private set; }
        public bool Docked { get; private set; }
        Vector2 origin;
        float lastPush = -1;
        void Awake()
        {
            Body = GetComponent<Rigidbody2D>(); origin = Body.position; Body.mass = 3; Body.gravityScale = 3;
            Body.constraints = RigidbodyConstraints2D.FreezeRotation;
            Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var box = GetComponent<BoxCollider2D>(); box.size = new Vector2(1.32f, 1.32f);
            box.sharedMaterial = new PhysicsMaterial2D("Crate grip") { friction = .45f, bounciness = 0 };
        }

        public void Push(float direction)
        {
            if (Docked || MoonlitGame.Instance.State != GameState.Playing) return;
            lastPush = Time.fixedTime; Body.linearVelocity = new Vector2(direction * 2f, Body.linearVelocity.y);
        }
        void FixedUpdate()
        {
            if (MoonlitGame.Instance.State != GameState.Playing) { Body.linearVelocity = Vector2.zero; return; }
            if (Time.fixedTime - lastPush > Time.fixedDeltaTime * 2)
                Body.linearVelocity = new Vector2(Mathf.MoveTowards(Body.linearVelocity.x, 0, 18 * Time.fixedDeltaTime), Body.linearVelocity.y);
            DistancePushed = Mathf.Max(DistancePushed, Mathf.Abs(Body.position.x - origin.x));
            if (Body.position.y < -6) ResetCrate();
        }
        public void Dock(Vector2 at)
        {
            Docked = true; Body.linearVelocity = Vector2.zero; Body.bodyType = RigidbodyType2D.Kinematic;
            Body.position = at;
        }
        public void ResetCrate()
        {
            Docked = false; Body.bodyType = RigidbodyType2D.Dynamic; Body.position = origin;
            Body.linearVelocity = Vector2.zero; Body.angularVelocity = 0; lastPush = -1; DistancePushed = 0;
        }
    }

    public sealed class CratePressureBridge : MonoBehaviour
    {
        public PushableCrate crate;
        public Transform bridge;
        public BoxCollider2D bridgeCollider;
        public SpriteRenderer plate;
        public Vector2 plateCenter;
        public float floorY;
        public bool Activated { get; private set; }
        public bool Open => Activated && progress >= 1;
        public float Progress => progress;
        float progress;
        Vector3 down;
        void Start() { down = bridge.position; ResetPuzzle(); }
        void Update()
        {
            var g = MoonlitGame.Instance;
            if (g.State != GameState.Playing) return;
            if (!Activated && Mathf.Abs(crate.Body.position.x - plateCenter.x) < .36f && Mathf.Abs(crate.Body.position.y - (floorY + .68f)) < .22f)
            {
                Activated = true; crate.Dock(new Vector2(plateCenter.x, floorY + .68f));
                g.effects.Ring(plateCenter, Visuals.Gold, 2);
                g.effects.Burst(plateCenter, Visuals.Gold, 24, 2); g.audioFx.Play("checkpoint");
                g.Toast("CẦU GỖ ĐÃ HẠ", "Thùng giữ bàn đạp. Nhảy qua thùng rồi đi tiếp.");
            }
            if (Activated)
            {
                progress = Mathf.MoveTowards(progress, 1, Time.deltaTime / .85f);
                bridge.position = down + Vector3.up * (3.5f * (1 - Mathf.SmoothStep(0, 1, progress)));
                plate.color = Color.Lerp(Color.white, Visuals.Gold, progress);
                bridgeCollider.enabled = Open;
            }
        }
        public void ResetPuzzle()
        {
            Activated = false; progress = 0; bridgeCollider.enabled = false;
            bridge.position = down + Vector3.up * 3.5f;
            if (plate) plate.color = Color.white;
            if (crate) crate.ResetCrate();
        }
    }
}
