using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moonlit
{
    public sealed class MoonlitHomeScene : MonoBehaviour
    {
        public const string SceneName = "00_Home";
        public MoonlitArt art;
        public PlayerTuning tuning;
        public SpriteRenderer backdrop;

        void Awake() => SceneManager.sceneLoaded += Loaded;
        void OnDestroy() => SceneManager.sceneLoaded -= Loaded;
        void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != gameObject.scene) return;
            SceneManager.sceneLoaded -= Loaded;
            MoonlitGame.EnterHomeScene(this);
            FitBackdrop();
        }

        void LateUpdate() => FitBackdrop();
        void FitBackdrop()
        {
            var session = MoonlitGame.Instance;
            if (!session || !session.ViewCamera || !backdrop || !backdrop.sprite) return;
            var camera = session.ViewCamera;
            float height = camera.orthographicSize * 2;
            var bounds = backdrop.sprite.bounds;
            float scale = Mathf.Min(height * camera.aspect / bounds.size.x, height / bounds.size.y);
            backdrop.transform.localScale = Vector3.one * scale;
            backdrop.transform.localPosition = -bounds.center * scale;
        }
    }
}