using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using ZXing;
using ZXing.Common;

namespace StockTill.Helpers
{
    internal class BarcodeHelper
    {
        public static ImageSource GenerateBarcode(string data)
        {
            var writer = new ZXing.Windows.Compatibility.BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = 300,
                    Height = 80
                }
            };

            using (var bitmap = writer.Write(data)) // bitmap 是 System.Drawing.Bitmap
            {
                using (var memory = new MemoryStream())
                {
                    // 将 Bitmap 保存到内存流
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0; // 重置流位置

                    // 创建 WPF 的 BitmapImage
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 确保流关闭后图像仍可用
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // 可选：使图像可跨线程访问

                    return bitmapImage;
                }
            }
        }
    }
}
