namespace CleanUpRegedit;

/// <summary>
/// 日志记录器类
/// </summary>
[SupportedOSPlatform("windows")]
internal static class Logger
{
    /// <summary>
    /// 日志文件夹路径
    /// </summary>
    private static readonly string LogFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CleanUpRegedit");

    /// <summary>
    /// 日志文件夹 DirectoryInfo 实例
    /// </summary>
    private static readonly DirectoryInfo LogFolder = new(LogFolderPath);

    /// <summary>
    /// 当前日志完整路径
    /// </summary>
    public static readonly string CurrentLogFilePath = Path.Combine(LogFolderPath, GetLogFileName());

    /// <summary>
    /// 获取日志文件最新命名
    /// </summary>
    private static string GetLogFileName()
    {
        // 当前日期字符串
        var dateString = $"{DateTime.Now:yyyy_MM_dd}";

        // 确保日志文件夹路径存在
        if (!LogFolder.Exists)
        {
            return $"{dateString}_1.log";
        }

        // 获取当天已有日志文件的最大索引
        var maxIndex = LogFolder.GetFiles($"{dateString}_*.log")
            .Select(file => Path.GetFileNameWithoutExtension(file.Name))
            .Select(name => name.Split('_').Last())
            .Select(indexStr => int.TryParse(indexStr, out var index) ? index : 0)
            .DefaultIfEmpty()
            .Max();

        // 返回新的日志文件名
        return $"{dateString}_{maxIndex + 1}.log";
    }

    /// <summary>
    /// 删除注册表子键并自动记录日志
    /// </summary>
    /// <param name="key">注册表键</param>
    /// <param name="subKey">子键名称</param>
    public static void DeleteSubKeyWithLogging(this RegistryKey key, string subKey)
    {
        // 确保日志文件夹路径存在
        if (!LogFolder.Exists)
        {
            LogFolder.Create();
        }

        try
        {
            key.DeleteSubKeyTree(subKey);
            var content = $"删除注册表项: {key.Name}\\{subKey}";
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {content}{Environment.NewLine}";
            File.AppendAllText(CurrentLogFilePath, logEntry);
            content.ColorfulWriteLine(ConsoleColor.DarkGreen);
        }
        catch (Exception ex)
        {
            var content = $"删除注册表项 {key.Name}\\{subKey} 时发生错误: {ex.Message}";
            var errorEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {content}{Environment.NewLine}";
            File.AppendAllText(CurrentLogFilePath, errorEntry);
            content.ColorfulWriteLine(ConsoleColor.Red);
        }
    }

    /// <summary>
    /// 彩色输出文本
    /// </summary>
    /// <param name="message">要输出的文本</param>
    /// <param name="color">文本颜色</param>
    public static void ColorfulWrite(this string? message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }

    /// <summary>
    /// 彩色输出文本并换行
    /// </summary>
    /// <param name="message">要输出的文本</param>
    /// <param name="color">文本颜色</param>
    public static void ColorfulWriteLine(this string? message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
