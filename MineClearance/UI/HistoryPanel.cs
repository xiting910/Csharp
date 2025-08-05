namespace MineClearance;

/// <summary>
/// 历史记录面板类
/// </summary>
public partial class HistoryPanel : Panel
{
    /// <summary>
    /// 历史记录顶部面板高度
    /// </summary>
    private static readonly int historyTopPanelHeight = (int)(43 * Constants.DpiScale);

    /// <summary>
    /// 历史记录顶部面板
    /// </summary>
    private readonly Panel historyTopPanel;

    /// <summary>
    /// 统计信息列表
    /// </summary>
    private readonly ListBox statisticsListBox;

    /// <summary>
    /// 历史记录列表框
    /// </summary>
    private readonly DoubleBufferedDataGridView historyDataGridView;

    /// <summary>
    /// 提示信息标签
    /// </summary>
    private readonly Label tipsLabel;

    /// <summary>
    /// 初始化历史记录面板
    /// </summary>
    public HistoryPanel()
    {
        // 设置历史记录面板属性
        Name = "HistoryPanel";
        Location = new(0, 0);
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight - Constants.BottomStatusBarHeight);

        // 创建历史记录顶部信息面板
        historyTopPanel = CreateHistoryTopInfoPanel();

        // 添加提示标签
        tipsLabel = new()
        {
            Text = "点击列头可进行排序和筛选, 点击显示详细历史记录按钮会清除所有筛选和排序条件",
            Font = new("Arial", 6 * Constants.DpiScale, FontStyle.Regular),
            Location = new((int)(450 * Constants.DpiScale), (int)(12 * Constants.DpiScale)),
            ForeColor = Color.DarkBlue,
            AutoSize = true
        };
        historyTopPanel.Controls.Add(tipsLabel);

        // 创建统计信息列表框
        statisticsListBox = CreateStatisticsListBox();

        // 创建历史记录数据网格视图
        historyDataGridView = CreateHistoryDataGridView();

