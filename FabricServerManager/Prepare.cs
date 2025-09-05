using System.Diagnostics;
using System.Text.Json;

namespace FabricServerManager;

/// <summary>
/// 服务器数据类, 用于存放服务器文件列表和上次运行的服务器文件
/// </summary>
public class ServerData
{
    /// <summary>
    /// 上次运行的服务器文件路径
    /// </summary>
    public string? LastServerFilePath { get; set; }

    /// <summary>
    /// 查找到的服务器文件列表
    /// </summary>
    public List<string> FoundServerFiles { get; set; } = [];
}

/// <summary>
/// 准备工作类
/// </summary>
public static class Prepare
{
    /// <summary>
    /// Java环境异常类
    /// </summary>
    private sealed class JavaEnvironmentException(string message) : Exception(message);

    /// <summary>
    /// 检测Java环境
    /// </summary>
    /// <returns>Java是否存在</returns>
    public static bool CheckJavaEnvironment()
    {
        IOMethods.WriteColorMessage("正在检测Java环境...\n", ConsoleColor.Yellow);

        try
        {
            // 尝试获取Java版本信息
            var javaVersion = Process.Start(new ProcessStartInfo
            {
                FileName = "java",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }) ?? throw new JavaEnvironmentException("Java未安装或未配置环境变量");
            javaVersion.WaitForExit();

            var output = javaVersion.StandardOutput.ReadToEnd().Trim();
            var lines = output.Split('\n');
            if (lines.Length > 0)
            {
                var firstLine = lines[0].Trim();
                // 例如 "java 21.0.7 2025-04-15 LTS"
                var parts = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var version = parts[1];
                    IOMethods.WriteColorMessage("Java环境检测成功, 版本: ", ConsoleColor.Green);
                    IOMethods.WriteColorMessage($"{version}\n\n", ConsoleColor.Blue);
                    return true;
                }
            }
            throw new JavaEnvironmentException("无法获取Java版本信息");
        }
        catch (JavaEnvironmentException ex)
        {
            IOMethods.WriteColorMessage($"Java环境检测失败: {ex.Message}\n", ConsoleColor.Red);
            return false;
        }
        catch (Exception ex)
        {
            IOMethods.WriteColorMessage($"Java环境检测失败: {ex.Message}\n", ConsoleColor.Red);
            return false;
        }
    }

    /// <summary>
    /// 选择服务器文件
    /// </summary>
    /// <returns>选择的服务器文件路径, 没有则返回null</returns>
    public static string? FindServerFiles()
    {
        var serverDataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ServerDataFile);

        if (File.Exists(serverDataFile))
        {
            IOMethods.WriteColorMessage("正在读取上次运行的服务器数据...\n", ConsoleColor.Yellow);

            try
            {
                Methods.RemoveHiddenAttribute(serverDataFile);
                var json = File.ReadAllText(serverDataFile);
                var serverData = JsonSerializer.Deserialize<ServerData>(json, Constants.JsonOptions) ?? new ServerData();
                Methods.SetHiddenAttribute(serverDataFile);

                if (serverData.LastServerFilePath != null && Config.Data.AutoStartLastServer)
                {
                    IOMethods.WriteColorMessage($"自动选择上次运行的服务器文件: {serverData.LastServerFilePath}\n", ConsoleColor.Green);
                    return serverData.LastServerFilePath;
                }
                else if (serverData.FoundServerFiles.Count > 0)
                {
                    IOMethods.WriteColorMessage("保存的服务器文件数据:\n", ConsoleColor.Green);
                    for (var i = 0; i < serverData.FoundServerFiles.Count; i++)
                    {
                        IOMethods.WriteColorMessage($"{i + 1}. {serverData.FoundServerFiles[i]}\n", ConsoleColor.Blue);
                    }
                    IOMethods.WriteColorMessage("请输入要使用的服务器文件编号(0表示重新查找): ", ConsoleColor.Yellow);
                    var input = Console.ReadLine();
                    if (int.TryParse(input, out var index) && index > 0 && index <= serverData.FoundServerFiles.Count)
                    {
                        IOMethods.WriteColorMessage($"已选择服务器文件: {serverData.FoundServerFiles[index - 1]}\n", ConsoleColor.Green);
                        return serverData.FoundServerFiles[index - 1];
                    }
                    else if (input != "0")
                    {
                        IOMethods.WriteColorMessage("无效的输入, 请重新运行程序.\n", ConsoleColor.Red);
                        return null;
                    }
                    IOMethods.WriteColorMessage("已选择重新查找服务器文件\n", ConsoleColor.Yellow);
                }
                else
                {
                    IOMethods.WriteColorMessage("未找到服务器文件数据\n", ConsoleColor.Yellow);
                }
            }
            catch (Exception ex)
            {
                IOMethods.WriteColorMessage($"读取服务器数据失败: {ex.Message}\n", ConsoleColor.Red);
                return null;
            }
        }

        // 开始全盘查找服务器文件
        IOMethods.WriteColorMessage("正在全盘查找服务器文件...\n", ConsoleColor.Yellow);
        IOMethods.WriteColorMessage("请稍等, 可能需要几分钟时间...\n", ConsoleColor.Yellow);

        try
        {
            List<string> serverFiles = [];
            string? selectedFile = null;

            // 获取当前系统所有驱动器
            var drives = DriveInfo.GetDrives();

            // 定义要搜索的目录
            List<string> searchDirectories = [];
            foreach (var drive in drives)
            {
                if (drive.IsReady)
                {
                    searchDirectories.Add(drive.RootDirectory.FullName);
                }
            }

            // 遍历每个目录
            foreach (var directory in searchDirectories)
            {
                if (Directory.Exists(directory))
                {
                    IOMethods.WriteColorMessage($"正在搜索目录: {directory}\n", ConsoleColor.Cyan);
                    Methods.SearchFiles(directory, $"{Constants.ServerFilePrefix}*{Constants.ServerFileSuffix}", serverFiles);
                }
            }

            if (serverFiles.Count == 0)
            {
                IOMethods.WriteColorMessage("未找到服务器文件.\n", ConsoleColor.Red);
                return null;
            }

            IOMethods.WriteColorMessage("查找完毕, 找到以下服务器文件:\n", ConsoleColor.Green);
            for (var i = 0; i < serverFiles.Count; i++)
            {
                IOMethods.WriteColorMessage($"{i + 1}. {serverFiles[i]}\n", ConsoleColor.Blue);
            }

            // 如果只有一个文件, 直接返回
            if (serverFiles.Count == 1)
            {
                IOMethods.WriteColorMessage($"自动选择服务器文件: {serverFiles[0]}\n", ConsoleColor.Green);
                selectedFile = serverFiles[0];
            }
            else
            {
                IOMethods.WriteColorMessage("请输入要使用的服务器文件编号: ", ConsoleColor.Yellow);
                var input = Console.ReadLine();
                if (int.TryParse(input, out var index) && index > 0 && index <= serverFiles.Count)
                {
                    selectedFile = serverFiles[index - 1];
                    IOMethods.WriteColorMessage($"已选择服务器文件: {selectedFile}\n", ConsoleColor.Green);
                }
                else
                {
                    IOMethods.WriteColorMessage("无效的输入, 请重新运行程序.\n", ConsoleColor.Red);
                }
            }

            // 保存服务器数据
            var serverData = new ServerData
            {
                LastServerFilePath = selectedFile,
                FoundServerFiles = serverFiles
            };
            try
            {
                if (File.Exists(serverDataFile))
                {
                    Methods.RemoveHiddenAttribute(serverDataFile);
                }
                else
                {
                    // 如果文件不存在, 创建新文件
                    using var fileStream = File.Create(serverDataFile);
                }

                var json = JsonSerializer.Serialize(serverData, Constants.JsonOptions);
                File.WriteAllText(serverDataFile, json);
                Methods.SetHiddenAttribute(serverDataFile);
            }
            catch (Exception ex)
            {
                IOMethods.WriteColorMessage($"保存服务器数据失败: {ex.Message}\n", ConsoleColor.Red);
            }

            // 返回选择的服务器文件路径
            return selectedFile;
        }
        catch (Exception ex)
        {
            IOMethods.WriteColorMessage($"查找服务器文件失败: {ex.Message}\n", ConsoleColor.Red);
            return null;
        }
    }
}