using System.Text.Json;

namespace MineClearance
{
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
                string json = File.ReadAllText(Constants.DataFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var gameResults = JsonSerializer.Deserialize<List<GameResult>>(json, _jsonOptions);

                    if (gameResults != null)
                    {
                        _gameResults.AddRange(gameResults);
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
                MessageBox.Show($"初始化游戏数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存游戏结果到数据文件
        /// </summary>
        private static async Task SaveGameResultsAsync()
        {
            try
            {
                // 将游戏结果序列化为 JSON 字符串
                string json = JsonSerializer.Serialize(_gameResults, _jsonOptions);

                // 异步写入数据文件
                await File.WriteAllTextAsync(Constants.DataFilePath, json);
            }
            catch (Exception ex)
            {
                // 显示错误信息
                MessageBox.Show($"保存游戏结果失败: {ex.Message}");
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
}