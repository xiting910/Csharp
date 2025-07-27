using System.Diagnostics;

namespace MineClearance;

/// <summary>
/// 底部状态栏
/// </summary>
public class BottomStatusBar : StatusStrip
{
    /// <summary>
    /// 左侧状态标签
    /// </summary>
    private readonly ToolStripStatusLabel _statusLabel;

    /// <summary>
    /// 右侧信息标签1
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel1;

    /// <summary>
    /// 右侧项目仓库链接标签
    /// </summary>
    private readonly ToolStripStatusLabel _repoLinkLabel;

    /// <summary>
    /// 右侧信息标签2
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel2;

    /// <summary>
    /// 构造函数，初始化状态栏
    /// </summary>
    /// <param name="author">作者信息</param>
    /// <param name="githubRepoUrl">GitHub 仓库链接</param>
    public BottomStatusBar(string author, string githubRepoUrl)
    {
        // 设置状态栏属性
        Size = new(Constants.MainFormWidth, Constants.BottomStatusBarHeight);
        Location = new(0, Constants.MainFormHeight - Constants.BottomStatusBarHeight);
        BackColor = Color.SkyBlue;
        ShowItemToolTips = true;

        // 左侧状态标签
        _statusLabel = new ToolStripStatusLabel
        {
            Text = "状态: 就绪",
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        // 右侧信息标签1
        _infoLabel1 = new ToolStripStatusLabel
        {
            Text = $"本项目由{author}一人开发。如您有建议或发现问题，请访问",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧GitHub仓库链接
        _repoLinkLabel = new ToolStripStatusLabel
        {
            Text = "项目github仓库",
            IsLink = true,
            ForeColor = Color.Blue,
            ToolTipText = githubRepoUrl,
            TextAlign = ContentAlignment.MiddleRight
        };
        _repoLinkLabel.Click += (s, e) =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = githubRepoUrl,
                UseShellExecute = true
            });
        };

        // 右侧信息标签2
        _infoLabel2 = new ToolStripStatusLabel
        {
            Text = "提交Issue或Pull Request",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 添加标签到状态栏
        Items.Add(_statusLabel);
        Items.Add(_infoLabel1);
        Items.Add(_repoLinkLabel);
        Items.Add(_infoLabel2);
    }

    /// <summary>
    /// 设置左侧状态文本
    /// </summary>
    /// <param name="status">状态</param>
    public void SetStatus(StatusBarState status)
    {
        _statusLabel.Text = status switch
        {
            StatusBarState.Ready => "状态: 就绪",
            StatusBarState.Ranking => "状态: 排行榜",
            StatusBarState.Preparing => "状态: 准备游戏",
            StatusBarState.InGame => "状态: 游戏进行中",
            StatusBarState.GameWon => "状态: 游戏胜利",
            StatusBarState.GameLost => "状态: 游戏失败",
            _ => "状态: 未知"
        };
    }
}