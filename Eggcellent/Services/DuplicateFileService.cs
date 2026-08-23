using System.IO;
using System.Security.Cryptography;
using Eggcellent.Models;

namespace Eggcellent.Services
{
    public static class DuplicateFileService
    {
        public static List<DuplicateGroup> Scan(string rootPath, CancellationToken token = default)
        {
            // Step 1: group by file size — real duplicates must match size first.
            var bySize = new Dictionary<long, List<string>>();

            foreach (var file in SafeFileWalker.EnumerateFiles(rootPath))
            {
                if (token.IsCancellationRequested) break;
                long size = SafeFileWalker.SafeLength(file);
                if (size <= 0) continue;

                if (!bySize.TryGetValue(size, out var list))
                {
                    list = new List<string>();
                    bySize[size] = list;
                }
                list.Add(file);
            }

            var groups = new List<DuplicateGroup>();

            // Step 2: only hash files that share a size with at least one other file.
            foreach (var kvp in bySize)
            {
                if (token.IsCancellationRequested) break;
                if (kvp.Value.Count < 2) continue;

                var byHash = new Dictionary<string, List<string>>();
                foreach (var file in kvp.Value)
                {
                    string? hash = TryHashFile(file);
                    if (hash is null) continue;

                    if (!byHash.TryGetValue(hash, out var list))
                    {
                        list = new List<string>();
                        byHash[hash] = list;
                    }
                    list.Add(file);
                }

                foreach (var hashGroup in byHash)
                {
                    if (hashGroup.Value.Count < 2) continue;

                    var items = hashGroup.Value
                        .Select(f => new FileItem(f, kvp.Key, SafeFileWalker.SafeLastWrite(f)))
                        .OrderBy(f => f.LastModified)
                        .ToList();

                    // Keep the oldest copy unselected by default; mark the rest for deletion.
                    for (int i = 1; i < items.Count; i++) items[i].IsSelected = true;

                    groups.Add(new DuplicateGroup(hashGroup.Key, kvp.Key, items));
                }
            }

            return groups.OrderByDescending(g => g.WastedBytes).ToList();
        }

        private static string? TryHashFile(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                using var md5 = MD5.Create();
                var hash = md5.ComputeHash(stream);
                return Convert.ToHexString(hash);
            }
            catch
            {
                return null;
            }
        }
    }
}
