namespace ServerManager;

/// <summary>
/// 服务器设置类, 用于存放服务器相关的设置
/// </summary>
public sealed class ServerSettings
{
    /// <summary>
    /// 服务器文件路径
    /// </summary>
    public string ServerFilePath { get; set; } = string.Empty;

    /// <summary>
    /// 是否显示 Info 级别日志锁
    /// </summary>
    private readonly Lock _showInfoLogsLock = new();

    /// <summary>
    /// 是否显示 Info 级别日志
    /// </summary>
    public bool ShowInfoLogs
    {
        get { lock (_showInfoLogsLock) { return field; } }
        set { lock (_showInfoLogsLock) { field = value; } }
    }

    /// <summary>
    /// 是否显示 Warn 级别日志锁
    /// </summary>
    private readonly Lock _showWarnLogsLock = new();

    /// <summary>
    /// 是否显示 Warn 级别日志
    /// </summary>
    public bool ShowWarnLogs
    {
        get { lock (_showWarnLogsLock) { return field; } }
        set { lock (_showWarnLogsLock) { field = value; } }
    }

    /// <summary>
    /// 是否自动关闭服务器锁
    /// </summary>
    private readonly Lock _autoCloseServerLock = new();

    /// <summary>
    /// 是否自动关闭服务器
    /// </summary>
    public bool AutoCloseServer
    {
        get { lock (_autoCloseServerLock) { return field; } }
        set { lock (_autoCloseServerLock) { field = value; } }
    } = true;

    /// <summary>
    /// 等待时间锁
    /// </summary>
    private readonly Lock _waitTimeLock = new();

    /// <summary>
    /// 等待时间
    /// </summary>
    public TimeSpan WaitTime
    {
        get { lock (_waitTimeLock) { return field; } }
        set { lock (_waitTimeLock) { field = value; } }
    } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 指令执行后输出详细信息时间锁
    /// </summary>
    private readonly Lock _commandExecutionInfoTimeLock = new();

    /// <summary>
    /// 指令执行后输出详细信息时间
    /// </summary>
    public TimeSpan CommandExecutionInfoTime
    {
        get { lock (_commandExecutionInfoTimeLock) { return field; } }
        set { lock (_commandExecutionInfoTimeLock) { field = value; } }
    } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Java Xms 参数
    /// </summary>
    public string JavaXms { get; init; } = "1G";

    /// <summary>
    /// Java Xmx 参数
    /// </summary>
    public string JavaXmx { get; init; } = "4G";

    /// <summary>
    /// 服务器启动成功语句
    /// </summary>
    public string ServerStartSuccessMessage { get; init; } = "Lithium Cached BlockState Flags are disabled!";

    /// <summary>
    /// 玩家加入服务器消息
    /// </summary>
    public string PlayerJoinMessage { get; init; } = "joined the game";

    /// <summary>
    /// 玩家离开服务器消息
    /// </summary>
    public string PlayerLeaveMessage { get; init; } = "left the game";

    /// <summary>
    /// 发言消息
    /// </summary>
    public string ChatMessage { get; init; } = "say ";

    /// <summary>
    /// 指令执行消息起始标志
    /// </summary>
    public string CommandBeginMessage { get; init; } = "[Command: /";

    /// <summary>
    /// 指令执行消息结束标志
    /// </summary>
    public string CommandEndMessage { get; init; } = "]";

    /// <summary>
    /// 指令错误消息
    /// </summary>
    public string CommandErrorMessage { get; init; } = "<--[HERE]";

    /// <summary>
    /// 停止命令
    /// </summary>
    public string StopCommand { get; init; } = "stop";
}
