namespace MineClearance;

/// <summary>
/// 常量类, 提供一些常量
/// </summary>
public static class Constants
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
    /// 自动更新的URL
    /// </summary>
    public const string AutoUpdateUrl = "https://gitee.com/xiting910/mine-clearance/raw/main/AutoUpdater.xml";

    /// <summary>
    /// 更新文件路径
    /// </summary>
    public static readonly string SevenZipPath = Path.Combine(Path.GetTempPath(), "MineClearance.7z");

    /// <summary>
    /// 用于更新程序的powershell脚本路径
    /// </summary>
    public static readonly string UpdatePowerShellScriptPath = Path.Combine(Path.GetTempPath(), "UpdateScript.ps1");

    /// <summary>
    /// 当前程序的目录
    /// </summary>
    public static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

    /// <summary>
    /// 当前程序的可执行文件名
    /// </summary>
    public static readonly string ExecutableFileName = Path.GetFileName(Application.ExecutablePath);

    /// <summary>
    /// 当前程序的完整路径
    /// </summary>
    public static readonly string ExecutableFilePath = Path.Combine(CurrentDirectory, ExecutableFileName);

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
    /// 简单、中等、困难和地狱的棋盘大小和地雷数量
    /// </summary>
    public static class BoardSettings
    {
        public static (int width, int height, int mineCount) GetSettings(DifficultyLevel level)
        {
            return level switch
            {
                DifficultyLevel.Easy => (9, 9, 10),
                DifficultyLevel.Medium => (16, 16, 40),
                DifficultyLevel.Hard => (30, 16, 99),
                DifficultyLevel.Hell => (50, 30, 300),
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }
    }

    /// <summary>
    /// 主窗体的宽度
    /// </summary>
    public const int MainFormWidth = 2527;

    /// <summary>
    /// 主窗体的高度
    /// </summary>
    public const int MainFormHeight = 1703;

    /// <summary>
    /// 底部状态栏的高度
    /// </summary>
    public const int BottomStatusBarHeight = 110;

    /// <summary>
    /// 扫雷棋盘的最大宽度
    /// </summary>
    public const int MaxBoardWidth = 50;

    /// <summary>
    /// 扫雷棋盘的最大高度
    /// </summary>
    public const int MaxBoardHeight = 30;

    /// <summary>
    /// 网格大小
    /// </summary>
    public const int GridSize = 50;
}