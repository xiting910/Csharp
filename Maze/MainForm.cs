using System.Runtime.InteropServices;

namespace Maze;

/// <summary>
/// 主窗体类, 负责显示和管理界面
/// </summary>
internal sealed class MainForm : Form
{
    /// <summary>
    /// 主窗体的单例
    /// </summary>
    public static MainForm Instance { get; } = new();

    /// <summary>
    /// 顶部信息面板
    /// </summary>
    private readonly TopInfoPanel _topInfoPanel;

    /// <summary>
    /// 迷宫面板
    /// </summary>
    private readonly MazePanel _mazePanel;

    /// <summary>
    /// 当前显示的信息面板
    /// </summary>
    private Panel _currentPanel;

    /// <summary>
    /// 私有构造函数, 初始化主窗体
    /// </summary>
    private MainForm()
    {
        // 设置窗口属性
        Text = "迷宫寻路";
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        // 初始化顶部信息面板和迷宫面板
        _topInfoPanel = new();
        _mazePanel = new();

        // 添加顶部信息面板切换的事件处理
        _topInfoPanel.OnPanelSwitched += OnTopInfoPanelSwitched;

        // 添加重置迷宫事件处理
        _topInfoPanel.OnMazeReset += _mazePanel.ResetMaze;

        // 添加全部设为障碍物事件处理
        _topInfoPanel.OnSetAllObstacles += _mazePanel.SetAllObstacles;

        // 添加清除障碍物事件处理
        _topInfoPanel.OnClearObstacles += _mazePanel.ClearAllObstacles;

        // 添加保存迷宫事件处理
        _topInfoPanel.OnSaveMaze += _mazePanel.SaveMaze;

        // 添加加载迷宫事件处理
        _topInfoPanel.OnLoadMaze += _mazePanel.LoadMaze;

        // 添加随机生成迷宫事件处理
        _topInfoPanel.OnGenerateMaze += _mazePanel.GenerateMaze;

        // 添加格子类型改变事件处理
        _mazePanel.OnGridTypeChanged += Maze.ChangeGridType;

        // 订阅迷宫格子事件
        Maze.OnGridTypeChanged += _mazePanel.ChangeGridType;

        // 添加刷新面板事件处理
        Maze.OnPanelRefreshed += _mazePanel.Panel.Invalidate;

        // 添加搜索状态切换事件处理
        Maze.OnSearchingChanged += OnSearchStatusChanged;

        // 添加面板到窗体
        _currentPanel = _topInfoPanel.CurrentPanel;
        Controls.Add(_topInfoPanel.CurrentPanel);
        Controls.Add(_mazePanel.Panel);
        Controls.Add(BottomStatusBar.Instance);
    }

    /// <summary>
    /// 顶部信息面板切换事件处理
    /// </summary>
    /// <param name="status">当前操作状态</param>
    private void OnTopInfoPanelSwitched(OperationStatus status)
    {
        // 如果不是在UI更新线程中, 则记录异常信息并退出应用程序
        if (InvokeRequired)
        {
            // 记录异常到日志文件并弹窗提示错误信息
            var ex = new InvalidOperationException("切换顶部信息面板时不是在UI线程中调用");
            Methods.LogException(ex);
            _ = MessageBox.Show($"发生错误: {ex.Message}\n错误日志已保存到: {Constants.ErrorFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // 退出应用程序
            Application.Exit();
        }

        // 切换顶部信息面板并更新当前状态
        _mazePanel.CurrentStatus = status;
        Controls.Remove(_currentPanel);
        Controls.Add(_topInfoPanel.CurrentPanel);
        Controls.SetChildIndex(_topInfoPanel.CurrentPanel, 0);
        _currentPanel = _topInfoPanel.CurrentPanel;

        // 如果当前正在搜索, 不更新底部状态栏状态
        if (Maze.IsSearching)
        {
            return;
        }

        // 更新状态栏状态
        BottomStatusBar.Instance.SetStatus(status switch
        {
            OperationStatus.Default => StatusBarState.Ready,
            OperationStatus.SetStartAndEnd => StatusBarState.SetStartAndEnd,
            OperationStatus.SetObstacle => StatusBarState.SetObstacle,
            _ => StatusBarState.Ready
        });
    }

    /// <summary>
    /// 搜索状态切换事件处理
    /// </summary>
    /// <param name="isSearching">是否正在搜索</param>
    private void OnSearchStatusChanged(bool isSearching)
    {
        // 根据搜索状态更新UI
        if (isSearching)
        {
            BottomStatusBar.Instance.SetStatus(StatusBarState.SearchingPath);
        }
        else
        {
            BottomStatusBar.Instance.SetStatus(StatusBarState.Ready);
        }
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
