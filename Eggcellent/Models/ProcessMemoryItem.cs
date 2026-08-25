using Eggcellent.Common;

namespace Eggcellent.Models
{
    public class ProcessMemoryItem
    {
        public int ProcessId { get; }
        public string Name { get; }
        public long WorkingSetBytes { get; }
        public long PrivateBytes { get; }
        public string WorkingSetDisplay => ByteFormatter.Format(WorkingSetBytes);
        public string PrivateBytesDisplay => ByteFormatter.Format(PrivateBytes);

        public ProcessMemoryItem(int processId, string name, long workingSetBytes, long privateBytes)
        {
            ProcessId = processId;
            Name = name;
            WorkingSetBytes = workingSetBytes;
            PrivateBytes = privateBytes;
        }
    }
}
