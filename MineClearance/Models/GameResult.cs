namespace MineClearance.Models;

/// <summary>
/// 表示扫雷游戏的结果
/// </summary>
/// <param name="difficulty">游戏难度级别</param>
/// <param name="startTime">游戏开始时间</param>
/// <param name="duration">游戏持续时间</param>
/// <param name="isWin">游戏是否获胜</param>
/// <param name="completion">游戏完成度, 取值范围 0 到 99, null 表示胜利</param>
/// <param name="boardWidth">棋盘宽度</param>
/// <param name="boardHeight">棋盘高度</param>
/// <param name="mineCount">地雷总数</param>
internal sealed class GameResult(DifficultyLevel difficulty, DateTime startTime, TimeSpan duration, bool isWin, double? completion = null, int? boardWidth = null, int? boardHeight = null, int? mineCount = null)
{
    /// <summary>
    /// 游戏的难度级别
    /// </summary>
    public DifficultyLevel Difficulty { get; private init; } = difficulty;

    /// <summary>
    /// 游戏的开始时间
    /// </summary>
    public DateTime StartTime { get; private init; } = startTime;

    /// <summary>
    /// 游戏时间
    /// </summary>
    public TimeSpan Duration { get; private init; } = duration;

    /// <summary>
    /// 游戏的结果(是否获胜)
    /// </summary>
    public bool IsWin { get; private init; } = isWin;

    /// <summary>
    /// 游戏完成度, 取值范围 0 到 99, null 表示胜利
    /// </summary>
    public double? Completion { get; private init; } = isWin ? null : completion;

    /// <summary>
    /// 棋盘宽度
    /// </summary>
    public int? BoardWidth { get; private init; } = difficulty == DifficultyLevel.Custom ? boardWidth : null;

    /// <summary>
    /// 棋盘高度
    /// </summary>
    public int? BoardHeight { get; private init; } = difficulty == DifficultyLevel.Custom ? boardHeight : null;

    /// <summary>
    /// 地雷数量
    /// </summary>
    public int? MineCount { get; private init; } = difficulty == DifficultyLevel.Custom ? mineCount : null;

    /// <summary>
    /// 重写==操作符, 用于比较两个游戏结果是否相等
    /// </summary>
    /// <param name="left">左侧游戏结果</param>
    /// <param name="right">右侧游戏结果</param>
    /// <returns>是否相等</returns>
    public static bool operator ==(GameResult? left, GameResult? right)
    {
        return (left is null && right is null) || (left is not null && left.Equals(right));
    }

    /// <summary>
    /// 重写!=操作符, 用于比较两个游戏结果是否不相等
    /// </summary>
    /// <param name="left">左侧游戏结果</param>
    /// <param name="right">右侧游戏结果</param>
    /// <returns>是否不相等</returns>
    public static bool operator !=(GameResult? left, GameResult? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 重写Equals方法, 用于比较两个游戏结果是否相等
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>是否相等</returns>
    public override bool Equals(object? obj)
    {
        return obj is GameResult other && StartTime == other.StartTime;
    }

    /// <summary>
    /// 获取对象的哈希码
    /// </summary>
    /// <returns>对象的哈希码</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Difficulty, StartTime, Duration, IsWin, Completion, BoardWidth, BoardHeight, MineCount);
    }
}

/// <summary>
/// 游戏结果比较器, 用于根据不同属性进行排序
/// </summary>
/// <param name="propertyName">属性名称</param>
/// <param name="sortOrder">排序顺序</param>
internal sealed class GameResultComparer(string propertyName, SortOrder sortOrder) : IComparer<GameResult>
{
    /// <summary>
    /// 属性名称
    /// </summary>
    private readonly string _propertyName = propertyName;

    /// <summary>
    /// 排序顺序
    /// </summary>
    private readonly SortOrder _sortOrder = sortOrder;

    /// <summary>
    /// 比较两个游戏结果对象
    /// </summary>
    /// <param name="x">第一个游戏结果对象</param>
    /// <param name="y">第二个游戏结果对象</param>
    /// <returns>比较结果</returns>
    /// <exception cref="ArgumentException">如果属性名不支持</exception>
    public int Compare(GameResult? x, GameResult? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        var result = _propertyName switch
        {
            "难度" or "Difficulty" => x.Difficulty == DifficultyLevel.Custom && y.Difficulty != DifficultyLevel.Custom ? -1 : x.Difficulty != DifficultyLevel.Custom && y.Difficulty == DifficultyLevel.Custom ? 1 : x.Difficulty.CompareTo(y.Difficulty),
            "开始时间" or "StartTime" => x.StartTime.CompareTo(y.StartTime),
            "用时" or "Duration" => x.Duration.CompareTo(y.Duration),
            "结果" or "IsWin" => x.IsWin.CompareTo(y.IsWin),
            "完成度" or "Completion" => x.Completion == y.Completion ? 0 : x.Completion is null ? 1 : y.Completion is null ? -1 : x.Completion.Value.CompareTo(y.Completion.Value),
            _ => throw new ArgumentException($"不支持的属性名: {_propertyName}"),
        };
        return _sortOrder == SortOrder.Ascending ? result : -result;
    }
}