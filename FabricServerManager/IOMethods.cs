namespace FabricServerManager;

/// <summary>
/// IO方法类, 用于存放一些输入输出方法
/// </summary>
public static class IOMethods
{
    /// <summary>
    /// 控制台锁
    /// </summary>
    private static readonly object _consoleLock = new();
    /// <summary>
    /// 输入历史记录锁
    /// </summary>
    private static readonly object _historyLock = new();
    /// <summary>
    /// 输入历史记录
    /// </summary>
    private static readonly List<string> _inputHistory = [];
    /// <summary>
    /// 当前历史记录索引 (-1表示没有选择历史记录)
    /// </summary>
    private static volatile int _historyIndex = -1;
    /// <summary>
    /// 临时保存当前正在输入的内容（用于上下切换时保持当前输入）
    /// </summary>
    private static volatile string _tempCurrentInput = "";
    /// <summary>
    /// 当前输入内容（单行）
    /// </summary>
    private static volatile string _currentInput = "";
    /// <summary>
    /// 输入起始位置（列）
    /// </summary>
    private static volatile int _inputStartLeft;
    /// <summary>
    /// 输入区域开始位置（行）
    /// </summary>
    private static volatile int _inputStartTop;
    /// <summary>
    /// 输入功能是否开启
    /// </summary>
    public static bool InputEnabled { get; set; }
    /// <summary>
    /// 上一次输出的message是否以换行符结尾
    /// </summary>
    private static volatile bool _lastMessageEndsWithNewLine = true;
    /// <summary>
    /// 上一次记录的窗口宽度
    /// </summary>
    private static volatile int _lastWindowWidth = Console.WindowWidth;
    /// <summary>
    /// 上一次记录的窗口高度
    /// </summary>
    private static volatile int _lastWindowHeight = Console.WindowHeight;

