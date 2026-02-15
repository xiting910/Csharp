using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CM = ServerManager.ConfigurationManager;

namespace ServerManager;

/// <summary>
/// 服务器核心类, 提供服务器管理和启动等核心功能
/// </summary>
public static partial class ServerCore
{
    /// <summary>
    /// 匹配带引号的 Java 版本号
    /// </summary>
    [GeneratedRegex("version \"(.*?)\"")]
    private static partial Regex VersionRegex();

    /// <summary>
    /// 匹配无引号的 Java 版本号
    /// </summary>
    [GeneratedRegex("(?:java|openjdk)\\s+(\\d+(\\.\\d+)*)")]
    private static partial Regex NoQuotationRegex();

    /// <summary>
    /// 匹配正在输出模组列表信息
    /// </summary>
    [GeneratedRegex(".*Loading (\\d+) mods:")]
    private static partial Regex OutputsModsStartRegex();

    /// <summary>
    /// 匹配中文字符
    /// </summary>
    [GeneratedRegex(@"[\u4e00-\u9fff]")]
    private static partial Regex ChineseRegex();

    /// <summary>
    /// 匹配服务器在线玩家数量
    /// </summary>
    [GeneratedRegex(".*There are (\\d+) of a max of (\\d+) players online:\\s*(.*)")]
    private static partial Regex PlayerNumRegex();

    /// <summary>
    /// 服务器是否已经启动
    /// </summary>
    private static volatile bool _serverStarted;

    /// <summary>
    /// 服务器是否正在输出模组列表
    /// </summary>
    private static volatile bool _isOutputtingModList;

    /// <summary>
    /// 最后一名玩家离开时间功能锁
    /// </summary>
    private static readonly Lock _lastPlayerLeaveTimeLock = new();

    /// <summary>
    /// 最后一名玩家离开时间
    /// </summary>
    private static DateTime? LastPlayerLeaveTime
    {
        get { lock (_lastPlayerLeaveTimeLock) { return field; } }
        set { lock (_lastPlayerLeaveTimeLock) { field = value; } }
    }

    /// <summary>
    /// 最后一次指令执行时间功能锁
    /// </summary>
    private static readonly Lock _lastCommandExecutionTimeLock = new();

    /// <summary>
    /// 最后一次指令执行的时间
    /// </summary>
    private static DateTime? LastCommandExecutionTime
    {
        get { lock (_lastCommandExecutionTimeLock) { return field; } }
        set { lock (_lastCommandExecutionTimeLock) { field = value; } }
    }

    /// <summary>
    /// Mod列表
    /// </summary>
    private static readonly ConcurrentBag<string> _modList = [];

