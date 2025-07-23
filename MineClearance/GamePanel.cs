namespace MineClearance;

/// <summary>
/// 游戏面板类
/// </summary>
public partial class GamePanel : Panel
{
    /// <summary>
    /// 返回菜单事件
    /// </summary>
    private event Action? BackToMenu;

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
    /// 鼠标按下状态
    /// </summary>
    private bool _isMouseDown;

    /// <summary>
    /// 游戏是否结束
    /// </summary>
    private bool _isGameOver;

    /// <summary>
    /// 鼠标按下的按钮类型
    /// </summary>
    private MouseButtons _mouseButton;

    /// <summary>
    /// 鼠标当前所在的格子位置
    /// </summary>
    private Position _mouseGridPosition;

    /// <summary>
    /// 初始化游戏面板
    /// </summary>
    /// <param name="mainForm">主窗体</param>
    public GamePanel(MainForm mainForm)
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

        // 设置面板的返回菜单事件
        BackToMenu += () =>
        {
            // 结束当前游戏并返回菜单
            _gameTimer.Stop();
            EndGame();
            mainForm.ShowPanel(PanelType.Menu);
        };

        // 初始化鼠标状态
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
        _mouseGridPosition = new(-1, -1);

        // 初始化信息面板
        _infoPanel = new()
        {
            Size = new Size(Constants.MainFormWidth, 40),
            Location = new Point(0, 0),
            BackColor = Color.LightBlue,
        };

        // 添加剩下的地雷数标签
        _minesLeftLabel = new()
        {
            Text = $"剩余地雷数: {_gameInstance?.TotalMines}",
            Location = new Point(200, 10),
            AutoSize = true
        };

        // 添加游戏时间标签
        _gameTimeLabel = new()
        {
            Text = "游戏时间: 00:00",
            Location = new Point(300, 10),
            AutoSize = true
        };

        // 添加返回菜单按钮
        Button btnBackMenu = new()
        {
            Text = "返回菜单",
            Location = new Point(10, 5),
            AutoSize = true
        };
        btnBackMenu.Click += (sender, e) =>
        {
            // 触发返回菜单事件
            BackToMenu?.Invoke();
        };

        // 添加重新开始按钮
        Button btnRestart = new()
        {
            Text = "重新开始",
            Location = new Point(100, 5),
            AutoSize = true
        };
        btnRestart.Click += BtnRestart_Click;

        // 添加提示信息
        Label hintLabel = new()
        {
            Text = "提示: 左键打开格子, 右键标记地雷（在打开一个格子之前无效）, 灰色格子为未打开, 绿色格子表示插旗",
            Location = new Point(500, 10),
            AutoSize = true
        };

        // 添加信息面板控件
        _infoPanel.Controls.Add(_minesLeftLabel);
        _infoPanel.Controls.Add(_gameTimeLabel);
        _infoPanel.Controls.Add(btnBackMenu);
        _infoPanel.Controls.Add(btnRestart);
        _infoPanel.Controls.Add(hintLabel);

        // 初始化游戏区域面板
        _gameAreaPanel = new()
        {
            BackColor = Color.White,
            Location = new(0, 40),
            Size = new(Constants.MainFormWidth, Constants.MainFormHeight - Constants.BottomStatusBarHeight - 40)
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
            var rowStart = Math.Max(0, clip.Top / Constants.GridSize);
            var rowEnd = clip.Bottom / Constants.GridSize;
            var colStart = Math.Max(0, clip.Left / Constants.GridSize);
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
        mainForm.Controls.Add(this);
    }

