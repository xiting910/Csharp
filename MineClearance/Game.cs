namespace MineClearance;

/// <summary>
/// 表示扫雷游戏的主类, 控制游戏的流程和逻辑
/// </summary>
public class Game
{
    /// <summary>
    /// 游戏失败时触发
    /// </summary>
    public event Action<GameResult>? GameLost;

    /// <summary>
    /// 游戏胜利时触发
    /// </summary>
    public event Action<GameResult>? GameWon;

    /// <summary>
    /// 游戏难度
    /// </summary>
    public DifficultyLevel Difficulty { get; private set; }

    /// <summary>
    /// 游戏的开始时间(首次点击时间)
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    /// 地雷总数
    /// </summary>
    public int TotalMines { get; private set; }

    /// <summary>
    /// 游戏棋盘
    /// </summary>
    public Board Board { get; private set; }

    /// <summary>
    /// 构造非自定义难度的扫雷游戏
    /// </summary>
    /// <param name="difficulty">游戏难度</param>
    /// <exception cref="ArgumentException">如果难度为自定义, 则抛出异常</exception>
    public Game(DifficultyLevel difficulty)
    {
        if (difficulty == DifficultyLevel.Custom)
        {
            throw new ArgumentException("自定义难度需要特定的棋盘设置");
        }
        Difficulty = difficulty;
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
        // 检查棋盘尺寸和地雷数量的合法性
        var maxWidth = Constants.MaxBoardWidth;
        var maxHeight = Constants.MaxBoardHeight;
        if (width <= 0 || height <= 0 || mineCount < 0 || width > maxWidth || height > maxHeight)
        {
            throw new ArgumentException("棋盘尺寸不合法");
        }
        if (mineCount >= width * height)
        {
            throw new ArgumentException("地雷数量必须少于总格子数");
        }
        TotalMines = mineCount;
        Difficulty = DifficultyLevel.Custom;
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
            StartTime = DateTime.Now;
        };

        // 监听棋盘的打开地雷事件, 触发时计算游戏结果, 并触发 GameLost 事件
        Board.HitMine += pos =>
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
                TotalMines)
            );
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