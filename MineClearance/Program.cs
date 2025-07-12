namespace MineClearance
{
    /// <summary>
    /// 主程序类
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 检测是否为Windows操作系统
            if (!Methods.IsWindows())
            {
                // 如果不是Windows操作系统，则显示错误信息并退出程序
                MessageBox.Show("本程序仅支持在Windows操作系统上运行。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 设置当前线程的UI文化为中文（简体）
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");

            // 设置应用程序的视觉样式和文本渲染方式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // TODO: 检测所有代码的throw语句
            // 设置未处理异常模式为捕获异常
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // 初始化数据
            Datas.Initialize();

            // 创建并显示主窗口
            var mainForm = new GUI();
            Application.Run(mainForm);
        }
    }
}