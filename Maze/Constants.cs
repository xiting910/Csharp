namespace Maze
{
    /// <summary>
    /// 常量类, 提供一些常用的常量
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 作者名字
        /// </summary>
        public readonly static string AuthorName = "xiting910";

        /// <summary>
        /// GitHub 仓库链接
        /// </summary>
        public readonly static string GitHubRepoUrl = "https://github.com/xiting910/Csharp/tree/main/Maze";

        /// <summary>
        /// 数据存储路径
        /// </summary>
        public readonly static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MazeData");

        /// <summary>
        /// 错误文件路径
        /// </summary>
        public readonly static string ErrorFilePath = Path.Combine(DataPath, "error.log");

        /// <summary>
        /// 主窗体的宽度
        /// </summary>
        public readonly static int MainFormWidth = 1266;

        /// <summary>
        /// 主窗体的高度
        /// </summary>
        public readonly static int MainFormHeight = 900;

        /// <summary>
        /// 主信息面板的高度
        /// </summary>
        public readonly static int MainInfoPanelHeight = 40;

        /// <summary>
        /// 迷宫的宽度
        /// </summary>
        public readonly static int MazeWidth = 125;

        /// <summary>
        /// 迷宫的高度
        /// </summary>
        public readonly static int MazeHeight = 80;

        /// <summary>
        /// 迷宫单元格的大小
        /// </summary>
        public readonly static int GridSize = 10;

        /// <summary>
        /// 随机生成迷宫的障碍物比例
        /// </summary>
        public readonly static float ObstacleRatio = 0.3f;

        /// <summary>
        /// 取消搜索时最大等待时间
        /// </summary>
        public readonly static TimeSpan MaxCancellationWaitTime = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 空格的颜色
        /// </summary>
        public readonly static Color EmptyColor = Color.LightGray;

        /// <summary>
        /// 起点的颜色
        /// </summary>
        public readonly static Color StartColor = Color.SkyBlue;

        /// <summary>
        /// 终点的颜色
        /// </summary>
        public readonly static Color EndColor = Color.SkyBlue;

        /// <summary>
        /// 障碍物的颜色
        /// </summary>
        public readonly static Color ObstacleColor = Color.Red;

        /// <summary>
        /// 路径的颜色
        /// </summary>
        public readonly static Color PathColor = Color.Green;

        /// <summary>
        /// 淡出路径的颜色
        /// </summary>
        public readonly static Color FadePathColor = Color.LightGreen;
    }
}