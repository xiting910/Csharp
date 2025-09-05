namespace GluttonousSnake
{
    /// <summary>
    /// 一些线程变量, 用来跨线程传递信息
    /// </summary>
    public static class ThreadVariables
    {
        private static bool _isWindows = true ;
        private static bool _isLoadUserSystem = true ;
        private static bool _isLoadUserDatas = true ;
        private static Direction _nextDirection = Direction.Right ;
        private static bool _isAccelarate = false ;
        private static bool _isPaused = false ;
        private static bool _isGameOver = false ;

        /// <summary>
        /// 是否是Windows系统
        /// </summary>
        public static bool IsWindowsSystem
        {
            get => _isWindows ;
            set => _isWindows = value ;
        }
        
        /// <summary>
        /// 加载用户系统是否完成
        /// </summary>
        public static bool IsLoadUserSystem
        {
            get => _isLoadUserSystem ;
            set => _isLoadUserSystem = value ;
        }
        
        /// <summary>
        /// 加载用户数据是否完成
        /// </summary>
        public static bool IsLoadUserDatas
        {
            get => _isLoadUserDatas ;
            set => _isLoadUserDatas = value ;
        }

        /// <summary>
        /// 下一个移动方向
        /// </summary>
        public static Direction NextDirection
        {
            get => _nextDirection ;
            set => _nextDirection = value ;
        }

        /// <summary>
        /// 是否加速
        /// </summary>
        public static bool IsAccelarate
        {
            get => _isAccelarate ;
            set => _isAccelarate = value ;
        }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public static bool IsPaused
        {
            get => _isPaused ;
            set => _isPaused = value ;
        }

        /// <summary>
        /// 游戏是否结束
        /// </summary>
        public static bool IsGameOver
        {
            get => _isGameOver ;
            set => _isGameOver = value ;
        }
    }
}