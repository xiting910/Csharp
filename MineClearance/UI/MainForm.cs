using System.Runtime.InteropServices;
using MineClearance.Models;
using MineClearance.Services;
using MineClearance.Utilities;

namespace MineClearance.UI;

/// <summary>
/// 表示游戏主界面的窗体
/// </summary>
internal sealed class MainForm : Form
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
        Size = new(UIConstants.MainFormWidth, UIConstants.MainFormHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.LightBlue;

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
    /// 重写OnFormClosing方法
    /// </summary>
    /// <param name="e">窗体关闭事件参数</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 保存主窗体位置
        Settings.ModifyConfig(config => config with
        {
            MainForm = new()
            {
                Left = Left,
                Top = Top
            }
        });
    }

    /// <summary>
    /// 重写OnLoad方法, 恢复窗口位置和大小
    /// </summary>
    /// <param name="e">窗体加载事件参数</param>
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
    /// <param name="m">Windows 消息</param>
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