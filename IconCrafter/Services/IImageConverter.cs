using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IconCrafter.Services
{
    /// <summary>
    /// 图像转换服务接口
    /// </summary>
    public interface IImageConverter
    {
        /// <summary>
        /// 异步转换图像为ICO格式
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <returns>ICO文件的字节数组</returns>
        Task<byte[]> ConvertToIcoAsync(string inputPath, List<int> sizes);
        
        /// <summary>
        /// 异步生成单个ICO文件
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="sizes">目标尺寸列表</param>
        Task GenerateSingleIcoFileAsync(string inputPath, string outputPath, List<int> sizes);
        
        /// <summary>
        /// 异步生成多个ICO文件
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <returns>生成的文件路径列表</returns>
        Task<List<string>> GenerateMultipleIcoFilesAsync(string inputPath, string outputDirectory, List<int> sizes);
        
        /// <summary>
        /// 批量处理多个图像文件（并行优化）
        /// </summary>
        /// <param name="inputFiles">输入文件路径列表</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="sizes">目标尺寸列表</param>
        /// <param name="progress">进度报告</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>处理结果列表</returns>
        Task<List<BatchProcessResult>> BatchProcessAsync(
            List<string> inputFiles, 
            string outputDirectory, 
            List<int> sizes,
            IProgress<BatchProcessProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// 批量处理结果
    /// </summary>
    public class BatchProcessResult
    {
        public string InputFile { get; set; } = string.Empty;
        public List<string> OutputFiles { get; set; } = new();
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }
    
    /// <summary>
    /// 批量处理进度
    /// </summary>
    public class BatchProcessProgress
    {
        public int TotalFiles { get; set; }
        public int CompletedFiles { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
        public double ProgressPercentage => TotalFiles > 0 ? (double)CompletedFiles / TotalFiles * 100 : 0;
    }
}