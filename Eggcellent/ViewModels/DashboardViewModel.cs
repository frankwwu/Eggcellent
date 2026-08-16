using System.Collections.ObjectModel;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;

namespace Eggcellent.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public ObservableCollection<DriveUsage> Drives { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { SetProperty(ref _isBusy, value); (QuickScanCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _junkSummary = "Run a quick scan to see how much space you can free.";
        public string JunkSummary
        {
            get => _junkSummary;
            set => SetProperty(ref _junkSummary, value);
        }

        public event EventHandler? CleanRequested;

        public RelayCommand QuickScanCommand { get; }
        public RelayCommand GoToCleanerCommand { get; }

        public DashboardViewModel()
        {
            QuickScanCommand = new RelayCommand(async () => await QuickScanAsync(), () => !IsBusy);
            GoToCleanerCommand = new RelayCommand(() => CleanRequested?.Invoke(this, EventArgs.Empty));
            RefreshDrives();
        }

        public void RefreshDrives()
        {
            Drives.Clear();
            foreach (var drive in DriveInfoService.GetFixedDrives())
                Drives.Add(drive);
        }

        private async Task QuickScanAsync()
        {
            IsBusy = true;
            JunkSummary = "Scanning...";

            long total = 0;
            await Task.Run(() =>
            {
                foreach (var category in CleanerService.CreateDefaultCategories())
                    total += CleanerService.ScanCategory(category);
            });

            JunkSummary = total > 0
                ? $"Found {ByteFormatter.Format(total)} of junk you can clean up."
                : "No junk found. Your system looks clean.";
            IsBusy = false;
        }
    }
}
