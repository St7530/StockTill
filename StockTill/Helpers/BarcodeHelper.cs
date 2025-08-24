using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace StockTill.Helpers
{
    internal class BarcodeHelper
    {
        private static Bitmap GenerateBarcode(string data)
        {
            BarcodeWriter writer = new ZXing.Windows.Compatibility.BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = 360,
                    Height = 100,
                    Margin = 10
                }
            };

            Bitmap bitmap = writer.Write(data);
            return bitmap;
        }

        // 保存为 PNG 格式 (推荐，无损压缩)
        //bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        private static ImageSource Bitmap2ImageSource(Bitmap bitmap)
        {
            using (bitmap) // bitmap 是 System.Drawing.Bitmap
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

        public static ImageSource GenerateBarcodeSource(string data)
        {
            return Bitmap2ImageSource(GenerateBarcode(data));
        }

        public static void SaveBarcode(string id, string name)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = $"保存条形码：{id} - {name}";
            saveFileDialog.Filter = "PNG 图片 (*.png)|*.png";
            saveFileDialog.DefaultExt = "png";
            saveFileDialog.FileName = $"{id} - {name}.png";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                GenerateBarcode(id).Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
