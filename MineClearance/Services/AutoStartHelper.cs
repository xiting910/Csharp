using Microsoft.Win32;
using MineClearance.Utilities;

namespace MineClearance.Services;

/// <summary>
/// 自动启动帮助类
/// </summary>
public static class AutoStartHelper
{
    /// <summary>
    /// RunKey 注册表路径
    /// </summary>
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// 应用程序名称
    /// </summary>
    private const string AppName = "MineClearance";

    /// <summary>
    /// 启用自动启动
    /// </summary>
    public static void EnableAutoStart()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true)!;
        key.SetValue(AppName, $"\"{Constants.ExecutableFilePath}\"");
    }

    /// <summary>
    /// 禁用自动启动
    /// </summary>
    public static void DisableAutoStart()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true)!;
        key.DeleteValue(AppName, false);
    }

    /// <summary>
    /// 检查是否启用自动启动
    /// </summary>
    public static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false)!;
        var value = key.GetValue(AppName) as string;
        return !string.IsNullOrEmpty(value);
    }
}