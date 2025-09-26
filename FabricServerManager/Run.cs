using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace FabricServerManager;

/// <summary>
/// 运行服务器类
/// </summary>
public static partial class Run
{
    /// <summary>
    /// 服务器玩家数量
    /// </summary>
    private static volatile int _playerCount;
    /// <summary>
    /// 服务器是否已经启动
    /// </summary>
    private static volatile bool _serverStarted;
    /// <summary>
    /// 是否开启自动关闭服务器功能
    /// </summary>
    private static volatile bool _autoCloseServerEnabled;
    /// <summary>
    /// 是否显示警告
    /// </summary>
    private static volatile bool _showWarning;
    /// <summary>
    /// 是否显示详细信息
    /// </summary>
    private static volatile bool _showDetailedInfo;
    /// <summary>
    /// 最后一名玩家离开时间
    /// </summary>
    private static DateTime? _lastPlayerLeaveTime;
    /// <summary>
    /// 最后一名玩家离开时间功能锁
    /// </summary>
    private static readonly object _lastPlayerLeaveTimeLock;
    /// <summary>
    /// 最后一次指令执行的时间
    /// </summary>
    private static DateTime? _lastCommandExecutionTime;
    /// <summary>
    /// 最后一次指令执行时间功能锁
    /// </summary>
    private static readonly object _lastCommandExecutionTimeLock;
    /// <summary>
    /// 是否要重启服务器
    /// </summary>
    private static volatile bool _restartServer;
    /// <summary>
    /// Mod列表
    /// </summary>
    private static readonly ConcurrentBag<string> _modList;

    /// <summary>
    /// 玩家数量增加
    /// </summary>
    private static void IncrementPlayerCount() => _ = Interlocked.Increment(ref _playerCount);

    /// <summary>
    /// 玩家数量减少
    /// </summary>
    private static void DecrementPlayerCount() => _ = Interlocked.Decrement(ref _playerCount);

    /// <summary>
    /// 自动关闭服务器功能切换
    /// </summary>
    private static bool ToggleAutoCloseServer() => _autoCloseServerEnabled = !_autoCloseServerEnabled;

    /// <summary>
    /// 显示警告信息功能切换
    /// </summary>
    private static bool ToggleShowWarning() => _showWarning = !_showWarning;

    /// <summary>
    /// 显示详细信息功能切换
    /// </summary>
    private static bool ToggleShowDetailedInfo() => _showDetailedInfo = !_showDetailedInfo;

    /// <summary>
    /// 静态构造函数, 初始化玩家数量和最后离开时间
    /// </summary>
    static Run()
    {
        _playerCount = 0;
        _autoCloseServerEnabled = true;
        _showWarning = false;
        _showDetailedInfo = false;
        _lastPlayerLeaveTime = null;
        _lastPlayerLeaveTimeLock = new object();
        _lastCommandExecutionTime = null;
        _lastCommandExecutionTimeLock = new object();
        _restartServer = false;
        _modList = [];
    }

    /// <summary>
    /// 检查字符串是否包含中文字符
    /// </summary>
    /// <param name="text">要检查的字符串</param>
    /// <returns>如果包含中文字符返回true，否则返回false</returns>
    private static bool ContainsChinese(string text) => MyRegex().IsMatch(text);

    /// <summary>
    /// 输出Mod列表
    /// </summary>
    private static void OutputModList()
    {
        if (!_modList.IsEmpty)
        {
            foreach (var mod in _modList)
            {
                IOMethods.WriteColorMessage($"- {mod}\n", ConsoleColor.DarkCyan);
            }
        }
        else
        {
            IOMethods.ShowCurrentTimestamp(ConsoleColor.Yellow);
            IOMethods.WriteColorMessage("未检测到任何Mod.\n", ConsoleColor.Yellow);
        }
    }

