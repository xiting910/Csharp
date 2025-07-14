namespace MineClearance
{
    /// <summary>
    /// 表示扫雷游戏的主类, 控制游戏的流程和逻辑
    /// </summary>
    public class Game
    {
        /// <summary>
        /// 游戏失败时触发
        /// </summary>
        public event Action<GameResult, List<(int, int)>>? GameLost;
        /// <summary>
        /// 游戏胜利时触发
        /// </summary>
        public event Action<GameResult>? GameWon;

        /// <summary>
        /// 游戏难度
        /// </summary>
        public DifficultyLevel Difficulty { get; set; }

        /// <summary>
        /// 游戏的开始时间(首次点击时间)
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// 地雷总数
        /// </summary>
        public int TotalMines { get; set; }

        /// <summary>
        /// 游戏棋盘
        /// </summary>
        public Board Board { get; set; }

        /// <summary>
        /// 构造非自定义难度的扫雷游戏
        /// </summary>
        /// <param name="difficulty">游戏难度</param>
        /// <exception cref="ArgumentException">如果难度为自定义, 则抛出异常</exception>
        public Game(DifficultyLevel difficulty)
        {
            Difficulty = difficulty;
            if (difficulty == DifficultyLevel.Custom)
            {
                throw new ArgumentException("Custom difficulty requires specific board settings.");
            }
            var (width, height, mineCount) = Constants.BoardSettings.GetSettings(difficulty);
            TotalMines = mineCount;
            Board = new Board(width, height, mineCount);
        }

        /// <summary>
        /// 构造自定义难度的扫雷游戏
        /// </summary>
        /// <param name="width">棋盘宽度</param>
        /// <param name="height">棋盘高度</param>
        /// <param name="mineCount">地雷数量</param>
        /// <exception cref="ArgumentOutOfRangeException">如果棋盘尺寸或地雷数量不合法, 则抛出异常</exception>
        /// <exception cref="ArgumentException">如果地雷数量超过棋盘格子总数, 则抛出异常</exception>
        public Game(int width, int height, int mineCount)
        {
            Difficulty = DifficultyLevel.Custom;
            var (maxWidth, maxHeight) = Constants.CustomBoardSettings.GetMaxDimensions();
            if (width <= 0 || height <= 0 || mineCount < 0 || width > maxWidth || height > maxHeight)
            {
                throw new ArgumentOutOfRangeException(
                    $"Board dimensions must be between 1 and {maxWidth} for width and 1 and {maxHeight} for height, with mine count non-negative.");
            }
            if (mineCount >= width * height)
            {
                throw new ArgumentException("Mine count must be less than total cells.");
            }
            TotalMines = mineCount;
            Board = new Board(width, height, mineCount);
        }

        /// <summary>
        /// 运行游戏
        /// </summary>
        public void Run()
        {
            // 监听棋盘的第一次点击事件
            Board.FirstClick += () =>
            {
                StartTime = DateTime.Now; // 记录开始时间
            };

            // 监听棋盘的打开地雷事件
            Board.HitMine += (row, column) =>
            {
                // 记录结束时间
                var endTime = DateTime.Now;

                // 计算游戏时长
                var duration = endTime - StartTime;

                // 计算完成度
                var completion = (double)Board.GetCorrectFlagCount() / TotalMines;

                // 触发 GameLost 事件
                GameLost?.Invoke(new GameResult(
                    Difficulty,
                    StartTime,
                    duration,
                    false,
                    completion * 100.0,
                    Board.Width,
                    Board.Height,
                    TotalMines), Board.Mines.GetAllMinePositions());
            };

            // 监听棋盘的胜利事件, 触发时计算游戏结果, 并触发 GameWon 事件
            Board.Won += () =>
            {
                // 记录结束时间
                var endTime = DateTime.Now;

                // 计算游戏时长
                var duration = endTime - StartTime;

                // 触发 GameWon 事件
                GameWon?.Invoke(new GameResult(
                    Difficulty,
                    StartTime,
                    duration,
                    true,
                    100.0,
                    Board.Width,
                    Board.Height,
                    TotalMines)
                );
            };
        }
    }
}