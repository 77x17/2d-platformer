using UnityEngine;

namespace Moonlit
{
    public sealed class RisingLanternBridge : MonoBehaviour
    {
        public bool Rising { get; private set; }
        public bool Open { get; private set; }
        public float Rise { get; private set; }
        BoxCollider2D[] decks;
        Vector3[] resting;
        float depth, elapsed;
        const float RiseTime = 1.55f, Stagger = .14f;

        public void Build(float submersion)
        {
            depth = submersion;
            decks = GetComponentsInChildren<BoxCollider2D>(true);
            resting = new Vector3[decks.Length];
            for (int i = 0; i < decks.Length; i++) resting[i] = decks[i].transform.localPosition;
            Lower();
        }

        public void StartRising()
        {
            if (Rising) return;
            Rising = true;
            gameObject.SetActive(true);
        }

        void FixedUpdate()
        {
            if (!Rising || Open || MoonlitGame.Instance.State != GameState.Playing) return;
            elapsed += Time.fixedDeltaTime;
            float duration = RiseTime + (decks.Length - 1) * Stagger;
            Rise = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < decks.Length; i++)
            {
                var plank = decks[i].transform;
                float t = Mathf.Clamp01((elapsed - Stagger * i) / RiseTime);
                float swell = Mathf.Sin(t * Mathf.PI);
                float wobble = swell * Mathf.Sin(t * 3.5f * Mathf.PI);
                plank.localPosition = resting[i] + new Vector3(.065f * wobble,
                    -depth * (1 - Mathf.SmoothStep(0, 1, t)) + .13f * wobble, 0);
                plank.localRotation = Quaternion.Euler(0, 0, wobble * 2.8f);
                if (t >= 1) {
                    plank.localPosition = resting[i];
                    plank.localRotation = Quaternion.identity;
                    decks[i].enabled = true;
                }
            }
            if (Rise < 1) return;
            Open = true;
            MoonlitGame.Instance.audioFx.Play("deckSettle");
        }

        public void Lower()
        {
            Rising = Open = false;
            Rise = elapsed = 0;
            for (int i = 0; i < decks.Length; i++)
            {
                decks[i].enabled = false;
                decks[i].transform.localPosition = resting[i] + Vector3.down * depth;
                decks[i].transform.localRotation = Quaternion.identity;
            }
            gameObject.SetActive(false);
        }
    }
}
