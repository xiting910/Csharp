using System.Diagnostics;
using MineClearance.Core;
using MineClearance.Models;
using MineClearance.Models.Enums;
using MineClearance.Services;
using MineClearance.Utilities;
using MineClearance.UI.Assist;

namespace MineClearance.UI.Main;

// 游戏面板类的游戏部分
internal partial class GamePanel
{
    /// <summary>
    /// 用于记录游戏实际时间的stopwatch
    /// </summary>
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// 游戏实例, 控制游戏逻辑
    /// </summary>
    private Game? _gameInstance;

    /// <summary>
    /// 游戏左上角格子的位置
    /// </summary>
    private Position _gameStartPosition;

    /// <summary>
    /// 游戏剩余地雷数量
    /// </summary>
    private int _remainingMines;

    /// <summary>
    /// 游戏未处理格子数量
    /// </summary>
    private int _unopenedCount;

    /// <summary>
    /// 游戏是否胜利
    /// </summary>
    private bool _isGameWon;

    /// <summary>
    /// 游戏是否失败
    /// </summary>
    private bool _isGameLost;

    /// <summary>
    /// 鼠标按下状态
    /// </summary>
    private bool _isMouseDown;

    /// <summary>
    /// 鼠标按下的按钮类型
    /// </summary>
    private MouseButtons _mouseButton;

    /// <summary>
    /// 鼠标当前所在的游戏格子位置
    /// </summary>
    private Position _mouseGridPosition;

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <exception cref="InvalidOperationException">当前存在游戏实例时抛出</exception>
    public void StartGame(Game game)
    {
        // 如果当前游戏实例不为空, 抛出异常
        if (_gameInstance != null)
        {
            throw new InvalidOperationException("当前已经存在游戏实例");
        }

        // 当前鼠标状态重置
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
        _mouseGridPosition = Position.Invalid;

        // 设置游戏实例
        _gameInstance = game;

        // 重置游戏状态
        _isGameWon = false;
        _isGameLost = false;
        _pauseResumeCheckBox.Checked = false;

        // 设置游戏左上角格子位置, 使游戏能在面板居中显示
        var colOffset = (Constants.MaxBoardWidth - _gameInstance.Board.Width) / 2;
        _gameStartPosition = new(0, colOffset);

        // 运行游戏实例
        _gameInstance.Run();

        // 订阅游戏事件
        _gameInstance.GameWon += OnGameWon;
        _gameInstance.GameLost += OnGameLost;
        _gameInstance.Board.FirstClick += OnGameStarted;
        _gameInstance.Board.GridChanged += OnGridChanged;
        _gameInstance.Board.RemainingMinesChanged += OnRemainingMinesChanged;
        _gameInstance.Board.UnopenedCountChanged += OnUnopenedCountChanged;
        _gameInstance.Board.UnopenedSafeCountChanged += OnUnopenedSafeCountChanged;

        // 重绘游戏区域面板
        _gameAreaPanel.Invalidate();

        // 切换状态栏状态
        BottomStatusBar.Instance.SetStatus(StatusBarState.WaitingStart);

        // 更新剩余地雷数
        _remainingMines = _gameInstance.TotalMines;

        // 更新未处理格子数量
        _unopenedCount = _gameInstance.Board.Width * _gameInstance.Board.Height;

        // 计算当前游戏雷率
        var mineRate = _remainingMines * 100.0 / _unopenedCount;

        // 更新当前游戏难度标签
        _difficultyLabel.Text = $"当前游戏难度: {UIMethods.GetDifficultyText(_gameInstance.Difficulty)}(雷率: {mineRate:0.##}%)";

        // 更新剩余地雷数标签
        _minesLeftLabel.Text = $"剩余地雷数: {_remainingMines}";

        // 更新未处理格子数标签
        _unopenedCountLabel.Text = $"未处理格子数: {_unopenedCount}";

        // 更新完成度标签
        _completionLabel.Text = $"完成度: 0%";

        // 更新游戏时间标签
        _gameTimeLabel.Text = "游戏时间: 00:00";

        // 添加提示气泡
        _toolTip.SetToolTip(_difficultyLabel, $"宽度: {_gameInstance.Board.Width}, 高度: {_gameInstance.Board.Height}, 总地雷数: {_remainingMines}");
        _toolTip.SetToolTip(_minesLeftLabel, $"总地雷数: {_remainingMines}");
        _toolTip.SetToolTip(_unopenedCountLabel, $"总格子数: {_unopenedCount}");
        _toolTip.SetToolTip(_completionLabel, $"已打开安全格子数: 0, 总安全格子数: {_gameInstance.TotalSafeCount}");

        // 记录游戏开始信息
        FileLogger.LogInfo($"开始难度为 {_gameInstance.Difficulty} 的新游戏: " + $"宽度为 {_gameInstance.Board.Width}, 高度为 {_gameInstance.Board.Height}, 地雷数为 {_remainingMines}");
    }

