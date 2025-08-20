using System.Runtime.InteropServices;

namespace WindowsUpdateManager;

/// <summary>
/// 方法类, 提供程序中常用的方法
/// </summary>
internal static class Methods
{
    /// <summary>
    /// 检测是否为 Windows 操作系统, 且为 Windows 10 及以上版本
    /// </summary>
    /// <returns>如果满足条件则返回true, 否则返回false</returns>
    public static bool IsWindows10OrGreater()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version.Major >= 10;
    }

    /// <summary>
    /// 记录异常到日志文件
    /// </summary>
    /// <param name="ex">要记录的异常</param>
    public static void LogException(Exception ex)
    {
        var log = $"[{DateTime.Now}] {ex}\n";
        try
        {
            if (!Directory.Exists(Constants.DataPath))
            {
                _ = Directory.CreateDirectory(Constants.DataPath);
            }
            File.AppendAllText(Constants.ErrorFilePath, log);
        }
        catch { /* 忽略日志写入异常 */ }
    }
}