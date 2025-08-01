namespace MineClearance;

/// <summary>
/// 表示扫雷游戏中格子的类型
/// </summary>
public enum GridType
{
    /// <summary>
    /// 未打开的格子
    /// </summary>
    Unopened,

    /// <summary>
    /// 空白格子, 表示没有地雷且周围没有地雷
    /// </summary>
    Empty,

    /// <summary>
    /// 数字格子, 表示周围地雷的数量
    /// </summary>
    Number,

    /// <summary>
    /// 标记的地雷
    /// </summary>
    Flagged,

    /// <summary>
    /// 地雷
    /// </summary>
    Mine
}

/// <summary>
/// 难度级别
/// </summary>
public enum DifficultyLevel
{
    /// <summary>
    /// 简单
    /// </summary>
    Easy,

    /// <summary>
    /// 中等
    /// </summary>
    Medium,

    /// <summary>
    /// 困难
    /// </summary>
    Hard,

    /// <summary>
    /// 地狱
    /// </summary>
    Hell,

    /// <summary>
    /// 自定义
    /// </summary>
    Custom
}

/// <summary>
/// 要显示的面板
/// </summary>
public enum PanelType
{
    /// <summary>
    /// 菜单面板
    /// </summary>
    Menu,

    /// <summary>
    /// 游戏准备面板
    /// </summary>
    GamePrepare,

    /// <summary>
    /// 游戏面板
    /// </summary>
    Game,

    /// <summary>
    /// 排行榜面板
    /// </summary>
    Ranking
}

/// <summary>
/// 底部状态栏状态
/// </summary>
public enum StatusBarState
{
    /// <summary>
    /// 就绪
    /// </summary>
    Ready,

    /// <summary>
    /// 排行榜
    /// </summary>
    Ranking,

    /// <summary>
    /// 准备游戏
    /// </summary>
    Preparing,

    /// <summary>
    /// 游戏进行中
    /// </summary>
    InGame,

    /// <summary>
    /// 游戏胜利
    /// </summary>
    GameWon,

    /// <summary>
    /// 游戏失败
    /// </summary>
    GameLost
}

/// <summary>
/// 排行榜显示模式
/// </summary>
public enum RankingDisplayMode
{
    /// <summary>
    /// 默认, 统计所有游戏结果, 计算各难度胜率等
    /// </summary>
    Default,

    /// <summary>
    /// 按照开始时间排序
    /// </summary>
    ByStartTime,

    /// <summary>
    /// 按照用时排序
    /// </summary>
    ByDuration
}