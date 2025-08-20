using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace IconCrafter.Models
{
    /// <summary>
    /// 应用程序设置类
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 默认输出目录
        /// </summary>
        public string DefaultOutputDirectory { get; set; } = "";
        
        /// <summary>
        /// 默认尺寸列表
        /// </summary>
        public List<int> DefaultSizes { get; set; } = new() { 32, 64, 128, 256, 512 };
        
        /// <summary>
        /// 是否记住上次使用的目录
        /// </summary>
        public bool RememberLastDirectory { get; set; } = true;
        
        /// <summary>
        /// 默认文件名
        /// </summary>
        public string DefaultFileName { get; set; } = "favicon.ico";
        
        /// <summary>
        /// 是否默认生成单个文件
        /// </summary>
        public bool DefaultSingleFile { get; set; } = true;
        
        /// <summary>
        /// 默认选中的尺寸
        /// </summary>
        public Dictionary<int, bool> DefaultSelectedSizes { get; set; } = new()
        {
            { 32, true },
            { 64, true },
            { 128, true },
            { 256, true },
            { 512, true }
        };
        
        /// <summary>
        /// 上次使用的输入文件路径
        /// </summary>
        public string LastInputFilePath { get; set; } = "";
        
        /// <summary>
        /// 上次使用的输出目录
        /// </summary>
        public string LastOutputDirectory { get; set; } = "";
    }
    
    /// <summary>
    /// 设置管理服务
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings? _settings;
        
        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "IconCrafter");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
        }
        
        /// <summary>
        /// 获取应用程序设置
        /// </summary>
        /// <returns>应用程序设置</returns>
        public async Task<AppSettings> GetSettingsAsync()
        {
            if (_settings != null)
                return _settings;
            
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _settings = new AppSettings();
                }
            }
            catch (Exception)
            {
                // 如果读取设置失败，使用默认设置
                _settings = new AppSettings();
            }
            
            return _settings;
        }
        
        /// <summary>
        /// 保存应用程序设置
        /// </summary>
        /// <param name="settings">要保存的设置</param>
        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                _settings = settings;
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception)
            {
                // 保存失败时静默处理，不影响应用程序运行
            }
        }
        
        /// <summary>
        /// 更新上次使用的路径
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出目录路径</param>
        public async Task UpdateLastUsedPathsAsync(string inputPath, string outputPath)
        {
            var settings = await GetSettingsAsync();
            
            if (settings.RememberLastDirectory)
            {
                settings.LastInputFilePath = inputPath;
                settings.LastOutputDirectory = outputPath;
                await SaveSettingsAsync(settings);
            }
        }
        
        /// <summary>
        /// 重置设置为默认值
        /// </summary>
        public async Task ResetToDefaultAsync()
        {
            _settings = new AppSettings();
            await SaveSettingsAsync(_settings);
        }
    }
}