namespace Maze;

/// <summary>
/// 程序入口类
/// </summary>
file static class Program
{
    /// <summary>
    /// 程序入口点
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // 设置未处理异常模式为捕获异常
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // 绑定未捕获异常事件
        Application.ThreadException += Methods.OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += Methods.OnUnhandledException;

        // 初始化应用程序配置
        ApplicationConfiguration.Initialize();

        // 初始化 DPI 缩放比例
        Constants.InitDpiScale();

        // 重置迷宫
        Maze.ResetMaze().GetAwaiter().GetResult();

        // 创建并运行主窗口
        Application.Run(MainForm.Instance);
    }
}
