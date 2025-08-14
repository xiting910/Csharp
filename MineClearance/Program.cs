using AutoUpdaterDotNET;
using MineClearance.UI;
using MineClearance.Services;
using MineClearance.Utilities;

namespace MineClearance;

/// <summary>
/// 主程序类
/// </summary>
internal static class Program
{
    /// <summary>
    /// 程序唯一标识符
    /// </summary>
    private const string AppId = "Local\\MineClearance_xiting910";

    /// <summary>
    /// 程序入口点
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // 检测是否为Windows操作系统
        if (!Methods.IsWindows())
        {
            // 如果不是Windows操作系统，则显示错误信息并退出程序
            _ = MessageBox.Show("本程序仅支持在Windows操作系统上运行", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 设置未处理异常模式为捕获异常
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // 绑定未捕获异常事件
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // 设置当前线程的UI文化为简体中文
        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");

        // 设置应用程序的视觉样式和文本渲染方式
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // 初始化应用程序配置
        ApplicationConfiguration.Initialize();

        // 初始化 DPI 缩放
        UI.Constants.InitDpiScale();

        // 保证只运行一个实例
        using var mutex = new Mutex(true, AppId, out var isNewInstance);
        if (!isNewInstance)
        {
            _ = MessageBox.Show("程序已在运行中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 初始化数据
        Datas.Initialize().Wait();

        // 初始化自动更新相关设置
        AutoUpdater.Mandatory = true;
        AutoUpdater.RunUpdateAsAdmin = false;

        // 订阅更新检查事件
        AutoUpdater.CheckForUpdateEvent += Methods.AutoUpdaterOnCheckForUpdateEvent;

        // 创建并显示主窗口
        Application.Run(new MainForm());
    }

    /// <summary>
    /// 处理未处理的线程异常
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">线程异常事件参数</param>
    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        // 取消下载
        Methods.CTS.Cancel();

        // 设置强制关闭标志
        Methods.IsForceClose = true;

        // 记录异常到日志文件并弹窗提示错误信息
        LogException(e.Exception);
        _ = MessageBox.Show($"发生未处理的线程异常：{e.Exception.Message}\n错误日志见 {Utilities.Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

        // 退出应用程序
        Application.Exit();
    }

    /// <summary>
    /// 处理未处理的应用程序异常
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">未处理异常事件参数</param>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // 取消下载
        Methods.CTS.Cancel();

        // 设置强制关闭标志
        Methods.IsForceClose = true;

        // 记录异常到日志文件并弹窗提示错误信息
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex);
            _ = MessageBox.Show($"发生未处理的应用程序异常：{ex.Message}\n错误日志见 {Utilities.Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            LogException(new UnknownException("发生未知的未处理异常。"));
            _ = MessageBox.Show($"发生未知的未处理异常\n错误日志见 {Utilities.Constants.ErrorFilePath}\n请联系开发者并提供相关信息", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 退出应用程序
        Application.Exit();
    }

    /// <summary>
    /// 记录异常到日志文件
    /// </summary>
    /// <param name="ex">要记录的异常</param>
    private static void LogException(Exception ex)
    {
        var log = $"[{DateTime.Now}] {ex}\n";
        try
        {
            if (!Directory.Exists(Utilities.Constants.DataPath))
            {
                _ = Directory.CreateDirectory(Utilities.Constants.DataPath);
            }
            File.AppendAllText(Utilities.Constants.ErrorFilePath, log);
        }
        catch { /* 忽略日志写入异常 */ }
    }
}

/// <summary>
/// 未知异常类
/// </summary>
/// <param name="message">异常消息</param>
file sealed class UnknownException(string message) : Exception(message);