namespace MineClearance;

/// <summary>
/// 带有版本号和最后更新时间的数据类.
/// 旧版本只有游戏结果列表, 且自定义难度的Difficulty为3, 称为版本0.
/// 版本1开始引入了GameData类, 包含了最后更新时间和版本号, 同时增加地狱难度, 地狱难度的Difficulty为3, 自定义难度的Difficulty变为4.
/// 版本2开始, 非自定义难度的宽度、高度和地雷数量都不再保存(为null).
/// </summary>
/// <param name="LastUpdate">最后更新时间</param>
/// <param name="GameResults">游戏结果列表</param>
/// <param name="Version">数据版本</param>
public record GameData(DateTime LastUpdate, List<GameResult> GameResults, int Version = 0)
{
    /// <summary>
    /// 数据版本号
    /// </summary>
    public int Version { get; private init; } = Version;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdate { get; private init; } = LastUpdate;

    /// <summary>
    /// 游戏结果列表
    /// </summary>
    public List<GameResult> GameResults { get; private init; } = GameResults;
}