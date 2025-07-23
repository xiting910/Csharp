namespace MineClearance;

/// <summary>
/// 表示扫雷游戏中的地雷, 记录地雷的位置
/// </summary>
/// <param name="width">棋盘的宽度</param>
/// <param name="height">棋盘的高度</param>
/// <param name="mineCount">地雷的数量</param>
public class Mines(int width, int height, int mineCount)
{
    /// <summary>
    /// 棋盘的宽度
    /// </summary>
    private readonly int _width = width;

    /// <summary>
    /// 棋盘的高度
    /// </summary>
    private readonly int _height = height;

    /// <summary>
    /// 地雷的数量
    /// </summary>
    private readonly int _mineCount = mineCount;

    /// <summary>
    /// 约束传播结果的数据结构
    /// </summary>
    private class PropagationResult
    {
        /// <summary>
        /// 约束传播是结果否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 过滤后的可用位置列表
        /// </summary>
        public List<int> FilteredAvailable { get; set; } = [];
    }

    /// <summary>
    /// 二维数组, 记录每个坐标周围地雷的数量, -1表示该坐标是地雷
    /// </summary>
    public int[,] MineGrid { get; private set; } = new int[height, width];

    /// <summary>
    /// 使用Fisher-Yates洗牌算法和约束满足算法随机生成地雷位置,
    /// 尽可能确保首次点击位置和其周围格子不包含地雷, 同时不会出现地雷周围全是地雷的情况
    /// </summary>
    /// <param name="firstClickPos">首次点击的位置</param>
    public void GenerateMines(Position firstClickPos)
    {
        int totalCells = _width * _height;

        // 全部格子都是地雷
        if (_mineCount == totalCells)
        {
            for (int row = 0; row < _height; row++)
            {
                for (int col = 0; col < _width; col++)
                {
                    MineGrid[row, col] = -1;
                }
            }
            return;
        }

        List<int> solution = [];
        int firstClickIndex = firstClickPos.Row * _width + firstClickPos.Col;

        // minePositions数组为所有格子索引除去首次点击位置
        int[] minePositions = [.. Enumerable.Range(0, totalCells)];
        minePositions = [.. minePositions.Where(pos => pos != firstClickIndex)];

        // availablePositions数组为所有格子索引除去首次点击位置和其周围格子
        int[] availablePositions = [.. minePositions.Where(pos => !IsAdjacentToFirstClick(pos, firstClickPos))];

        if (_mineCount > availablePositions.Length)
        {
            // 如果availablePositions的长度小于地雷数量, 则从minePositions中随机选择地雷位置
            Methods.RandomInstance.Shuffle(minePositions);
            solution = [.. minePositions.Take(_mineCount)];
        }
        else if (_mineCount == availablePositions.Length)
        {
            // 如果地雷数量等于可用格子数量, 则直接使用可用格子作为地雷位置
            solution = [.. availablePositions];
        }
        else
        {
            // 随机打乱可用格子顺序
            Methods.RandomInstance.Shuffle(availablePositions);

            // 计算雷的密度比例
            var density = (float)_mineCount / totalCells;
            if (density > Constants.MineDensityThreshold)
            {
                // 如果雷的密度过高, 直接随机生成
                solution = [.. availablePositions.Take(_mineCount)];
            }
            else
            {
                try
                {
                    // 否则, 尝试使用约束满足算法生成地雷位置
                    solution = SolveMineCSPWithPropagation([.. availablePositions], []) ?? throw new Exception("无法在给定约束下生成地雷布局");
                }
                catch (Exception)
                {
                    // 如果失败, 则随机选择地雷位置
                    solution = [.. availablePositions.Take(_mineCount)];
                }
            }
        }

        // 更新MineGrid数组
        foreach (var pos in solution)
        {
            int row = pos / _width;
            int column = pos % _width;
            MineGrid[row, column] = -1;
        }

        // 计算不是地雷的格子周围的地雷数量
        for (int row = 0; row < _height; row++)
        {
            for (int column = 0; column < _width; column++)
            {
                if (MineGrid[row, column] == -1) continue;

                MineGrid[row, column] = CountAdjacentMines(row, column);
            }
        }
    }

