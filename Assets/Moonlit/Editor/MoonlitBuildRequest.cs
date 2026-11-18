using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moonlit.Editor
{
    // Enables Tools/Build-Game.ps1 while this project is already open in Unity.
    // The only accepted request is a local Windows build of this project.
    [InitializeOnLoad]
    static class MoonlitBuildRequest
    {
        static double nextPoll;
        static MoonlitBuildRequest() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextPoll) return;
            nextPoll = EditorApplication.timeSinceStartup + 1;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            const string request = "QA/build.request";
            if (!File.Exists(request)) return;
            string command = File.ReadAllText(request).Trim(); File.Delete(request);
            if (command != "build-windows") return;
            try { LevelOneBuilder.Build(); }
            catch (Exception ex) { File.WriteAllText("QA/build-result.txt", "FAILED\n" + ex); Debug.LogException(ex); }
        }
    }
}
