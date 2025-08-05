namespace MineClearance;

/// <summary>
/// 游戏面板类
/// </summary>
public partial class GamePanel : Panel
{
    /// <summary>
    /// 游戏顶部信息栏
    /// </summary>
    private readonly Panel _infoPanel;

    /// <summary>
    /// 游戏区域面板, 显示游戏格子
    /// </summary>
    private readonly DoubleBufferedPanel _gameAreaPanel;

    /// <summary>
    /// 剩余地雷数标签
    /// </summary>
    private readonly Label _minesLeftLabel;

    /// <summary>
    /// 游戏时间标签
    /// </summary>
    private readonly Label _gameTimeLabel;

    /// <summary>
    /// 游戏计时器
    /// </summary>
    private readonly System.Windows.Forms.Timer _gameTimer;

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
    /// 开启新游戏事件
    /// </summary>
    private static event Action<Game>? GameStarted;

    /// <summary>
    /// 初始化游戏面板
    /// </summary>
    public GamePanel()
    {
        // 设置游戏面板属性
        Name = "GamePanel";
        Location = new(0, 0);
        Size = new(Constants.MainFormWidth, Constants.MainFormHeight - Constants.BottomStatusBarHeight);

        // 初始化游戏计时器
        _gameTimer = new System.Windows.Forms.Timer
        {
            Interval = 10
        };
        _gameTimer.Tick += GameTimer_Tick;

        // 初始化鼠标状态
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
        _mouseGridPosition = Constants.InvalidPosition;

        // 设置游戏左上角格子位置
        _gameStartPosition = new(0, 0);

        // 信息面板高度
        var infoPanelHeight = (int)(47 * Constants.DpiScale);

        // 标签Y轴位置
        var labelY = (int)(15 * Constants.DpiScale);

        // 初始化信息面板
        _infoPanel = new()
        {
            Size = new(Constants.MainFormWidth, infoPanelHeight),
            Location = new(0, 0),
            BackColor = Color.Cyan,
        };

        // 添加剩下的地雷数标签
        _minesLeftLabel = new()
        {
            Text = $"剩余地雷数: {_gameInstance?.TotalMines}",
            ForeColor = Color.DarkGreen,
            Location = new((int)(5 * Constants.DpiScale), labelY),
            AutoSize = true
        };

        // 添加游戏时间标签
        _gameTimeLabel = new()
        {
            Text = "游戏时间: 00:00",
            ForeColor = Color.DarkBlue,
            Location = new((int)(125 * Constants.DpiScale), labelY),
            AutoSize = true
        };

        // 添加提示信息
        Label hintLabel = new()
        {
            Text = "提示: 左键打开格子, 右键标记地雷(在打开一个格子之前无效), 支持按住鼠标滑动操作多个格子, 灰色格子为未打开, 绿色格子表示插旗\n左键点击数字格子时, 如果周围插旗数量等于数字, 则打开周围所有未插旗的格子(注意: 如果错误插旗可能会导致打开到地雷)\n右键点击数字格子时, 如果周围未打开格子数量等于数字, 则插旗所有周围未插旗的格子\n当数字格子周围插旗的数量大于数字时, 该格子会变为黄色表示警告状态",
            Font = new(DefaultFont.FontFamily, 3.1f * Constants.DpiScale, DefaultFont.Style),
            Location = new((int)(250 * Constants.DpiScale), 0),
            AutoSize = true
        };

        // 按钮Y轴位置
        var buttonY = (int)(12.5 * Constants.DpiScale);

        // 添加显示/隐藏提示按钮
        Button btnToggleHint = new()
        {
            Text = "显示/隐藏提示",
            BackColor = Color.YellowGreen,
            Location = new(Constants.MainFormWidth - (int)(270 * Constants.DpiScale), buttonY),
            FlatStyle = FlatStyle.Flat,
            AutoSize = true
        };
        btnToggleHint.Click += (sender, e) =>
        {
            hintLabel.Visible = !hintLabel.Visible;
        };

        // 添加重新开始按钮
        Button btnRestart = new()
        {
            Text = "重新开始",
            BackColor = Color.Yellow,
            Location = new(Constants.MainFormWidth - (int)(165 * Constants.DpiScale), buttonY),
            FlatStyle = FlatStyle.Flat,
            AutoSize = true
        };
        btnRestart.Click += BtnRestart_Click;

        // 添加返回菜单按钮
        Button btnBackMenu = new()
        {
            Text = "返回菜单",
            BackColor = Color.LightCoral,
            Location = new(Constants.MainFormWidth - (int)(90 * Constants.DpiScale), buttonY),
            FlatStyle = FlatStyle.Flat,
            AutoSize = true
        };
        btnBackMenu.Click += (sender, e) =>
        {
            // 结束当前游戏并返回菜单
            _gameTimer.Stop();
            EndGame();
            MainForm.ShowPanel(PanelType.Menu);
        };

        // 添加信息面板控件
        _infoPanel.Controls.Add(_minesLeftLabel);
        _infoPanel.Controls.Add(_gameTimeLabel);
        _infoPanel.Controls.Add(hintLabel);
        _infoPanel.Controls.Add(btnToggleHint);
        _infoPanel.Controls.Add(btnRestart);
        _infoPanel.Controls.Add(btnBackMenu);

        // 初始化游戏区域面板
        _gameAreaPanel = new()
        {
            BackColor = Color.White,
            Location = new(0, infoPanelHeight),
            Size = new(Constants.MainFormWidth, Constants.MainFormHeight - Constants.BottomStatusBarHeight - infoPanelHeight)
        };

        // 添加鼠标按下事件
        _gameAreaPanel.MouseDown += GameAreaMouseDown;

        // 添加鼠标移动事件
        _gameAreaPanel.MouseMove += GameAreaMouseMove;

        // 添加鼠标释放事件
        _gameAreaPanel.MouseUp += GameAreaMouseUp;

        // 绘制格子
        _gameAreaPanel.Paint += (s, e) =>
        {
            // 获取当前绘图区域
            var clip = e.ClipRectangle;
            var rowStart = clip.Top / Constants.GridSize;
            var rowEnd = clip.Bottom / Constants.GridSize;
            var colStart = clip.Left / Constants.GridSize;
            var colEnd = clip.Right / Constants.GridSize;

            // 遍历指定区域的格子并绘制
            for (var row = rowStart; row <= rowEnd; ++row)
            {
                for (var col = colStart; col <= colEnd; ++col)
                {
                    DrawGrid(e.Graphics, row, col);
                }
            }
        };

        // 添加信息面板和游戏区域面板到主窗体
        Controls.Add(_infoPanel);
        Controls.Add(_gameAreaPanel);

        // 订阅游戏开始事件
        GameStarted += StartGame;
    }

