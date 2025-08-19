using Microsoft.Data.SqlClient;
using StockTill.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace StockTill
{
    /// <summary>
    /// InitWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InitWindow : Window
    {
        public InitWindow()
        {
            InitializeComponent();

            InitialzeSql();
        }

        private async void InitialzeSql()
        {
            await Task.Delay(1);

            try
            {
                InitBlock.Text = "正在连接数据库";
                if (await SqlHelper.Instance.ConnectAsync())
                {
                    InitBlock.Text = "欢迎使用 StockTill";
                    await Task.Delay(1000);
                    new MainWindow().Show();
                    this.Close();
                }
                else
                {  //数据库中不存在所需的表。此时应创建
                    InitBlock.Text = "正在初始化数据库";
                    SqlHelper.Instance.Initialize();
                    InitialzeSql();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "无法连接到数据库", MessageBoxButton.OK, MessageBoxImage.Hand);
                Application.Current.Shutdown();
            }
        }
    }
}
