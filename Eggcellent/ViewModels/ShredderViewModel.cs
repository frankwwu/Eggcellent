using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;
using Microsoft.Win32;

namespace Eggcellent.ViewModels
{
    public class ShredderViewModel : ViewModelBase
    {
        public ObservableCollection<FileItem> Files { get; } = new();

        private int _passes = 3;
        public int Passes
        {
            get => _passes;
            set => SetProperty(ref _passes, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
                (ShredCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _status = "Add files to permanently and securely delete them.";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand AddFilesCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand ShredCommand { get; }

        public ShredderViewModel()
        {
            AddFilesCommand = new RelayCommand(AddFiles);
            RemoveCommand = new RelayCommand(p => { if (p is FileItem f) Files.Remove(f); });
            ShredCommand = new RelayCommand(async () => await ShredAsync(), () => !IsBusy && Files.Count > 0);
        }

        private void AddFiles(object? parameter)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Choose files to shred",
                Multiselect = true,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true) return;

            foreach (var path in dialog.FileNames)
            {
                if (Files.Any(f => f.FullPath == path)) continue;
                try
                {
                    var info = new FileInfo(path);
                    Files.Add(new FileItem(path, info.Length, info.LastWriteTime));
                }
                catch { }
            }

            (ShredCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task ShredAsync()
        {
            var result = MessageBox.Show(
                $"This will permanently and securely delete {Files.Count} file(s) using {Passes} overwrite pass(es). " +
                "This cannot be undone. Continue?",
                "Confirm Shred", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            IsBusy = true;
            Status = "Shredding files...";

            var toShred = Files.ToList();
            int success = 0, failed = 0;

            await Task.Run(() =>
            {
                foreach (var file in toShred)
                {
                    if (ShredderService.Shred(file.FullPath, Passes))
                        Application.Current.Dispatcher.Invoke(() => { Files.Remove(file); success++; });
                    else
                        failed++;
                }
            });

            Status = $"Shredded {success} file(s)." + (failed > 0 ? $" {failed} could not be shredded (in use or access denied)." : "");
            (ShredCommand as RelayCommand)?.RaiseCanExecuteChanged();
            IsBusy = false;
        }
    }
}
