using System.Text;

namespace ServerManager;

/// <summary>
/// IO 方法类, 用于存放控制台的输入输出方法
/// </summary>
public static class IOMethods
{
    /// <summary>
    /// 控制台锁
    /// </summary>
    private static readonly Lock _consoleLock = new();

    /// <summary>
    /// 输入历史记录
    /// </summary>
    private static readonly List<string> _inputHistory = [];

    /// <summary>
    /// 当前历史记录索引
    /// </summary>
    private static volatile int _historyIndex;

    /// <summary>
    /// 临时保存当前正在输入的内容(用于上下切换时保持当前输入)
    /// </summary>
    private static volatile string _tempCurrentInput = string.Empty;

    /// <summary>
    /// 当前输入内容
    /// </summary>
    private static StringBuilder _currentInput = new();

    /// <summary>
    /// 输入起始位置(行)
    /// </summary>
    private static volatile int _inputStartTop;

    /// <summary>
    /// 输入起始位置(列)
    /// </summary>
    private static volatile int _inputStartLeft;

    /// <summary>
    /// 当前光标在输入字符串中的索引位置
    /// </summary>
    private static volatile int _inputCursorIndex;

    /// <summary>
    /// 当前是否在等待输出完成
    /// </summary>
    private static volatile bool _isWaitingOutput;

    /// <summary>
    /// 输入功能是否开启字段
    /// </summary>
    private static volatile bool _inputEnabled;

    /// <summary>
    /// 上一次记录的窗口宽度
    /// </summary>
    private static volatile int _lastWindowWidth = Console.WindowWidth;

    /// <summary>
    /// 上一次记录的窗口高度
    /// </summary>
    private static volatile int _lastWindowHeight = Console.WindowHeight;

    /// <summary>
    /// 开启输入功能
    /// </summary>
    public static void EnableInput() => _inputEnabled = true;

    /// <summary>
    /// 关闭输入功能
    /// </summary>
    public static void DisableInput()
    {
        _inputEnabled = false;

        // 清除当前行内容
        lock (_consoleLock)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }

    /// <summary>
    /// 检查窗口大小是否发生变化, 如果变化则调整输入位置
    /// </summary>
    /// <param name="token">取消令牌</param>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
    public static async Task CheckWindowSizeChangeAsync(CancellationToken token)
    {
        while (true)
        {
            // 检查取消请求
            token.ThrowIfCancellationRequested();

            // 检查窗口大小是否变化
            lock (_consoleLock)
            {
                if (Console.WindowWidth != _lastWindowWidth || Console.WindowHeight != _lastWindowHeight)
                {
                    // 窗口大小变化时, 更新记录的窗口大小
                    _lastWindowWidth = Console.WindowWidth;
                    _lastWindowHeight = Console.WindowHeight;

                    // 调整输入位置
                    _inputStartTop = Console.CursorTop;
                    if (_inputStartTop >= Console.WindowHeight)
                    {
                        _inputStartTop = Console.WindowHeight - 1;
                    }
                }
            }

            // 延迟 100 毫秒后继续检查
            await Task.Delay(100, token);
        }
    }

    /// <summary>
    /// <para>用于输出指定颜色的信息, 支持多行输出和长信息自动换行, 输出后会重置颜色</para>
    /// <para>如果输入功能已开启, 则在输出前清除当前输入区域, 输出完成后重绘输入区域,
    /// 以确保输入行始终在输出内容的下方</para>
    /// <para>如果一次信息输出想分成多段进行以显示不同颜色, 可以通过设置参数 <paramref name="OutputCompleted"/> 来通知方法输出是否完成并多次调用此方法</para>
    /// <para>注意: 只有不间断地调用此方法输出信息时, 才允许将 <paramref name="OutputCompleted"/> 设置为 <c>false</c>, 并且结束时必须将其设置为 <c>true</c></para>
    /// </summary>
    /// <param name="message">要输出的信息(支持多行和超长字符串)</param>
    /// <param name="color">颜色, 默认为白色</param>
    /// <param name="OutputCompleted">当前输出是否完成, 默认为 <c>true</c></param>
    public static void WriteColorMessage(string message, ConsoleColor color = ConsoleColor.White, bool OutputCompleted = true)
    {
        lock (_consoleLock)
        {
            if (!_inputEnabled)
            {
                // 如果输入功能未启用, 直接输出消息
                Console.ForegroundColor = color;
                WriteWithAutoWrap(message);
                Console.ResetColor();
                if (OutputCompleted)
                {
                    Console.WriteLine();
                }
                _inputStartTop = Console.CursorTop;
            }
            else
            {
                // 如果当前没有在等待输出完成, 则清除输入区域
                if (!_isWaitingOutput)
                {
                    ClearInputArea();
                }

                // 输出消息
                Console.ForegroundColor = color;
                WriteWithAutoWrap(message);
                Console.ResetColor();

                // 更新等待输出状态
                _isWaitingOutput = !OutputCompleted;

                // 如果输出已完成, 则自动换行并重绘输入区域
                if (OutputCompleted)
                {
                    Console.WriteLine();
                    RedrawInputArea();
                }
            }
        }
    }