    /// <summary>
    /// 启动新游戏
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void StartNewGame(Game game)
    {
        // 触发游戏开始事件
        GameStarted?.Invoke(game);

        // 切换到游戏面板
        MainForm.ShowPanel(PanelType.Game);
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="game">游戏实例</param>
    private void StartGame(Game game)
    {
        // 切换状态栏状态
        BottomStatusBar.ChangeStatus(StatusBarState.InGame);

        // 当前鼠标状态重置
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
        _mouseGridPosition = Constants.InvalidPosition;

        // 设置游戏实例
        _gameInstance = game;
        _isGameWon = false;
        _isGameLost = false;

        // 设置游戏左上角格子位置, 使游戏能在面板居中显示
        var colOffset = (Constants.MaxBoardWidth - _gameInstance.Board.Width) / 2;
        _gameStartPosition = new(0, colOffset);

        // 运行游戏实例
        _gameInstance.Run();

        // 订阅游戏事件
        _gameInstance.GameWon += OnGameWon;
        _gameInstance.GameLost += OnGameLost;
        _gameInstance.Board.FirstClick += _gameTimer.Start;
        _gameInstance.Board.GridChanged += OnGridChanged;
        _gameInstance.Board.RemainingMinesChanged += OnRemainingMinesChanged;

        // 重绘游戏区域面板
        _gameAreaPanel.Invalidate();

        // 更新剩余地雷数
        _remainingMines = _gameInstance.TotalMines;

        // 更新剩余地雷数标签
        _minesLeftLabel.Text = $"剩余地雷数: {_remainingMines}";

        // 更新游戏时间标签
        _gameTimeLabel.Text = "游戏时间: 00:00";
    }

    /// <summary>
    /// 结束游戏
    /// </summary>
    private void EndGame()
    {
        // 取消订阅游戏事件
        if (_gameInstance != null)
        {
            _gameInstance.GameWon -= OnGameWon;
            _gameInstance.GameLost -= OnGameLost;

            _gameInstance.Board.FirstClick -= _gameTimer.Start;
            _gameInstance.Board.GridChanged -= OnGridChanged;
            _gameInstance.Board.RemainingMinesChanged -= OnRemainingMinesChanged;
        }

        // 清理游戏实例
        _gameInstance = null;
    }

    /// <summary>
    /// 重新开始当前游戏
    /// </summary>
    /// <exception cref="ArgumentNullException">如果当前游戏实例为空</exception>
    private void RestartGame()
    {
        // 如果当前游戏实例为空, 抛出异常
        if (_gameInstance == null)
        {
            throw new ArgumentNullException(nameof(_gameInstance), "当前游戏实例为空");
        }

        // 获取当前游戏难度、高度、宽度和地雷数
        var difficulty = _gameInstance.Difficulty;
        var width = _gameInstance.Board.Width;
        var height = _gameInstance.Board.Height;
        var mineCount = _gameInstance.TotalMines;

        // 结束当前游戏
        _gameTimer.Stop();
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
    /// 游戏胜利事件处理
    /// </summary>
    /// <param name="gameResult">游戏结果</param>
    private void OnGameWon(GameResult gameResult)
    {
        // 切换状态栏状态
        BottomStatusBar.ChangeStatus(StatusBarState.GameWon);

        // 游戏胜利
        _isGameWon = true;

        // 重绘游戏区域面板
        _gameAreaPanel.Invalidate();

        // 显示剩余地雷数量为0
        _minesLeftLabel.Text = "剩余地雷数: 0";

        // 保存游戏结果
        Task.Run(() => Datas.AddGameResultAsync(gameResult));

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
        // 切换状态栏状态
        BottomStatusBar.ChangeStatus(StatusBarState.GameLost);

        // 游戏失败
        _isGameLost = true;

        // 重绘游戏区域面板
        _gameAreaPanel.Invalidate();

        // 保存游戏结果
        Task.Run(() => Datas.AddGameResultAsync(gameResult));

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
    /// 格子状态改变事件处理
    /// </summary>
    /// <param name="position">格子位置</param>
    private void OnGridChanged(Position position)
    {
        // 获取格子行列坐标
        var row = position.Row + _gameStartPosition.Row;
        var col = position.Col + _gameStartPosition.Col;

        // 重绘指定格子
        _gameAreaPanel.Invalidate(new Rectangle(col * Constants.GridSize, row * Constants.GridSize, Constants.GridSize, Constants.GridSize));
    }

    /// <summary>
    /// 剩余地雷数量改变事件处理
    /// </summary>
    /// <param name="changeNum">变化的数量</param>
    private void OnRemainingMinesChanged(int changeNum)
    {
        // 更新剩余地雷数量
        _remainingMines += changeNum;

        // 确保显示的剩余地雷数不小于0
        var showRemainingMines = Math.Max(0, _remainingMines);
        _minesLeftLabel.Text = $"剩余地雷数: {showRemainingMines}";
    }

    /// <summary>
    /// 游戏计时器事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        // 如果游戏实例为空, 不做任何处理
        if (_gameInstance == null)
        {
            return;
        }

        // 更新游戏时间标签
        var elapsed = DateTime.Now - _gameInstance.StartTime;
        var minutes = (int)elapsed.TotalMinutes;
        var seconds = elapsed.Seconds;
        _gameTimeLabel.Text = $"游戏时间: {minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// 重新开始按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void BtnRestart_Click(object? sender, EventArgs e)
    {
        try
        {
            // 尝试重新开始游戏
            RestartGame();
        }
        catch (ArgumentNullException ex)
        {
            MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    private void GameAreaMouseDown(object? sender, MouseEventArgs e)
    {
        // 如果游戏已经结束, 不做任何处理
        if (_isGameWon || _isGameLost)
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
            if (pos == Constants.InvalidPosition)
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
    private void GameAreaMouseMove(object? sender, MouseEventArgs e)
    {
        // 如果游戏已经结束, 不做任何处理
        if (_isGameWon || _isGameLost)
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

            // 如果先前鼠标格子位置不为(-1,-1), 则重绘先前格子
            if (prevPos != Constants.InvalidPosition)
            {
                // 获取先前格子行列坐标
                var prevRow = prevPos.Row + _gameStartPosition.Row;
                var prevCol = prevPos.Col + _gameStartPosition.Col;

                // 重绘先前格子
                _gameAreaPanel.Invalidate(new Rectangle(prevCol * Constants.GridSize, prevRow * Constants.GridSize, Constants.GridSize, Constants.GridSize));
            }

            // 如果pos为(-1,-1), 则返回
            if (pos == Constants.InvalidPosition)
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
                _gameAreaPanel.Invalidate(new Rectangle(currCol * Constants.GridSize, currRow * Constants.GridSize, Constants.GridSize, Constants.GridSize));
            }
        }
    }

    /// <summary>
    /// 处理鼠标释放事件
    /// </summary>
    private void GameAreaMouseUp(object? sender, MouseEventArgs e)
    {
        // 重置鼠标状态
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
    }

    /// <summary>
    /// 根据鼠标当前位置返回所在的游戏格子位置
    /// </summary>
    /// <param name="mousePosition">鼠标位置</param>
    /// <returns>对应的格子位置, 如果不在任何格子上则返回(-1,-1)</returns>
    private Position GetGridPositionAtMousePosition(Point mousePosition)
    {
        // 计算鼠标位置对应的游戏格子行列
        var col = mousePosition.X / Constants.GridSize - _gameStartPosition.Col;
        var row = mousePosition.Y / Constants.GridSize - _gameStartPosition.Row;

        if (row >= 0 && row < _gameInstance?.Board.Height && col >= 0 && col < _gameInstance?.Board.Width)
        {
            // 返回对应的位置
            return new(row, col);
        }

        // 如果鼠标位置不在任何格子上, 返回(-1,-1)
        return Constants.InvalidPosition;
    }

    /// <summary>
    /// 绘制指定位置的格子
    /// </summary>
    /// <param name="g">绘图对象</param>
    /// <param name="row">行索引</param>
    /// <param name="col">列索引</param>
    private void DrawGrid(Graphics g, int row, int col)
    {
        // 如果游戏实例为空, 不绘制
        if (_gameInstance == null || row < _gameStartPosition.Row || col < _gameStartPosition.Col)
        {
            return;
        }

        // 获取游戏的行列坐标
        var gameRow = row - _gameStartPosition.Row;
        var gameCol = col - _gameStartPosition.Col;

        // 如果行列索引无效, 不绘制
        if (gameRow >= _gameInstance.Board.Height || gameCol >= _gameInstance.Board.Width)
        {
            return;
        }

        // 获取格子矩形区域
        var rect = new Rectangle(col * Constants.GridSize, row * Constants.GridSize, Constants.GridSize, Constants.GridSize);

        // 当前格子是否真的是地雷格子
        var isRealMine = _gameInstance.Board.Mines.MineGrid[gameRow, gameCol] == -1;

        // 鼠标是否在当前格子上
        var isMouseOver = _mouseGridPosition.Row == gameRow && _mouseGridPosition.Col == gameCol;

        // 根据格子类型选择颜色和文本
        var text = "";
        var fillColor = Color.LightGray;
        switch (_gameInstance.Board.Grids[gameRow, gameCol].Type)
        {
            case GridType.Empty:
                fillColor = Color.White;
                break;
            case GridType.Unopened:
                if (_isGameLost && isRealMine)
                {
                    fillColor = Color.Red;
                }
                else if (_isGameWon && isRealMine)
                {
                    fillColor = Color.Green;
                }
                else
                {
                    fillColor = isMouseOver ? Color.Gray : Color.LightGray;
                }
                break;
            case GridType.Flagged:
                if (_isGameLost || _isGameWon)
                {
                    fillColor = isRealMine ? Color.Green : Color.Yellow;
                }
                else
                {
                    fillColor = isMouseOver ? Color.DarkGreen : Color.Green;
                }
                break;
            case GridType.Number:
                fillColor = Color.White;
                text = _gameInstance.Board.Grids[gameRow, gameCol].SurroundingMines.ToString();
                break;
            case GridType.WarningNumber:
                fillColor = Color.Yellow;
                text = _gameInstance.Board.Grids[gameRow, gameCol].SurroundingMines.ToString();
                break;
            case GridType.Mine:
                fillColor = Color.DarkRed;
                break;
        }

        // 绘制格子背景
        using var brush = new SolidBrush(fillColor);
        g.FillRectangle(brush, rect);

        // 绘制格子边框
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, rect);

        // 绘制文本（居中）
        if (!string.IsNullOrEmpty(text))
        {
            // 根据周围地雷数量设置文字颜色
            var textColor = text switch
            {
                "1" => Color.Blue,
                "2" => Color.Green,
                "3" => Color.Red,
                "4" => Color.Purple,
                "5" => Color.Maroon,
                "6" => Color.Turquoise,
                "7" => Color.Black,
                "8" => Color.Gray,
                _ => Color.Black
            };

            var textSize = g.MeasureString(text, DefaultFont);
            var textX = rect.X + (rect.Width - textSize.Width) / 2;
            var textY = rect.Y + (rect.Height - textSize.Height) / 2;
            using var textBrush = new SolidBrush(textColor);
            g.DrawString(text, DefaultFont, textBrush, textX, textY);
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
            GameStarted -= StartGame;
        }
        base.Dispose(disposing);
    }
}