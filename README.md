# IconCrafter - PNG转ICO工具

一个基于WPF的现代化PNG转ICO转换工具，支持生成多种尺寸的ICO文件。提供完整的Windows安装包，支持自定义应用图标。

## 功能特性

- 🖼️ **多格式支持**: 支持PNG、JPG、JPEG、BMP格式的输入图片
- 📏 **多尺寸生成**: 支持32x32、64x64、128x128、256x256像素的ICO文件
- 📦 **灵活输出**: 
  - 生成单个包含所有尺寸的ICO文件
  - 为每个尺寸生成单独的ICO文件
- 📁 **智能目录**: 默认输出到原图片目录，也可自定义输出目录
- 🎯 **默认命名**: 生成的文件默认命名为`favicon.ico`
- 🖥️ **现代界面**: 基于WPF的现代化用户界面，首页显示应用图标
- 📦 **完整安装包**: 提供NSIS制作的Windows安装程序
- 🎨 **自定义图标**: 应用程序使用自定义favicon.ico图标

## 系统要求

- Windows 10 版本 1809 (build 17763) 或更高版本
- .NET 8.0 运行时

## 使用方法

1. **选择图片**: 点击"浏览"按钮选择要转换的图片文件
2. **选择尺寸**: 勾选需要生成的ICO尺寸（默认全选）
3. **选择输出方式**: 
   - 单个文件：生成包含所有尺寸的favicon.ico
   - 多个文件：为每个尺寸生成单独的ICO文件
4. **设置输出目录**: 默认为原图片目录，可点击"选择目录"自定义
5. **开始转换**: 点击"开始转换"按钮执行转换

## 技术栈

- **框架**: WPF (.NET 8.0)
- **图像处理**: SixLabors.ImageSharp
- **安装包制作**: NSIS (Nullsoft Scriptable Install System)
- **开发环境**: Visual Studio 2022

## 构建说明

### 前置要求
- Visual Studio 2022 (带有.NET桌面开发工作负载)
- .NET 8.0 SDK
- NSIS (用于制作安装包)

### 构建步骤
```bash
# 克隆项目
git clone <repository-url>
cd IconCrafter

# 还原NuGet包
dotnet restore

# 构建项目
dotnet build

# 运行项目
dotnet run --project IconCrafter\IconCrafter.csproj
```

### 发布应用
```bash
# 构建Release版本
dotnet build IconCrafter\IconCrafter.csproj --configuration Release

# 复制文件到发布目录
Copy-Item -Path .\IconCrafter\bin\Release\net8.0-windows10.0.19041.0\win-x64\* -Destination .\Release_PKG\ -Recurse

# 使用NSIS制作安装包
makensis.exe IconCrafter_Installer.nsi
```

## 项目结构

```
IconCrafter/
├── IconCrafter/
│   ├── MainWindow.xaml          # 主界面XAML
│   ├── MainWindow.xaml.cs       # 主界面逻辑
│   ├── App.xaml                 # 应用程序XAML
│   ├── App.xaml.cs              # 应用程序逻辑
│   ├── IconCrafter.csproj       # 项目文件
│   └── Assets/                  # 资源文件
│       ├── ico.png              # 应用图标PNG
│       └── favicon.ico          # 应用图标ICO
├── IconCrafter.sln              # 解决方案文件
├── IconCrafter_Installer.nsi    # NSIS安装脚本
├── IconCrafter_Setup.exe        # 生成的安装包
├── Release_PKG/                 # 发布文件目录
├── LICENSE.txt                  # 许可证文件
├── .gitignore                   # Git忽略文件
└── README.md                    # 说明文档
```

## 依赖包

- `SixLabors.ImageSharp` - 图像处理库
- `Microsoft.Windows.SDK.BuildTools` - Windows SDK构建工具

## 许可证

本项目采用MIT许可证。详见LICENSE文件。

## 贡献

欢迎提交Issue和Pull Request来改进这个项目！

## 安装说明

### 使用安装包（推荐）
1. 下载 `IconCrafter_Setup.exe` 安装包
2. 以管理员身份运行安装程序
3. 按照安装向导完成安装
4. 安装完成后可在开始菜单找到IconCrafter

### 手动安装
1. 下载Release_PKG文件夹中的所有文件
2. 解压到任意目录
3. 双击IconCrafter.exe运行

## 更新日志

### v1.1.0
- 添加应用程序自定义图标
- 界面首页显示ico.png图片
- 提供完整的NSIS安装包
- 优化构建配置，解决剪裁兼容性问题
- 添加.gitignore文件

### v1.0.0
- 初始版本发布
- 支持PNG转ICO基本功能
- 支持多尺寸ICO生成
- 现代化WPF界面