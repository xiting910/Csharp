namespace MineClearance.Models;

/// <summary>
/// 带有版本号和最后更新时间的数据类.
/// 旧版本只有游戏结果列表, 且自定义难度的Difficulty为3, 称为版本0.
/// 版本1开始引入了GameData类, 包含了最后更新时间和版本号, 同时增加地狱难度, 地狱难度的Difficulty为3, 自定义难度的Difficulty变为4.
/// 版本2开始, 非自定义难度的宽度、高度和地雷数量都不再保存(为null).
/// 版本3开始, 胜利时不再保存完成度(为null).
/// 版本4开始, 游戏结果保存按时间倒序排列.
/// </summary>
internal sealed record GameData
{
    /// <summary>
    /// 数据版本号
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdate { get; init; }

    /// <summary>
    /// 游戏结果列表
    /// </summary>
    public required List<GameResult> GameResults { get; init; }
}