    /// <summary>
    /// 输出当前时间戳, 如 [21:06:35]
    /// </summary>
    /// <param name="color">颜色</param>
    public static void WriteCurrentTimestamp(ConsoleColor color)
    {
        var now = DateTime.Now;
        WriteColorMessage($"[{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}] ", color, false);
    }

    /// <summary>
    /// 输出错误信息
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="exception">异常对象</param>
    public static void WriteErrorMessage(string message, Exception exception)
    {
        WriteCurrentTimestamp(ConsoleColor.Red);
        WriteColorMessage(message, ConsoleColor.Red, false);
        WriteColorMessage(exception.Message, ConsoleColor.DarkRed);
    }

    /// <summary>
    /// 单行用户输入处理
    /// </summary>
    /// <param name="token">取消令牌</param>
    /// <returns>用户输入的字符串</returns>
    /// <exception cref="InvalidOperationException">当输入功能未开启时抛出</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
    public static async Task<string> ReadUserInputAsync(CancellationToken token)
    {
        // 检测输入功能是否开启
        if (!_inputEnabled)
        {
            throw new InvalidOperationException("输入功能未开启!");
        }

        // 在行首输出提示符
        lock (_consoleLock)
        {
            Console.SetCursorPosition(0, _inputStartTop);
            Console.Write("> ");

            // 记录输入区域的起始位置
            _inputStartLeft = Console.CursorLeft;
        }

        // 初始化输入状态
        _ = _currentInput.Clear();
        _tempCurrentInput = string.Empty;
        _historyIndex = _inputHistory.Count;
        _inputCursorIndex = 0;

        // 定义变量接受用户输入
        ConsoleKeyInfo keyInfo;

        // 主输入循环
        while (true)
        {
            // 检查取消请求
            token.ThrowIfCancellationRequested();

            // 检查是否有按键按下
            if (!Console.KeyAvailable)
            {
                await Task.Delay(20, token);
                continue;
            }

            // 读取按键
            keyInfo = Console.ReadKey(true);
            lock (_consoleLock)
            {
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    // 用户按下回车键, 结束输入
                    Console.SetCursorPosition(_inputStartLeft + _currentInput.Length, _inputStartTop);
                    Console.WriteLine();
                    _inputStartTop = Console.CursorTop;
                    var result = _currentInput.ToString();

                    // 如果输入不为空, 则添加到历史记录
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        _inputHistory.Add(result);
                    }

                    // 重置输入状态
                    _historyIndex = _inputHistory.Count;
                    _ = _currentInput.Clear();
                    _tempCurrentInput = string.Empty;
                    _inputCursorIndex = 0;
                    return result;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    // 导航到更早的历史记录
                    NavigateHistory(true);
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    // 导航到更新的历史记录
                    NavigateHistory(false);
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    // 向左移动光标
                    if (_inputCursorIndex > 0)
                    {
                        --_inputCursorIndex;
                        Console.SetCursorPosition(_inputStartLeft + _inputCursorIndex, _inputStartTop);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    // 向右移动光标
                    if (_inputCursorIndex < _currentInput.Length)
                    {
                        ++_inputCursorIndex;
                        Console.SetCursorPosition(_inputStartLeft + _inputCursorIndex, _inputStartTop);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    // 用户按下退格键, 删除光标前一个字符
                    if (_inputCursorIndex > 0)
                    {
                        // 删除光标前的一个字符
                        _ = _currentInput.Remove(_inputCursorIndex - 1, 1);
                        --_inputCursorIndex;

                        if (_historyIndex == _inputHistory.Count)
                        {
                            _tempCurrentInput = _currentInput.ToString();
                        }
                        RedrawCurrentInput();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    // 用户按下Delete键, 删除光标当前位置的字符
                    if (_inputCursorIndex < _currentInput.Length)
                    {
                        _ = _currentInput.Remove(_inputCursorIndex, 1);

                        if (_historyIndex == _inputHistory.Count)
                        {
                            _tempCurrentInput = _currentInput.ToString();
                        }
                        RedrawCurrentInput();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Home)
                {
                    // 移动到行首
                    _inputCursorIndex = 0;
                    Console.SetCursorPosition(_inputStartLeft, _inputStartTop);
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    // 移动到行尾
                    _inputCursorIndex = _currentInput.Length;
                    Console.SetCursorPosition(_inputStartLeft + _inputCursorIndex, _inputStartTop);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    // 普通字符输入, 在光标位置插入字符
                    _ = _currentInput.Insert(_inputCursorIndex, keyInfo.KeyChar);
                    ++_inputCursorIndex;

                    if (_historyIndex == _inputHistory.Count)
                    {
                        _tempCurrentInput = _currentInput.ToString();
                    }
                    RedrawCurrentInput();
                }
            }
        }
    }

    /// <summary>
    /// 用于暂停程序, 并输出提示信息, 要按任意键继续
    /// </summary>
    /// <param name="message">提示信息, 默认为"按任意键继续..."</param>
    public static void ReturnByKey(string message = "按任意键继续...")
    {
        lock (_consoleLock)
        {
            Console.WriteLine();
            Console.Write(message);
            _ = Console.ReadKey();
        }
    }

    /// <summary>
    /// 输出信息到控制台, 支持多行和超长字符串自动换行, 末尾的换行符会被忽略
    /// </summary>
    /// <param name="message">要输出的信息(支持多行和超长字符串)</param>
    private static void WriteWithAutoWrap(string message)
    {
        // 空消息直接返回
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        // 按换行符分割
        var lines = message.Split(Environment.NewLine, StringSplitOptions.None);

        // 找到最后一行非空行的索引
        var lastNonEmptyLineIndex = lines.Length - 1;
        while (lastNonEmptyLineIndex >= 0 && string.IsNullOrEmpty(lines[lastNonEmptyLineIndex]))
        {
            lastNonEmptyLineIndex--;
        }

        // 如果全部是空行, 直接返回
        if (lastNonEmptyLineIndex == -1)
        {
            return;
        }

        // 否则, 忽略末尾的所有空行
        var length = lastNonEmptyLineIndex + 1;
        lines = lines[..length];

        // 逐行处理
        for (var i = 0; i < length; i++)
        {
            var line = lines[i].AsSpan();
            var maxWidth = Console.WindowWidth - 1;

            // 处理每一行的自动换行
            while (line.Length > 0)
            {
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

            // 如果不是最后一行, 添加换行
            if (i < lines.Length - 1)
            {
                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// 导航历史记录
    /// </summary>
    /// <param name="isUp"><c>true</c>表示向上(更早的记录), <c>false</c>表示向下(更新的记录)</param>
    private static void NavigateHistory(bool isUp)
    {
        // 如果没有历史记录, 直接返回
        var count = _inputHistory.Count;
        if (count == 0)
        {
            return;
        }

        // 向上箭头: 显示更早的历史记录
        if (isUp && _historyIndex > 0)
        {
            // 第一次向上浏览, 保存当前输入内容
            if (_historyIndex == count)
            {
                _tempCurrentInput = _currentInput.ToString();
            }

            // 向上浏览
            _historyIndex--;

            // 显示选中的历史记录
            _currentInput = new(_inputHistory[_historyIndex]);
            _inputCursorIndex = _currentInput.Length;
            RedrawCurrentInput();
        }
        // 向下箭头: 显示更新的历史记录
        else if (!isUp && _historyIndex < count)
        {
            // 向下浏览
            _historyIndex++;

            // 显示选中的历史记录或临时输入内容
            _currentInput = _historyIndex == count ? new(_tempCurrentInput) : new(_inputHistory[_historyIndex]);
            _inputCursorIndex = _currentInput.Length;
            RedrawCurrentInput();
        }
    }

    /// <summary>
    /// 清除输入区域
    /// </summary>
    private static void ClearInputArea()
    {
        // 隐藏光标以防闪烁
        Console.CursorVisible = false;

        // 移动到输入行最前面
        Console.SetCursorPosition(0, _inputStartTop);

        // 清除整行
        Console.Write(new string(' ', Console.WindowWidth - 1));

        // 回到行首
        Console.SetCursorPosition(0, _inputStartTop);

        // 恢复光标
        Console.CursorVisible = true;
    }

    /// <summary>
    /// 重新绘制输入区域
    /// </summary>
    private static void RedrawInputArea()
    {
        // 隐藏光标以防闪烁
        Console.CursorVisible = false;

        // 设置输入起始位置为光标当前位置
        _inputStartTop = Console.CursorTop;

        // 输出提示符
        Console.Write("> ");

        // 记录输入起始位置
        _inputStartLeft = Console.CursorLeft;

        // 重新输出当前输入内容
        Console.Write(_currentInput);

        // 将光标移动到正确位置
        Console.SetCursorPosition(_inputStartLeft + _inputCursorIndex, _inputStartTop);

        // 恢复光标
        Console.CursorVisible = true;
    }

    /// <summary>
    /// 重新绘制当前输入内容
    /// </summary>
    private static void RedrawCurrentInput()
    {
        // 隐藏光标以防闪烁
        Console.CursorVisible = false;

        // 清除行上的旧内容
        Console.SetCursorPosition(_inputStartLeft, _inputStartTop);
        Console.Write(new string(' ', Console.WindowWidth - _inputStartLeft - 1));

        // 回到输入起始位置
        Console.SetCursorPosition(_inputStartLeft, _inputStartTop);

        // 输出当前输入内容
        Console.Write(_currentInput);

        // 将光标移动到正确位置
        Console.SetCursorPosition(_inputStartLeft + _inputCursorIndex, _inputStartTop);

        // 恢复光标
        Console.CursorVisible = true;
    }
}
