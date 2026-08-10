using Eggcellent.Common;

namespace Eggcellent.Models
{
    public class CleanCategory : ViewModelBase
    {
        public string Name { get; }
        public string Description { get; }
        public string[] Paths { get; }
        public bool IsRecycleBin { get; }

        private long _sizeBytes;
        public long SizeBytes
        {
            get => _sizeBytes;
            set
            {
                if (SetProperty(ref _sizeBytes, value))
                    OnPropertyChanged(nameof(SizeDisplay));
            }
        }

        public string SizeDisplay => ByteFormatter.Format(SizeBytes);

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public CleanCategory(string name, string description, string[] paths, bool isRecycleBin = false, bool defaultSelected = true)
        {
            Name = name;
            Description = description;
            Paths = paths;
            IsRecycleBin = isRecycleBin;
            _isSelected = defaultSelected;
        }
    }
}
