using Eggcellent.Models;
using Microsoft.Win32;

namespace Eggcellent.Services
{
    /// <summary>
    /// Manages Windows startup entries under the standard "Run" registry keys.
    /// Disabling an item backs its command up under a Eggcellent-owned key so it can
    /// be restored later, rather than deleting it outright.
    /// </summary>
    public static class StartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string BackupKeyPath = @"Software\Eggcellent\DisabledStartupItems";

        public static List<StartupItem> GetItems()
        {
            var items = new List<StartupItem>();

            AddFromHive(items, Registry.CurrentUser, StartupHive.CurrentUser, enabled: true);
            AddFromHive(items, Registry.LocalMachine, StartupHive.LocalMachine, enabled: true);

            using var backupKey = Registry.CurrentUser.OpenSubKey(BackupKeyPath, writable: false);
            if (backupKey != null)
            {
                foreach (var name in backupKey.GetValueNames())
                {
                    if (items.Any(i => i.Name == name)) continue;
                    var command = backupKey.GetValue(name) as string ?? "";
                    items.Add(new StartupItem(name, command, StartupHive.CurrentUser, isEnabled: false));
                }
            }

            return items.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void AddFromHive(List<StartupItem> items, RegistryKey rootKey, StartupHive hive, bool enabled)
        {
            try
            {
                using var key = rootKey.OpenSubKey(RunKeyPath, writable: false);
                if (key is null) return;

                foreach (var name in key.GetValueNames())
                {
                    var command = key.GetValue(name) as string ?? "";
                    items.Add(new StartupItem(name, command, hive, enabled));
                }
            }
            catch
            {
                // No access (common for HKLM without elevation) — just skip.
            }
        }

        public static bool SetEnabled(StartupItem item, bool enable)
        {
            try
            {
                if (item.Hive == StartupHive.LocalMachine)
                {
                    // Machine-wide entries need admin rights to modify; not supported here.
                    return false;
                }

                using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                                   ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);
                using var backupKey = Registry.CurrentUser.CreateSubKey(BackupKeyPath);

                if (enable)
                {
                    runKey.SetValue(item.Name, item.Command);
                    backupKey.DeleteValue(item.Name, throwOnMissingValue: false);
                }
                else
                {
                    backupKey.SetValue(item.Name, item.Command);
                    runKey.DeleteValue(item.Name, throwOnMissingValue: false);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Delete(StartupItem item)
        {
            try
            {
                if (item.Hive == StartupHive.LocalMachine) return false;

                using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
                runKey?.DeleteValue(item.Name, throwOnMissingValue: false);

                using var backupKey = Registry.CurrentUser.OpenSubKey(BackupKeyPath, writable: true);
                backupKey?.DeleteValue(item.Name, throwOnMissingValue: false);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
