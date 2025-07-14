using System.Text.RegularExpressions;

namespace MineClearance
{
    /// <summary>
    /// 提供一些常用方法
    /// </summary>
    public static partial class Methods
    {
        [GeneratedRegex(@"<li>(.*?)</li>", RegexOptions.Singleline)]
        private static partial Regex MyRegex();

        /// <summary>
        /// 当前是否为第一次检查更新
        /// </summary>
        public static bool IsFirstCheck { get; set; } = true;

        /// <summary>
        /// 线程安全的随机数生成器
        /// </summary>
        public static Random RandomInstance => Random.Shared;

        /// <summary>
        /// 取消下载的令牌源
        /// </summary>
        public static CancellationTokenSource CTS { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// 检测是否为Windows操作系统
        /// </summary>
        /// <returns>如果是Windows操作系统则返回true，否则返回false</returns>
        public static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        /// <summary>
        /// 获取最新版本更新日志
        /// </summary>
        /// <returns>返回最新版本的更新日志内容</returns>
        public static async Task<string> GetLatestUpdateLog(string changelogURL, string currentVersion)
        {
            string changelog = "无法获取更新日志";
            try
            {
                using var client = new HttpClient();
                string html = await client.GetStringAsync(changelogURL);

                // 用正则提取当前版本的日志（假设版本号格式和html结构固定）
                string pattern = $@"<h2>\s*v{Regex.Escape(currentVersion)}.*?</h2>\s*<ul>(.*?)</ul>";
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
            catch { }
            return changelog;
        }

        /// <summary>
        /// 自动下载更新程序
        /// </summary>
        /// <param name="downloadURL">更新文件的下载链接</param>
        /// <returns>如果下载完成则返回true，否则返回false</returns>
        public static async Task<bool> DownloadUpdate(string downloadURL)
        {
            // 更新尝试次数
            int retryCount = 0;

            // 定时器用于定时刷新进度表单
            var timer = new System.Windows.Forms.Timer
            {
                Interval = Constants.UpdateSpeedRefreshInterval
            };

            // 下载进度弹窗
            var progressForm = new DownloadProgressForm();

            // 记录下载是否完成或者暂停
            var downloadCompleted = false;
            var isPaused = false;

            // 处理关闭事件, 如果下载未完成且未取消则提示用户是否取消
            void closingHandler(object? s, FormClosingEventArgs e)
            {
                // 如果下载未完成且未取消则提示用户是否取消
                if (!downloadCompleted && !CTS.IsCancellationRequested)
                {
                    var result = MessageBox.Show("确定要取消下载吗？", "取消下载", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        // 用户选择取消, 取消下载
                        CTS.Cancel();
                    }
                    else
                    {
                        // 用户选择不取消, 则取消关闭事件
                        e.Cancel = true;
                    }
                }
            }

            // 判断下载是否过慢相关变量
            DateTime lastProgressTime = DateTime.Now;
            var isNoProgress = false;

            // 下载进度相关变量
            var canReportProgress = false;
            var lastUpdate = DateTime.Now;
            var buffer = new byte[81920];
            long totalBytes = 0;
            long totalRead = 0;
            long lastRead = 0;
            string speedStr = "";
            int read;

            // 绑定下载进度表单更新事件
            timer.Tick += (s, e) =>
            {
                // 如果下载已暂停
                if (isPaused)
                {
                    lastProgressTime = DateTime.Now;
                    progressForm.ProgressBar.Style = ProgressBarStyle.Blocks;
                    progressForm.StatusLabel.Text = "下载已暂停";
                    progressForm.Refresh();
                    return;
                }

                // 无进度检测
                var now = DateTime.Now;
                if ((now - lastProgressTime).TotalSeconds > Constants.NoProgressRetryInterval)
                {
                    isNoProgress = true;
                    CTS.Cancel();
                    timer.Stop();
                    return;
                }

                // 时间间隔为0直接返回
                double seconds = (now - lastUpdate).TotalSeconds;
                if (seconds == 0)
                {
                    return;
                }

                // 计算下载速度
                speedStr = CalculateDownloadSpeed(totalRead - lastRead, seconds);

                // 更新最后更新时间和读取量
                lastUpdate = now;
                lastRead = totalRead;

                if (!canReportProgress)
                {
                    // 如果不能报告进度
                    progressForm.ProgressBar.Style = ProgressBarStyle.Marquee;
                    progressForm.StatusLabel.Text = $"正在下载更新文件... (进度未知){speedStr}";
                    progressForm.Refresh();
                }
                else
                {
                    // 可以报告进度
                    int percent = (int)(totalRead * 100 / totalBytes);
                    progressForm.ProgressBar.Value = percent;
                    progressForm.ProgressBar.Style = ProgressBarStyle.Continuous;
                    progressForm.StatusLabel.Text = $"已下载 {percent}% ({totalRead / 1024} KB / {totalBytes / 1024} KB){speedStr}";
                    progressForm.Refresh();
                }
            };

            // 下载更新文件
            while (retryCount < Constants.NoProgressMaxRetries)
            {
                // 重置CancellationTokenSource
                CTS = new CancellationTokenSource();

                // 重置下载进度弹窗
                progressForm = new();

                // 绑定暂停/继续按钮事件
                progressForm.PauseResumeButton.Click += (s, e) =>
                {
                    isPaused = !isPaused;
                };
                // 绑定取消按钮事件
                progressForm.CancelButton.Click += (s, e) =>
                {
                    progressForm.Close();
                };
                // 绑定关闭事件
                progressForm.FormClosing += closingHandler;

                // 重置相关变量
                totalRead = 0;
                lastRead = 0;
                isPaused = false;
                isNoProgress = false;

                try
                {
                    // 使用 HttpClient 下载更新文件
                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(Constants.HttpRequestTimeout)
                    };
                    using var response = await httpClient.GetAsync(downloadURL, HttpCompletionOption.ResponseHeadersRead, CTS.Token);
                    response.EnsureSuccessStatusCode();

                    // 判断是否可以报告进度
                    totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    canReportProgress = totalBytes != -1;

                    // 创建文件流以保存下载的文件
                    using var fs = new FileStream(Constants.SevenZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    using var stream = await response.Content.ReadAsStreamAsync(CTS.Token);

                    // 开始下载
                    lastProgressTime = DateTime.Now;

                    // 显示下载进度表单
                    progressForm.Show();

                    // 开始定时器
                    timer.Start();

                    // 循环读取数据并写入文件
                    while ((read = await stream.ReadAsync(buffer, CTS.Token)) > 0)
                    {
                        await fs.WriteAsync(buffer.AsMemory(0, read), CTS.Token);
                        lastProgressTime = DateTime.Now;
                        totalRead += read;
                        retryCount = 0;

                        // 暂停判断
                        while (isPaused && !CTS.IsCancellationRequested)
                        {
                            await Task.Delay(100);
                        }
                    }

                    // 下载完成
                    downloadCompleted = true;
                    return true;
                }
                catch (TimeoutException tex)
                {
                    // http请求超时
                    MessageBox.Show($"http请求超时：{tex.Message}", "http请求超时", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (OperationCanceledException)
                {
                    // 如果是因为长时间无下载进度而取消
                    if (isNoProgress)
                    {
                        // 重试次数+1
                        retryCount++;

                        // 超过最大重试次数
                        if (retryCount >= Constants.NoProgressMaxRetries)
                        {
                            MessageBox.Show("下载超时, 请检查网络后重试。", "下载超时", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        // 弹窗提示用户下载超时，是重试还是取消
                        var result = TimeoutMessageBox.Show("下载长时间无进度，是否重试？", "下载超时", Constants.NoProgressDialogWaitTime);
                        if (result == DialogResult.No)
                        {
                            // 用户选择取消
                            return false;
                        }

                        // 重试下载
                        continue;
                    }

                    // 用户自己手动取消
                    return false;
                }
                catch (Exception ex)
                {
                    // 下载过程中发生错误
                    MessageBox.Show($"下载更新失败：{ex.Message}", "下载错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    // 如果下载未完成，则删除下载的文件
                    if (!downloadCompleted && File.Exists(Constants.SevenZipPath))
                    {
                        try { File.Delete(Constants.SevenZipPath); } catch { }
                    }

                    // 关闭进度表单和定时器
                    progressForm.FormClosing -= closingHandler;
                    progressForm.Close();
                    timer.Stop();
                }
            }

            // 超过最大重试次数返回失败
            return false;
        }

        /// <summary>
        /// 计算下载更新的速度
        /// </summary>
        /// <param name="totalBytes">下载的总字节数</param>
        /// <param name="elapsedTime">下载所用时间(秒)</param>
        /// <returns>下载速度字符串</returns>
        private static string CalculateDownloadSpeed(long totalBytes, double elapsedTime)
        {
            // 计算速度, 先计算B/s
            var speedUnit = "B/s";
            var speed = totalBytes / elapsedTime;

            // 如果速度超过1024B/s, 则转换为KB/s
            if (speed >= 1024)
            {
                speed /= 1024;
                speedUnit = "KB/s";
            }

            // 如果速度超过1024KB/s, 则转换为MB/s
            if (speed >= 1024)
            {
                speed /= 1024;
                speedUnit = "MB/s";
            }

            // 如果速度超过1024MB/s, 则转换为GB/s
            if (speed >= 1024)
            {
                speed /= 1024;
                speedUnit = "GB/s";
            }

            // 返回速度字符串
            return $" (速度: {speed:F2} {speedUnit})";
        }

        /// <summary>
        /// 创建并启动自动更新的powershell脚本
        /// </summary>
        public static void StartAutoUpdateScript()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动更新失败: {ex.Message}\n请手动将 {Constants.SevenZipPath} 解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换)", @"自动更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}