namespace Maze;

// 迷宫类的生成算法实现部分
internal static partial class Maze
{
    /// <summary>
    /// 根据指定的生成算法随机生成迷宫
    /// </summary>
    /// <param name="algorithm">生成算法</param>
    /// <exception cref="ArgumentOutOfRangeException">如果算法不在支持的范围内</exception>
    public static async Task RandomGenerateMaze(GenerationAlgorithm algorithm)
    {
        // 根据指定的生成算法调用相应的生成方法
        switch (algorithm)
        {
            case GenerationAlgorithm.Random:
                await RandomGenerateMaze();
                break;
            case GenerationAlgorithm.DFS:
                await GenerateMazeByDFS();
                break;
            case GenerationAlgorithm.RecursiveDivision:
                await GenerateMazeByRecursiveDivision();
                break;
            case GenerationAlgorithm.Prim:
                await GenerateMazeByPrim();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(algorithm), "生成算法错误");
        }

        // 生成完成后触发面板刷新事件
        OnPanelRefreshed?.Invoke();
    }

    /// <summary>
    /// 完全随机生成迷宫
    /// </summary>
    /// <remarks>
    /// 通过调整Constants.ObstacleRatio来控制障碍物的密度
    /// </remarks>
    private static async Task RandomGenerateMaze() =>
        // 遍历所有格子, 根据Constants.ObstacleRatio随机设置为障碍物或空白
        await Task.Run(() =>
        {
            for (var i = 0; i < Constants.MazeHeight; ++i)
            {
                for (var j = 0; j < Constants.MazeWidth; ++j)
                {
                    // 保持起点和终点不变
                    if (Grids[i, j] is GridType.Start or GridType.End)
                    {
                        continue;
                    }

                    // 根据障碍物比例随机设置格子类型
                    Grids[i, j] = Methods.RandomInstance.NextDouble() < Constants.ObstacleRatio ? GridType.Obstacle : GridType.Empty;
                }
            }
        });

    /// <summary>
    /// 使用DFS算法随机生成迷宫
    /// </summary>
    private static async Task GenerateMazeByDFS()
    {
        // DFS算法
        static void DFS(Position pos)
        {
            // 将当前格子设置为空白
            Grids[pos.Row, pos.Col] = GridType.Empty;

            // 遍历所有方向
            foreach (var dir in Methods.GetShuffledDirections())
            {
                // 计算下一个格子的位置
                var next = pos + dir + dir;

                // 检查下一个格子是否在边界内且未被设置为空白
                if (IsInBounds(next) && Grids[next.Row, next.Col] == GridType.Obstacle)
                {
                    // 打通路径
                    var between = pos + dir;
                    Grids[between.Row, between.Col] = GridType.Empty;

                    // 递归访问下一个格子
                    DFS(next);
                }
            }
        }

        // 异步生成迷宫
        await Task.Run(() =>
        {
            // 初始化所有格子为障碍物
            for (var i = 0; i < Constants.MazeHeight; ++i)
            {
                for (var j = 0; j < Constants.MazeWidth; ++j)
                {
                    Grids[i, j] = GridType.Obstacle;
                }
            }

            // 从起点开始DFS
            DFS(StartPosition);

            // 设置起点位置和终点位置
            Grids[StartPosition.Row, StartPosition.Col] = GridType.Start;
            Grids[EndPosition.Row, EndPosition.Col] = GridType.End;

            // 确保终点可达
            EnsureEndPositionIsReachable();
        });
    }

    /// <summary>
    /// 使用递归分割算法随机生成迷宫
    /// </summary>
    private static async Task GenerateMazeByRecursiveDivision()
    {
        // 递归分割算法
        static void RecursiveDivision(Position topLeft, Position bottomRight)
        {
            // 计算分割区域的宽度和高度
            var width = bottomRight.Col - topLeft.Col + 1;
            var height = bottomRight.Row - topLeft.Row + 1;

            // 水平方向是否无法分割
            var cannotSplitHorizontally = height < 3;

            // 垂直方向是否无法分割
            var cannotSplitVertically = width < 3;

            // 如果无法分割, 直接返回
            if (cannotSplitHorizontally && cannotSplitVertically)
            {
                return;
            }

            // 分割方向
            var horizontals = Array.Empty<bool>();
            if (cannotSplitVertically)
            {
                // 如果只能水平分割, 则强制设置为水平分割
                horizontals = [true];
            }
            else if (cannotSplitHorizontally)
            {
                // 如果只能垂直分割, 则强制设置为垂直分割
                horizontals = [false];
            }
            else
            {
                // 随机打乱分割方向进行选择
                horizontals = [true, false];
                Methods.RandomInstance.Shuffle(horizontals);
            }

            // 边界墙的4个角位置
            var wallTopLeft = new Position(topLeft.Row - 1, topLeft.Col - 1);
            var wallBottomLeft = new Position(bottomRight.Row + 1, topLeft.Col - 1);
            var wallTopRight = new Position(topLeft.Row - 1, bottomRight.Col + 1);
            var wallBottomRight = new Position(bottomRight.Row + 1, bottomRight.Col + 1);

            // 是否切割完成
            var isCutCompleted = false;

            // 处理分割
            foreach (var horizontal in horizontals)
            {
                if (isCutCompleted)
                {
                    break;
                }

                if (horizontal)
                {
                    // 获取左右两个边界的所有开口
                    var leftOpenings = GetAllOpenings(wallTopLeft, wallBottomLeft);
                    var rightOpenings = GetAllOpenings(wallTopRight, wallBottomRight);

                    // 所有开口都不能是分割线的位置
                    List<int> openings = [];
                    foreach (var opening in leftOpenings)
                    {
                        openings.Add(opening.Row);
                    }
                    foreach (var opening in rightOpenings)
                    {
                        openings.Add(opening.Row);
                    }

                    // 随机选择分割线的位置
                    var lineRowPos = Methods.Next(topLeft.Row + 1, bottomRight.Row - 1, openings);

                    // 分割线可用时
                    if (lineRowPos != -1)
                    {
                        // 随机选择开口位置
                        var openPos = Methods.Next(topLeft.Col, bottomRight.Col, []);

                        // 在分割线上除了随机开口的位置设置障碍物
                        for (var i = topLeft.Col; i <= bottomRight.Col; ++i)
                        {
                            if (i != openPos)
                            {
                                Grids[lineRowPos, i] = GridType.Obstacle;
                            }
                        }

                        // 记录切割完成
                        isCutCompleted = true;

                        // 递归分割上下部分
                        RecursiveDivision(topLeft, new(lineRowPos - 1, bottomRight.Col));
                        RecursiveDivision(new(lineRowPos + 1, topLeft.Col), bottomRight);
                    }
                }
                else
                {
                    // 获取上下两个边界的所有开口
                    var topOpenings = GetAllOpenings(wallTopLeft, wallTopRight);
                    var bottomOpenings = GetAllOpenings(wallBottomLeft, wallBottomRight);

                    // 所有开口都不能是分割线的位置
                    List<int> openings = [];
                    foreach (var opening in topOpenings)
                    {
                        openings.Add(opening.Col);
                    }
                    foreach (var opening in bottomOpenings)
                    {
                        openings.Add(opening.Col);
                    }

                    // 随机选择分割线的位置
                    var lineColPos = Methods.Next(topLeft.Col + 1, bottomRight.Col - 1, openings);

                    // 分割线可用时
                    if (lineColPos != -1)
                    {
                        // 随机选择开口位置
                        var openPos = Methods.Next(topLeft.Row, bottomRight.Row, []);

                        // 在分割线上除了随机开口的位置设置障碍物
                        for (var i = topLeft.Row; i <= bottomRight.Row; ++i)
                        {
                            if (i != openPos)
                            {
                                Grids[i, lineColPos] = GridType.Obstacle;
                            }
                        }

                        // 记录切割完成
                        isCutCompleted = true;

                        // 递归分割左右部分
                        RecursiveDivision(topLeft, new(bottomRight.Row, lineColPos - 1));
                        RecursiveDivision(new(topLeft.Row, lineColPos + 1), bottomRight);
                    }
                }
            }
        }

        // 异步生成迷宫
        await Task.Run(() =>
        {
            // 设置边界为障碍物
            for (var i = 0; i < Constants.MazeHeight; ++i)
            {
                Grids[i, 0] = GridType.Obstacle;
                Grids[i, Constants.MazeWidth - 1] = GridType.Obstacle;
            }
            for (var j = 0; j < Constants.MazeWidth; ++j)
            {
                Grids[0, j] = GridType.Obstacle;
                Grids[Constants.MazeHeight - 1, j] = GridType.Obstacle;
            }

            // 开始递归分割
            RecursiveDivision(new(1, 1), new(Constants.MazeHeight - 2, Constants.MazeWidth - 2));
        });
    }

    /// <summary>
    /// 使用Prim算法随机生成迷宫
    /// </summary>
    private static async Task GenerateMazeByPrim() => await Task.Run(() =>
                                                           {
                                                               // 初始化所有格子为障碍物
                                                               for (var i = 0; i < Constants.MazeHeight; ++i)
                                                               {
                                                                   for (var j = 0; j < Constants.MazeWidth; ++j)
                                                                   {
                                                                       Grids[i, j] = GridType.Obstacle;
                                                                   }
                                                               }

                                                               // 设置起点位置为路点
                                                               Grids[StartPosition.Row, StartPosition.Col] = GridType.Empty;

                                                               // 创建待选路点列表
                                                               var candidatePositions = new List<Position>();

                                                               // 添加起点周围的候选位置
                                                               foreach (var dir in Enum.GetValues<Direction>())
                                                               {
                                                                   var next = StartPosition + dir + dir;
                                                                   if (IsInBounds(next))
                                                                   {
                                                                       candidatePositions.Add(next);
                                                                   }
                                                               }

                                                               // Prim算法核心逻辑
                                                               while (candidatePositions.Count > 0)
                                                               {
                                                                   // 随机选择一个候选位置
                                                                   var pos = Methods.RandomSelectPosition(candidatePositions);
                                                                   _ = candidatePositions.Remove(pos);

                                                                   // 如果当前格子已经是空白, 跳过
                                                                   if (Grids[pos.Row, pos.Col] == GridType.Empty)
                                                                   {
                                                                       continue;
                                                                   }

                                                                   // 将当前格子设置为空白
                                                                   Grids[pos.Row, pos.Col] = GridType.Empty;

                                                                   // 是否打通路径
                                                                   var isPathCreated = false;

                                                                   // 打通当前格子与周围随机的空白格子之间的路径
                                                                   foreach (var dir in Methods.GetShuffledDirections())
                                                                   {
                                                                       var next = pos + dir + dir;
                                                                       if (IsInBounds(next))
                                                                       {
                                                                           if (!isPathCreated && Grids[next.Row, next.Col] == GridType.Empty)
                                                                           {
                                                                               // 打通路径
                                                                               var between = pos + dir;
                                                                               Grids[between.Row, between.Col] = GridType.Empty;

                                                                               // 只打通一次路径
                                                                               isPathCreated = true;
                                                                           }
                                                                           else if (Grids[next.Row, next.Col] == GridType.Obstacle)
                                                                           {
                                                                               // 添加周围的候选位置
                                                                               candidatePositions.Add(next);
                                                                           }
                                                                       }
                                                                   }
                                                               }

                                                               // 设置起点和终点
                                                               Grids[StartPosition.Row, StartPosition.Col] = GridType.Start;
                                                               Grids[EndPosition.Row, EndPosition.Col] = GridType.End;

                                                               // 确保终点可达
                                                               EnsureEndPositionIsReachable();
                                                           });

    /// <summary>
    /// 如果终点不可达, 随机打通终点周围的一个障碍物
    /// </summary>
    private static void EnsureEndPositionIsReachable()
    {
        // 判断终点是否可达
        var canReachEnd = false;
        foreach (var dir in Enum.GetValues<Direction>())
        {
            var next = EndPosition + dir;
            if (IsInBounds(next) && Grids[next.Row, next.Col] == GridType.Empty)
            {
                canReachEnd = true;
                break;
            }
        }

        // 如果终点不可达, 随机打通终点周围的一个障碍物
        if (!canReachEnd)
        {
            foreach (var dir in Methods.GetShuffledDirections())
            {
                var next = EndPosition + dir;
                if (IsInBounds(next) && Grids[next.Row, next.Col] == GridType.Obstacle)
                {
                    Grids[next.Row, next.Col] = GridType.Empty;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 获取两个位置之间的所有开口
    /// </summary>
    /// <param name="start">起点位置</param>
    /// <param name="end">终点位置</param>
    /// <returns>返回开口位置列表</returns>
    /// <exception cref="ArgumentException">如果两个位置不在同一行或列</exception>
    private static List<Position> GetAllOpenings(Position start, Position end)
    {
        // 如果两个位置不在同一行或列, 则抛出异常
        if (start.Row != end.Row && start.Col != end.Col)
        {
            throw new ArgumentException("两个位置必须在同一行或列");
        }

        var openings = new List<Position>();
        if (start.Row == end.Row)
        {
            // 在同一行, 遍历列
            for (var col = Math.Min(start.Col, end.Col); col < Math.Max(start.Col, end.Col); ++col)
            {
                if (Grids[start.Row, col] == GridType.Empty)
                {
                    openings.Add(new(start.Row, col));
                }
            }
        }
        else
        {
            // 在同一列, 遍历行
            for (var row = Math.Min(start.Row, end.Row); row < Math.Max(start.Row, end.Row); ++row)
            {
                if (Grids[row, start.Col] == GridType.Empty)
                {
                    openings.Add(new(row, start.Col));
                }
            }
        }

        // 如果有开口, 则返回开口列表
        return openings;
    }
}
