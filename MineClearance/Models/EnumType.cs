namespace MineClearance;

/// <summary>
/// 格子类型枚举
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
    /// 警告数字格子, 表示玩家在其周围插旗的数量超过了实际地雷数量
    /// </summary>
    WarningNumber,

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
/// 难度级别枚举
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
/// 面板类型枚举
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
    /// 历史记录面板
    /// </summary>
    History
}

/// <summary>
/// 底部状态栏状态枚举
/// </summary>
public enum StatusBarState
{
    /// <summary>
    /// 就绪
    /// </summary>
    Ready,

    /// <summary>
    /// 历史记录
    /// </summary>
    History,

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