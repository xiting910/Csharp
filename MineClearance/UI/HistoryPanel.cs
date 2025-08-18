using System.Globalization;
using MineClearance.Models;
using MineClearance.Utilities;

namespace MineClearance.UI;

/// <summary>
/// 历史记录面板类
/// </summary>
internal sealed class HistoryPanel : Panel
{
    /// <summary>
    /// 历史记录顶部面板高度
    /// </summary>
    private static readonly int historyTopPanelHeight = (int)(43 * UIConstants.DpiScale);

    /// <summary>
    /// 数据网格视图的宽度
    /// </summary>
    private static readonly int dataGridViewWidth = UIConstants.MainFormWidth - (int)(12 * UIConstants.DpiScale);

    /// <summary>
    /// 数据网格视图的高度
    /// </summary>
    private static readonly int dataGridViewHeight = UIConstants.MainFormHeight - historyTopPanelHeight - UIConstants.BottomStatusBarHeight;

    /// <summary>
    /// 历史记录顶部面板
    /// </summary>
    private readonly Panel historyTopPanel;

    /// <summary>
    /// 提示信息标签
    /// </summary>
    private readonly Label tipsLabel;

    /// <summary>
    /// 统计信息列表框
    /// </summary>
    private readonly DoubleBufferedDataGridView statisticsDataGridView;

    /// <summary>
    /// 历史记录列表框
    /// </summary>
    private readonly DoubleBufferedDataGridView historyDataGridView;

    /// <summary>
    /// 历史记录列表框行的右键菜单
    /// </summary>
    private readonly ContextMenuStrip historyContextMenu;

    /// <summary>
    /// 记录当前选中的行索引
    /// </summary>
    private int selectedRowIndex = -1;

    /// <summary>
    /// 初始化历史记录面板
    /// </summary>
    public HistoryPanel()
    {
        // 设置历史记录面板属性
        Name = "HistoryPanel";
        Location = new(0, 0);
        Size = new(UIConstants.MainFormWidth, UIConstants.MainFormHeight - UIConstants.BottomStatusBarHeight);

        // 创建历史记录顶部信息面板
        historyTopPanel = CreateHistoryTopInfoPanel();

        // 添加提示标签
        tipsLabel = new()
        {
            Text = "点击列头可进行排序和筛选, 点击显示详细历史记录按钮会清除所有筛选和排序条件",
            Font = new("Arial", 6 * UIConstants.DpiScale, FontStyle.Regular),
            Location = new((int)(450 * UIConstants.DpiScale), (int)(12 * UIConstants.DpiScale)),
            ForeColor = Color.DarkBlue,
            AutoSize = true
        };
        historyTopPanel.Controls.Add(tipsLabel);

        // 创建统计信息数据网格视图
        statisticsDataGridView = CreateStatisticsDataGridView();

        // 创建历史记录数据网格视图
        historyDataGridView = CreateHistoryDataGridView();

        // 创建历史记录右键菜单
        historyContextMenu = CreateHistoryContextMenu();

        // 订阅列头点击事件
        historyDataGridView.ColumnHeaderMouseClick += (sender, e) =>
        {
            // 获取当前列的名字
            var columnName = historyDataGridView.Columns[e.ColumnIndex].Name;

            // 获取右键菜单实例
            var contextMenu = HistoryContextMenu.GetInstance(columnName);

            // 显示右键菜单
            contextMenu.Show(Cursor.Position);
        };

        // 订阅 ResultManager 的条件变化事件
        ResultManager.ConditionsChanged += UpdateHistoryDataGridView;

        // 订阅 CellValueNeeded 事件
        historyDataGridView.CellValueNeeded += HistoryDataGridView_CellValueNeeded;

        // 订阅单元格鼠标点击事件
        historyDataGridView.CellMouseClick += (sender, e) =>
        {
            // 检测行索引是否有效
            if (e.RowIndex < 0 || e.RowIndex >= ResultManager.Results.Count)
            {
                return;
            }

            // 只有点击序号列时才显示菜单
            if (historyDataGridView.Columns[e.ColumnIndex].Name == "序号")
            {
                selectedRowIndex = e.RowIndex;
                historyContextMenu.Items[0].Text = $"删除序号 {e.RowIndex + 1} 的游戏记录";
                historyContextMenu.Show(Cursor.Position);
            }
        };

        // 订阅选中变化事件
        historyDataGridView.SelectionChanged += (sender, e) => historyDataGridView.ClearSelection();

        // 添加历史记录顶部面板和数据网格视图到排行榜面板
        Controls.Add(historyTopPanel);
        Controls.Add(statisticsDataGridView);
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
            // 提示信息标签可见
            tipsLabel.Visible = true;

            // 切换到历史记录数据网格视图
            statisticsDataGridView.Visible = false;
            historyDataGridView.Visible = true;

            // 重置右键菜单实例(会自动更新历史记录数据网格视图)
            HistoryContextMenu.ResetInstances();
        }
        else
        {
            // 提示信息标签不可见
            tipsLabel.Visible = false;

            // 切换到统计信息数据网格视图
            historyDataGridView.Visible = false;
            statisticsDataGridView.Visible = true;

            // 更新统计信息数据网格视图
            UpdateStatisticsDataGridView();
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
            Size = new(UIConstants.MainFormWidth, historyTopPanelHeight),
            BackColor = Color.LightSalmon,
            Location = new(0, 0)
        };

