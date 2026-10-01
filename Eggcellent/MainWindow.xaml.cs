using System.Windows;
using Eggcellent.ViewModels;
using MahApps.Metro.Controls;

namespace Eggcellent
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // The saved position can end up off-screen — most commonly because the window
            // was last closed on a second monitor that isn't connected anymore. If none of
            // the window is within the current virtual screen bounds, recenter it instead
            // of leaving it stranded somewhere the user can't see or reach.
            var windowRect = new Rect(Left, Top, ActualWidth > 0 ? ActualWidth : Width, ActualHeight > 0 ? ActualHeight : Height);
            var virtualScreen = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);

            if (!virtualScreen.IntersectsWith(windowRect))
            {
                Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
                Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;

            // If the window is maximized, RestoreBounds holds the size/position it would
            // return to when un-maximized — that's what we want to remember as "Width" and
            // "Height" for next launch, not the current full-screen dimensions.
            if (WindowState == WindowState.Maximized)
            {
                vm.Left = RestoreBounds.Left;
                vm.Top = RestoreBounds.Top;
                vm.Width = RestoreBounds.Width;
                vm.Height = RestoreBounds.Height;
            }
            else
            {
                vm.Left = Left;
                vm.Top = Top;
                vm.Width = Width;
                vm.Height = Height;
            }

            vm.WindowState = WindowState;
            vm.SaveWindowPlacement();
        }
    }
}
