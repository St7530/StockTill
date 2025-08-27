using iNKORE.UI.WPF.Modern.Controls;
using PropertyChanged;
using System.Windows;
using StockTill.Helpers;

namespace StockTill.Controls
{
    [AddINotifyPropertyChangedInterface]
    public partial class AddCategoryDialog
    {
        public string category { get; set; } = string.Empty;
        public AddCategoryDialog()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogProgress.Visibility = Visibility.Visible;

            SqlHelper.InsertCategory(category);
        }
    }
}