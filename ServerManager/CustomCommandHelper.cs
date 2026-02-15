namespace ServerManager;

/// <summary>
/// 自定义命令帮助类, 用于存放一些命令相关的辅助方法
/// </summary>
public static class CustomCommandHelper
{
    /// <summary>
    /// 设置等待时间命令
    /// </summary>
    private static readonly string WaitMinutesCommand = "waitminutes";

    /// <summary>
    /// 设置指令执行后输出详细信息时间命令
    /// </summary>
    private static readonly string CommandExecutionInfoTimeCommand = "commandexecutioninfotime";

    /// <summary>
    /// 检测输入是否为自定义命令
    /// </summary>
    /// <param name="input">用户输入的命令</param>
    /// <returns>如果是自定义命令则返回 <c>true</c>, 否则返回 <c>false</c></returns>
    public static bool IsCustomCommand(string input)
    {
        var command = input.Split(' ')[0].ToLowerInvariant();
        return command == WaitMinutesCommand || command == CommandExecutionInfoTimeCommand;
    }

    /// <summary>
    /// 处理自定义命令
    /// </summary>
    /// <param name="input">用户输入</param>
    public static void HandleCustomCommand(string input)
    {
        // 提取命令和参数
        var settings = ConfigurationManager.Settings;
        var parts = input.Split(' ', 2);
        var command = parts[0].Trim().ToLowerInvariant();
        var hasValue = parts.Length > 1;
        var value = string.Empty;
        if (hasValue)
        {
            value = parts[1].Trim();
        }

        // 根据命令处理不同的自定义命令
        switch (command)
        {
            // 设置等待时间命令
            case var cmd when cmd.StartsWith(WaitMinutesCommand, StringComparison.OrdinalIgnoreCase):
                if (hasValue)
                {
                    // 有参数输入, 尝试解析参数
                    if (double.TryParse(value, out var waitMinutes) && waitMinutes > 0)
                    {
                        // 参数有效, 设置新的等待时间并保存配置
                        settings.WaitTime = TimeSpan.FromMinutes(waitMinutes);
                        ConfigurationManager.Save();

                        // 输出设置成功信息
                        IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
                        IOMethods.WriteColorMessage($"等待时间已设置为: {settings.WaitTime.TotalMinutes} 分钟", ConsoleColor.Green);
                    }
                    else
                    {
                        // 参数无效, 输出错误信息
                        IOMethods.WriteCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage("无效的等待时间格式, 请使用正数", ConsoleColor.Red);
                    }
                }
                else
                {
                    // 无参数, 显示当前设置
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
                    IOMethods.WriteColorMessage("当前等待时间: ", ConsoleColor.Green, false);
                    IOMethods.WriteColorMessage(settings.WaitTime.TotalMinutes.ToString(), ConsoleColor.Blue, false);
                    IOMethods.WriteColorMessage(" 分钟", ConsoleColor.Green);
                }
                break;

            // 设置指令执行后输出详细信息时间命令
            case var cmd when cmd.StartsWith(CommandExecutionInfoTimeCommand, StringComparison.OrdinalIgnoreCase):
                if (hasValue)
                {
                    // 有参数输入, 尝试解析参数
                    if (double.TryParse(value, out var executionTime) && executionTime > 0)
                    {
                        // 参数有效, 设置新的等待时间并保存配置
                        settings.CommandExecutionInfoTime = TimeSpan.FromSeconds(executionTime);
                        ConfigurationManager.Save();

                        // 输出设置成功信息
                        IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
                        IOMethods.WriteColorMessage($"指令执行后输出详细信息时间已设置为: {settings.CommandExecutionInfoTime.TotalSeconds} 秒", ConsoleColor.Green);
                    }
                    else
                    {
                        // 参数无效, 输出错误信息
                        IOMethods.WriteCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage("无效的指令执行后输出详细信息时间格式, 请使用正数", ConsoleColor.Red);
                    }
                }
                else
                {
                    // 无参数, 显示当前设置
                    IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
                    IOMethods.WriteColorMessage("当前指令执行后输出详细信息时间: ", ConsoleColor.Green, false);
                    IOMethods.WriteColorMessage(settings.CommandExecutionInfoTime.TotalSeconds.ToString(), ConsoleColor.Blue, false);
                    IOMethods.WriteColorMessage(" 秒", ConsoleColor.Green);
                }
                break;

            default:
                IOMethods.WriteCurrentTimestamp(ConsoleColor.Red);
                IOMethods.WriteColorMessage("未知的配置命令", ConsoleColor.Red);
                break;
        }
    }
}
