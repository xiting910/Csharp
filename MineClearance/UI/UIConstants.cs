using System.Runtime.InteropServices;

namespace MineClearance.UI;

/// <summary>
/// UI 常量类, 提供一些 UI 相关的常量
/// </summary>
internal static partial class UIConstants
{
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
    public static int SettingFormMinHeight => (int)(150 * DpiScale);

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