    /// <summary>
    /// 检查窗口大小是否发生变化，如果变化则调整输入位置
    /// </summary>
    /// <param name="token">取消令牌</param>
    public static void CheckWindowSizeChange(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            lock (_consoleLock)
            {
                if (Console.WindowWidth != _lastWindowWidth || Console.WindowHeight != _lastWindowHeight)
                {
                    _lastWindowWidth = Console.WindowWidth;
                    _lastWindowHeight = Console.WindowHeight;

                    // 窗口大小变化时，重新设置输入区域位置
                    _inputStartTop = Console.CursorTop;
                    if (_inputStartTop >= Console.WindowHeight)
                    {
                        _inputStartTop = Console.WindowHeight - 1;
                    }
                }
            }

            // 每隔50毫秒检查一次
            Thread.Sleep(50);
        }
    }

    /// <summary>
    /// 用于输出指定颜色的多行信息，支持自动换行，输出后会重置颜色
    /// </summary>
    /// <param name="message">要输出的信息（支持多行和超长字符串）</param>
    /// <param name="color">颜色, 默认为白色</param>
    public static void WriteColorMessage(string message, ConsoleColor color = ConsoleColor.White)
    {
        lock (_consoleLock)
        {
            if (!InputEnabled)
            {
                Console.ForegroundColor = color;
                WriteWithAutoWrap(message);
                Console.ResetColor();
                _inputStartTop = Console.CursorTop;
            }
            else
            {
                // 如果上一次输出以换行符结尾，则清除当前输入区域
                if (_lastMessageEndsWithNewLine)
                {
                    ClearInputArea();
                }

                // 输出消息（支持自动换行）
                Console.ForegroundColor = color;
                WriteWithAutoWrap(message);
                Console.ResetColor();

                // 如果message以换行符结尾，则重绘输入区域
                _lastMessageEndsWithNewLine = message.EndsWith('\n');
                if (_lastMessageEndsWithNewLine)
                {
                    RedrawInputArea();
                }
            }
        }
    }

    /// <summary>
    /// 带自动换行的写入方法
    /// </summary>
    /// <param name="message">要写入的消息</param>
    private static void WriteWithAutoWrap(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        // 按换行符分割
        var lines = message.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // 处理每一行的自动换行
            while (line.Length > 0)
            {
                var maxWidth = Console.WindowWidth;

                if (line.Length <= maxWidth)
                {
                    // 整行可以显示
                    Console.Write(line);
                    break;
                }
                else
                {
                    // 需要分割显示
                    Console.Write(line[..maxWidth]);
                    Console.WriteLine();
                    line = line[maxWidth..];
                }
            }

            // 如果不是最后一行，添加换行
            if (i < lines.Length - 1)
            {
                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// 清除输入区域（单行）
    /// </summary>
    private static void ClearInputArea()
    {
        // 移动到输入行开始位置
        Console.SetCursorPosition(0, _inputStartTop);
        // 清除整行
        Console.Write(new string(' ', Console.WindowWidth - 1));
        // 回到行首
        Console.SetCursorPosition(0, _inputStartTop);
    }

    /// <summary>
    /// 重新绘制输入区域（单行）
    /// </summary>
    private static void RedrawInputArea()
    {
        _inputStartTop = Console.CursorTop;
        Console.Write("> ");
        _inputStartLeft = Console.CursorLeft;
        Console.Write(_currentInput);
    }

    /// <summary>
    /// 导航历史记录
    /// </summary>
    /// <param name="goUp">true表示向上（更早的记录），false表示向下（更新的记录）</param>
    private static void NavigateHistory(bool goUp)
    {
        if (_inputHistory.Count == 0)
        {
            return;
        }

        if (goUp)
        {
            // 向上箭头：显示更早的历史记录
            if (_historyIndex == -1)
            {
                // 第一次按上箭头，保存当前输入并显示最新的历史记录
                _tempCurrentInput = _currentInput;
                _historyIndex = _inputHistory.Count - 1;
            }
            else if (_historyIndex > 0)
            {
                // 继续向上浏览
                _historyIndex--;
            }

            if (_historyIndex >= 0)
            {
                _currentInput = _inputHistory[_historyIndex];
                RedrawCurrentInput();
            }
        }
        else
        {
            // 向下箭头：显示更新的历史记录
            if (_historyIndex >= 0)
            {
                _historyIndex++;

                if (_historyIndex >= _inputHistory.Count)
                {
                    // 超出历史记录范围，恢复到当前输入
                    _historyIndex = -1;
                    _currentInput = _tempCurrentInput;
                }
                else
                {
                    _currentInput = _inputHistory[_historyIndex];
                }

                RedrawCurrentInput();
            }
        }
    }

    /// <summary>
    /// 重新绘制当前输入内容
    /// </summary>
    private static void RedrawCurrentInput()
    {
        // 清除行上的旧内容
        Console.SetCursorPosition(_inputStartLeft, _inputStartTop);
        Console.Write(new string(' ', Console.WindowWidth - _inputStartLeft - 1));

        // 回到输入起始位置
        Console.SetCursorPosition(_inputStartLeft, _inputStartTop);

        // 输出当前输入内容
        Console.Write(_currentInput);
    }

    /// <summary>
    /// 单行用户输入处理
    /// </summary>
    /// <param name="token">取消令牌</param>
    /// <returns>用户输入的字符串</returns>
    public static string? ReadUserInput(CancellationToken token)
    {
        // 检测输入功能是否开启
        if (!InputEnabled)
        {
            WriteColorMessage("输入功能未开启, 请先启用输入功能.\n", ConsoleColor.Red);
            return null;
        }

        lock (_consoleLock)
        {
            // 在行首输出提示符
            Console.SetCursorPosition(0, _inputStartTop);
            Console.Write("> ");

            // 记录输入区域的起始位置
            _inputStartLeft = Console.CursorLeft;
        }

        _currentInput = "";
        _tempCurrentInput = "";
        _historyIndex = -1;

        while (!token.IsCancellationRequested)
        {
            // 检查是否有按键可用
            if (!Console.KeyAvailable)
            {
                Thread.Sleep(50);
                continue;
            }

            var keyInfo = Console.ReadKey(true);

            lock (_consoleLock)
            {
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    _inputStartTop = Console.CursorTop;
                    var result = _currentInput;

                    // 如果输入不为空且与上一次输入不同，则添加到历史记录
                    lock (_historyLock)
                    {
                        if (!string.IsNullOrWhiteSpace(result) && (_inputHistory.Count == 0 || _inputHistory[^1] != result))
                        {
                            _inputHistory.Add(result);
                        }
                    }

                    _historyIndex = -1;
                    _currentInput = "";
                    _tempCurrentInput = "";
                    return result;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && _currentInput.Length > 0)
                {
                    _currentInput = _currentInput[..^1];
                    if (_historyIndex == -1)
                    {
                        _tempCurrentInput = _currentInput;
                    }
                    RedrawCurrentInput();
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    NavigateHistory(true);
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    NavigateHistory(false);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    _currentInput += keyInfo.KeyChar;
                    if (_historyIndex == -1)
                    {
                        _tempCurrentInput = _currentInput;
                    }
                    RedrawCurrentInput();
                }
            }
        }

        // 如果被取消，返回null
        return null;
    }

    /// <summary>
    /// 显示当前时间戳, 如[21:06:35]
    /// </summary>
    /// <param name="color">颜色, 默认为白色</param>
    public static void ShowCurrentTimestamp(ConsoleColor color = ConsoleColor.White)
    {
        var now = DateTime.Now;
        WriteColorMessage($"[{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}] ", color);
    }
}