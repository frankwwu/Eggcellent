using System.Windows;
using ControlzEx.Theming;
using Eggcellent.Properties;

namespace Eggcellent
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Theme must be applied through ThemeManager's own API (not a static merged
            // ResourceDictionary) so it can track the currently-applied theme and swap it
            // later when the user picks a different accent/base color from the menu.
            //
            // This has to happen in OnStartup, not the App() constructor: Main() runs
            // `new App(); app.InitializeComponent(); app.Run();`, so anything in the
            // constructor executes before App.xaml's own resources (Controls.xaml,
            // Fonts.xaml) are merged in. ThemeManager needs those merged first, or
            // ChangeTheme throws ArgumentNullException trying to resolve the theme.
            // OnStartup runs after InitializeComponent, so it's safe here.
            var baseColor = string.IsNullOrWhiteSpace(Settings.Default.Theme) ? "Light" : Settings.Default.Theme;
            var accentColor = string.IsNullOrWhiteSpace(Settings.Default.Accent) ? "Blue" : Settings.Default.Accent;

            ThemeManager.Current.ChangeTheme(this, $"{baseColor}.{accentColor}");

            base.OnStartup(e);
        }
    }
}
