using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IconCrafter.Logging
{
    /// <summary>
    /// 文件日志记录器实现
    /// </summary>
    public class FileLogger : ILogger, IDisposable
    {
        private readonly string _logFilePath;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;
        
        /// <summary>
        /// 初始化文件日志记录器
        /// </summary>
        /// <param name="logFileName">日志文件名（可选，默认为IconCrafter.log）</param>
        public FileLogger(string? logFileName = null)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDirectory = Path.Combine(appDataPath, "IconCrafter", "Logs");
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            var fileName = logFileName ?? $"IconCrafter_{DateTime.Now:yyyyMMdd}.log";
            _logFilePath = Path.Combine(logDirectory, fileName);
            _semaphore = new SemaphoreSlim(1, 1);
        }
        
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        public async Task LogAsync(LogLevel level, string message, Exception? exception = null)
        {
            if (_disposed)
                return;
                
            try
            {
                await _semaphore.WaitAsync();
                
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelString = level.ToString().ToUpper().PadRight(7);
                var logEntry = $"[{timestamp}] [{levelString}] {message}";
                
                if (exception != null)
                {
                    logEntry += $"\n异常详情: {exception}";
                }
                
                logEntry += Environment.NewLine;
                
                await File.AppendAllTextAsync(_logFilePath, logEntry);
                
                // 清理旧日志文件（保留最近7天）
                await CleanupOldLogsAsync();
            }
            catch
            {
                // 日志记录失败时静默处理，避免影响主程序
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        public async Task LogDebugAsync(string message)
        {
            await LogAsync(LogLevel.Debug, message);
        }
        
        /// <summary>
        /// 记录一般信息
        /// </summary>
        /// <param name="message">日志消息</param>
        public async Task LogInfoAsync(string message)
        {
            await LogAsync(LogLevel.Info, message);
        }
        
        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        public async Task LogWarningAsync(string message, Exception? exception = null)
        {
            await LogAsync(LogLevel.Warning, message, exception);
        }
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        public async Task LogErrorAsync(string message, Exception? exception = null)
        {
            await LogAsync(LogLevel.Error, message, exception);
        }
        
        /// <summary>
        /// 记录致命错误
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        public async Task LogFatalAsync(string message, Exception? exception = null)
        {
            await LogAsync(LogLevel.Fatal, message, exception);
        }
        
        /// <summary>
        /// 清理旧日志文件
        /// </summary>
        private async Task CleanupOldLogsAsync()
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(_logFilePath);
                if (string.IsNullOrEmpty(logDirectory) || !Directory.Exists(logDirectory))
                    return;
                
                var cutoffDate = DateTime.Now.AddDays(-7);
                var logFiles = Directory.GetFiles(logDirectory, "IconCrafter_*.log");
                
                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        try
                        {
                            File.Delete(logFile);
                        }
                        catch
                        {
                            // 删除失败时忽略
                        }
                    }
                }
            }
            catch
            {
                // 清理失败时忽略
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore?.Dispose();
                _disposed = true;
            }
        }
    }
}