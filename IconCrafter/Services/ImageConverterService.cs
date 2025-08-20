using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
                
                // 使用并行处理提高性能，限制并发数以避免内存压力
                var semaphore = new SemaphoreSlim(Math.Max(1, Environment.ProcessorCount / 2));
                
                var tasks = sizes.Select(async size =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var outputPath = Path.Combine(outputDirectory, $"favicon_{size}x{size}.ico");
                        
                        // 在任务中创建图像副本以避免并发访问问题
                        using var imageClone = originalImage.Clone(ctx => { });
                        var icoData = CreateIcoFile(imageClone, new List<int> { size });
                        await File.WriteAllBytesAsync(outputPath, icoData);
                        return outputPath;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
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
        
        /// <summary>
        /// 批量处理多个图像文件（并行优化）
        /// </summary>
        /// <param name="inputFiles">输入文件路径列表</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <param name="progress">进度报告</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>处理结果列表</returns>
        public async Task<List<BatchProcessResult>> BatchProcessAsync(
            List<string> inputFiles, 
            string outputDirectory, 
            List<int> sizes,
            IProgress<BatchProcessProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (inputFiles == null || !inputFiles.Any())
                throw new ArgumentException("输入文件列表不能为空", nameof(inputFiles));
            
            if (string.IsNullOrEmpty(outputDirectory))
                throw new ArgumentException("输出目录不能为空", nameof(outputDirectory));
            
            if (sizes == null || !sizes.Any())
                throw new ArgumentException("尺寸列表不能为空", nameof(sizes));
            
            // 确保输出目录存在
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var results = new ConcurrentBag<BatchProcessResult>();
            var completedCount = 0;
            
            // 报告初始进度
            progress?.Report(new BatchProcessProgress
            {
                TotalFiles = inputFiles.Count,
                CompletedFiles = 0,
                CurrentFile = string.Empty
            });
            
            // 配置并行选项
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1), // 保留一个核心给UI
                CancellationToken = cancellationToken
            };
            
            try
            {
                await Task.Run(() =>
                {
                    Parallel.ForEach(inputFiles, parallelOptions, inputFile =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var stopwatch = Stopwatch.StartNew();
                        var result = new BatchProcessResult
                        {
                            InputFile = inputFile,
                            Success = false
                        };
                        
                        try
                        {
                            // 报告当前处理的文件
                            progress?.Report(new BatchProcessProgress
                            {
                                TotalFiles = inputFiles.Count,
                                CompletedFiles = completedCount,
                                CurrentFile = Path.GetFileName(inputFile)
                            });
                            
                            // 验证输入文件
                            if (!File.Exists(inputFile))
                            {
                                result.ErrorMessage = $"文件不存在: {inputFile}";
                                return;
                            }
                            
                            // 生成输出文件名
                            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputFile);
                            var outputFiles = new List<string>();
                            
                            // 为每个尺寸生成ICO文件
                            foreach (var size in sizes)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                
                                var outputFileName = $"{fileNameWithoutExt}_{size}x{size}.ico";
                                var outputPath = Path.Combine(outputDirectory, outputFileName);
                                
                                // 同步调用转换方法（在并行上下文中）
                                var icoData = ConvertToIcoSync(inputFile, new List<int> { size });
                                File.WriteAllBytes(outputPath, icoData);
                                
                                outputFiles.Add(outputPath);
                            }
                            
                            result.OutputFiles = outputFiles;
                            result.Success = true;
                        }
                        catch (OperationCanceledException)
                        {
                            result.ErrorMessage = "操作已取消";
                            throw; // 重新抛出取消异常
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = ex.Message;
                        }
                        finally
                        {
                            stopwatch.Stop();
                            result.ProcessingTime = stopwatch.Elapsed;
                            results.Add(result);
                            
                            // 更新完成计数和进度
                            var completed = Interlocked.Increment(ref completedCount);
                            progress?.Report(new BatchProcessProgress
                            {
                                TotalFiles = inputFiles.Count,
                                CompletedFiles = completed,
                                CurrentFile = completed < inputFiles.Count ? "处理中..." : "完成"
                            });
                        }
                    });
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 处理取消操作
                throw;
            }
            
            return results.OrderBy(r => inputFiles.IndexOf(r.InputFile)).ToList();
        }
        
        /// <summary>
        /// 同步版本的图像转换方法（用于并行处理）
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <returns>ICO文件的字节数组</returns>
        private byte[] ConvertToIcoSync(string inputPath, List<int> sizes)
        {
            try
            {
                if (string.IsNullOrEmpty(inputPath))
                    throw new ArgumentException("输入文件路径不能为空", nameof(inputPath));
                
                if (sizes == null || !sizes.Any())
                    throw new ArgumentException("尺寸列表不能为空", nameof(sizes));
                
                if (!File.Exists(inputPath))
                    throw new FileNotFoundException($"找不到输入文件: {inputPath}", inputPath);
                
                using var originalImage = ImageSharpImage.Load(inputPath);
                return CreateIcoFile(originalImage, sizes);
            }
            catch (Exception ex) when (!(ex is ImageConversionException))
            {
                throw new ImageConversionException(inputPath, sizes, $"转换图像时发生错误: {ex.Message}", ex);
            }
        }
    }
}