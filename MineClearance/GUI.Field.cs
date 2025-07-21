namespace MineClearance
{
    public partial class GUI
    {
        /// <summary>
        /// 菜单面板, 包含新游戏、显示排行榜和退出按钮
        /// </summary>
        private Panel? menuPanel;
        /// <summary>
        /// 游戏准备面板, 选择难度或者自定义
        /// </summary>
        private Panel? gamePreparationPanel;
        /// <summary>
        /// 游戏面板, 显示游戏
        /// </summary>
        private Panel? gamePanel;
        /// <summary>
        /// 排行榜面板, 显示游戏历史记录
        /// </summary>  
        private Panel? rankingPanel;
        /// <summary>
        /// 游戏顶部信息栏
        /// </summary>
        private Panel? infoPanel;
        /// <summary>
        /// 游戏区域面板, 显示游戏格子
        /// </summary>
        private DoubleBufferedPanel? gameAreaPanel;
        /// <summary>
        /// 剩余地雷数标签
        /// </summary>
        private Label? minesLeftLabel;
        /// <summary>
        /// 游戏时间标签
        /// </summary>
        private Label? gameTimeLabel;
        /// <summary>
        /// 游戏计时器
        /// </summary>
        private System.Windows.Forms.Timer? gameTimer;
        /// <summary>
        /// 游戏开始时间
        /// </summary>
        private DateTime gameStartTime;
        /// <summary>
        /// 游戏格子按钮
        /// </summary>
        private Button[,]? gameButtons;
        /// <summary>
        /// 游戏实例, 控制游戏逻辑
        /// </summary>
        private Game? gameInstance;
        /// <summary>
        /// 排行榜顶部面板
        /// </summary>
        private Panel? rankingTopPanel;
        /// <summary>
        /// 排行榜列表框
        /// </summary>
        private ListBox? rankingListBox;
        /// <summary>
        /// 要显示的排行榜信息列表
        /// </summary>
        private List<string> showRankingList;
        /// <summary>
        /// 是否正在处理更新事件
        /// </summary>
        private volatile bool isHandlingUpdateEvent;
    }
}