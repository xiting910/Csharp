namespace ServerManager;

/// <summary>
/// 程序入口类
/// </summary>
file static class Program
{
    /// <summary>
    /// 欢迎信息
    /// </summary>
    private const string WelcomeMessage = "欢迎使用 ServerManager";

    /// <summary>
    /// 作者信息
    /// </summary>
    private const string AuthorMessage = "作者: xiting910";

    /// <summary>
    /// 未知异常类
    /// </summary>
    private sealed class UnknownException() : Exception("未知异常");

    /// <summary>
    /// 程序主入口点
    /// </summary>
    private static async Task Main()
    {
        // 注册全局异常处理程序
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // 设置控制台的编码格式为 UTF-8, 以支持中文字符
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // 设置控制台的标题为 "ServerManager"
        Console.Title = "ServerManager";

        // 获取当前控制台窗口的宽度, 用于居中显示欢迎信息
        var consoleWidth = Console.WindowWidth - 5;

        // 构造欢迎信息边界和内容
        var boundary = new string('=', consoleWidth);
        var paddedWelcomeMessage = WelcomeMessage.PadLeft((consoleWidth + WelcomeMessage.Length) / 2).PadRight(consoleWidth);
        var paddedAuthorMessage = AuthorMessage.PadLeft((consoleWidth + AuthorMessage.Length) / 2).PadRight(consoleWidth);

        // 输出欢迎信息
        IOMethods.WriteColorMessage(boundary);
        IOMethods.WriteColorMessage(paddedWelcomeMessage, ConsoleColor.Green);
        IOMethods.WriteColorMessage(paddedAuthorMessage, ConsoleColor.Magenta);
        IOMethods.WriteColorMessage(boundary);
        IOMethods.WriteColorMessage(string.Empty);

        // 检测Java环境
        if (!ServerCore.CheckJavaEnvironment())
        {
            IOMethods.ReturnByKey();
            return;
        }
        IOMethods.WriteColorMessage(string.Empty);

        // 加载服务器设置
        ConfigurationManager.Load();

        // 如果服务器文件路径为空, 提示用户进行设置
        if (string.IsNullOrWhiteSpace(ConfigurationManager.Settings.ServerFilePath))
        {
            IOMethods.WriteCurrentTimestamp(ConsoleColor.Cyan);
            IOMethods.WriteColorMessage("请设置服务器文件路径: ", ConsoleColor.Cyan, false);

            var path = Console.ReadLine() ?? string.Empty;
            ConfigurationManager.Settings.ServerFilePath = path.Trim();
            ConfigurationManager.Save();

            IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
            IOMethods.WriteColorMessage("服务器文件路径已保存", ConsoleColor.Green);
        }

        // 运行服务器
        IOMethods.WriteColorMessage(string.Empty);
        await ServerCore.RunAsync();

        // 等待用户按下任意键后退出程序
        IOMethods.ReturnByKey();
    }

    /// <summary>
    /// 处理未捕获的异常
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception ?? new UnknownException();
        IOMethods.WriteErrorMessage("发生未捕获的程序域异常: ", exception);
    }

    /// <summary>
    /// 处理未观察到的任务异常
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        IOMethods.WriteErrorMessage("发生未观察到的任务异常: ", e.Exception);
        e.SetObserved();
    }
}
