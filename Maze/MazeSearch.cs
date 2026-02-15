namespace Maze;

// 迷宫类的搜索算法实现部分
internal static partial class Maze
{
    /// <summary>
    /// 根据选择的算法搜索路径
    /// </summary>
    /// <param name="algorithm">搜索算法</param>
    /// <param name="showSearchProcess">是否显示搜索过程</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>搜索结果</returns>
    /// <exception cref="InvalidOperationException">如果起点和终点未设置</exception>
    public static async Task<SearchResult> SearchPath(SearchAlgorithm algorithm, bool showSearchProcess, CancellationToken cancellationToken)
    {
        // 检查起点和终点是否已设置
        if (!IsStartAndEndSet())
        {
            // 如果起点和终点未设置, 说明程序设计出错了, 抛出异常
            throw new InvalidOperationException("起点和终点未设置, 请先设置起点和终点位置。");
        }

        // 如果已经申请了取消令牌, 则直接返回取消结果
        if (cancellationToken.IsCancellationRequested)
        {
            return new(true, false, 0);
        }

        // 根据选择的算法执行搜索
        return algorithm switch
        {
            SearchAlgorithm.BFS => await SearchPathByBFS(showSearchProcess, cancellationToken),
            SearchAlgorithm.DFS => await SearchPathByDFS(showSearchProcess, cancellationToken),
            SearchAlgorithm.AStar => await SearchPathByAStar(showSearchProcess, cancellationToken),
            _ => new(false, false, 0),
        };
    }

