using System.Text.Json;
using System.Text.Json.Serialization;
using MineClearance.Models;
using MineClearance.Utilities;

namespace MineClearance.Services;

/// <summary>
/// 数据类, 记录和控制所有历史游戏数据
/// </summary>
public static class Datas
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
    /// 所有游戏结果列表
    /// </summary>
    public static readonly List<GameResult> _gameResults = [];

    /// <summary> 
    /// 所有游戏结果的只读列表
    /// </summary>
    public static IReadOnlyList<GameResult> GameResults => _gameResults.AsReadOnly();

    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    public static async Task Initialize()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                Directory.CreateDirectory(Constants.DataPath);

                // 创建数据文件
                await using (File.Create(Constants.DataFilePath)) { }
                return;
            }

            // 如果数据文件不存在, 则创建一个空的文件
            if (!File.Exists(Constants.DataFilePath))
            {
                await using (File.Create(Constants.DataFilePath)) { }
                return;
            }

            // 读取数据文件内容
            var json = await File.ReadAllTextAsync(Constants.DataFilePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    // 尝试反序列化为 GameData 对象
                    var gameData = JsonSerializer.Deserialize<GameData>(json, _jsonOptions);
                    if (gameData != null)
                    {
                        // 更新游戏结果列表
                        _gameResults.AddRange(gameData.GameResults);

                        // 如果数据版本低于当前版本, 则进行数据升级
                        if (gameData.Version < Constants.CurrentDataVersion)
                        {
                            await SaveGameResultsAsync();
                        }
                    }
                }
                catch (JsonException)
                {
                    // 如果反序列化为 GameData 对象失败, 可能是旧版本数据格式
                    // 将文件出现的所有"Difficulty": 3替换为"Difficulty": 4
                    var updatedJson = json.Replace("\"Difficulty\": 3", "\"Difficulty\": 4");

                    // 尝试反序列化为 List<GameResult> 对象
                    var oldGameResults = JsonSerializer.Deserialize<List<GameResult>>(updatedJson, _jsonOptions);

                    // 更新游戏结果列表
                    if (oldGameResults != null)
                    {
                        _gameResults.AddRange(oldGameResults);
                    }

                    // 保存更新后的数据
                    await SaveGameResultsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            // 显示错误信息
            MessageBox.Show($"初始化游戏数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 添加游戏结果
    /// </summary>
    /// <param name="result">游戏结果</param>
    public static async Task AddGameResultAsync(GameResult result)
    {
        _gameResults.Add(result);
        await SaveGameResultsAsync();
    }

    /// <summary>
    /// 清空游戏结果
    /// </summary>
    public static async Task ClearGameResultsAsync()
    {
        _gameResults.Clear();
        await SaveGameResultsAsync();
    }

    /// <summary>
    /// 保存游戏结果到数据文件
    /// </summary>
    private static async Task SaveGameResultsAsync()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                Directory.CreateDirectory(Constants.DataPath);
            }

            // 要保存的数据
            var data = new GameData(DateTime.Now, [.. GameResults], Constants.CurrentDataVersion);

            // 异步序列化 data 到游戏历史记录文件
            await using var stream = File.Create(Constants.DataFilePath);
            await JsonSerializer.SerializeAsync(stream, data, _jsonOptions);
        }
        catch (Exception ex)
        {
            // 显示错误信息
            MessageBox.Show($"保存游戏结果失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}