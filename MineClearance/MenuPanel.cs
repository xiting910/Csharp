using AutoUpdaterDotNET;

namespace MineClearance;

/// <summary>
/// 菜单面板类
/// </summary>
public partial class MenuPanel : Panel
{
    /// <summary>
    /// 初始化菜单面板
    /// </summary>
    /// <param name="mainForm">主窗体</param>
    public MenuPanel(MainForm mainForm)
    {
        // 设置菜单面板属性
        Name = "MenuPanel";
        Location = new(0, 0);
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight - Constants.BottomStatusBarHeight);
        BackColor = Color.LightBlue;

        // 标题标签宽度和高度
        var titleLabelWidth = (int)(150 * Constants.DpiScale);
        var titleLabelHeight = (int)(50 * Constants.DpiScale);

        // 标题标签左侧位置和顶部位置
        var titleLabelLeft = (Constants.MainFormWidth - titleLabelWidth) / 2;
        var titleLabelTop = (int)(25 * Constants.DpiScale);

        // 添加标题标签
        Label titleLabel = new()
        {
            Text = "扫雷游戏",
            Font = new("Microsoft YaHei", 24, FontStyle.Bold),
            ForeColor = Color.DarkRed,
            BackColor = Color.Transparent,
            Size = new(titleLabelWidth, titleLabelHeight),
            Location = new(titleLabelLeft, titleLabelTop),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 按钮宽度和高度
        var buttonWidth = (int)(125 * Constants.DpiScale);
        var buttonHeight = (int)(40 * Constants.DpiScale);

        // 按钮间距
        var buttonMargin = (int)(15 * Constants.DpiScale);

        // 按钮左侧位置和顶部位置
        var buttonLeft = (Constants.MainFormWidth - buttonWidth) / 2;
        var buttonTop = titleLabelTop + titleLabelHeight + buttonMargin;

        // 添加新游戏按钮
        Button btnNewGame = new()
        {
            Text = "新游戏",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightGreen,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnNewGame.Click += (sender, e) => mainForm.ShowPanel(PanelType.GamePrepare);
        buttonTop += buttonHeight + buttonMargin;

        // 添加显示排行榜按钮
        Button btnShowRanking = new()
        {
            Text = "游戏历史记录",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightYellow,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnShowRanking.Click += (sender, e) => mainForm.ShowPanel(PanelType.Ranking);
        buttonTop += buttonHeight + buttonMargin;

        // 添加检查更新按钮
        Button btnCheckUpdate = new()
        {
            Text = "检查更新",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.DarkCyan,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnCheckUpdate.Click += BtnCheckUpdate_Click;
        buttonTop += buttonHeight + buttonMargin;

        // 添加退出按钮
        Button btnExit = new()
        {
            Text = "退出",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightCoral,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnExit.Click += (sender, e) => mainForm.Close();

        // 添加控件到菜单面板
        Controls.Add(titleLabel);
        Controls.Add(btnNewGame);
        Controls.Add(btnShowRanking);
        Controls.Add(btnCheckUpdate);
        Controls.Add(btnExit);

        // 将菜单面板添加到窗体
        mainForm.Controls.Add(this);
    }

    /// <summary>
    /// 检查更新按钮点击事件处理
    /// </summary>
    private void BtnCheckUpdate_Click(object? sender, EventArgs e)
    {
        if (Methods.IsHandlingUpdateEvent)
        {
            MessageBox.Show("当前已经有更新事件正在处理, 请稍后再试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Methods.IsHandlingUpdateEvent = true;
        AutoUpdater.Start(Constants.AutoUpdateUrl);
    }
}