using System.Runtime.InteropServices;

namespace WindowsUpdateManager;

/// <summary>
/// 主窗体类, 表示主界面的窗体
/// </summary>
internal sealed class MainForm : Form
{
    /// <summary>
    /// 主窗体的单例
    /// </summary>
    public static MainForm Instance { get; } = new();

    /// <summary>
    /// 菜单面板
    /// </summary>
    private readonly MenuPanel _menuPanel;

    /// <summary>
    /// 私有构造函数, 初始化主窗体
    /// </summary>
    private MainForm()
    {
        // 设置窗口属性
        Text = Constants.ProgramName;
        Size = new(Constants.MainFormMinWidth, Constants.MainFormMinHeight);
        MinimumSize = new(Constants.MainFormMinWidth, Constants.MainFormMinHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;

        // 初始化菜单面板
        _menuPanel = new();

        // 添加控件到主窗体
        Controls.Add(_menuPanel);
        Controls.Add(BottomStatusBar.Instance);
    }

    /// <summary>
    /// 重写OnLoad方法, 恢复窗口位置和大小
    /// </summary>
    /// <param name="e">事件参数</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // 加载配置数据
        Settings.LoadConfig();

        // 保存的位置
        var left = Settings.Config.MainFormLeft;
        var top = Settings.Config.MainFormTop;

        // 保存的大小
        var width = Settings.Config.MainFormWidth;
        var height = Settings.Config.MainFormHeight;

        // 当前屏幕的工作区域
        var workingArea = Screen.GetWorkingArea(this);

        // 确保位置在工作区域内
        if (left >= 0 && top >= 0 && left < workingArea.Width && top < workingArea.Height)
        {
            Left = left;
            Top = top;
            Width = width < Constants.MainFormMinWidth ? Constants.MainFormMinWidth : width;
            Height = height < Constants.MainFormMinHeight ? Constants.MainFormMinHeight : height;
        }
    }

    /// <summary>
    /// 重写OnFormClosing方法, 保存窗口位置和大小
    /// </summary>
    /// <param name="e">窗口关闭事件参数</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 保存配置数据
        Settings.ModifyConfig(config => config with
        {
            MainFormWidth = Width,
            MainFormHeight = Height,
            MainFormLeft = Left,
            MainFormTop = Top
        });
    }

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
    /// 重写WndProc方法, 处理WM_MOVING消息, 用于使窗口保持在可见区域内
    /// </summary>
    /// <param name="m">Windows 消息参数</param>
    protected override void WndProc(ref Message m)
    {
        const int WM_MOVING = 0x0216;

        if (m.Msg == WM_MOVING)
        {
            // 获取当前屏幕的工作区域
            var workingArea = Screen.GetWorkingArea(this);

            // 获取窗口的当前位置
            var rectObj = Marshal.PtrToStructure<RECT>(m.LParam);

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