    /// <summary>
    /// 检查给定位置是否与首次点击位置相邻
    /// </summary>
    /// <param name="pos">要检查的地雷位置索引</param>
    /// <param name="firstClickPos">首次点击位置</param>
    /// <returns>如果相邻返回true, 否则返回false</returns>
    private bool IsAdjacentToFirstClick(int pos, Position firstClickPos)
    {
        int row = pos / _width;
        int col = pos % _width;

        return Math.Abs(row - firstClickPos.Row) <= 1 && Math.Abs(col - firstClickPos.Col) <= 1;
    }

    /// <summary>
    /// 在当前约束下尝试生成地雷布局
    /// </summary>
    /// <param name="available">可用的格子索引列表</param>
    /// <param name="current">当前已放置的地雷索引列表</param>
    /// <returns>如果找到有效的地雷布局, 返回地雷索引列表, 否则返回null</returns>
    private List<int>? SolveMineCSPWithPropagation(List<int> available, List<int> current)
    {
        if (current.Count == _mineCount)
        {
            return [.. current];
        }

        for (int i = 0; i < available.Count; i++)
        {
            int pos = available[i];

            // 尝试在当前位置放置地雷
            current.Add(pos);
            available.RemoveAt(i);

            // 约束传播：计算这个新地雷对可用位置的影响
            var propagationResult = PropagateConstraints(available, current, pos);

            if (propagationResult.IsValid)
            {
                var result = SolveMineCSPWithPropagation(propagationResult.FilteredAvailable, current);

                if (result != null)
                    return result;
            }

            // 回溯: 移除当前地雷位置
            current.RemoveAt(current.Count - 1);
            available.Insert(i, pos);
        }

        return null;
    }

    /// <summary>
    /// 约束传播的核心方法
    /// </summary>
    /// <param name="available">可用的格子索引列表</param>
    /// <param name="current">当前已放置的地雷索引列表</param>
    /// <param name="newMinePos">新加入的地雷位置</param>
    /// <returns>约束传播的结果, 包含是否有效和过滤后的可用位置列表</returns>
    private PropagationResult PropagateConstraints(List<int> available, List<int> current, int newMinePos)
    {
        // 检查新加入的地雷是否立即违反约束
        if (WouldBeSurrounded(newMinePos, current))
        {
            return new PropagationResult { IsValid = false };
        }

        // 检查新地雷是否会导致其他地雷被包围
        foreach (var existingMine in current.Take(current.Count - 1))
        {
            if (WouldBeSurrounded(existingMine, current))
            {
                return new PropagationResult { IsValid = false };
            }
        }

        // 过滤掉不能再放置地雷的位置
        var filteredAvailable = available.Where(pos =>
            !WouldCreateSurroundedMine(pos, current)).ToList();

        return new PropagationResult
        {
            IsValid = true,
            FilteredAvailable = filteredAvailable
        };
    }

    /// <summary>
    /// 辅助方法：检查在指定位置放置地雷是否会创建被包围的地雷
    /// </summary>
    /// <param name="candidatePos">候选地雷位置</param>
    /// <param name="currentMines">当前地雷位置列表</param>
    /// <returns>如果会创建被包围的地雷返回true, 否则返回false</returns>
    private bool WouldCreateSurroundedMine(int candidatePos, List<int> currentMines)
    {
        // 模拟放置地雷
        var tempMines = new List<int>(currentMines) { candidatePos };

        // 检查是否会导致任何地雷被包围
        foreach (var mine in tempMines)
        {
            if (WouldBeSurrounded(mine, tempMines))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查地雷是否会被包围
    /// </summary>
    /// <param name="pos">要检查的地雷位置索引</param>
    /// <param name="minePositions">当前地雷位置列表</param>
    /// <returns>是否被地雷包围</returns>
    private bool WouldBeSurrounded(int pos, List<int> minePositions)
    {
        int row = pos / _width;
        int column = pos % _width;

        // 检查周围的格子
        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = column - 1; c <= column + 1; c++)
            {
                if (r < 0 || r >= _height || c < 0 || c >= _width) continue;
                if (r == row && c == column) continue;

                int neighborPos = r * _width + c;
                if (!minePositions.Contains(neighborPos))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 计算指定格子周围的地雷数量
    /// </summary>
    /// <param name="row">格子的行索引</param>
    /// <param name="column">格子的列索引</param>
    /// <returns>周围地雷的数量</returns>
    private int CountAdjacentMines(int row, int column)
    {
        int mineCount = 0;

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = column - 1; c <= column + 1; c++)
            {
                if (r < 0 || r >= _height || c < 0 || c >= _width) continue;
                if (MineGrid[r, c] == -1) mineCount++;
            }
        }

        return mineCount;
    }
}