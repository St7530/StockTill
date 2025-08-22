using AutoUpdaterDotNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace StockTill.Helpers
{
    internal class UpdateHelper
    {
        private static string UpdateURL => ConfigHelper.Configuration["Update:UpdateURL"];
        private static bool IsOnStartup = true;
        static UpdateHelper()
        {
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.HttpUserAgent = $"StockTill/{Assembly.GetExecutingAssembly().GetName().Version}";
        }
        public static void CheckForUpdateOnStartup()
        {
            AutoUpdater.Synchronous = true;
            AutoUpdater.Start(UpdateURL);
        }
        public static void CheckForUpdate()
        {
            IsOnStartup = false;
            AutoUpdater.Start(UpdateURL);
        }
        private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            { // 检查更新成功
                if (args.IsUpdateAvailable)
                { // 发现新版本
                    if (MessageBox.Show(
                        $"发现新版本：{args.CurrentVersion}\n" +
                        $"当前版本：{args.InstalledVersion}\n" +
                        $"是否更新？",
                        "StockTill 更新",
                        MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                    { // 更新
                        try
                        {
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                Application.Current.Shutdown();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                else
                { // 暂无新版本
                    if(!IsOnStartup)
                        MessageBox.Show("当前版本已是最新。", "StockTill 更新", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
            else
            { // 检查更新失败
                if (!IsOnStartup)
                    MessageBox.Show(
                    "无法连接至更新服务器。\n" +
                    "错误信息：" + args.Error.Message,
                    "StockTill 更新", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
