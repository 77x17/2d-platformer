using UnityEditor;
using UnityEditor.Callbacks;

namespace Moonlit.Editor
{
    public static class PortableWebBuild
    {
        [PostProcessBuild(2000)]
        public static void PackWebFiles(BuildTarget target, string outputDirectory)
        {
            if (target == BuildTarget.WebGL) PortableWebFiles.Pack(outputDirectory);
        }
    }
}
