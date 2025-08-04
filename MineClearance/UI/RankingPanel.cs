namespace MineClearance;

/// <summary>
/// 排行榜面板类
/// </summary>
public partial class RankingPanel : Panel
{
    /// <summary>
    /// 排行榜顶部面板
    /// </summary>
    private readonly Panel rankingTopPanel;

    /// <summary>
    /// 排行榜列表框
    /// </summary>
    private readonly DoubleBufferedListView rankingListView;

    /// <summary>
    /// 要显示的排行榜信息列表
    /// </summary>
    private List<string> showRankingList;

    /// <summary>
    /// 初始化排行榜面板
    /// </summary>
    public RankingPanel()
    {
        // 设置排行榜面板属性
        Name = "RankingPanel";
        Location = new(0, 0);
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight - Constants.BottomStatusBarHeight);

        // 初始化排行榜信息列表
        showRankingList = [];

        // 排行榜顶部面板高度
        var rankingTopPanelHeight = (int)(43f * Constants.DpiScale);

        // 创建排行榜顶部面板
        rankingTopPanel = new()
        {
            Name = "RankingTopPanel",
            Size = new(Constants.MainFormWidth, rankingTopPanelHeight),
            BackColor = Color.LightSalmon,
            Location = new(0, 0)
        };

        // 按钮X位置和Y位置
        var buttonXPosition = (int)(10 * Constants.DpiScale);
        var buttonYPosition = (int)(12 * Constants.DpiScale);

        // 按钮宽度和高度
        var buttonWidth = (int)(115 * Constants.DpiScale);
        var buttonHeight = (int)(20 * Constants.DpiScale);

        // 按钮水平间距
        var buttonSpacing = (int)(10 * Constants.DpiScale);

