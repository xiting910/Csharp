using MineClearance.Models;

namespace MineClearance.Utilities;

/// <summary>
/// 方法类, 提供一些常用方法
/// </summary>
internal static partial class Methods
{
    /// <summary>
    /// 检测是否为Windows操作系统
    /// </summary>
    /// <returns>如果是Windows操作系统则返回true，否则返回false</returns>
    public static bool IsWindows()
    {
        return Environment.OSVersion.Platform == PlatformID.Win32NT;
    }

    /// <summary>
    /// 根据难度返回对应的文本
    /// </summary>
    /// <param name="difficulty">难度枚举值</param>
    /// <returns>返回对应的难度文本</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果难度未知则抛出异常</exception>
    public static string GetDifficultyText(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => "简单",
            DifficultyLevel.Medium => "普通",
            DifficultyLevel.Hard => "困难",
            DifficultyLevel.Hell => "地狱",
            DifficultyLevel.Custom => "自定义",
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), "未知的难度")
        };
    }

    /// <summary>
    /// 根据当前要排序的游戏结果属性获取对应的优先级
    /// </summary>
    /// <param name="propertyName">要排序的属性名称</param>
    /// <returns>返回对应的优先级</returns>
    /// <exception cref="ArgumentException">如果属性名不支持</exception>
    public static int GetSortPriority(string propertyName)
    {
        return propertyName switch
        {
            "难度" or "Difficulty" => 1,
            "开始时间" or "StartTime" => 4,
            "用时" or "Duration" => 3,
            "结果" or "IsWin" => 0,
            "完成度" or "Completion" => 2,
            _ => throw new ArgumentException($"不支持的属性名: {propertyName}", nameof(propertyName))
        };
    }

    /// <summary>
    /// 根据当前面板类型获取对应底部状态栏状态
    /// </summary>
    /// <param name="panelType">当前面板类型</param>
    /// <returns>返回对应的底部状态栏状态</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果面板类型未知则抛出异常</exception>
    public static StatusBarState GetBottomStatusBarState(PanelType panelType)
    {
        return panelType switch
        {
            PanelType.Menu => StatusBarState.Ready,
            PanelType.GamePrepare => StatusBarState.Preparing,
            PanelType.Game => StatusBarState.InGame,
            PanelType.History => StatusBarState.History,
            _ => throw new ArgumentOutOfRangeException(nameof(panelType), "未知的面板类型")
        };
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