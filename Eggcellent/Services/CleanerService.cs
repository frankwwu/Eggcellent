using Eggcellent.Models;
using System.IO;

namespace Eggcellent.Services
{
    public static class CleanerService
    {
        public static List<CleanCategory> CreateDefaultCategories()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var systemDrive = Path.GetPathRoot(windowsDir) ?? "C:\\";

            var categories = new List<CleanCategory>
            {
                new CleanCategory(
                    "Temporary Files",
                    "Windows and app temp folders",
                    new[]
                    {
                        Path.GetTempPath(),
                        Path.Combine(windowsDir, "Temp")
                    }),

                new CleanCategory(
                    "Windows Error Reports",
                    "Crash and diagnostic reports",
                    new[] { Path.Combine(localAppData, "Microsoft", "Windows", "WER") }),

                new CleanCategory(
                    "Thumbnail Cache",
                    "Cached thumbnail images",
                    new[] { Path.Combine(localAppData, "Microsoft", "Windows", "Explorer") }),

                new CleanCategory(
                    "Recent Items",
                    "Shortcuts to recently opened files",
                    new[] { Path.Combine(roamingAppData, "Microsoft", "Windows", "Recent") }),

                new CleanCategory(
                    "Chrome Cache",
                    "Google Chrome browser cache",
                    new[] { Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Cache") }),

                new CleanCategory(
                    "Edge Cache",
                    "Microsoft Edge browser cache",
                    new[] { Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Cache") }),

                new CleanCategory(
                    "Firefox Cache",
                    "Mozilla Firefox browser cache",
                    GetFirefoxCachePaths(localAppData)),

                new CleanCategory(
                    "Delivery Optimization Cache",
                    "Windows Update files shared with other devices",
                    new[] { Path.Combine(programData, "Microsoft", "Windows", "DeliveryOptimization", "Cache") }),

                new CleanCategory(
                    "Windows Update Cache",
                    "Downloaded Windows Update installers — some files may be skipped while the Windows Update service is running",
                    new[] { Path.Combine(windowsDir, "SoftwareDistribution", "Download") },
                    defaultSelected: false),

                new CleanCategory(
                    "Prefetch Files",
                    "Windows app-launch cache — safe to clear, Windows rebuilds it automatically",
                    new[] { Path.Combine(windowsDir, "Prefetch") },
                    defaultSelected: false),

                new CleanCategory(
                    "Previous Windows Installation (Windows.old)",
                    "Backup of your prior Windows install, kept in case you want to roll back — only remove this once you're sure you won't need to",
                    new[] { Path.Combine(systemDrive, "Windows.old") },
                    defaultSelected: false),

                new CleanCategory(
                    "Recycle Bin",
                    "Files waiting to be permanently deleted",
                    Array.Empty<string>(),
                    isRecycleBin: true)
            };

            return categories;
        }

        private static string[] GetFirefoxCachePaths(string localAppData)
        {
            var profilesRoot = Path.Combine(localAppData, "Mozilla", "Firefox", "Profiles");
            if (!Directory.Exists(profilesRoot)) return Array.Empty<string>();

            try
            {
                return Directory.GetDirectories(profilesRoot)
                    .Select(p => Path.Combine(p, "cache2"))
                    .Where(Directory.Exists)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public static long ScanCategory(CleanCategory category)
        {
            if (category.IsRecycleBin) return RecycleBinService.GetSize();

            long total = 0;
            foreach (var path in category.Paths)
                foreach (var file in SafeFileWalker.EnumerateFiles(path))
                    total += SafeFileWalker.SafeLength(file);
            return total;
        }

        public static (long freed, int errors) CleanCategory(Models.CleanCategory category)
        {
            if (category.IsRecycleBin)
                return (category.SizeBytes, RecycleBinService.Empty() ? 0 : 1);

            long freed = 0;
            int errors = 0;

            foreach (var path in category.Paths)
            {
                foreach (var file in SafeFileWalker.EnumerateFiles(path))
                {
                    try
                    {
                        long len = new FileInfo(file).Length;
                        File.Delete(file);
                        freed += len;
                    }
                    catch
                    {
                        errors++;
                    }
                }

                SafeFileWalker.RemoveEmptySubdirectories(path);
            }

            return (freed, errors);
        }
    }
}
