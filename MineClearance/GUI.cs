namespace MineClearance
{
    public partial class CustomDifficultyDialog : Form
    {
        public (int width, int height, int mineCount) CustomDifficulty { get; private set; }

        private readonly NumericUpDown widthInput;
        private readonly NumericUpDown heightInput;
        private readonly NumericUpDown mineCountInput;
        private readonly Button okButton;
        private readonly Button cancelButton;

        public CustomDifficultyDialog()
        {
            // 初始化控件和布局
            Text = "自定义难度";
            Size = new Size(300, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // 创建控件
            var widthLabel = new Label { Text = "宽度:", Location = new Point(20, 20), Size = new Size(60, 23) };
            widthInput = new NumericUpDown { Location = new Point(90, 20), Size = new Size(100, 23), Minimum = 1, Maximum = 40, Value = 16 };

            var heightLabel = new Label { Text = "高度:", Location = new Point(20, 50), Size = new Size(60, 23) };
            heightInput = new NumericUpDown { Location = new Point(90, 50), Size = new Size(100, 23), Minimum = 1, Maximum = 25, Value = 16 };

            var mineLabel = new Label { Text = "地雷数:", Location = new Point(20, 80), Size = new Size(60, 23) };
            mineCountInput = new NumericUpDown { Location = new Point(90, 80), Size = new Size(100, 23), Minimum = 1, Maximum = 999, Value = 40 };

            okButton = new Button { Text = "确定", Location = new Point(110, 120), Size = new Size(75, 23), DialogResult = DialogResult.OK };
            cancelButton = new Button { Text = "取消", Location = new Point(200, 120), Size = new Size(75, 23), DialogResult = DialogResult.Cancel };

            okButton.Click += OkButton_Click;

            // 添加控件到窗体
            Controls.AddRange([widthLabel, widthInput, heightLabel, heightInput, mineLabel, mineCountInput, okButton, cancelButton]);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            var width = (int)widthInput.Value;
            var height = (int)heightInput.Value;
            var mineCount = (int)mineCountInput.Value;

            // 验证地雷数不能超过总格子数
            if (mineCount >= width * height)
            {
                MessageBox.Show("地雷数必须小于总格子数！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            CustomDifficulty = (width, height, mineCount);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>
    /// 提供游戏的图形用户界面
    /// </summary>
    public partial class GUI : Form
    {
        /// <summary>
        /// 菜单面板, 包含新游戏、显示排行榜和退出按钮
        /// </summary>
        private Panel? menuPanel;
        /// <summary>
        /// 游戏准备面板, 选择难度或者自定义
        /// </summary>
        private Panel? gamePreparationPanel;
        /// <summary>
        /// 游戏面板, 显示游戏
        /// </summary>
        private Panel? gamePanel;
        /// <summary>
        /// 排行榜面板, 显示游戏历史记录
        /// </summary>  
        private Panel? rankingPanel;
        /// <summary>
        /// 顶部信息栏
        /// </summary>
        private Panel? infoPanel;
        /// <summary>
        /// 游戏区域面板, 显示游戏格子
        /// </summary>
        private Panel? gameAreaPanel;
        /// <summary>
        /// 剩余地雷数标签
        /// </summary>
        private Label? minesLeftLabel;
        /// <summary>
        /// 游戏时间标签
        /// </summary>
        private Label? gameTimeLabel;
        /// <summary>
        /// 游戏计时器
        /// </summary>
        private System.Windows.Forms.Timer? gameTimer;
        /// <summary>
        /// 游戏开始时间
        /// </summary>
        private DateTime gameStartTime;
        /// <summary>
        /// 游戏格子按钮
        /// </summary>
        private Button[,]? gameButtons;
        /// <summary>
        /// 游戏实例, 控制游戏逻辑
        /// </summary>
        private Game? gameInstance;
        /// <summary>
        /// 排行榜顶部面板
        /// </summary>
        private Panel? rankingTopPanel;
        /// <summary>
        /// 排行榜列表框
        /// </summary>
        private ListBox? rankingListBox;
        /// <summary>
        /// 游戏结果列表
        /// </summary>
        private List<GameResult>? gameResults;

        /// <summary>
        /// 构造函数, 初始化GUI
        /// </summary>
        public GUI()
        {
            // 设置窗口属性
            Text = "扫雷游戏";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.LightBlue;

            // 创建所有面板
            CreateMenuPanel();
            CreateGamePreparationPanel();

            ShowPanel(menuPanel);
        }

        /// <summary>
        /// 切换到指定面板
        /// </summary>
        /// <param name="targetPanel">目标面板</param>
        private void ShowPanel(Panel? targetPanel)
        {
            // 隐藏所有面板
            if (menuPanel != null) menuPanel.Visible = false;
            if (gamePreparationPanel != null) gamePreparationPanel.Visible = false;
            if (gamePanel != null) gamePanel.Visible = false;
            if (rankingPanel != null) rankingPanel.Visible = false;

            // 显示目标面板
            if (targetPanel != null)
            {
                targetPanel.Visible = true;
                targetPanel.BringToFront();

                // 根据显示的面板更新窗口标题
                if (targetPanel == menuPanel)
                    Text = "扫雷游戏";
                else if (targetPanel == gamePreparationPanel)
                    Text = "扫雷游戏 - 准备游戏";
                else if (targetPanel == gamePanel)
                    Text = "扫雷游戏 - 游戏中";
                else if (targetPanel == rankingPanel)
                    Text = "扫雷游戏 - 排行榜";
            }
        }

        /// <summary>
        /// 创建游戏菜单面板, 包含新游戏、显示排行榜和退出按钮
        /// </summary>
        private void CreateMenuPanel()
        {
            menuPanel = new()
            {
                Name = "MenuPanel",
                Size = ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.LightBlue,
                Dock = DockStyle.Fill
            };

            // 添加新游戏按钮
            Button btnNewGame = new()
            {
                Text = "新游戏",
                Size = new Size(120, 40),
                Location = new Point(540, 100),
                BackColor = Color.LightGreen,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnNewGame.Click += BtnNewGame_Click;

            // 添加显示排行榜按钮
            Button btnShowRanking = new()
            {
                Text = "显示排行榜",
                Size = new Size(120, 40),
                Location = new Point(540, 160),
                BackColor = Color.LightYellow,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnShowRanking.Click += BtnShowRanking_Click;

            // 添加退出按钮
            Button btnExit = new()
            {
                Text = "退出",
                Size = new Size(120, 40),
                Location = new Point(540, 220),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExit.Click += BtnExit_Click;

            // 添加标题标签
            Label titleLabel = new()
            {
                Text = "扫雷游戏",
                Font = new Font("Microsoft YaHei", 24, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                BackColor = Color.Transparent,
                Size = new Size(300, 50),
                Location = new Point(450, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 添加控件到菜单面板
            menuPanel.Controls.Add(btnNewGame);
            menuPanel.Controls.Add(btnShowRanking);
            menuPanel.Controls.Add(btnExit);
            menuPanel.Controls.Add(titleLabel);

            // 将菜单面板添加到窗体
            Controls.Add(menuPanel);
        }

        /// <summary>
        /// 创建游戏准备面板, 选择难度或者自定义
        /// </summary>
        private void CreateGamePreparationPanel()
        {
            gamePreparationPanel = new()
            {
                Name = "GamePreparationPanel",
                Size = ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.LightYellow,
                Dock = DockStyle.Fill
            };

            // 添加简单按钮
            Button btnEasy = new()
            {
                Text = "简单",
                Size = new Size(120, 40),
                Location = new Point(540, 100),
                BackColor = Color.LightGreen,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnEasy.Click += BtnEasy_Click;

            // 添加普通按钮
            Button btnMedium = new()
            {
                Text = "普通",
                Size = new Size(120, 40),
                Location = new Point(540, 160),
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnMedium.Click += BtnMedium_Click;

            // 添加困难按钮
            Button btnHard = new()
            {
                Text = "困难",
                Size = new Size(120, 40),
                Location = new Point(540, 220),
                BackColor = Color.DarkRed,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnHard.Click += BtnHard_Click;

            // 添加自定义按钮
            Button btnCustom = new()
            {
                Text = "自定义",
                Size = new Size(120, 40),
                Location = new Point(540, 280),
                BackColor = Color.LightYellow,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnCustom.Click += BtnCustom_Click;

            // 添加返回菜单按钮
            Button btnBackMenu = new()
            {
                Text = "返回",
                Size = new Size(120, 40),
                Location = new Point(540, 340),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBackMenu.Click += BtnBackMenu_Click;

            // 添加标题标签
            Label titleLabel = new()
            {
                Text = "扫雷游戏",
                Font = new Font("Microsoft YaHei", 24, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                BackColor = Color.Transparent,
                Size = new Size(300, 50),
                Location = new Point(450, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 添加控件到游戏准备面板
            gamePreparationPanel.Controls.Add(btnEasy);
            gamePreparationPanel.Controls.Add(btnMedium);
            gamePreparationPanel.Controls.Add(btnHard);
            gamePreparationPanel.Controls.Add(btnCustom);
            gamePreparationPanel.Controls.Add(btnBackMenu);
            gamePreparationPanel.Controls.Add(titleLabel);

            // 将游戏准备面板添加到窗体
            Controls.Add(gamePreparationPanel);
        }

        /// <summary>
        /// 创建游戏面板, 包含游戏区域和操作按钮
        /// </summary>
        private void CreateGamePanel()
        {
            gamePanel = new()
            {
                Name = "GamePanel",
                Size = ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.LightGreen,
                Dock = DockStyle.Fill
            };

            CreateInfoPanel();
            CreateGameAreaPanel();

            gamePanel.Controls.Add(infoPanel);
            gamePanel.Controls.Add(gameAreaPanel);

            // 将游戏面板添加到窗体
            Controls.Add(gamePanel);
        }

        /// <summary>
        /// 创建信息面板, 显示游戏状态信息
        /// </summary>
        private void CreateInfoPanel()
        {
            infoPanel = new()
            {
                Size = new Size(ClientSize.Width, 40),
                Location = new Point(0, 0),
                BackColor = Color.LightGray,
                Dock = DockStyle.Top
            };

            // 添加剩下的地雷数标签
            minesLeftLabel = new()
            {
                Text = $"剩余地雷数: {gameInstance?.TotalMines}",
                Location = new Point(200, 10),
                AutoSize = true
            };

            // 添加游戏时间标签
            gameTimeLabel = new()
            {
                Text = "游戏时间: 00:00",
                Location = new Point(300, 10),
                AutoSize = true
            };

            // 添加返回菜单按钮
            Button btnBackMenu = new()
            {
                Text = "返回菜单",
                Location = new Point(10, 10),
                AutoSize = true
            };
            btnBackMenu.Click += BtnBackMenu_Click;

            // 添加重新开始按钮
            Button btnRestart = new()
            {
                Text = "重新开始",
                Location = new Point(100, 10),
                AutoSize = true
            };
            btnRestart.Click += BtnRestart_Click;

            // 添加提示信息
            Label hintLabel = new()
            {
                Text = "提示: 左键打开格子, 右键标记地雷（在打开一个格子之前无效）, 灰色格子为未打开, 黄色格子表示插旗",
                Location = new Point(500, 10),
                AutoSize = true
            };

            infoPanel.Controls.Add(minesLeftLabel);
            infoPanel.Controls.Add(gameTimeLabel);
            infoPanel.Controls.Add(btnBackMenu);
            infoPanel.Controls.Add(btnRestart);
            infoPanel.Controls.Add(hintLabel);
        }

        /// <summary>
        /// 创建游戏区域面板, 显示游戏格子
        /// </summary>
        private void CreateGameAreaPanel()
        {
            gameAreaPanel = new()
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill
            };

            // 如果有游戏实例，则根据游戏尺寸初始化按钮
            if (gameInstance != null)
            {
                int width = gameInstance.Board.Width;
                int height = gameInstance.Board.Height;
                gameButtons = new Button[height, width];

                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        gameButtons[row, col] = new Button
                        {
                            Size = new Size(Constants.buttonSize, Constants.buttonSize),
                            Location = new Point(col * Constants.buttonSize + 10, row * Constants.buttonSize + 50),
                            Text = "",
                            BackColor = Color.LightGray,
                            FlatStyle = FlatStyle.Flat,
                            Tag = (row, col),
                            TabStop = false,
                            FlatAppearance = { BorderSize = 1, BorderColor = Color.Gray }
                        };


                        // 添加左键点击事件
                        gameButtons[row, col].Click += GameButton_Click;

                        // 添加右键点击事件（标记地雷）
                        gameButtons[row, col].MouseDown += GameButton_MouseDown;

                        gameAreaPanel.Controls.Add(gameButtons[row, col]);
                    }
                }
            }
        }

        /// <summary>
        /// 创建排行榜面板, 显示游戏历史记录
        /// </summary>
        private void CreateRankingPanel()
        {
            rankingPanel = new()
            {
                Name = "RankingPanel",
                Size = ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.LightCoral,
                Dock = DockStyle.Fill
            };

            // 创建排行榜顶部面板
            CreateTopRankingPanel();
            // 创建排行榜列表框
            CreateRankingListBox();

            // 添加排行榜顶部面板和列表框到排行榜面板
            rankingPanel.Controls.Add(rankingTopPanel);
            rankingPanel.Controls.Add(rankingListBox);

            // 将排行榜面板添加到窗体
            Controls.Add(rankingPanel);
        }

        /// <summary>
        /// 创建排行榜顶部面板
        /// </summary>
        private void CreateTopRankingPanel()
        {
            rankingTopPanel = new()
            {
                Name = "RankingTopPanel",
                Size = new Size(ClientSize.Width, 50),
                BackColor = Color.LightSalmon
            };

            Label titleLabel = new()
            {
                Text = "排行榜",
                Font = new Font("Arial", 24, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            rankingTopPanel.Controls.Add(titleLabel);

            Button btnBackMenu = new()
            {
                Text = "返回菜单",
                Location = new Point(ClientSize.Width - 100, 10),
                AutoSize = true
            };
            btnBackMenu.Click += BtnBackMenu_Click;
            rankingTopPanel.Controls.Add(btnBackMenu);

            // 添加按钮以清除历史记录
            Button btnClearHistory = new()
            {
                Text = "清除历史",
                Location = new Point(ClientSize.Width - 250, 10),
                AutoSize = true
            };
            btnClearHistory.Click += BtnClearHistory_Click;
            rankingTopPanel.Controls.Add(btnClearHistory);

            // 添加按钮以按开始时间排序
            Button btnSortByStartTime = new()
            {
                Text = "按开始时间排序",
                Location = new Point(200, 10),
                AutoSize = true
            };
            btnSortByStartTime.Click += BtnSortByStartTime_Click;
            rankingTopPanel.Controls.Add(btnSortByStartTime);

            // 添加按钮以按用时排序
            Button btnSortByDuration = new()
            {
                Text = "按难度和用时排序",
                Location = new Point(350, 10),
                AutoSize = true
            };
            btnSortByDuration.Click += BtnSortByDuration_Click;
            rankingTopPanel.Controls.Add(btnSortByDuration);
        }

        /// <summary>
        /// 创建排行榜列表框
        /// </summary>
        private void CreateRankingListBox()
        {
            rankingListBox = new()
            {
                Name = "RankingListBox",
                Location = new Point(0, 60),
                Size = new Size(ClientSize.Width, ClientSize.Height - 60),
                BackColor = Color.White
            };

            // 如果游戏结果列表未初始化，则从数据源获取非自定义难度的游戏结果
            gameResults ??= Datas.GetNonCustomDifficultyGameResults();

            // 如果游戏结果列表为空，则显示提示信息
            if (gameResults.Count == 0)
            {
                rankingListBox.Items.Add("暂无游戏记录");
                return;
            }

            // 否则，遍历游戏结果列表并添加到列表框
            foreach (var result in gameResults)
            {
                rankingListBox.Items.Add(result.ToString());
            }
        }

        /// <summary>
        /// 新游戏按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnNewGame_Click(object? sender, EventArgs e)
        {
            // 开始新游戏的逻辑
            ShowPanel(gamePreparationPanel);
        }

        /// <summary>
        /// 显示排行榜按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnShowRanking_Click(object? sender, EventArgs e)
        {
            RestartRankingPanel();
        }

        /// <summary>
        /// 退出按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnExit_Click(object? sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 简单按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnEasy_Click(object? sender, EventArgs e)
        {
            // 开始简单难度游戏的逻辑
            gameInstance = new Game(DifficultyLevel.Easy);
            StartGame();
        }

        /// <summary>
        /// 普通按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnMedium_Click(object? sender, EventArgs e)
        {
            // 开始普通难度游戏的逻辑
            gameInstance = new Game(DifficultyLevel.Medium);
            StartGame();
        }

        /// <summary>
        /// 困难按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnHard_Click(object? sender, EventArgs e)
        {
            // 开始困难难度游戏的逻辑
            gameInstance = new Game(DifficultyLevel.Hard);
            StartGame();
        }

        /// <summary>
        /// 自定义按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnCustom_Click(object? sender, EventArgs e)
        {
            // 开始自定义难度游戏的逻辑(弹出对话框获取自定义参数)
            using var dialog = new CustomDifficultyDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var (width, height, mineCount) = dialog.CustomDifficulty;
                try
                {
                    gameInstance = new Game(width, height, mineCount);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    MessageBox.Show($"参数错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"参数错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                StartGame();
            }
        }

        /// <summary>
        /// 返回菜单按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnBackMenu_Click(object? sender, EventArgs e)
        {
            EndGame();
            rankingPanel?.Dispose();
            rankingPanel = null;
            gameResults = null;
            ShowPanel(menuPanel);
        }

        /// <summary>
        /// 重新开始按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnRestart_Click(object? sender, EventArgs e)
        {
            // 获取当前游戏难度、高度、宽度和地雷数
            DifficultyLevel difficulty = gameInstance?.Difficulty ?? DifficultyLevel.Easy;
            int width = gameInstance?.Board.Width ?? 10;
            int height = gameInstance?.Board.Height ?? 10;
            int mineCount = gameInstance?.TotalMines ?? 10;

            // 结束当前游戏
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
        /// 启动游戏, 自动切换到游戏面板或者返回菜单
        /// </summary>
        public void StartGame()
        {
            // 如果游戏实例已存在，则重新创建游戏区域
            if (gameInstance != null)
            {
                // 先移除旧的游戏面板
                if (gamePanel != null)
                {
                    Controls.Remove(gamePanel);
                    gamePanel.Dispose();
                }

                // 创建新的游戏面板
                CreateGamePanel();
                ShowPanel(gamePanel);

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
        public void EndGame()
        {
            StopGameTimer();

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
        /// 处理游戏格子按钮的左键点击事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void GameButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button button && gameInstance != null)
            {
                int row, col;
                if (button.Tag is (int r, int c))
                {
                    row = r;
                    col = c;
                }
                else
                {
                    // 如果按钮的Tag值无效, 不做任何处理
                    return;
                }

                // 处理游戏逻辑(左键点击)
                gameInstance.Board.OnGridClick(row, col);
            }
        }

        /// <summary>
        /// 处理游戏格子按钮的右键点击事件(标记地雷)
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void GameButton_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && sender is Button button && gameInstance != null)
            {
                int row, col;
                if (button.Tag is (int r, int c))
                {
                    row = r;
                    col = c;
                }
                else
                {
                    // 如果按钮的Tag值无效, 不做任何处理
                    return;
                }

                // 处理游戏逻辑(右键点击)
                gameInstance.Board.OnGridClick(row, col, true);
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
            MessageBox.Show("恭喜你，赢得了游戏！", "游戏胜利", MessageBoxButtons.OK, MessageBoxIcon.Information);
            EndGame();

            // 返回菜单面板
            ShowPanel(menuPanel);
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

            // 弹出游戏失败提示
            MessageBox.Show("很遗憾，你踩到了地雷！\n红色格子表示未标记的地雷\n绿色格子表示正确标记的地雷\n黄色格子表示错误标记的地雷", "游戏失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            EndGame();

            // 返回菜单面板
            ShowPanel(menuPanel);
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

        /// <summary>
        /// 重启排行榜面板
        /// </summary>
        private void RestartRankingPanel()
        {
            // 如果排行榜面板已存在，则清理旧数据
            if (rankingPanel != null)
            {
                rankingPanel.Controls.Clear();
                rankingPanel.Dispose();
            }

            // 创建新的排行榜面板
            CreateRankingPanel();
            ShowPanel(rankingPanel);
        }

        /// <summary>
        /// 清除历史记录按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnClearHistory_Click(object? sender, EventArgs e)
        {
            // 添加确认对话框
            var confirmResult = MessageBox.Show("确定要清除所有历史记录吗？", "清除历史记录", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            // 用户选择取消，直接返回
            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            // 清空排行榜数据
            Task.Run(Datas.ClearGameResultsAsync);
            gameResults = null;

            // 重新加载排行榜面板
            RestartRankingPanel();

            // 弹窗提示清除成功
            MessageBox.Show("历史记录已清除！", "清除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 按开始时间排序按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnSortByStartTime_Click(object? sender, EventArgs e)
        {
            // 按开始时间排序排行榜数据
            gameResults = Datas.GetSortedNonCustomDifficultyGameResults();

            // 重新加载排行榜面板
            RestartRankingPanel();
        }

        /// <summary>
        /// 按用时排序按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnSortByDuration_Click(object? sender, EventArgs e)
        {
            // 按用时排序排行榜数据
            gameResults = Datas.GetSortedNonCustomDifficultyGameResults(false);

            // 重新加载排行榜面板
            RestartRankingPanel();

            // 弹窗提示
            MessageBox.Show("排行榜已按难度和用时排序\n注意: 本模式只会排序胜利的游戏结果", "排序成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}