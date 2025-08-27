using iNKORE.UI.WPF.Modern.Controls;
using PropertyChanged;
using System.Windows;
using StockTill.Helpers;

namespace StockTill.Controls
{
    [AddINotifyPropertyChangedInterface]
    public partial class EditCategoryDialog
    {
        public int category_id { get; set; }
        public string category { get; set; } = string.Empty;
        public EditCategoryDialog(int category_id, string category)
        {
            InitializeComponent();
            DataContext = this;

            this.category_id = category_id;
            this.category = category;
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogProgress.Visibility = Visibility.Visible;

            SqlHelper.UpdateCategory(category_id,category);
        }
    }
}