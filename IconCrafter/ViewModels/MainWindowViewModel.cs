using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using IconCrafter.Commands;
using IconCrafter.Services;
using IconCrafter.Exceptions;
using IconCrafter.Logging;
using IconCrafter.Models;
using Microsoft.Win32;
using System.Windows.Forms;
using DialogResult = System.Windows.Forms.DialogResult;

namespace IconCrafter.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IImageConverter _imageConverter;
        private string? _inputFilePath;
        private string? _outputDirectory;
        private bool _isConverting;
        private string _statusMessage = "请选择要转换的图片文件";
        private bool _isSingleFile = true;
        private bool _size32Selected = true;
        private bool _size64Selected = true;
        private bool _size128Selected = true;
        private bool _size256Selected = true;
        private readonly SettingsService _settingsService;
        private readonly ILogger _logger;
        
        public MainWindowViewModel(IImageConverter imageConverter, SettingsService settingsService, ILogger? logger = null)
        {
            _imageConverter = imageConverter ?? throw new ArgumentNullException(nameof(imageConverter));
            _settingsService = settingsService;
            _logger = logger ?? new FileLogger();
            
            BrowseCommand = new RelayCommand(ExecuteBrowse);
            SelectOutputDirectoryCommand = new RelayCommand(ExecuteSelectOutputDirectory);
            ConvertCommand = new RelayCommand(ExecuteConvert, CanExecuteConvert);
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                await _logger.LogInfoAsync("正在初始化应用程序设置");
                
                var settings = await _settingsService.GetSettingsAsync();
                
                // 应用保存的设置
                if (!string.IsNullOrEmpty(settings.LastOutputDirectory) && Directory.Exists(settings.LastOutputDirectory))
                {
                    OutputDirectory = settings.LastOutputDirectory;
                    await _logger.LogInfoAsync($"已恢复上次使用的输出目录: {settings.LastOutputDirectory}");
                }
                
                // 应用默认尺寸选择
                if (settings.DefaultSelectedSizes != null)
                {
                    Size32Selected = settings.DefaultSelectedSizes.GetValueOrDefault(32, true);
                    Size64Selected = settings.DefaultSelectedSizes.GetValueOrDefault(64, true);
                    Size128Selected = settings.DefaultSelectedSizes.GetValueOrDefault(128, true);
                    Size256Selected = settings.DefaultSelectedSizes.GetValueOrDefault(256, true);
                }
                
                IsSingleFile = settings.DefaultSingleFile;
                
                await _logger.LogInfoAsync("应用程序设置初始化完成");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("初始化应用程序设置失败", ex);
                // 如果加载设置失败，使用默认值
            }
        }
        
        #region Properties
        
        /// <summary>
        /// 输入文件路径
        /// </summary>
        public string? InputFilePath
        {
            get => _inputFilePath;
            set
            {
                if (SetProperty(ref _inputFilePath, value))
                {
                    OnPropertyChanged(nameof(CanConvert));
                    ((RelayCommand)ConvertCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// 输出目录
        /// </summary>
        public string? OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                if (SetProperty(ref _outputDirectory, value))
                {
                    OnPropertyChanged(nameof(CanConvert));
                    ((RelayCommand)ConvertCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// 是否正在转换
        /// </summary>
        public bool IsConverting
        {
            get => _isConverting;
            set
            {
                if (SetProperty(ref _isConverting, value))
                {
                    OnPropertyChanged(nameof(CanConvert));
                    ((RelayCommand)ConvertCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        /// <summary>
        /// 是否生成单个文件
        /// </summary>
        public bool IsSingleFile
        {
            get => _isSingleFile;
            set => SetProperty(ref _isSingleFile, value);
        }
        
        /// <summary>
        /// 32x32尺寸是否选中
        /// </summary>
        public bool Size32Selected
        {
            get => _size32Selected;
            set => SetProperty(ref _size32Selected, value);
        }
        
        /// <summary>
        /// 64x64尺寸是否选中
        /// </summary>
        public bool Size64Selected
        {
            get => _size64Selected;
            set => SetProperty(ref _size64Selected, value);
        }
        
        /// <summary>
        /// 128x128尺寸是否选中
        /// </summary>
        public bool Size128Selected
        {
            get => _size128Selected;
            set => SetProperty(ref _size128Selected, value);
        }
        
        /// <summary>
        /// 256x256尺寸是否选中
        /// </summary>
        public bool Size256Selected
        {
            get => _size256Selected;
            set => SetProperty(ref _size256Selected, value);
        }
        
        /// <summary>
        /// 是否可以执行转换
        /// </summary>
        public bool CanConvert => !string.IsNullOrEmpty(InputFilePath) && 
                                  !string.IsNullOrEmpty(OutputDirectory) && 
                                  !IsConverting;
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// 浏览文件命令
        /// </summary>
        public ICommand BrowseCommand { get; }
        
        /// <summary>
        /// 选择输出目录命令
        /// </summary>
        public ICommand SelectOutputDirectoryCommand { get; }
        
        /// <summary>
        /// 转换命令
        /// </summary>
        public ICommand ConvertCommand { get; }
        
        #endregion
        
        #region Command Implementations
        
        private void ExecuteBrowse()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|PNG文件|*.png|JPEG文件|*.jpg;*.jpeg|BMP文件|*.bmp",
                Title = "选择图片文件",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                InputFilePath = openFileDialog.FileName;
                
                // 设置默认输出目录为原图片目录
                OutputDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                
                UpdateStatus($"已选择文件: {Path.GetFileName(openFileDialog.FileName)}");
            }
        }
        
        private void ExecuteSelectOutputDirectory()
        {
            var folderDialog = new FolderBrowserDialog
            {
                Description = "选择输出目录",
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                OutputDirectory = folderDialog.SelectedPath;
                UpdateStatus($"输出目录已设置: {folderDialog.SelectedPath}");
            }
        }
        
        private async void ExecuteConvert()
        {
            if (!CanExecuteConvert())
                return;
            
            IsConverting = true;
            UpdateStatus("开始转换...");
            
            try
            {
                await _logger.LogInfoAsync($"开始转换图像: {InputFilePath} -> {OutputDirectory}");
                
                await ConvertToIcoAsync();
                
                // 保存最后使用的路径
                await _settingsService.UpdateLastUsedPathsAsync(InputFilePath!, OutputDirectory!);
                
                await _logger.LogInfoAsync("图像转换成功完成");
                System.Windows.MessageBox.Show("ICO文件转换成功！", "成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (ImageConversionException ex)
            {
                await _logger.LogErrorAsync($"图像转换失败: {ex.Message}", ex);
                UpdateStatus($"转换失败: {ex.Message}\n文件: {ex.InputPath}");
                System.Windows.MessageBox.Show($"图像转换错误:\n{ex.Message}\n\n输入文件: {ex.InputPath}\n请求尺寸: {string.Join(", ", ex.RequestedSizes)}", "转换错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _logger.LogErrorAsync("文件访问权限不足", ex);
                UpdateStatus("转换失败: 文件访问权限不足");
                System.Windows.MessageBox.Show($"文件访问权限不足:\n{ex.Message}\n\n请检查文件是否被其他程序占用，或者是否有足够的权限访问目标目录。", "权限错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (FileNotFoundException ex)
            {
                await _logger.LogErrorAsync($"找不到文件: {ex.FileName}", ex);
                UpdateStatus($"错误: 找不到文件 {ex.FileName}");
                System.Windows.MessageBox.Show($"找不到指定的文件:\n{ex.FileName}\n\n请确保文件路径正确且文件存在。", "文件未找到", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (DirectoryNotFoundException ex)
            {
                await _logger.LogErrorAsync("指定目录不存在", ex);
                UpdateStatus("转换失败: 目录不存在");
                System.Windows.MessageBox.Show($"指定的目录不存在:\n{ex.Message}\n\n请确保输出目录存在且可访问。", "目录错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                await _logger.LogErrorAsync("文件IO错误", ex);
                UpdateStatus("转换失败: 文件IO错误");
                System.Windows.MessageBox.Show($"文件读写错误:\n{ex.Message}\n\n请检查磁盘空间是否充足，文件是否被占用。", "IO错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                await _logger.LogFatalAsync($"未知错误: {ex.Message}", ex);
                UpdateStatus($"转换失败: {ex.Message}");
                System.Windows.MessageBox.Show($"发生未知错误:\n{ex.Message}\n\n请联系开发者获取支持。", "未知错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsConverting = false;
            }
        }
        
        private bool CanExecuteConvert()
        {
            return CanConvert;
        }
        
        #endregion
        
        #region Private Methods
        
        private async Task ConvertToIcoAsync()
        {
            var selectedSizes = GetSelectedSizes();
            if (!selectedSizes.Any())
            {
                UpdateStatus("错误: 请至少选择一个尺寸");
                return;
            }
            
            UpdateStatus($"正在处理 {selectedSizes.Count} 个尺寸...");
            
            if (IsSingleFile)
            {
                // 生成单个包含所有尺寸的ICO文件
                var outputPath = Path.Combine(OutputDirectory!, "favicon.ico");
                await _imageConverter.GenerateSingleIcoFileAsync(InputFilePath!, outputPath, selectedSizes);
                UpdateStatus($"成功生成ICO文件: {outputPath}");
            }
            else
            {
                // 为每个尺寸生成单独的ICO文件
                var results = await _imageConverter.GenerateMultipleIcoFilesAsync(InputFilePath!, OutputDirectory!, selectedSizes);
                UpdateStatus($"成功生成 {results.Count} 个ICO文件:\n{string.Join("\n", results)}");
            }
        }
        
        private List<int> GetSelectedSizes()
        {
            var sizes = new List<int>();
            if (Size32Selected) sizes.Add(32);
            if (Size64Selected) sizes.Add(64);
            if (Size128Selected) sizes.Add(128);
            if (Size256Selected) sizes.Add(256);
            return sizes;
        }
        
        private void UpdateStatus(string message)
        {
            StatusMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }
        
        #endregion
        
        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        #endregion
    }
}