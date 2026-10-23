using Eggcellent.Common;

namespace Eggcellent.Models
{
    public class FolderSizeItem : ViewModelBase
    {
        public string Name { get; }
        public string FullPath { get; }
        public long SizeBytes { get; }
        public string SizeDisplay => ByteFormatter.Format(SizeBytes);
        public double PercentOfParent { get; set; }
        public bool IsFolder { get; }

        public FolderSizeItem(string name, string fullPath, long sizeBytes, bool isFolder)
        {
            Name = name;
            FullPath = fullPath;
            SizeBytes = sizeBytes;
            IsFolder = isFolder;
        }
    }
}
