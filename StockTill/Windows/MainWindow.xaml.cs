using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace StockTill
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Pages.TillPage Page_Till = new Pages.TillPage();
        public Pages.StockPage Page_Stock = new Pages.StockPage();
        public Pages.QueryPage Page_Query = new Pages.QueryPage();
        public Pages.DeveloperPage Page_Developer = new Pages.DeveloperPage();
        public Pages.SettingsPage Page_Settings = new Pages.SettingsPage();
        //public Pages.WelcomePage Page_Welcome = new Pages.WelcomePage();
        public MainWindow()
        {
            InitializeComponent();

            NavigationView_Root.SelectedItem = NavigationViewItem_Till;
            NavigationView_Root.Header = Page_Till.Title;
            Frame_Main.Navigate(Page_Till);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var item = sender.SelectedItem;
            Page? page = item switch
            {
                var i when i == NavigationViewItem_Till => Page_Till,
                var i when i == NavigationViewItem_Stock => Page_Stock,
                var i when i == NavigationViewItem_Query => Page_Query,
                var i when i == NavigationViewItem_Developer => Page_Developer,
                var i when i == NavigationViewItem_Settings => Page_Settings,
                _ => null
            };

            if (page != null)
            {
                NavigationView_Root.Header = page.Title;
                Frame_Main.Navigate(page);
            }

        }
        public void SetDeveloperPageVisibility(Visibility visibility)
        {
            NavigationViewItem_Developer.Visibility = visibility;
        }
    }
}