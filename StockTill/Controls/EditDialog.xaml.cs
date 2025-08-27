using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using StockTill.Helpers;
using System.Data;
using System.Windows;

namespace StockTill.Controls
{
    [AddINotifyPropertyChangedInterface]
    public partial class EditDialog
    {
        private string id;
        public string Id
        {
            get => id;
            set
            {
                id = value;
                IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(value);
            }
        }
        public string name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        private int formerQuantity;
        public int quantity { get; set; }
        public decimal cost { get; set; }
        public decimal price { get; set; }
        public DataTable Categories { get; set; } = SqlHelper.SelectAllCategories();
        public EditDialog(string id, string name, int category_id, int quantity, decimal cost, decimal price)
        {
            InitializeComponent();
            DataContext = this;

            this.Id = id;
            this.name = name;
            this.CategoryId = category_id;
            this.formerQuantity = quantity;
            this.quantity = quantity;
            this.cost = cost;
            this.price = price;
            BarcodeImage.Source = BarcodeHelper.GenerateBarcodeSource(Id);
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogProgress.Visibility = Visibility.Visible;
            SqlHelper.UpdateById(Id, name, CategoryId, quantity, cost, price);
            if (quantity > formerQuantity)
            {
                SqlHelper.InsertLog(id, false, quantity - formerQuantity);
            }
            else if (quantity < formerQuantity)
            {
                SqlHelper.InsertLog(id, true, formerQuantity - quantity);
            }
        }

        private void SaveBarcodeButton_Click(object sender, RoutedEventArgs e)
        {
            BarcodeHelper.SaveBarcode(Id, name);
        }
    }
}