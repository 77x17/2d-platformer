using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Moonlit.Editor
{
    // A classic script can load from the local directory even when fetch(file:...) is unavailable.
    public static class PortableWebFiles
    {
        const int ChunkSize = 96 * 1024;
        const string Marker = "<meta name=\"moonlit-portable\" content=\"1\">";
        static readonly Regex Slots = new Regex(
            @"\b(?:dataUrl|frameworkUrl|codeUrl)\s*:\s*""Build/(?<name>[^""/\\:?#<>]+\.unityweb)\.js""",
            RegexOptions.CultureInvariant);

        // Returns false for other Unity Web templates, which keep their original files.
        public static bool Pack(string outputDirectory)
        {
            var root = Path.GetFullPath(outputDirectory);
            var index = Path.Combine(root, "index.html");
            if (!File.Exists(index)) return false;
            var page = File.ReadAllText(index);
            if (!page.Contains(Marker)) return false;
            var names = Slots.Matches(page).Cast<Match>().Select(match => match.Groups["name"].Value).Distinct().ToArray();
            if (names.Length != 3)
                throw new InvalidDataException("Expected three portable Unity payload URLs in " + index);

            var build = Path.Combine(root, "Build");
            var lockPath = Path.Combine(build, ".moonlit-pack.lock");
            using (new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None,
                       4096, FileOptions.DeleteOnClose))
            {
                var sources = names.Select(name => Path.Combine(build, name)).ToArray();
                foreach (var source in sources)
                    if (!File.Exists(source) && !File.Exists(source + ".js"))
                        throw new FileNotFoundException("Missing Unity Web payload", source);

                // Incremental builds may regenerate only a subset of the three files.
                var changed = sources.Where(File.Exists).ToArray();
                if (changed.Length > 0) Publish(build, changed);
            }
            return true;
        }

        static void Publish(string build, string[] sources)
        {
            var staging = Path.Combine(build, ".moonlit-pack-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            var replaced = new List<string>(sources.Length);
            var retired = new List<string>(sources.Length);
            bool removeStaging = false;
            try
            {
                foreach (var source in sources)
                    WriteScript(source, Path.Combine(staging, Path.GetFileName(source) + ".new"));

                foreach (var source in sources)
                {
                    var prefix = Path.Combine(staging, Path.GetFileName(source));
                    if (File.Exists(source + ".js"))
                        File.Replace(prefix + ".new", source + ".js", prefix + ".old");
                    else
                        File.Move(prefix + ".new", source + ".js");
                    replaced.Add(source);
                }

                // Retire originals only after every wrapper is in place. A locked source can
                // still abort the pack: restore both the originals and the previous wrappers.
                foreach (var source in sources)
                {
                    File.Move(source, Path.Combine(staging, Path.GetFileName(source)));
                    retired.Add(source);
                }
                removeStaging = true;
            }
            catch
            {
                foreach (var source in retired)
                    File.Move(Path.Combine(staging, Path.GetFileName(source)), source);
                foreach (var source in replaced)
                {
                    var previous = Path.Combine(staging, Path.GetFileName(source) + ".old");
                    if (File.Exists(previous)) File.Replace(previous, source + ".js", null);
                    else File.Delete(source + ".js");
                }
                // If rollback itself throws, retain the staging directory for recovery.
                removeStaging = true;
                throw;
            }
            finally
            {
                if (removeStaging) Directory.Delete(staging, true);
            }
        }

        static void WriteScript(string source, string target)
        {
            var bytes = new byte[ChunkSize];
            using (var input = File.OpenRead(source))
            using (var writer = new StreamWriter(target, false, new UTF8Encoding(false), 64 * 1024))
            {
                if (input.Length == 0) throw new InvalidDataException("Empty Unity Web payload: " + source);
                writer.WriteLine("window.moonlitWebFiles.accept(document.currentScript, [");
                bool first = true;
                int count;
                while ((count = input.Read(bytes, 0, bytes.Length)) != 0)
                {
                    if (!first) writer.WriteLine(",");
                    writer.Write("window.moonlitWebFiles.decode(\"");
                    writer.Write(Convert.ToBase64String(bytes, 0, count));
                    writer.Write("\")");
                    first = false;
                }
                writer.WriteLine();
                writer.WriteLine("]);");
            }
        }
    }
}
