using System;
using System.Threading.Tasks;

namespace IconCrafter.Logging
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试信息
        /// </summary>
        Debug,
        
        /// <summary>
        /// 一般信息
        /// </summary>
        Info,
        
        /// <summary>
        /// 警告信息
        /// </summary>
        Warning,
        
        /// <summary>
        /// 错误信息
        /// </summary>
        Error,
        
        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal
    }
    
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task LogAsync(LogLevel level, string message, Exception? exception = null);
        
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        Task LogDebugAsync(string message);
        
        /// <summary>
        /// 记录一般信息
        /// </summary>
        /// <param name="message">日志消息</param>
        Task LogInfoAsync(string message);
        
        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task LogWarningAsync(string message, Exception? exception = null);
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task LogErrorAsync(string message, Exception? exception = null);
        
        /// <summary>
        /// 记录致命错误
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task LogFatalAsync(string message, Exception? exception = null);
    }
}