    /// <summary>
    /// 显示帮助
    /// </summary>
    private static void ShowHelp()
    {
        IOMethods.WriteColorMessage("在'>'后可输入指令交互\n", ConsoleColor.DarkYellow);
        IOMethods.WriteColorMessage("输入'warn'来切换是否显示警告信息, 默认关闭\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'info'来切换是否显示详细信息, 默认关闭\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'switch'来切换是否开启自动关闭服务器功能, 默认开启\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'asls'来切换程序是否自动启动上次运行的服务器\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'listmods'来查看Mod列表\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'list'来查看在线玩家列表\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'restart'来重启程序\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'help'来查看帮助信息\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'javaxms <值>'来设置Java Xms参数\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'javaxmx <值>'来设置Java Xmx参数\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'waitminutes <值>'来设置等待时间（分钟）\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("输入'commandexecutioninfotime <值>'来设置指令执行后输出详细信息时间（秒）\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("其余输入将发送到服务器执行.\n", ConsoleColor.Blue);
        IOMethods.WriteColorMessage("注意: 服务器内存配置需重启后生效\n", ConsoleColor.Yellow);
    }

    /// <summary>
    /// 监测用户输入的线程
    /// </summary>
    /// <param name="process">服务器进程</param>
    /// <param name="token">取消令牌</param>
    private static void MonitorUserInput(Process process, CancellationToken token)
    {
        while (!process.HasExited)
        {
            try
            {
                var inputTask = Task.Run(() => IOMethods.ReadUserInput(token));
                while (!inputTask.IsCompleted)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    Thread.Sleep(100);
                }

                var input = inputTask.Result;
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (input.Trim().Equals("warn", StringComparison.OrdinalIgnoreCase))
                {
                    _ = ToggleShowWarning();
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage($"警告信息显示已切换到: {_showWarning}\n", ConsoleColor.Cyan);
                }
                else if (input.Trim().Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    _ = ToggleShowDetailedInfo();
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage($"详细信息显示已切换到: {_showDetailedInfo}\n", ConsoleColor.Cyan);
                }
                else if (input.Trim().Equals("switch", StringComparison.OrdinalIgnoreCase))
                {
                    _ = ToggleAutoCloseServer();
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage($"自动关闭服务器已切换到: {_autoCloseServerEnabled}\n", ConsoleColor.Cyan);
                    if (_autoCloseServerEnabled && _playerCount == 0)
                    {
                        lock (_lastPlayerLeaveTimeLock)
                        {
                            _lastPlayerLeaveTime = DateTime.Now;
                        }
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage("服务器无玩家活动, 将在", ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage(Config.Data.WaitMinutes.ToString(CultureInfo.InvariantCulture), ConsoleColor.Red);
                        IOMethods.WriteColorMessage("分钟后自动关闭.\n", ConsoleColor.Yellow);
                    }
                }
                else if (input.Trim().Equals("asls", StringComparison.OrdinalIgnoreCase))
                {
                    Config.Data.AutoStartLastServer = !Config.Data.AutoStartLastServer;
                    Config.SaveConfig();
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage($"自动启动上次运行的服务器已切换到: {Config.Data.AutoStartLastServer}\n", ConsoleColor.Cyan);
                }
                else if (input.Trim().Equals("listmods", StringComparison.OrdinalIgnoreCase))
                {
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage("Mod列表:\n", ConsoleColor.Cyan);
                    OutputModList();
                }
                else if (input.Trim().Equals("restart", StringComparison.OrdinalIgnoreCase))
                {
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Cyan);
                    IOMethods.WriteColorMessage("正在重启程序...\n", ConsoleColor.Cyan);
                    _restartServer = true;
                    _ = process.StandardInput.WriteLineAsync(Constants.StopCommand);
                }
                else if (input.Trim().Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    ShowHelp();
                }
                else if (ConfigCommands.IsConfigCommand(input.Trim()))
                {
                    ConfigCommands.HandleConfigCommand(input.Trim());
                }
                else if (_serverStarted)
                {
                    _ = process.StandardInput.WriteLineAsync(input);
                }
            }
            catch (OperationCanceledException)
            {
                // 操作被取消，退出循环
                break;
            }
            catch (Exception ex)
            {
                IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                IOMethods.WriteColorMessage($"输入处理错误: {ex.Message}\n", ConsoleColor.Red);
            }

            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// 运行服务器
    /// </summary>
    /// <param name="serverPath">服务器文件路径</param>
    /// <returns>是否要重启</returns>
    public static bool RunServer(string serverPath)
    {
        // 获取服务器文件名和文件夹路径
        serverPath = Path.GetFullPath(serverPath);
        if (!File.Exists(serverPath))
        {
            IOMethods.WriteColorMessage($"服务器文件不存在: {serverPath}\n", ConsoleColor.Red);
            return false;
        }
        var serverFile = Path.GetFileName(serverPath);
        var ServerFolder = Path.GetDirectoryName(serverPath) ?? throw new InvalidOperationException("无法获取服务器文件夹路径");

        IOMethods.WriteColorMessage("\n正在启动服务器...\n", ConsoleColor.Yellow);
        IOMethods.WriteColorMessage($"内存分配: {Config.Data.JavaXms} - {Config.Data.JavaXmx}\n", ConsoleColor.Cyan);

        try
        {
            // 构建启动命令
            var command = $"java -Xms{Config.Data.JavaXms} -Xmx{Config.Data.JavaXmx} -jar \"{serverFile}\" nogui";
            ProcessStartInfo startInfo = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                WorkingDirectory = ServerFolder,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // 启动服务器进程
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("无法启动服务器进程");
            process.EnableRaisingEvents = true;
            IOMethods.WriteColorMessage($"服务器进程已启动, ID: {process.Id}\n", ConsoleColor.Green);
            IOMethods.WriteColorMessage("正在监听服务器输出...\n", ConsoleColor.DarkGreen);
            ShowHelp();
            IOMethods.InputEnabled = true;
            IOMethods.WriteColorMessage("\n");

            // 获取服务器的Mod列表
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains("- ") && !_serverStarted)
                {
                    if (e.Data.Trim().Split("- ")[0].All(c => c == ' ') && !e.Data.Contains("-- "))
                    {
                        if (!e.Data.Contains("Unsupported root entry") && !e.Data.Contains("java") && !ContainsChinese(e.Data))
                        {
                            // 提取Mod名称并添加到ModList
                            var modName = e.Data.Trim().Split("- ")[1].Trim();
                            _modList.Add(modName);
                        }
                    }
                }
            };

            // 服务器详细信息输出
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is null)
                {
                    return;
                }

                // 如果显示详细信息, 则直接输出
                if (_showDetailedInfo)
                {
                    IOMethods.WriteColorMessage(e.Data + '\n', ConsoleColor.White);
                    return;
                }

                // 如果不显示详细信息, 则只在指令执行后输出
                lock (_lastCommandExecutionTimeLock)
                {
                    if (_lastCommandExecutionTime.HasValue)
                    {
                        var elapsed = DateTime.Now - _lastCommandExecutionTime.Value;
                        if (elapsed.TotalSeconds <= Config.Data.CommandExecutionInfoTime)
                        {
                            IOMethods.WriteColorMessage(e.Data + '\n', ConsoleColor.White);
                        }
                        else
                        {
                            _lastCommandExecutionTime = null;
                        }
                    }
                }
            };

