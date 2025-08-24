using System.Windows;
using StockTill.Helpers;
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

            ThemeHelper.LoadTheme();
            CheckForUpdate();
        }

        private async void CheckForUpdate()
        {
            await Task.Delay(1);

            if (bool.Parse(ConfigHelper.Configuration["Update:IsAutoCheckForUpdate"]) == true)
            {
                InitBlock.Text = "正在检查更新";
                await Task.Delay(1000);
                UpdateHelper.CheckForUpdateOnStartup();
            }
            InitialzeSql();
        }
        private async void InitialzeSql()
        {
            await Task.Delay(1);

            try
            {
                InitBlock.Text = "正在连接数据库";
                if (await SqlHelper.ConnectAsync())
                {
                    InitBlock.Text = "欢迎使用 StockTill";
                    await Task.Delay(1000);
                    new MainWindow().Show();
                    this.Close();
                }
                else
                { //数据库连接成功，但未初始化
                    InitBlock.Text = "正在初始化数据库";
                    SqlHelper.Initialize();
                    InitialzeSql();
                }
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(
                    "数据库可能尚未安装或启动，或者连接字符串设置有误。\n" +
                    "请检查数据库服务是否正常，以及 StockTill 设置的连接字符串是否正确。\n\n" +
                    $"错误信息：{ex.Message}\n\n" +
                    "是否要退出 StockTill ？",
                    "无法连接到数据库", MessageBoxButton.YesNo, MessageBoxImage.Hand) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    new MainWindow(false).Show();
                    this.Close();
                }
            }
        }
    }
}
