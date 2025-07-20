namespace MineClearance
{
    public partial class GUI
    {
        /// <summary>
        /// 启动游戏, 自动切换到游戏面板或者返回菜单
        /// </summary>
        private void StartGame()
        {
            // 如果游戏实例已存在，则重新创建游戏区域
            if (gameInstance != null)
            {
                // 运行游戏实例
                gameInstance.Run();

                // 订阅游戏事件
                gameInstance.GameWon += OnGameWon;
                gameInstance.GameLost += OnGameLost;
                gameInstance.Board.FirstClick += StartGameTimer;
                gameInstance.Board.GridOpened += OnGridOpened;
                gameInstance.Board.GridFlagged += OnGridFlagged;
                gameInstance.Board.GridUnflagged += OnGridUnflagged;
                gameInstance.Board.RemainingMinesChanged += OnRemainingMinesChanged;

                // 先移除旧的游戏面板
                if (gamePanel != null)
                {
                    Controls.Remove(gamePanel);
                    gamePanel.Dispose();
                }

                // 创建新的游戏面板并显示
                CreateGamePanel();
                ShowPanel(gamePanel);
            }
            else
            {
                MessageBox.Show("请先选择游戏难度或自定义设置", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowPanel(menuPanel);
            }
        }

        /// <summary>
        /// 结束游戏, 不会自动返回菜单
        /// </summary>
        private void EndGame()
        {
            // 取消订阅游戏事件
            if (gameInstance != null)
            {
                gameInstance.GameWon -= OnGameWon;
                gameInstance.GameLost -= OnGameLost;

                if (gameInstance.Board != null)
                {
                    gameInstance.Board.FirstClick -= StartGameTimer;
                    gameInstance.Board.GridOpened -= OnGridOpened;
                    gameInstance.Board.GridFlagged -= OnGridFlagged;
                    gameInstance.Board.GridUnflagged -= OnGridUnflagged;
                    gameInstance.Board.RemainingMinesChanged -= OnRemainingMinesChanged;
                }
            }

            // 清理游戏实例和按钮
            gameInstance = null;
            if (gameButtons != null)
            {
                foreach (var button in gameButtons)
                {
                    // 取消订阅按钮事件
                    button.Click -= GameButton_Click;
                    button.MouseDown -= GameButton_MouseDown;
                    button.Dispose();
                }
                gameButtons = null;
            }

            // 清理游戏区域面板中的控件
            gameAreaPanel?.Controls.Clear();
        }

        /// <summary>
        /// 重新开始当前游戏
        /// </summary>
        /// <exception cref="ArgumentNullException">如果当前游戏实例为空</exception>
        private void RestartGame()
        {
            // 如果当前游戏实例为空, 抛出异常
            if (gameInstance == null)
            {
                throw new ArgumentNullException(nameof(gameInstance), "当前游戏实例为空");
            }

            // 获取当前游戏难度、高度、宽度和地雷数
            DifficultyLevel difficulty = gameInstance.Difficulty;
            int width = gameInstance.Board.Width;
            int height = gameInstance.Board.Height;
            int mineCount = gameInstance.TotalMines;

            // 结束当前游戏
            StopGameTimer();
            EndGame();

            // 使用获取的参数重新开始游戏
            if (difficulty == DifficultyLevel.Custom)
            {
                gameInstance = new Game(width, height, mineCount);
            }
            else
            {
                gameInstance = new Game(difficulty);
            }
            StartGame();
        }

        /// <summary>
        /// 启动游戏计时器
        /// </summary>
        private void StartGameTimer()
        {
            gameStartTime = DateTime.Now;

            // 创建计时器，每0.01秒更新一次
            gameTimer = new System.Windows.Forms.Timer
            {
                Interval = 10
            };
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        /// <summary>
        /// 游戏计时器事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (gameTimeLabel != null)
            {
                var elapsed = DateTime.Now - gameStartTime;
                var minutes = (int)elapsed.TotalMinutes;
                var seconds = elapsed.Seconds;
                gameTimeLabel.Text = $"游戏时间: {minutes:D2}:{seconds:D2}";
            }
        }

        /// <summary>
        /// 停止游戏计时器
        /// </summary>
        private void StopGameTimer()
        {
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Tick -= GameTimer_Tick;
                gameTimer.Dispose();
                gameTimer = null;
            }
        }

        /// <summary>
        /// 游戏胜利事件处理
        /// </summary>
        /// <param name="gameResult">游戏结果</param>
        private void OnGameWon(GameResult gameResult)
        {
            // 将所有标记的地雷设置为绿色
            if (gameButtons != null)
            {
                foreach (var button in gameButtons)
                {
                    if (button.BackColor == Color.Yellow)
                    {
                        button.BackColor = Color.Green;
                    }
                }
            }

            // 保存游戏结果
            Task.Run(() => Datas.AddGameResultAsync(gameResult));

            // 停止计时器
            StopGameTimer();

            // 弹出游戏胜利提示
            var result = MessageBox.Show("恭喜你，赢得了游戏！\n绿色格子表示正确标记的地雷\n\n是否再来一局？", "游戏胜利", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                // 如果用户选择再来一局, 则重新开始游戏
                RestartGame();
            }
            else
            {
                // 否则结束游戏并返回菜单面板
                EndGame();
                ShowPanel(menuPanel);
            }
        }

        /// <summary>
        /// 游戏失败事件处理
        /// </summary>
        /// <param name="gameResult">游戏结果</param>
        /// <param name="minePositions">地雷位置列表</param>
        private void OnGameLost(GameResult gameResult, List<(int row, int column)> minePositions)
        {
            // 显示所有地雷位置
            foreach (var (row, column) in minePositions)
            {
                if (gameButtons != null && row < gameButtons.GetLength(0) && column < gameButtons.GetLength(1))
                {
                    if (gameButtons[row, column].BackColor == Color.Yellow)
                    {
                        // 如果是标记的地雷, 设置为绿色
                        gameButtons[row, column].BackColor = Color.Green;
                    }
                    else
                    {
                        // 如果是未标记的地雷, 设置为红色
                        gameButtons[row, column].BackColor = Color.Red;
                    }
                }
            }

            // 保存游戏结果
            Task.Run(() => Datas.AddGameResultAsync(gameResult));

            // 停止计时器
            StopGameTimer();

            // 弹出游戏失败提示，增加“再来一局”按钮
            var result = MessageBox.Show("很遗憾，你踩到了地雷！\n红色格子表示未标记的地雷\n绿色格子表示正确标记的地雷\n黄色格子表示错误标记的地雷\n\n是否再来一局？", "游戏失败", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                // 如果用户选择再来一局, 则重新开始游戏
                RestartGame();
            }
            else
            {
                // 否则结束游戏并返回菜单面板
                EndGame();
                ShowPanel(menuPanel);
            }
        }

        /// <summary>
        /// 格子打开事件处理
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="column">列索引</param>
        /// <param name="surroundingMines">周围地雷数量</param>
        private void OnGridOpened(int row, int column, int surroundingMines)
        {
            if (gameButtons != null && row < gameButtons.GetLength(0) && column < gameButtons.GetLength(1))
            {
                var button = gameButtons[row, column];

                // 移除事件处理程序而不是禁用按钮以启用文本颜色显示
                button.Click -= GameButton_Click;
                button.MouseDown -= GameButton_MouseDown;

                // 把按钮设置为白色, 同时取消按钮的聚焦效果
                button.BackColor = Color.White;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Color.Gray;
                button.FlatAppearance.MouseOverBackColor = Color.White;
                button.FlatAppearance.MouseDownBackColor = Color.White;

                // 设置按钮文本为周围地雷数量, 如果为0则不显示文本
                button.Text = surroundingMines > 0 ? surroundingMines.ToString() : "";

                // 根据周围地雷数量设置文字颜色
                button.ForeColor = surroundingMines switch
                {
                    1 => Color.Blue,
                    2 => Color.Green,
                    3 => Color.Red,
                    4 => Color.Purple,
                    5 => Color.Maroon,
                    6 => Color.Turquoise,
                    7 => Color.Black,
                    8 => Color.Gray,
                    _ => Color.Black
                };
            }
        }

        /// <summary>
        /// 格子标记事件处理
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="column">列索引</param>
        private void OnGridFlagged(int row, int column)
        {
            if (gameButtons != null && row < gameButtons.GetLength(0) && column < gameButtons.GetLength(1))
            {
                var button = gameButtons[row, column];
                button.Click -= GameButton_Click;
                button.BackColor = Color.Yellow;
            }
        }

        /// <summary>
        /// 格子取消标记事件处理
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="column">列索引</param>
        private void OnGridUnflagged(int row, int column)
        {
            if (gameButtons != null && row < gameButtons.GetLength(0) && column < gameButtons.GetLength(1))
            {
                var button = gameButtons[row, column];
                button.Click += GameButton_Click;
                button.BackColor = Color.LightGray;
            }
        }

        /// <summary>
        /// 剩余地雷数变化事件处理
        /// </summary>
        /// <param name="remainingMines">剩余地雷数</param>
        private void OnRemainingMinesChanged(int remainingMines)
        {
            if (minesLeftLabel != null)
            {
                // 确保剩余地雷数不为负数
                var showRemainingMines = remainingMines < 0 ? 0 : remainingMines;
                minesLeftLabel.Text = $"剩余地雷数: {showRemainingMines}";
            }
        }
    }
}