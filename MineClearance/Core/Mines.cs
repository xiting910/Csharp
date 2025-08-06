using MineClearance.Models;

namespace MineClearance.Core;

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
    /// 二维数组, 记录每个坐标周围地雷的数量, -1表示该坐标是地雷
    /// </summary>
    public int[,] MineGrid { get; private set; } = new int[height, width];

    /// <summary>
    /// 使用Fisher-Yates洗牌算法随机生成地雷位置, 尽可能确保首次点击位置和其周围格子不包含地雷
    /// </summary>
    /// <param name="firstClickPos">首次点击的位置</param>
    public void GenerateMines(Position firstClickPos)
    {
        // 生成地雷位置的列表
        List<int> minePositions;

        // 生成首次点击位置周围的格子索引
        var safePositionList = new List<int>();
        for (var r = firstClickPos.Row - 1; r <= firstClickPos.Row + 1; ++r)
        {
            for (var c = firstClickPos.Col - 1; c <= firstClickPos.Col + 1; ++c)
            {
                if (r < 0 || r >= _height || c < 0 || c >= _width) continue;
                if (r == firstClickPos.Row && c == firstClickPos.Col) continue;
                safePositionList.Add(r * _width + c);
            }
        }

        // 生成除了首次点击位置周围的格子之外的所有格子索引
        var allPositionList = new List<int>();
        for (var r = 0; r < _height; ++r)
        {
            for (var c = 0; c < _width; ++c)
            {
                if (r == firstClickPos.Row && c == firstClickPos.Col) continue;
                var pos = r * _width + c;
                if (!safePositionList.Contains(pos))
                {
                    allPositionList.Add(pos);
                }
            }
        }

        // 将格子索引列表转化为数组
        var allPositions = allPositionList.ToArray();
        var safePositions = safePositionList.ToArray();

        // 如果地雷数量小于allPositions的数量, 则从中随机选择地雷位置
        if (_mineCount < allPositions.Length)
        {
            Random.Shared.Shuffle(allPositions);
            minePositions = [.. allPositions.Take(_mineCount)];
        }
        else
        {
            // 如果地雷数量大于等于allPositions的数量, 则将所有位置都设置为地雷
            minePositions = [.. allPositions];

            // 还未设置的地雷数量
            var remainingMines = _mineCount - allPositions.Length;

            // 如果剩余地雷数量大于0, 则从安全位置中随机选择位置填充地雷
            if (remainingMines > 0)
            {
                Random.Shared.Shuffle(safePositions);
                minePositions.AddRange(safePositions.Take(remainingMines));
            }
        }

        // 更新MineGrid数组
        foreach (var pos in minePositions)
        {
            var row = pos / _width;
            var column = pos % _width;
            MineGrid[row, column] = -1;
        }

        // 计算不是地雷的格子周围的地雷数量
        for (var row = 0; row < _height; ++row)
        {
            for (var column = 0; column < _width; ++column)
            {
                if (MineGrid[row, column] == -1) continue;
                MineGrid[row, column] = CountAdjacentMines(row, column);
            }
        }
    }

    /// <summary>
    /// 计算指定格子周围的地雷数量
    /// </summary>
    /// <param name="row">格子的行索引</param>
    /// <param name="column">格子的列索引</param>
    /// <returns>周围地雷的数量</returns>
    private int CountAdjacentMines(int row, int column)
    {
        var mineCount = 0;

        for (var r = row - 1; r <= row + 1; ++r)
        {
            for (var c = column - 1; c <= column + 1; ++c)
            {
                if (r < 0 || r >= _height || c < 0 || c >= _width) continue;
                if (MineGrid[r, c] == -1) mineCount++;
            }
        }

        return mineCount;
    }
}