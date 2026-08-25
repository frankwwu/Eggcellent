using Eggcellent.Common;

namespace Eggcellent.Models
{
    public class MemorySnapshot
    {
        public long PhysicalTotalBytes { get; init; }
        public long PhysicalAvailableBytes { get; init; }
        public long PhysicalInUseBytes => PhysicalTotalBytes - PhysicalAvailableBytes;
        public long SystemCacheBytes { get; init; }
        public long CommitTotalBytes { get; init; }
        public long CommitLimitBytes { get; init; }
        public long KernelPagedBytes { get; init; }
        public long KernelNonpagedBytes { get; init; }
        public int ProcessCount { get; init; }
        public int ThreadCount { get; init; }
        public int HandleCount { get; init; }

        public double PhysicalInUsePercent => PhysicalTotalBytes == 0 ? 0 : Math.Round((double)PhysicalInUseBytes / PhysicalTotalBytes * 100, 1);
        public double CommitPercent => CommitLimitBytes == 0 ? 0 : Math.Round((double)CommitTotalBytes / CommitLimitBytes * 100, 1);

        public string PhysicalTotalDisplay => ByteFormatter.Format(PhysicalTotalBytes);
        public string PhysicalAvailableDisplay => ByteFormatter.Format(PhysicalAvailableBytes);
        public string PhysicalInUseDisplay => ByteFormatter.Format(PhysicalInUseBytes);
        public string SystemCacheDisplay => ByteFormatter.Format(SystemCacheBytes);
        public string CommitTotalDisplay => ByteFormatter.Format(CommitTotalBytes);
        public string CommitLimitDisplay => ByteFormatter.Format(CommitLimitBytes);
        public string KernelPagedDisplay => ByteFormatter.Format(KernelPagedBytes);
        public string KernelNonpagedDisplay => ByteFormatter.Format(KernelNonpagedBytes);
    }
}
