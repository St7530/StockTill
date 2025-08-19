using PropertyChanged;
using StockTill.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace StockTill.Pages
{
    /// <summary>
    /// QueryPage.xaml 的交互逻辑
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class QueryPage : Page
    {
        public decimal Cost { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public decimal Profit { get; set; } = 0;

        public QueryPage()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void QueryGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Cost = 0;
            Price = 0;
            Profit = 0;
            DataTable data = new DataTable();

            data.Columns.Add("商品编号");
            data.Columns.Add("商品名称");
            data.Columns.Add("操作", typeof(string));
            data.Columns.Add("数量", typeof(int));
            data.Columns.Add("单件金额", typeof(decimal));
            data.Columns.Add("小计", typeof(decimal), "数量 * 单件金额");
            data.Columns.Add("操作时间");

            // 2. 绑定到 DataGrid（建议使用 ObservableCollection 替代 DataTable）
            QueryGrid.ItemsSource = data.DefaultView;
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            Cost = 0;
            Price = 0;
            DateTimeOffset startDate = StartPicker.SelectedDate?.Date ?? DateTime.Now.Date;
            DateTimeOffset endDate = EndPicker.SelectedDate?.Date ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);
            bool? isTill = null;
            switch (OperationCombo.SelectedIndex)
            {
                case 0: // 入库
                    isTill = false;
                    break;
                case 1: // 出库
                    isTill = true;
                    break;
                case 2: // 入库 & 出库
                    break;
            }

            DataTable data = SqlHelper.Instance.SelectLog(isTill, startDate, endDate);
            data.Columns["id"].ColumnName = "商品编号";
            data.Columns["name"].ColumnName = "商品名称";
            data.Columns["operation"].ColumnName = "操作";
            data.Columns["quantity"].ColumnName = "数量";
            data.Columns["amount"].ColumnName = "单件金额";
            data.Columns.Add("小计", typeof(decimal), "数量 * 单件金额").SetOrdinal(5);
            data.Columns["time"].ColumnName = "操作时间";

            QueryGrid.ItemsSource = data.DefaultView;

            // 计算总成本、总售价、总盈利
            foreach (DataRow row in data.Rows)
            {
                switch (row["操作"])
                {
                    case "入库":
                        Cost -= (int)row["数量"] * Convert.ToDecimal(row["单件金额"]);
                        break;
                    case "出库":
                        Price += (int)row["数量"] * Convert.ToDecimal(row["单件金额"]);
                        break;
                }
            }
            try
            {
                Profit = Convert.ToDecimal(data.Compute("SUM(小计)", ""));
                NotFoundBlock.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            { // 找不到结果
                NotFoundBlock.Visibility = Visibility.Visible;
            }
            
        }
    }
}
