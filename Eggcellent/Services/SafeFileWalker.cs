using System.IO;

namespace Eggcellent.Services
{
    /// <summary>
    /// Walks directory trees while silently skipping folders that throw
    /// access-denied or other IO exceptions, instead of aborting the whole scan.
    /// </summary>
    public static class SafeFileWalker
    {
        public static IEnumerable<string> EnumerateFiles(string root)
        {
            if (!Directory.Exists(root)) yield break;

            var dirs = new Stack<string>();
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                var dir = dirs.Pop();

                string[] files = Array.Empty<string>();
                try { files = Directory.GetFiles(dir); } catch { }
                foreach (var f in files) yield return f;

                string[] subDirs = Array.Empty<string>();
                try { subDirs = Directory.GetDirectories(dir); } catch { }
                foreach (var d in subDirs) dirs.Push(d);
            }
        }

        public static long SafeLength(string file)
        {
            try { return new FileInfo(file).Length; }
            catch { return 0; }
        }

        public static DateTime SafeLastWrite(string file)
        {
            try { return new FileInfo(file).LastWriteTime; }
            catch { return DateTime.MinValue; }
        }

        public static void RemoveEmptySubdirectories(string root)
        {
            if (!Directory.Exists(root)) return;
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(root))
                {
                    try
                    {
                        RemoveEmptySubdirectories(dir);
                        if (!Directory.EnumerateFileSystemEntries(dir).Any())
                            Directory.Delete(dir);
                    }
                    catch { /* locked or protected, skip */ }
                }
            }
            catch { }
        }
    }
}
