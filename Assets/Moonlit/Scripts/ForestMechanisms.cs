using UnityEngine;

namespace Moonlit
{
    [DefaultExecutionOrder(-30)]
    public sealed class MovingPlatform : MonoBehaviour
    {
        public Vector2 travel = new Vector2(0, 1.4f);
        public float period = 4;
        public Vector2 Delta { get; private set; }
        Vector2 origin, previous;
        Rigidbody2D rb;
        float clock;
        void Awake() { rb = gameObject.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.interpolation = RigidbodyInterpolation2D.Interpolate; }
        void Start() { origin = previous = rb.position; }
        void FixedUpdate()
        {
            if (MoonlitGame.Instance.State != GameState.Playing) { Delta = Vector2.zero; return; }
            clock += Time.fixedDeltaTime;
            Vector2 p = origin + travel * Mathf.Sin(clock * Mathf.PI * 2 / period);
            Delta = p - previous; previous = p; rb.MovePosition(p);
        }
        public void ResetPlatform() { clock = 0; rb.position = origin; previous = origin; Delta = Vector2.zero; }
        public Vector2 FuturePosition(float seconds) => origin + travel * Mathf.Sin((clock + seconds) * Mathf.PI * 2 / period);
    }

    public sealed class FallingPlatform : MonoBehaviour
    {
        public bool Fallen { get; private set; }
        public int CollapseCount { get; private set; }
        float timer = -1;
        Vector3 origin;
        Collider2D surface;
        SpriteRenderer[] renderers;
        void Start() { origin = transform.position; surface = GetComponent<Collider2D>(); renderers = GetComponentsInChildren<SpriteRenderer>(); }
        void OnCollisionStay2D(Collision2D collision)
        {
            if (timer < 0 && collision.collider.GetComponent<CuoiController>() && collision.transform.position.y > origin.y - .15f) timer = .7f;
        }
        void Update()
        {
            if ((timer < 0 && !Fallen) || MoonlitGame.Instance.State != GameState.Playing) return;
            timer -= Time.deltaTime;
            if (timer > 0)
            {
                transform.position = origin + Vector3.right * Mathf.Sin(Time.time * 65) * .035f;
                foreach (var sr in renderers) sr.color = Color.Lerp(Color.white, Visuals.Gold, (Mathf.Sin(Time.time * 25) + 1) * .25f);
            }
            else
            {
                if (!Fallen) { Fallen = true; surface.enabled = false; CollapseCount++; MoonlitGame.Instance.effects.Burst(origin, Visuals.Gold, 16, 2); }
                transform.position += Vector3.down * Time.deltaTime * 5;
                foreach (var sr in renderers) { var c = sr.color; c.a = Mathf.Clamp01(1 + timer * .6f); sr.color = c; }
                if (timer < -3.5f) ResetPlatform();
            }
        }
        public void ResetPlatform()
        {
            timer = -1; Fallen = false; transform.position = origin;
            if (surface) surface.enabled = true;
            if (renderers != null) foreach (var sr in renderers) sr.color = Color.white;
        }
    }

    public enum PickupKind { Mote, Checkpoint, Moon }
    public sealed class ForestPickup : MonoBehaviour
    {
        public PickupKind kind;
        public Vector2 respawn;
        public int index;
        public bool Collected { get; private set; }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<CuoiController>() || MoonlitGame.Instance.State != GameState.Playing || Collected) return;
            var g = MoonlitGame.Instance;
            if (kind == PickupKind.Mote)
            {
                Collected = true; g.Motes++; g.effects.Burst(transform.position, Visuals.Mint, 10, 2); g.audioFx.Play("collect"); gameObject.SetActive(false);
            }
            else if (kind == PickupKind.Checkpoint)
            {
                if (index > g.CheckpointIndex)
                {
                    Collected = true; g.SetCheckpoint(index, respawn); g.effects.Ring(transform.position + Vector3.up, Visuals.Gold, 2.4f);
                    g.effects.Burst(transform.position + Vector3.up, Visuals.Gold, 20, 3);
                }
            }
            else { Collected = true; g.Complete(); }
        }
        public void ResetPickup() { Collected = false; gameObject.SetActive(true); }
    }

    public sealed class LanternSwitch : MonoBehaviour
    {
        public GameObject bridge;
        public bool Activated { get; private set; }
        SpriteRenderer glow;
        void Start() { glow = Visuals.Halo("Sleeping shrine light", transform.position + Vector3.up * 1.1f, 3, new Color(.5f, .8f, 1, .3f), transform); }
        public bool Near => Vector2.Distance(MoonlitGame.Instance.player.transform.position, transform.position) < 2.5f;
        public void Activate()
        {
            if (Activated || !Near) return;
            Activated = true; bridge.SetActive(true); glow.color = new Color(1, .75f, .3f, .8f);
            var g = MoonlitGame.Instance; g.effects.Ring(transform.position + Vector3.up, Visuals.Gold, 5);
            g.effects.Burst(transform.position + Vector3.up, Visuals.Gold, 35, 3); g.audioFx.Play("checkpoint");
            g.Toast("CẦU ĐÃ THỨC GIẤC", "Ánh đèn nối lại con đường qua khe núi.");
        }
        public void ResetSwitch() { Activated = false; bridge.SetActive(false); if (glow) glow.color = new Color(.5f, .8f, 1, .3f); }
    }
}
