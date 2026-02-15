namespace Maze;

// 迷宫类的主体实现部分
/// <summary>
/// 迷宫类, 提供迷宫的随机生成和搜索功能
/// </summary>
internal static partial class Maze
{
    /// <summary>
    /// 格子状态改变事件
    /// </summary>
    public static event Action<Position>? OnGridTypeChanged;

    /// <summary>
    /// 刷新面板事件
    /// </summary>
    public static event Action? OnPanelRefreshed;

    /// <summary>
    /// 搜索状态改变事件
    /// </summary>
    public static event Action<bool>? OnSearchingChanged;

    /// <summary>
    /// 取消搜索令牌
    /// </summary>
    public static CancellationTokenSource CTS { get; set; }

    /// <summary>
    /// 当前是否正在搜索路径
    /// </summary>
    public static bool IsSearching
    {
        get => _isSearching;
        set
        {
            if (_isSearching != value)
            {
                _isSearching = value;
                OnSearchingChanged?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// 起点位置, 默认为(-1, -1)表示未设置
    /// </summary>
    public static Position StartPosition { get; set; }

    /// <summary>
    /// 终点位置, 默认为(-1, -1)表示未设置
    /// </summary>
    public static Position EndPosition { get; set; }

    /// <summary>
    /// 格子的二维数组, 用于存储迷宫的格子信息
    /// </summary>
    public static GridType[,] Grids { get; private set; }

    /// <summary>
    /// 当前是否在搜索路径
    /// </summary>
    private static volatile bool _isSearching;

    /// <summary>
    /// 静态构造函数, 初始化迷宫格子数组
    /// </summary>
    static Maze()
    {
        // 初始化搜索相关
        CTS = new();
        _isSearching = false;

        // 初始化迷宫格子数组
        Grids = new GridType[Constants.MazeHeight, Constants.MazeWidth];

        // 初始化起点和终点位置
        StartPosition = Position.Invalid;
        EndPosition = Position.Invalid;
    }

    /// <summary>
    /// 判断起点和终点是否都已设置
    /// </summary>
    /// <returns>如果都已设置则返回true, 否则返回false</returns>
    public static bool IsStartAndEndSet() => StartPosition != Position.Invalid && EndPosition != Position.Invalid;

    /// <summary>
    /// 确保当前没有正在搜索
    /// </summary>
    /// <returns>如果当前没有正在搜索则返回true, 否则返回false</returns>
    public static async Task<bool> EnsureNotSearching()
    {
        if (_isSearching)
        {
            // 弹出提示框, 提示用户当前正在搜索
            var result = MessageBox.Show("当前正在搜索迷宫, 请等待搜索完成后再进行其他操作\n或者您想要取消当前搜索吗?", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            // 用户不选择取消搜索
            if (result != DialogResult.Yes)
            {
                return false;
            }

            // 用户选择取消搜索, 则取消当前搜索
            CTS.Cancel();
            CTS.Dispose();

            // 等待搜索取消完成
            await WaitForCancellation();

            // 清除路径
            ClearPaths();
        }
        return true;
    }

    /// <summary>
    /// 等待搜索取消, 如果超过最大取消时间则抛出TimeoutException
    /// </summary>
    /// <exception cref="TimeoutException">如果等待超过最大取消时间</exception>
    public static async Task WaitForCancellation()
    {
        // 超时任务
        var timeoutTask = Task.Delay(Constants.MaxCancellationWaitTime);
        while (_isSearching)
        {
            var delayTask = Task.Delay(100);
            var completed = await Task.WhenAny(delayTask, timeoutTask);
            if (completed == timeoutTask)
            {
                // 超时, 抛出异常
                throw new TimeoutException("取消搜索操作超时");
            }
        }
    }

    /// <summary>
    /// 重置迷宫为初始状态
    /// </summary>
    /// <param name="resetStartAndEnd">是否重置起点和终点</param>
    public static async Task ResetMaze(bool resetStartAndEnd = true) => await Task.Run(() =>
                                                                             {
                                                                                 // 重置起点和终点位置
                                                                                 if (resetStartAndEnd)
                                                                                 {
                                                                                     StartPosition = Position.Invalid;
                                                                                     EndPosition = Position.Invalid;
                                                                                 }

                                                                                 // 清空迷宫格子数组
                                                                                 for (var i = 0; i < Constants.MazeHeight; ++i)
                                                                                 {
                                                                                     for (var j = 0; j < Constants.MazeWidth; ++j)
                                                                                     {
                                                                                         Grids[i, j] = GridType.Empty;
                                                                                     }
                                                                                 }

                                                                                 // 如果没有重置起点和终点, 且它们已设置, 则重新设置起点和终点格子
                                                                                 if (StartPosition != Position.Invalid)
                                                                                 {
                                                                                     Grids[StartPosition.Row, StartPosition.Col] = GridType.Start;
                                                                                 }
                                                                                 if (EndPosition != Position.Invalid)
                                                                                 {
                                                                                     Grids[EndPosition.Row, EndPosition.Col] = GridType.End;
                                                                                 }
                                                                             });

    /// <summary>
    /// 设置所有空格子为障碍物
    /// </summary>
    public static async Task SetAllObstacles() => await Task.Run(() =>
                                                       {
                                                           // 遍历所有格子, 将空格子设置为障碍物
                                                           for (var i = 0; i < Constants.MazeHeight; ++i)
                                                           {
                                                               for (var j = 0; j < Constants.MazeWidth; ++j)
                                                               {
                                                                   if (Grids[i, j] == GridType.Empty)
                                                                   {
                                                                       Grids[i, j] = GridType.Obstacle;
                                                                   }
                                                               }
                                                           }
                                                       });

    /// <summary>
    /// 清除所有已设置路径
    /// </summary>
    public static void ClearPaths()
    {
        // 清除所有路径格子
        for (var i = 0; i < Constants.MazeHeight; ++i)
        {
            for (var j = 0; j < Constants.MazeWidth; ++j)
            {
                if (Grids[i, j] is GridType.Path or GridType.FadePath)
                {
                    Grids[i, j] = GridType.Empty;
                    OnGridTypeChanged?.Invoke(new(i, j));
                }
            }
        }
    }

    /// <summary>
    /// 格子状态改变事件处理
    /// </summary>
    /// <param name="position">格子位置</param>
    /// <param name="type">新的格子类型</param>
    /// <exception cref="ArgumentOutOfRangeException">如果位置超出迷宫边界</exception>
    public static void ChangeGridType(Position position, GridType type)
    {
        if (!IsInBounds(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "位置超出迷宫边界");
        }

        // 更新格子类型
        Grids[position.Row, position.Col] = type;

        // 如果是起点或终点, 更新起点或终点位置
        if (type == GridType.Start)
        {
            StartPosition = position;
        }
        else if (type == GridType.End)
        {
            EndPosition = position;
        }
        else if (type == GridType.Empty)
        {
            // 如果设置为空格, 且是起点或终点, 则重置起点或终点位置
            if (position == StartPosition)
            {
                StartPosition = Position.Invalid;
            }
            else if (position == EndPosition)
            {
                EndPosition = Position.Invalid;
            }
        }
    }

    /// <summary>
    /// 检查位置是否在迷宫边界内
    /// </summary>
    /// <param name="position">位置</param>
    /// <returns>如果位置在边界内返回true, 否则返回false</returns>
    private static bool IsInBounds(Position position) => position.Row >= 0 && position.Row < Constants.MazeHeight && position.Col >= 0 && position.Col < Constants.MazeWidth;
}
