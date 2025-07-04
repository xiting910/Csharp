namespace MineClearance
{
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
        /// <summary>`
        /// 二维数组, 记录每个坐标周围地雷的数量, -1表示该坐标是地雷
        /// </summary>
        public int[,] MineGrid { get; private set; } = new int[height, width];

        /// <summary>
        /// 使用Fisher-Yates洗牌算法随机生成地雷位置(不包括首次点击位置和周围格子)
        /// </summary>
        /// <param name="firstClickPos">首次点击的位置</param>
        /// <exception cref="InvalidOperationException">如果剩余格子数量小于地雷数量, 则抛出异常</exception>
        public void GenerateMines((int row, int column) firstClickPos)
        {
            int totalCells = _width * _height;
            int[] minePositions = [.. Enumerable.Range(0, totalCells)];

            // 确保首次点击位置和其周围格子不包含地雷
            int firstClickIndex = firstClickPos.row * _width + firstClickPos.column;
            minePositions = [.. minePositions.Where(pos =>
                pos != firstClickIndex &&
                !IsAdjacentToFirstClick(pos, firstClickPos))];

            // 如果剩余格子数量小于地雷数量, 抛出异常
            if (minePositions.Length < _mineCount)
            {
                throw new InvalidOperationException("Not enough cells to place mines without hitting the first click area.");
            }

            // 随机打乱地雷位置数组, 并取前 _mineCount 个位置作为地雷位置
            Methods.RandomInstance.Shuffle(minePositions);
            minePositions = [.. minePositions.Take(_mineCount)];

            // 更新MineGrid数组
            foreach (int pos in minePositions)
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
        private bool IsAdjacentToFirstClick(int pos, (int row, int column) firstClickPos)
        {
            int row = pos / _width;
            int column = pos % _width;

            return Math.Abs(row - firstClickPos.row) <= 1 && Math.Abs(column - firstClickPos.column) <= 1;
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

        /// <summary>
        /// 获取所有地雷的位置
        /// </summary>
        /// <returns>地雷的位置列表</returns>
        public List<(int row, int column)> GetAllMinePositions()
        {
            var minePositions = new List<(int, int)>();
            for (int row = 0; row < _height; row++)
            {
                for (int column = 0; column < _width; column++)
                {
                    if (MineGrid[row, column] == -1)
                    {
                        minePositions.Add((row, column));
                    }
                }
            }
            return minePositions;
        }
    }
}