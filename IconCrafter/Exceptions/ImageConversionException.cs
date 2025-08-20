using System;
using System.Collections.Generic;

namespace IconCrafter.Exceptions
{
    /// <summary>
    /// 图像转换过程中发生的异常
    /// </summary>
    public class ImageConversionException : Exception
    {
        /// <summary>
        /// 输入文件路径
        /// </summary>
        public string InputPath { get; }
        
        /// <summary>
        /// 请求的尺寸列表
        /// </summary>
        public List<int> RequestedSizes { get; }
        
        /// <summary>
        /// 初始化ImageConversionException实例
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="sizes">请求的尺寸列表</param>
        /// <param name="message">异常消息</param>
        public ImageConversionException(string inputPath, List<int> sizes, string message)
            : base(message)
        {
            InputPath = inputPath;
            RequestedSizes = sizes;
        }
        
        /// <summary>
        /// 初始化ImageConversionException实例
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="sizes">请求的尺寸列表</param>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public ImageConversionException(string inputPath, List<int> sizes, string message, Exception innerException)
            : base(message, innerException)
        {
            InputPath = inputPath;
            RequestedSizes = sizes;
        }
    }
}