using System.Net;
using System.Text.RegularExpressions;
using AutoUpdaterDotNET;

namespace MineClearance
{
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
        /// 游戏顶部信息栏
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
        /// 要显示的排行榜信息列表
        /// </summary>
        private List<string> showRankingList;

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
            showRankingList = [];

            // 创建菜单面板和游戏准备面板
            CreateMenuPanel();
            CreateGamePreparationPanel();

            // 显示菜单面板
            ShowPanel(menuPanel);

            // 程序启动时检查更新
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Mandatory = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.Start(Constants.AutoUpdateUrl);
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
                    Text = "扫雷游戏 - 历史记录";
            }
        }

        /// <summary>
        /// 创建游戏菜单面板, 包含新游戏、显示排行榜、检查更新和退出按钮
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
                Text = "游戏历史记录",
                Size = new Size(120, 40),
                Location = new Point(540, 160),
                BackColor = Color.LightYellow,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnShowRanking.Click += BtnShowRanking_Click;

            // 添加检查更新按钮
            Button btnCheckUpdate = new()
            {
                Text = "检查更新",
                Size = new Size(120, 40),
                Location = new Point(540, 220),
                BackColor = Color.DarkCyan,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnCheckUpdate.Click += BtnCheckUpdate_Click;

            // 添加退出按钮
            Button btnExit = new()
            {
                Text = "退出",
                Size = new Size(120, 40),
                Location = new Point(540, 280),
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
            menuPanel.Controls.Add(btnCheckUpdate);
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
                BackColor = Color.Yellow,
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
                Text = "历史记录",
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

            // 添加按钮显示统计信息
            Button btnShowStatistics = new()
            {
                Text = "显示统计信息",
                Location = new Point(200, 10),
                AutoSize = true
            };
            btnShowStatistics.Click += BtnShowRanking_Click;
            rankingTopPanel.Controls.Add(btnShowStatistics);

            // 添加按钮以按开始时间排序
            Button btnSortByStartTime = new()
            {
                Text = "按开始时间排序",
                Location = new Point(350, 10),
                AutoSize = true
            };
            btnSortByStartTime.Click += BtnSortByStartTime_Click;
            rankingTopPanel.Controls.Add(btnSortByStartTime);

            // 添加按钮以按用时排序
            Button btnSortByDuration = new()
            {
                Text = "按难度和用时排序",
                Location = new Point(500, 10),
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

            foreach (var showMessage in showRankingList)
            {
                rankingListBox.Items.Add(showMessage);
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
        /// 检查更新按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnCheckUpdate_Click(object? sender, EventArgs e)
        {
            AutoUpdater.Start(Constants.AutoUpdateUrl);
        }

        /// <summary>
        /// 退出按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnExit_Click(object? sender, EventArgs e)
        {
            // 检测有没有更新文件残留
            if (File.Exists(Constants.SevenZipPath))
            {
                var deleteResult = MessageBox.Show($"检测到更新文件 {Constants.SevenZipPath} 残留, 可能是之前程序尝试自动更新失败导致的, 您可以手动将该 7z 压缩文件解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) , 或者您想要将其删除吗？", @"更新文件残留", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (deleteResult == DialogResult.Yes)
                {
                    File.Delete(Constants.SevenZipPath);
                }
            }

            // 删除残留的powershell脚本
            if (File.Exists(Constants.UpdatePowerShellScriptPath))
            {
                File.Delete(Constants.UpdatePowerShellScriptPath);
            }

            // 关闭窗口
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
            showRankingList = [];
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
        /// <param name="displayMode">显示模式</param>
        private void RestartRankingPanel(RankingDisplayMode displayMode = RankingDisplayMode.Default)
        {
            // 根据显示模式获取游戏结果
            var gameResults = Datas.GetSortedGameResults(displayMode);
            showRankingList = [];

            if (gameResults.Count == 0)
            {
                showRankingList.Add("暂无游戏记录");
            }
            else
            {
                switch (displayMode)
                {
                    case RankingDisplayMode.Default:
                        UpdateDefaultRanking(gameResults);
                        break;
                    case RankingDisplayMode.ByStartTime:
                        foreach (var result in gameResults)
                        {
                            showRankingList.Add(result.ToString());
                        }
                        break;
                    case RankingDisplayMode.ByDuration:
                        foreach (var result in gameResults)
                        {
                            showRankingList.Add(result.ToString());
                        }
                        break;
                    default:
                        MessageBox.Show("未知的显示模式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ShowPanel(menuPanel);
                        break;
                }
            }

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
        /// 更新默认排行榜
        /// </summary>
        private void UpdateDefaultRanking(List<GameResult> gameResults)
        {
            // 显示总游戏次数
            showRankingList.Add($"总游戏次数: {gameResults.Count}, 其中:");

            // 统计各难度的游戏次数、胜利次数、总胜利用时、总完成度
            var difficultyStats = new Dictionary<DifficultyLevel, (int total, int wins, TimeSpan totalDuration, double totalCompletion, TimeSpan shortestDuration)>
            {
                { DifficultyLevel.Easy, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
                { DifficultyLevel.Medium, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
                { DifficultyLevel.Hard, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) },
                { DifficultyLevel.Custom, (0, 0, TimeSpan.Zero, 0, TimeSpan.MaxValue) }
            };
            foreach (var result in gameResults)
            {
                var stats = difficultyStats[result.Difficulty];
                ++stats.total;
                stats.wins += result.IsWin ? 1 : 0;
                stats.totalDuration += result.IsWin ? result.Duration : TimeSpan.Zero;
                stats.totalCompletion += result.Completion;
                stats.shortestDuration = result.IsWin && result.Duration < stats.shortestDuration ? result.Duration : stats.shortestDuration;
                difficultyStats[result.Difficulty] = stats;
            }

            // 计算平均胜利率、平均胜利用时和平均完成度
            foreach (var (level, stats) in difficultyStats)
            {
                // 格式化难度名称
                string formattedDifficulty = level switch
                {
                    DifficultyLevel.Easy => "简单",
                    DifficultyLevel.Medium => "普通",
                    DifficultyLevel.Hard => "困难",
                    DifficultyLevel.Custom => "自定义",
                    _ => "未知"
                };

                if (stats.total == 0)
                {
                    showRankingList.Add($"难度: {formattedDifficulty}, 游戏次数: 0");
                    continue;
                }

                double winRate = (double)stats.wins / stats.total;
                TimeSpan avgDuration = stats.wins > 0 ? TimeSpan.FromMilliseconds(stats.totalDuration.TotalMilliseconds / stats.wins) : TimeSpan.Zero;
                double avgCompletion = stats.totalCompletion / stats.total;

                // 格式化用时为 xx:xx.xx 格式
                string formattedDuration = $"{(int)avgDuration.TotalMinutes:D2}:{avgDuration.Seconds:D2}.{avgDuration.Milliseconds / 10:D2}";

                // 完成度格式化为百分比, 保留两位小数
                string formattedCompletion = $"{avgCompletion,6:0.00}%";

                // 格式化最短胜利用时
                string formattedShortestDuration = stats.shortestDuration == TimeSpan.MaxValue ? "无" : $"{(int)stats.shortestDuration.TotalMinutes:D2}:{stats.shortestDuration.Seconds:D2}.{stats.shortestDuration.Milliseconds / 10:D2}";

                // 添加到排行榜列表
                showRankingList.Add($"难度: {formattedDifficulty}, 游戏次数: {stats.total}, 胜利次数: {stats.wins}, 胜利率: {winRate:P2}, 平均胜利用时: {formattedDuration}, 平均完成度: {formattedCompletion}, 最短胜利用时: {formattedShortestDuration}");
            }

            showRankingList.Add("\n");
            showRankingList.Add("点击按开始时间排序按钮可以查看所有游戏记录");
            showRankingList.Add("点击按难度和用时排序按钮可以将所有非自定义难度的胜利游戏记录先按难度高低后按用时快慢进行排序");
        }

        /// <summary>
        /// 清除历史记录按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnClearHistory_Click(object? sender, EventArgs e)
        {
            // 添加确认对话框
            var confirmResult = MessageBox.Show("确定要清除所有历史记录吗？\n注意: 一旦清除将无法找回！！！", "清除历史记录", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            // 用户选择取消，直接返回
            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            // 清空排行榜数据
            Task.Run(Datas.ClearGameResultsAsync);
            showRankingList = ["暂无游戏记录"];

            // 如果排行榜面板已存在，则清理旧数据
            if (rankingPanel != null)
            {
                rankingPanel.Controls.Clear();
                rankingPanel.Dispose();
            }

            // 创建新的排行榜面板
            CreateRankingPanel();
            ShowPanel(rankingPanel);

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
            // 重新加载排行榜面板
            RestartRankingPanel(RankingDisplayMode.ByStartTime);
        }

        /// <summary>
        /// 按用时排序按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnSortByDuration_Click(object? sender, EventArgs e)
        {
            // 重新加载排行榜面板
            RestartRankingPanel(RankingDisplayMode.ByDuration);
        }

        /// <summary>
        /// 处理自动更新检查事件
        /// </summary>
        /// <param name="args"></param>
        private async void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    // 获取更新日志内容
                    string changelog = "";
                    try
                    {
                        using var client = new HttpClient();
                        string html = await client.GetStringAsync(args.ChangelogURL);

                        // 用正则提取当前版本的日志（假设版本号格式和html结构固定）
                        string pattern = $@"<h2>\s*v{Regex.Escape(args.CurrentVersion)}.*?</h2>\s*<ul>(.*?)</ul>";
                        var match = Regex.Match(html, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            // 提取ul中的li内容
                            string ulContent = match.Groups[1].Value;
                            var liMatches = MyRegex().Matches(ulContent);
                            if (liMatches.Count > 0)
                            {
                                changelog = string.Join("\n", liMatches.Select(m => m.Groups[1].Value.Trim()));
                            }
                            else
                            {
                                changelog = "未找到详细更新内容";
                            }
                        }
                        else
                        {
                            changelog = "未找到当前版本的更新日志";
                        }
                    }
                    catch
                    {
                        changelog = "无法获取更新日志";
                    }

                    DialogResult dialogResult;
                    if (args.Mandatory.Value)
                    {
                        dialogResult = MessageBox.Show($"新版本 {args.CurrentVersion} 可用, 更新日志: {changelog}\n您当前正在使用版本 {args.InstalledVersion}。这是强制更新。按确定开始更新应用程序。", @"更新可用", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        dialogResult = MessageBox.Show($"新版本 {args.CurrentVersion} 可用, 更新日志: {changelog}\n您当前正在使用版本 {args.InstalledVersion}。你想现在更新应用程序吗？", @"更新可用", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    }

                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        // 检测下载的更新文件是否存在
                        if (File.Exists(Constants.SevenZipPath))
                        {
                            // 如果文件已存在, 提示用户是否覆盖
                            var overwriteResult = MessageBox.Show($"文件 {Constants.SevenZipPath} 已存在, 可能是之前程序尝试自动更新失败导致的残留, 您可以手动将该 7z 压缩文件解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) , 或者您想要覆盖下载更新吗？", @"更新文件已存在", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (overwriteResult == DialogResult.Yes)
                            {
                                // 用户选择覆盖, 删除旧文件
                                File.Delete(Constants.SevenZipPath);
                            }
                            else
                            {
                                // 用户选择不覆盖, 取消更新
                                Methods.IsFirstCheck = false;
                                return;
                            }
                        }

                        try
                        {
                            // 下载更新文件, 并显示下载进度
                            var progressForm = new DownloadProgressForm();
                            try
                            {
                                // 使用 HttpClient 下载更新文件
                                using var httpClient = new HttpClient();
                                using var response = await httpClient.GetAsync(args.DownloadURL, HttpCompletionOption.ResponseHeadersRead);
                                response.EnsureSuccessStatusCode();

                                // 判断是否可以报告进度
                                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                                var canReportProgress = totalBytes != -1;

                                // 创建文件流以保存下载的文件
                                using var fs = new FileStream(Constants.SevenZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                                using var stream = await response.Content.ReadAsStreamAsync();

                                // 显示下载进度表单
                                progressForm.Show();

                                var buffer = new byte[81920];
                                long totalRead = 0;
                                int read;
                                var lastUpdate = DateTime.Now;
                                long lastRead = 0;
                                double speed = 0;
                                while ((read = await stream.ReadAsync(buffer)) > 0)
                                {
                                    await fs.WriteAsync(buffer.AsMemory(0, read));
                                    totalRead += read;
                                    if (canReportProgress)
                                    {
                                        int percent = (int)(totalRead * 100 / totalBytes);

                                        // 计算速度
                                        var now = DateTime.Now;
                                        double seconds = (now - lastUpdate).TotalSeconds;

                                        // 每0.1秒刷新一次速度
                                        if (seconds > 0.1)
                                        {
                                            // MB/秒
                                            speed = (totalRead - lastRead) / seconds / (1024 * 1024);
                                            lastUpdate = now;
                                            lastRead = totalRead;
                                        }

                                        string speedStr = speed > 0 ? $"，速度 {speed:F1} MB/s" : "";

                                        progressForm.ProgressBar.Value = percent;
                                        progressForm.StatusLabel.Text = $"已下载 {percent}% ({totalRead / 1024} KB / {totalBytes / 1024} KB){speedStr}";
                                        progressForm.Refresh();
                                    }
                                }
                                progressForm.Close();
                            }
                            catch (Exception ex)
                            {
                                progressForm.Close();
                                Methods.IsFirstCheck = false;
                                MessageBox.Show($"下载更新失败：{ex.Message}", "下载错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // 下载完成后, 弹窗提示用户
                            MessageBox.Show($"更新文件已成功下载到{Constants.SevenZipPath}\n程序将尝试删除 {Constants.CurrentDirectory} 文件夹后使用 {Constants.SevenZipExe} 解压下载的 7z 压缩包并自动更新\n如果自动更新失败, 请手动将下载的 7z 压缩文件包解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) ", @"下载完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 创建批处理脚本内容, 使用7za.exe命令解压缩
                            File.WriteAllText(Constants.UpdatePowerShellScriptPath, $@"
                                chcp 65001 > $null
                                [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
                                Get-Process -Name ""{Path.GetFileNameWithoutExtension(Constants.ExecutableFileName)}"" -ErrorAction SilentlyContinue | ForEach-Object {{ $_.Kill() }}
                                Remove-Item -Path ""{Constants.CurrentDirectory}"" -Recurse -Force
                                & ""{Constants.SevenZipExe}"" x -y ""{Constants.SevenZipPath}"" -o""{Constants.ParentDirectory}""
                                Remove-Item ""{Constants.SevenZipPath}""
                                Start-Process ""{Constants.ExecutableFilePath}""
                                Remove-Item -Path $MyInvocation.MyCommand.Path -Force
                                ", System.Text.Encoding.UTF8);

                            // 启动powershell脚本
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "powershell",
                                Arguments = $"-ExecutionPolicy Bypass -File \"{Constants.UpdatePowerShellScriptPath}\"",
                                UseShellExecute = true
                            });

                            // 退出应用程序
                            Application.Exit();
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else if (!Methods.IsFirstCheck)
                {
                    MessageBox.Show($@"您当前的版本 {args.InstalledVersion} 已经是最新版本, 无需更新。", @"没有可用的更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (!Methods.IsFirstCheck)
            {
                if (args.Error is WebException)
                {
                    MessageBox.Show(
                        @"无法连接到更新服务器。请检查您的互联网连接, 然后稍后再试。",
                        @"更新检查失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(args.Error.Message, args.Error.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Methods.IsFirstCheck = false;
        }

        [GeneratedRegex(@"<li>(.*?)</li>", RegexOptions.Singleline)]
        private static partial Regex MyRegex();
    }
}