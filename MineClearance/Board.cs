namespace MineClearance;

/// <summary>
/// 表示扫雷游戏的场地
/// </summary>
public class Board
{
    /// <summary>
    /// 格子状态改变事件
    /// </summary>
    public event Action<Position>? GridChanged;

    /// <summary>
    /// 当剩余地雷数量改变时触发
    /// </summary>
    public event Action<int>? RemainingMinesChanged;

    /// <summary>
    /// 当游戏胜利时触发
    /// </summary>
    public event Action? Won;

    /// <summary>
    /// 当打开地雷时触发(游戏失败)
    /// </summary>
    public event Action<Position>? HitMine;

    /// <summary>
    /// 首次点击时触发
    /// </summary>
    public event Action? FirstClick;

    /// <summary>
    /// 游戏的宽度
    /// </summary>
    public readonly int Width;

    /// <summary>
    /// 游戏的高度
    /// </summary>
    public readonly int Height;

    /// <summary>
    /// 游戏上的格子数组
    /// </summary>
    public readonly Grid[,] Grids;

    /// <summary>
    /// 管理地雷的集合
    /// </summary>
    public readonly Mines Mines;

    /// <summary>
    /// 未打开的非地雷格子数量
    /// </summary>
    private int _unopenedSafeCount;

    /// <summary>
    /// 剩余地雷数量
    /// </summary>
    private int _remainingMines;

    /// <summary>
    /// 是否是第一次点击
    /// </summary>
    private bool _isFirstClick;

    /// <summary>
    /// 构造函数, 创建一个新的游戏场地实例
    /// </summary>
    /// <param name="height">游戏高度</param>
    /// <param name="width">游戏宽度</param>
    /// <param name="mineCount">地雷数量</param>
    public Board(int width, int height, int mineCount)
    {
        Width = width;
        Height = height;
        Grids = new Grid[height, width];
        Mines = new Mines(width, height, mineCount);
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                Grids[row, column] = new Grid(row, column, GridType.Unopened);
            }
        }

        _unopenedSafeCount = height * width - mineCount;
        _remainingMines = mineCount;
        _isFirstClick = true;
    }

    /// <summary>
    /// 获取正确标记地雷的数量
    /// </summary>
    /// <returns>正确标记地雷的数量</returns>
    public int GetCorrectFlagCount()
    {
        int correctFlagCount = 0;
        for (int row = 0; row < Height; row++)
        {
            for (int column = 0; column < Width; column++)
            {
                if (Grids[row, column].Type == GridType.Flagged && Mines.MineGrid[row, column] == -1)
                {
                    correctFlagCount++;
                }
            }
        }
        return correctFlagCount;
    }

    /// <summary>
    /// 鼠标点击格子
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    /// <param name="isRightClick">是否为右键点击</param>
    public void OnGridClick(Position position, bool isRightClick)
    {
        // 检查点击位置是否在游戏范围内
        if (position.Row < 0 || position.Row >= Height || position.Col < 0 || position.Col >= Width)
        {
            return;
        }

        // 如果是第一次点击并且是右键点击, 则不处理
        if (_isFirstClick && isRightClick)
        {
            return;
        }

        // 如果是第一次点击并且不是右键点击, 则生成地雷并打开格子
        if (_isFirstClick && !isRightClick)
        {
            Mines.GenerateMines(position);
            _isFirstClick = false;
            FirstClick?.Invoke();
            OpenGrid(position);
            return;
        }

        // 如果不是第一次点击且是右键点击, 则插旗或取消插旗
        if (isRightClick)
        {
            ToggleFlag(position);
            return;
        }

        // 如果不是第一次点击且不是右键点击, 则打开格子
        OpenGrid(position);

        // 检查是否胜利
        CheckWin();
    }

    /// <summary>
    /// 打开格子
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    private void OpenGrid(Position position)
    {
        // 如果点击的格子不是未打开状态, 则不处理
        if (Grids[position.Row, position.Col].Type != GridType.Unopened)
        {
            return;
        }

        // 如果点击的格子是地雷
        if (Mines.MineGrid[position.Row, position.Col] == -1)
        {
            Grids[position.Row, position.Col].Type = GridType.Mine;
            HitMine?.Invoke(position);
            return;
        }

        // 如果点击的格子不是地雷
        int surroundingMines = Mines.MineGrid[position.Row, position.Col];
        --_unopenedSafeCount;

        // 如果周围地雷数量为0, 则打开周围格子
        if (surroundingMines == 0)
        {
            Grids[position.Row, position.Col].Type = GridType.Empty;
            for (int r = position.Row - 1; r <= position.Row + 1; r++)
            {
                for (int c = position.Col - 1; c <= position.Col + 1; c++)
                {
                    if (r >= 0 && r < Height && c >= 0 && c < Width)
                    {
                        OpenGrid(new(r, c));
                    }
                }
            }
        }
        else
        {
            Grids[position.Row, position.Col].Type = GridType.Number;
        }

        Grids[position.Row, position.Col].SurroundingMines = surroundingMines;
        GridChanged?.Invoke(new(position.Row, position.Col));
    }

    /// <summary>
    /// 插旗/取消插旗
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    private void ToggleFlag(Position position)
    {
        if (Grids[position.Row, position.Col].Type == GridType.Unopened)
        {
            // 插旗
            Grids[position.Row, position.Col].Type = GridType.Flagged;
            GridChanged?.Invoke(new(position.Row, position.Col));
            _remainingMines--;
            RemainingMinesChanged?.Invoke(_remainingMines);
        }
        else if (Grids[position.Row, position.Col].Type == GridType.Flagged)
        {
            // 取消插旗
            Grids[position.Row, position.Col].Type = GridType.Unopened;
            GridChanged?.Invoke(new(position.Row, position.Col));
            _remainingMines++;
            RemainingMinesChanged?.Invoke(_remainingMines);
        }
    }

    /// <summary>
    /// 检查游戏是否胜利, 如果胜利触发胜利事件
    /// </summary>
    private void CheckWin()
    {
        // 未打开的安全格子数量等于0
        if (_unopenedSafeCount == 0)
        {
            Won?.Invoke();
        }
    }
}