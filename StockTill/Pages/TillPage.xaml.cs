using PropertyChanged;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StockTill.Helpers;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace StockTill.Pages
{
    /// <summary>
    /// TillPage.xaml 的交互逻辑
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class TillPage : Page
    {
        DataTable data = new DataTable();
        public decimal TotalPrice { get; set; } = 0;
        public string CountText { get; set; } = string.Empty;

        public TillPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void InitData()
        {
            TotalPrice = 0;
            CountText = string.Empty;
            data.Clear();
            data.Columns.Clear();

            data.Columns.Add("商品编号");
            data.Columns.Add("商品名称");
            data.Columns.Add("数量", typeof(int));
            data.Columns.Add("单价", typeof(decimal));
            data.Columns.Add("小计", typeof(decimal), "数量 * 单价");

            TillGrid.ItemsSource = data.DefaultView;

            // 只有“数量”列可以编辑，其他都只读
            foreach (DataGridColumn column in TillGrid.Columns)
            {
                if (column.Header?.ToString() != "数量")
                {
                    column.IsReadOnly = true;
                }
            }
        }

        private bool UpdateTill(string id, int quantity)
        {
            DataRow[] foundRows = data.Select($"商品编号 = '{id}'");

            if (foundRows.Length > 0)
            { // 商品已经存在，需要对其进行数量自增
                DataRow foundRow = foundRows[0];
                DataRow row = SqlHelper.SelectById(id);
                if ((int)row["quantity"] >= (int)foundRow["数量"] + quantity)
                { // 商品库存数量足够
                    foundRow["数量"] = (int)foundRow["数量"] + quantity;
                }
                else
                { // 商品库存数量不足
                    MessageBox.Show($"商品编号：{id}\n商品名称：{row["name"]}\n当前库存数量：{row["quantity"]}", "商品库存不足", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return false;
                }
            }
            else
            { // 商品尚未存在，需要创建新列
                if (SqlHelper.SelectById(id) != null)
                { // 库存中有该商品
                    DataRow? row = SqlHelper.SelectById(id);

                    if (row != null)
                    {
                        if ((int)row["quantity"] > 0)
                        { // 商品库存数量足够
                            DataRow newRow = data.NewRow();
                            newRow["商品编号"] = row["id"];
                            newRow["商品名称"] = row["name"];
                            newRow["数量"] = quantity;
                            newRow["单价"] = row["price"];
                            data.Rows.Add(newRow);
                        }
                        else
                        { // 商品库存数量不足
                            MessageBox.Show($"商品编号：{id}\n商品名称：{row["name"]}\n当前库存数量：{row["quantity"]}", "商品库存不足", MessageBoxButton.OK, MessageBoxImage.Hand);
                            return false;
                        }
                    }
                }
                else
                { // 库存中没有该商品
                    MessageBox.Show($"库存中没有商品编号为 {id} 的商品。", "找不到商品", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return false;
                }
            }

            TotalPrice = Convert.ToDecimal(data.Compute("SUM(小计)", ""));
            CountText = $"共 {data.Compute("SUM(数量)", "")} 件商品";
            return true;
        }

        private void IdBox_Loaded(object sender, RoutedEventArgs e)
        {
            IdBox.Focus(); // 自动获得焦点
        }

        private void IdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && IdBox.Text != "")
            {
                UpdateTill(IdBox.Text, 1);
                IdBox.Text = "";
            }
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        private void TillGrid_Loaded(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        private void TillGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is DataRowView rowView)
            {
                string id = (string)rowView["商品编号"];
                int oldQuantity = (int)rowView["数量"];
                var textBox = e.EditingElement as TextBox;
                string input = textBox?.Text.Trim();
                if (string.IsNullOrWhiteSpace(input))
                { // 数量为空
                    MessageBox.Show("请输入正整数。", "数量无效", MessageBoxButton.OK, MessageBoxImage.Hand);
                    e.Cancel = true;
                }
                else if (int.TryParse(input, out int newQuantity))
                { // 数量为整数
                    if (!UpdateTill(id, newQuantity-oldQuantity))
                    { // 更新失败
                        e.Cancel = true;
                    }
                }
            }
        }

        private void TillButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRow row in data.Rows)
            {
                string id = row["商品编号"].ToString();
                int quantity = (int)row["数量"];
                SqlHelper.InsertLog(id, true, quantity);
                SqlHelper.ReduceQuantityById(id, quantity);
            }
            data.Clear();
            TotalPrice = 0;
        }
    }
}
