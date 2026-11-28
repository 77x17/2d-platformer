using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moonlit
{
    // A chapter's hierarchy is its source of truth for the traversable world.
    public sealed class MoonlitChapterScene : MonoBehaviour
    {
        public static readonly string[] SceneNames = {
            "01_Bamboo_Village", "02_Bamboo_Forest", "03_Lantern_River", "04_Banyan_Hill"
        };

        [Range(1,4)] public int chapter = 1;
        public MoonlitArt art;
        public PlayerTuning tuning;
        public Transform spawn, moon, boundary, guardian;
        public Transform[] checkpoints;
        public Transform[] hiddenMotes;
        public ChapterSurface[] banks, platforms;

        void Awake()
        {
            // Even when starting directly in a chapter, initialization waits for its
            // scene to become active. All generated scenery then belongs to that scene.
            SceneManager.sceneLoaded += Loaded;
        }

        void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != gameObject.scene) return;
            SceneManager.sceneLoaded -= Loaded;
            MoonlitGame.EnterScene(this);
        }

        void OnDestroy() => SceneManager.sceneLoaded -= Loaded;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            if (hiddenMotes != null) foreach (var mote in hiddenMotes) Marker(mote, mote ? mote.name : string.Empty, new Color(1,.73f,.24f));
            Marker(spawn, "Player Spawn", Color.cyan);
            Marker(moon, "Moon Fragment", Color.yellow);
            Marker(guardian, "Guardian", new Color(1,.45f,.25f));
            if (checkpoints != null)
                for (int i=0; i<checkpoints.Length; i++) Marker(checkpoints[i],"Checkpoint " + (i+1), Color.green);
        }
        static void Marker(Transform point, string label, Color color)
        {
            if (!point) return;
            Gizmos.color = color;
            Gizmos.DrawLine(point.position, point.position + Vector3.up * 2);
            Gizmos.DrawWireSphere(point.position + Vector3.up * 2, .25f);
            UnityEditor.Handles.Label(point.position + Vector3.up * 2.4f, label);
        }
#endif
    }
}
