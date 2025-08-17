using System.Text.Json;
using System.Text.RegularExpressions;

namespace FabricServerManager;

/// <summary>
/// 配置数据类
/// </summary>
public class ConfigData
{
    /// <summary>
    /// Java Xms 参数
    /// </summary>
    public string JavaXms { get; set; } = string.Empty;

    /// <summary>
    /// Java Xmx 参数
    /// </summary>
    public string JavaXmx { get; set; } = string.Empty;

    /// <summary>
    /// 等待时间（分钟）
    /// </summary>
    public float WaitMinutes { get; set; }

    /// <summary>
    /// 指令执行后输出详细信息时间（秒）
    /// </summary>
    public int CommandExecutionInfoTime { get; set; }

    /// <summary>
    /// 是否自动启动上次运行的服务器
    /// </summary>
    public bool AutoStartLastServer { get; set; }

    /// <summary>
    /// 默认配置数据
    /// </summary>
    public static ConfigData DefaultConfigData { get; } = new ConfigData
    {
        JavaXms = Constants.DefaultJavaXms,
        JavaXmx = Constants.DefaultJavaXmx,
        WaitMinutes = Constants.DefaultWaitMinutes,
        CommandExecutionInfoTime = Constants.DefaultCommandExecutionInfoTime,
        AutoStartLastServer = true
    };
}

/// <summary>
/// 配置类, 用于管理配置
/// </summary>
public static class Config
{
    /// <summary>
    /// 配置文件路径
    /// </summary>
    private static readonly string _configFilePath;

    /// <summary>
    /// 配置数据
    /// </summary>
    public static ConfigData Data { get; private set; }

    /// <summary>
    /// 构造函数, 从配置文件加载配置
    /// </summary>
    static Config()
    {
        // 初始化配置数据为默认配置
        Data = ConfigData.DefaultConfigData;

        // 获取配置文件路径
        _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ConfigFileName);

