using Eggcellent.Common;
using Eggcellent.Models;
using Eggcellent.Properties;
using ControlzEx.Theming;
using MahApps.Metro.IconPacks;
using System.Collections.ObjectModel;
using System.Windows;

namespace Eggcellent.ViewModels
{
    public class NavItem : ViewModelBase
    {
        public string Name { get; }
        public PackIconMaterialKind Icon { get; }
        public ViewModelBase ViewModel { get; }

        public NavItem(string name, PackIconMaterialKind icon, ViewModelBase viewModel)
        {
            Name = name;
            Icon = icon;
            ViewModel = viewModel;
        }
    }

    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<NavItem> NavItems { get; }

        private NavItem _selectedNavItem = null!;
        public NavItem SelectedNavItem
        {
            get => _selectedNavItem;
            set
            {
                if (SetProperty(ref _selectedNavItem, value))
                    OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ViewModelBase CurrentViewModel => SelectedNavItem.ViewModel;

        public MainViewModel()
        {
            var dashboard = new DashboardViewModel();
            var cleaner = new CleanerViewModel();
            var diskAnalyzer = new DiskAnalyzerViewModel();
            var largeFiles = new LargeFilesViewModel();
            var duplicates = new DuplicatesViewModel();
            var uninstaller = new UninstallerViewModel();
            var DeepDelete = new DeepDeleteViewModel();
            var startup = new StartupViewModel();
            var memoryMap = new MemoryMapViewModel();
            var toolbox = new ToolboxViewModel();
            var registry = new RegistryInfoViewModel();

            NavItems = new ObservableCollection<NavItem>
            {
                new NavItem("Dashboard", PackIconMaterialKind.ViewDashboard, dashboard),
                new NavItem("Smart Clean", PackIconMaterialKind.Broom, cleaner),
                new NavItem("Disk Analyzer", PackIconMaterialKind.ChartDonut, diskAnalyzer),
                new NavItem("Large Files", PackIconMaterialKind.FileChart, largeFiles),
                new NavItem("Duplicates", PackIconMaterialKind.ContentDuplicate, duplicates),
                new NavItem("Uninstaller", PackIconMaterialKind.Delete, uninstaller),
                new NavItem("Deep Delete", PackIconMaterialKind.ShieldOutline, DeepDelete),
                new NavItem("Startup", PackIconMaterialKind.Rocket, startup),
                new NavItem("Memory Map", PackIconMaterialKind.Memory, memoryMap),
                new NavItem("Toolbox", PackIconMaterialKind.Toolbox, toolbox),
                new NavItem("Registry", PackIconMaterialKind.Key, registry),
            };

            dashboard.CleanRequested += (_, __) =>
                SelectedNavItem = NavItems.First(n => n.ViewModel == cleaner);

            SelectedNavItem = NavItems[0];
        }

        private List<AccentColorMenuData>? _accentColors;

        public List<AccentColorMenuData> AccentColors
        {
            get
            {
                if (_accentColors == null)
                {
                    _accentColors = ThemeManager.Current.Themes
                                            .GroupBy(x => x.ColorScheme)
                                            .OrderBy(a => a.Key)
                                            .Select(a => new AccentColorMenuData { Name = a.Key, ColorBrush = a.First().ShowcaseBrush })
                                            .ToList();
                }
                return _accentColors;
            }
        }

        public WindowState WindowState
        {
            get { return (WindowState)Enum.Parse(typeof(WindowState), Settings.Default.WindowState); }
            set { Settings.Default.WindowState = value.ToString(); }
        }

        public double Left
        {
            get { return Settings.Default.Left; }
            set { Settings.Default.Left = value; }
        }

        public double Top
        {
            get { return Settings.Default.Top; }
            set { Settings.Default.Top = value; }
        }

        public double Width
        {
            get { return Settings.Default.Width; }
            set { Settings.Default.Width = value; }
        }

        public double Height
        {
            get { return Settings.Default.Height; }
            set { Settings.Default.Height = value; }
        }

        /// <summary>
        /// Persists the current window placement to disk. Called once, from MainWindow's
        /// Closing handler — not from the individual property setters above, since those
        /// fire continuously while the user drags or resizes the window and a disk write
        /// on every tick would make dragging feel laggy.
        /// </summary>
        public void SaveWindowPlacement() => Settings.Default.Save();
    }
}
