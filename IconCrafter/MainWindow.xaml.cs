using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Forms;
using DialogResult = System.Windows.Forms.DialogResult;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace IconCrafter
{
    /// <summary>
    /// PNG转ICO工具主窗口
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private string? _inputFilePath;
        private string? _outputDirectory;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|PNG文件|*.png|JPEG文件|*.jpg;*.jpeg|BMP文件|*.bmp";
            openFileDialog.Title = "选择图片文件";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (openFileDialog.ShowDialog() == true)
            {
                _inputFilePath = openFileDialog.FileName;
                InputFileTextBox.Text = openFileDialog.FileName;
                InputFileTextBox.Foreground = System.Windows.Media.Brushes.Black;
                
                // 设置默认输出目录为原图片目录
                _outputDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                OutputDirTextBox.Text = _outputDirectory;
                OutputDirTextBox.Foreground = System.Windows.Media.Brushes.Black;
                
                ConvertButton.IsEnabled = true;
                UpdateStatus($"已选择文件: {Path.GetFileName(openFileDialog.FileName)}");
            }
        }

        private void OutputDirButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "选择输出目录";
            folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _outputDirectory = folderDialog.SelectedPath;
                OutputDirTextBox.Text = folderDialog.SelectedPath;
                OutputDirTextBox.Foreground = System.Windows.Media.Brushes.Black;
                UpdateStatus($"输出目录已设置: {folderDialog.SelectedPath}");
            }
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_inputFilePath) || string.IsNullOrEmpty(_outputDirectory))
            {
                UpdateStatus("错误: 请选择输入文件和输出目录");
                return;
            }

            ConvertButton.IsEnabled = false;
            UpdateStatus("开始转换...");

            try
            {
                await ConvertToIcoAsync();
            }
            catch (Exception ex)
            {
                UpdateStatus($"转换失败: {ex.Message}");
            }
            finally
            {
                ConvertButton.IsEnabled = true;
            }
        }

        private async Task ConvertToIcoAsync()
        {
            var selectedSizes = GetSelectedSizes();
            if (!selectedSizes.Any())
            {
                UpdateStatus("错误: 请至少选择一个尺寸");
                return;
            }

            UpdateStatus($"正在处理 {selectedSizes.Count} 个尺寸...");

            using var originalImage = await ImageSharpImage.LoadAsync(_inputFilePath!);
            
            if (SingleFileRadio.IsChecked == true)
            {
                // 生成单个包含所有尺寸的ICO文件
                await GenerateSingleIcoFile(originalImage, selectedSizes);
            }
            else
            {
                // 为每个尺寸生成单独的ICO文件
                await GenerateMultipleIcoFiles(originalImage, selectedSizes);
            }
        }

        private async Task GenerateSingleIcoFile(ImageSharpImage originalImage, List<int> sizes)
        {
            var outputPath = Path.Combine(_outputDirectory!, "favicon.ico");
            
            // 创建ICO文件内容
            var icoData = CreateIcoFile(originalImage, sizes);
            
            await File.WriteAllBytesAsync(outputPath, icoData);
            UpdateStatus($"成功生成ICO文件: {outputPath}");
        }

        private async Task GenerateMultipleIcoFiles(ImageSharpImage originalImage, List<int> sizes)
        {
            var tasks = sizes.Select(async size =>
            {
                var outputPath = Path.Combine(_outputDirectory!, $"favicon_{size}x{size}.ico");
                var icoData = CreateIcoFile(originalImage, new List<int> { size });
                await File.WriteAllBytesAsync(outputPath, icoData);
                return outputPath;
            });

            var results = await Task.WhenAll(tasks);
            UpdateStatus($"成功生成 {results.Length} 个ICO文件:\n{string.Join("\n", results)}");
        }

        private byte[] CreateIcoFile(ImageSharpImage originalImage, List<int> sizes)
        {
            using var memoryStream = new MemoryStream();
            
            // ICO文件头
            var writer = new BinaryWriter(memoryStream);
            writer.Write((ushort)0); // Reserved
            writer.Write((ushort)1); // Type (1 = ICO)
            writer.Write((ushort)sizes.Count); // Number of images

            var imageDataList = new List<byte[]>();
            var currentOffset = 6 + (sizes.Count * 16); // Header + directory entries

            // 写入目录条目
            foreach (var size in sizes)
            {
                var resizedImage = originalImage.Clone(x => x.Resize(size, size));
                var imageData = new MemoryStream();
                resizedImage.Save(imageData, new PngEncoder());
                var imageBytes = imageData.ToArray();
                imageDataList.Add(imageBytes);

                writer.Write((byte)(size == 256 ? 0 : size)); // Width (0 = 256)
                writer.Write((byte)(size == 256 ? 0 : size)); // Height (0 = 256)
                writer.Write((byte)0); // Color count
                writer.Write((byte)0); // Reserved
                writer.Write((ushort)1); // Color planes
                writer.Write((ushort)32); // Bits per pixel
                writer.Write((uint)imageBytes.Length); // Image data size
                writer.Write((uint)currentOffset); // Image data offset

                currentOffset += imageBytes.Length;
            }

            // 写入图像数据
            foreach (var imageData in imageDataList)
            {
                writer.Write(imageData);
            }

            return memoryStream.ToArray();
        }

        private List<int> GetSelectedSizes()
        {
            var sizes = new List<int>();
            if (Size16CheckBox.IsChecked == true) sizes.Add(16);
            if (Size32CheckBox.IsChecked == true) sizes.Add(32);
            if (Size64CheckBox.IsChecked == true) sizes.Add(64);
            if (Size128CheckBox.IsChecked == true) sizes.Add(128);
            if (Size256CheckBox.IsChecked == true) sizes.Add(256);
            return sizes;
        }

        private void UpdateStatus(string message)
        {
            StatusTextBlock.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }
    }
}
