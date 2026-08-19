using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;
using Microsoft.Win32;

namespace Eggcellent.ViewModels
{
    public class DiskAnalyzerViewModel : ViewModelBase
    {
        public ObservableCollection<FolderSizeItem> Items { get; } = new();

        private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public string CurrentPath
        {
            get => _currentPath;
            set => SetProperty(ref _currentPath, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { SetProperty(ref _isBusy, value); (ScanCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _status = "Choose a folder to see what's taking up space.";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand BrowseCommand { get; }
        public RelayCommand ScanCommand { get; }
        public RelayCommand UpCommand { get; }
        public RelayCommand DrillDownCommand { get; }
        public RelayCommand OpenInExplorerCommand { get; }

        public DiskAnalyzerViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);
            ScanCommand = new RelayCommand(async () => await ScanAsync(), () => !IsBusy);
            UpCommand = new RelayCommand(async () => await GoUpAsync());
            DrillDownCommand = new RelayCommand(async p => await DrillDownAsync(p as FolderSizeItem));
            OpenInExplorerCommand = new RelayCommand(p => OpenInExplorer(p as FolderSizeItem));
        }

        private void Browse(object? parameter)
        {
            var dialog = new OpenFolderDialog { Title = "Choose a folder to analyze", InitialDirectory = CurrentPath };
            if (dialog.ShowDialog() == true)
            {
                CurrentPath = dialog.FolderName;
                _ = ScanAsync();
            }
        }

        private async Task ScanAsync()
        {
            IsBusy = true;
            Status = "Calculating folder sizes...";
            Items.Clear();

            var results = await Task.Run(() => DiskAnalyzerService.AnalyzeFolder(CurrentPath));
            foreach (var item in results) Items.Add(item);

            Status = Items.Count > 0 ? $"{Items.Count} item(s) in this folder." : "This folder has no subfolders or files.";
            IsBusy = false;
        }

        private async Task GoUpAsync()
        {
            var parent = Directory.GetParent(CurrentPath);
            if (parent is null) return;
            CurrentPath = parent.FullName;
            await ScanAsync();
        }

        private async Task DrillDownAsync(FolderSizeItem? item)
        {
            if (item is null || !item.IsFolder) return;
            CurrentPath = item.FullPath;
            await ScanAsync();
        }

        private void OpenInExplorer(FolderSizeItem? item)
        {
            var path = item?.FullPath ?? CurrentPath;
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
            }
            catch { }
        }
    }
}