    /// <summary>
    /// 启动游戏, 自动切换到游戏面板或者返回菜单
    /// </summary>
    /// <param name="game">游戏实例</param>
    public void StartGame(Game game)
    {
        // 当前鼠标状态重置
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
        _mouseGridPosition = new(-1, -1);

        // 设置游戏实例
        _gameInstance = game;
        _isGameOver = false;

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

        // 更新剩余地雷数标签
        _minesLeftLabel.Text = $"剩余地雷数: {_gameInstance.TotalMines}";

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
        DifficultyLevel difficulty = _gameInstance.Difficulty;
        int width = _gameInstance.Board.Width;
        int height = _gameInstance.Board.Height;
        int mineCount = _gameInstance.TotalMines;

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
        // 设置鼠标按下状态
        _isMouseDown = true;
        _mouseButton = e.Button;

        // 获取鼠标位置
        if (sender is DoubleBufferedPanel)
        {
            // 获取鼠标位置对应的格子坐标
            var pos = GetGridPositionAtMousePosition(e.Location);

            // 如果鼠标位置不在任何格子上, 不做任何处理
            if (pos == new Position(-1, -1))
            {
                return;
            }

            // 如果鼠标位置在格子上, 触发相应的左键或右键操作
            if (_mouseButton == MouseButtons.Left)
            {
                _gameInstance?.Board.OnGridClick(pos, false);
            }
            else if (_mouseButton == MouseButtons.Right)
            {
                _gameInstance?.Board.OnGridClick(pos, true);
            }
        }
    }

