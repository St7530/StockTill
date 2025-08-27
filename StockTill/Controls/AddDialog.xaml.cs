using iNKORE.UI.WPF.Modern.Controls;
using PropertyChanged;
using StockTill.Helpers;
using System.Data;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace StockTill.Controls
{
    [AddINotifyPropertyChangedInterface]
    public partial class AddDialog
    {
        private string id;
        public string Id
        {
            get => id;
            set
            {
                id = value;
                IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(value);
                SaveBarcodeButton.IsEnabled = !string.IsNullOrWhiteSpace(value);
                BarcodeImage.Source = string.IsNullOrWhiteSpace(value) ? null : BarcodeHelper.GenerateBarcodeSource(value);
            }
        }
        public string name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int quantity { get; set; }
        public decimal cost { get; set; }
        public decimal price { get; set; }
        public DataTable Categories { get; set; } = SqlHelper.SelectAllCategories();
        public AddDialog()
        {
            InitializeComponent();
            DataContext = this;

            IsPrimaryButtonEnabled = false;
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral(); // 阻止默认的关闭行为

            DialogProgress.Visibility = Visibility.Visible;

            if (SqlHelper.SelectById(id) != null)
            {
                MessageBox.Show("商品编号重复。", "新增失败", MessageBoxButton.OK, MessageBoxImage.Hand);
                args.Cancel = true; // 阻止关闭
            }
            else
            {
                SqlHelper.Insert(Id, name, CategoryId, quantity, cost, price);
                if (quantity > 0)
                {
                    SqlHelper.InsertLog(id, false, quantity);
                }
                args.Cancel = false; // 允许关闭
            }

            DialogProgress.Visibility = Visibility.Collapsed;
            deferral.Complete();
        }

        private void SaveBarcodeButton_Click(object sender, RoutedEventArgs e)
        {
            BarcodeHelper.SaveBarcode(Id, name);
        }
    }
}