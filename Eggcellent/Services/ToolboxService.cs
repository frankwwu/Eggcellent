using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Eggcellent.Services
{
    public static class ToolboxService
    {
        [DllImport("psapi.dll")]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        public static bool FlushDns()
        {
            try
            {
                var psi = new ProcessStartInfo("ipconfig", "/flushdns")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                using var process = Process.Start(psi);
                process?.WaitForExit(5000);
                return process?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Trims the working set of accessible running processes, prompting Windows to
        /// hand idle physical memory back to the system. Processes we don't have access to
        /// (most system/protected processes) are silently skipped rather than treated as errors.
        /// </summary>
        public static int FreeUpMemory()
        {
            int trimmed = 0;
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (EmptyWorkingSet(process.Handle)) trimmed++;
                }
                catch
                {
                    // No access to this process — expected for most system processes.
                }
                finally
                {
                    process.Dispose();
                }
            }
            return trimmed;
        }

        public static bool OpenRegistryEditor() => TryLaunch("regedit.exe");

        public static bool OpenSystemProtection() => TryLaunch("SystemPropertiesProtection.exe");

        public static bool OpenDiskCleanup() => TryLaunch("cleanmgr.exe");

        public static bool OpenTaskManager() => TryLaunch("taskmgr.exe");

        private static bool TryLaunch(string fileName)
        {
            try
            {
                Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
