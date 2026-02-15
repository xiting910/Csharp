using Microsoft.Win32;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;

namespace WindowsUpdateManager;

/// <summary>
/// 更新管理器类, 用于管理和处理 Windows 更新相关操作
/// </summary>
public static class UpdateManager
{
    /// <summary>
    /// Windows 更新设置注册表路径
    /// </summary>
    private const string WindowsUpdateSettingsRegistryPath = @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings";

    /// <summary>
    /// 系统服务注册表路径
    /// </summary>
    private const string SystemServicesRegistryPath = @"SYSTEM\CurrentControlSet\Services";

    /// <summary>
    /// 暂停更新开始时间注册表项的不可变集合
    /// </summary>
    private static readonly ImmutableArray<string> PauseUpdateStartTimeRegistryKeys = [
        "PauseUpdatesStartTime",
        "PauseFeatureUpdatesStartTime",
        "PauseQualityUpdatesStartTime"
    ];

    /// <summary>
    /// 暂停更新结束时间注册表项的不可变集合
    /// </summary>
    private static readonly ImmutableArray<string> PauseUpdateEndTimeRegistryKeys = [
        "PauseUpdatesExpiryTime",
        "PauseFeatureUpdatesEndTime",
        "PauseQualityUpdatesEndTime"
    ];

    /// <summary>
    /// Windows 更新服务列表及其默认值的不可变字典
    /// </summary>
    private static readonly ImmutableDictionary<string, int> UpdateServices = new Dictionary<string, int> { { "UsoSvc", 2 }, { "wuauserv", 3 }, { "WaaSMedicSvc", 3 } }.ToImmutableDictionary();

    /// <summary>
    /// Windows 更新计划任务
    /// </summary>
    private const string UpdateTask = @"\Microsoft\Windows\WindowsUpdate\Scheduled Start";

    /// <summary>
    /// 打开 Windows 更新设置注册表项
    /// </summary>
    /// <returns>Windows 更新设置注册表项的句柄</returns>
    /// <exception cref="System.Security.SecurityException">如果没有足够的权限访问注册表</exception>
    /// <exception cref="InvalidOperationException">如果无法打开注册表项</exception>
    private static RegistryKey OpenWindowsUpdateSettingsKey() => Registry.LocalMachine.OpenSubKey(WindowsUpdateSettingsRegistryPath, true) ?? throw new InvalidOperationException("无法打开 Windows 更新设置注册表项");

    /// <summary>
    /// 打开系统服务注册表项
    /// </summary>
    /// <returns>系统服务注册表项的句柄</returns>
    /// <exception cref="System.Security.SecurityException">如果没有足够的权限访问注册表</exception>
    /// <exception cref="InvalidOperationException">如果无法打开注册表项</exception>
    private static RegistryKey OpenSystemServicesKey() => Registry.LocalMachine.OpenSubKey(SystemServicesRegistryPath, true) ?? throw new InvalidOperationException("无法打开系统服务注册表项");

    /// <summary>
    /// 打开系统更新页面
    /// </summary>
    public static void OpenWindowsUpdatePage() => _ = Process.Start(new ProcessStartInfo
    {
        FileName = "ms-settings:windowsupdate",
        UseShellExecute = true
    });

    /// <summary>
    /// 重启计算机
    /// </summary>
    /// <exception cref="InvalidOperationException">如果重启计算机失败</exception>
    public static void RestartComputer()
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "shutdown",
            Arguments = "/r /t 0",
            CreateNoWindow = true,
            UseShellExecute = false,
            Verb = "runas"
        };

        try
        {
            _ = Process.Start(processInfo);
            Application.Exit();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("重启计算机失败", ex);
        }
    }

    /// <summary>
    /// 暂停更新到指定时间
    /// </summary>
    /// <param name="endTime">暂停更新的结束时间</param>
    /// <exception cref="InvalidOperationException">如果暂停更新失败</exception>
    public static void PauseUpdateUntil(DateTime endTime)
    {
        try
        {
            using var settingsKey = OpenWindowsUpdateSettingsKey();

            foreach (var key in PauseUpdateStartTimeRegistryKeys)
            {
                settingsKey.SetValue(key, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), RegistryValueKind.String);
            }

            foreach (var key in PauseUpdateEndTimeRegistryKeys)
            {
                settingsKey.SetValue(key, endTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), RegistryValueKind.String);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("暂停更新失败", ex);
        }
    }

    /// <summary>
    /// 禁用 Windows 更新相关服务
    /// </summary>
    /// <exception cref="InvalidOperationException">如果禁用服务失败</exception>
    public static void DisableUpdateServices()
    {
        try
        {
            using var serviceKey = OpenSystemServicesKey();

            foreach (var service in UpdateServices.Keys)
            {
                using var key = serviceKey.OpenSubKey(service, true) ?? throw new InvalidOperationException($"无法打开服务 {service} 的注册表项");
                key.SetValue("Start", 4, RegistryValueKind.DWord);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"禁用服务时出错", ex);
        }
    }

    /// <summary>
    /// 禁用 Windows 更新相关计划
    /// </summary>
    /// <exception cref="InvalidOperationException">如果禁用计划失败</exception>
    public static void DisableUpdateTasks()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/Change /TN \"{UpdateTask}\" /DISABLE",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas"
            };

            using var process = Process.Start(processInfo) ?? throw new InvalidOperationException("无法启动 schtasks 进程");

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"{error} (退出代码: {process.ExitCode})");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"禁用任务失败", ex);
        }
    }

    /// <summary>
    /// 恢复更新
    /// </summary>
    /// <exception cref="InvalidOperationException">如果恢复暂停更新失败</exception>
    public static void ResumeUpdate()
    {
        try
        {
            using var settingsKey = OpenWindowsUpdateSettingsKey();

            foreach (var key in PauseUpdateStartTimeRegistryKeys)
            {
                settingsKey.DeleteValue(key, false);
            }

            foreach (var key in PauseUpdateEndTimeRegistryKeys)
            {
                settingsKey.DeleteValue(key, false);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("恢复暂停更新失败", ex);
        }

        ResumeUpdateServices();
        ResumeUpdateTasks();
    }

    /// <summary>
    /// 恢复 Windows 更新相关服务
    /// </summary>
    /// <exception cref="InvalidOperationException">如果恢复服务失败</exception>
    private static void ResumeUpdateServices()
    {
        try
        {
            using var serviceKey = OpenSystemServicesKey();

            foreach (var service in UpdateServices)
            {

                using var key = serviceKey.OpenSubKey(service.Key, true) ?? throw new InvalidOperationException($"无法打开服务 {service} 的注册表项");
                key.SetValue("Start", service.Value, RegistryValueKind.DWord);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"恢复服务时出错", ex);
        }
    }

    /// <summary>
    /// 恢复 Windows 更新相关计划任务
    /// </summary>
    /// <exception cref="InvalidOperationException">如果恢复计划任务失败</exception>
    private static void ResumeUpdateTasks()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/Change /TN \"{UpdateTask}\" /ENABLE",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas"
            };

            using var process = Process.Start(processInfo) ?? throw new InvalidOperationException("无法启动 schtasks 进程");

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"{error} (退出代码: {process.ExitCode})");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"恢复任务失败", ex);
        }
    }
}
