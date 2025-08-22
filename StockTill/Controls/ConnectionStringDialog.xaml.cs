using iNKORE.UI.WPF.Modern.Controls;
using PropertyChanged;
using System.Diagnostics;
using System.Windows;
using StockTill.Helpers;

namespace StockTill.Controls
{
    [AddINotifyPropertyChangedInterface]
    public partial class ConnectionStringDialog
    {
        private string connectionString = ConfigHelper.GetConnectionString();
        public string ConnectionString
        {
            get => connectionString;
            set
            {
                connectionString = value;
                IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(value);
            }
        }
        public ConnectionStringDialog()
        {
            InitializeComponent();
            DataContext = this;

            IsPrimaryButtonEnabled = false;
        }
        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogProgress.Visibility = Visibility.Visible;

            ConfigHelper.EditConfig("ConnectionString", ConnectionString);

            // 重启应用程序
            Process.Start(Environment.ProcessPath);
            Application.Current.Shutdown();
        }
    }
}