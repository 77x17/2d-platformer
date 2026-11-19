using UnityEditor;

namespace Moonlit.Editor
{
    // One-time asset wiring after the Editor compiles. Never builds or starts Play mode.
    [InitializeOnLoad]
    static class ReadabilityAssetImporter
    {
        static ReadabilityAssetImporter() { EditorApplication.update += ImportWhenReady; }
        static void ImportWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= ImportWhenReady;
            LevelOneBuilder.PrepareReadabilityAssets();
        }
    }
}
