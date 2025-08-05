namespace MineClearance;

/// <summary>
/// 表示扫雷游戏的结果
/// </summary>
/// <param name="Difficulty">游戏难度级别</param>
/// <param name="StartTime">游戏开始时间（首次点击时间）</param>
/// <param name="Duration">游戏持续时间</param>
/// <param name="IsWin">游戏是否获胜</param>
/// <param name="Completion">游戏完成度，取值范围 0 到 100</param>
/// <param name="BoardWidth">棋盘宽度</param>
/// <param name="BoardHeight">棋盘高度</param>
/// <param name="MineCount">地雷总数</param>
public record GameResult(DifficultyLevel Difficulty, DateTime StartTime, TimeSpan Duration, bool IsWin, double Completion, int? BoardWidth = null, int? BoardHeight = null, int? MineCount = null)
{
    /// <summary>
    /// 游戏的开始时间(首次点击时间)
    /// </summary>
    public DateTime StartTime { get; private init; } = StartTime;

    /// <summary>
    /// 游戏时间
    /// </summary>
    public TimeSpan Duration { get; private init; } = Duration;

    /// <summary>
    /// 游戏的结果（胜利或失败）
    /// </summary>
    public bool IsWin { get; private init; } = IsWin;

    /// <summary>
    /// 游戏完成度, 取值范围 0 到 100
    /// </summary>
    public double Completion { get; private init; } = Completion;

    /// <summary>
    /// 棋盘宽度
    /// </summary>
    public int? BoardWidth { get; private init; } = Difficulty == DifficultyLevel.Custom ? BoardWidth : null;

    /// <summary>
    /// 棋盘高度
    /// </summary>
    public int? BoardHeight { get; private init; } = Difficulty == DifficultyLevel.Custom ? BoardHeight : null;

    /// <summary>
    /// 地雷数量
    /// </summary>
    public int? MineCount { get; private init; } = Difficulty == DifficultyLevel.Custom ? MineCount : null;

    /// <summary>
    /// 获取游戏结果的格式化字符串表示
    /// </summary>
    /// <returns>游戏结果的格式化字符串表示</returns>
    public override string ToString()
    {
        // 格式化难度名称
        var formattedDifficulty = Difficulty switch
        {
            DifficultyLevel.Easy => "简单",
            DifficultyLevel.Medium => "普通",
            DifficultyLevel.Hard => "困难",
            DifficultyLevel.Hell => "地狱",
            DifficultyLevel.Custom => "自定",
            _ => "未知"
        };

        // 完成度格式化为百分比, 保留两位小数
        var formattedCompletion = $"{Completion,6:0.00}%";

        // 格式化用时为 xx:xx.xx 格式
        var formattedDuration = $"{(int)Duration.TotalMinutes:D2}:{Duration.Seconds:D2}.{Duration.Milliseconds / 10:D2}";

        // 构建最终的格式化字符串
        var formattedMessage = $"开始时间: {StartTime}, 难度: {formattedDifficulty}, 结果: {(IsWin ? "胜利" : "失败")}, 完成度: {formattedCompletion}, 用时: {formattedDuration}";
        if (Difficulty == DifficultyLevel.Custom)
        {
            formattedMessage += $", 宽: {BoardWidth}, 高: {BoardHeight}, 地雷数: {MineCount}";
        }

        // 返回游戏结果的格式化字符串表示
        return formattedMessage;
    }
}