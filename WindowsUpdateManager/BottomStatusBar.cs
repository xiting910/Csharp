using System.Diagnostics;

namespace WindowsUpdateManager;

/// <summary>
/// 底部状态栏
/// </summary>
internal sealed class BottomStatusBar : StatusStrip
{
    /// <summary>
    /// 底部状态栏的单例
    /// </summary>
    public static BottomStatusBar Instance { get; } = new();

    /// <summary>
    /// 作者名字
    /// </summary>
    private const string AuthorName = "xiting910";

    /// <summary>
    /// GitHub 仓库链接
    /// </summary>
    private const string GitHubRepoUrl = "https://github.com/xiting910/Csharp/tree/main/WindowsUpdateManager";

    /// <summary>
    /// 信息标签1
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel1;

    /// <summary>
    /// 项目仓库链接标签
    /// </summary>
    private readonly ToolStripStatusLabel _repoLinkLabel;

    /// <summary>
    /// 信息标签2
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel2;

    /// <summary>
    /// 私有构造函数, 初始化状态栏
    /// </summary>
    private BottomStatusBar()
    {
        // 设置状态栏属性
        Dock = DockStyle.Bottom;
        BackColor = Color.White;
        ShowItemToolTips = true;

        // 右侧信息标签1
        _infoLabel1 = new()
        {
            Text = $"本项目由 {AuthorName} 一人开发。如您有建议或发现问题，请访问",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // GitHub仓库链接
        _repoLinkLabel = new()
        {
            Text = "项目github仓库",
            IsLink = true,
            ForeColor = Color.Blue,
            ToolTipText = GitHubRepoUrl,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // GitHub仓库链接点击事件处理
        _repoLinkLabel.Click += (s, e) =>
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = GitHubRepoUrl,
                UseShellExecute = true
            });
        };

        // 右侧信息标签2
        _infoLabel2 = new()
        {
            Text = "提交Issue或Pull Request",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 创建左侧弹性填充标签
        var springLabelLeft = new ToolStripStatusLabel
        {
            Spring = true,
            BorderSides = ToolStripStatusLabelBorderSides.None
        };

        // 创建右侧弹性填充标签
        var springLabelRight = new ToolStripStatusLabel
        {
            Spring = true,
            BorderSides = ToolStripStatusLabelBorderSides.None
        };

        // 添加标签到状态栏
        _ = Items.Add(springLabelLeft);
        _ = Items.Add(_infoLabel1);
        _ = Items.Add(_repoLinkLabel);
        _ = Items.Add(_infoLabel2);
        _ = Items.Add(springLabelRight);
    }
}