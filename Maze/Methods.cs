namespace Maze;

/// <summary>
/// 方法类, 提供一些常用的方法
/// </summary>
internal static class Methods
{
    /// <summary>
    /// 线程安全的随机数生成器
    /// </summary>
    public static Random RandomInstance => Random.Shared;

    /// <summary>
    /// 未知异常类
    /// </summary>
    /// <param name="message">异常消息</param>
    private sealed class UnknownException(string message) : Exception(message);

    /// <summary>
    /// 生成一个在[min, max]范围内的不等于excludes中的任意值的随机整数
    /// </summary>
    /// <param name="min">范围最小值</param>
    /// <param name="max">范围最大值</param>
    /// <param name="excludes">排除的值</param>
    /// <returns>生成的随机整数, 如果无法生成则返回-1</returns>
    public static int Next(int min, int max, List<int> excludes)
    {
        // 如果min大于max或者任一小于0, 则返回-1
        if (min > max || min < 0 || max < 0)
        {
            return -1;
        }

        // 如果min等于max
        if (min == max)
        {
            return excludes.Contains(min) ? -1 : min;
        }

        // 如果排除列表为空, 则直接返回随机数
        if (excludes.Count == 0)
        {
            return RandomInstance.Next(min, max + 1);
        }

        // 获得[min, max]范围内不在排除列表中的所有整数
        var validNumbers = new List<int>();
        for (var i = min; i <= max; i++)
        {
            if (!excludes.Contains(i))
            {
                validNumbers.Add(i);
            }
        }

        // 如果没有有效数字, 则返回-1
        if (validNumbers.Count == 0)
        {
            return -1;
        }

        // 否则返回一个随机选择的有效数字
        return validNumbers[RandomInstance.Next(validNumbers.Count)];
    }

    /// <summary>
    /// 记录异常到日志文件
    /// </summary>
    /// <param name="ex">要记录的异常</param>
    public static void LogException(Exception ex)
    {
        var log = $"[{DateTime.Now}] {ex}\n";
        try
        {
            if (!Directory.Exists(Constants.DataPath))
            {
                _ = Directory.CreateDirectory(Constants.DataPath);
            }
            File.AppendAllText(Constants.ErrorFilePath, log);
        }
        catch { /* 忽略日志写入异常 */ }
    }

    /// <summary>
    /// 处理未经处理的线程异常
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">线程异常事件参数</param>
    public static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        // 记录异常到日志文件并弹窗提示错误信息
        LogException(e.Exception);
        _ = MessageBox.Show($"发生未处理的线程异常：{e.Exception.Message}\n错误日志已保存到：{Constants.ErrorFilePath}", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

        // 退出应用程序
        Application.Exit();
    }

