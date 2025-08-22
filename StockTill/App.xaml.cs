using System.Globalization;
using System.Windows;

namespace StockTill
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 根据需要设置不同的语言  
            SetApplicationCulture("zh-CN"); // 中文简体  
                                            // SetApplicationCulture("zh-TW"); // 中文繁体  
                                            // SetApplicationCulture("en-US"); // 英语  
                                            // SetApplicationCulture("ja-JP"); // 日语  

            base.OnStartup(e);
        }

        private void SetApplicationCulture(string cultureName)
        {
            var culture = new CultureInfo(cultureName);

            // 设置当前线程的文化  
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // 设置应用程序域的默认文化  
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }

}
