using iNKORE.UI.WPF.Modern.Controls;
using PropertyChanged;
using System.Windows;
using StockTill.Helpers;

namespace StockTill.Controls
{
    [AddINotifyPropertyChangedInterface]
    public partial class UpdateURLDialog
    {
        private string updateURL = ConfigHelper.Configuration["Update:UpdateURL"];
        public string UpdateURL
        {
            get => updateURL;
            set
            {
                updateURL = value;
                IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(value);
            }
        }
        public UpdateURLDialog()
        {
            InitializeComponent();
            DataContext = this;

            IsPrimaryButtonEnabled = false;
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogProgress.Visibility = Visibility.Visible;

            ConfigHelper.EditConfig("Update:UpdateURL", UpdateURL);
        }
    }
}