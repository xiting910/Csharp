namespace MineClearance
{
    /// <summary>
    /// 表示扫雷游戏
    /// </summary>
    public class Board
    {
        /// <summary>
        /// 当格子被打开时触发
        /// </summary>
        public event Action<int, int, int>? GridOpened;
        /// <summary>
        /// 当格子被插旗时触发
        /// </summary>
        public event Action<int, int>? GridFlagged;
        /// <summary>
        /// 当格子被取消插旗时触发
        /// </summary>
        public event Action<int, int>? GridUnflagged;
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
        public event Action<int, int>? HitMine;
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
        /// 未打开格子的数量
        /// </summary>
        private int _unopenedCount;
        /// <summary>
        /// 剩余地雷数量
        /// </summary>
        private int _remainingMines;
        /// <summary>
        /// 错误标记地雷的数量
        /// </summary>
        private int _wrongFlagCount;
        /// <summary>
        /// 管理地雷的集合
        /// </summary>
        public readonly Mines Mines;
        /// <summary>
        /// 是否是第一次点击
        /// </summary>
        private bool _isFirstClick;

        /// <summary>
        /// 创建一个新的游戏实例
        /// </summary>
        /// <param name="height">游戏高度</param>
        /// <param name="width">游戏宽度</param>
        /// <param name="mineCount">地雷数量</param>
        public Board(int width, int height, int mineCount)
        {
            Width = width;
            Height = height;
            Grids = new Grid[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    Grids[row, column] = new Grid(row, column, GridType.Unopened);
                }
            }

            _unopenedCount = height * width;
            _remainingMines = mineCount;
            _wrongFlagCount = 0;
            Mines = new Mines(width, height, mineCount);
            _isFirstClick = true;
        }

        /// <summary>
        /// 鼠标点击格子
        /// </summary>
        /// <param name="row">格子的行索引</param>
        /// <param name="column">格子的列索引</param>
        /// <param name="isRightClick">是否为右键点击</param>
        public void OnGridClick(int row, int column, bool isRightClick = false)
        {
            // 检查点击位置是否在游戏范围内
            if (row < 0 || row >= Height || column < 0 || column >= Width)
            {
                return; 
            }

            // 如果是第一次点击并且是右键点击, 则不处理
            if (_isFirstClick && isRightClick)
            {
                return;
            }

            // 如果是第一次点击并且不是右键点击, 则生成地雷
            if (_isFirstClick && !isRightClick)
            {
                Mines.GenerateMines((row, column));
                _isFirstClick = false;
                FirstClick?.Invoke();
            }

            // 如果是右键点击
            if (isRightClick)
            {
                ToggleFlag(row, column);
                if (CheckWin())
                {
                    Won?.Invoke();
                }
                return;
            }

            // 如果是左键点击并且格子不是未打开状态
            if (Grids[row, column].Type != GridType.Unopened)
            {
                return;
            }

            // 打开格子
            OpenGrid(row, column);

            // 检查是否胜利
            if (CheckWin())
            {
                Won?.Invoke();
            }
        }

        /// <summary>
        /// 打开格子
        /// </summary>
        /// <param name="row">格子的行索引</param>
        /// <param name="column">格子的列索引</param>
        private void OpenGrid(int row, int column)
        {
            // 如果点击的格子是地雷
            if (Mines.MineGrid[row, column] == -1)
            {
                Grids[row, column].Type = GridType.Mine;
                HitMine?.Invoke(row, column);
                return;
            }

            // 如果点击的格子不是地雷
            int surroundingMines = Mines.MineGrid[row, column];
            --_unopenedCount;

            // 如果周围地雷数量为0, 则打开周围格子
            if (surroundingMines == 0)
            {
                Grids[row, column].Type = GridType.Empty;
                for (int r = row - 1; r <= row + 1; r++)
                {
                    for (int c = column - 1; c <= column + 1; c++)
                    {
                        if (r >= 0 && r < Height && c >= 0 && c < Width && Grids[r, c].Type == GridType.Unopened)
                        {
                            OpenGrid(r, c);
                        }
                    }
                }
            }
            else
            {
                Grids[row, column].Type = GridType.Number;
            }

            Grids[row, column].SurroundingMines = surroundingMines;
            GridOpened?.Invoke(row, column, surroundingMines);
        }

        /// <summary>
        /// 插旗/取消插旗
        /// </summary>
        /// <param name="row">格子的行索引</param>
        /// <param name="column">格子的列索引</param>
        private void ToggleFlag(int row, int column)
        {
            if (Grids[row, column].Type == GridType.Unopened)
            {
                // 插旗
                Grids[row, column].Type = GridType.Flagged;
                GridFlagged?.Invoke(row, column);
                _unopenedCount--;
                _remainingMines--;
                RemainingMinesChanged?.Invoke(_remainingMines);
                if (Mines.MineGrid[row, column] != -1)
                {
                    // 错误标记地雷
                    _wrongFlagCount++;
                }
            }
            else if (Grids[row, column].Type == GridType.Flagged)
            {
                // 取消插旗
                Grids[row, column].Type = GridType.Unopened;
                GridUnflagged?.Invoke(row, column);
                _unopenedCount++;
                _remainingMines++;
                RemainingMinesChanged?.Invoke(_remainingMines);
                if (Mines.MineGrid[row, column] != -1)
                {
                    // 取消错误标记地雷
                    _wrongFlagCount--;
                }
            }
        }

        /// <summary>
        /// 检查游戏是否胜利
        /// </summary>
        /// <returns>是否胜利</returns>
        private bool CheckWin()
        {
            // 未打开格子的数量等于0, 剩余地雷数量也等于0, 且错误标记地雷数量也等于0
            return _unopenedCount == 0 && _remainingMines == 0 && _wrongFlagCount == 0;
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
    }
}