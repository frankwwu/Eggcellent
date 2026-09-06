using Eggcellent.Common;
using Eggcellent.Services;

namespace Eggcellent.ViewModels
{
    public class ToolboxViewModel : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _status = "";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand FlushDnsCommand { get; }
        public RelayCommand FreeMemoryCommand { get; }
        public RelayCommand EmptyRecycleBinCommand { get; }
        public RelayCommand OpenSystemProtectionCommand { get; }
        public RelayCommand OpenDiskCleanupCommand { get; }
        public RelayCommand OpenTaskManagerCommand { get; }

        public ToolboxViewModel()
        {
            FlushDnsCommand = new RelayCommand(async () => await FlushDnsAsync());
            FreeMemoryCommand = new RelayCommand(async () => await FreeMemoryAsync());
            EmptyRecycleBinCommand = new RelayCommand(EmptyRecycleBin);
            OpenSystemProtectionCommand = new RelayCommand(() =>
                Status = ToolboxService.OpenSystemProtection() ? "Opened System Protection settings." : "Could not open System Protection.");
            OpenDiskCleanupCommand = new RelayCommand(() =>
                Status = ToolboxService.OpenDiskCleanup() ? "Opened Disk Cleanup." : "Could not open Disk Cleanup.");
            OpenTaskManagerCommand = new RelayCommand(() =>
                Status = ToolboxService.OpenTaskManager() ? "Opened Task Manager." : "Could not open Task Manager.");
        }

        private async Task FlushDnsAsync()
        {
            IsBusy = true;
            Status = "Flushing DNS cache...";
            bool ok = await Task.Run(ToolboxService.FlushDns);
            Status = ok ? "DNS cache flushed." : "Could not flush the DNS cache.";
            IsBusy = false;
        }

        private async Task FreeMemoryAsync()
        {
            IsBusy = true;
            Status = "Freeing up memory...";
            int count = await Task.Run(ToolboxService.FreeUpMemory);
            Status = $"Trimmed memory for {count} process(es).";
            IsBusy = false;
        }

        private void EmptyRecycleBin(object? parameter)
        {
            Status = RecycleBinService.Empty() ? "Recycle Bin emptied." : "Could not empty the Recycle Bin.";
        }
    }
}
