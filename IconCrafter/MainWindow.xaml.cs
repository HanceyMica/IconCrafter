using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using IconCrafter.ViewModels;
using IconCrafter.Services;
using IconCrafter.Models;
using IconCrafter.Logging;

namespace IconCrafter
{
    /// <summary>
    /// PNG转ICO工具主窗口
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly SettingsService _settingsService;

        public MainWindow()
        {
            InitializeComponent();
            
            _settingsService = new SettingsService();
            var imageConverter = new ImageConverterService();
            var logger = new FileLogger();
            _viewModel = new MainWindowViewModel(imageConverter, _settingsService, logger);
            
            DataContext = _viewModel;
            
            LoadIcoImage();
            _ = InitializeAsync();
        }
        
        private async Task InitializeAsync()
        {
            await _viewModel.InitializeAsync();
        }

        private void LoadIcoImage()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Assets/ico.png");
                bitmap.EndInit();
                IcoImage.Source = bitmap;
            }
            catch
            {
                // 如果加载失败，保持默认状态
            }
        }
    }
}
