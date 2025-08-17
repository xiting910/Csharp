using System.Text.Json;

namespace FabricServerManager;

/// <summary>
/// 常量类, 用于存放一些常量
/// </summary>
public static class Constants
{
    /// <summary>
    /// 配置保存文件名
    /// </summary>
    public static readonly string ConfigFileName = "config.json";

    /// <summary>
    /// 服务器数据文件, 保存程序之前运行时查找的服务器文件列表和上次运行的服务器文件
    /// </summary>
    public static readonly string ServerDataFile = "server.json";

    /// <summary>
    /// 服务器文件名开头
    /// </summary>
    public static readonly string ServerFilePrefix = "fabric-server-";

    /// <summary>
    /// 服务器文件名后缀
    /// </summary>
    public static readonly string ServerFileSuffix = ".jar";

    /// <summary>
    /// 服务器启动成功语句
    /// </summary>
    public static readonly string ServerStartSuccessMessage = "Lithium Cached BlockState Flags are disabled!";

    /// <summary>
    /// 玩家加入服务器消息
    /// </summary>
    public static readonly string PlayerJoinMessage = "joined the game";

    /// <summary>
    /// 玩家离开服务器消息
    /// </summary>
    public static readonly string PlayerLeaveMessage = "left the game";

    /// <summary>
    /// 发言消息
    /// </summary>
    public static readonly string ChatMessage = "say ";

    /// <summary>
    /// 指令执行消息
    /// </summary>
    public static readonly string CommandMessage = "[Command: /";

    /// <summary>
    /// 指令错误消息
    /// </summary>
    public static readonly string CommandErrorMessage = "<--[HERE]";

    /// <summary>
    /// Java Xms 参数默认值
    /// </summary>
    public static readonly string DefaultJavaXms = "1G";

    /// <summary>
    /// Java Xmx 参数默认值
    /// </summary>
    public static readonly string DefaultJavaXmx = "6G";

    /// <summary>
    /// 停止命令
    /// </summary>
    public static readonly string StopCommand = "stop";

    /// <summary>
    /// 等待时间（分钟）默认值
    /// </summary>
    public static readonly float DefaultWaitMinutes = 1.0f;

    /// <summary>
    /// 指令执行后输出详细信息时间（秒）默认值
    /// </summary>
    public static readonly int DefaultCommandExecutionInfoTime = 1;

    /// <summary>
    /// JSON序列化选项 - 缓存以避免重复创建
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
}