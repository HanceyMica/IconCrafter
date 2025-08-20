using System.Collections.Generic;
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
    }
}