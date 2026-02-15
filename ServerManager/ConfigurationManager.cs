using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerManager;

/// <summary>
/// 配置管理类, 用于读取和存储配置信息
/// </summary>
public static class ConfigurationManager
{
    /// <summary>
    /// 配置文件路径
    /// </summary>
    private static readonly string ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "config.json");

    /// <summary>
    /// Json 序列化选项, 缓存以避免重复创建
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// 当前服务器设置
    /// </summary>
    public static ServerSettings Settings { get; private set; } = new();

    /// <summary>
    /// 加载配置文件
    /// </summary>
    public static void Load()
    {
        IOMethods.WriteCurrentTimestamp(ConsoleColor.Yellow);
        IOMethods.WriteColorMessage("正在加载配置文件...", ConsoleColor.Yellow);

        if (File.Exists(ConfigFilePath))
        {
            try
            {
                Settings = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllText(ConfigFilePath), JsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                Settings = new();
                IOMethods.WriteErrorMessage("配置文件加载失败: ", ex);
                return;
            }
        }
        else
        {
            Settings = new();
        }

        IOMethods.WriteCurrentTimestamp(ConsoleColor.Green);
        IOMethods.WriteColorMessage("配置文件加载完成", ConsoleColor.Green);
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public static void Save() => File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(Settings, JsonOptions));
}