    /// <summary>
    /// 结束游戏(停止计时器并清理游戏实例)
    /// </summary>
    private void EndGame()
    {
        // 如果计时器没有停止, 则停止计时器
        if (_gameTimer.Enabled)
        {
            _gameTimer.Stop();
        }

        // 如果实际时间计时器没有停止, 则停止实际时间计时器
        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
        }

        // 取消订阅游戏事件
        if (_gameInstance != null)
        {
            _gameInstance.GameWon -= OnGameWon;
            _gameInstance.GameLost -= OnGameLost;
            _gameInstance.Board.FirstClick -= OnGameStarted;
            _gameInstance.Board.GridChanged -= OnGridChanged;
            _gameInstance.Board.RemainingMinesChanged -= OnRemainingMinesChanged;
            _gameInstance.Board.UnopenedCountChanged -= OnUnopenedCountChanged;
            _gameInstance.Board.UnopenedSafeCountChanged -= OnUnopenedSafeCountChanged;
        }

        // 清理游戏实例
        _gameInstance = null;
    }

    /// <summary>
    /// 重新开始当前游戏
    /// </summary>
    /// <exception cref="ArgumentNullException">当前游戏实例为空抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">自定义的宽度、高度或地雷数超出范围时抛出</exception>
    /// <exception cref="ArgumentException">非自定义难度没有对应的宽度、高度或地雷数时抛出</exception>
    private void RestartGame()
    {
        // 如果当前游戏实例为空, 抛出异常
        ArgumentNullException.ThrowIfNull(_gameInstance, nameof(_gameInstance));

        // 如果游戏还没有开始, 不做任何处理
        if (_gameInstance.StartTime == DateTime.MinValue)
        {
            return;
        }

        // 获取当前游戏难度、高度、宽度和地雷数
        var difficulty = _gameInstance.Difficulty;
        var width = _gameInstance.Board.Width;
        var height = _gameInstance.Board.Height;
        var mineCount = _gameInstance.TotalMines;

        // 结束当前游戏
        EndGame();

        // 使用获取的参数重新开始游戏
        if (difficulty == DifficultyLevel.Custom)
        {
            StartGame(new(width, height, mineCount));
        }
        else
        {
            StartGame(new(difficulty));
        }
    }

    /// <summary>
    /// 游戏开始事件处理
    /// </summary>
    private void OnGameStarted()
    {
        // 重启计时器
        _stopwatch.Restart();

        // 启动游戏计时器
        _gameTimer.Start();

        // 更新状态栏
        BottomStatusBar.Instance.SetStatus(StatusBarState.InGame);
    }

    /// <summary>
    /// 游戏胜利事件处理
    /// </summary>
    /// <param name="gameResult">游戏结果</param>
    private void OnGameWon(GameResult gameResult)
    {
        // 保存游戏结果
        _ = Datas.AddGameResultAsync(gameResult);

        // 切换状态栏状态
        BottomStatusBar.Instance.SetStatus(StatusBarState.GameWon);

        // 游戏胜利
        _isGameWon = true;

        // 重绘游戏区域面板
        _gameAreaPanel.Invalidate();

        // 更新剩余地雷数标签
        _minesLeftLabel.Text = "剩余地雷数: 0";

        // 更新未处理格子数标签
        _unopenedCountLabel.Text = $"未处理格子数: 0";

        // 停止计时器
        _gameTimer.Stop();

        // 弹出游戏胜利提示, 并询问是否再来一局
        var result = MessageBox.Show("恭喜你，赢得了游戏！\n绿色格子表示地雷\n是否再来一局?", "游戏胜利", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

        if (result == DialogResult.Yes)
        {
            // 重新开始游戏
            RestartGame();
        }
    }

    /// <summary>
    /// 游戏失败事件处理
    /// </summary>
    /// <param name="gameResult">游戏结果</param>
    private void OnGameLost(GameResult gameResult)
    {
        // 保存游戏结果
        _ = Datas.AddGameResultAsync(gameResult);

        // 切换状态栏状态
        BottomStatusBar.Instance.SetStatus(StatusBarState.GameLost);

        // 游戏失败
        _isGameLost = true;

        // 重绘游戏区域面板
        _gameAreaPanel.Invalidate();

        // 停止计时器
        _gameTimer.Stop();

        // 弹出游戏失败提示, 并询问是否再来一局
        var result = MessageBox.Show("很遗憾，你踩到了地雷！\n深红色格子表示踩到的地雷\n红色格子表示未标记的地雷\n绿色格子表示正确标记的地雷\n黄色格子表示错误标记的地雷\n是否再来一局?", "游戏失败", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

        if (result == DialogResult.Yes)
        {
            // 重新开始游戏
            RestartGame();
        }
    }

    /// <summary>
    /// 暂停/继续游戏复选框点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnPauseResumeCheckBoxCheckedChanged(object? sender, EventArgs e)
    {
        // 游戏是否已经开始
        var gameStarted = _gameInstance != null && _gameInstance.StartTime != DateTime.MinValue;

        // 游戏是否已经结束
        var gameEnded = _isGameWon || _isGameLost;

        // 根据复选框状态暂停或继续游戏
        if (_pauseResumeCheckBox.Checked)
        {
            // 如果游戏未开始或者游戏已经结束, 则切换复选框状态为未选中
            if (!gameStarted || gameEnded)
            {
                _pauseResumeCheckBox.Checked = false;
                return;
            }

            // 更新_pausedResumedCheckBox的文本和颜色
            _pauseResumeCheckBox.Text = "继续游戏";
            _pauseResumeCheckBox.BackColor = Color.DarkGreen;

            // 更新状态栏
            BottomStatusBar.Instance.SetStatus(StatusBarState.Paused);

            // 暂停实际时间计时器
            _stopwatch.Stop();

            // 暂停游戏计时器
            _gameTimer.Stop();
        }
        else
        {
            // 更新_pausedResumedCheckBox的文本和颜色
            _pauseResumeCheckBox.Text = "暂停游戏";
            _pauseResumeCheckBox.BackColor = Color.Coral;

            // 如果游戏已经开始并且没有结束, 则继续游戏
            if (gameStarted && !gameEnded)
            {
                // 更新状态栏
                BottomStatusBar.Instance.SetStatus(StatusBarState.InGame);

                // 继续实际时间计时器
                _stopwatch.Start();

                // 继续游戏计时器
                _gameTimer.Start();
            }
        }
    }

    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnGameAreaMouseDown(object? sender, MouseEventArgs e)
    {
        // 如果游戏已经结束或者暂停, 不做任何处理
        if (_isGameWon || _isGameLost || _pauseResumeCheckBox.Checked)
        {
            return;
        }

        // 设置鼠标按下状态
        _isMouseDown = true;
        _mouseButton = e.Button;

        // 获取鼠标位置
        if (sender is DoubleBufferedPanel)
        {
            // 获取鼠标位置对应的格子坐标
            var pos = GetGridPositionAtMousePosition(e.Location);

            // 如果鼠标位置不在任何格子上, 不做任何处理
            if (pos == Position.Invalid)
            {
                return;
            }

            // 如果鼠标位置在格子上, 触发相应的左键或右键操作
            if (_mouseButton == MouseButtons.Left)
            {
                _gameInstance?.Board.OnLeftClick(pos);
            }
            else if (_mouseButton == MouseButtons.Right)
            {
                _gameInstance?.Board.OnRightClick(pos);
            }
        }
    }

    /// <summary>
    /// 处理鼠标移动事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnGameAreaMouseMove(object? sender, MouseEventArgs e)
    {
        // 如果游戏已经结束或者暂停, 不做任何处理
        if (_isGameWon || _isGameLost || _pauseResumeCheckBox.Checked)
        {
            return;
        }

        if (sender is DoubleBufferedPanel)
        {
            // 获取鼠标位置对应的格子坐标
            var pos = GetGridPositionAtMousePosition(e.Location);

            // 如果pos与当前鼠标格子位置相同, 则不做任何处理
            if (pos == _mouseGridPosition)
            {
                return;
            }

            // 先前鼠标格子位置
            var prevPos = _mouseGridPosition;

            // 更新当前鼠标格子位置
            _mouseGridPosition = pos;

            // 如果先前鼠标格子位置不为无效位置, 则重绘先前格子
            if (prevPos != Position.Invalid)
            {
                // 获取先前格子行列坐标
                var prevRow = prevPos.Row + _gameStartPosition.Row;
                var prevCol = prevPos.Col + _gameStartPosition.Col;

                // 重绘先前格子
                _gameAreaPanel.Invalidate(new Rectangle(prevCol * UIConstants.GridSize, prevRow * UIConstants.GridSize, UIConstants.GridSize, UIConstants.GridSize));
            }

            // 如果pos为无效位置, 则返回
            if (pos == Position.Invalid)
            {
                return;
            }

            // 如果鼠标按下且鼠标按钮为右键, 则标记地雷
            if (_isMouseDown && _mouseButton == MouseButtons.Right)
            {
                _gameInstance?.Board.OnRightClick(pos);
            }
            // 如果鼠标按下且鼠标按钮为左键, 则打开格子
            else if (_isMouseDown && _mouseButton == MouseButtons.Left)
            {
                _gameInstance?.Board.OnLeftClick(pos);
            }
            // 如果鼠标未按下, 则聚焦
            else if (!_isMouseDown)
            {
                // 获取当前格子行列坐标
                var currRow = _mouseGridPosition.Row + _gameStartPosition.Row;
                var currCol = _mouseGridPosition.Col + _gameStartPosition.Col;

                // 重绘当前鼠标格子
                _gameAreaPanel.Invalidate(new Rectangle(currCol * UIConstants.GridSize, currRow * UIConstants.GridSize, UIConstants.GridSize, UIConstants.GridSize));
            }
        }
    }

    /// <summary>
    /// 处理鼠标释放事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnGameAreaMouseUp(object? sender, MouseEventArgs e)
    {
        // 重置鼠标状态
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
    }
}