        // 添加按钮显示统计信息
        Button btnShowStatistics = new()
        {
            Text = "显示统计信息",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        buttonXPosition += buttonWidth + buttonSpacing;
        btnShowStatistics.Click += (sender, e) => RestartRankingPanel();
        rankingTopPanel.Controls.Add(btnShowStatistics);

        // 添加按钮以按开始时间排序
        Button btnSortByStartTime = new()
        {
            Text = "按开始时间排序",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        buttonXPosition += buttonWidth + buttonSpacing;
        btnSortByStartTime.Click += (sender, e) => RestartRankingPanel(RankingDisplayMode.ByStartTime);
        rankingTopPanel.Controls.Add(btnSortByStartTime);

        // 添加按钮以按用时排序
        Button btnSortByDuration = new()
        {
            Text = "按难度和用时排序",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        btnSortByDuration.Click += (sender, e) => RestartRankingPanel(RankingDisplayMode.ByDuration);
        rankingTopPanel.Controls.Add(btnSortByDuration);

        // 更新按钮宽度
        buttonWidth = (int)(70 * Constants.DpiScale);

        // 更新按钮X位置
        buttonXPosition = Constants.MainFormWidth - buttonWidth - 2 * buttonSpacing;

        // 添加按钮以返回菜单
        Button btnBackMenu = new()
        {
            Text = "返回菜单",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightCoral,
            FlatStyle = FlatStyle.Flat
        };
        buttonXPosition -= btnBackMenu.Width + buttonSpacing;
        btnBackMenu.Click += (sender, e) => MainForm.ShowPanel(PanelType.Menu);
        rankingTopPanel.Controls.Add(btnBackMenu);

        // 添加按钮以清除历史记录
        Button btnClearHistory = new()
        {
            Text = "清除历史",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.Red,
            FlatStyle = FlatStyle.Flat
        };
        btnClearHistory.Click += BtnClearHistory_Click;
        rankingTopPanel.Controls.Add(btnClearHistory);

        // 创建排行榜列表框
        rankingListView = new()
        {
            Name = "RankingListView",
            BackColor = Color.White,
            ForeColor = Color.Black,
            View = View.Details,
            VirtualMode = true,
            FullRowSelect = true,
            VirtualListSize = showRankingList.Count,
            Location = new(0, rankingTopPanel.Height),
            Size = new(Constants.MainFormWidth - (int)(12 * Constants.DpiScale), Constants.MainFormHeight - rankingTopPanelHeight - Constants.BottomStatusBarHeight)
        };

        // 添加一列，宽度设置为控件宽度
        var width = rankingListView.Width - (int)(20 * Constants.DpiScale);
        rankingListView.Columns.Add("历史记录", width);

        // 设置列表框的RetrieveVirtualItem事件处理
        rankingListView.RetrieveVirtualItem += (s, e) =>
        {
            e.Item = new ListViewItem(showRankingList[e.ItemIndex]);
        };

        // 添加排行榜顶部面板和列表框到排行榜面板
        Controls.Add(rankingTopPanel);
        Controls.Add(rankingListView);
    }

    /// <summary>
    /// 重启排行榜面板
    /// </summary>
    /// <param name="displayMode">显示模式</param>
    public void RestartRankingPanel(RankingDisplayMode displayMode = RankingDisplayMode.Default)
    {
        // 根据显示模式获取游戏结果
        var gameResults = Datas.GetSortedGameResults(displayMode);
        showRankingList = [];

        if (gameResults.Count == 0)
        {
            showRankingList.Add("暂无游戏记录");
        }
        else
        {
            switch (displayMode)
            {
                case RankingDisplayMode.Default:
                    UpdateDefaultRanking(gameResults);
                    break;
                case RankingDisplayMode.ByStartTime:
                    foreach (var result in gameResults)
                    {
                        showRankingList.Add(result.ToString());
                    }
                    break;
                case RankingDisplayMode.ByDuration:
                    foreach (var result in gameResults)
                    {
                        showRankingList.Add(result.ToString());
                    }
                    break;
                default:
                    MessageBox.Show("未知的显示模式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        // 更新列表框的虚拟列表大小和内容
        rankingListView.VirtualListSize = showRankingList.Count;
        rankingListView.Invalidate();
    }

    /// <summary>
    /// 更新默认排行榜
    /// </summary>
    /// <param name="gameResults">游戏结果列表</param>
    private void UpdateDefaultRanking(List<GameResult> gameResults)
    {
        // 显示总游戏次数
        showRankingList.Add($"总游戏次数: {gameResults.Count}, 其中:");

        // 统计各难度的游戏次数、胜利次数、总胜利用时、总完成度
        var difficultyStats = new Dictionary<DifficultyLevel, (int total, int wins, TimeSpan totalDuration, double totalCompletion, TimeSpan shortestDuration)>
            {
                { DifficultyLevel.Easy, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
                { DifficultyLevel.Medium, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
                { DifficultyLevel.Hard, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
                { DifficultyLevel.Hell, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
                { DifficultyLevel.Custom, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) }
            };
        foreach (var result in gameResults)
        {
            var stats = difficultyStats[result.Difficulty];
            ++stats.total;
            stats.wins += result.IsWin ? 1 : 0;
            stats.totalDuration += result.IsWin ? result.Duration : TimeSpan.Zero;
            stats.totalCompletion += result.Completion;
            stats.shortestDuration = result.IsWin && result.Duration < stats.shortestDuration ? result.Duration : stats.shortestDuration;
            difficultyStats[result.Difficulty] = stats;
        }

        // 计算平均胜利率、平均胜利用时和平均完成度
        foreach (var (level, stats) in difficultyStats)
        {
            // 格式化难度名称
            var formattedDifficulty = level switch
            {
                DifficultyLevel.Easy => "简单",
                DifficultyLevel.Medium => "普通",
                DifficultyLevel.Hard => "困难",
                DifficultyLevel.Hell => "地狱",
                DifficultyLevel.Custom => "自定义",
                _ => "未知"
            };

            if (stats.total == 0)
            {
                showRankingList.Add($"难度: {formattedDifficulty}, 游戏次数: 0");
                continue;
            }

            var winRate = (double)stats.wins / stats.total;
            var avgDuration = stats.wins > 0 ? TimeSpan.FromMilliseconds(stats.totalDuration.TotalMilliseconds / stats.wins) : TimeSpan.Zero;
            var avgCompletion = stats.totalCompletion / stats.total;

            // 格式化用时为 xx:xx.xx 格式
            var formattedDuration = $"{(int)avgDuration.TotalMinutes:D2}:{avgDuration.Seconds:D2}.{avgDuration.Milliseconds / 10:D2}";

            // 完成度格式化为百分比, 保留两位小数
            var formattedCompletion = $"{avgCompletion,6:0.00}%";

            // 格式化最短胜利用时
            var formattedShortestDuration = stats.shortestDuration == TimeSpan.MaxValue ? "无" : $"{(int)stats.shortestDuration.TotalMinutes:D2}:{stats.shortestDuration.Seconds:D2}.{stats.shortestDuration.Milliseconds / 10:D2}";

            // 添加到排行榜列表
            showRankingList.Add($"难度: {formattedDifficulty}, 游戏次数: {stats.total}, 胜利次数: {stats.wins}, 胜利率: {winRate:P2}, 平均胜利用时: {formattedDuration}, 平均完成度: {formattedCompletion}, 最短胜利用时: {formattedShortestDuration}");
        }

        showRankingList.Add("\n");
        showRankingList.Add("点击按开始时间排序按钮可以查看所有游戏记录");
        showRankingList.Add("点击按难度和用时排序按钮可以将所有非自定义难度的胜利游戏记录先按难度高低后按用时快慢进行排序");
    }

    /// <summary>
    /// 清除历史记录按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void BtnClearHistory_Click(object? sender, EventArgs e)
    {
        // 添加确认对话框
        var confirmResult = MessageBox.Show("确定要清除所有历史记录吗？\n注意: 一旦清除将无法找回！！！", "清除历史记录", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

        // 用户选择取消，直接返回
        if (confirmResult != DialogResult.Yes)
        {
            return;
        }

        // 清空排行榜数据
        Task.Run(Datas.ClearGameResultsAsync);
        showRankingList = ["暂无游戏记录"];

        // 更新列表框
        rankingListView.VirtualListSize = showRankingList.Count;
        rankingListView.Invalidate();

        // 弹窗提示清除成功
        MessageBox.Show("历史记录已清除！", "清除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}