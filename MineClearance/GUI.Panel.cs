namespace MineClearance
{
    public partial class GUI
    {
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
    }
}