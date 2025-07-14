using AutoUpdaterDotNET;

namespace MineClearance
{
    /// <summary>
    /// 提供游戏的图形用户界面
    /// </summary>
    public partial class GUI : Form
    {
        /// <summary>
        /// 构造函数, 初始化GUI
        /// </summary>
        public GUI()
        {
            // 绑定未捕获异常事件
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // 设置窗口属性
            Text = "扫雷游戏";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.LightBlue;
            showRankingList = [];

            // 绑定关闭事件
            FormClosing += GUI_FormClosing;

            // 创建菜单面板和游戏准备面板
            CreateMenuPanel();
            CreateGamePreparationPanel();

            // 显示菜单面板
            ShowPanel(menuPanel);

            // 程序启动时检查更新
            isHandlingUpdateEvent = true;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Mandatory = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.Start(Constants.AutoUpdateUrl);
        }

        /// <summary>
        /// 记录异常到日志文件
        /// </summary>
        /// <param name="ex">要记录的异常</param>
        private static async Task LogException(Exception ex)
        {
            string log = $"[{DateTime.Now}] {ex}\n";
            try
            {
                await File.AppendAllTextAsync(Constants.ErrorFilePath, log);
            }
            catch { /* 忽略日志写入异常 */ }
        }

        /// <summary>
        /// 处理未处理的线程异常
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">线程异常事件参数</param>
        private void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // 取消下载
            Methods.CTS.Cancel();

            // 取消关闭事件的绑定
            FormClosing -= GUI_FormClosing;

            // 记录异常到日志文件并弹窗提示错误信息
            var logTask = LogException(e.Exception);
            MessageBox.Show($"发生未处理的线程异常：{e.Exception.Message}\n错误日志见 {Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // 确认日志写入完成后退出应用程序
            logTask.GetAwaiter().GetResult();
            Application.Exit();
        }

        /// <summary>
        /// 处理未处理的应用程序异常
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">未处理异常事件参数</param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 取消下载
            Methods.CTS.Cancel();

            // 取消关闭事件的绑定
            FormClosing -= GUI_FormClosing;

            // 记录异常到日志文件并弹窗提示错误信息
            Task logTask;
            if (e.ExceptionObject is Exception ex)
            {
                logTask = LogException(ex);
                MessageBox.Show($"发生未处理的应用程序异常：{ex.Message}\n错误日志见 {Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                logTask = LogException(new Exception("发生未知的未处理异常。"));
                MessageBox.Show($"发生未知的未处理异常\n错误日志见 {Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 确认日志写入完成后退出应用程序
            logTask.GetAwaiter().GetResult();
            Application.Exit();
        }

        /// <summary>
        /// 关闭程序事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GUI_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 如果正在处理更新事件, 向用户确认是否要强制关闭
            if (isHandlingUpdateEvent)
            {
                var result = MessageBox.Show("正在处理更新事件, 确定要强制关闭程序吗？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // 如果用户不选择强制关闭, 则取消关闭事件
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                // 如果用户选择强制关闭, 则取消更新事件处理
                Methods.CTS.Cancel();

                // 弹窗提示正在等待
                using var waitingForm = new WaitingForm();
                waitingForm.Show();

                // 等待处理更新事件完成
                while (isHandlingUpdateEvent)
                {
                    Application.DoEvents();

                    // 适当Sleep减少CPU占用
                    Thread.Sleep(50);
                }
                waitingForm.Close();
            }

            // 检测有没有更新文件残留
            if (File.Exists(Constants.SevenZipPath))
            {
                var deleteResult = MessageBox.Show($"检测到更新文件 {Constants.SevenZipPath} 残留, 可能是之前程序尝试自动更新失败导致的, 您可以手动将该 7z 压缩文件解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) , 或者您想要将其删除吗？", @"更新文件残留", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                try
                {
                    if (deleteResult == DialogResult.Yes)
                    {
                        File.Delete(Constants.SevenZipPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除更新文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // 删除残留的powershell脚本
            if (File.Exists(Constants.UpdatePowerShellScriptPath))
            {
                try { File.Delete(Constants.UpdatePowerShellScriptPath); } catch { }
            }
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
                    string changelog = await Methods.GetLatestUpdateLog(args.ChangelogURL, args.CurrentVersion);

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
                                try { File.Delete(Constants.SevenZipPath); } catch { }
                            }
                            else
                            {
                                // 用户选择不覆盖, 取消更新
                                isHandlingUpdateEvent = false;
                                Methods.IsFirstCheck = false;
                                return;
                            }
                        }

                        // 自动下载更新文件
                        bool downloadSuccess = await Methods.DownloadUpdate(args.DownloadURL);

                        // 如果下载成功, 自动启动更新脚本并退出应用程序
                        if (downloadSuccess)
                        {
                            // 弹窗提示下载完成
                            MessageBox.Show($"更新文件已成功下载到{Constants.SevenZipPath}\n程序将尝试删除 {Constants.CurrentDirectory} 文件夹后使用 {Constants.SevenZipExe} 解压下载的 7z 压缩包并自动更新\n如果自动更新失败, 请手动将下载的 7z 压缩文件包解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) ", @"下载完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 取消关闭事件的绑定
                            FormClosing -= GUI_FormClosing;

                            // 创建并启动自动更新的 PowerShell 脚本
                            Methods.StartAutoUpdateScript();

                            // 退出应用程序
                            Application.Exit();
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
                if (args.Error is System.Net.WebException)
                {
                    MessageBox.Show(@"无法连接到更新服务器。请检查您的互联网连接, 然后稍后再试。", @"更新检查失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(args.Error.Message, args.Error.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            isHandlingUpdateEvent = false;
            Methods.IsFirstCheck = false;
        }
    }
}