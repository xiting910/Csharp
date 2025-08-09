using System.Diagnostics;
using MineClearance.Models;

namespace MineClearance.UI;

/// <summary>
/// 底部状态栏
/// </summary>
public class BottomStatusBar : StatusStrip
{
    /// <summary>
    /// 作者名字
    /// </summary>
    private const string AuthorName = "xiting910";

    /// <summary>
    /// GitHub 仓库链接
    /// </summary>
    private const string GitHubRepoUrl = "https://github.com/xiting910/Csharp/tree/main/MineClearance";

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
    /// 状态栏状态切换事件
    /// </summary>
    private static event Action<StatusBarState>? StatusBarStateChanged;

    /// <summary>
    /// 构造函数，初始化状态栏
    /// </summary>
    public BottomStatusBar()
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
            Text = $"本项目由{AuthorName}一人开发。如您有建议或发现问题，请访问",
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
            ToolTipText = GitHubRepoUrl,
            TextAlign = ContentAlignment.MiddleRight
        };
        _repoLinkLabel.Click += (s, e) =>
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = GitHubRepoUrl,
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
        _ = Items.Add(_statusLabel);
        _ = Items.Add(_infoLabel1);
        _ = Items.Add(_repoLinkLabel);
        _ = Items.Add(_infoLabel2);

        // 订阅状态栏状态切换事件
        StatusBarStateChanged += SetStatus;
    }

    /// <summary>
    /// 切换状态栏状态
    /// </summary>
    /// <param name="status">新的状态</param>
    public static void ChangeStatus(StatusBarState status)
    {
        StatusBarStateChanged?.Invoke(status);
    }

    /// <summary>
    /// 设置左侧状态文本
    /// </summary>
    /// <param name="status">状态</param>
    private void SetStatus(StatusBarState status)
    {
        _statusLabel.Text = status switch
        {
            StatusBarState.Ready => "状态: 就绪",
            StatusBarState.History => "状态: 历史记录",
            StatusBarState.Preparing => "状态: 准备游戏",
            StatusBarState.InGame => "状态: 游戏中",
            StatusBarState.Paused => "状态: 游戏暂停",
            StatusBarState.GameWon => "状态: 游戏胜利",
            StatusBarState.GameLost => "状态: 游戏失败",
            _ => "状态: 未知"
        };
    }

    /// <summary>
    /// 重写Dispose方法, 取消静态事件的订阅, 防止内存泄漏
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 取消静态事件的订阅，防止内存泄漏
            StatusBarStateChanged -= SetStatus;
        }
        base.Dispose(disposing);
    }
}