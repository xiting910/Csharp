using MineClearance.Models;

namespace MineClearance.UI;

/// <summary>
/// 准备游戏面板类
/// </summary>
internal sealed class GamePreparePanel : Panel
{
    /// <summary>
    /// 标题标签
    /// </summary>
    private readonly Label _titleLabel;

    /// <summary>
    /// 简单按钮
    /// </summary>
    private readonly Button _btnEasy;

    /// <summary>
    /// 普通按钮
    /// </summary>
    private readonly Button _btnMedium;

    /// <summary>
    /// 困难按钮
    /// </summary>
    private readonly Button _btnHard;

    /// <summary>
    /// 地狱按钮
    /// </summary>
    private readonly Button _btnHell;

    /// <summary>
    /// 自定义按钮
    /// </summary>
    private readonly Button _btnCustom;

    /// <summary>
    /// 返回按钮
    /// </summary>
    private readonly Button _btnBack;

    /// <summary>
    /// 初始化准备游戏面板
    /// </summary>
    public GamePreparePanel()
    {
        // 设置准备游戏面板属性
        Name = "GamePreparationPanel";
        Location = new(0, 0);
        Size = new(UIConstants.MainFormWidth, UIConstants.MainFormHeight - UIConstants.BottomStatusBarHeight);
        BackColor = Color.LightYellow;

        // 标题标签宽度和高度
        var titleLabelWidth = (int)(150 * UIConstants.DpiScale);
        var titleLabelHeight = (int)(50 * UIConstants.DpiScale);

        // 标题标签左侧位置和顶部位置
        var titleLabelLeft = (UIConstants.MainFormWidth - titleLabelWidth) / 2;
        var titleLabelTop = (int)(25 * UIConstants.DpiScale);

        // 添加标题标签
        _titleLabel = new()
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

        // 添加简单按钮
        _btnEasy = new()
        {
            Text = "简单",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightGreen,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        _btnEasy.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Easy));
        buttonTop += buttonHeight + buttonMargin;

        // 添加普通按钮
        _btnMedium = new()
        {
            Text = "普通",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.Yellow,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        _btnMedium.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Medium));
        buttonTop += buttonHeight + buttonMargin;

        // 添加困难按钮
        _btnHard = new()
        {
            Text = "困难",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.Red,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        _btnHard.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Hard));
        buttonTop += buttonHeight + buttonMargin;

        // 添加地狱按钮
        _btnHell = new()
        {
            Text = "地狱",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.DarkRed,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        _btnHell.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Hell));
        buttonTop += buttonHeight + buttonMargin;

        // 添加自定义按钮
        _btnCustom = new()
        {
            Text = "自定义",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightBlue,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        _btnCustom.Click += OnCustomButtonClick;
        buttonTop += buttonHeight + buttonMargin;

        // 添加返回菜单按钮
        _btnBack = new()
        {
            Text = "返回",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightCoral,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _btnBack.Click += (sender, e) => MainForm.ShowPanel(PanelType.Menu);

        // 添加控件到游戏准备面板
        Controls.Add(_titleLabel);
        Controls.Add(_btnEasy);
        Controls.Add(_btnMedium);
        Controls.Add(_btnHard);
        Controls.Add(_btnHell);
        Controls.Add(_btnCustom);
        Controls.Add(_btnBack);
    }

    /// <summary>
    /// 自定义按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private void OnCustomButtonClick(object? sender, EventArgs e)
    {
        // 开始自定义难度游戏的逻辑(弹出对话框获取自定义参数)
        using var dialog = new CustomDifficultyDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var (width, height, mineCount) = dialog.CustomDifficulty;
            try
            {
                GamePanel.StartNewGame(new(width, height, mineCount));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _ = MessageBox.Show($"参数错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException ex)
            {
                _ = MessageBox.Show($"参数错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}