using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using StockTill.Helpers;
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
        private int formerQuantity;
        public int quantity { get; set; }
        public decimal cost { get; set; }
        public decimal price { get; set; }
        public EditDialog(string id,string name,int quantity,decimal cost,decimal price)
        {
            InitializeComponent();
            DataContext = this;

            this.Id = id;
            this.name = name;
            this.formerQuantity = quantity;
            this.quantity = quantity;
            this.cost = cost;
            this.price = price;
            BarcodeImage.Source = BarcodeHelper.GenerateBarcode(Id);
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogProgress.Visibility = Visibility.Visible;
            SqlHelper.UpdateById(Id, name, quantity, cost, price);
			if (quantity > formerQuantity)
			{
				SqlHelper.InsertLog(id, false, quantity-formerQuantity);
			}else if (quantity < formerQuantity)
            {
                SqlHelper.InsertLog(id, true, formerQuantity - quantity);
            }
		}
    }
}