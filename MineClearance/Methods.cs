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
        public static async Task<bool> AutoUpdate(string downloadURL)
        {
            // 定时器用于定时刷新进度表单
            var timer = new System.Windows.Forms.Timer
            {
                Interval = Constants.UpdateSpeedRefreshInterval
            };

            // 下载更新文件, 并显示下载进度, 支持暂停和取消
            CTS = new CancellationTokenSource();
            var progressForm = new DownloadProgressForm();
            var downloadCompleted = false;
            var isPaused = false;

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
            progressForm.FormClosing += (s, e) =>
            {
                if (!downloadCompleted && !CTS.IsCancellationRequested)
                {
                    var result = MessageBox.Show("确定要取消下载吗？", "取消下载", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        // 用户选择取消下载, 发送取消请求
                        CTS.Cancel();
                    }
                    else
                    {
                        // 阻止关闭
                        e.Cancel = true;
                    }
                }
            };

            // 下载更新文件
            try
            {
                // 使用 HttpClient 下载更新文件
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(downloadURL, HttpCompletionOption.ResponseHeadersRead, CTS.Token);
                response.EnsureSuccessStatusCode();

                // 判断是否可以报告进度
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                // 创建文件流以保存下载的文件
                using var fs = new FileStream(Constants.SevenZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var stream = await response.Content.ReadAsStreamAsync(CTS.Token);

                // 显示下载进度表单
                progressForm.Show();

                var buffer = new byte[81920];
                int read;
                long totalRead = 0;
                long lastRead = 0;
                var lastUpdate = DateTime.Now;
                double speed = 0;
                string speedStr = "";
                string speedUnit = "";

                // 绑定下载进度表单更新事件
                timer.Tick += (s, e) =>
                {
                    // 如果下载已暂停
                    if (isPaused)
                    {
                        progressForm.ProgressBar.Style = ProgressBarStyle.Blocks;
                        progressForm.StatusLabel.Text = "下载已暂停";
                        progressForm.Refresh();
                        return;
                    }

                    // 时间间隔为0直接返回
                    var now = DateTime.Now;
                    double seconds = (now - lastUpdate).TotalSeconds;
                    if (seconds == 0)
                    {
                        return;
                    }

                    // 计算速度, 先计算B/s
                    speedUnit = "B/s";
                    speed = (totalRead - lastRead) / seconds;

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

                    // 速度字符串
                    speedStr = $" (速度: {speed:F2} {speedUnit})";

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
                        progressForm.StatusLabel.Text = $"已下载 {percent}% ({totalRead / 1024} KB / {totalBytes / 1024} KB){speedStr}";
                        progressForm.Refresh();
                    }
                };
                timer.Start();

                // 循环读取数据并写入文件
                while ((read = await stream.ReadAsync(buffer, CTS.Token)) > 0)
                {
                    // 增加暂停判断
                    while (isPaused && !CTS.IsCancellationRequested)
                    {
                        await Task.Delay(100);
                    }
                    await fs.WriteAsync(buffer.AsMemory(0, read), CTS.Token);
                    totalRead += read;
                }
            }
            catch (OperationCanceledException)
            {
                // 取消下载
                if (File.Exists(Constants.SevenZipPath))
                {
                    try { File.Delete(Constants.SevenZipPath); } catch { }
                }
                return false;
            }
            catch (Exception ex)
            {
                // 下载过程中发生错误
                if (File.Exists(Constants.SevenZipPath))
                {
                    try { File.Delete(Constants.SevenZipPath); } catch { }
                }
                MessageBox.Show($"下载更新失败：{ex.Message}", "下载错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                timer.Stop();
                downloadCompleted = true;
                progressForm.Close();
            }

            // 下载完成后, 弹窗提示用户
            MessageBox.Show($"更新文件已成功下载到{Constants.SevenZipPath}\n程序将尝试删除 {Constants.CurrentDirectory} 文件夹后使用 {Constants.SevenZipExe} 解压下载的 7z 压缩包并自动更新\n如果自动更新失败, 请手动将下载的 7z 压缩文件包解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) ", @"下载完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
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