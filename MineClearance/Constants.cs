namespace MineClearance
{
    /// <summary>
    /// 提供一些常量
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 数据存储路径
        /// </summary>
        public readonly static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MineClearanceData");

        /// <summary>
        /// 数据文件路径
        /// </summary>
        public readonly static string DataFilePath = Path.Combine(DataPath, "history.json");

        /// <summary>
        /// 错误文件路径
        /// </summary>
        public readonly static string ErrorFilePath = Path.Combine(DataPath, "error.log");

        /// <summary>
        /// 简单、中等、困难的棋盘大小和地雷数量
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
                    _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
                };
            }
        }

        /// <summary>
        /// 自定义棋盘最大宽度和高度
        /// </summary>
        public static class CustomBoardSettings
        {
            public static (int maxWidth, int maxHeight) GetMaxDimensions()
            {
                return (50, 30);
            }
        }

        /// <summary>
        /// 雷的密度阈值, 大于等于此值时, 不确保地雷周围全是地雷的情况不会出现
        /// </summary>
        public static readonly float mineDensityThreshold = 0.3f;

        /// <summary>
        /// 按钮大小
        /// </summary>
        public static readonly int buttonSize = 25;

        /// <summary>
        /// 自动更新的URL
        /// </summary>
        public static readonly string AutoUpdateUrl = "https://gitee.com/xiting910/mine-clearance/raw/main/AutoUpdater.xml";

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
        public static readonly int UpdateSpeedRefreshInterval = 300;

        /// <summary>
        /// http请求超时时间(秒)
        /// </summary>
        public static readonly int HttpRequestTimeout = 15;

        /// <summary>
        /// 无进度一定秒后重试
        /// </summary>
        public static readonly int NoProgressRetryInterval = 20;

        /// <summary>
        /// 无进度弹窗等待时间(秒)
        /// </summary>
        public static readonly int NoProgressDialogWaitTime = 5;

        /// <summary>
        /// 无进度最大重试次数
        /// </summary>
        public static readonly int NoProgressMaxRetries = 3;
    }
}