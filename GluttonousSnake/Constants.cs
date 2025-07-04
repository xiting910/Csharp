namespace GluttonousSnake
{
    /// <summary>
    /// 常量类, 用于存储游戏中的常量
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 用户名的最小长度
        /// </summary>
        public const int MinUsernameLength = 2 ;

        /// <summary>
        /// 用户名的最大长度
        /// </summary>
        public const int MaxUsernameLength = 10 ;

        /// <summary>
        /// 密码的最小长度
        /// </summary>
        public const int MinPasswordLength = 6 ;

        /// <summary>
        /// 密码的最大长度
        /// </summary>
        public const int MaxPasswordLength = 15 ;

        /// <summary>
        /// 用户ID的长度
        /// </summary>
        public const int IdLength = 8 ;

        /// <summary>
        /// 用户ID的最小值
        /// </summary>
        public const int MinId = 10000000 ;

        /// <summary>
        /// 用户ID的最大值
        /// </summary>
        public const int MaxId = 99999999 ;

        /// <summary>
        /// 分数的最大长度
        /// </summary>
        public const int MaxScoreLength = 5 ;

        /// <summary>
        /// 排行榜的最大长度
        /// </summary>
        public const int MaxRankListLength = 20 ;

        /// <summary>
        /// 管理员密码
        /// </summary>
        public const string AdministratorPassword = "Lvxiting20190910" ;

        /// <summary>
        /// 数据存储路径
        /// </summary>
        public const string DataPath = @"D:\GluttonousSnakeData" ;

        /// <summary>
        /// 用户文件路径
        /// </summary>
        public const string UserFilePath = @"D:\GluttonousSnakeData\users.json" ;

        /// <summary>
        /// 用户数据文件路径
        /// </summary>
        public const string LogFileCatalogue = @"D:\GluttonousSnakeData\UserData" ;

        /// <summary>
        /// 一个格子的大小
        /// </summary>
        public const int OneGridSize = 2 ;

        /// <summary>
        /// 格子的宽度
        /// </summary>
        public const int GridWidth = 50 ;

        /// <summary>
        /// 格子的高度
        /// </summary>
        public const int GridHeight = 40 ;

        /// <summary>
        /// 需要的窗口宽度
        /// </summary>
        public const int NeedWindowWidth = GridWidth * OneGridSize + 3 ;

        /// <summary>
        /// 需要的窗口高度
        /// </summary>
        public const int NeedWindowHeight = GridHeight + 2 ;

        /// <summary>
        /// 蛇的初始长度
        /// </summary>
        public const int InitialSnakeLength = 3 ;

        /// <summary>
        /// 蛇的初始X坐标
        /// </summary>
        public const int InitialSnakeX = 5 ;

        /// <summary>
        /// 蛇的初始Y坐标
        /// </summary>
        public const int InitialSnakeY = 5 ;

        /// <summary>
        /// 蛇的正常移动速度
        /// </summary>
        public const int NormalSpeed = 120 ;

        /// <summary>
        /// 蛇的加速移动速度
        /// </summary>
        public const int FastSpeed = 60 ;

        /// <summary>
        /// 场上食物的最大数量
        /// </summary>
        public const int MaxFoodCount = 500 ;

        /// <summary>
        /// 生成食物的速度
        /// </summary>
        public const int GenerateFoodSpeed = 1000 ;

        /// <summary>
        /// 一次生成食物的最大数量
        /// </summary>
        public const int GenerateFoodCountOnce = 4 ;
    }
}