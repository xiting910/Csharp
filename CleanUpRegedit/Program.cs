using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace CleanUpRegedit;

[SupportedOSPlatform("windows")]
file static class Program
{
    private static void Main()
    {
        if (!IsAdministrator())
        {
            "程序需要管理员权限, 正在尝试以管理员权限重启...".ColorfulWriteLine(ConsoleColor.Yellow);
            RestartAsAdministrator();
            return;
        }

        using var im = new InstallerRegeditKeysManager();
        using var um = new UninstallerRegeditKeysManager();

        $"准备扫描注册表 {InstallerRegeditKeysManager.InstallerRegeditKeyPath} 下的所有项".ColorfulWriteLine(ConsoleColor.DarkCyan);
        MainLogic(im.GetAllKeys(RequestsUserConfirmation("是否删除无效的安装程序注册表项?")), true);
        Console.WriteLine();
        $"准备扫描注册表 {UninstallerRegeditKeysManager.UninstallerRegeditKeyPath} 下的所有项".ColorfulWriteLine(ConsoleColor.DarkCyan);
        MainLogic(um.GetAllKeys(RequestsUserConfirmation("是否删除无效的卸载程序注册表项?")), false);

        if (File.Exists(Logger.CurrentLogFilePath))
        {
            Console.WriteLine();
            $"操作日志已保存至 {Logger.CurrentLogFilePath}".ColorfulWriteLine(ConsoleColor.Green);
        }

        WaitForKeyPress();
    }

    private static void WaitForKeyPress()
    {
        Console.WriteLine();
        Console.Write("按任意键继续...");
        _ = Console.ReadKey();
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static bool RequestsUserConfirmation(string message)
    {
        $"{message}(y/n): ".ColorfulWrite(ConsoleColor.DarkYellow);
        return Console.ReadLine()?.Equals("y", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static void RestartAsAdministrator()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
        {
            "无法获取当前程序路径".ColorfulWriteLine(ConsoleColor.Red);
            WaitForKeyPress();
            return;
        }

        var startInfo = new ProcessStartInfo("wt.exe")
        {
            WorkingDirectory = Environment.CurrentDirectory,
            Arguments = exePath,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            _ = Process.Start(startInfo);
        }
        catch (Win32Exception)
        {
            "权限请求被拒绝".ColorfulWriteLine(ConsoleColor.Red);
            WaitForKeyPress();
        }
    }

    private static void MainLogic(List<CustomRegistryKey> keys, bool isInstaller)
    {
        var temp = isInstaller ? "安装" : "卸载";
        var needDelete = new List<CustomRegistryKey>();

        Console.WriteLine();
        $"扫描完成, 以下是检测到的{temp}程序注册表项: ".ColorfulWriteLine(ConsoleColor.DarkCyan);
        foreach (var (index, key) in keys.Index())
        {
            $"{index}. ".ColorfulWrite(ConsoleColor.Blue);
            Console.WriteLine($"{key.DisplayName}");
        }

        Console.WriteLine();
        $"请输入要删除的{temp}程序注册表项, 多个编号用空格分隔(如果想选择符合某一条件的全部项, 请按回车跳过此步骤): ".ColorfulWrite(ConsoleColor.DarkYellow);
        var input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            needDelete.AddRange(GetIndicesFromInput(input, keys.Count).Select(i => keys[i]));
        }

        Console.WriteLine();
        $"请输入关键字, 程序会自动添加所有名字中包含该关键字的注册表项(不区分大小写): ".ColorfulWrite(ConsoleColor.DarkYellow);
        input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            var keyword = input.Trim();
            var matchedKeys = keys.Where(k => k.DisplayName.Contains(keyword, StringComparison.OrdinalIgnoreCase) && !needDelete.Contains(k));
            needDelete.AddRange(matchedKeys);
        }

        Console.WriteLine();
        $"以下是选择要删除的{temp}程序注册表项: ".ColorfulWriteLine(ConsoleColor.Magenta);
        foreach (var (index, key) in needDelete.Index())
        {
            $"{index}. ".ColorfulWrite(ConsoleColor.Blue);
            $"{key.DisplayName}".ColorfulWriteLine(ConsoleColor.DarkRed);
        }
        $"请输入要取消删除的{temp}程序注册表项, 多个编号用空格分隔(如果想全部取消, 请按回车跳过此步骤): ".ColorfulWrite(ConsoleColor.DarkYellow);
        input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            var indicesToRemove = GetIndicesFromInput(input, needDelete.Count).ToHashSet();
            needDelete = [.. needDelete.Where((_, i) => !indicesToRemove.Contains(i))];
        }

        Console.WriteLine();
        $"以下是将要删除的{temp}程序注册表项: ".ColorfulWriteLine(ConsoleColor.Magenta);
        foreach (var (index, key) in needDelete.Index())
        {
            $"{index}. ".ColorfulWrite(ConsoleColor.Blue);
            $"{key.DisplayName}".ColorfulWriteLine(ConsoleColor.DarkRed);
        }
        if (RequestsUserConfirmation("确认删除以上注册表项?"))
        {
            foreach (var key in needDelete)
            {
                key.Delete();
            }
        }
        else
        {
            "已取消删除操作".ColorfulWriteLine(ConsoleColor.Cyan);
        }
    }

    private static IEnumerable<int> GetIndicesFromInput(string input, int maxIndex)
        => input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var i) ? i : -1)
            .Where(i => i >= 0 && i < maxIndex)
            .Distinct();
}
