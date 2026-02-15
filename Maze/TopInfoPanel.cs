using System.Diagnostics;

namespace Maze;

/// <summary>
/// 顶部信息面板类, 用于管理和显示顶部信息面板, 提供按钮点击事件处理
/// </summary>
internal sealed class TopInfoPanel : IDisposable
{
    /// <summary>
    /// 面板切换事件
    /// </summary>
    public event Action<OperationStatus>? OnPanelSwitched;

    /// <summary>
    /// 随机生成迷宫事件
    /// </summary>
    public event Func<GenerationAlgorithm, Task>? OnGenerateMaze;

    /// <summary>
    /// 重置迷宫事件
    /// </summary>
    public event Func<Task>? OnMazeReset;

    /// <summary>
    /// 全部设为障碍物事件
    /// </summary>
    public event Func<Task>? OnSetAllObstacles;

    /// <summary>
    /// 清除障碍物事件
    /// </summary>
    public event Func<Task>? OnClearObstacles;

    /// <summary>
    /// 保存迷宫事件
    /// </summary>
    public event Func<Task>? OnSaveMaze;

    /// <summary>
    /// 加载迷宫事件
    /// </summary>
    public event Func<Task>? OnLoadMaze;

    /// <summary>
    /// 当前显示的面板
    /// </summary>
    public Panel CurrentPanel { get; private set; }

    /// <summary>
    /// 搜索时间标签
    /// </summary>
    public Label? SearchingTimeLabel { get; set; }

    /// <summary>
    /// 主信息面板
    /// </summary>
    private readonly Panel _mainPanel;

    /// <summary>
    /// 设置起点和终点面板
    /// </summary>
    private readonly Panel _startEndPanel;

    /// <summary>
    /// 设置障碍物面板
    /// </summary>
    private readonly Panel _obstaclePanel;

    /// <summary>
    /// 生成迷宫算法选择面板
    /// </summary>
    private readonly Panel _generationAlgorithmPanel;

    /// <summary>
    /// 选择搜索算法面板
    /// </summary>
    private readonly Panel _searchAlgorithmPanel;

    /// <summary>
    /// 正在搜索面板
    /// </summary>
    private readonly Panel _searchingPanel;

    /// <summary>
    /// 是否显示搜索过程的复选框
    /// </summary>
    private bool _showSearchProcessChecked;

    /// <summary>
    /// 正在搜索计时器
    /// </summary>
    private readonly System.Windows.Forms.Timer _searchingTimer;

    /// <summary>
    /// 搜索用时
    /// </summary>
    private readonly Stopwatch _searchingStopwatch;

    /// <summary>
    /// 构造函数, 初始化顶部信息面板
    /// </summary>
    public TopInfoPanel()
    {
        _showSearchProcessChecked = false;
        _mainPanel = CreateInfoPanel.CreateMainPanel(this);
        _startEndPanel = CreateInfoPanel.CreateStartEndPanel(this);
        _obstaclePanel = CreateInfoPanel.CreateObstaclePanel(this);
        _generationAlgorithmPanel = CreateInfoPanel.CreateGenerationAlgorithmPanel(this);
        _searchAlgorithmPanel = CreateInfoPanel.CreateSearchAlgorithmPanel(this);
        _searchingPanel = CreateInfoPanel.CreateSearchingPanel(this);
        CurrentPanel = _mainPanel;

        // 设置默认的搜索计时器
        _searchingStopwatch = new Stopwatch();
        _searchingTimer = new System.Windows.Forms.Timer
        {
            Interval = 10
        };
        _searchingTimer.Tick += (s, e) => SearchingTimeLabel?.Text = $"已用时: {_searchingStopwatch.Elapsed.TotalSeconds:F2} 秒";
    }

    /// <summary>
    /// 返回按钮点击事件处理
    /// </summary>
    public void OnBackButtonClick(object? sender, EventArgs e)
    {
        CurrentPanel = _mainPanel;
        OnPanelSwitched?.Invoke(OperationStatus.Default);
    }

