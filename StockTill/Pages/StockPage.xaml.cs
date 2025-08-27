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
    /// StockPage.xaml 的交互逻辑
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class StockPage : Page
    {
        private DataView _dataView; // 保存 DataView，用于设置 Filter
        private string category = "[全部]";
        private string search = string.Empty;
        public string CountText { get; set; } = string.Empty;

        public StockPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadData()
        {
            DataTable data = SqlHelper.SelectAll();
            data.Columns["id"].ColumnName = "商品编号";
            data.Columns["name"].ColumnName = "商品名称";
            data.Columns["category"].ColumnName = "商品分类";
            data.Columns["quantity"].ColumnName = "库存数量";
            data.Columns["cost"].ColumnName = "单件成本（元）";
            data.Columns["price"].ColumnName = "单件售价（元）";

            // 保存 DataView
            _dataView = data.DefaultView;
            StockGrid.ItemsSource = _dataView;

            DataTable categories = SqlHelper.SelectAllCategories();
            DataRow newRow = categories.NewRow();
            newRow["category_id"] = "0";
            newRow["category"] = "[全部]";
            categories.Rows.InsertAt(newRow, 0); // 插到最前面
            CategoryBox.ItemsSource = categories.DefaultView;
            CategoryBox.SelectedIndex = 0; // 默认选择 "[全部]"
        }
        private void FilterData()
        {
            if (category == "[全部]" && string.IsNullOrEmpty(search))
            { // 清空筛选
                _dataView.RowFilter = "";
            }
            else
            { // 模糊搜索
                search = search.Replace("'", "''"); // 转义单引号
                if (category == "[全部]")
                { // 无视商品分类
                    _dataView.RowFilter = $@"
                    商品编号 LIKE '%{search}%' OR
                    商品名称 LIKE '%{search}%'
                    ";
                }
                else
                {
                    _dataView.RowFilter = $@"
                    商品分类 = '{category}' AND (
                        商品编号 LIKE '%{search}%' OR
                        商品名称 LIKE '%{search}%'
                    )";
                }
            }

            CountText = (_dataView.Count > 0) ? $"共 {_dataView.Count} 个商品" : "找不到商品";
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddDialog dialog = new AddDialog();
            if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
            {
                LoadData();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var row = ((DataRowView)StockGrid.SelectedItems[0]).Row;
            string id = (string)row["商品编号"];
            string name = (string)row["商品名称"];
            int category_id = (int)SqlHelper.SelectCategoryIdByCategory((string)row["商品分类"]);
            int quantity = (int)row["库存数量"];
            decimal cost = (decimal)row["单件成本（元）"];
            decimal price = (decimal)row["单件售价（元）"];

            EditDialog dialog = new EditDialog(id, name, category_id, quantity, cost, price);
            if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
            {
                LoadData();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView rowView in StockGrid.SelectedItems)
            {
                var row = rowView.Row;
                string id = (string)row["商品编号"];

                SqlHelper.DeleteById(id);
            }

            DeleteFlyout.Hide();
            LoadData();
        }

        private void SearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus(); // 自动获取焦点
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            search = SearchBox.Text;
            FilterData();
        }

        private void StockGrid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void StockGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditButton.IsEnabled = StockGrid.SelectedItems.Count == 1;
            DeleteButton.IsEnabled = StockGrid.SelectedItems.Count > 0;
        }

        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            category = (string)CategoryBox.SelectedValue;
            FilterData();
        }

        private void CategoryBox_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
