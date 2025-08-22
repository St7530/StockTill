using iNKORE.UI.WPF.Modern;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Controls;

namespace StockTill.Helpers
{
    internal class ConfigHelper
    {
        public static IConfiguration Configuration { get; private set; }
        public record Config
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public ApplicationTheme? Theme { get; set; } = null; // 主题颜色
            public ConnectionStringsSection ConnectionStrings { get; set; } = new(); // 数据库连接字符串
            public UpdateSection Update { get; set; } = new(); // 更新
        }
        public record ConnectionStringsSection
        {
            public string Default { get; set; } = @"Server=localhost\SQLEXPRESS;Database=StockTillDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }
        public record UpdateSection
        {
            public bool IsAutoCheckForUpdate { get; set; } = false;
            public string UpdateURL { get; set; } = "https://github.com/St7530/StockTill/raw/refs/heads/master/UpdateInfo.xml";
        }

        private static string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StockTill", "appsettings.json");
        static ConfigHelper()
        {
            string configDir = Path.GetDirectoryName(configFile);

            if (!File.Exists(configFile))
            { // 配置文件不存在，写入默认配置
                Directory.CreateDirectory(configDir); // 确保目录存在
                File.WriteAllText(configFile, JsonConvert.SerializeObject(new Config(), Formatting.Indented), Encoding.UTF8); // 写入默认配置
            }

            // 构建配置
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();
        }
        public static string GetConnectionString()
        {
            return Configuration.GetConnectionString("Default");
        }
        public static void EditConfig(string key, object value)
        {
            string json = File.ReadAllText(configFile, Encoding.UTF8);
            Config config = JsonConvert.DeserializeObject<Config>(json);

            switch (key)
            {
                case "Theme":
                    config.Theme = (ApplicationTheme?)value;
                    break;
                case "ConnectionString":
                    config.ConnectionStrings.Default = (string)value;
                    break;
                case "Update:IsAutoCheckForUpdate":
                    config.Update.IsAutoCheckForUpdate = (bool)value;
                    break;
                case "Update:UpdateURL":
                    config.Update.UpdateURL = (string)value;
                    break;
            }

            File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented), Encoding.UTF8);
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();
        }
        public static void DeleteConfig()
        {
            File.Delete(configFile);
        }
    }
}
