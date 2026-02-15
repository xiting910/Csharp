namespace WindowsUpdateManager;

/// <summary>
/// 菜单面板类
/// </summary>
internal sealed class MenuPanel : Panel
{
    /// <summary>
    /// 按钮宽度
    /// </summary>
    private static readonly int ButtonWidth = (int)(130 * Constants.DpiScale);

    /// <summary>
    /// 按钮高度
    /// </summary>
    private static readonly int ButtonHeight = (int)(40 * Constants.DpiScale);

    /// <summary>
    /// 标题标签
    /// </summary>
    private readonly Label _titleLabel;

    /// <summary>
    /// 打开更新页面按钮
    /// </summary>
    private readonly Button _btnOpenUpdate;

    /// <summary>
    /// 暂停更新按钮
    /// </summary>
    private readonly Button _btnPauseUpdate;

    /// <summary>
    /// 恢复更新按钮
    /// </summary>
    private readonly Button _btnResumeUpdate;

    /// <summary>
    /// 退出按钮
    /// </summary>
    private readonly Button _btnExit;

    /// <summary>
    /// 初始化菜单面板
    /// </summary>
    public MenuPanel()
    {
        // 设置菜单面板属性
        Name = "MenuPanel";
        Dock = DockStyle.Fill;
        BackColor = Color.LightBlue;

        // 标题字体样式
        var titleFontSize = Constants.DpiScale * 12;
        var titleFont = new Font("Microsoft YaHei", titleFontSize, FontStyle.Bold);

        // 初始化标题标签
        _titleLabel = new()
        {
            AutoSize = true,
            Font = titleFont,
            ForeColor = Color.DarkRed,
            BackColor = Color.Transparent,
            Text = Constants.ProgramName,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 初始化打开更新页面按钮
        _btnOpenUpdate = new()
        {
            Text = "打开更新页面",
            Size = new(ButtonWidth, ButtonHeight),
            ForeColor = Color.DarkBlue,
            BackColor = Color.LightGray,
            FlatStyle = FlatStyle.Flat
        };
        _btnOpenUpdate.Click += (sender, e) => UpdateManager.OpenWindowsUpdatePage();

        // 初始化暂停更新按钮
        _btnPauseUpdate = new()
        {
            Text = "暂停更新到指定日期",
            Size = new(ButtonWidth, ButtonHeight),
            ForeColor = Color.DarkBlue,
            BackColor = Color.LightYellow,
            FlatStyle = FlatStyle.Flat
        };
        _btnPauseUpdate.Click += OnPauseUpdateButtonClick;

        // 初始化恢复更新按钮
        _btnResumeUpdate = new()
        {
            Text = "恢复更新",
            Size = new(ButtonWidth, ButtonHeight),
            ForeColor = Color.DarkBlue,
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        _btnResumeUpdate.Click += OnResumeUpdateButtonClick;

        // 初始化退出按钮
        _btnExit = new()
        {
            Text = "退出",
            Size = new(ButtonWidth, ButtonHeight),
            ForeColor = Color.White,
            BackColor = Color.LightCoral,
            FlatStyle = FlatStyle.Flat
        };
        _btnExit.Click += (sender, e) => Application.Exit();

        // 添加控件到菜单面板
        Controls.Add(_titleLabel);
        Controls.Add(_btnOpenUpdate);
        Controls.Add(_btnPauseUpdate);
        Controls.Add(_btnResumeUpdate);
        Controls.Add(_btnExit);
    }

    /// <summary>
    /// 暂停更新按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnPauseUpdateButtonClick(object? sender, EventArgs e)
    {
        // 创建日期选择对话框
        using var dateTimeDialog = new DatePickerDialog();

        // 显示对话框并检查结果
        if (dateTimeDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                // 尝试暂停更新
                UpdateManager.PauseUpdateUntil(dateTimeDialog.SelectedDate);

                // 询问用户是否需要禁用 Windows 更新计划和服务以彻底暂停更新
                var result = MessageBox.Show("更新已暂停至 " + dateTimeDialog.SelectedDate.ToShortDateString() + "\n是否禁用 Windows 更新计划和服务以彻底暂停更新？\n注意: 彻底禁用虽然不会损坏系统, 但是会导致微软应用商店无法打开等问题\n您随时可以通过恢复更新按钮重新启用这些计划和服务", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                // 如果用户选择是, 则禁用更新计划和服务
                if (result == DialogResult.Yes)
                {
                    UpdateManager.DisableUpdateServices();
                    UpdateManager.DisableUpdateTasks();
                    _ = MessageBox.Show("Windows 更新计划和服务已被禁用", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (InvalidOperationException ex)
            {
                _ = MessageBox.Show(ex.Message + "\n" + ex.InnerException?.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 恢复更新按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnResumeUpdateButtonClick(object? sender, EventArgs e)
    {
        try
        {
            // 尝试恢复更新
            UpdateManager.ResumeUpdate();
            var result = MessageBox.Show("更新已恢复, 需要重启才能生效, 是否立即重启?", "需要重启", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // 如果用户选择是, 则重启计算机
            if (result == DialogResult.Yes)
            {
                UpdateManager.RestartComputer();
            }
        }
        catch (InvalidOperationException ex)
        {
            _ = MessageBox.Show(ex.Message + "\n" + ex.InnerException?.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 重写 OnResize 方法, 用于调整控件位置
    /// </summary>
    /// <param name="e">事件参数</param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // 标签底部与按钮顶部的间距
        var titleBottomSpacing = (int)(40 * Constants.DpiScale);

        // 按钮Y轴间距
        var buttonSpacing = (int)(20 * Constants.DpiScale);

        // 按钮总高度(包括间距)
        var buttonTotalHeight = ButtonHeight * 4 + buttonSpacing * 3;

        // 标签与按钮所占据的总高度(包括间距)
        var totalHeight = _titleLabel.Height + titleBottomSpacing + buttonTotalHeight;

        // 居中标题标签
        _titleLabel.Left = (Width - _titleLabel.Width) / 2;
        _titleLabel.Top = (Height - totalHeight) / 3;

        // 按钮当前X、Y位置
        var buttonX = (Width - ButtonWidth) / 2;
        var buttonY = _titleLabel.Bottom + titleBottomSpacing;

        // 更新打开更新页面按钮位置
        _btnOpenUpdate.Left = buttonX;
        _btnOpenUpdate.Top = buttonY;
        buttonY += ButtonHeight + buttonSpacing;

        // 更新暂停更新按钮位置
        _btnPauseUpdate.Left = buttonX;
        _btnPauseUpdate.Top = buttonY;
        buttonY += ButtonHeight + buttonSpacing;

        // 更新恢复更新按钮位置
        _btnResumeUpdate.Left = buttonX;
        _btnResumeUpdate.Top = buttonY;
        buttonY += ButtonHeight + buttonSpacing;

        // 更新退出按钮位置
        _btnExit.Left = buttonX;
        _btnExit.Top = buttonY;
    }
}
