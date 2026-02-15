namespace WindowsUpdateManager;

/// <summary>
/// 方法类, 提供程序中常用的方法
/// </summary>
internal static class Methods
{
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