        // 添加历史记录顶部面板和列表框到排行榜面板
        Controls.Add(historyTopPanel);
        Controls.Add(statisticsListBox);
        Controls.Add(historyDataGridView);
    }

    /// <summary>
    /// 重启历史记录面板
    /// </summary>
    /// <param name="showHistory">是否显示详细历史记录</param>
    public void RestartHistoryPanel(bool showHistory = false)
    {
        // 如果是显示详细历史记录，则显示历史记录数据网格视图
        if (showHistory)
        {
            // 切换到历史记录数据网格视图
            statisticsListBox.Visible = false;
            historyDataGridView.Visible = true;

            // 提示信息标签可见
            tipsLabel.Visible = true;

            // 重置右键菜单实例(会自动更新数据网格视图)
            HistoryContextMenu.ResetInstances();
        }
        else
        {
            // 切换到统计信息列表框
            historyDataGridView.Visible = false;
            statisticsListBox.Visible = true;

            // 提示信息标签不可见
            tipsLabel.Visible = false;

            // 更新统计信息列表框
            UpdateStatisticsListBox();
        }
    }

    /// <summary>
    /// 创建历史记录顶部信息面板
    /// </summary>
    /// <returns>返回创建的面板</returns>
    private Panel CreateHistoryTopInfoPanel()
    {
        // 创建历史记录顶部面板
        var panel = new Panel
        {
            Name = "HistoryTopPanel",
            Size = new(Constants.MainFormWidth, historyTopPanelHeight),
            BackColor = Color.LightSalmon,
            Location = new(0, 0)
        };

        // 添加标题标签
        Label titleLabel = new()
        {
            Text = "历史记录",
            Font = new("Arial", 12 * Constants.DpiScale, FontStyle.Bold),
            Location = new((int)(5 * Constants.DpiScale), (int)(2 * Constants.DpiScale)),
            AutoSize = true
        };
        panel.Controls.Add(titleLabel);

        // 按钮X位置和Y位置
        var buttonXPosition = (int)(170 * Constants.DpiScale);
        var buttonYPosition = (int)(9 * Constants.DpiScale);

        // 按钮宽度和高度
        var buttonWidth = (int)(120 * Constants.DpiScale);
        var buttonHeight = (int)(25 * Constants.DpiScale);

        // 按钮水平间距
        var buttonSpacing = (int)(10 * Constants.DpiScale);

        // 添加按钮以显示统计信息
        Button btnShowStatistics = new()
        {
            Text = "显示统计信息",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        btnShowStatistics.Click += (sender, e) => RestartHistoryPanel();
        buttonXPosition += buttonWidth + buttonSpacing;
        panel.Controls.Add(btnShowStatistics);

        // 添加按钮以显示详细历史记录
        Button btnShowHistory = new()
        {
            Text = "显示详细历史记录",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnShowHistory.Click += (sender, e) => RestartHistoryPanel(true);
        buttonXPosition += buttonWidth + 2 * buttonSpacing;
        panel.Controls.Add(btnShowHistory);

        // 更新按钮宽度
        buttonWidth = (int)(70 * Constants.DpiScale);

        // 更新按钮X位置
        buttonXPosition = Constants.MainFormWidth - buttonWidth - 4 * buttonSpacing;

        // 更新按钮间距
        buttonSpacing *= 3;

        // 添加按钮以返回菜单
        Button btnBackMenu = new()
        {
            Text = "返回菜单",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightCoral,
            FlatStyle = FlatStyle.Flat
        };
        buttonXPosition -= buttonWidth + buttonSpacing;
        btnBackMenu.Click += (sender, e) => MainForm.ShowPanel(PanelType.Menu);
        panel.Controls.Add(btnBackMenu);

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
        panel.Controls.Add(btnClearHistory);

        return panel;
    }

    /// <summary>
    /// 创建统计信息列表框
    /// </summary>
    /// <returns>返回创建的列表框</returns>
    private static ListBox CreateStatisticsListBox()
    {
        // 统计信息列表框的宽度和高度
        var listBoxWidth = Constants.MainFormWidth - (int)(12 * Constants.DpiScale);
        var listBoxHeight = Constants.MainFormHeight - historyTopPanelHeight - Constants.BottomStatusBarHeight + (int)(10 * Constants.DpiScale);

        // 创建统计信息列表框
        var listBox = new ListBox
        {
            Name = "StatisticsListBox",
            BackColor = Color.White,
            ForeColor = Color.Black,
            Size = new(listBoxWidth, listBoxHeight),
            Location = new(0, historyTopPanelHeight)
        };
        return listBox;
    }

    /// <summary>
    /// 创建历史记录数据网格视图
    /// </summary>
    /// <returns>返回创建的历史记录数据网格视图</returns>
    private DoubleBufferedDataGridView CreateHistoryDataGridView()
    {
        // 计算历史记录数据网格视图的宽度和高度
        var dataGridViewWidth = Constants.MainFormWidth - (int)(12 * Constants.DpiScale);
        var dataGridViewHeight = Constants.MainFormHeight - historyTopPanelHeight - Constants.BottomStatusBarHeight;

        // 设置数据网格视图的列头高度
        var columnHeaderHeight = (int)(29 * Constants.DpiScale);

        // 设置数据网格视图的行高
        var rowHeight = (int)(25 * Constants.DpiScale);

        // 创建历史记录数据网格视图
        var dataGridView = new DoubleBufferedDataGridView
        {
            Name = "HistoryListView",
            VirtualMode = true,
            BackColor = Color.White,
            ForeColor = Color.Black,
            RowHeadersVisible = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            AllowUserToResizeColumns = false,
            Location = new(0, historyTopPanelHeight),
            Size = new(dataGridViewWidth, dataGridViewHeight),
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = columnHeaderHeight,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = rowHeight }
        };

        // 保存每一列的名字和最小宽度的字典
        var columnDefinitions = new Dictionary<string, int>
        {
            { "序号", (int)(80 * Constants.DpiScale) },
            { "开始时间", 0 },
            { "难度", (int)(160 * Constants.DpiScale) },
            { "结果", (int)(160 * Constants.DpiScale) },
            { "完成度", (int)(160 * Constants.DpiScale) },
            { "用时", (int)(160 * Constants.DpiScale) },
            { "宽度", (int)(100 * Constants.DpiScale) },
            { "高度", (int)(100 * Constants.DpiScale) },
            { "地雷数", (int)(100 * Constants.DpiScale) }
        };

        // 剩余宽度
        var remainWidth = dataGridViewWidth - columnDefinitions.Values.Sum();
        remainWidth -= (int)(19 * Constants.DpiScale);

        // 开始时间列增加剩余宽度
        columnDefinitions["开始时间"] += remainWidth;

        // 添加所有列
        foreach (var (name, width) in columnDefinitions)
        {
            // 对齐方式
            var alignment = DataGridViewContentAlignment.MiddleCenter;

            // 创建列
            var column = new DataGridViewTextBoxColumn
            {
                Name = name,
                Width = width,
                ReadOnly = true,
                HeaderText = "    " + name,
                DefaultCellStyle = { Alignment = alignment }
            };

            // 设置列头对齐方式
            column.HeaderCell.Style.Alignment = alignment;
            column.HeaderCell.Style.Font = new("微软雅黑", 11F, FontStyle.Bold);
            column.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            column.HeaderCell.Style.ForeColor = Color.DarkBlue;

            // 添加列到数据网格视图
            dataGridView.Columns.Add(column);
        }

        // 订阅列头点击事件
        dataGridView.ColumnHeaderMouseClick += (sender, e) =>
        {
            // 获取当前列的名字
            var columnName = dataGridView.Columns[e.ColumnIndex].Name;

            // 获取右键菜单实例
            var contextMenu = HistoryContextMenu.GetInstance(columnName);

            // 显示右键菜单
            contextMenu.Show(Cursor.Position);
        };

        // 订阅 ResultManager 的条件变化事件
        ResultManager.ConditionsChanged += UpdateHistoryDataGridView;

        // 订阅 CellValueNeeded 事件
        dataGridView.CellValueNeeded += HistoryDataGridView_CellValueNeeded;

        // 选择时清除选择
        dataGridView.SelectionChanged += (s, e) => dataGridView.ClearSelection();
        return dataGridView;
    }

    /// <summary>
    /// 更新统计信息列表框
    /// </summary>
    private void UpdateStatisticsListBox()
    {
        // 获取所有游戏结果
        var gameResults = Datas.GameResults;

        // 清空统计信息列表框
        statisticsListBox.Items.Clear();

        // 显示总游戏次数
        statisticsListBox.Items.Add($"总游戏次数: {gameResults.Count}, 其中:");

        // 使用字典来统计每个难度的游戏结果
        var difficultyStats = new Dictionary<DifficultyLevel, (int total, int wins, TimeSpan totalDuration, double totalCompletion, TimeSpan shortestDuration)>
        {
            { DifficultyLevel.Easy, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
            { DifficultyLevel.Medium, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
            { DifficultyLevel.Hard, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
            { DifficultyLevel.Hell, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
            { DifficultyLevel.Custom, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) }
        };

        // 统计每个难度的游戏结果
        foreach (var result in gameResults)
        {
            var stats = difficultyStats[result.Difficulty];
            ++stats.total;
            stats.wins += result.IsWin ? 1 : 0;
            stats.totalDuration += result.IsWin ? result.Duration : TimeSpan.Zero;
            stats.totalCompletion += result.Completion ?? 100.0;
            stats.shortestDuration = (result.IsWin && result.Duration < stats.shortestDuration) ? result.Duration : stats.shortestDuration;
            difficultyStats[result.Difficulty] = stats;
        }

        // 计算平均胜利率、平均胜利用时和平均完成度
        foreach (var (level, stats) in difficultyStats)
        {
            // 格式化难度名称
            var formattedDifficulty = Methods.GetDifficultyText(level);

            // 如果没有游戏记录，则跳过
            if (stats.total == 0)
            {
                statisticsListBox.Items.Add($"难度: {formattedDifficulty}, 游戏次数: 0");
                continue;
            }

            // 计算胜率、平均胜利用时和平均完成度
            var winRate = (double)stats.wins / stats.total;
            var avgDuration = stats.wins > 0 ? TimeSpan.FromMilliseconds(stats.totalDuration.TotalMilliseconds / stats.wins) : TimeSpan.Zero;
            var avgCompletion = stats.totalCompletion / stats.total;

            // 格式化用时为 xx:xx.xx 格式
            var formattedDuration = $"{(int)avgDuration.TotalMinutes:D2}:{avgDuration.Seconds:D2}.{avgDuration.Milliseconds / 10:D2}";

            // 完成度格式化为百分比, 保留两位小数
            var formattedCompletion = $"{avgCompletion,6:0.00}%";

            // 格式化最短胜利用时
            var formattedShortestDuration = stats.shortestDuration == TimeSpan.MaxValue ? "无" : $"{(int)stats.shortestDuration.TotalMinutes:D2}:{stats.shortestDuration.Seconds:D2}.{stats.shortestDuration.Milliseconds / 10:D2}";

            // 添加到历史记录列表
            statisticsListBox.Items.Add($"难度: {formattedDifficulty}, 游戏次数: {stats.total}, 胜利次数: {stats.wins}, 胜利率: {winRate:P2}, 平均胜利用时: {formattedDuration}, 平均完成度: {formattedCompletion}, 最短胜利用时: {formattedShortestDuration}");
        }
    }

    /// <summary>
    /// 更新历史记录数据网格视图
    /// </summary>
    private void UpdateHistoryDataGridView()
    {
        historyDataGridView.RowCount = ResultManager.Results.Count;
        historyDataGridView.Invalidate();

        // 滚动到最顶部
        if (historyDataGridView.RowCount > 0)
        {
            historyDataGridView.FirstDisplayedScrollingRowIndex = 0;
        }
    }

    /// <summary>
    /// DataGridView 的单元格值需要时触发的事件
    /// </summary>
    private void HistoryDataGridView_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
    {
        // 检测行索引是否有效
        if (e.RowIndex < 0 || e.RowIndex >= ResultManager.Results.Count)
        {
            return;
        }

        // 检测列索引是否有效
        if (e.ColumnIndex < 0 || e.ColumnIndex >= historyDataGridView.Columns.Count)
        {
            return;
        }

        // 获取当前行的游戏结果
        var result = ResultManager.Results[e.RowIndex];

        // 根据列索引设置单元格值
        e.Value = historyDataGridView.Columns[e.ColumnIndex].Name switch
        {
            "序号" => e.RowIndex + 1,
            "开始时间" => result.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
            "难度" => Methods.GetDifficultyText(result.Difficulty),
            "结果" => result.IsWin ? "胜利" : "失败",
            "完成度" => $"{result.Completion ?? 100.0:0.##}%",
            "用时" => $"{(int)result.Duration.TotalMinutes:D2}:{result.Duration.Seconds:D2}.{result.Duration.Milliseconds / 10:D2}",
            "宽度" => result.BoardWidth ?? Constants.GetSettings(result.Difficulty).width,
            "高度" => result.BoardHeight ?? Constants.GetSettings(result.Difficulty).height,
            "地雷数" => result.MineCount ?? Constants.GetSettings(result.Difficulty).mineCount,
            _ => "",
        };
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

        // 清空历史记录数据
        Task.Run(Datas.ClearGameResultsAsync);

        // 重启历史记录面板
        RestartHistoryPanel();

        // 弹窗提示清除成功
        MessageBox.Show("历史记录已清除！", "清除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}