using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;
using Microsoft.Win32;

namespace Eggcellent.ViewModels
{
    public class LargeFilesViewModel : ViewModelBase
    {
        public ObservableCollection<FileItem> Results { get; } = new();

        private string _selectedFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public string SelectedFolder
        {
            get => _selectedFolder;
            set => SetProperty(ref _selectedFolder, value);
        }

        private double _minSizeMb = 100;
        public double MinSizeMb
        {
            get => _minSizeMb;
            set => SetProperty(ref _minSizeMb, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
                (ScanCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _status = "Choose a folder and scan for large files.";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand BrowseCommand { get; }
        public RelayCommand ScanCommand { get; }
        public RelayCommand DeleteSelectedCommand { get; }

        public LargeFilesViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);
            ScanCommand = new RelayCommand(async () => await ScanAsync(), () => !IsBusy);
            DeleteSelectedCommand = new RelayCommand(DeleteSelected, _ => !IsBusy && Results.Any(f => f.IsSelected));
        }

        private void Browse(object? parameter)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Choose a folder to scan",
                InitialDirectory = SelectedFolder
            };

            if (dialog.ShowDialog() == true)
                SelectedFolder = dialog.FolderName;
        }

        private async Task ScanAsync()
        {
            IsBusy = true;
            Status = "Scanning...";
            Results.Clear();

            long minBytes = (long)(MinSizeMb * 1024 * 1024);
            var found = await Task.Run(() => LargeFileService.Scan(SelectedFolder, minBytes, 200));

            foreach (var item in found)
            {
                item.PropertyChanged += (_, __) => (DeleteSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                Results.Add(item);
            }

            Status = Results.Count > 0
                ? $"Found {Results.Count} file(s) over {MinSizeMb:0} MB."
                : "No files found over that size.";
            IsBusy = false;
        }

        private void DeleteSelected(object? parameter)
        {
            var selected = Results.Where(f => f.IsSelected).ToList();
            if (selected.Count == 0) return;

            var result = MessageBox.Show(
                $"Permanently delete {selected.Count} file(s)?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            int deleted = 0, errors = 0;
            foreach (var file in selected)
            {
                try
                {
                    File.Delete(file.FullPath);
                    Results.Remove(file);
                    deleted++;
                }
                catch
                {
                    errors++;
                }
            }

            Status = $"Deleted {deleted} file(s)." + (errors > 0 ? $" {errors} could not be deleted." : "");
        }
    }
}
