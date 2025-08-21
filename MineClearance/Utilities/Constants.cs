using MineClearance.Models;

namespace MineClearance.Utilities;

/// <summary>
/// 常量类, 提供一些程序通用的常量
/// </summary>
internal static class Constants
{
    /// <summary>
    /// 数据存储路径
    /// </summary>
    public static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MineClearanceData");

    /// <summary>
    /// 数据文件路径
    /// </summary>
    public static readonly string DataFilePath = Path.Combine(DataPath, "history.json");

    /// <summary>
    /// 错误文件路径
    /// </summary>
    public static readonly string ErrorFilePath = Path.Combine(DataPath, "error.log");

    /// <summary>
    /// 配置文件路径
    /// </summary>
    public static readonly string ConfigFilePath = Path.Combine(DataPath, "config.json");

    /// <summary>
    /// 当前程序的完整路径
    /// </summary>
    public static readonly string ExecutableFilePath = Application.ExecutablePath;

    /// <summary>
    /// 当前程序的可执行文件名
    /// </summary>
    public static readonly string ExecutableFileName = Path.GetFileName(ExecutableFilePath);

    /// <summary>
    /// 扫雷棋盘的最大宽度
    /// </summary>
    public const int MaxBoardWidth = 50;

    /// <summary>
    /// 扫雷棋盘的最大高度
    /// </summary>
    public const int MaxBoardHeight = 30;

    /// <summary>
    /// 获取指定难度的棋盘设置
    /// </summary>
    /// <param name="level">难度级别</param>
    /// <returns>棋盘宽度、高度和地雷数量的元组</returns>
    /// <exception cref="ArgumentException">如果难度级别为自定义, 则抛出异常</exception>
    /// <exception cref="ArgumentOutOfRangeException">如果难度级别不在预定义范围内</exception>
    public static (int width, int height, int mineCount) GetSettings(DifficultyLevel level)
    {
        return level switch
        {
            DifficultyLevel.Easy => (9, 9, 10),
            DifficultyLevel.Medium => (16, 16, 40),
            DifficultyLevel.Hard => (30, 16, 99),
            DifficultyLevel.Hell => (50, 30, 309),
            DifficultyLevel.Custom => throw new ArgumentException("自定义难度需要手动设置棋盘参数", nameof(level)),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "未知的难度级别")
        };
    }
}