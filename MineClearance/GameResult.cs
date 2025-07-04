namespace MineClearance
{
    /// <summary>
    /// 表示扫雷游戏的结果
    /// </summary>
    /// <param name="difficulty">游戏难度级别</param>
    /// <param name="startTime">游戏开始时间（首次点击时间）</param>
    /// <param name="duration">游戏持续时间</param>
    /// <param name="isWin">游戏是否获胜</param>
    /// <param name="completion">游戏完成度，取值范围 0 到 100</param>
    /// <param name="boardWidth">棋盘宽度</param>
    /// <param name="boardHeight">棋盘高度</param>
    /// <param name="mineCount">地雷总数</param>
    public class GameResult(DifficultyLevel difficulty, DateTime startTime, TimeSpan duration, bool isWin, double completion, int boardWidth, int boardHeight, int mineCount)
    {
        /// <summary>
        /// 游戏难度
        /// </summary>
        public DifficultyLevel Difficulty { get; private set; } = difficulty;

        /// <summary>
        /// 游戏的开始时间(首次点击时间)
        /// </summary>
        public DateTime StartTime { get; private set; } = startTime;

        /// <summary>
        /// 游戏时间
        /// </summary>
        public TimeSpan Duration { get; private set; } = duration;

        /// <summary>
        /// 游戏的结果（胜利或失败）
        /// </summary>
        public bool IsWin { get; private set; } = isWin;

        /// <summary>
        /// 游戏完成度, 取值范围 0 到 100
        /// </summary>
        public double Completion { get; private set; } = completion;

        /// <summary>
        /// 棋盘宽度
        /// </summary>
        public int BoardWidth { get; private set; } = boardWidth;

        /// <summary>
        /// 棋盘高度
        /// </summary>
        public int BoardHeight { get; private set; } = boardHeight;

        /// <summary>
        /// 地雷数量
        /// </summary>
        public int MineCount { get; private set; } = mineCount;

        /// <summary>
        /// 获取非自定义难度游戏结果的字符串表示
        /// </summary>
        /// <param name="requiresWon">是否只需要胜利的结果</param>
        /// <returns>游戏结果的字符串表示</returns>
        public string ToString(bool requiresWon = false)
        {
            if (requiresWon && !IsWin)
            {
                return string.Empty; // 如果只需要胜利的结果，且当前结果不是胜利，则返回空字符串
            }

            // 格式化时间为 xx:xx:xx.xx 格式
            string formattedDuration = $"{(int)Duration.TotalMinutes:D2}:{Duration.Seconds:D2}:{Duration.Milliseconds / 10:D2}";

            if (requiresWon)
            {
                return $"开始时间: {StartTime}, 难度: {Difficulty}, 用时: {formattedDuration}";
            }
            else
            {
                return $"开始时间: {StartTime}, 难度: {Difficulty}, 结果: {(IsWin ? "胜利" : "失败")}, 完成度: {Completion}%, 用时: {formattedDuration}";
            }
        }
    }
}