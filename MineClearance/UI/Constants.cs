using System.Runtime.InteropServices;
using MineClearance.Models;

namespace MineClearance.UI;

/// <summary>
/// UI常量类, 提供一些UI相关的常量
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// 自动更新的URL
    /// </summary>
    public const string AutoUpdateUrl = "https://gitee.com/xiting910/mine-clearance/raw/main/AutoUpdater.xml";

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

    /// <summary>
    /// 获取系统DPI
    /// </summary>
    /// <returns>系统DPI</returns>
    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForSystem();
}