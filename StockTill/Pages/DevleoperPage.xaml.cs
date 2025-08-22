using System.Windows;
using StockTill.Helpers;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace StockTill.Pages
{
    /// <summary>
    /// DeveloperPage.xaml 的交互逻辑
    /// </summary>
    public partial class DeveloperPage : Page
    {
        public DeveloperPage()
        {
            InitializeComponent();
        }

		private void DeleteConfigButton_Click(object sender, RoutedEventArgs e)
		{
			ConfigHelper.DeleteConfig();
			DeleteConfigFlyout.Hide();
		}
		private void DropDBButton_Click(object sender, RoutedEventArgs e)
		{
			SqlHelper.DropSchema();
			DropDBFlyout.Hide();
		}
	}
}
