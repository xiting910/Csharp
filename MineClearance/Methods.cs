namespace MineClearance
{
    /// <summary>
    /// 提供一些常用方法
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// 线程安全的随机数生成器
        /// </summary>
        public static Random RandomInstance => Random.Shared;

        /// <summary>
        /// 检测是否为Windows操作系统
        /// </summary>
        /// <returns>如果是Windows操作系统则返回true，否则返回false</returns>
        public static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
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

        /// <summary>
        /// 移除文件夹隐藏属性
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        public static void RemoveHiddenAttributeForDirectory(string directoryPath)
        {
            DirectoryInfo directoryInfo = new(directoryPath);
            directoryInfo.Attributes &= ~FileAttributes.Hidden;
        }

        /// <summary>
        /// 设置文件夹隐藏属性
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        public static void SetHiddenAttributeForDirectory(string directoryPath)
        {
            DirectoryInfo directoryInfo = new(directoryPath);
            directoryInfo.Attributes |= FileAttributes.Hidden;
        }

        /// <summary>
        /// 当前是否为第一次检查更新
        /// </summary>
        public static bool IsFirstCheck { get; set; } = true;
    }
}