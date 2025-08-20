using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using IconCrafter.Exceptions;

namespace IconCrafter.Services
{
    /// <summary>
    /// 图像转换服务实现
    /// </summary>
    public class ImageConverterService : IImageConverter
    {
        /// <summary>
        /// 异步转换图像为ICO格式
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <returns>ICO文件的字节数组</returns>
        public async Task<byte[]> ConvertToIcoAsync(string inputPath, List<int> sizes)
        {
            try
            {
                if (string.IsNullOrEmpty(inputPath))
                    throw new ArgumentException("输入文件路径不能为空", nameof(inputPath));
                
                if (sizes == null || !sizes.Any())
                    throw new ArgumentException("尺寸列表不能为空", nameof(sizes));
                
                if (!File.Exists(inputPath))
                    throw new FileNotFoundException($"找不到输入文件: {inputPath}", inputPath);
                
                using var originalImage = await ImageSharpImage.LoadAsync(inputPath);
                return CreateIcoFile(originalImage, sizes);
            }
            catch (Exception ex) when (!(ex is ImageConversionException))
            {
                throw new ImageConversionException(inputPath, sizes, $"转换图像时发生错误: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 异步生成单个ICO文件
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="sizes">目标尺寸列表</param>
        public async Task GenerateSingleIcoFileAsync(string inputPath, string outputPath, List<int> sizes)
        {
            try
            {
                var icoData = await ConvertToIcoAsync(inputPath, sizes);
                
                // 确保输出目录存在
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                
                await File.WriteAllBytesAsync(outputPath, icoData);
            }
            catch (Exception ex) when (!(ex is ImageConversionException))
            {
                throw new ImageConversionException(inputPath, sizes, $"生成ICO文件时发生错误: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 异步生成多个ICO文件
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <returns>生成的文件路径列表</returns>
        public async Task<List<string>> GenerateMultipleIcoFilesAsync(string inputPath, string outputDirectory, List<int> sizes)
        {
            try
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                
                using var originalImage = await ImageSharpImage.LoadAsync(inputPath);
                
                // 使用并行处理提高性能
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                
                var tasks = sizes.Select(async size =>
                {
                    var outputPath = Path.Combine(outputDirectory, $"favicon_{size}x{size}.ico");
                    var icoData = CreateIcoFile(originalImage, new List<int> { size });
                    await File.WriteAllBytesAsync(outputPath, icoData);
                    return outputPath;
                });
                
                var results = await Task.WhenAll(tasks);
                return results.ToList();
            }
            catch (Exception ex) when (!(ex is ImageConversionException))
            {
                throw new ImageConversionException(inputPath, sizes, $"生成多个ICO文件时发生错误: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 创建ICO文件数据
        /// </summary>
        /// <param name="originalImage">原始图像</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <returns>ICO文件的字节数组</returns>
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
                using var resizedImage = originalImage.Clone(x => x.Resize(size, size));
                using var imageData = new MemoryStream();
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
    }
}