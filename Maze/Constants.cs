using System.Runtime.InteropServices;

namespace Maze;

/// <summary>
/// 常量类, 提供一些常用的常量
/// </summary>
internal static partial class Constants
{
    /// <summary>
    /// 数据存储路径
    /// </summary>
    public static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MazeData");

    /// <summary>
    /// 错误文件路径
    /// </summary>
    public static readonly string ErrorFilePath = Path.Combine(DataPath, "error.log");

    /// <summary>
    /// 迷宫的宽度
    /// </summary>
    public const int MazeWidth = 125;

    /// <summary>
    /// 迷宫的高度
    /// </summary>
    public const int MazeHeight = 80;

    /// <summary>
    /// 随机生成迷宫的障碍物比例
    /// </summary>
    public const float ObstacleRatio = 0.3f;

    /// <summary>
    /// 取消搜索时最大等待时间
    /// </summary>
    public static readonly TimeSpan MaxCancellationWaitTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 空格的颜色
    /// </summary>
    public static readonly Color EmptyColor = Color.LightGray;

    /// <summary>
    /// 起点的颜色
    /// </summary>
    public static readonly Color StartColor = Color.SkyBlue;

    /// <summary>
    /// 终点的颜色
    /// </summary>
    public static readonly Color EndColor = Color.SkyBlue;

    /// <summary>
    /// 障碍物的颜色
    /// </summary>
    public static readonly Color ObstacleColor = Color.Red;

    /// <summary>
    /// 路径的颜色
    /// </summary>
    public static readonly Color PathColor = Color.Green;

    /// <summary>
    /// 淡出路径的颜色
    /// </summary>
    public static readonly Color FadePathColor = Color.LightGreen;

    /// <summary>
    /// 主窗体的宽度
    /// </summary>
    public static int MainFormWidth => (int)(1263 * DpiScale);

    /// <summary>
    /// 主窗体的高度
    /// </summary>
    public static int MainFormHeight => (int)(893 * DpiScale);

    /// <summary>
    /// 主信息面板的高度
    /// </summary>
    public static int MainInfoPanelHeight => (int)(40 * DpiScale);

    /// <summary>
    /// 迷宫单元格的大小
    /// </summary>
    public static int GridSize => (int)(10 * DpiScale);

    /// <summary>
    /// 按钮的宽度
    /// </summary>
    public static int ButtonWidth => (int)(100 * DpiScale);

    /// <summary>
    /// 按钮的高度
    /// </summary>
    public static int ButtonHeight => (int)(30 * DpiScale);

    /// <summary>
    /// 按钮的Y位置
    /// </summary>
    public static int ButtonY => (int)(5 * DpiScale);

    /// <summary>
    /// 按钮的间距
    /// </summary>
    public static int ButtonSpacing => (int)(10 * DpiScale);

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
