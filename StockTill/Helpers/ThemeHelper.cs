using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockTill.Helpers
{
    internal class ThemeHelper
    {
        public static ApplicationTheme? Theme => ConfigHelper.Configuration["Theme"] switch
        {
            "Light" => ApplicationTheme.Light,
            "Dark" => ApplicationTheme.Dark,
            _ => null
        };

        public static void SetTheme(ApplicationTheme? theme)
        {
            ThemeManager.Current.ApplicationTheme = theme;
            ConfigHelper.EditConfig("Theme", theme);
        }

        public static void LoadTheme()
        {
            SetTheme(Theme);
        }
    }
}
