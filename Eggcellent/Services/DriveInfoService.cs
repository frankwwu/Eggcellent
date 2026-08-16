using Eggcellent.Models;
using System.IO;

namespace Eggcellent.Services
{
    public static class DriveInfoService
    {
        public static List<DriveUsage> GetFixedDrives()
        {
            var result = new List<DriveUsage>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed || !drive.IsReady) continue;
                try
                {
                    result.Add(new DriveUsage(drive.Name.TrimEnd('\\'), drive.TotalSize, drive.AvailableFreeSpace));
                }
                catch { }
            }
            return result;
        }
    }
}
