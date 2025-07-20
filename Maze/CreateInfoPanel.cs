namespace Maze
{
    /// <summary>
    /// 创建信息面板类, 提供创建信息面板的功能
    /// </summary>
    public static class CreateInfoPanel
    {
        /// <summary>
        /// 创建主信息面板
        /// </summary>
        /// <param name="topInfoPanel">顶部信息面板实例</param>
        /// <returns>主信息面板</returns>
        public static Panel CreateMainPanel(TopInfoPanel topInfoPanel)
        {
            // 初始化主信息面板
            var panel = new Panel()
            {
                Size = new(Constants.MainFormWidth, Constants.MainInfoPanelHeight),
                Location = new(0, 0),
                BackColor = Color.DarkCyan
            };

            /// <summary>
            /// 保存当前迷宫按钮
            /// </summary>
            var saveMazeButton = new Button()
            {
                Text = "保存当前迷宫",
                Size = new(120, 30),
                Location = new(10, 5),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            saveMazeButton.Click += topInfoPanel.OnSaveMazeButtonClick;

            // 加载迷宫按钮
            var loadMazeButton = new Button()
            {
                Text = "加载迷宫",
                Size = new(120, 30),
                Location = new(140, 5),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            loadMazeButton.Click += topInfoPanel.OnLoadMazeButtonClick;

            // 设置起点和终点按钮
            var startEndButton = new Button()
            {
                Text = "设置起点和终点",
                Size = new(120, 30),
                Location = new(270, 5),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            startEndButton.Click += topInfoPanel.OnStartEndButtonClick;

            // 设置障碍物按钮
            var setObstacleButton = new Button()
            {
                Text = "设置障碍物",
                Size = new(120, 30),
                Location = new(400, 5),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            setObstacleButton.Click += topInfoPanel.OnObstacleButtonClick;

            // 随机生成迷宫按钮
            var randomMazeButton = new Button()
            {
                Text = "随机生成迷宫",
                Size = new(120, 30),
                Location = new(530, 5),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            randomMazeButton.Click += topInfoPanel.OnGenerateMazeButtonClick;

            // 开始搜索按钮
            var startSearchButton = new Button()
            {
                Text = "开始搜索",
                Size = new(120, 30),
                Location = new(660, 5),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            startSearchButton.Click += topInfoPanel.OnStartSearchButtonClick;

            // 取消当前搜索按钮
            var cancelSearchButton = Methods.CreateCancelSearchButton();
            cancelSearchButton.Click += topInfoPanel.OnCancelSearchButtonClick;

            // 重置迷宫按钮
            var resetMazeButton = new Button()
            {
                Text = "重置迷宫",
                Size = new(120, 30),
                Location = new(Constants.MainFormWidth - 280, 5),
                BackColor = Color.LightYellow,
                FlatStyle = FlatStyle.Flat
            };
            resetMazeButton.Click += topInfoPanel.OnResetButtonClick;

            // 退出按钮
            var exitButton = Methods.CreateExitButton();
            exitButton.Click += (sender, e) => Application.Exit();

            // 将按钮添加到主信息面板并返回
            panel.Controls.Add(saveMazeButton);
            panel.Controls.Add(loadMazeButton);
            panel.Controls.Add(startEndButton);
            panel.Controls.Add(setObstacleButton);
            panel.Controls.Add(randomMazeButton);
            panel.Controls.Add(startSearchButton);
            panel.Controls.Add(cancelSearchButton);
            panel.Controls.Add(resetMazeButton);
            panel.Controls.Add(exitButton);
            return panel;
        }

        /// <summary>
        /// 创建设置起点和终点面板
        /// </summary>
        /// <param name="topInfoPanel">顶部信息面板实例</param>
        /// <returns>设置起点和终点面板</returns>
        public static Panel CreateStartEndPanel(TopInfoPanel topInfoPanel)
        {
            // 初始化设置起点和终点面板
            var panel = new Panel()
            {
                Size = new(Constants.MainFormWidth, Constants.MainInfoPanelHeight),
                Location = new(0, 0),
                BackColor = Color.Cyan
            };

            // 提示信息标签
            var label = new Label()
            {
                Text = "按住鼠标左键设置起点, 按住鼠标右键设置终点",
                Font = new("Arial", 10, FontStyle.Bold),
                Location = new(10, 10),
                ForeColor = Color.Black,
                AutoSize = true
            };

            // 返回按钮
            var backButton = Methods.CreateBackButton();
            backButton.Click += topInfoPanel.OnBackButtonClick;

            // 退出按钮
            var exitButton = Methods.CreateExitButton();
            exitButton.Click += (sender, e) => Application.Exit();

            // 将标签和按钮添加到面板并返回
            panel.Controls.Add(label);
            panel.Controls.Add(backButton);
            panel.Controls.Add(exitButton);
            return panel;
        }

        /// <summary>
        /// 创建设置障碍物面板
        /// </summary>
        /// <param name="topInfoPanel">顶部信息面板实例</param>
        /// <returns>设置障碍物面板</returns>
        public static Panel CreateObstaclePanel(TopInfoPanel topInfoPanel)
        {
            // 初始化设置障碍物面板
            var panel = new Panel()
            {
                Size = new(Constants.MainFormWidth, Constants.MainInfoPanelHeight),
                Location = new(0, 0),
                BackColor = Color.Yellow
            };

            // 提示信息标签
            var label = new Label()
            {
                Text = "按住鼠标左键设置障碍物, 按住鼠标右键取消设置, 支持拖动以设置多个格子, 不能操作起点和终点",
                Font = new("Arial", 10, FontStyle.Bold),
                Location = new(10, 10),
                ForeColor = Color.Black,
                AutoSize = true
            };

            /// <summary>
            /// 全部设为障碍物按钮
            /// </summary>
            var setAllObstaclesButton = new Button()
            {
                Text = "全部设为障碍物",
                Size = new(120, 30),
                Location = new(Constants.MainFormWidth - 560, 5),
                BackColor = Color.LightYellow,
                FlatStyle = FlatStyle.Flat
            };
            setAllObstaclesButton.Click += topInfoPanel.OnSetAllObstaclesButtonClick;

            // 清空障碍物按钮
            var clearObstaclesButton = new Button()
            {
                Text = "清空障碍物",
                Size = new(120, 30),
                Location = new(Constants.MainFormWidth - 420, 5),
                BackColor = Color.YellowGreen,
                FlatStyle = FlatStyle.Flat
            };
            clearObstaclesButton.Click += topInfoPanel.OnClearObstaclesButtonClick;

            // 返回按钮
            var backButton = Methods.CreateBackButton();
            backButton.Click += topInfoPanel.OnBackButtonClick;

            // 退出按钮
            var exitButton = Methods.CreateExitButton();
            exitButton.Click += (sender, e) => Application.Exit();

            // 将标签和按钮添加到面板并返回
            panel.Controls.Add(label);
            panel.Controls.Add(setAllObstaclesButton);
            panel.Controls.Add(clearObstaclesButton);
            panel.Controls.Add(backButton);
            panel.Controls.Add(exitButton);
            return panel;
        }

        /// <summary>
        /// 创建生成迷宫算法选择面板
        /// </summary>
        /// <param name="topInfoPanel">顶部信息面板实例</param>
        /// <returns>生成迷宫算法选择面板</returns>
        public static Panel CreateGenerationAlgorithmPanel(TopInfoPanel topInfoPanel)
        {
            // 初始化生成迷宫算法选择面板
            var panel = new Panel()
            {
                Size = new(Constants.MainFormWidth, Constants.MainInfoPanelHeight),
                Location = new(0, 0),
                BackColor = Color.LightBlue
            };

            /// <summary>
            /// 完全随机生成按钮
            /// </summary>
            var randomButton = new Button()
            {
                Text = "完全随机生成",
                Size = new(120, 30),
                Location = new(10, 5),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            randomButton.Click += topInfoPanel.OnRandomGenerateMazeButtonClick;

            // DFS生成按钮
            var dfsButton = new Button()
            {
                Text = "DFS生成",
                Size = new(120, 30),
                Location = new(140, 5),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            dfsButton.Click += topInfoPanel.OnDfsGenerateMazeButtonClick;

            // 递归分割生成按钮
            var recursiveDivisionButton = new Button()
            {
                Text = "递归分割生成",
                Size = new(120, 30),
                Location = new(270, 5),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            recursiveDivisionButton.Click += topInfoPanel.OnRecursiveDivisionGenerateMazeButtonClick;

            // Prim生成按钮
            var primButton = new Button()
            {
                Text = "Prim生成",
                Size = new(120, 30),
                Location = new(400, 5),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            primButton.Click += topInfoPanel.OnPrimGenerateMazeButtonClick;

            // 提示信息标签
            var label = new Label()
            {
                Text = "程序会先清空当前所有障碍物后再生成迷宫",
                Font = new("Arial", 10, FontStyle.Regular),
                Location = new(600, 10),
                AutoSize = true
            };

            // 返回按钮
            var backButton = Methods.CreateBackButton();
            backButton.Click += topInfoPanel.OnBackButtonClick;

            // 退出按钮
            var exitButton = Methods.CreateExitButton();
            exitButton.Click += (sender, e) => Application.Exit();

            // 将标签和按钮添加到面板并返回
            panel.Controls.Add(randomButton);
            panel.Controls.Add(dfsButton);
            panel.Controls.Add(recursiveDivisionButton);
            panel.Controls.Add(primButton);
            panel.Controls.Add(label);
            panel.Controls.Add(backButton);
            panel.Controls.Add(exitButton);
            return panel;
        }

        /// <summary>
        /// 创建选择搜索算法面板
        /// </summary>
        /// <param name="topInfoPanel">顶部信息面板实例</param>
        /// <returns>选择搜索算法面板</returns>
        public static Panel CreateSearchAlgorithmPanel(TopInfoPanel topInfoPanel)
        {
            // 初始化选择搜索算法面板
            var panel = new Panel()
            {
                Size = new(Constants.MainFormWidth, Constants.MainInfoPanelHeight),
                Location = new(0, 0),
                BackColor = Color.LightBlue
            };

            // BFS搜索按钮
            var bfsButton = new Button()
            {
                Text = "BFS搜索",
                Size = new(120, 30),
                Location = new(10, 5),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            bfsButton.Click += topInfoPanel.OnBfsButtonClick;

            // DFS搜索按钮
            var dfsButton = new Button()
            {
                Text = "DFS搜索",
                Size = new(120, 30),
                Location = new(140, 5),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            dfsButton.Click += topInfoPanel.OnDfsButtonClick;

            // A*搜索按钮
            var aStarButton = new Button()
            {
                Text = "A*搜索",
                Size = new(120, 30),
                Location = new(270, 5),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            aStarButton.Click += topInfoPanel.OnAStarButtonClick;

            // 显示搜索过程复选框
            var showSearchProcessCheckBox = new CheckBox()
            {
                Text = "显示搜索过程(注意:勾选会使搜索变慢)",
                Location = new(450, 10),
                AutoSize = true,
                Checked = false
            };
            showSearchProcessCheckBox.CheckedChanged += topInfoPanel.OnShowSearchProcessCheckedChanged;

            // 查看当前搜索过程按钮
            var viewSearchProcessButton = new Button()
            {
                Text = "查看当前搜索",
                Size = new(120, 30),
                Location = new(Constants.MainFormWidth - 560, 5),
                BackColor = Color.Green,
                FlatStyle = FlatStyle.Flat
            };
            viewSearchProcessButton.Click += topInfoPanel.OnViewSearchProcessButtonClick;

            // 创建取消搜索按钮
            var cancelSearchButton = Methods.CreateCancelSearchButton("取消当前搜索");
            cancelSearchButton.Click += topInfoPanel.OnCancelSearchButtonClick;

            // 返回按钮
            var backButton = Methods.CreateBackButton();
            backButton.Click += topInfoPanel.OnBackButtonClick;

            // 退出按钮
            var exitButton = Methods.CreateExitButton();
            exitButton.Click += (sender, e) => Application.Exit();

            // 将按钮添加到面板并返回
            panel.Controls.Add(bfsButton);
            panel.Controls.Add(dfsButton);
            panel.Controls.Add(aStarButton);
            panel.Controls.Add(showSearchProcessCheckBox);
            panel.Controls.Add(viewSearchProcessButton);
            panel.Controls.Add(cancelSearchButton);
            panel.Controls.Add(backButton);
            panel.Controls.Add(exitButton);
            return panel;
        }

        /// <summary>
        /// 创建正在搜索面板
        /// </summary>
        /// <param name="topInfoPanel">顶部信息面板实例</param>
        /// <returns>正在搜索面板</returns>
        public static Panel CreateSearchingPanel(TopInfoPanel topInfoPanel)
        {
            // 初始化正在搜索面板
            var panel = new Panel()
            {
                Size = new(Constants.MainFormWidth, Constants.MainInfoPanelHeight),
                Location = new(0, 0),
                BackColor = Color.LightGray
            };

            // 提示信息标签
            var label = new Label()
            {
                Text = "正在搜索路径, 绿色为当前正在探索的路径, 浅绿色为探索完的路径...",
                Font = new("Arial", 10, FontStyle.Regular),
                Location = new(10, 10),
                ForeColor = Color.Black,
                AutoSize = true
            };

            // 用时标签
            var timeLabel = new Label()
            {
                Text = "已用时: 0.00 秒",
                Font = new("Arial", 10, FontStyle.Bold),
                Location = new(600, 10),
                ForeColor = Color.Black,
                AutoSize = true
            };

            // 取消当前搜索按钮
            var cancelSearchButton = Methods.CreateCancelSearchButton("取消");
            cancelSearchButton.Click += topInfoPanel.OnCancelSearchButtonClick;

            // 返回按钮
            var backButton = Methods.CreateBackButton();
            backButton.Click += topInfoPanel.OnSearchBackButtonClick;

            // 退出按钮
            var exitButton = Methods.CreateExitButton();
            exitButton.Click += (sender, e) => Application.Exit();

            // 将标签和按钮添加到面板并返回
            panel.Controls.Add(label);
            panel.Controls.Add(timeLabel);
            panel.Controls.Add(cancelSearchButton);
            panel.Controls.Add(backButton);
            panel.Controls.Add(exitButton);

            // 让 TopInfoPanel 能访问到 timeLabel
            topInfoPanel.SearchingTimeLabel = timeLabel;
            return panel;
        }

        /// <summary>
        /// 创建搜索结束面板
        /// </summary>
        /// <param name="topInfoPanel">顶部信息面板实例</param>
        /// <param name="searchResult">搜索结果</param>
        /// <param name="searchTime">搜索耗时</param>
        /// <returns>搜索结束面板</returns>
        public static Panel CreateSearchEndPanel(TopInfoPanel topInfoPanel, SearchResult searchResult, TimeSpan searchTime)
        {
            // 初始化搜索结束面板
            var panel = new Panel()
            {
                Size = new(Constants.MainFormWidth, Constants.MainInfoPanelHeight),
                Location = new(0, 0),
                BackColor = searchResult.IsSuccess ? Color.LightGreen : Color.Coral
            };

            // 格式化耗时为2位小数
            var formattedSearchTime = $"{searchTime.TotalSeconds:F2} 秒";

            // 提示信息标签
            var label = new Label()
            {
                Text = searchResult.IsSuccess ? $"搜索成功, 找到路径, 绿色为找到的路径, 路径长度: {searchResult.PathLength}, 耗时: {formattedSearchTime}" : $"搜索失败, 未找到路径, 浅绿色为探索过的路径, 耗时: {formattedSearchTime}",
                Font = new("Arial", 10, FontStyle.Bold),
                Location = new(10, 10),
                ForeColor = Color.Black,
                AutoSize = true
            };

            // 返回按钮
            var backButton = Methods.CreateBackButton();
            backButton.Click += topInfoPanel.OnSearchEndBackButtonClick;

            // 退出按钮
            var exitButton = Methods.CreateExitButton();
            exitButton.Click += (sender, e) => Application.Exit();

            // 将标签和按钮添加到面板并返回
            panel.Controls.Add(label);
            panel.Controls.Add(backButton);
            panel.Controls.Add(exitButton);
            return panel;
        }
    }
}