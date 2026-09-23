using Eggcellent.Models;
using Microsoft.Win32;

namespace Eggcellent.Services
{
    public static class InstalledAppsService
    {
        private const string UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string UninstallKeyPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

        public static List<InstalledAppItem> GetInstalledApps()
        {
            var apps = new List<InstalledAppItem>();

            ReadFrom(Registry.LocalMachine, UninstallKeyPath, apps);
            ReadFrom(Registry.LocalMachine, UninstallKeyPathWow64, apps);
            ReadFrom(Registry.CurrentUser, UninstallKeyPath, apps);

            return apps
                .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool LaunchUninstaller(InstalledAppItem item)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", $"/c {item.UninstallString}")
                {
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ReadFrom(RegistryKey root, string path, List<InstalledAppItem> apps)
        {
            try
            {
                using var uninstallKey = root.OpenSubKey(path, writable: false);
                if (uninstallKey is null) return;

                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    try
                    {
                        using var appKey = uninstallKey.OpenSubKey(subKeyName, writable: false);
                        if (appKey is null) continue;

                        var name = appKey.GetValue("DisplayName") as string;
                        if (string.IsNullOrWhiteSpace(name)) continue;

                        // Skip Windows updates / system components that clutter the list.
                        if (Convert.ToInt32(appKey.GetValue("SystemComponent", 0)) == 1) continue;
                        if (!string.IsNullOrEmpty(appKey.GetValue("ParentKeyName") as string)) continue;

                        var publisher = appKey.GetValue("Publisher") as string ?? "";
                        var version = appKey.GetValue("DisplayVersion") as string ?? "";
                        var uninstallString = appKey.GetValue("UninstallString") as string ?? "";
                        var installLocation = appKey.GetValue("InstallLocation") as string;

                        long sizeBytes = 0;
                        var sizeKb = appKey.GetValue("EstimatedSize");
                        if (sizeKb != null) sizeBytes = Convert.ToInt64(sizeKb) * 1024L;

                        if (string.IsNullOrWhiteSpace(uninstallString)) continue;

                        apps.Add(new InstalledAppItem(name, publisher, version, sizeBytes, uninstallString, installLocation));
                    }
                    catch
                    {
                        // Skip entries we can't read.
                    }
                }
            }
            catch
            {
                // No access to this hive/key — skip.
            }
        }
    }
}
