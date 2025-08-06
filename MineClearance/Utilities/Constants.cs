using System.Runtime.InteropServices;
using MineClearance.Models;

namespace MineClearance.Utilities;

/// <summary>
/// 常量类, 提供一些常量
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// 作者名字
    /// </summary>
    public const string AuthorName = "xiting910";

    /// <summary>
    /// GitHub 仓库链接
    /// </summary>
    public const string GitHubRepoUrl = "https://github.com/xiting910/Csharp/tree/main/MineClearance";

    /// <summary>
    /// 数据存储路径
    /// </summary>
    public static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MineClearanceData");

    /// <summary>
    /// 数据文件路径
    /// </summary>
    public static readonly string DataFilePath = Path.Combine(DataPath, "history.json");

    /// <summary>
    /// 错误文件路径
    /// </summary>
    public static readonly string ErrorFilePath = Path.Combine(DataPath, "error.log");

    /// <summary>
    /// 配置文件路径
    /// </summary>
    public static readonly string ConfigFilePath = Path.Combine(DataPath, "config.json");

    /// <summary>
    /// 桌面路径
    /// </summary>
    public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    /// <summary>
    /// 卸载脚本路径
    /// </summary>
    public static readonly string UninstallPowerShellScriptPath = Path.Combine(Path.GetTempPath(), "UninstallScript.ps1");

    /// <summary>
    /// 用于更新程序的powershell脚本路径
    /// </summary>
    public static readonly string UpdatePowerShellScriptPath = Path.Combine(Path.GetTempPath(), "UpdateScript.ps1");

    /// <summary>
    /// 自动更新的URL
    /// </summary>
    public const string AutoUpdateUrl = "https://gitee.com/xiting910/mine-clearance/raw/main/AutoUpdater.xml";

    /// <summary>
    /// 更新文件路径
    /// </summary>
    public static readonly string SevenZipPath = Path.Combine(Path.GetTempPath(), "MineClearance.7z");

    /// <summary>
    /// 当前程序的目录
    /// </summary>
    public static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

    /// <summary>
    /// 当前程序的完整路径
    /// </summary>
    public static readonly string ExecutableFilePath = Application.ExecutablePath;

    /// <summary>
    /// 当前程序的可执行文件名
    /// </summary>
    public static readonly string ExecutableFileName = Path.GetFileName(ExecutableFilePath);

    /// <summary>
    /// 当前程序的目录的上级目录
    /// </summary>
    public static readonly string ParentDirectory = Path.GetDirectoryName(CurrentDirectory) ?? throw new InvalidOperationException("无法获取上级目录");

    /// <summary>
    /// 7za.exe的路径
    /// </summary>
    public static readonly string SevenZipExe = Path.Combine(ParentDirectory, "7za.exe");

    /// <summary>
    /// 更新下载速度刷新间隔(毫秒)
    /// </summary>
    public const int UpdateSpeedRefreshInterval = 300;

    /// <summary>
    /// http请求超时时间(秒)
    /// </summary>
    public const int HttpRequestTimeout = 20;

    /// <summary>
    /// 无进度一定秒后重试
    /// </summary>
    public const int NoProgressRetryInterval = 15;

    /// <summary>
    /// 无进度弹窗等待时间(秒)
    /// </summary>
    public const int NoProgressDialogWaitTime = 5;

    /// <summary>
    /// 无进度最大重试次数
    /// </summary>
    public const int NoProgressMaxRetries = 3;

    /// <summary>
    /// 获取指定难度的棋盘设置
    /// </summary>
    /// <param name="level">难度级别</param>
    /// <returns>棋盘宽度、高度和地雷数量的元组</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果难度级别不在预定义范围内</exception>
    public static (int width, int height, int mineCount) GetSettings(DifficultyLevel level)
    {
        return level switch
        {
            DifficultyLevel.Easy => (9, 9, 10),
            DifficultyLevel.Medium => (16, 16, 40),
            DifficultyLevel.Hard => (30, 16, 99),
            DifficultyLevel.Hell => (50, 30, 309),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    /// <summary>
    /// 当前数据版本号
    /// </summary>
    public const int CurrentDataVersion = 3;

    /// <summary>
    /// 扫雷棋盘的最大宽度
    /// </summary>
    public const int MaxBoardWidth = 50;

    /// <summary>
    /// 扫雷棋盘的最大高度
    /// </summary>
    public const int MaxBoardHeight = 30;

    /// <summary>
    /// 无效位置
    /// </summary>
    public static readonly Position InvalidPosition = new(-1, -1);

    /// <summary>
    /// 主窗体的宽度
    /// </summary>
    public static int MainFormWidth => (int)(1264 * DpiScale);

    /// <summary>
    /// 主窗体的高度
    /// </summary>
    public static int MainFormHeight => (int)(853 * DpiScale);

    /// <summary>
    /// 底部状态栏的高度
    /// </summary>
    public static int BottomStatusBarHeight => (int)(55 * DpiScale);

    /// <summary>
    /// 网格大小
    /// </summary>
    public static int GridSize => (int)(25 * DpiScale);

    /// <summary>
    /// 设置窗体的最小宽度
    /// </summary>
    public static int SettingFormMinWidth => (int)(250 * DpiScale);

    /// <summary>
    /// 设置窗体的最小高度
    /// </summary>
    public static int SettingFormMinHeight => (int)(250 * DpiScale);

    /// <summary>
    /// DPI缩放比例
    /// </summary>
    public static float DpiScale { get; private set; } = 1.0f;

    /// <summary>
    /// 初始化DPI缩放比例
    /// </summary>
    public static void InitDpiScale()
    {
        // Windows 10 Creators Update (1703) 及以上支持 GetDpiForSystem
        if (Environment.OSVersion.Version.Major >= 10)
        {
            DpiScale = GetDpiForSystem() / 96f;
        }
        else
        {
            // 兼容旧系统
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            DpiScale = g.DpiX / 96f;
        }
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForSystem();
}