    /// <summary>
    /// 处理未经处理的应用程序域异常
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">应用程序域异常事件参数</param>
    public static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // 记录异常到日志文件并弹窗提示错误信息
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex);
            _ = MessageBox.Show($"发生未处理的应用程序域异常：{ex.Message}\n错误日志已保存到：{Constants.ErrorFilePath}", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            LogException(new UnknownException("未知错误"));
            _ = MessageBox.Show($"发生未处理的应用程序域异常：未知错误\n错误日志已保存到：{Constants.ErrorFilePath}", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 退出应用程序
        Application.Exit();
    }

    /// <summary>
    /// 创建退出按钮
    /// </summary>
    /// <returns>返回一个退出按钮</returns>
    public static Button CreateExitButton()
    {
        var buttonX = Constants.MainFormWidth - (int)(10 * Constants.DpiScale);
        buttonX -= Constants.ButtonWidth + Constants.ButtonSpacing;
        return new()
        {
            Text = "退出",
            Size = new(Constants.ButtonWidth, Constants.ButtonHeight),
            Location = new(buttonX, Constants.ButtonY),
            BackColor = Color.Red,
            FlatStyle = FlatStyle.Flat
        };
    }

    /// <summary>
    /// 创建返回按钮
    /// </summary>
    /// <returns>返回一个返回按钮</returns>
    public static Button CreateBackButton()
    {
        var buttonX = Constants.MainFormWidth - (int)(10 * Constants.DpiScale);
        buttonX -= (Constants.ButtonWidth + Constants.ButtonSpacing) * 2;
        return new()
        {
            Text = "返回",
            Size = new(Constants.ButtonWidth, Constants.ButtonHeight),
            Location = new(buttonX, Constants.ButtonY),
            BackColor = Color.LightCoral,
            FlatStyle = FlatStyle.Flat
        };
    }

    /// <summary>
    /// 创建取消当前搜索按钮
    /// </summary>
    /// <returns>返回一个取消当前搜索按钮</returns>
    public static Button CreateCancelSearchButton(string text = "取消当前搜索")
    {
        var buttonX = Constants.MainFormWidth - (int)(10 * Constants.DpiScale);
        buttonX -= (Constants.ButtonWidth + Constants.ButtonSpacing) * 3;
        return new()
        {
            Text = text,
            Size = new(Constants.ButtonWidth, Constants.ButtonHeight),
            Location = new(buttonX, Constants.ButtonY),
            BackColor = Color.Orange,
            FlatStyle = FlatStyle.Flat
        };
    }

    /// <summary>
    /// 随机生成一个位置
    /// </summary>
    /// <returns>返回一个随机位置</returns>
    public static Position RandomGeneratePosition()
    {
        var row = RandomInstance.Next(0, Constants.MazeHeight);
        var col = RandomInstance.Next(0, Constants.MazeWidth);
        return new(row, col);
    }

    /// <summary>
    /// 随机生成一个位置, 确保该位置与要排除的位置不同, 支持传入的位置为无效位置
    /// </summary>
    /// <param name="exclude">要排除的位置</param>
    /// <returns>返回一个随机位置</returns>
    public static Position RandomGeneratePosition(Position exclude)
    {
        if (exclude == Position.Invalid)
        {
            return RandomGeneratePosition();
        }

        var position = RandomGeneratePosition();
        while (position == exclude)
        {
            position = RandomGeneratePosition();
        }
        return position;
    }

    /// <summary>
    /// 从所给的位置列表中随机选择一个位置
    /// </summary>
    /// <param name="positions">位置列表</param>
    /// <returns>返回一个随机选择的位置</returns>
    /// <exception cref="ArgumentException">如果位置列表为空</exception>
    public static Position RandomSelectPosition(List<Position> positions)
    {
        if (positions.Count == 0)
        {
            throw new ArgumentException("位置列表不能为空", nameof(positions));
        }

        var index = RandomInstance.Next(0, positions.Count);
        return positions[index];
    }

    /// <summary>
    /// 获得随机打乱的方向数组
    /// </summary>
    /// <returns>返回一个随机打乱的方向数组</returns>
    public static Direction[] GetShuffledDirections()
    {
        var directions = Enum.GetValues<Direction>();
        RandomInstance.Shuffle(directions);
        return directions;
    }

    /// <summary>
    /// 根据当前位置和终点位置获取排列好的方向
    /// </summary>
    /// <param name="current">当前格子位置</param>
    /// <param name="end">终点格子位置</param>
    /// <returns>排列好的方向数组</returns>
    /// <exception cref="ArgumentException">如果当前位置和终点位置相同</exception>
    public static Direction[] GetOrderedDirections(Position current, Position end)
    {
        // 确保当前位置和终点位置不相同
        if (current == end)
        {
            throw new ArgumentException("当前位置和终点位置不能相同");
        }

        // 如果当前位置和终点位置在同一行或同一列
        if (current.Row == end.Row)
        {
            var direction = end.Col < current.Col ? Direction.Left : Direction.Right;
            return GetOrderedDirections(direction, direction);
        }
        if (current.Col == end.Col)
        {
            var direction = end.Row < current.Row ? Direction.Up : Direction.Down;
            return GetOrderedDirections(direction, direction);
        }

        // 如果当前位置和终点位置不在同一行或同一列, 计算行列差
        var rowDiff = end.Row - current.Row;
        var colDiff = end.Col - current.Col;

        // 判断上下方向和左右方向
        var verticalDirection = rowDiff > 0 ? Direction.Down : Direction.Up;
        var horizontalDirection = colDiff > 0 ? Direction.Right : Direction.Left;

        // 根据行列差的绝对值判断优先方向
        return Math.Abs(rowDiff) <= Math.Abs(colDiff)
            ? GetOrderedDirections(horizontalDirection, verticalDirection)
            : GetOrderedDirections(verticalDirection, horizontalDirection);
    }

    /// <summary>
    /// 获取根据第一优先方向和第二优先方向排列的方向数组, 二者必须是相同或者相邻方向
    /// </summary>
    /// <param name="firstPriority">第一优先方向</param>
    /// <param name="secondPriority">第二优先方向</param>
    /// <returns>排列的方向数组</returns>
    /// <exception cref="ArgumentException">如果第二优先方向不是第一优先方向的相邻方向</exception>
    private static Direction[] GetOrderedDirections(Direction firstPriority, Direction secondPriority)
    {
        // 枚举总数
        var dirCount = Enum.GetValues<Direction>().Length;

        // 创建一个方向数组
        var orderedDirs = new Direction[dirCount];

        // 获取第一优先方向的索引
        var firstIndex = (int)firstPriority;

        // 获取第二优先方向的索引
        var secondIndex = (int)secondPriority;

        // 如果第一优先方向和第二优先方向相同, 则以第一优先方向为首顺时针排列
        if (firstIndex == secondIndex)
        {
            for (var i = 0; i < dirCount; i++)
            {
                orderedDirs[i] = (Direction)((firstIndex + i) % dirCount);
            }
        }
        else
        {
            // 否则, 判断第二优先方向的索引与第一优先方向是否相邻
            if ((firstIndex + 1) % dirCount == secondIndex)
            {
                // 第二优先方向是第一优先方向的顺时针相邻方向
                for (var i = 0; i < dirCount; ++i)
                {
                    orderedDirs[i] = (Direction)((firstIndex + i) % dirCount);
                }
            }
            else if ((firstIndex - 1 + dirCount) % dirCount == secondIndex)
            {
                // 第二优先方向是第一优先方向的逆时针相邻方向
                for (var i = 0; i < dirCount; ++i)
                {
                    orderedDirs[i] = (Direction)((firstIndex - i + dirCount) % dirCount);
                }
            }
            else
            {
                // 如果不相邻, 则抛出异常
                throw new ArgumentException("第二优先方向必须与第一优先方向相邻");
            }
        }

        return orderedDirs;
    }
}
