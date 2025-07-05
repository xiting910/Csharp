namespace MineClearance
{
    /// <summary>
    /// 提供一些常量
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 数据存储路径
        /// </summary>
        public readonly static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MineClearanceData");

        /// <summary>
        /// 数据文件路径
        /// </summary>
        public readonly static string DataFilePath = Path.Combine(DataPath, "history.json");

        /// <summary>
        /// 简单、中等、困难的棋盘大小和地雷数量
        /// </summary>
        public static class BoardSettings
        {
            public static (int width, int height, int mineCount) GetSettings(DifficultyLevel level)
            {
                return level switch
                {
                    DifficultyLevel.Easy => (9, 9, 10),
                    DifficultyLevel.Medium => (16, 16, 40),
                    DifficultyLevel.Hard => (30, 16, 99),
                    _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
                };
            }
        }

        /// <summary>
        /// 自定义棋盘最大宽度和高度
        /// </summary>
        public static class CustomBoardSettings
        {
            public static (int maxWidth, int maxHeight) GetMaxDimensions()
            {
                return (50, 30);
            }
        }

        /// <summary>
        /// 雷的密度阈值, 大于等于此值时, 不确保地雷周围全是地雷的情况不会出现
        /// </summary>
        public static readonly float mineDensityThreshold = 0.3f;

        /// <summary>
        /// 按钮大小
        /// </summary>
        public static readonly int buttonSize = 25;
    }
}