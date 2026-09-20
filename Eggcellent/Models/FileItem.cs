using Eggcellent.Common;
using System.IO;

namespace Eggcellent.Models
{
    public class FileItem : ViewModelBase
    {
        public string FullPath { get; }
        public string Name { get; }
        public long SizeBytes { get; }
        public string SizeDisplay => ByteFormatter.Format(SizeBytes);
        public DateTime LastModified { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public FileItem(string fullPath, long sizeBytes, DateTime lastModified)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
            SizeBytes = sizeBytes;
            LastModified = lastModified;
        }
    }
}
