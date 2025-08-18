using AutoUpdaterDotNET;
using MineClearance.Models;
using MineClearance.Utilities;

namespace MineClearance.UI;

/// <summary>
/// 菜单面板类
/// </summary>
internal sealed class MenuPanel : Panel
{
    /// <summary>
    /// 初始化菜单面板
    /// </summary>
    public MenuPanel()
    {
        // 设置菜单面板属性
        Name = "MenuPanel";
        Location = new(0, 0);
        Size = new(UIConstants.MainFormWidth, UIConstants.MainFormHeight - UIConstants.BottomStatusBarHeight);
        BackColor = Color.LightBlue;

        // 标题标签宽度和高度
        var titleLabelWidth = (int)(150 * UIConstants.DpiScale);
        var titleLabelHeight = (int)(50 * UIConstants.DpiScale);

        // 标题标签左侧位置和顶部位置
        var titleLabelLeft = (UIConstants.MainFormWidth - titleLabelWidth) / 2;
        var titleLabelTop = (int)(25 * UIConstants.DpiScale);

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
        var buttonWidth = (int)(125 * UIConstants.DpiScale);
        var buttonHeight = (int)(40 * UIConstants.DpiScale);

        // 按钮间距
        var buttonMargin = (int)(15 * UIConstants.DpiScale);

        // 按钮左侧位置和顶部位置
        var buttonLeft = (UIConstants.MainFormWidth - buttonWidth) / 2;
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
        btnNewGame.Click += BtnNewGame_Click;
        buttonTop += buttonHeight + buttonMargin;

        // 添加显示历史记录按钮
        Button btnShowHistory = new()
        {
            Text = "游戏历史记录",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightYellow,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnShowHistory.Click += BtnShowHistory_Click;
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

        // 创建设置按钮
        Button btnSettings = new()
        {
            Text = "设置",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightGray,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnSettings.Click += BtnSettings_Click;
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
        btnExit.Click += (sender, e) => Application.Exit();

        // 添加控件到菜单面板
        Controls.Add(titleLabel);
        Controls.Add(btnNewGame);
        Controls.Add(btnShowHistory);
        Controls.Add(btnCheckUpdate);
        Controls.Add(btnSettings);
        Controls.Add(btnExit);
    }

    /// <summary>
    /// 新游戏按钮点击事件处理
    /// </summary>
    private void BtnNewGame_Click(object? sender, EventArgs e)
    {
        // 如果需要强制更新, 则提示用户
        if (Methods.IsForceUpdate)
        {
            _ = MessageBox.Show("当前需要强制更新, 请先更新应用程序后再开始新游戏。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 显示游戏准备面板
        MainForm.ShowPanel(PanelType.GamePrepare);
    }

    /// <summary>
    /// 游戏历史记录按钮点击事件处理
    /// </summary>
    private void BtnShowHistory_Click(object? sender, EventArgs e)
    {
        // 如果需要强制更新, 则提示用户
        if (Methods.IsForceUpdate)
        {
            _ = MessageBox.Show("当前需要强制更新, 请先更新应用程序后再查看游戏历史记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 显示历史记录面板
        MainForm.ShowPanel(PanelType.History);
    }

    /// <summary>
    /// 检查更新按钮点击事件处理
    /// </summary>
    private void BtnCheckUpdate_Click(object? sender, EventArgs e)
    {
        if (Methods.IsHandlingUpdateEvent)
        {
            _ = MessageBox.Show("当前已经有更新事件正在处理, 请稍后再试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Methods.IsHandlingUpdateEvent = true;
        AutoUpdater.Start(UIConstants.AutoUpdateUrl);
    }

    /// <summary>
    /// 设置按钮点击事件处理
    /// </summary>
    private void BtnSettings_Click(object? sender, EventArgs e)
    {
        // 如果需要强制更新, 则提示用户
        if (Methods.IsForceUpdate)
        {
            _ = MessageBox.Show("当前需要强制更新, 请先更新应用程序后再打开设置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 显示设置窗口
        SettingForm.ShowForm();
    }
}