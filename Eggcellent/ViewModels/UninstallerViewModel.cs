using System.Collections.ObjectModel;
using System.Windows;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;

namespace Eggcellent.ViewModels
{
    public class UninstallerViewModel : ViewModelBase
    {
        private readonly List<InstalledAppItem> _allApps = new();
        public ObservableCollection<InstalledAppItem> Apps { get; } = new();

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilter();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { SetProperty(ref _isBusy, value); (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _status = "";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand UninstallCommand { get; }

        public UninstallerViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await LoadAsync(), () => !IsBusy);
            UninstallCommand = new RelayCommand(p => Uninstall(p as InstalledAppItem));
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;
            Status = "Reading installed programs...";

            var apps = await Task.Run(InstalledAppsService.GetInstalledApps);

            _allApps.Clear();
            _allApps.AddRange(apps);
            ApplyFilter();

            Status = $"{_allApps.Count} program(s) found.";
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            Apps.Clear();
            var query = string.IsNullOrWhiteSpace(SearchText)
                ? _allApps
                : _allApps.Where(a => a.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                       || a.Publisher.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var app in query) Apps.Add(app);
        }

        private void Uninstall(InstalledAppItem? item)
        {
            if (item is null) return;

            var result = MessageBox.Show(
                $"This will launch the uninstaller for \"{item.Name}\". Follow its prompts to finish removing it. Continue?",
                "Confirm Uninstall", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            if (!InstalledAppsService.LaunchUninstaller(item))
            {
                Status = $"Could not launch the uninstaller for {item.Name}.";
                return;
            }

            Status = $"Uninstaller launched for {item.Name}. Refresh this list once it's finished.";
        }
    }
}
