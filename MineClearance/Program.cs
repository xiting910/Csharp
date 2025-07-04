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

            // 初始化数据
            Datas.Initialize();

            // 设置应用程序的视觉样式和文本渲染方式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 创建并显示主窗口
            var mainForm = new GUI();
            Application.Run(mainForm);
        }
    }
}