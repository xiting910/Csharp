using System.Runtime.InteropServices;

namespace FabricServerManager
{
    /// <summary>
    /// 方法类, 用于存放一些方法
    /// </summary>
    public static partial class Methods
    {
        /// <summary>
        /// 判断当前操作系统是否为Windows
        /// </summary>
        /// <returns>如果是Windows系统则返回true, 否则返回false</returns>
        public static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        /// 用于暂停程序, 并输出提示信息, 要按任意键继续
        /// </summary>
        /// <param name="message">提示信息, 默认为"按任意键继续..."</param>
        public static void ReturnByKey(string message = "按任意键继续...")
        {
            Console.WriteLine();
            Console.Write(message);
            Console.ReadKey();
        }

        /// <summary>
        /// 搜索指定目录下的所有文件
        /// </summary>
        /// <param name="directory">要搜索的目录</param>
        /// <param name="searchPattern">搜索模式</param>
        /// <param name="Files">返回的文件列表</param>
        public static void SearchFiles(string directory, string searchPattern, List<string> Files)
        {
            try
            {
                var files = Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly);
                Files.AddRange(files);

                var directories = Directory.GetDirectories(directory);
                foreach (var dir in directories)
                {
                    try
                    {
                        SearchFiles(dir, searchPattern, Files);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 移除文件隐藏属性
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void RemoveHiddenAttribute(string filePath)
        {
            File.SetAttributes(filePath, File.GetAttributes(filePath) & ~FileAttributes.Hidden);
        }

        /// <summary>
        /// 设置文件隐藏属性
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void SetHiddenAttribute(string filePath)
        {
            File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Hidden);
        }
    }
}