        if (!File.Exists(_configFilePath))
        {
            SaveConfig();
        }
        else
        {
            LoadConfig();
        }
    }

    /// <summary>
    /// 从配置文件加载配置
    /// </summary>
    private static void LoadConfig()
    {
        try
        {
            Methods.RemoveHiddenAttribute(_configFilePath);
            var json = File.ReadAllText(_configFilePath);
            Data = JsonSerializer.Deserialize<ConfigData>(json, Constants.JsonOptions) ?? ConfigData.DefaultConfigData;
            Methods.SetHiddenAttribute(_configFilePath);
        }
        catch (Exception ex)
        {
            IOMethods.WriteColorMessage($"加载配置文件失败: {ex.Message}", ConsoleColor.Red);
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// 保存配置到配置文件
    /// </summary>
    public static void SaveConfig()
    {
        if (!File.Exists(_configFilePath))
        {
            try
            {
                using var fileStream = File.Create(_configFilePath);
            }
            catch (Exception ex)
            {
                IOMethods.WriteColorMessage($"创建配置文件失败: {ex.Message}", ConsoleColor.Red);
                return;
            }
        }
        else
        {
            Methods.RemoveHiddenAttribute(_configFilePath);
        }

        try
        {
            var json = JsonSerializer.Serialize(Data, Constants.JsonOptions);
            File.WriteAllText(_configFilePath, json);
            Methods.SetHiddenAttribute(_configFilePath);
        }
        catch (Exception ex)
        {
            IOMethods.WriteColorMessage($"保存配置文件失败: {ex.Message}", ConsoleColor.Red);
        }
    }
}

/// <summary>
/// 配置设置命令类
/// </summary>
public static partial class ConfigCommands
{
    /// <summary>
    /// 设置Java Xms参数命令
    /// </summary>
    private static readonly string JavaXmsCommand = "javaxms";
    /// <summary>
    /// 设置Java Xmx参数命令
    /// </summary>
    private static readonly string JavaXmxCommand = "javaxmx";
    /// <summary>
    /// 设置等待时间命令
    /// </summary>
    private static readonly string WaitMinutesCommand = "waitminutes";
    /// <summary>
    /// 设置指令执行后输出详细信息时间命令
    /// </summary>
    private static readonly string CommandExecutionInfoTimeCommand = "commandexecutioninfotime";

    /// <summary>
    /// 所有配置命令的数组
    /// </summary>
    private static readonly string[] AllCommands =
    [
        JavaXmsCommand,
        JavaXmxCommand,
        WaitMinutesCommand,
        CommandExecutionInfoTimeCommand
    ];

    /// <summary>
    /// 检查输入是否为配置命令
    /// </summary>
    /// <param name="input">用户输入</param>
    /// <returns>如果是配置命令则返回true, 否则返回false</returns>
    public static bool IsConfigCommand(string input)
    {
        return AllCommands.Any(command => input.StartsWith(command, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 处理配置命令
    /// </summary>
    /// <param name="input">用户输入</param>
    public static void HandleConfigCommand(string input)
    {
        // 提取命令和参数
        var parts = input.Split(' ', 2);
        var command = parts[0].Trim().ToLower(System.Globalization.CultureInfo.InvariantCulture);
        var value = "";
        var hasValue = parts.Length > 1;
        if (hasValue)
        {
            value = parts[1].Trim();
        }

        // 根据命令处理不同的配置
        switch (command)
        {
            case var cmd when cmd.StartsWith(JavaXmsCommand, StringComparison.OrdinalIgnoreCase):
                if (hasValue)
                {
                    if (MyRegex().IsMatch(value))
                    {
                        Config.Data.JavaXms = value;
                        Config.SaveConfig();
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                        IOMethods.WriteColorMessage($"JavaXms 参数已设置为: {Config.Data.JavaXms}\n", ConsoleColor.Green);
                    }
                    else
                    {
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage("无效的 JavaXms 参数格式, 请使用正数后跟 M 或 G\n", ConsoleColor.Red);
                    }
                }
                else
                {
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                    IOMethods.WriteColorMessage("当前 JavaXms 参数: " + Config.Data.JavaXms + "\n", ConsoleColor.Green);
                }
                break;

            case var cmd when cmd.StartsWith(JavaXmxCommand, StringComparison.OrdinalIgnoreCase):
                if (hasValue)
                {
                    if (MyRegex().IsMatch(value))
                    {
                        Config.Data.JavaXmx = value;
                        Config.SaveConfig();
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                        IOMethods.WriteColorMessage($"JavaXmx 参数已设置为: {Config.Data.JavaXmx}\n", ConsoleColor.Green);
                    }
                    else
                    {
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage("无效的 JavaXmx 参数格式, 请使用正数后跟 M 或 G\n", ConsoleColor.Red);
                    }
                }
                else
                {
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                    IOMethods.WriteColorMessage("当前 JavaXmx 参数: " + Config.Data.JavaXmx + "\n", ConsoleColor.Green);
                }
                break;

            case var cmd when cmd.StartsWith(WaitMinutesCommand, StringComparison.OrdinalIgnoreCase):
                if (hasValue)
                {
                    if (float.TryParse(value, out var waitMinutes) && waitMinutes > 0)
                    {
                        Config.Data.WaitMinutes = waitMinutes;
                        Config.SaveConfig();
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                        IOMethods.WriteColorMessage($"等待时间已设置为: {Config.Data.WaitMinutes} 分钟\n", ConsoleColor.Green);
                    }
                    else
                    {
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage("无效的等待时间格式, 请使用正数\n", ConsoleColor.Red);
                    }
                }
                else
                {
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                    IOMethods.WriteColorMessage("当前等待时间: " + Config.Data.WaitMinutes + " 分钟\n", ConsoleColor.Green);
                }
                break;

            case var cmd when cmd.StartsWith(CommandExecutionInfoTimeCommand, StringComparison.OrdinalIgnoreCase):
                if (hasValue)
                {
                    if (int.TryParse(value, out var executionTime) && executionTime > 0)
                    {
                        Config.Data.CommandExecutionInfoTime = executionTime;
                        Config.SaveConfig();
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                        IOMethods.WriteColorMessage($"指令执行后输出详细信息时间已设置为: {Config.Data.CommandExecutionInfoTime} 秒\n", ConsoleColor.Green);
                    }
                    else
                    {
                        IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                        IOMethods.WriteColorMessage("无效的指令执行后输出详细信息时间格式, 请使用正整数\n", ConsoleColor.Red);
                    }
                }
                else
                {
                    IOMethods.ShowCurrentTimestamp(ConsoleColor.Green);
                    IOMethods.WriteColorMessage("当前指令执行后输出详细信息时间: " + Config.Data.CommandExecutionInfoTime + " 秒\n", ConsoleColor.Green);
                }
                break;

            default:
                IOMethods.ShowCurrentTimestamp(ConsoleColor.Red);
                IOMethods.WriteColorMessage("未知的配置命令\n", ConsoleColor.Red);
                break;
        }
    }

    [GeneratedRegex(@"^\d+[MG]$")]
    private static partial Regex MyRegex();
}