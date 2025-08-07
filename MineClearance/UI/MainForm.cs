using AutoUpdaterDotNET;
using System.Runtime.InteropServices;
using MineClearance.Models;
using MineClearance.Services;
using MineClearance.Utilities;

namespace MineClearance.UI;

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
    /// 面板字典, 存储不同类型的面板
    /// </summary>
    private readonly Dictionary<PanelType, Panel> _panels;

    /// <summary>
    /// 底部状态栏
    /// </summary>
    private readonly BottomStatusBar _bottomStatusBar;

    /// <summary>
    /// 面板切换事件
    /// </summary>
    private static event Action<PanelType>? OnPanelSwitched;

    /// <summary>
    /// 初始化主窗体
    /// </summary>
    public MainForm()
    {
        // 获取当前版本号
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        // 设置窗口属性
        Text = "扫雷游戏 - 版本 " + version;
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.LightBlue;

        // 绑定关闭事件
        FormClosing += MainFormClosing;

        // 初始化所有面板
        _panels = new Dictionary<PanelType, Panel>
        {
            { PanelType.Game, new GamePanel() },
            { PanelType.Menu, new MenuPanel() },
            { PanelType.History, new HistoryPanel() },
            { PanelType.GamePrepare, new GamePreparePanel() }
        };

        // 初始化底部状态栏
        _bottomStatusBar = new();

        // 添加所有面板到窗体
        Controls.AddRange([.. _panels.Values]);

        // 添加底部状态栏到窗体
        Controls.Add(_bottomStatusBar);

        // 订阅面板切换事件
        OnPanelSwitched += SwitchToPanel;

        // 显示菜单面板
        ShowPanel(PanelType.Menu);

        // 程序启动时检查更新
        Methods.IsHandlingUpdateEvent = true;
        AutoUpdater.Start(Constants.AutoUpdateUrl);
    }

    /// <summary>
    /// 显示指定类型的面板
    /// </summary>
    /// <param name="panelType">面板类型</param>
    public static void ShowPanel(PanelType panelType)
    {
        OnPanelSwitched?.Invoke(panelType);
    }

    /// <summary>
    /// 切换到指定类型的面板
    /// </summary>
    /// <param name="panelType">面板类型</param>
    private void SwitchToPanel(PanelType panelType)
    {
        // 隐藏所有面板
        foreach (var panel in _panels.Values)
        {
            panel.Visible = false;
        }

        // 更新底部状态栏
        BottomStatusBar.ChangeStatus(Methods.GetBottomStatusBarState(panelType));

        // 显示指定类型的面板
        if (_panels.TryGetValue(panelType, out var selectedPanel))
        {
            // 如果是历史记录面板, 则重启
            if (selectedPanel is HistoryPanel historyPanel)
            {
                historyPanel.RestartHistoryPanel();
            }

            // 设置面板可见
            selectedPanel.Visible = true;
        }
    }

    /// <summary>
    /// 关闭程序事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MainFormClosing(object? sender, FormClosingEventArgs e)
    {
        // 如果强制关闭标志为真, 则关闭
        if (Methods.IsForceClose)
        {
            // 保存主窗体位置
            Settings.ModifyConfig(config =>
            {
                return config with
                {
                    MainForm = new()
                    {
                        Left = Left,
                        Top = Top
                    }
                };
            });

            // 直接返回
            return;
        }

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
        if (File.Exists(Utilities.Constants.SevenZipPath))
        {
            var deleteResult = MessageBox.Show($"检测到更新文件 {Utilities.Constants.SevenZipPath} 残留, 可能是之前程序尝试自动更新失败导致的, 您可以手动将该 7z 压缩文件解压到目录 {Utilities.Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) , 或者您想要将其删除吗？", @"更新文件残留", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            try
            {
                if (deleteResult == DialogResult.Yes)
                {
                    File.Delete(Utilities.Constants.SevenZipPath);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"删除残留的更新文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 删除所有残留的脚本文件
        Script.RemoveAllResidualScripts();

        // 设置强制关闭标志
        Methods.IsForceClose = true;

        // 关闭应用程序
        Close();
    }

    /// <summary>
    /// 重写OnLoad方法, 恢复窗口位置和大小
    /// </summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // 加载配置数据
        Settings.LoadConfig();

        // 恢复窗口位置和大小
        if (Settings.Config.MainForm != null)
        {
            // 保存的位置
            var left = Settings.Config.MainForm.Left;
            var top = Settings.Config.MainForm.Top;

            // 当前屏幕的工作区域
            var workingArea = Screen.GetWorkingArea(this);

            // 确保位置在工作区域内
            if (left >= 0 && top >= 0 && left < workingArea.Width && top < workingArea.Height)
            {
                Left = left;
                Top = top;
            }
        }
    }

    /// <summary>
    /// 重写Dispose方法, 取消静态事件的订阅, 防止内存泄漏
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 取消静态事件订阅，防止内存泄漏
            OnPanelSwitched -= SwitchToPanel;
        }
        base.Dispose(disposing);
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
            var workingArea = Screen.GetWorkingArea(this);

            // 获取窗口的当前位置
            var rectObj = Marshal.PtrToStructure(m.LParam, typeof(RECT));

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