using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Data.SqlClient;
using PropertyChanged;
using StockTill.Helpers;
using System;
using System.ComponentModel;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

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
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogProgress.Visibility = Visibility.Visible;
            SqlHelper.Instance.UpdateById(Id, name, quantity, cost, price);
			if (quantity > formerQuantity)
			{
				SqlHelper.Instance.InsertLog(id, false, quantity-formerQuantity);
			}else if (quantity < formerQuantity)
            {
                SqlHelper.Instance.InsertLog(id, true, formerQuantity - quantity);
            }
		}
    }
}