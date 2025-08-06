using AutoUpdaterDotNET;
using System.Text.RegularExpressions;
using MineClearance.UI;
using MineClearance.Models;
using MineClearance.Services;

namespace MineClearance.Utilities;

/// <summary>
/// 方法类, 提供一些常用方法
/// </summary>
public static partial class Methods
{
    [GeneratedRegex(@"<li>(.*?)</li>", RegexOptions.Singleline)]
    private static partial Regex ChangeLogRegex();

    /// <summary>
    /// 是否要强制关闭程序
    /// </summary>
    public static bool IsForceClose { get; set; } = false;

    /// <summary>
    /// 是否需要强制更新
    /// </summary>
    public static bool IsForceUpdate { get; set; } = false;

    /// <summary>
    /// 是否正在处理更新事件
    /// </summary>
    public static bool IsHandlingUpdateEvent { get; set; } = false;

    /// <summary>
    /// 当前是否为第一次检查更新
    /// </summary>
    public static bool IsFirstCheck { get; set; } = true;

    /// <summary>
    /// 取消下载的令牌源
    /// </summary>
    public static CancellationTokenSource CTS { get; private set; } = new CancellationTokenSource();

    /// <summary>
    /// 检测是否为Windows操作系统
    /// </summary>
    /// <returns>如果是Windows操作系统则返回true，否则返回false</returns>
    public static bool IsWindows()
    {
        return Environment.OSVersion.Platform == PlatformID.Win32NT;
    }

