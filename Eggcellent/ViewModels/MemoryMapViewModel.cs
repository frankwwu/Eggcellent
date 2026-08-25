using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;

namespace Eggcellent.ViewModels
{
    public class MemoryMapViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _autoRefreshTimer;

        public ObservableCollection<ProcessMemoryItem> Processes { get; } = new();

        private MemorySnapshot? _snapshot;
        public MemorySnapshot? Snapshot
        {
            get => _snapshot;
            set => SetProperty(ref _snapshot, value);
        }

        public bool IsElevated { get; } = MemoryMapService.IsRunningElevated();

        private bool _autoRefresh;
        public bool AutoRefresh
        {
            get => _autoRefresh;
            set
            {
                if (SetProperty(ref _autoRefresh, value))
                {
                    if (value) _autoRefreshTimer.Start();
                    else _autoRefreshTimer.Stop();
                }
            }
        }

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

        public RelayCommand RefreshCommand { get; }
        public RelayCommand EmptyStandbyListCommand { get; }
        public RelayCommand EmptyPriority0StandbyListCommand { get; }
        public RelayCommand FlushModifiedListCommand { get; }
        public RelayCommand EmptyAllWorkingSetsCommand { get; }

        public MemoryMapViewModel()
        {
            RefreshCommand = new RelayCommand(Refresh);
            EmptyStandbyListCommand = new RelayCommand(async () => await RunActionAsync(
                MemoryMapService.EmptyStandbyList, "Standby list emptied.", "Could not empty the standby list."));
            EmptyPriority0StandbyListCommand = new RelayCommand(async () => await RunActionAsync(
                MemoryMapService.EmptyPriority0StandbyList, "Priority 0 standby list emptied.", "Could not empty the priority 0 standby list."));
            FlushModifiedListCommand = new RelayCommand(async () => await RunActionAsync(
                MemoryMapService.FlushModifiedList, "Modified page list flushed to disk.", "Could not flush the modified page list."));
            EmptyAllWorkingSetsCommand = new RelayCommand(async () => await RunActionAsync(
                MemoryMapService.EmptyAllWorkingSets, "Working sets emptied for all processes.", "Could not empty working sets."));

            _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _autoRefreshTimer.Tick += (_, __) => Refresh();

            Status = IsElevated
                ? "Running with administrator rights — all actions are available."
                : "Not running as administrator — the actions below need elevation and will tell you if they fail.";

            Refresh();
        }

        private void Refresh(object? parameter = null)
        {
            Snapshot = MemoryMapService.GetSnapshot();

            Processes.Clear();
            foreach (var item in MemoryMapService.GetProcessMemoryList().Take(200))
                Processes.Add(item);
        }

        private async Task RunActionAsync(Func<bool> action, string successMessage, string failureMessage)
        {
            if (!IsElevated)
            {
                var proceed = MessageBox.Show(
                    "This action needs Eggcellent to be running as administrator, or it will simply fail. " +
                    "Close Eggcellent and reopen it with \"Run as administrator\" to use it. Try anyway?",
                    "Administrator rights needed", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (proceed != MessageBoxResult.Yes) return;
            }

            IsBusy = true;
            Status = "Working...";

            bool success = await Task.Run(action);

            Status = success ? successMessage : failureMessage;
            if (success) Refresh();
            IsBusy = false;
        }
    }
}
