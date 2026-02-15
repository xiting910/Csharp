namespace Maze;

/// <summary>
/// 底部状态栏状态
/// </summary>
internal enum StatusBarState
{
    /// <summary>
    /// 就绪状态
    /// </summary>
    Ready,

    /// <summary>
    /// 设置起点和终点状态
    /// </summary>
    SetStartAndEnd,

    /// <summary>
    /// 设置障碍物状态
    /// </summary>
    SetObstacle,

    /// <summary>
    /// 正在搜索路径状态
    /// </summary>
    SearchingPath
}

/// <summary>
/// 当前操作状态
/// </summary>
internal enum OperationStatus
{
    /// <summary>
    /// 默认状态
    /// </summary>
    Default,

    /// <summary>
    /// 设置起点和终点状态
    /// </summary>
    SetStartAndEnd,

    /// <summary>
    /// 设置障碍物状态
    /// </summary>
    SetObstacle
}

/// <summary>
/// 表示迷宫的格子类型
/// </summary>
internal enum GridType
{
    /// <summary>
    /// 空白格子
    /// </summary>
    Empty,

    /// <summary>
    /// 起点
    /// </summary>
    Start,

    /// <summary>
    /// 终点
    /// </summary>
    End,

    /// <summary>
    /// 障碍物
    /// </summary>
    Obstacle,

    /// <summary>
    /// 路径
    /// </summary>
    Path,

    /// <summary>
    /// 淡出路径
    /// </summary>
    FadePath
}

/// <summary>
/// 表示迷宫的生成算法
/// </summary>
internal enum GenerationAlgorithm
{
    /// <summary>
    /// 完全随机生成
    /// </summary>
    Random,

    /// <summary>
    /// 深度优先生成
    /// </summary>
    DFS,

    /// <summary>
    /// 递归分割算法
    /// </summary>
    RecursiveDivision,

    /// <summary>
    /// Prim算法
    /// </summary>
    Prim
}

/// <summary>
/// 表示迷宫的搜索算法
/// </summary>
internal enum SearchAlgorithm
{
    /// <summary>
    /// 广度优先搜索
    /// </summary>
    BFS,

    /// <summary>
    /// 深度优先搜索
    /// </summary>
    DFS,

    /// <summary>
    /// A*搜索
    /// </summary>
    AStar
}

/// <summary>
/// 表示迷宫的方向(顺时针排列)
/// </summary>
internal enum Direction
{
    /// <summary>
    /// 上
    /// </summary>
    Up,

    /// <summary>
    /// 右
    /// </summary>
    Right,

    /// <summary>
    /// 下
    /// </summary>
    Down,

    /// <summary>
    /// 左
    /// </summary>
    Left
}
