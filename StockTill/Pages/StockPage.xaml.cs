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
using Page = iNKORE.UI.WPF.Modern.Controls.Page;
using StockTill.Controls;
using StockTill.Helpers;
using System.Data;

namespace StockTill.Pages
{
    /// <summary>
    /// StockPage.xaml 的交互逻辑
    /// </summary>
    public partial class StockPage : Page
    {
        private DataView _dataView; // 保存 DataView，用于设置 Filter
        public StockPage()
        {
            InitializeComponent();
        }
        private void LoadData()
        {
            DataTable data = SqlHelper.Instance.SelectAll();
            data.Columns["id"].ColumnName = "商品编号";
            data.Columns["name"].ColumnName = "商品名称";
            data.Columns["quantity"].ColumnName = "库存数量";
            data.Columns["cost"].ColumnName = "单件成本";
            data.Columns["price"].ColumnName = "单件售价";

            // 保存 DataView
            _dataView = data.DefaultView;

            // 绑定到 StockGrid
            StockGrid.ItemsSource = _dataView;
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
            int quantity = (int)row["库存数量"];
            decimal cost = (decimal)row["单件成本"];
            decimal price = (decimal)row["单件售价"];

            EditDialog dialog = new EditDialog(id, name, quantity, cost, price);
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

                SqlHelper.Instance.DeleteById(id);
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
            if (_dataView == null) return;

            string keyword = SearchBox.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            { // 清空筛选
                _dataView.RowFilter = "";
            }
            else
            { // 模糊搜索
                keyword = keyword.Replace("'", "''"); // 转义单引号（防止 SQL 注入式错误）
                _dataView.RowFilter = $@"
            商品编号 LIKE '%{keyword}%' OR
            商品名称 LIKE '%{keyword}%'";
            }
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
    }
}