    /// <summary>
    /// 保存当前迷宫按钮点击事件处理
    /// </summary>
    public async void OnSaveMazeButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        if (OnSaveMaze is not null)
        {
            await OnSaveMaze.Invoke();
        }
    }

    /// <summary>
    /// 加载迷宫按钮点击事件处理
    /// </summary>
    public async void OnLoadMazeButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        if (OnLoadMaze is not null)
        {
            await OnLoadMaze.Invoke();
        }
    }

    /// <summary>
    /// 设置起点和终点按钮点击事件处理
    /// </summary>
    public async void OnStartEndButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        // 切换到设置起点和终点面板
        CurrentPanel = _startEndPanel;
        OnPanelSwitched?.Invoke(OperationStatus.SetStartAndEnd);
    }

    /// <summary>
    /// 设置障碍物按钮点击事件处理
    /// </summary>
    public async void OnObstacleButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        // 切换到设置障碍物面板
        CurrentPanel = _obstaclePanel;
        OnPanelSwitched?.Invoke(OperationStatus.SetObstacle);
    }

    /// <summary>
    /// 重置迷宫按钮点击事件处理
    /// </summary>
    public async void OnResetButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        if (OnMazeReset is not null)
        {
            await OnMazeReset.Invoke();
        }
    }

    /// <summary>
    /// 全部设为障碍物按钮点击事件处理
    /// </summary>
    public async void OnSetAllObstaclesButtonClick(object? sender, EventArgs e)
    {
        if (OnSetAllObstacles is not null)
        {
            await OnSetAllObstacles.Invoke();
        }
    }

    /// <summary>
    /// 清除障碍物按钮点击事件处理
    /// </summary>
    public async void OnClearObstaclesButtonClick(object? sender, EventArgs e)
    {
        if (OnClearObstacles is not null)
        {
            await OnClearObstacles.Invoke();
        }
    }

    /// <summary>
    /// 开始搜索按钮点击事件处理
    /// </summary>
    public void OnStartSearchButtonClick(object? sender, EventArgs e)
    {
        // 检查起点和终点是否已设置
        if (!Maze.IsStartAndEndSet())
        {
            _ = MessageBox.Show("请先设置起点和终点", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        CurrentPanel = _searchAlgorithmPanel;
        OnPanelSwitched?.Invoke(OperationStatus.Default);
    }

    /// <summary>
    /// BFS搜索按钮点击事件处理
    /// </summary>
    public async void OnBfsButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        // 重置取消令牌
        Maze.CTS.Dispose();
        Maze.CTS = new CancellationTokenSource();

        // 执行BFS搜索
        await SearchPathAsync(SearchAlgorithm.BFS, Maze.CTS.Token);
    }

    /// <summary>
    /// DFS搜索按钮点击事件处理
    /// </summary>
    public async void OnDfsButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        // 重置取消令牌
        Maze.CTS.Dispose();
        Maze.CTS = new CancellationTokenSource();

        // 执行DFS搜索
        await SearchPathAsync(SearchAlgorithm.DFS, Maze.CTS.Token);
    }

    /// <summary>
    /// A*搜索按钮点击事件处理
    /// </summary>
    public async void OnAStarButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        // 重置取消令牌
        Maze.CTS.Dispose();
        Maze.CTS = new CancellationTokenSource();

        // 执行A*搜索
        await SearchPathAsync(SearchAlgorithm.AStar, Maze.CTS.Token);
    }

    /// <summary>
    /// 显示搜索过程复选框的勾选状态改变事件处理
    /// </summary>
    public void OnShowSearchProcessCheckedChanged(object? sender, EventArgs e) => _showSearchProcessChecked = !_showSearchProcessChecked;

    /// <summary>
    /// 查看搜索过程按钮点击事件处理
    /// </summary>
    public void OnViewSearchProcessButtonClick(object? sender, EventArgs e)
    {
        // 如果当前没有在搜索, 则不执行操作
        if (!Maze.IsSearching)
        {
            return;
        }

        // 切换到正在搜索面板
        CurrentPanel = _searchingPanel;
        OnPanelSwitched?.Invoke(OperationStatus.Default);
    }

    /// <summary>
    /// 搜索界面的返回按钮点击事件处理
    /// </summary>
    public void OnSearchBackButtonClick(object? sender, EventArgs e)
    {
        // 弹窗提示是否要返回, 支持本次运行不再提示
        var result = CustomMessageBox.Show("是否要退出当前界面? 注意: 这并不会取消当前搜索", "提示", "SearchBack");
        if (result != DialogResult.Yes)
        {
            return;
        }

        // 如果还在正在搜索面板, 则返回到搜索算法选择面板
        if (CurrentPanel == _searchingPanel)
        {
            CurrentPanel = _searchAlgorithmPanel;
            OnPanelSwitched?.Invoke(OperationStatus.Default);
        }
    }

    /// <summary>
    /// 搜索结束界面的返回按钮点击事件处理
    /// </summary>
    public void OnSearchEndBackButtonClick(object? sender, EventArgs e)
    {
        Maze.ClearPaths();
        CurrentPanel = _searchAlgorithmPanel;
        OnPanelSwitched?.Invoke(OperationStatus.Default);
    }

    /// <summary>
    /// 随机生成迷宫按钮点击事件处理
    /// </summary>
    public async void OnGenerateMazeButtonClick(object? sender, EventArgs e)
    {
        // 确保当前没有正在搜索
        if (!await Maze.EnsureNotSearching())
        {
            return;
        }

        CurrentPanel = _generationAlgorithmPanel;
        OnPanelSwitched?.Invoke(OperationStatus.Default);
    }

    /// <summary>
    /// 完全随机生成迷宫按钮点击事件处理
    /// </summary>
    public async void OnRandomGenerateMazeButtonClick(object? sender, EventArgs e)
    {
        if (OnGenerateMaze is not null)
        {
            await OnGenerateMaze.Invoke(GenerationAlgorithm.Random);
        }
    }

    /// <summary>
    /// DFS生成迷宫按钮点击事件处理
    /// </summary>
    public async void OnDfsGenerateMazeButtonClick(object? sender, EventArgs e)
    {
        if (OnGenerateMaze is not null)
        {
            await OnGenerateMaze.Invoke(GenerationAlgorithm.DFS);
        }
    }

    /// <summary>
    /// 递归分割生成迷宫按钮点击事件处理
    /// </summary>
    public async void OnRecursiveDivisionGenerateMazeButtonClick(object? sender, EventArgs e)
    {
        if (OnGenerateMaze is not null)
        {
            await OnGenerateMaze.Invoke(GenerationAlgorithm.RecursiveDivision);
        }
    }

    /// <summary>
    /// Prim生成迷宫按钮点击事件处理
    /// </summary>
    public async void OnPrimGenerateMazeButtonClick(object? sender, EventArgs e)
    {
        if (OnGenerateMaze is not null)
        {
            await OnGenerateMaze.Invoke(GenerationAlgorithm.Prim);
        }
    }

    /// <summary>
    /// 取消当前搜索按钮点击事件处理
    /// </summary>
    public async void OnCancelSearchButtonClick(object? sender, EventArgs e)
    {
        // 如果当前没有在搜索, 则不执行取消操作
        if (!Maze.IsSearching)
        {
            return;
        }

        // 取消当前搜索
        Maze.CTS.Cancel();
        Maze.CTS.Dispose();

        // 如果当前面板是正在搜索面板, 则切换到搜索算法选择面板
        if (CurrentPanel == _searchingPanel)
        {
            CurrentPanel = _searchAlgorithmPanel;
            OnPanelSwitched?.Invoke(OperationStatus.Default);
        }

        // 等待搜索取消完成
        await Maze.WaitForCancellation();

        // 清除路径
        Maze.ClearPaths();
    }

    /// <summary>
    /// 搜索路径
    /// </summary>
    /// <param name="algorithm">搜索算法</param>
    /// <param name="token">取消令牌</param>
    private async Task SearchPathAsync(SearchAlgorithm algorithm, CancellationToken token)
    {
        // 切换到正在搜索面板
        CurrentPanel = _searchingPanel;
        OnPanelSwitched?.Invoke(OperationStatus.Default);

        // 设置正在搜索状态
        Maze.IsSearching = true;

        // 开始计时
        _searchingStopwatch.Restart();
        _searchingTimer.Start();

        // 执行搜索
        await Maze.SearchPath(algorithm, _showSearchProcessChecked, token).ContinueWith(t =>
        {
            // 清理搜索状态
            Maze.IsSearching = false;

            // 停止计时
            _searchingStopwatch.Stop();
            _searchingTimer.Stop();

            // 这里可以根据t.Status判断是否取消、异常或成功
            if (t.IsCanceled)
            {
                // 取消时的处理
                return;
            }
            if (t.IsFaulted)
            {
                // 异常时的处理
                _ = MessageBox.Show("搜索过程中发生异常: " + t.Exception?.GetBaseException().Message);
                return;
            }

            // 正常完成时的处理
            var result = t.Result;

            // 如果搜索被取消, 则返回
            if (result.IsCanceled)
            {
                return;
            }

            // 切换到搜索结束面板
            CurrentPanel = CreateInfoPanel.CreateSearchEndPanel(this, result, _searchingStopwatch.Elapsed);
            OnPanelSwitched?.Invoke(OperationStatus.Default);

        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// 实现 Dispose 方法, 释放资源
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _searchingTimer.Dispose();
    }
}
