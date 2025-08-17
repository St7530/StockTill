using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StockTill;
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

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
			{
				Window window = Window.GetWindow(toggleSwitch);
				if (window is MainWindow mainWindow)
				{
					mainWindow.SetDeveloperPageVisibility(
						toggleSwitch.IsOn ? Visibility.Visible : Visibility.Hidden
					);
				}
			}
        }
    }
}
