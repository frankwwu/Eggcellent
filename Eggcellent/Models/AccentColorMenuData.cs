using Eggcellent.Common;
using Eggcellent.Properties;
using ControlzEx.Theming;
using System.Windows;
using System.Windows.Media;

namespace Eggcellent.Models
{
    public class AccentColorMenuData
    {
        public string Name { get; set; } = "";

        public Brush? BorderColorBrush { get; set; }

        public Brush? ColorBrush { get; set; }

        public AccentColorMenuData()
        {
            ChangeAccentCommand = new RelayCommand(ChangeTheme);
        }

        public RelayCommand ChangeAccentCommand { get; }

        protected virtual void ChangeTheme(object? sender)
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
            Settings.Default.Accent = Name;
            Settings.Default.Save();
        }
    }
}
