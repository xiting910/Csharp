using MineClearance.Models;

namespace MineClearance.UI;

/// <summary>
/// 准备游戏面板类
/// </summary>
public partial class GamePreparePanel : Panel
{
    /// <summary>
    /// 初始化准备游戏面板
    /// </summary>
    public GamePreparePanel()
    {
        // 设置准备游戏面板属性
        Name = "GamePreparationPanel";
        Location = new(0, 0);
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight - Constants.BottomStatusBarHeight);
        BackColor = Color.LightYellow;

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

        // 添加简单按钮
        Button btnEasy = new()
        {
            Text = "简单",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightGreen,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnEasy.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Easy));
        buttonTop += buttonHeight + buttonMargin;

        // 添加普通按钮
        Button btnMedium = new()
        {
            Text = "普通",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.Yellow,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnMedium.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Medium));
        buttonTop += buttonHeight + buttonMargin;

        // 添加困难按钮
        Button btnHard = new()
        {
            Text = "困难",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.Red,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnHard.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Hard));
        buttonTop += buttonHeight + buttonMargin;

        // 添加地狱按钮
        Button btnHell = new()
        {
            Text = "地狱",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.DarkRed,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnHell.Click += (sender, e) => GamePanel.StartNewGame(new(DifficultyLevel.Hell));
        buttonTop += buttonHeight + buttonMargin;

        // 添加自定义按钮
        Button btnCustom = new()
        {
            Text = "自定义",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightBlue,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnCustom.Click += (sender, e) =>
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
                    MessageBox.Show($"参数错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"参数错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        };
        buttonTop += buttonHeight + buttonMargin;

        // 添加返回菜单按钮
        Button btnBackMenu = new()
        {
            Text = "返回",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightCoral,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnBackMenu.Click += (sender, e) => MainForm.ShowPanel(PanelType.Menu);

        // 添加控件到游戏准备面板
        Controls.Add(titleLabel);
        Controls.Add(btnEasy);
        Controls.Add(btnMedium);
        Controls.Add(btnHard);
        Controls.Add(btnHell);
        Controls.Add(btnCustom);
        Controls.Add(btnBackMenu);
    }
}