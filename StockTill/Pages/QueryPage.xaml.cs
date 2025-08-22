using PropertyChanged;
using System.Data;
using System.Windows;
using StockTill.Helpers;
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
        public string CountText { get; set; } = string.Empty;

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
            CountText = string.Empty;
            DataTable data = new DataTable();

            data.Columns.Add("商品编号");
            data.Columns.Add("商品名称");
            data.Columns.Add("操作", typeof(string));
            data.Columns.Add("数量", typeof(int));
            data.Columns.Add("单件金额", typeof(decimal));
            data.Columns.Add("小计", typeof(decimal), "数量 * 单件金额");
            data.Columns.Add("操作时间");

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

            DataTable data = SqlHelper.SelectLog(isTill, startDate, endDate);
            data.Columns["id"].ColumnName = "商品编号";
            data.Columns["name"].ColumnName = "商品名称";
            data.Columns["operation"].ColumnName = "操作";
            data.Columns["quantity"].ColumnName = "数量";
            data.Columns["amount"].ColumnName = "单件金额";
            data.Columns.Add("小计", typeof(decimal), "数量 * 单件金额").SetOrdinal(5);
            data.Columns["time"].ColumnName = "操作时间";

            CountText = $"共 {data.Rows.Count} 条结果";
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
            }
            catch (Exception)
            { // 找不到结果
                CountText = "找不到结果";
            }

        }
    }
}
