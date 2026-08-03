using Eggcellent.Models;
using System.IO;

namespace Eggcellent.Services
{
    public static class DiskAnalyzerService
    {
        /// <summary>
        /// Returns the immediate subfolders of <paramref name="path"/> with their total
        /// recursive size, plus a synthetic "Files here" entry for loose files in the folder
        /// itself. Sorted largest first.
        /// </summary>
        public static List<FolderSizeItem> AnalyzeFolder(string path, CancellationToken token = default)
        {
            var items = new List<FolderSizeItem>();
            if (!Directory.Exists(path)) return items;

            string[] subDirs = Array.Empty<string>();
            try { subDirs = Directory.GetDirectories(path); } catch { }

            foreach (var dir in subDirs)
            {
                if (token.IsCancellationRequested) break;

                long size = 0;
                foreach (var file in SafeFileWalker.EnumerateFiles(dir))
                    size += SafeFileWalker.SafeLength(file);

                items.Add(new FolderSizeItem(Path.GetFileName(dir), dir, size, isFolder: true));
            }

            long looseFilesSize = 0;
            try
            {
                foreach (var file in Directory.GetFiles(path))
                    looseFilesSize += SafeFileWalker.SafeLength(file);
            }
            catch { }

            if (looseFilesSize > 0)
                items.Add(new FolderSizeItem("(Files in this folder)", path, looseFilesSize, isFolder: false));

            long total = items.Sum(i => i.SizeBytes);
            foreach (var item in items)
                item.PercentOfParent = total == 0 ? 0 : Math.Round((double)item.SizeBytes / total * 100, 1);

            return items.OrderByDescending(i => i.SizeBytes).ToList();
        }
    }
}