            // 检测服务器错误或者警告
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null)
                {
                    if (e.Data.Contains("error") || e.Data.Contains("Exception"))
                    {
                        IOMethods.WriteColorMessage(e.Data, ConsoleColor.Red);
                        IOMethods.WriteColorMessage("\n");
                    }
                    else if (e.Data.Contains("WARN") && _showWarning)
                    {
                        IOMethods.WriteColorMessage(e.Data, ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage("\n");
                    }
                }
            };

            // 检测服务器启动成功
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains(Constants.ServerStartSuccessMessage))
                {
                    _serverStarted = true;
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                    IOMethods.WriteColorMessage("服务器已成功启动.\n", ConsoleColor.Green);

                    if (_autoCloseServerEnabled)
                    {
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage("服务器无玩家活动, 将在", ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage(Config.Data.WaitMinutes.ToString(CultureInfo.InvariantCulture), ConsoleColor.Red);
                        IOMethods.WriteColorMessage("分钟后自动关闭.\n", ConsoleColor.Yellow);
                        lock (_lastPlayerLeaveTimeLock)
                        {
                            _lastPlayerLeaveTime = DateTime.Now;
                        }
                    }

                }
            };

            // 检测玩家加入
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains(Constants.PlayerJoinMessage))
                {
                    IncrementPlayerCount();
                    var parts = e.Data.Split(Constants.PlayerJoinMessage, StringSplitOptions.RemoveEmptyEntries);
                    var playerName = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Blue);
                    IOMethods.WriteColorMessage($"玩家 {playerName} 加入服务器.\n", ConsoleColor.Blue);
                    lock (_lastPlayerLeaveTimeLock)
                    {
                        // 更新最后离开时间
                        _lastPlayerLeaveTime = null;
                    }
                }
            };

            // 检测玩家离开
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains(Constants.PlayerLeaveMessage))
                {
                    DecrementPlayerCount();
                    var parts = e.Data.Split(Constants.PlayerLeaveMessage, StringSplitOptions.RemoveEmptyEntries);
                    var playerName = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Blue);
                    IOMethods.WriteColorMessage($"玩家 {playerName} 离开服务器.\n", ConsoleColor.Blue);
                    if (_autoCloseServerEnabled && _playerCount == 0)
                    {
                        lock (_lastPlayerLeaveTimeLock)
                        {
                            // 更新最后离开时间
                            _lastPlayerLeaveTime = DateTime.Now;
                        }
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage("服务器无玩家活动, 将在", ConsoleColor.Yellow);
                        IOMethods.WriteColorMessage(Config.Data.WaitMinutes.ToString(CultureInfo.InvariantCulture), ConsoleColor.Red);
                        IOMethods.WriteColorMessage("分钟后自动关闭.\n", ConsoleColor.Yellow);
                    }
                }
            };

            // 检测指令执行
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains(Constants.CommandMessage))
                {
                    try
                    {
                        // 提取指令执行者和指令内容
                        var startIndex = e.Data.IndexOf('<') + 1;
                        var endIndex = e.Data.IndexOf('>');
                        var executor = e.Data[startIndex..endIndex];

                        startIndex = e.Data.IndexOf("[Command: /", StringComparison.Ordinal) + "[Command: /".Length;
                        endIndex = e.Data.IndexOf(']', startIndex);
                        var command = e.Data[startIndex..endIndex];

                        if (command.StartsWith(Constants.ChatMessage, StringComparison.Ordinal))
                        {
                            // 提取聊天消息内容
                            var chatMessage = command[Constants.ChatMessage.Length..].Trim();
                            // 输出聊天消息
                            IOMethods.ShowCurrentTimestamp(ConsoleColor.Magenta);
                            IOMethods.WriteColorMessage($"{executor} 发言: {chatMessage}\n", ConsoleColor.Magenta);
                        }
                        else if (command == Constants.StopCommand)
                        {
                            IOMethods.ShowCurrentTimestamp(ConsoleColor.Yellow);
                            IOMethods.WriteColorMessage(executor, ConsoleColor.Blue);
                            IOMethods.WriteColorMessage("正在关闭服务器...\n", ConsoleColor.Yellow);
                        }
                        else if (command != "list")
                        {
                            // 输出指令执行者和指令内容
                            IOMethods.ShowCurrentTimestamp(ConsoleColor.DarkBlue);
                            IOMethods.WriteColorMessage($"指令执行者: {executor}, 指令内容: {command}\n", ConsoleColor.DarkBlue);
                            lock (_lastCommandExecutionTimeLock)
                            {
                                // 更新最后一次指令执行时间
                                _lastCommandExecutionTime = DateTime.Now;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage($"解析指令失败: {ex.Message}\n", ConsoleColor.Red);
                    }
                }
            };

            // 检测list指令输出在线玩家信息
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains("There are") && e.Data.Contains("players online:"))
                {
                    try
                    {
                        // 提取玩家数量和最大玩家数
                        var pattern = @"There are (\d+) of a max of (\d+) players online: (.*)";
                        var match = System.Text.RegularExpressions.Regex.Match(e.Data, pattern);

                        if (match.Success)
                        {
                            var currentPlayers = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                            var maxPlayers = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                            var playersString = match.Groups[3].Value.Trim();

                            IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                            IOMethods.WriteColorMessage($"在线玩家数 {currentPlayers}/{maxPlayers} :", ConsoleColor.Green);

                            if (currentPlayers == 0)
                            {
                                IOMethods.WriteColorMessage(" 无玩家在线.\n", ConsoleColor.Green);
                            }
                            else
                            {
                                // 分割玩家名称
                                var players = playersString.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                                IOMethods.WriteColorMessage("在线玩家: ", ConsoleColor.Green);
                                for (var i = 0; i < players.Length; i++)
                                {
                                    IOMethods.WriteColorMessage(players[i], ConsoleColor.Blue);
                                    if (i < players.Length - 1)
                                    {
                                        IOMethods.WriteColorMessage(", ", ConsoleColor.Green);
                                    }
                                }
                                IOMethods.WriteColorMessage("\n");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage($"解析在线玩家信息失败: {ex.Message}\n", ConsoleColor.Red);
                    }
                }
            };

            // 检测指令错误
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains(Constants.CommandErrorMessage))
                {
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                    IOMethods.WriteColorMessage($"指令执行错误: {e.Data}\n", ConsoleColor.Red);
                }
            };

            // 检测到服务器输出错误信息
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data is not null)
                {
                    IOMethods.WriteColorMessage(e.Data, ConsoleColor.Red);
                    IOMethods.WriteColorMessage("\n");
                }
            };

            // 开始异步读取输出
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource();

            // 启动检测控制台大小变化的线程
            Thread consoleResizeThread = new(() => IOMethods.CheckWindowSizeChange(cts.Token));
            consoleResizeThread.Start();

            // 启动监测用户输入的线程
            Thread inputThread = new(() => MonitorUserInput(process, cts.Token));
            inputThread.Start();

            // 无玩家活动一定时间后自动关闭服务器
            while (!process.HasExited)
            {
                if (!_autoCloseServerEnabled)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                lock (_lastPlayerLeaveTimeLock)
                {
                    if (_lastPlayerLeaveTime.HasValue)
                    {
                        var idleTime = DateTime.Now - _lastPlayerLeaveTime.Value;
                        if (idleTime.TotalMinutes >= Config.Data.WaitMinutes)
                        {
                            IOMethods.ShowCurrentTimestamp(ConsoleColor.DarkRed);
                            IOMethods.WriteColorMessage($"无玩家活动超过{Config.Data.WaitMinutes}分钟, 自动发送停止命令\n", ConsoleColor.DarkRed);
                            _ = process.StandardInput.WriteLineAsync(Constants.StopCommand);
                            break;
                        }
                    }
                }
                Thread.Sleep(1000);
            }

            // 等待服务器进程退出
            process.WaitForExit();

            // 停止所有线程
            cts.Cancel();
            inputThread.Join();
            consoleResizeThread.Join();

            // 输出服务器关闭信息
            IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
            IOMethods.WriteColorMessage("服务器已关闭.\n", ConsoleColor.Green);
            IOMethods.InputEnabled = false;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

            return _restartServer;
        }
        catch (Exception ex)
        {
            IOMethods.WriteColorMessage($"启动服务器失败: {ex.Message}\n", ConsoleColor.Red);
            return false;
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"[\u4e00-\u9fff]")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}