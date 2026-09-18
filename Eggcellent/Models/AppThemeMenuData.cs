using Eggcellent.Properties;
using ControlzEx.Theming;
using System.Windows;

namespace Eggcellent.Models
{
    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void ChangeTheme(object? sender)
        {
            ThemeManager.Current.ChangeThemeBaseColor(Application.Current, Name);
            Settings.Default.Theme = Name;
            Settings.Default.Save();
        }
    }
}
