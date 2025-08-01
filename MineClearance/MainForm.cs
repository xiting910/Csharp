using AutoUpdaterDotNET;
using System.Runtime.InteropServices;

namespace MineClearance;

/// <summary>
/// 表示游戏主界面的窗体
/// </summary>
public partial class MainForm : Form
{
    /// <summary>
    /// 用于处理WM_MOVING消息的结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        /// <summary>
        /// 左边界
        /// </summary>
        public int Left;
        /// <summary>
        /// 上边界
        /// </summary>
        public int Top;
        /// <summary>
        /// 右边界
        /// </summary>
        public int Right;
        /// <summary>
        /// 下边界
        /// </summary>
        public int Bottom;
    }

    /// <summary>
    /// 菜单面板, 包含新游戏、显示排行榜和退出按钮
    /// </summary>
    private readonly MenuPanel _menuPanel;

    /// <summary>
    /// 游戏准备面板, 选择难度或者自定义
    /// </summary>
    private readonly GamePreparePanel _gamePreparePanel;

    /// <summary>
    /// 游戏面板, 显示当前游戏状态
    /// </summary>
    private readonly GamePanel _gamePanel;

    /// <summary>
    /// 排行榜面板, 显示历史记录
    /// </summary>
    private readonly RankingPanel _rankingPanel;

    /// <summary>
    /// 底部状态栏
    /// </summary>
    private readonly BottomStatusBar _bottomStatusBar;

    /// <summary>
    /// 初始化主窗体
    /// </summary>
    public MainForm()
    {
        // 绑定未捕获异常事件
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        // 设置窗口属性
        Text = "扫雷游戏";
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.LightBlue;

        // 绑定关闭事件
        FormClosing += MainFormClosing;

        // 初始化面板
        _gamePanel = new(this);
        _menuPanel = new(this);
        _rankingPanel = new(this);
        _gamePreparePanel = new(this, _gamePanel);
        _bottomStatusBar = new(Constants.AuthorName, Constants.GitHubRepoUrl);
        Controls.Add(_bottomStatusBar);

        // 订阅底部状态栏状态改变事件
        _gamePanel.StatusBarStateChanged += _bottomStatusBar.SetStatus;

        // 显示菜单面板
        ShowPanel(PanelType.Menu);

        // 程序启动时检查更新
        Methods.IsHandlingUpdateEvent = true;
        AutoUpdater.Mandatory = true;
        AutoUpdater.RunUpdateAsAdmin = false;
        AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
        AutoUpdater.Start(Constants.AutoUpdateUrl);
    }

    /// <summary>
    /// 切换到指定面板
    /// </summary>
    /// <param name="targetPanel">目标面板</param>
    public void ShowPanel(PanelType targetPanel)
    {
        // 隐藏所有面板
        if (_menuPanel != null) _menuPanel.Visible = false;
        if (_gamePreparePanel != null) _gamePreparePanel.Visible = false;
        if (_gamePanel != null) _gamePanel.Visible = false;
        if (_rankingPanel != null) _rankingPanel.Visible = false;

        // 显示目标面板
        switch (targetPanel)
        {
            case PanelType.Menu:
                if (_menuPanel != null) _menuPanel.Visible = true;
                _bottomStatusBar.SetStatus(StatusBarState.Ready);
                break;
            case PanelType.GamePrepare:
                if (_gamePreparePanel != null) _gamePreparePanel.Visible = true;
                _bottomStatusBar.SetStatus(StatusBarState.Preparing);
                break;
            case PanelType.Game:
                if (_gamePanel != null) _gamePanel.Visible = true;
                break;
            case PanelType.Ranking:
                _rankingPanel?.RestartRankingPanel();
                if (_rankingPanel != null) _rankingPanel.Visible = true;
                _bottomStatusBar.SetStatus(StatusBarState.Ranking);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetPanel), targetPanel, null);
        }
    }

    /// <summary>
    /// 关闭程序事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MainFormClosing(object? sender, FormClosingEventArgs e)
    {
        // 先取消关闭
        e.Cancel = true;

        // 如果正在处理更新事件, 向用户确认是否要强制关闭
        if (Methods.IsHandlingUpdateEvent)
        {
            var result = MessageBox.Show("正在处理更新事件, 确定要强制关闭程序吗？", "警告", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            // 如果用户不选择强制关闭, 则取消关闭事件
            if (result != DialogResult.Yes)
            {
                return;
            }

            // 如果用户选择强制关闭, 则取消更新事件处理
            Methods.CTS.Cancel();

            // 弹窗提示正在等待
            using var waitingForm = new WaitingForm();
            waitingForm.Show();

            // 等待处理更新事件完成时间
            var waitUntil = DateTime.Now.AddSeconds(10);
            while (Methods.IsHandlingUpdateEvent && DateTime.Now < waitUntil)
            {
                await Task.Delay(100);
            }

            // 关闭等待提示窗口
            waitingForm.Close();
        }

        // 检测有没有更新文件残留
        if (File.Exists(Constants.SevenZipPath))
        {
            var deleteResult = MessageBox.Show($"检测到更新文件 {Constants.SevenZipPath} 残留, 可能是之前程序尝试自动更新失败导致的, 您可以手动将该 7z 压缩文件解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) , 或者您想要将其删除吗？", @"更新文件残留", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            try
            {
                if (deleteResult == DialogResult.Yes)
                {
                    File.Delete(Constants.SevenZipPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除残留的更新文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 删除残留的powershell脚本
        if (File.Exists(Constants.UpdatePowerShellScriptPath))
        {
            try { File.Delete(Constants.UpdatePowerShellScriptPath); } catch { }
        }

        // 取消关闭事件的绑定
        FormClosing -= MainFormClosing;

        // 关闭应用程序
        Close();
    }

    /// <summary>
    /// 处理自动更新检查事件
    /// </summary>
    /// <param name="args"></param>
    private async void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error == null)
        {
            if (args.IsUpdateAvailable)
            {
                // 获取更新日志内容
                string changelog = await Methods.GetLatestUpdateLog(args.ChangelogURL, args.CurrentVersion);

                DialogResult dialogResult;
                if (args.Mandatory.Value)
                {
                    dialogResult = MessageBox.Show($"新版本 {args.CurrentVersion} 可用, 更新日志: {changelog}\n您当前正在使用版本 {args.InstalledVersion}。这是强制更新。按确定开始更新应用程序。", @"更新可用", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    dialogResult = MessageBox.Show($"新版本 {args.CurrentVersion} 可用, 更新日志: {changelog}\n您当前正在使用版本 {args.InstalledVersion}。你想现在更新应用程序吗？", @"更新可用", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                }

                if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                {
                    // 检测下载的更新文件是否存在
                    if (File.Exists(Constants.SevenZipPath))
                    {
                        // 如果文件已存在, 提示用户是否覆盖
                        var overwriteResult = MessageBox.Show($"文件 {Constants.SevenZipPath} 已存在, 可能是之前程序尝试自动更新失败导致的残留, 您可以手动将该 7z 压缩文件解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) , 或者您想要覆盖下载更新吗？", @"更新文件已存在", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        if (overwriteResult == DialogResult.Yes)
                        {
                            // 用户选择覆盖, 删除旧文件
                            try { File.Delete(Constants.SevenZipPath); } catch { }
                        }
                        else
                        {
                            // 用户选择不覆盖, 取消更新
                            Methods.IsHandlingUpdateEvent = false;
                            Methods.IsFirstCheck = false;
                            return;
                        }
                    }

                    // 自动下载更新文件
                    bool downloadSuccess = await Methods.DownloadUpdate(args.DownloadURL);

                    // 如果下载成功, 自动启动更新脚本并退出应用程序
                    if (downloadSuccess)
                    {
                        // 弹窗提示下载完成
                        MessageBox.Show($"更新文件已成功下载到{Constants.SevenZipPath}\n程序将尝试删除 {Constants.CurrentDirectory} 文件夹后使用 {Constants.SevenZipExe} 解压下载的 7z 压缩包并自动更新\n如果自动更新失败, 请手动将下载的 7z 压缩文件包解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) ", @"下载完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 取消关闭事件的绑定
                        FormClosing -= MainFormClosing;

                        // 创建并启动自动更新的 PowerShell 脚本
                        Methods.StartAutoUpdateScript();

                        // 退出应用程序
                        Application.Exit();
                    }
                }
            }
            else if (!Methods.IsFirstCheck)
            {
                MessageBox.Show($@"您当前的版本 {args.InstalledVersion} 已经是最新版本, 无需更新。", @"没有可用的更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else if (!Methods.IsFirstCheck)
        {
            if (args.Error is System.Net.WebException)
            {
                MessageBox.Show(@"无法连接到更新服务器。请检查您的互联网连接, 然后稍后再试。", @"更新检查失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(args.Error.Message, args.Error.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        Methods.IsHandlingUpdateEvent = false;
        Methods.IsFirstCheck = false;
    }

    /// <summary>
    /// 记录异常到日志文件
    /// </summary>
    /// <param name="ex">要记录的异常</param>
    private static async Task LogException(Exception ex)
    {
        string log = $"[{DateTime.Now}] {ex}\n";
        try
        {
            if (!Directory.Exists(Constants.DataPath))
            {
                Directory.CreateDirectory(Constants.DataPath);
            }
            await File.AppendAllTextAsync(Constants.ErrorFilePath, log);
        }
        catch { /* 忽略日志写入异常 */ }
    }

    /// <summary>
    /// 处理未处理的线程异常
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">线程异常事件参数</param>
    private async void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        // 取消下载
        Methods.CTS.Cancel();

        // 取消关闭事件的绑定
        FormClosing -= MainFormClosing;

        // 记录异常到日志文件并弹窗提示错误信息
        var logTask = LogException(e.Exception);
        MessageBox.Show($"发生未处理的线程异常：{e.Exception.Message}\n错误日志见 {Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

        // 确认日志写入完成后退出应用程序
        await logTask;
        Application.Exit();
    }

    /// <summary>
    /// 处理未处理的应用程序异常
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">未处理异常事件参数</param>
    private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // 取消下载
        Methods.CTS.Cancel();

        // 取消关闭事件的绑定
        FormClosing -= MainFormClosing;

        // 记录异常到日志文件并弹窗提示错误信息
        Task logTask;
        if (e.ExceptionObject is Exception ex)
        {
            logTask = LogException(ex);
            MessageBox.Show($"发生未处理的应用程序异常：{ex.Message}\n错误日志见 {Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            logTask = LogException(new Exception("发生未知的未处理异常。"));
            MessageBox.Show($"发生未知的未处理异常\n错误日志见 {Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 确认日志写入完成后退出应用程序
        await logTask;
        Application.Exit();
    }

    /// <summary>
    /// 重写WndProc方法, 处理WM_MOVING消息, 用于使窗口保持在可见区域内
    /// </summary>
    protected override void WndProc(ref Message m)
    {
        const int WM_MOVING = 0x0216;

        if (m.Msg == WM_MOVING)
        {
            // 获取当前屏幕的工作区域
            Rectangle workingArea = Screen.GetWorkingArea(this);

            // 获取窗口的当前位置
            object? rectObj = Marshal.PtrToStructure(m.LParam, typeof(RECT));

            if (rectObj is RECT rect)
            {
                // 调整位置
                if (rect.Left < workingArea.Left)
                {
                    rect.Right = workingArea.Left + (rect.Right - rect.Left);
                    rect.Left = workingArea.Left;
                }

                if (rect.Top < workingArea.Top)
                {
                    rect.Bottom = workingArea.Top + (rect.Bottom - rect.Top);
                    rect.Top = workingArea.Top;
                }

                if (rect.Right > workingArea.Right)
                {
                    rect.Left = workingArea.Right - (rect.Right - rect.Left);
                    rect.Right = workingArea.Right;
                }

                if (rect.Bottom > workingArea.Bottom)
                {
                    rect.Top = workingArea.Bottom - (rect.Bottom - rect.Top);
                    rect.Bottom = workingArea.Bottom;
                }

                // 将调整后的位置写回消息
                Marshal.StructureToPtr(rect, m.LParam, true);
            }
        }

        base.WndProc(ref m);
    }
}