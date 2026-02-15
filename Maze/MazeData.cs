using System.Text.RegularExpressions;

namespace Maze;

/// <summary>
/// 迷宫数据类, 提供迷宫的保存和加载功能
/// </summary>
internal static partial class MazeData
{
    /// <summary>
    /// 静态构造函数, 初始化迷宫数据类
    /// </summary>
    static MazeData()
    {
        // 创建数据存储路径
        try
        {
            if (!Directory.Exists(Constants.DataPath))
            {
                _ = Directory.CreateDirectory(Constants.DataPath);
            }
        }
        catch (Exception ex)
        {
            // 记录异常到日志文件并弹窗提示错误信息
            Methods.LogException(ex);
            _ = MessageBox.Show($"无法创建数据存储路径: {ex.Message}\n错误日志已保存到: {Constants.ErrorFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // 退出应用程序
            Application.Exit();
        }
    }

    /// <summary>
    /// 获取保存到指定目录下的推荐迷宫数据文件名
    /// </summary>
    /// <param name="directory">目录路径</param>
    /// <returns>推荐的文件名, 如果目录不存在则返回null</returns>
    public static string? GetRecommendedMazeDataFileName(string directory)
    {
        // 如果目录不存在, 返回null
        if (!Directory.Exists(directory))
        {
            return null;
        }

        // 获取目录下所有maze(数字).maze格式的文件
        var files = Directory.EnumerateFiles(directory, "maze*.maze");

        // 如果没有找到任何文件, 返回默认的maze1.maze
        if (!files.Any())
        {
            return "maze1.maze";
        }

        // 提取所有出现的数字
        var numbers = new HashSet<int>();
        foreach (var file in files)
        {
            var match = MazeFileRegex().Match(Path.GetFileName(file));
            if (match.Success && int.TryParse(match.Groups[1].Value, out var num))
            {
                _ = numbers.Add(num);
            }
        }

        // 返回未出现的最小数字(除了0)对应的文件名
        for (var i = 1; i <= numbers.Count; ++i)
        {
            if (!numbers.Contains(i))
            {
                return $"maze{i}.maze";
            }
        }

        // 如果所有数字都出现了, 返回下一个数字的文件名
        return $"maze{numbers.Count + 1}.maze";
    }

    /// <summary>
    /// 返回指定目录下以.maze结尾的修改时间最新的迷宫数据文件名
    /// </summary>
    /// <param name="directory">目录路径</param>
    /// <returns>文件名, 如果没有找到或者发生异常则返回null</returns>
    public static string? GetLatestMazeDataFile(string directory)
    {
        // 如果目录不存在, 返回null
        if (!Directory.Exists(directory))
        {
            return null;
        }

        try
        {
            // 使用 LINQ 按修改时间降序排序并取第一个文件
            var latest = Directory.EnumerateFiles(directory, "*.maze").OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
            return Path.GetFileName(latest);
        }
        catch
        {
            // 如果发生异常, 返回null
            return null;
        }
    }

    /// <summary>
    /// 判断一个文件名是否是有效的迷宫数据文件名
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>如果是有效的迷宫数据文件名则返回true, 否则返回false</returns>
    public static bool IsValidMazeDataFileName(string fileName) =>
        // 只要后缀名是.maze即可
        Path.GetExtension(fileName).Equals(".maze", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 异步保存迷宫数据到指定路径
    /// </summary>
    /// <param name="path">保存路径</param>
    /// <param name="mazeData">迷宫数据</param>
    public static async Task Save(string path, GridType[,] mazeData)
    {
        try
        {
            using var writer = new StreamWriter(path);
            for (var row = 0; row < Constants.MazeHeight; ++row)
            {
                for (var col = 0; col < Constants.MazeWidth; ++col)
                {
                    await writer.WriteAsync(mazeData[row, col] switch
                    {
                        GridType.Start => "S",
                        GridType.End => "E",
                        GridType.Obstacle => "X",
                        GridType.Empty => ".",
                        GridType.Path => ".",
                        GridType.FadePath => ".",
                        _ => "."
                    });
                }
                await writer.WriteLineAsync();
            }
        }
        catch (Exception ex)
        {
            Methods.LogException(ex);
            _ = MessageBox.Show($"保存迷宫数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 从指定路径异步加载迷宫数据
    /// </summary>
    /// <param name="path">加载路径</param>
    /// <returns>返回加载的迷宫数据</returns>
    /// <exception cref="FormatException">迷宫数据格式不正确</exception>
    public static async Task<GridType[,]?> Load(string path)
    {
        var mazeData = new GridType[Constants.MazeHeight, Constants.MazeWidth];
        try
        {
            using var reader = new StreamReader(path);
            for (var row = 0; row < Constants.MazeHeight; ++row)
            {
                var line = await reader.ReadLineAsync();
                if (line is null || line.Length != Constants.MazeWidth)
                {
                    throw new FormatException("迷宫数据格式不正确");
                }
                for (var col = 0; col < Constants.MazeWidth; ++col)
                {
                    mazeData[row, col] = line[col] switch
                    {
                        'S' => GridType.Start,
                        'E' => GridType.End,
                        'X' => GridType.Obstacle,
                        _ => GridType.Empty
                    };
                }
            }
            return mazeData;
        }
        catch (Exception ex)
        {
            Methods.LogException(ex);
            _ = MessageBox.Show($"加载迷宫数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // 如果加载失败, 返回一个空的迷宫数据
            return null;
        }
    }

    [GeneratedRegex(@"^maze(\d+)\.maze$", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex MazeFileRegex();
}
