using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Eggcellent.Models;

namespace Eggcellent.Services
{
    /// <summary>
    /// A simplified, live view into physical memory: a system-wide breakdown
    /// (via the public GetPerformanceInfo API), a per-process working-set list, and
    /// standby-list/working-set "purge" operations via NtSetSystemInformation. This isn't
    /// a full PFN-level memory map — that requires querying the undocumented PFN database
    /// directly, which is far more invasive than a general-purpose cleaner needs.
    /// </summary>
    public static class MemoryMapService
    {
        #region GetPerformanceInfo (public, documented API)

        [StructLayout(LayoutKind.Sequential)]
        private struct PERFORMANCE_INFORMATION
        {
            public int cb;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonpaged;
            public IntPtr PageSize;
            public int HandleCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pPerformanceInformation, int cb);

        public static MemorySnapshot? GetSnapshot()
        {
            var info = new PERFORMANCE_INFORMATION { cb = Marshal.SizeOf<PERFORMANCE_INFORMATION>() };
            if (!GetPerformanceInfo(ref info, info.cb)) return null;

            long pageSize = info.PageSize.ToInt64();

            return new MemorySnapshot
            {
                PhysicalTotalBytes = info.PhysicalTotal.ToInt64() * pageSize,
                PhysicalAvailableBytes = info.PhysicalAvailable.ToInt64() * pageSize,
                SystemCacheBytes = info.SystemCache.ToInt64() * pageSize,
                CommitTotalBytes = info.CommitTotal.ToInt64() * pageSize,
                CommitLimitBytes = info.CommitLimit.ToInt64() * pageSize,
                KernelPagedBytes = info.KernelPaged.ToInt64() * pageSize,
                KernelNonpagedBytes = info.KernelNonpaged.ToInt64() * pageSize,
                ProcessCount = info.ProcessCount,
                ThreadCount = info.ThreadCount,
                HandleCount = info.HandleCount
            };
        }

        #endregion

        #region Per-process working sets

        public static List<ProcessMemoryItem> GetProcessMemoryList()
        {
            var items = new List<ProcessMemoryItem>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    items.Add(new ProcessMemoryItem(process.Id, process.ProcessName, process.WorkingSet64, process.PrivateMemorySize64));
                }
                catch
                {
                    // Access denied on some system processes — skip rather than fail the whole list.
                }
                finally
                {
                    process.Dispose();
                }
            }

            return items.OrderByDescending(p => p.WorkingSetBytes).ToList();
        }

        #endregion

        #region Standby list / working set purge operations

        private enum SYSTEM_MEMORY_LIST_COMMAND
        {
            MemoryCaptureAccessedBits = 0,
            MemoryCaptureAndResetAccessedBits = 1,
            MemoryEmptyWorkingSets = 2,
            MemoryFlushModifiedList = 3,
            MemoryPurgeStandbyList = 4,
            MemoryPurgeLowPriorityStandbyList = 5
        }

        private const int SystemMemoryListInformation = 80;

        [DllImport("ntdll.dll")]
        private static extern int NtSetSystemInformation(int systemInformationClass, IntPtr systemInformation, int systemInformationLength);

        public static bool IsRunningElevated()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Empties the standby list — pages Windows is caching "just in case" but nothing is actively using.</summary>
        public static bool EmptyStandbyList() => RunMemoryListCommand(SYSTEM_MEMORY_LIST_COMMAND.MemoryPurgeStandbyList);

        /// <summary>Empties only the lowest-priority part of the standby list, leaving higher-priority cached pages alone.</summary>
        public static bool EmptyPriority0StandbyList() => RunMemoryListCommand(SYSTEM_MEMORY_LIST_COMMAND.MemoryPurgeLowPriorityStandbyList);

        /// <summary>Writes out the modified (dirty) page list to disk so those pages can be reused.</summary>
        public static bool FlushModifiedList() => RunMemoryListCommand(SYSTEM_MEMORY_LIST_COMMAND.MemoryFlushModifiedList);

        /// <summary>Trims every process's working set in one system call (system-wide equivalent of the Toolbox's per-process "Free Up Memory").</summary>
        public static bool EmptyAllWorkingSets() => RunMemoryListCommand(SYSTEM_MEMORY_LIST_COMMAND.MemoryEmptyWorkingSets);

        private static bool RunMemoryListCommand(SYSTEM_MEMORY_LIST_COMMAND command)
        {
            if (!EnablePrivilege("SeProfileSingleProcessPrivilege")) return false;

            var buffer = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                Marshal.WriteInt32(buffer, (int)command);
                int status = NtSetSystemInformation(SystemMemoryListInformation, buffer, sizeof(int));
                return status == 0; // STATUS_SUCCESS
            }
            catch
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion

        #region Privilege enabling (required for the memory-list commands above, needs admin)

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint SE_PRIVILEGE_ENABLED = 0x00000002;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LookupPrivilegeValue(string? systemName, string name, out LUID luid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges,
            ref TOKEN_PRIVILEGES newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        private static bool EnablePrivilege(string privilegeName)
        {
            if (!IsRunningElevated()) return false;

            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out var tokenHandle))
                return false;

            try
            {
                if (!LookupPrivilegeValue(null, privilegeName, out var luid))
                    return false;

                var privileges = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new LUID_AND_ATTRIBUTES { Luid = luid, Attributes = SE_PRIVILEGE_ENABLED }
                };

                return AdjustTokenPrivileges(tokenHandle, false, ref privileges, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                CloseHandle(tokenHandle);
            }
        }

        #endregion
    }
}