    /// <summary>
    /// 根据难度返回对应的文本
    /// </summary>
    /// <param name="difficulty">难度枚举值</param>
    /// <returns>返回对应的难度文本</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果难度未知则抛出异常</exception>
    public static string GetDifficultyText(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => "简单",
            DifficultyLevel.Medium => "普通",
            DifficultyLevel.Hard => "困难",
            DifficultyLevel.Hell => "地狱",
            DifficultyLevel.Custom => "自定义",
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), "未知的难度")
        };
    }

    /// <summary>
    /// 根据当前要排序的游戏结果属性获取对应的优先级
    /// </summary>
    /// <param name="propertyName">要排序的属性名称</param>
    /// <returns>返回对应的优先级</returns>
    /// <exception cref="ArgumentException">如果属性名不支持</exception>
    public static int GetSortPriority(string propertyName)
    {
        return propertyName switch
        {
            "难度" or "Difficulty" => 1,
            "开始时间" or "StartTime" => 4,
            "用时" or "Duration" => 3,
            "结果" or "IsWin" => 0,
            "完成度" or "Completion" => 2,
            _ => throw new ArgumentException($"不支持的属性名: {propertyName}", nameof(propertyName))
        };
    }

    /// <summary>
    /// 根据当前面板类型获取对应底部状态栏状态
    /// </summary>
    /// <param name="panelType">当前面板类型</param>
    /// <returns>返回对应的底部状态栏状态</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果面板类型未知则抛出异常</exception>
    public static StatusBarState GetBottomStatusBarState(PanelType panelType)
    {
        return panelType switch
        {
            PanelType.Menu => StatusBarState.Ready,
            PanelType.GamePrepare => StatusBarState.Preparing,
            PanelType.Game => StatusBarState.InGame,
            PanelType.History => StatusBarState.History,
            _ => throw new ArgumentOutOfRangeException(nameof(panelType), "未知的面板类型")
        };
    }

    /// <summary>
    /// 处理自动更新检查事件
    /// </summary>
    /// <param name="args">更新信息事件参数</param>
    public async static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error == null)
        {
            if (args.IsUpdateAvailable)
            {
                // 获取更新日志内容
                var changelog = await GetLatestUpdateLog(args.ChangelogURL, args.CurrentVersion);

                DialogResult dialogResult;
                if (args.Mandatory.Value)
                {
                    // 如果是强制更新, 则设置强制更新标志
                    IsForceUpdate = true;
                    dialogResult = MessageBox.Show($"新版本 {args.CurrentVersion} 可用, 更新日志: {changelog}\n您当前正在使用版本 {args.InstalledVersion}。这是强制更新。按确定开始更新应用程序。", @"更新可用", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    dialogResult = MessageBox.Show($"新版本 {args.CurrentVersion} 可用, 更新日志: {changelog}\n您当前正在使用版本 {args.InstalledVersion}。你想现在更新应用程序吗？", @"更新可用", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                }

                if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                {
                    // 检测下载的更新文件是否存在
                    if (File.Exists(Constants.SevenZipPath))
                    {
                        // 是否覆盖
                        var overwrite = true;

                        // 如果没有开启隐藏更新提示信息, 提示用户是否覆盖
                        if (!Settings.Config.HideUpdateDetails)
                        {
                            var overwriteResult = MessageBox.Show($"文件 {Constants.SevenZipPath} 已存在, 可能是之前程序尝试自动更新失败导致的残留, 您可以手动将该 7z 压缩文件解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) , 或者您想要覆盖下载更新吗？", @"更新文件已存在", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            overwrite = overwriteResult == DialogResult.Yes;
                        }

                        if (overwrite)
                        {
                            // 用户选择覆盖, 删除旧文件
                            try { File.Delete(Constants.SevenZipPath); } catch { }
                        }
                        else
                        {
                            // 用户选择不覆盖, 取消更新
                            IsHandlingUpdateEvent = false;
                            IsFirstCheck = false;
                            return;
                        }
                    }

                    // 自动下载更新文件
                    var downloadSuccess = await DownloadUpdate(args.DownloadURL);

                    // 如果下载成功, 自动启动更新脚本并退出应用程序
                    if (downloadSuccess)
                    {
                        // 如果没有开启隐藏更新提示信息, 弹窗提示下载完成
                        if (!Settings.Config.HideUpdateDetails)
                        {
                            MessageBox.Show($"更新文件已成功下载到{Constants.SevenZipPath}\n程序将尝试删除 {Constants.CurrentDirectory} 文件夹后使用 {Constants.SevenZipExe} 解压下载的 7z 压缩包并自动更新\n如果自动更新失败, 请手动将下载的 7z 压缩文件包解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换) ", @"下载完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        // 设置强制关闭标志
                        IsForceClose = true;

                        // 创建并启动自动更新的 PowerShell 脚本
                        Script.StartAutoUpdateScript();

                        // 退出应用程序
                        Application.Exit();
                    }
                }
            }
            else if (!IsFirstCheck)
            {
                MessageBox.Show($@"您当前的版本 {args.InstalledVersion} 已经是最新版本, 无需更新。", @"没有可用的更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else if (!IsFirstCheck)
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

        IsHandlingUpdateEvent = false;
        IsFirstCheck = false;
    }

    /// <summary>
    /// 获取最新版本更新日志
    /// </summary>
    /// <param name="changelogURL">更新日志的URL</param>
    /// <param name="currentVersion">当前版本号</param>
    /// <returns>返回最新版本的更新日志内容</returns>
    private static async Task<string> GetLatestUpdateLog(string changelogURL, string currentVersion)
    {
        var changelog = "无法获取更新日志";
        try
        {
            using var client = new HttpClient();
            var html = await client.GetStringAsync(changelogURL);

            // 用正则提取当前版本的日志（假设版本号格式和html结构固定）
            var pattern = $@"<h2>\s*v{Regex.Escape(currentVersion)}.*?</h2>\s*<ul>(.*?)</ul>";
            var match = Regex.Match(html, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // 提取ul中的li内容
                var ulContent = match.Groups[1].Value;
                var liMatches = ChangeLogRegex().Matches(ulContent);
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
    private static async Task<bool> DownloadUpdate(string downloadURL)
    {
        // 更新尝试次数
        var retryCount = 0;

        // 定时器用于定时刷新进度表单
        var timer = new System.Windows.Forms.Timer
        {
            Interval = DownloadConstants.UpdateSpeedRefreshInterval
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
                var result = MessageBox.Show("确定要取消下载吗？", "取消下载", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
        var lastProgressTime = DateTime.Now;
        var isNoProgress = false;

        // 下载进度相关变量
        var canReportProgress = false;
        var lastUpdate = DateTime.Now;
        var buffer = new byte[81920];
        long totalBytes = 0;
        long totalRead = 0;
        long lastRead = 0;
        var speedStr = "";
        var read = 0;

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
            if ((now - lastProgressTime).TotalSeconds > DownloadConstants.NoProgressRetryInterval)
            {
                isNoProgress = true;
                CTS.Cancel();
                timer.Stop();
                return;
            }

            // 时间间隔为0直接返回
            var seconds = (now - lastUpdate).TotalSeconds;
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
        while (retryCount < DownloadConstants.NoProgressMaxRetries)
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
                    Timeout = TimeSpan.FromSeconds(DownloadConstants.HttpRequestTimeout)
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
                    while (isPaused)
                    {
                        CTS.Token.ThrowIfCancellationRequested();
                        await Task.Delay(100, CTS.Token);
                    }
                }

                // 下载完成
                downloadCompleted = true;
                return true;
            }
            catch (TimeoutException tex)
            {
                // http请求超时
                MessageBox.Show($"http请求超时: {tex.Message}", "http请求超时", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (OperationCanceledException)
            {
                // 如果是因为长时间无下载进度而取消
                if (isNoProgress)
                {
                    // 重试次数+1
                    ++retryCount;

                    // 超过最大重试次数
                    if (retryCount >= DownloadConstants.NoProgressMaxRetries)
                    {
                        MessageBox.Show("下载超时, 请检查网络后重试。", "下载超时", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // 是否重试下载
                    var retry = true;

                    // 如果没有开启隐藏更新提示信息, 弹窗提示用户下载超时，是重试还是取消
                    if (!Settings.Config.HideUpdateDetails)
                    {
                        var result = TimeoutMessageBox.Show("下载长时间无进度，是否重试？", "下载超时", DownloadConstants.NoProgressDialogWaitTime);
                        retry = result == DialogResult.Yes;
                    }

                    if (!retry)
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
        var units = new[] { "B/s", "KB/s", "MB/s", "GB/s", "TB/s" };
        var speed = totalBytes / elapsedTime;
        var unitIndex = 0;

        while (speed >= 1024 && unitIndex < units.Length - 1)
        {
            speed /= 1024;
            ++unitIndex;
        }

        return $" (速度: {speed:F2} {units[unitIndex]})";
    }
}