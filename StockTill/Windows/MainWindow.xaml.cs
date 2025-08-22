using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Navigation;
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
        public MainWindow(bool isInitSuccess = true)
        {
            InitializeComponent();

            NavigateTo(isInitSuccess ? NavigationViewItem_Till : NavigationViewItem_Settings);
        }
        public void NavigateTo(NavigationViewItem item)
        {
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
                NavigationView_Root.SelectedItem = item;
                NavigationView_Root.Header = page.Title;
                RootFrame.Navigate(page);
            }
        }
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigateTo((NavigationViewItem)sender.SelectedItem);
        }
        public void SetDeveloperPageVisibility(Visibility visibility)
        {
            NavigationViewItem_Developer.Visibility = visibility;
        }
    }
}