using Eggcellent.Common;

namespace Eggcellent.Models
{
    public enum StartupHive
    {
        CurrentUser,
        LocalMachine
    }

    public class StartupItem : ViewModelBase
    {
        public string Name { get; }
        public string Command { get; }
        public StartupHive Hive { get; }
        public string HiveDisplay => Hive == StartupHive.CurrentUser ? "This user" : "All users";

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetProperty(ref _isEnabled, value))
                    OnPropertyChanged(nameof(ToggleLabel));
            }
        }

        public string ToggleLabel => IsEnabled ? "Disable" : "Enable";

        public StartupItem(string name, string command, StartupHive hive, bool isEnabled)
        {
            Name = name;
            Command = command;
            Hive = hive;
            _isEnabled = isEnabled;
        }
    }
}