        // 添加标题标签
        Label titleLabel = new()
        {
            Text = "历史记录",
            Font = new("Arial", 12 * UIConstants.DpiScale, FontStyle.Bold),
            Location = new((int)(5 * UIConstants.DpiScale), (int)(2 * UIConstants.DpiScale)),
            AutoSize = true
        };
        panel.Controls.Add(titleLabel);

        // 按钮X位置和Y位置
        var buttonXPosition = (int)(170 * UIConstants.DpiScale);
        var buttonYPosition = (int)(9 * UIConstants.DpiScale);

        // 按钮宽度和高度
        var buttonWidth = (int)(120 * UIConstants.DpiScale);
        var buttonHeight = (int)(25 * UIConstants.DpiScale);

        // 按钮水平间距
        var buttonSpacing = (int)(10 * UIConstants.DpiScale);

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
        buttonXPosition += buttonWidth + (2 * buttonSpacing);
        panel.Controls.Add(btnShowHistory);

        // 更新按钮宽度
        buttonWidth = (int)(70 * UIConstants.DpiScale);

        // 更新按钮X位置
        buttonXPosition = UIConstants.MainFormWidth - buttonWidth - (4 * buttonSpacing);

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
    /// 创建统计信息数据网格视图
    /// </summary>
    /// <returns>返回创建的数据网格视图</returns>
    private static DoubleBufferedDataGridView CreateStatisticsDataGridView()
    {
        // 设置数据网格视图的列头高度
        var columnHeaderHeight = (int)(94 * UIConstants.DpiScale);

        // 设置数据网格视图的行高
        var rowHeight = (int)(110 * UIConstants.DpiScale);

        // 创建统计信息数据网格视图
        var dataGridView = new DoubleBufferedDataGridView
        {
            Name = "StatisticsListView",
            BackColor = Color.White,
            ForeColor = Color.Black,
            RowHeadersVisible = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            AllowUserToResizeColumns = false,
            Location = new(0, historyTopPanelHeight),
            Size = new(UIConstants.MainFormWidth, dataGridViewHeight),
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = columnHeaderHeight,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = rowHeight }
        };

        // 保存每一列的名字和最小宽度的字典
        var columnDefinitions = new Dictionary<string, int>
        {
            { "难度", 0 },
            { "游戏次数", (int)(150 * UIConstants.DpiScale) },
            { "胜利次数", (int)(150 * UIConstants.DpiScale) },
            { "胜率", (int)(150 * UIConstants.DpiScale) },
            { "平均胜利用时", (int)(200 * UIConstants.DpiScale) },
            { "最短胜利用时", (int)(200 * UIConstants.DpiScale) },
            { "平均完成度", (int)(200 * UIConstants.DpiScale) }
        };

        // 剩余宽度
        var remainWidth = UIConstants.MainFormWidth - columnDefinitions.Values.Sum();
        remainWidth -= (int)(10 * UIConstants.DpiScale);

