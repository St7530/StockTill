using iNKORE.UI.WPF.Modern.Controls;
using PropertyChanged;
using StockTill.Controls;
using StockTill.Helpers;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace StockTill.Pages
{
    /// <summary>
    /// CategoriesPage.xaml 的交互逻辑
    /// </summary>
    public partial class CategoriesPage : Page
    {
        public CategoriesPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadData()
        {
            DataTable data = SqlHelper.SelectAllCategories();
            data.Columns["category_id"].ColumnName = "分类编号";
            data.Columns["category"].ColumnName = "商品分类";

            CategoriesGrid.ItemsSource = data.DefaultView;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategoryDialog dialog = new();
            if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
            {
                LoadData();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var row = ((DataRowView)CategoriesGrid.SelectedItems[0]).Row;
            int category_id = (int)row["分类编号"];
            string category = (string)row["商品分类"];

            EditCategoryDialog dialog = new(category_id, category);
            if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
            {
                LoadData();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView rowView in CategoriesGrid.SelectedItems)
            {
                var row = rowView.Row;
                int category_id = (int)row["分类编号"];

                SqlHelper.DeleteCategoryById(category_id);
            }

            DeleteFlyout.Hide();
            LoadData();
        }

        private void CategoriesGrid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void CategoriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditButton.IsEnabled = CategoriesGrid.SelectedItems.Count == 1;
            DeleteButton.IsEnabled = CategoriesGrid.SelectedItems.Count > 0;
        }
    }
}
