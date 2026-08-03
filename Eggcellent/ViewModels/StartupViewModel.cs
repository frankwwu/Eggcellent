using System.Collections.ObjectModel;
using System.Windows;
using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Services;

namespace Eggcellent.ViewModels
{
    public class StartupViewModel : ViewModelBase
    {
        public ObservableCollection<StartupItem> Items { get; } = new();

        private string _status = "";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ToggleCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public StartupViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Load());
            ToggleCommand = new RelayCommand(p => Toggle(p as StartupItem));
            DeleteCommand = new RelayCommand(p => Delete(p as StartupItem));
            Load();
        }

        private void Load()
        {
            Items.Clear();
            foreach (var item in StartupService.GetItems())
                Items.Add(item);

            Status = $"{Items.Count} startup item(s) found.";
        }

        private void Toggle(StartupItem? item)
        {
            if (item is null) return;

            if (item.Hive == StartupHive.LocalMachine)
            {
                MessageBox.Show(
                    "This entry applies to all users and requires running Eggcellent as administrator to change.",
                    "Admin rights required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool newState = !item.IsEnabled;
            if (StartupService.SetEnabled(item, newState))
            {
                item.IsEnabled = newState;
                Status = $"{item.Name} {(newState ? "enabled" : "disabled")}.";
            }
            else
            {
                Status = $"Could not update {item.Name}.";
            }
        }

        private void Delete(StartupItem? item)
        {
            if (item is null) return;

            if (item.Hive == StartupHive.LocalMachine)
            {
                MessageBox.Show(
                    "This entry applies to all users and requires running Eggcellent as administrator to remove.",
                    "Admin rights required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Remove \"{item.Name}\" from startup permanently?", "Confirm Remove",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            if (StartupService.Delete(item))
            {
                Items.Remove(item);
                Status = $"Removed {item.Name}.";
            }
            else
            {
                Status = $"Could not remove {item.Name}.";
            }
        }
    }
}
