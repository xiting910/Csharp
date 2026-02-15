using System.Text.Json;
using System.Text.Json.Serialization;

namespace WindowsUpdateManager;

/// <summary>
/// 设置类, 用于存储和管理设置
/// </summary>
internal static class Settings
{
    /// <summary>
    /// JSON序列化选项 - 缓存以避免重复创建
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// 配置信息
    /// </summary>
    public static Config Config { get; private set; } = new();

    /// <summary>
    /// 加载配置数据
    /// </summary>
    public static void LoadConfig()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                _ = Directory.CreateDirectory(Constants.DataPath);
            }

            // 如果配置文件不存在, 则创建一个空的文件
            if (!File.Exists(Constants.ConfigFilePath))
            {
                // 创建一个新的配置对象
                Config = new();

                // 保存配置数据
                SaveConfig();
                return;
            }

            // 读取配置文件内容
            var json = File.ReadAllText(Constants.ConfigFilePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    // 尝试反序列化为 Config 对象
                    Config = JsonSerializer.Deserialize<Config>(json, _jsonOptions) ?? new();
                }
                catch (JsonException ex)
                {
                    // 如果反序列化失败, 显示错误信息并创建一个新的配置对象
                    _ = MessageBox.Show($"加载配置数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Config = new();
                }
            }
        }
        catch (Exception ex)
        {
            // 显示错误信息
            _ = MessageBox.Show($"加载配置数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 修改配置数据
    /// </summary>
    /// <param name="modifyFunc">修改方法</param>
    public static void ModifyConfig(Func<Config, Config> modifyFunc)
    {
        Config = modifyFunc.Invoke(Config);
        SaveConfig();
    }

    /// <summary>
    /// 保存配置数据
    /// </summary>
    private static void SaveConfig()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                _ = Directory.CreateDirectory(Constants.DataPath);
            }

            // 将配置对象序列化为 JSON 字符串
            var json = JsonSerializer.Serialize(Config, _jsonOptions);

            // 将 JSON 字符串写入配置文件
            File.WriteAllText(Constants.ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            // 显示错误信息
            _ = MessageBox.Show($"保存配置数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
