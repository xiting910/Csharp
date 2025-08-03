using System.Text.Json;

namespace MineClearance;

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
        WriteIndented = true
    };

    /// <summary>
    /// 所有游戏结果列表
    /// </summary>
    private static readonly List<GameResult> _gameResults = [];

    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    public static void Initialize()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                Directory.CreateDirectory(Constants.DataPath);

                // 创建数据文件
                using (File.Create(Constants.DataFilePath)) { }
                return;
            }

            // 如果数据文件不存在, 则创建一个空的文件
            if (!File.Exists(Constants.DataFilePath))
            {
                using (File.Create(Constants.DataFilePath)) { }
                return;
            }

            // 读取数据文件内容
            var json = File.ReadAllText(Constants.DataFilePath);
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
                    }
                }
                catch (JsonException)
                {
                    // 如果反序列化为 GameData 对象失败, 可能是旧版本数据格式, 直接处理为 GameResult 列表
                    var gameResults = JsonSerializer.Deserialize<List<GameResult>>(json, _jsonOptions);

                    // 将所有"地狱"难度的游戏结果转换为"自定义"难度
                    gameResults = gameResults?.Select(result =>
                    {
                        if (result.Difficulty == DifficultyLevel.Hell)
                        {
                            return new GameResult(
                                DifficultyLevel.Custom,
                                result.StartTime,
                                result.Duration,
                                result.IsWin,
                                result.Completion,
                                result.BoardWidth,
                                result.BoardHeight,
                                result.MineCount);
                        }
                        return result;
                    }).ToList() ?? [];

                    // 更新游戏结果列表
                    _gameResults.AddRange(gameResults);

                    // 保存转换后的数据
                    SaveGameResultsAsync().Wait();
                }
            }
            else
            {
                // 如果文件内容为空, 初始化为空列表
                _gameResults.Clear();
            }
        }
        catch (Exception ex)
        {
            // 显示错误信息
            MessageBox.Show($"初始化游戏数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 保存游戏结果到数据文件
    /// </summary>
    private static async Task SaveGameResultsAsync()
    {
        try
        {
            // 要保存的数据
            var data = new GameData(DateTime.Now, _gameResults, 1);

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
    /// 将游戏结果按指定显示模式排序
    /// </summary>
    /// <param name="displayMode">显示模式</param>
    /// <returns>排序后的游戏结果列表</returns>
    public static List<GameResult> GetSortedGameResults(RankingDisplayMode displayMode)
    {
        if (displayMode == RankingDisplayMode.ByStartTime)
        {
            return [.. _gameResults.OrderByDescending(result => result.StartTime)];
        }
        else if (displayMode == RankingDisplayMode.ByDuration)
        {
            return [.. _gameResults
                    .Where(result => result.Difficulty != DifficultyLevel.Custom && result.IsWin)
                    .OrderByDescending(result => result.Difficulty)
                    .ThenBy(result => result.Duration)];
        }
        else
        {
            // 默认模式获取所有游戏结果
            return _gameResults;
        }
    }
}