        // 难度列增加剩余宽度
        columnDefinitions["难度"] += remainWidth;

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
                HeaderText = name,
                DefaultCellStyle = { Alignment = alignment },
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            // 设置列头对齐方式
            column.HeaderCell.Style.Alignment = alignment;
            column.HeaderCell.Style.Font = new("微软雅黑", 11F, FontStyle.Bold);
            column.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            column.HeaderCell.Style.ForeColor = Color.DarkBlue;

            // 添加列到数据网格视图
            _ = dataGridView.Columns.Add(column);
        }

        // 选择时清除选择
        dataGridView.SelectionChanged += (s, e) => dataGridView.ClearSelection();
        return dataGridView;
    }

    /// <summary>
    /// 创建历史记录数据网格视图
    /// </summary>
    /// <returns>返回创建的历史记录数据网格视图</returns>
    private static DoubleBufferedDataGridView CreateHistoryDataGridView()
    {
        // 设置数据网格视图的列头高度
        var columnHeaderHeight = (int)(29 * UIConstants.DpiScale);

        // 设置数据网格视图的行高
        var rowHeight = (int)(25 * UIConstants.DpiScale);

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
            { "序号", (int)(80 * UIConstants.DpiScale) },
            { "开始时间", 0 },
            { "难度", (int)(160 * UIConstants.DpiScale) },
            { "结果", (int)(160 * UIConstants.DpiScale) },
            { "完成度", (int)(160 * UIConstants.DpiScale) },
            { "用时", (int)(160 * UIConstants.DpiScale) },
            { "宽度", (int)(100 * UIConstants.DpiScale) },
            { "高度", (int)(100 * UIConstants.DpiScale) },
            { "地雷数", (int)(100 * UIConstants.DpiScale) }
        };

        // 剩余宽度
        var remainWidth = dataGridViewWidth - columnDefinitions.Values.Sum();
        remainWidth -= (int)(19 * UIConstants.DpiScale);

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
                HeaderText = name,
                DefaultCellStyle = { Alignment = alignment },
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            // 设置列头对齐方式
            column.HeaderCell.Style.Alignment = alignment;
            column.HeaderCell.Style.Font = new("微软雅黑", 11F, FontStyle.Bold);
            column.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            column.HeaderCell.Style.ForeColor = Color.DarkBlue;

            // 添加列到数据网格视图
            _ = dataGridView.Columns.Add(column);
        }
        return dataGridView;
    }

    /// <summary>
    /// 创建历史记录右键菜单
    /// </summary>
    /// <returns>历史记录右键菜单</returns>
    private ContextMenuStrip CreateHistoryContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        // 添加菜单项
        _ = contextMenu.Items.Add("删除记录", null, (s, e) => DeleteHistoryRecord());
        return contextMenu;
    }

    /// <summary>
    /// 更新统计信息数据网格视图
    /// </summary>
    private void UpdateStatisticsDataGridView()
    {
        // 获取所有游戏结果
        var gameResults = ResultManager.OriginalResults;

        // 清空统计信息数据网格视图
        statisticsDataGridView.Rows.Clear();

        // 全部难度的游戏结果统计
        var allDifficultyStats = new Stats() { ShortestDuration = TimeSpan.MaxValue };

        // 使用字典来统计每个难度的游戏结果
        var difficultyStats = new Dictionary<DifficultyLevel, Stats>
        {
            { DifficultyLevel.Easy, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Medium, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Hard, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Hell, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Custom, new Stats() { ShortestDuration = TimeSpan.MaxValue } }
        };

        // 统计游戏结果
        foreach (var result in gameResults)
        {
            // 更新全部难度的游戏结果
            allDifficultyStats = AddResultToStats(result, allDifficultyStats);

            // 更新对应难度的游戏结果
            difficultyStats[result.Difficulty] = AddResultToStats(result, difficultyStats[result.Difficulty]);
        }

        // 添加全部难度的统计信息
        AddStatsToStatisticsDataGridView("全部", allDifficultyStats);

        // 添加每个难度的统计信息
        foreach (var difficulty in difficultyStats.Keys)
        {
            var difficultyText = Methods.GetDifficultyText(difficulty);
            AddStatsToStatisticsDataGridView(difficultyText, difficultyStats[difficulty]);
        }

        // 重绘统计信息数据网格视图
        statisticsDataGridView.Invalidate();
    }

    /// <summary>
    /// 将游戏结果添加到stats结构体
    /// </summary>
    /// <param name="result">要添加的游戏结果</param>
    /// <param name="stats">要更新的统计信息</param>
    /// <returns>返回更新后的统计信息</returns>
    private static Stats AddResultToStats(GameResult result, Stats stats)
    {
        // 更新统计信息
        return new()
        {
            Total = stats.Total + 1,
            Wins = stats.Wins + (result.IsWin ? 1 : 0),
            TotalDuration = stats.TotalDuration + (result.IsWin ? result.Duration : TimeSpan.Zero),
            TotalCompletion = stats.TotalCompletion + (result.Completion ?? 100.0),
            ShortestDuration = (result.IsWin && result.Duration < stats.ShortestDuration) ? result.Duration : stats.ShortestDuration
        };
    }

    /// <summary>
    /// 将stats结构体添加到统计信息数据网格视图
    /// </summary>
    /// <param name="difficultyText">要添加的难度文本</param>
    /// <param name="stats">要添加的统计信息</param>
    private void AddStatsToStatisticsDataGridView(string difficultyText, Stats stats)
    {
        // 计算胜率、平均胜利用时和平均完成度
        var winRate = stats.Total > 0 ? (double)stats.Wins / stats.Total * 100 : 0;
        var avgDuration = stats.Wins > 0 ? TimeSpan.FromMilliseconds(stats.TotalDuration.TotalMilliseconds / stats.Wins) : TimeSpan.Zero;
        var avgCompletion = stats.Total > 0 ? stats.TotalCompletion / stats.Total : 0;

        // 格式化用时为 xx:xx.xx 格式
        var formattedDuration = $"{(int)avgDuration.TotalMinutes:D2}:{avgDuration.Seconds:D2}.{avgDuration.Milliseconds / 10:D2}";

        // 格式化最短胜利用时为 xx:xx.xx 格式
        var formattedShortestDuration = stats.ShortestDuration == TimeSpan.MaxValue ? "无" : $"{(int)stats.ShortestDuration.TotalMinutes:D2}:{stats.ShortestDuration.Seconds:D2}.{stats.ShortestDuration.Milliseconds / 10:D2}";

        // 添加当前难度的统计信息到数据网格视图
        _ = statisticsDataGridView.Rows.Add(
            difficultyText,
            stats.Total,
            stats.Wins,
            $"{winRate:0.##}%",
            formattedDuration,
            formattedShortestDuration,
            $"{avgCompletion:0.##}%"
        );
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
            "开始时间" => result.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
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
    /// 删除选中的历史记录
    /// </summary>
    private async void DeleteHistoryRecord()
    {
        if (selectedRowIndex >= 0 && selectedRowIndex < ResultManager.Results.Count)
        {
            var confirmResult = CustomMessageBox.Show("确定要删除选中的历史记录吗？\n注意: 一旦删除将无法找回！！！", "删除历史记录", "删除指定历史记录");
            if (confirmResult == DialogResult.Yes)
            {
                await ResultManager.RemoveResultAt(selectedRowIndex);
                selectedRowIndex = -1;
            }
        }
    }

    /// <summary>
    /// 清除历史记录按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private async void BtnClearHistory_Click(object? sender, EventArgs e)
    {
        // 添加确认对话框
        var confirmResult = MessageBox.Show("确定要清除所有历史记录吗？\n注意: 一旦清除将无法找回！！！", "清除历史记录", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

        // 用户选择取消，直接返回
        if (confirmResult != DialogResult.Yes)
        {
            return;
        }

        // 清空历史记录数据
        await ResultManager.ClearAllResultsAsync();

        // 弹窗提示清除成功
        _ = MessageBox.Show("历史记录已清除！", "清除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 统计信息结构体
    /// </summary>
    private readonly struct Stats
    {
        /// <summary>
        /// 总游戏次数
        /// </summary>
        public int Total { get; init; }

        /// <summary>
        /// 胜利次数
        /// </summary>
        public int Wins { get; init; }

        /// <summary>
        /// 总胜利用时
        /// </summary>
        public TimeSpan TotalDuration { get; init; }

        /// <summary>
        /// 最短胜利用时
        /// </summary>
        public TimeSpan ShortestDuration { get; init; }

        /// <summary>
        /// 总完成度
        /// </summary>
        public double TotalCompletion { get; init; }
    }
}