    /// <summary>
    /// 处理鼠标移动事件
    /// </summary>
    private void GameAreaMouseMove(object? sender, MouseEventArgs e)
    {
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
            if (prevPos != new Position(-1, -1))
            {
                _gameAreaPanel.Invalidate(new Rectangle(prevPos.Col * Constants.GridSize, prevPos.Row * Constants.GridSize, Constants.GridSize, Constants.GridSize));
            }

            // 如果pos为(-1,-1), 则返回
            if (pos == new Position(-1, -1))
            {
                return;
            }

            // 如果鼠标按下且鼠标按钮为右键, 则标记地雷
            if (_isMouseDown && _mouseButton == MouseButtons.Right)
            {
                _gameInstance?.Board.OnGridClick(pos, true);
            }
            // 如果鼠标按下且鼠标按钮为左键, 则打开格子
            else if (_isMouseDown && _mouseButton == MouseButtons.Left)
            {
                _gameInstance?.Board.OnGridClick(pos, false);
            }
            // 如果鼠标未按下, 则聚焦
            else if (!_isMouseDown)
            {
                // 重绘当前鼠标格子
                _gameAreaPanel.Invalidate(new Rectangle(_mouseGridPosition.Col * Constants.GridSize, _mouseGridPosition.Row * Constants.GridSize, Constants.GridSize, Constants.GridSize));
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
    /// 根据鼠标当前位置返回所在的格子位置
    /// </summary>
    /// <param name="mousePosition">鼠标位置</param>
    /// <returns>对应的格子位置, 如果不在任何格子上则返回(-1,-1)</returns>
    private Position GetGridPositionAtMousePosition(Point mousePosition)
    {
        // 计算鼠标位置对应的格子行列
        var col = mousePosition.X / Constants.GridSize;
        var row = mousePosition.Y / Constants.GridSize;

        if (row >= 0 && row < _gameInstance?.Board.Height && col >= 0 && col < _gameInstance?.Board.Width)
        {
            // 返回对应的位置
            return new(row, col);
        }

        // 如果鼠标位置不在任何格子上, 返回(-1,-1)
        return new(-1, -1);
    }

    /// <summary>
    /// 绘制指定位置的格子
    /// </summary>
    /// <param name="g">绘图对象</param>
    /// <param name="row">行索引</param>
    /// <param name="col">列索引</param>
    private void DrawGrid(Graphics g, int row, int col)
    {
        // 如果游戏实例为空或行列索引无效, 不绘制
        if (_gameInstance == null || row < 0 || col < 0 || row >= _gameInstance.Board.Height || col >= _gameInstance.Board.Width)
        {
            return;
        }

        // 当前格子是否真的是地雷格子
        var isRealMine = _gameInstance.Board.Mines.MineGrid[row, col] == -1;

        // 获取格子矩形区域
        var rect = new Rectangle(col * Constants.GridSize, row * Constants.GridSize, Constants.GridSize, Constants.GridSize);

        // 根据格子类型选择颜色和文本
        var fillColor = Color.LightGray;
        var text = "";
        switch (_gameInstance.Board.Grids[row, col].Type)
        {
            case GridType.Empty:
                fillColor = Color.White;
                break;
            case GridType.Unopened:
                if (_isGameOver && isRealMine)
                {
                    fillColor = Color.Red;
                }
                else
                {
                    fillColor = _mouseGridPosition == new Position(row, col) ? Color.Gray : Color.LightGray;
                }
                break;
            case GridType.Flagged:
                if (_isGameOver)
                {
                    fillColor = isRealMine ? Color.Green : Color.Yellow;
                }
                else
                {
                    fillColor = _mouseGridPosition == new Position(row, col) ? Color.DarkGreen : Color.Green;
                }
                break;
            case GridType.Number:
                fillColor = Color.White;
                text = _gameInstance.Board.Grids[row, col].SurroundingMines.ToString();
                break;
            case GridType.Mine:
                fillColor = Color.DarkRed;
                break;
        }

        // 绘制格子背景
        using (var brush = new SolidBrush(fillColor)) g.FillRectangle(brush, rect);

        // 绘制格子边框
        using (var pen = new Pen(Color.Black, 1))
        {
            g.DrawRectangle(pen, rect);
        }

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
    /// 游戏胜利事件处理
    /// </summary>
    /// <param name="gameResult">游戏结果</param>
    private void OnGameWon(GameResult gameResult)
    {
        // 游戏结束
        _isGameOver = true;

        // 保存游戏结果
        Task.Run(() => Datas.AddGameResultAsync(gameResult));

        // 停止计时器
        _gameTimer.Stop();

        // 弹出游戏胜利提示, 并询问是否再来一局
        var result = MessageBox.Show("恭喜你，赢得了游戏！\n绿色格子表示正确标记的地雷\n\n是否再来一局?", "游戏胜利", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

        if (result == DialogResult.Yes)
        {
            // 重新开始游戏
            RestartGame();
        }
        else
        {
            // 返回主菜单
            BackToMenu?.Invoke();
        }
    }

    /// <summary>
    /// 游戏失败事件处理
    /// </summary>
    /// <param name="gameResult">游戏结果</param>
    private void OnGameLost(GameResult gameResult)
    {
        // 游戏结束
        _isGameOver = true;

        // 显示所有地雷位置
        for (int row = 0; row < _gameInstance?.Board.Height; ++row)
        {
            for (int col = 0; col < _gameInstance?.Board.Width; ++col)
            {
                var grid = _gameInstance.Board.Grids[row, col];
                var realMine = _gameInstance.Board.Mines.MineGrid[row, col] == -1;
                if (realMine || grid.Type == GridType.Flagged)
                {
                    _gameAreaPanel.Invalidate(new Rectangle(col * Constants.GridSize, row * Constants.GridSize, Constants.GridSize, Constants.GridSize));
                }
            }
        }

        // 保存游戏结果
        Task.Run(() => Datas.AddGameResultAsync(gameResult));

        // 停止计时器
        _gameTimer.Stop();

        // 弹出游戏失败提示, 并询问是否再来一局
        var result = MessageBox.Show("很遗憾，你踩到了地雷！\n深红色格子表示踩到的地雷\n红色格子表示未标记的地雷\n绿色格子表示正确标记的地雷\n黄色格子表示错误标记的地雷\n\n是否再来一局?", "游戏失败", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

        if (result == DialogResult.Yes)
        {
            // 重新开始游戏
            RestartGame();
        }
        else
        {
            // 返回主菜单
            BackToMenu?.Invoke();
        }
    }

    /// <summary>
    /// 格子状态改变事件处理
    /// </summary>
    private void OnGridChanged(Position position)
    {
        // 更新格子显示
        _gameAreaPanel.Invalidate(new Rectangle(position.Col * Constants.GridSize, position.Row * Constants.GridSize, Constants.GridSize, Constants.GridSize));
    }

    /// <summary>
    /// 剩余地雷数量改变事件处理
    /// </summary>
    private void OnRemainingMinesChanged(int remainingMines)
    {
        // 更新剩余地雷数量显示
        remainingMines = Math.Max(0, remainingMines);
        _minesLeftLabel.Text = $"剩余地雷: {remainingMines}";
    }
}