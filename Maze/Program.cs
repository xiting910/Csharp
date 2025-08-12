namespace Maze;

/// <summary>
/// 程序入口类
/// </summary>
internal static class Program
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

        // 设置应用程序的视觉样式和文本渲染方式
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // 初始化应用程序配置
        ApplicationConfiguration.Initialize();

        // 初始化 DPI 缩放比例
        Constants.InitDpiScale();

        // 重置迷宫
        Maze.ResetMaze().Wait();

        // 创建主窗口
        var mainForm = new MainForm();

        // 运行主窗口
        Application.Run(mainForm);
    }
}