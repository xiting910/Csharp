using AutoUpdaterDotNET;

namespace MineClearance
{
    public partial class GUI
    {
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
            StopGameTimer();
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
        /// <exception cref="Exception">如果当前游戏实例为空</exception>
        private void BtnRestart_Click(object? sender, EventArgs e)
        {
            // 如果当前游戏实例为空, 抛出异常
            if (gameInstance == null)
            {
                throw new Exception("当前游戏实例为空");
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
    }
}