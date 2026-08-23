using System.Collections.ObjectModel;
using Eggcellent.Common;

namespace Eggcellent.Models
{
    public class DuplicateGroup : ViewModelBase
    {
        public string Hash { get; }
        public long SizeBytesEach { get; }
        public string SizeDisplay => ByteFormatter.Format(SizeBytesEach);
        public ObservableCollection<FileItem> Files { get; }

        public long WastedBytes => SizeBytesEach * Math.Max(0, Files.Count - 1);
        public string WastedDisplay => ByteFormatter.Format(WastedBytes);

        public DuplicateGroup(string hash, long sizeBytesEach, IEnumerable<FileItem> files)
        {
            Hash = hash;
            SizeBytesEach = sizeBytesEach;
            Files = new ObservableCollection<FileItem>(files);
        }
    }
}
