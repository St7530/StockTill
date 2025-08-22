using AutoUpdaterDotNET;
using iNKORE.UI.WPF.Modern;
using StockTill.Controls;
using StockTill.Helpers;
using System.Reflection;
using System.Windows;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace StockTill.Pages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
        
        private void ThemeCombo_Loaded(object sender, RoutedEventArgs e)
        {
            switch (ThemeHelper.Theme)
            {
                case ApplicationTheme.Light:
                    ThemeCombo.SelectedIndex = 0;
                    break;
                case ApplicationTheme.Dark:
                    ThemeCombo.SelectedIndex = 1;
                    break;
                default:
                    ThemeCombo.SelectedIndex = 2;
                    break;
            }
        }

        private void ThemeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (ThemeCombo.SelectedIndex)
            {
                case 0:
                    ThemeHelper.SetTheme(ApplicationTheme.Light);
                    break;
                case 1:
                    ThemeHelper.SetTheme(ApplicationTheme.Dark);
                    break;
                default:
                    ThemeHelper.SetTheme(null);
                    break;
            }
        }

        private async void ConnectionStringButton_Click(object sender, RoutedEventArgs e)
        {
            await new ConnectionStringDialog().ShowAsync();
        }

        private void AutoUpdateSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            AutoUpdateSwitch.IsOn = bool.Parse(ConfigHelper.Configuration["Update:IsAutoCheckForUpdate"]) == true;
        }
        private void AutoUpdateSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ConfigHelper.EditConfig("Update:IsAutoCheckForUpdate", AutoUpdateSwitch.IsOn);
        }

        private async void UpdateURLButton_Click(object sender, RoutedEventArgs e)
        {
            await new UpdateURLDialog().ShowAsync();
        }

        private void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHelper.CheckForUpdate();
        }

        private void DeveloperSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Window.GetWindow(DeveloperSwitch);
            mainWindow.SetDeveloperPageVisibility(DeveloperSwitch.IsOn ? Visibility.Visible : Visibility.Hidden);
        }

        private void VersionBlock_Loaded(object sender, RoutedEventArgs e)
        {
            VersionBlock.Text = $"{Assembly.GetExecutingAssembly().GetName().Version}";
        }
    }
}
