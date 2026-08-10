using System.Collections.ObjectModel;
using System.Windows;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;

namespace Eggcellent.ViewModels
{
    public class CleanerViewModel : ViewModelBase
    {
        public ObservableCollection<CleanCategory> Categories { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { SetProperty(ref _isBusy, value); (ScanCommand as RelayCommand)?.RaiseCanExecuteChanged(); (CleanCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _summary = "Click \"Scan\" to check for junk files.";
        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        private long _totalBytes;
        public string TotalDisplay => ByteFormatter.Format(_totalBytes);

        private bool _canClean;
        public bool CanClean
        {
            get => _canClean;
            set { SetProperty(ref _canClean, value); (CleanCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public ObservableCollection<string> Log { get; } = new();

        public RelayCommand ScanCommand { get; }
        public RelayCommand CleanCommand { get; }

        public CleanerViewModel()
        {
            Categories = new ObservableCollection<CleanCategory>(CleanerService.CreateDefaultCategories());
            ScanCommand = new RelayCommand(async () => await ScanAsync(), () => !IsBusy);
            CleanCommand = new RelayCommand(async () => await CleanAsync(), () => !IsBusy && CanClean);
        }

        private async Task ScanAsync()
        {
            IsBusy = true;
            Summary = "Scanning...";
            AddLog("Starting scan...");

            await Task.Run(() =>
            {
                foreach (var category in Categories)
                {
                    long size = CleanerService.ScanCategory(category);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        category.SizeBytes = size;
                        AddLog($"{category.Name}: {ByteFormatter.Format(size)}");
                    });
                }
            });

            _totalBytes = Categories.Sum(c => c.SizeBytes);
            OnPropertyChanged(nameof(TotalDisplay));
            Summary = _totalBytes > 0 ? "Junk found. Choose what to clean below." : "No junk found. Your system looks clean.";
            CanClean = _totalBytes > 0;
            AddLog("Scan complete.");
            IsBusy = false;
        }

        private async Task CleanAsync()
        {
            var selected = Categories.Where(c => c.IsSelected && c.SizeBytes > 0).ToList();
            if (selected.Count == 0)
            {
                AddLog("Nothing selected to clean.");
                return;
            }

            var result = MessageBox.Show(
                $"This will permanently delete files in {selected.Count} categor{(selected.Count == 1 ? "y" : "ies")}. Continue?",
                "Confirm Clean", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            IsBusy = true;
            Summary = "Cleaning...";
            AddLog("Starting clean...");

            long freedTotal = 0;
            int errorTotal = 0;

            await Task.Run(() =>
            {
                foreach (var category in selected)
                {
                    var (freed, errors) = CleanerService.CleanCategory(category);
                    freedTotal += freed;
                    errorTotal += errors;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        category.SizeBytes = 0;
                        category.IsSelected = false;
                        AddLog($"Cleaned {category.Name}: freed {ByteFormatter.Format(freed)}" + (errors > 0 ? $" ({errors} file(s) skipped)" : ""));
                    });
                }
            });

            _totalBytes = Categories.Sum(c => c.SizeBytes);
            OnPropertyChanged(nameof(TotalDisplay));
            Summary = $"Freed {ByteFormatter.Format(freedTotal)}" + (errorTotal > 0 ? $" ({errorTotal} file(s) in use were skipped)." : ".");
            CanClean = _totalBytes > 0;
            AddLog($"Clean complete. Total freed: {ByteFormatter.Format(freedTotal)}");
            IsBusy = false;
        }

        private void AddLog(string message) => Log.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
