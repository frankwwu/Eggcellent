using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;
using Microsoft.Win32;

namespace Eggcellent.ViewModels
{
    public class DuplicatesViewModel : ViewModelBase
    {
        public ObservableCollection<DuplicateGroup> Groups { get; } = new();

        private string _selectedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string SelectedFolder
        {
            get => _selectedFolder;
            set => SetProperty(ref _selectedFolder, value);
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

        private string _status = "Choose a folder and scan for duplicate files.";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand BrowseCommand { get; }
        public RelayCommand ScanCommand { get; }
        public RelayCommand DeleteSelectedCommand { get; }

        public DuplicatesViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);
            ScanCommand = new RelayCommand(async () => await ScanAsync(), () => !IsBusy);
            DeleteSelectedCommand = new RelayCommand(DeleteSelected, _ => !IsBusy && Groups.Any(g => g.Files.Any(f => f.IsSelected)));
        }

        private void Browse(object? parameter)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Choose a folder to scan for duplicates",
                InitialDirectory = SelectedFolder
            };

            if (dialog.ShowDialog() == true)
                SelectedFolder = dialog.FolderName;
        }

        private async Task ScanAsync()
        {
            IsBusy = true;
            Status = "Scanning and hashing files — this can take a while for large folders...";
            Groups.Clear();

            var found = await Task.Run(() => DuplicateFileService.Scan(SelectedFolder));

            foreach (var group in found)
            {
                foreach (var file in group.Files)
                    file.PropertyChanged += (_, __) => (DeleteSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                Groups.Add(group);
            }

            long wasted = Groups.Sum(g => g.WastedBytes);
            Status = Groups.Count > 0
                ? $"Found {Groups.Count} duplicate group(s), wasting {ByteFormatter.Format(wasted)}."
                : "No duplicates found.";
            IsBusy = false;
        }

        private void DeleteSelected(object? parameter)
        {
            var selectedFiles = Groups.SelectMany(g => g.Files.Where(f => f.IsSelected)).ToList();
            if (selectedFiles.Count == 0) return;

            var result = MessageBox.Show(
                $"Permanently delete {selectedFiles.Count} duplicate file(s)?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            int deleted = 0, errors = 0;

            foreach (var group in Groups.ToList())
            {
                foreach (var file in group.Files.Where(f => f.IsSelected).ToList())
                {
                    try
                    {
                        File.Delete(file.FullPath);
                        group.Files.Remove(file);
                        deleted++;
                    }
                    catch
                    {
                        errors++;
                    }
                }

                if (group.Files.Count < 2) Groups.Remove(group);
            }

            Status = $"Deleted {deleted} file(s)." + (errors > 0 ? $" {errors} could not be deleted." : "");
        }
    }
}