    /// <summary>
    /// 服务器玩家数量
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">当设置的玩家数量为负数时抛出</exception>
    public static int PlayerCount
    {
        get => field;
        private set
        {
            // 确保玩家数量不为负数
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(PlayerCount));

            // 设置玩家数量
            _ = Interlocked.Exchange(ref field, value);

            // 如果玩家数量变为 0, 记录最后玩家离开时间; 否则清除记录
            if (value == 0)
            {
                // 记录最后玩家离开时间
                LastPlayerLeaveTime = DateTime.Now;

                // 如果自动关闭功能启用, 输出提示信息
                if (CM.Settings.AutoCloseServer)
                {
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Yellow);
                    IOMethods.WriteColorMessage("服务器无玩家活动, 将在", ConsoleColor.Yellow, false);
                    IOMethods.WriteColorMessage($" {CM.Settings.WaitTime.TotalMinutes} ", ConsoleColor.DarkMagenta, false);
                    IOMethods.WriteColorMessage("分钟后自动关闭", ConsoleColor.Yellow);
                }
            }
            else
            {
                // 清除最后玩家离开时间记录
                LastPlayerLeaveTime = null;
            }
        }
    }

    /// <summary>
    /// 检测 Java 环境
    /// </summary>
    /// <returns>Java 是否存在</returns>
    public static bool CheckJavaEnvironment()
    {
        IOMethods.WriteCurrentTimestamp(ConsoleColor.Yellow);
        IOMethods.WriteColorMessage("正在检测 Java 环境...", ConsoleColor.Yellow);

        try
        {
            // 使用 java -version 命令获取版本信息
            using var getJavaProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }) ?? throw new InvalidOperationException("无法启动 Java 进程, 请确保已正确安装 Java 并配置环境变量");

            // 读取全部输出
            var output = getJavaProcess.StandardOutput.ReadToEnd().Trim();
            var error = getJavaProcess.StandardError.ReadToEnd().Trim();

            // 确保进程结束
            getJavaProcess.WaitForExit();

            // 优先使用 error 流, 因为 java -version 传统上输出到 stderr
            var log = string.IsNullOrWhiteSpace(error) ? output : error;
            if (string.IsNullOrWhiteSpace(log))
            {
                throw new InvalidOperationException("未能获取 Java 版本信息 (输出为空)");
            }

            // 使用正则表达式提取版本号
            var match = VersionRegex().Match(log);

            // 如果没找到带引号的, 尝试匹配 openjdk 21.0.1 这种无引号格式
            if (!match.Success)
            {
                match = NoQuotationRegex().Match(log);
            }

            if (match.Success)
            {
                // Groups[1] 是括号内捕获到的具体版本号
                IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
                IOMethods.WriteColorMessage($"检测到 Java 环境, 版本: ", ConsoleColor.Green, false);
                IOMethods.WriteColorMessage(match.Groups[1].Value, ConsoleColor.Blue);
            }
            else
            {
                // 虽然没解析出版本号, 但有输出, 大概率Java是存在的, 只是格式不认识
                IOMethods.WriteCurrentTimestamp(ConsoleColor.DarkGreen);
                IOMethods.WriteColorMessage($"检测到 Java 环境, 但无法解析具体版本号", ConsoleColor.DarkGreen);
            }

            return true;
        }
        catch (Exception ex)
        {
            IOMethods.WriteErrorMessage("Java 环境检测失败: ", ex);
            return false;
        }
    }

    /// <summary>
    /// 运行服务器
    /// </summary>
    public static async Task RunAsync()
    {
        // 控制取消令牌源
        using var cts = new CancellationTokenSource();

        // 检测服务器文件是否存在
        var serverFileInfo = new FileInfo(CM.Settings.ServerFilePath);
        if (!serverFileInfo.Exists)
        {
            IOMethods.WriteCurrentTimestamp(ConsoleColor.Red);
            IOMethods.WriteColorMessage($"服务器文件 {CM.Settings.ServerFilePath} 不存在", ConsoleColor.Red);
            return;
        }

        // 输出提示信息
        IOMethods.WriteCurrentTimestamp(ConsoleColor.Yellow);
        IOMethods.WriteColorMessage($"正在启动服务器 {serverFileInfo.Name}...", ConsoleColor.Yellow);
        IOMethods.WriteCurrentTimestamp(ConsoleColor.DarkBlue);
        IOMethods.WriteColorMessage($"内存设置: {CM.Settings.JavaXms} - {CM.Settings.JavaXmx}", ConsoleColor.DarkBlue);
        IOMethods.WriteColorMessage(string.Empty);

        // 构建启动命令
        var startInfo = new ProcessStartInfo()
        {
            FileName = "java",
            Arguments = $"-Xms{CM.Settings.JavaXms} -Xmx{CM.Settings.JavaXmx} -jar \"{serverFileInfo.Name}\" nogui",
            WorkingDirectory = serverFileInfo.DirectoryName,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // 退出代码
        var exitCode = 0;

        try
        {
            // 启动服务器进程
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("无法启动服务器进程");
            process.EnableRaisingEvents = true;
            IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
            IOMethods.WriteColorMessage($"服务器进程已启动, ID: {process.Id}", ConsoleColor.Green);
            IOMethods.WriteCurrentTimestamp(ConsoleColor.DarkGreen);
            IOMethods.WriteColorMessage("正在监听服务器输出...", ConsoleColor.DarkGreen);
            ShowHelp();
            IOMethods.WriteColorMessage(string.Empty);
            IOMethods.EnableInput();

            // 订阅输出事件
            process.OutputDataReceived += OnOutputDataReceived;

            // 检测到服务器输出错误信息
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data is not null)
                {
                    IOMethods.WriteColorMessage(e.Data, ConsoleColor.Red);
                }
            };

            // 开始异步读取输出
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 后台检测控制台窗口大小变化的任务
            var consoleResizeTask = IOMethods.CheckWindowSizeChangeAsync(cts.Token);

            // 后台监测用户输入的任务
            var userInputTask = MonitorUserInputAsync(process, cts.Token);

            // 监测服务器进程是否需要自动关闭
            while (!process.HasExited)
            {
                // 如果自动关闭功能未启用, 则继续循环
                if (!CM.Settings.AutoCloseServer)
                {
                    await Task.Delay(100, cts.Token);
                    continue;
                }

                // 如果有玩家离开时间记录, 且无玩家活动时间超过设置的等待时间, 则发送停止命令
                if (LastPlayerLeaveTime.HasValue && DateTime.Now - LastPlayerLeaveTime.Value >= CM.Settings.WaitTime)
                {
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.DarkRed);
                    IOMethods.WriteColorMessage($"无玩家活动超过 {CM.Settings.WaitTime.TotalMinutes} 分钟, 自动发送停止命令", ConsoleColor.DarkRed);
                    await process.StandardInput.WriteLineAsync(CM.Settings.StopCommand);
                    break;
                }

                // 短暂等待避免高 CPU 占用
                await Task.Delay(100, cts.Token);
            }

            // 取消后台任务
            cts.Cancel();

            // 停止所有后台任务
            await Task.WhenAll(process.WaitForExitAsync(cts.Token), consoleResizeTask, userInputTask);

            // 获取退出代码
            exitCode = process.ExitCode;

            // 如果服务器还未成功启动就退出, 则抛出异常
            if (!_serverStarted)
            {
                throw new InvalidOperationException("服务器未能成功启动, 请检查日志以获取更多信息");
            }
        }
        catch (OperationCanceledException) { /* 任务被取消是正常退出 */ }
        catch (Exception ex)
        {
            // 输出错误信息
            IOMethods.WriteErrorMessage("运行服务器时发生错误: ", ex);
        }

        // 输出服务器关闭信息
        IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
        IOMethods.WriteColorMessage("服务器已关闭, 退出代码: ", ConsoleColor.Green, false);
        var exitColor = exitCode == 0 ? ConsoleColor.Blue : ConsoleColor.Red;
        IOMethods.WriteColorMessage(exitCode.ToString(), exitColor);
        IOMethods.DisableInput();
    }

    /// <summary>
    /// 监测用户输入
    /// </summary>
    /// <param name="process">服务器进程</param>
    /// <param name="token">取消令牌</param>
    /// <exception cref="InvalidOperationException">当输入功能未开启时抛出</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
    private static async Task MonitorUserInputAsync(Process process, CancellationToken token)
    {
        // 用于存储用户输入的变量
        string input;

        // 主输入循环
        while (!process.HasExited)
        {
            // 检查取消令牌
            token.ThrowIfCancellationRequested();

            try
            {
                // 读取用户输入
                input = (await IOMethods.ReadUserInputAsync(token)).Trim();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                // 处理用户输入
                if (input.Equals("warn", StringComparison.OrdinalIgnoreCase))
                {
                    CM.Settings.ShowWarnLogs = !CM.Settings.ShowWarnLogs;
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage($"警告信息显示已切换到: {CM.Settings.ShowWarnLogs}", ConsoleColor.Cyan);
                }
                else if (input.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    CM.Settings.ShowInfoLogs = !CM.Settings.ShowInfoLogs;
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage($"详细信息显示已切换到: {CM.Settings.ShowInfoLogs}", ConsoleColor.Cyan);
                }
                else if (input.Equals("switch", StringComparison.OrdinalIgnoreCase))
                {
                    CM.Settings.AutoCloseServer = !CM.Settings.AutoCloseServer;
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage($"自动关闭服务器已切换到: {CM.Settings.AutoCloseServer}", ConsoleColor.Cyan);

                    // 如果开启了自动关闭功能且当前无玩家, 则重新设置玩家数量为 0 以更新提示
                    if (CM.Settings.AutoCloseServer && PlayerCount == 0)
                    {
                        PlayerCount = 0;
                    }
                }
                else if (input.Equals("listmods", StringComparison.OrdinalIgnoreCase))
                {
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage("Mod列表:", ConsoleColor.Cyan);
                    if (_modList.IsEmpty)
                    {
                        IOMethods.WriteCurrentTimestamp(ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage("未检测到任何 Mod", ConsoleColor.Yellow);
                    }
                    else
                    {
                        foreach (var mod in _modList)
                        {
                            IOMethods.WriteColorMessage($"- {mod}", ConsoleColor.DarkCyan);
                        }
                    }
                }
                else if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    ShowHelp();
                }
                else if (CustomCommandHelper.IsCustomCommand(input))
                {
                    CustomCommandHelper.HandleCustomCommand(input);
                }
                else if (_serverStarted)
                {
                    await process.StandardInput.WriteLineAsync(input);
                }
                else
                {
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Yellow);
                    IOMethods.WriteColorMessage("服务器尚未启动, 无法发送指令", ConsoleColor.Yellow);
                }
            }
            catch (OperationCanceledException)
            {
                // 操作被取消, 退出循环
                break;
            }
            catch (Exception ex)
            {
                IOMethods.WriteErrorMessage("处理用户输入时发生错误: ", ex);
            }

            // 短暂等待避免高 CPU 占用
            await Task.Delay(50, token);
        }
    }

    /// <summary>
    /// 显示帮助
    /// </summary>
    private static void ShowHelp()
    {
        IOMethods.WriteColorMessage("在'>'后可输入指令交互", ConsoleColor.DarkYellow);
        IOMethods.WriteColorMessage("输入'info'来切换是否显示详细信息, 默认关闭", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'warn'来切换是否显示警告信息, 默认关闭", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'switch'来切换是否开启自动关闭服务器功能, 默认开启", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'listmods'来查看Mod列表", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'help'来查看帮助信息", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'waitminutes <值>'来设置无玩家活动等待时间(分钟)", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'commandexecutioninfotime <值>'来设置指令执行后输出详细信息时间(秒)", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("其余输入将发送到服务器执行", ConsoleColor.Blue);
    }

    /// <summary>
    /// 处理服务器输出
    /// </summary>
    private static void OnOutputDataReceived(object? sender, DataReceivedEventArgs e)
    {
        // 如果输出数据为空, 则直接返回
        var data = e.Data?.Trim();
        if (data is null)
        {
            return;
        }

        // 判断是否正在输出 Mod 列表
        if (OutputsModsStartRegex().IsMatch(data))
        {
            _isOutputtingModList = true;
        }
        // 遇到下一行以 [ 开头的信息则表示 Mod 列表输出结束
        else if (_isOutputtingModList && data.StartsWith('['))
        {
            _isOutputtingModList = false;
        }

        // 如果当前正在输出 Mod 列表, 则尝试提取 Mod 名称
        if (_isOutputtingModList && data.StartsWith('-') && !ChineseRegex().IsMatch(data))
        {
            // 提取 Mod 名称并添加到 ModList
            _modList.Add(data[1..].Trim());
        }

        // 如果显示详细信息, 则直接输出
        if (CM.Settings.ShowInfoLogs)
        {
            IOMethods.WriteColorMessage(data, ConsoleColor.White);
        }
        else
        {
            // 错误信息优先输出
            if (data.Contains("error") || data.Contains("Exception"))
            {
                IOMethods.WriteColorMessage(data, ConsoleColor.Red);
            }
            // 开启警告信息输出时输出警告信息
            else if (data.Contains("WARN") && CM.Settings.ShowWarnLogs)
            {
                IOMethods.WriteColorMessage(data, ConsoleColor.Yellow);
            }
            // 否则只在指令执行后的一段时间内输出详细信息
            else if (LastCommandExecutionTime.HasValue && DateTime.Now - LastCommandExecutionTime.Value <= CM.Settings.CommandExecutionInfoTime)
            {
                IOMethods.WriteColorMessage(data, ConsoleColor.White);
            }
        }

        // 服务器成功启动信息处理
        if (data.Contains(CM.Settings.ServerStartSuccessMessage))
        {
            // 标记服务器已启动
            _serverStarted = true;

            // 输出服务器启动成功信息
            IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
            IOMethods.WriteColorMessage("服务器已成功启动", ConsoleColor.Green);

            // 将玩家数量初始化为 0
            PlayerCount = 0;
        }

        // 玩家加入信息处理
        if (data.Contains(CM.Settings.PlayerJoinMessage))
        {
            // 提取玩家名称
            var playerName = data.Split(CM.Settings.PlayerJoinMessage, StringSplitOptions.RemoveEmptyEntries)[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();

            // 输出玩家加入信息
            IOMethods.WriteCurrentTimestamp(ConsoleColor.Blue);
            IOMethods.WriteColorMessage($"玩家 {playerName} 加入服务器", ConsoleColor.Blue);

            // 增加玩家数量
            ++PlayerCount;
        }

        // 玩家离开信息处理
        if (data.Contains(CM.Settings.PlayerLeaveMessage))
        {
            // 提取玩家名称
            var playerName = data.Split(CM.Settings.PlayerLeaveMessage, StringSplitOptions.RemoveEmptyEntries)[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();

            // 输出玩家离开信息
            IOMethods.WriteCurrentTimestamp(ConsoleColor.Blue);
            IOMethods.WriteColorMessage($"玩家 {playerName} 离开服务器", ConsoleColor.Blue);

            // 减少玩家数量
            --PlayerCount;
        }

        // 在线玩家列表信息处理
        try
        {
            // 提取玩家数量和最大玩家数
            var match = PlayerNumRegex().Match(data);

            // 如果匹配成功, 则输出在线玩家数量和名称
            if (match.Success)
            {
                // 提取当前玩家数, 最大玩家数和玩家名称列表
                var currentPlayers = int.Parse(match.Groups[1].Value.Trim());
                var maxPlayers = int.Parse(match.Groups[2].Value.Trim());
                var playersString = match.Groups[3].Value.Trim();

                // 输出在线玩家信息
                IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
                IOMethods.WriteColorMessage($"在线玩家数 {currentPlayers}/{maxPlayers} : ", ConsoleColor.Green, false);
                if (currentPlayers == 0)
                {
                    IOMethods.WriteColorMessage("无玩家在线", ConsoleColor.Green);
                }
                else
                {
                    // 分割玩家名称
                    var players = playersString.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < players.Length; i++)
                    {
                        IOMethods.WriteColorMessage(players[i], ConsoleColor.Blue, false);
                        if (i < players.Length - 1)
                        {
                            IOMethods.WriteColorMessage(", ", ConsoleColor.Green, false);
                        }
                    }
                    IOMethods.WriteColorMessage(string.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            IOMethods.WriteErrorMessage("解析在线玩家列表失败: ", ex);
        }

        // 指令执行信息处理
        if (data.Contains(CM.Settings.CommandBeginMessage))
        {
            try
            {
                // 提取指令执行者
                var startIndex = data.IndexOf('<') + 1;
                var endIndex = data.IndexOf('>');
                var executor = data[startIndex..endIndex].Trim();

                // 提取指令内容
                startIndex = data.IndexOf(CM.Settings.CommandBeginMessage, StringComparison.Ordinal) + CM.Settings.CommandBeginMessage.Length;
                endIndex = data.IndexOf(CM.Settings.CommandEndMessage, startIndex, StringComparison.Ordinal);
                var command = data[startIndex..endIndex].Trim();

                // 特殊指令处理
                if (command.StartsWith(CM.Settings.ChatMessage, StringComparison.Ordinal))
                {
                    // 提取聊天消息内容
                    var chatMessage = command[CM.Settings.ChatMessage.Length..];

                    // 输出聊天消息
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Magenta);
                    IOMethods.WriteColorMessage($"{executor} 发言: {chatMessage}", ConsoleColor.Magenta);
                }
                else if (command == CM.Settings.StopCommand)
                {
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Yellow);
                    IOMethods.WriteColorMessage(executor, ConsoleColor.Blue, false);
                    IOMethods.WriteColorMessage(" 正在关闭服务器...", ConsoleColor.Yellow);
                }
                else if (command != "list")
                {
                    // 更新最后一次指令执行时间
                    LastCommandExecutionTime = DateTime.Now;

                    // 输出指令执行者和指令内容
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.DarkBlue);
                    IOMethods.WriteColorMessage($"指令执行者: {executor}, 指令内容: {command}", ConsoleColor.DarkBlue);
                }
            }
            catch (Exception ex)
            {
                IOMethods.WriteErrorMessage("解析指令执行信息失败: ", ex);
            }
        }

        // 指令执行错误信息处理
        if (data.Contains(CM.Settings.CommandErrorMessage))
        {
            IOMethods.WriteCurrentTimestamp(ConsoleColor.Red);
            IOMethods.WriteColorMessage($"指令执行错误: {e.Data}", ConsoleColor.Red);
        }
    }
}
