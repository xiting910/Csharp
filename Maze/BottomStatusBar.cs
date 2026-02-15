using System.Diagnostics;

namespace Maze;

/// <summary>
/// 底部状态栏
/// </summary>
internal sealed class BottomStatusBar : StatusStrip
{
    /// <summary>
    /// 底部状态栏单例
    /// </summary>
    public static BottomStatusBar Instance { get; } = new();

    /// <summary>
    /// 作者名字
    /// </summary>
    private const string AuthorName = "xiting910";

    /// <summary>
    /// 作者主页链接
    /// </summary>
    private const string AuthorHomepageUrl = "https://github.com/xiting910";

    /// <summary>
    /// GitHub 仓库链接
    /// </summary>
    private const string GitHubRepoUrl = "https://github.com/xiting910/Csharp/tree/main/Maze";

    /// <summary>
    /// 左侧状态标签
    /// </summary>
    private readonly ToolStripStatusLabel _statusLabel;

    /// <summary>
    /// 右侧信息标签1
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel1;

    /// <summary>
    /// 右侧作者主页链接标签
    /// </summary>
    private readonly ToolStripStatusLabel _authorLinkLabel;

    /// <summary>
    /// 右侧信息标签2
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel2;

    /// <summary>
    /// 右侧项目仓库链接标签
    /// </summary>
    private readonly ToolStripStatusLabel _repoLinkLabel;

    /// <summary>
    /// 右侧信息标签3
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel3;

    /// <summary>
    /// 私有构造函数, 初始化状态栏
    /// </summary>
    private BottomStatusBar()
    {
        // 设置状态栏属性
        Dock = DockStyle.Bottom;
        ShowItemToolTips = true;
        SizingGrip = false;

        // 左侧状态标签
        _statusLabel = new()
        {
            Text = "状态: 就绪",
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        // 右侧信息标签1
        _infoLabel1 = new()
        {
            Text = $"本项目由",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧作者主页链接
        _authorLinkLabel = new()
        {
            Text = AuthorName,
            IsLink = true,
            ForeColor = Color.Blue,
            ToolTipText = AuthorHomepageUrl,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧作者主页链接点击事件处理
        _authorLinkLabel.Click += (s, e) =>
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo
                {
                    FileName = AuthorHomepageUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        // 右侧信息标签2
        _infoLabel2 = new()
        {
            Text = "一人开发。如您有建议或发现问题，请访问",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧 GitHub 仓库链接
        _repoLinkLabel = new()
        {
            Text = "项目github仓库",
            IsLink = true,
            ForeColor = Color.Blue,
            ToolTipText = GitHubRepoUrl,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧 GitHub 仓库链接点击事件处理
        _repoLinkLabel.Click += (s, e) =>
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo
                {
                    FileName = GitHubRepoUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        // 右侧信息标签3
        _infoLabel3 = new()
        {
            Text = "提交 Issue 或 Pull Request",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 添加标签到状态栏
        _ = Items.Add(_statusLabel);
        _ = Items.Add(_infoLabel1);
        _ = Items.Add(_authorLinkLabel);
        _ = Items.Add(_infoLabel2);
        _ = Items.Add(_repoLinkLabel);
        _ = Items.Add(_infoLabel3);
    }

    /// <summary>
    /// 设置左侧状态文本
    /// </summary>
    /// <param name="status">状态枚举</param>
    public void SetStatus(StatusBarState status) => _statusLabel.Text = status switch
    {
        StatusBarState.Ready => "状态: 就绪",
        StatusBarState.SetStartAndEnd => "状态: 设置起点和终点",
        StatusBarState.SetObstacle => "状态: 设置障碍物",
        StatusBarState.SearchingPath => "状态: 正在搜索路径",
        _ => "状态: 未知",
    };
}