    /// <summary>
    /// BFS算法搜索路径
    /// </summary>
    /// <param name="showSearchProcess">是否显示搜索过程</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>搜索结果</returns>
    private static async Task<SearchResult> SearchPathByBFS(bool showSearchProcess, CancellationToken cancellationToken)
    {
        // BFS算法所需变量
        var queue = new Queue<Position>();
        var visited = new bool[Constants.MazeHeight, Constants.MazeWidth];
        var cameFrom = new Position[Constants.MazeHeight, Constants.MazeWidth];

        // 记录是否找到终点
        var foundEnd = false;

        // 标记起点为已访问, 并将其加入队列
        visited[StartPosition.Row, StartPosition.Col] = true;
        queue.Enqueue(StartPosition);

        // 记录起点的父节点为无效位置
        cameFrom[StartPosition.Row, StartPosition.Col] = Position.Invalid;

        // BFS搜索逻辑
        async Task BFS()
        {
            while (queue.Count > 0)
            {
                // 检查是否取消了搜索
                cancellationToken.ThrowIfCancellationRequested();

                // 获取队列中的当前格子
                var current = queue.Dequeue();
                if (current == EndPosition)
                {
                    // 找到终点, 结束搜索
                    foundEnd = true;
                    break;
                }

                // 不是起点时设置当前格子为淡出路径
                if (current != StartPosition)
                {
                    Grids[current.Row, current.Col] = GridType.FadePath;
                }

                // 如果需要显示搜索过程
                if (showSearchProcess)
                {
                    OnGridTypeChanged?.Invoke(current);
                }

                // 遍历四个方向
                foreach (var dir in Enum.GetValues<Direction>())
                {
                    // 检查是否取消了搜索
                    cancellationToken.ThrowIfCancellationRequested();

                    // 计算新位置
                    var next = current + dir;

                    // 检查新位置是否在边界内
                    if (!IsInBounds(next))
                    {
                        continue;
                    }

                    // 如果新位置不是障碍物且未被访问过
                    if (Grids[next.Row, next.Col] != GridType.Obstacle && !visited[next.Row, next.Col])
                    {
                        // 将新位置加入队列, 标记为已访问, 设置父节点
                        queue.Enqueue(next);
                        visited[next.Row, next.Col] = true;
                        cameFrom[next.Row, next.Col] = current;

                        // 找到终点, 结束遍历
                        if (next == EndPosition)
                        {
                            foundEnd = true;
                            break;
                        }

                        // 设置新位置为路径
                        Grids[next.Row, next.Col] = GridType.Path;

                        // 如果需要显示搜索过程
                        if (showSearchProcess)
                        {
                            OnGridTypeChanged?.Invoke(next);
                            await Task.Delay(1, cancellationToken);
                        }
                    }
                }
            }
        }

        // 执行BFS搜索
        try
        {
            await Task.Run(BFS, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return new(true, false, 0);
        }
        catch (OperationCanceledException)
        {
            return new(true, false, 0);
        }

        // 没有找到路径
        if (!foundEnd)
        {
            OnPanelRefreshed?.Invoke();
            return new(false, false, 0);
        }

        // 回溯路径, 并设置和显示路径
        var path = await ReconstructPath(cameFrom);
        ShowPath(path);

        // 返回搜索成功
        return new(false, true, path.Count);
    }

    /// <summary>
    /// DFS算法搜索路径
    /// </summary>
    /// <param name="showSearchProcess">是否显示搜索过程</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>搜索结果</returns>
    private static async Task<SearchResult> SearchPathByDFS(bool showSearchProcess, CancellationToken cancellationToken)
    {
        // DFS算法所需变量
        var stack = new Stack<Position>();
        var visited = new bool[Constants.MazeHeight, Constants.MazeWidth];
        var cameFrom = new Position[Constants.MazeHeight, Constants.MazeWidth];

        // 记录每个位置的有效路径数
        var pathCount = new int[Constants.MazeHeight, Constants.MazeWidth];

        // 是否找到终点
        var foundEnd = false;

        // 标记起点为已访问, 并将其加入栈
        visited[StartPosition.Row, StartPosition.Col] = true;
        stack.Push(StartPosition);

        // 记录起点的父节点为无效位置
        cameFrom[StartPosition.Row, StartPosition.Col] = Position.Invalid;

        // DFS搜索逻辑
        async Task DFS()
        {
            while (stack.Count > 0)
            {
                // 检查是否取消了搜索
                cancellationToken.ThrowIfCancellationRequested();

                // 获取栈顶的当前格子
                var current = stack.Pop();
                if (current == EndPosition)
                {
                    // 找到终点, 结束搜索
                    foundEnd = true;
                    break;
                }

                // 不是起点时设置当前格子为路径
                if (current != StartPosition)
                {
                    Grids[current.Row, current.Col] = GridType.Path;
                }

                // 如果需要显示搜索过程
                if (showSearchProcess)
                {
                    OnGridTypeChanged?.Invoke(current);
                    await Task.Delay(1, cancellationToken);
                }

                // 获取根据当前位置和终点位置排序好的方向
                var orderedDirections = Methods.GetOrderedDirections(current, EndPosition);
                Array.Reverse(orderedDirections);

                // 遍历四个方向
                foreach (var dir in orderedDirections)
                {
                    // 检查是否取消了搜索
                    cancellationToken.ThrowIfCancellationRequested();

                    // 计算新位置
                    var next = current + dir;

                    // 检查新位置是否在边界内
                    if (!IsInBounds(next))
                    {
                        continue;
                    }

                    // 如果新位置不是障碍物且未被访问过
                    if (Grids[next.Row, next.Col] != GridType.Obstacle && !visited[next.Row, next.Col])
                    {
                        // 将新位置加入栈, 标记为已访问, 设置父节点
                        stack.Push(next);
                        visited[next.Row, next.Col] = true;
                        cameFrom[next.Row, next.Col] = current;

                        // 记录路径数
                        ++pathCount[current.Row, current.Col];

                        // 找到终点, 结束遍历
                        if (next == EndPosition)
                        {
                            foundEnd = true;
                            break;
                        }
                    }
                }

                // 如果路径数为0
                while (pathCount[current.Row, current.Col] == 0)
                {
                    // 检测是否取消了搜索
                    cancellationToken.ThrowIfCancellationRequested();

                    // 不是起点时设置当前格子为淡出路径
                    if (current != StartPosition)
                    {
                        Grids[current.Row, current.Col] = GridType.FadePath;
                    }

                    // 如果需要显示搜索过程
                    if (showSearchProcess)
                    {
                        OnGridTypeChanged?.Invoke(current);
                        await Task.Delay(1, cancellationToken);
                    }

                    // 回溯到上一个节点
                    current = cameFrom[current.Row, current.Col];

                    // 如果当前位置为无效, 则结束
                    if (current == Position.Invalid)
                    {
                        break;
                    }

                    // 上一个节点的路径数减1
                    --pathCount[current.Row, current.Col];
                }
            }
        }

        // 执行DFS搜索
        try
        {
            await Task.Run(DFS, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return new(true, false, 0);
        }
        catch (OperationCanceledException)
        {
            return new(true, false, 0);
        }

        // 没有找到路径
        if (!foundEnd)
        {
            OnPanelRefreshed?.Invoke();
            return new(false, false, 0);
        }

        // 回溯路径, 并设置和显示路径
        var path = await ReconstructPath(cameFrom);
        ShowPath(path);

        // 返回搜索成功
        return new(false, true, path.Count);
    }

    /// <summary>
    /// A*算法搜索路径
    /// </summary>
    /// <param name="showSearchProcess">是否显示搜索过程</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>搜索结果</returns>
    private static async Task<SearchResult> SearchPathByAStar(bool showSearchProcess, CancellationToken cancellationToken)
    {
        // 定义A*所需结构
        var openSet = new PriorityQueue<Position, int>();
        var cameFrom = new Position[Constants.MazeHeight, Constants.MazeWidth];
        var gScore = new int[Constants.MazeHeight, Constants.MazeWidth];
        var visited = new bool[Constants.MazeHeight, Constants.MazeWidth];

        // 初始化
        for (var i = 0; i < Constants.MazeHeight; ++i)
        {
            for (var j = 0; j < Constants.MazeWidth; ++j)
            {
                gScore[i, j] = int.MaxValue;
                cameFrom[i, j] = Position.Invalid;
            }
        }

        // 添加起点
        gScore[StartPosition.Row, StartPosition.Col] = 0;
        openSet.Enqueue(StartPosition, ManhattanDistance(StartPosition, EndPosition));

        // 记录是否找到终点
        var found = false;

        // A*搜索逻辑
        async Task AStar()
        {
            while (openSet.Count > 0)
            {
                // 检查是否取消了搜索
                cancellationToken.ThrowIfCancellationRequested();

                // 获取当前格子, 并从优先队列中移除
                var current = openSet.Dequeue();

                // 如果当前格子是终点, 结束搜索
                if (current == EndPosition)
                {
                    found = true;
                    break;
                }

                // 如果当前格子已经被访问过, 跳过
                if (visited[current.Row, current.Col])
                {
                    continue;
                }

                // 标记当前格子为已访问
                visited[current.Row, current.Col] = true;

                // 不是起点时设置当前格子为淡出路径
                if (current != StartPosition)
                {
                    Grids[current.Row, current.Col] = GridType.FadePath;
                }

                // 如果需要显示搜索过程
                if (showSearchProcess)
                {
                    OnGridTypeChanged?.Invoke(current);
                }

                // 遍历四个方向
                foreach (var dir in Enum.GetValues<Direction>())
                {
                    // 检查是否取消了搜索
                    cancellationToken.ThrowIfCancellationRequested();

                    // 计算新位置
                    var neighbor = current + dir;

                    //  如果新位置不在边界内, 跳过
                    if (!IsInBounds(neighbor))
                    {
                        continue;
                    }

                    // 如果新位置是障碍物, 跳过
                    if (Grids[neighbor.Row, neighbor.Col] == GridType.Obstacle)
                    {
                        continue;
                    }

                    // 计算新的gScore
                    var tentative_gScore = gScore[current.Row, current.Col] + 1;

                    // 如果新的gScore小于当前gScore, 则更新路径
                    if (tentative_gScore < gScore[neighbor.Row, neighbor.Col])
                    {
                        // 更新cameFrom, gScore和fScore
                        cameFrom[neighbor.Row, neighbor.Col] = current;
                        gScore[neighbor.Row, neighbor.Col] = tentative_gScore;
                        var fScore = tentative_gScore + ManhattanDistance(neighbor, EndPosition);

                        // 新位置加入优先队列中
                        openSet.Enqueue(neighbor, fScore);

                        // 如果新位置是终点, 结束搜索
                        if (neighbor == EndPosition)
                        {
                            found = true;
                            break;
                        }

                        // 不是起点时设置当前格子为路径
                        if (neighbor != StartPosition)
                        {
                            Grids[neighbor.Row, neighbor.Col] = GridType.Path;
                        }

                        // 如果需要显示搜索过程
                        if (showSearchProcess)
                        {
                            OnGridTypeChanged?.Invoke(neighbor);
                            await Task.Delay(1, cancellationToken);
                        }
                    }
                }
            }
        }

        // 执行A*搜索
        try
        {
            await Task.Run(AStar, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return new(true, false, 0);
        }
        catch (OperationCanceledException)
        {
            return new(true, false, 0);
        }

        // 没有找到路径
        if (!found)
        {
            OnPanelRefreshed?.Invoke();
            return new(false, false, 0);
        }

        // 回溯路径, 并设置和显示路径
        var path = await ReconstructPath(cameFrom);
        ShowPath(path);

        // 返回搜索成功
        return new(false, true, path.Count);
    }

    /// <summary>
    /// 重建除了起点和终点之外的路径
    /// </summary>
    /// <param name="cameFrom">记录每个节点的父节点</param>
    /// <returns>返回重建的路径</returns>
    /// <exception cref="InvalidOperationException">重建路径失败</exception>
    private static async Task<List<Position>> ReconstructPath(Position[,] cameFrom)
    {
        var path = new Stack<Position>();
        var cur = EndPosition;

        // 从终点开始回溯路径
        await Task.Run(() =>
        {
            while (true)
            {
                // 回溯到上一个节点
                cur = cameFrom[cur.Row, cur.Col];

                // 如果当前位置为无效
                if (cur == Position.Invalid)
                {
                    throw new InvalidOperationException("重建路径失败");
                }

                // 如果回溯到起点, 则结束
                if (cur == StartPosition)
                {
                    break;
                }

                // 添加当前位置到路径
                path.Push(cur);
            }
        });

        return [.. path];
    }

    /// <summary>
    /// 设置并显示找到的路径
    /// </summary>
    /// <param name="path">找到的路径</param>
    private static void ShowPath(List<Position> path)
    {
        // 清除之前的路径
        ClearPaths();

        // 遍历路径, 设置为路径颜色
        foreach (var pos in path)
        {
            Grids[pos.Row, pos.Col] = GridType.Path;
            OnGridTypeChanged?.Invoke(pos);
        }
    }

    /// <summary>
    /// 曼哈顿距离启发函数
    /// </summary>
    /// <param name="a">起点位置</param>
    /// <param name="b">终点位置</param>
    /// <returns>返回曼哈顿距离</returns>
    private static int ManhattanDistance(Position a, Position b) => Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);
}
