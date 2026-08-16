using Eggcellent.Common;

namespace Eggcellent.Models
{
    public class DriveUsage
    {
        public string Name { get; }
        public long TotalBytes { get; }
        public long FreeBytes { get; }
        public long UsedBytes => TotalBytes - FreeBytes;
        public double UsedPercent => TotalBytes == 0 ? 0 : Math.Round((double)UsedBytes / TotalBytes * 100, 1);
        public string TotalDisplay => ByteFormatter.Format(TotalBytes);
        public string FreeDisplay => ByteFormatter.Format(FreeBytes);
        public string UsedDisplay => ByteFormatter.Format(UsedBytes);

        public DriveUsage(string name, long totalBytes, long freeBytes)
        {
            Name = name;
            TotalBytes = totalBytes;
            FreeBytes = freeBytes;
        }
    }
}
