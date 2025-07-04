using System.Diagnostics;
using System.Reflection;

namespace FabricServerManager
{
    /// <summary>
    /// 程序入口点类
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        public static void Main()
        {
            // 设置控制台的编码格式为UTF-8，以支持中文字符
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 设置控制台的标题为"FabricServerManager"
            Console.Title = "FabricServerManager";

            // 如果当前操作系统不是Windows, 则输出提示信息并退出程序
            if (!Methods.IsWindows())
            {
                IOMethods.WriteColorMessage("当前操作系统不是Windows, 该程序无法运行.\n", ConsoleColor.Red);
                Methods.ReturnByKey();
                return;
            }

            // 输出欢迎信息
            IOMethods.WriteColorMessage("欢迎使用FabricServerManager!\n", ConsoleColor.Green);
            IOMethods.WriteColorMessage("本程序支持管理Carpet端Fabric服务器\n", ConsoleColor.Blue);
            IOMethods.WriteColorMessage("在服务器无玩家活动", ConsoleColor.Yellow);
            IOMethods.WriteColorMessage(Config.Data.WaitMinutes.ToString(), ConsoleColor.Red);
            IOMethods.WriteColorMessage("分钟后自动关闭服务器.\n\n", ConsoleColor.Yellow);

            // 检测Java环境
            if (!Prepare.CheckJavaEnvironment())
            {
                Methods.ReturnByKey();
                return;
            }

            // 选择要使用的服务器文件
            string? serverPath = Prepare.FindServerFiles();
            if (serverPath == null)
            {
                Methods.ReturnByKey();
                return;
            }

            // 运行服务器
            bool shouldRestart = Run.RunServer(serverPath);
            if (!shouldRestart)
            {
                Methods.ReturnByKey();
                return;
            }

            // 重启程序
            IOMethods.WriteColorMessage("正在重启程序...\n", ConsoleColor.Yellow);
            string currentExecutable = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
            ProcessStartInfo startInfo = new()
            {
                FileName = currentExecutable,
                UseShellExecute = true,
                CreateNoWindow = false
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}
