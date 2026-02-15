using System.Runtime.InteropServices;

namespace WindowsUpdateManager;

/// <summary>
/// 常量类, 提供一些程序使用的常量
/// </summary>
internal static partial class Constants
{
    /// <summary>
    /// 应用程序数据路径
    /// </summary>
    public static string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WindowsUpdateManagerData");

    /// <summary>
    /// 错误日志文件路径
    /// </summary>
    public static string ErrorFilePath => Path.Combine(DataPath, "error.log");

    /// <summary>
    /// 配置文件路径
    /// </summary>
    public static string ConfigFilePath => Path.Combine(DataPath, "config.json");

    /// <summary>
    /// 程序名字
    /// </summary>
    public const string ProgramName = "Windows更新管理器";

    /// <summary>
    /// 当前配置版本
    /// </summary>
    public const int CurrentConfigVersion = 1;

    /// <summary>
    /// 暂停更新的最晚日期
    /// </summary>
    public static DateTime MaxPauseUpdateDate => new(3001, 1, 1);

    /// <summary>
    /// 主窗体的最小宽度
    /// </summary>
    public static int MainFormMinWidth => (int)(700 * DpiScale);

    /// <summary>
    /// 主窗体的最小高度
    /// </summary>
    public static int MainFormMinHeight => (int)(450 * DpiScale);

    /// <summary>
    /// DPI 缩放比例的字段
    /// </summary>
    private static float? _dpiScale;

    /// <summary>
    /// DPI 缩放比例, 如果未初始化则抛出异常
    /// </summary>
    public static float DpiScale => _dpiScale ?? throw new InvalidOperationException("DPI 未初始化");

    /// <summary>
    /// 初始化DPI缩放比例
    /// </summary>
    public static void InitDpiScale()
    {
        // Windows 10 Creators Update (1703) 及以上支持 GetDpiForSystem
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            _dpiScale = GetDpiForSystem() / 96f;
        }
        else
        {
            // 兼容旧系统
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            _dpiScale = g.DpiX / 96f;
        }
    }

    /// <summary>
    /// 获取系统DPI
    /// </summary>
    /// <returns>系统DPI</returns>
    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForSystem();
}
