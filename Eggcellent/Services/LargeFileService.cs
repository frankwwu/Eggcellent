using Eggcellent.Models;

namespace Eggcellent.Services
{
    public static class LargeFileService
    {
        public static List<FileItem> Scan(string rootPath, long minSizeBytes, int maxResults, CancellationToken token = default)
        {
            var results = new List<FileItem>();

            foreach (var file in SafeFileWalker.EnumerateFiles(rootPath))
            {
                if (token.IsCancellationRequested) break;

                long size = SafeFileWalker.SafeLength(file);
                if (size < minSizeBytes) continue;

                results.Add(new FileItem(file, size, SafeFileWalker.SafeLastWrite(file)));
            }

            return results
                .OrderByDescending(f => f.SizeBytes)
                .Take(maxResults)
                .ToList();
        }
    }
}
