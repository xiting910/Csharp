using System.Runtime.CompilerServices;

namespace Maze;

/// <summary>
/// 表示迷宫面板的类
/// </summary>
internal sealed class MazePanel
{
    /// <summary>
    /// 格子状态改变事件
    /// </summary>
    public event Action<Position, GridType>? OnGridTypeChanged;

    /// <summary>
    /// 当前操作状态
    /// </summary>
    public OperationStatus CurrentStatus { get; set; }

    /// <summary>
    /// 迷宫面板
    /// </summary>
    public DoubleBufferedPanel Panel { get; private set; }

    /// <summary>
    /// 鼠标是否被按下
    /// </summary>
    private volatile bool _isMouseDown;

    /// <summary>
    /// 记录鼠标被哪个键按下
    /// </summary>
    private volatile MouseButtons _mouseButton;

    /// <summary>
    /// 上次的目录
    /// </summary>
    private string _lastDirectory;

    /// <summary>
    /// 构造函数, 初始化迷宫面板
    /// </summary>
    public MazePanel()
    {
        // 设置上次的目录为默认目录
        _lastDirectory = Constants.DataPath;

        // 设置当前状态为默认状态
        CurrentStatus = OperationStatus.Default;

        // 初始化迷宫面板
        Panel = new()
        {
            Size = new(Constants.MazeWidth * Constants.GridSize, Constants.MazeHeight * Constants.GridSize),
            Location = new(0, Constants.MainInfoPanelHeight),
            BackColor = Color.White
        };

        // 添加鼠标按下事件
        Panel.MouseDown += MouseDown;

        // 添加鼠标移动事件
        Panel.MouseMove += MouseMove;

        // 添加鼠标释放事件
        Panel.MouseUp += MouseUp;

        // 绘制格子
        Panel.Paint += (s, e) =>
        {
            // 获取当前绘图区域
            var clip = e.ClipRectangle;
            var rowStart = Math.Max(0, clip.Top / Constants.GridSize);
            var rowEnd = Math.Min(Constants.MazeHeight - 1, clip.Bottom / Constants.GridSize);
            var colStart = Math.Max(0, clip.Left / Constants.GridSize);
            var colEnd = Math.Min(Constants.MazeWidth - 1, clip.Right / Constants.GridSize);

            // 遍历指定区域的格子并绘制
            for (var row = rowStart; row <= rowEnd; ++row)
            {
                for (var col = colStart; col <= colEnd; ++col)
                {
                    DrawGrid(e.Graphics, row, col, Maze.Grids[row, col]);
                }
            }
        };
    }

    /// <summary>
    /// 更改指定位置的格子类型
    /// </summary>
    /// <param name="position">更改的格子位置</param>
    public void ChangeGridType(Position position)
    {
        // 如果位置无效, 直接返回
        if (position == Position.Invalid)
        {
            return;
        }

        // 如果不是UI线程, 使用Invoke切回UI线程
        if (Panel.InvokeRequired)
        {
            Panel.Invoke(new Action(() => ChangeGridType(position)));
            return;
        }

        // 重绘指定位置的格子
        Panel.Invalidate(new Rectangle(position.Col * Constants.GridSize, position.Row * Constants.GridSize, Constants.GridSize, Constants.GridSize));
    }

    /// <summary>
    /// 重置迷宫
    /// </summary>
    public async Task ResetMaze()
    {
        // 重置当前状态为默认状态
        CurrentStatus = OperationStatus.Default;

        // 通知Maze重置
        await Maze.ResetMaze();

        // 重绘面板
        Panel.Invalidate();
    }

    /// <summary>
    /// 全部设为障碍物
    /// </summary>
    public async Task SetAllObstacles()
    {
        // 通知Maze设置所有格子为障碍物
        await Maze.SetAllObstacles();

        // 重绘面板
        Panel.Invalidate();
    }

    /// <summary>
    /// 清除所有障碍物
    /// </summary>
    public async Task ClearAllObstacles()
    {
        // 通知Maze清除所有障碍物
        await Maze.ResetMaze(false);

        // 重绘面板
        Panel.Invalidate();
    }

    /// <summary>
    /// 异步保存当前迷宫
    /// </summary>
    public async Task SaveMaze()
    {
        // 如果当前迷宫全部为空, 提示用户
        if (Maze.Grids.Cast<GridType>().All(g => g == GridType.Empty))
        {
            _ = MessageBox.Show("当前迷宫全部为空, 无法保存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 创建保存文件对话框
        var saveFileDialog = new SaveFileDialog
        {
            Title = "保存迷宫",
            DefaultExt = "maze",
            InitialDirectory = _lastDirectory,
            Filter = "Maze Files (*.maze)|*.maze|All Files (*.*)|*.*",
            FileName = MazeData.GetRecommendedMazeDataFileName(_lastDirectory)
        };

        // 如果用户选择了文件路径
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            // 获取文件路径
            var filePath = saveFileDialog.FileName;

            // 更新上次目录
            _lastDirectory = Path.GetDirectoryName(filePath) ?? Constants.DataPath;

            // 如果文件名为空, 提示用户
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _ = MessageBox.Show("文件名不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 如果文件名不是以.maze结尾, 提示用户
            if (!MazeData.IsValidMazeDataFileName(filePath))
            {
                _ = MessageBox.Show("文件名必须以.maze结尾", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 保存迷宫数据
            await MazeData.Save(filePath, Maze.Grids);
        }
    }

    /// <summary>
    /// 异步加载迷宫
    /// </summary>
    public async Task LoadMaze()
    {
        // 创建加载文件对话框
        var openFileDialog = new OpenFileDialog
        {
            Title = "加载迷宫",
            DefaultExt = "maze",
            InitialDirectory = _lastDirectory,
            Filter = "Maze Files (*.maze)|*.maze|All Files (*.*)|*.*",
            FileName = MazeData.GetLatestMazeDataFile(_lastDirectory)
        };

        // 如果用户选择了文件路径
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            // 获取文件路径
            var filePath = openFileDialog.FileName;

            // 更新上次目录
            _lastDirectory = Path.GetDirectoryName(filePath) ?? Constants.DataPath;

            // 如果文件不存在, 提示用户
            if (!File.Exists(filePath))
            {
                _ = MessageBox.Show("指定的文件不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 如果文件名不是以.maze结尾, 提示用户
            if (!MazeData.IsValidMazeDataFileName(filePath))
            {
                _ = MessageBox.Show("指定的文件名必须以.maze结尾", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 加载迷宫数据
            var mazeData = await MazeData.Load(filePath);

            // 更新迷宫格子
            if (mazeData is not null)
            {
                await UpdateGrids(mazeData);
            }
        }
    }

    /// <summary>
    /// 根据指定的生成算法随机生成迷宫
    /// </summary>
    /// <param name="algorithm">生成算法</param>
    public async Task GenerateMaze(GenerationAlgorithm algorithm)
    {
        // 清除所有障碍物
        await ClearAllObstacles();

        // 如果是递归分割算法, 起点和终点只能设置在左上角和右下角
        if (algorithm == GenerationAlgorithm.RecursiveDivision)
        {
            ConvertToPosition(new(1, 1), true);
            ConvertToPosition(new(Constants.MazeHeight - 2, Constants.MazeWidth - 2), false);
        }
        else
        {
            // 判断起点和终点是否已设置
            var isStartSet = Maze.StartPosition != Position.Invalid;
            var isEndSet = Maze.EndPosition != Position.Invalid;

            if (!isStartSet)
            {
                // 如果起点未设置, 随机设置一个起点, 确保起点不与终点重合
                Maze.StartPosition = Methods.RandomGeneratePosition(Maze.EndPosition);
                OnGridTypeChanged?.Invoke(Maze.StartPosition, GridType.Start);
                ChangeGridType(Maze.StartPosition);
            }
            if (!isEndSet)
            {
                // 如果终点未设置, 随机设置一个终点, 确保终点不与起点重合
                Maze.EndPosition = Methods.RandomGeneratePosition(Maze.StartPosition);
                OnGridTypeChanged?.Invoke(Maze.EndPosition, GridType.End);
                ChangeGridType(Maze.EndPosition);
            }
        }

        // 等待迷宫生成完成
        await Maze.RandomGenerateMaze(algorithm);
    }

    /// <summary>
    /// 根据GridType数组更新_Grids
    /// </summary>
    /// <param name="gridTypes">GridType数组</param>
    private async Task UpdateGrids(GridType[,] gridTypes)
    {
        // 重置迷宫
        await ResetMaze();

        // 遍历GridType数组, 更新_Grids中的每个格子
        var isError = false;
        await Task.Run(() =>
        {
            // 遍历每个格子并更新_Grids
            for (var row = 0; row < Constants.MazeHeight; ++row)
            {
                for (var col = 0; col < Constants.MazeWidth; ++col)
                {
                    var pos = new Position(row, col);
                    switch (gridTypes[row, col])
                    {
                        case GridType.Empty:
                            break;
                        case GridType.Start:
                            OnGridTypeChanged?.Invoke(pos, GridType.Start);
                            break;
                        case GridType.End:
                            OnGridTypeChanged?.Invoke(pos, GridType.End);
                            break;
                        case GridType.Obstacle:
                            OnGridTypeChanged?.Invoke(pos, GridType.Obstacle);
                            break;
                        case GridType.Path:
                            isError = true;
                            break;
                        case GridType.FadePath:
                            isError = true;
                            break;
                        default:
                            isError = true;
                            return;
                    }
                }
            }
        });

        // 如果有错误, 提示用户并重置迷宫
        if (isError)
        {
            _ = MessageBox.Show("迷宫数据无效", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            await Maze.ResetMaze();
            return;
        }

        // 重绘面板
        Panel.Invalidate();
    }

    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    private void MouseDown(object? sender, MouseEventArgs e)
    {
        // 设置鼠标按下状态
        _isMouseDown = true;
        _mouseButton = e.Button;

        // 获取鼠标位置
        if (sender is DoubleBufferedPanel)
        {
            // 获取鼠标位置对应的格子坐标
            var pos = GetGridPositionAtMousePosition(e.Location);

            // 如果鼠标位置在格子上, 触发相应的左键或右键操作
            if (pos != Position.Invalid)
            {
                if (_mouseButton == MouseButtons.Left)
                {
                    GridLeft(pos);
                }
                else if (_mouseButton == MouseButtons.Right)
                {
                    GridRight(pos);
                }
            }
        }
    }

    /// <summary>
    /// 处理鼠标移动事件
    /// </summary>
    private void MouseMove(object? sender, MouseEventArgs e)
    {
        // 获取鼠标位置
        if (_isMouseDown && sender is DoubleBufferedPanel panel)
        {
            var mousePosition = panel.PointToClient(Cursor.Position);

            // 获取鼠标位置对应的格子按钮
            var pos = GetGridPositionAtMousePosition(mousePosition);

            // 如果鼠标位置在格子上, 触发相应的左键或右键操作
            if (pos != Position.Invalid)
            {
                if (_mouseButton == MouseButtons.Left)
                {
                    GridLeft(pos);
                }
                else if (_mouseButton == MouseButtons.Right)
                {
                    GridRight(pos);
                }
            }
        }
    }

    /// <summary>
    /// 处理鼠标释放事件
    /// </summary>
    private void MouseUp(object? sender, MouseEventArgs e)
    {
        // 重置鼠标状态
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
    }

    /// <summary>
    /// 根据鼠标当前位置返回所在的格子位置
    /// </summary>
    /// <param name="mousePosition">鼠标位置</param>
    /// <returns>对应的格子位置, 如果不在任何格子上则返回无效位置</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Position GetGridPositionAtMousePosition(Point mousePosition)
    {
        // 计算鼠标位置对应的格子行列
        var col = mousePosition.X / Constants.GridSize;
        var row = mousePosition.Y / Constants.GridSize;

        if (row >= 0 && row < Constants.MazeHeight && col >= 0 && col < Constants.MazeWidth)
        {
            // 返回对应的位置
            return new(row, col);
        }
        return Position.Invalid;
    }

    /// <summary>
    /// 格子左键事件
    /// </summary>
    /// <param name="pos">格子位置</param>
    private void GridLeft(Position pos)
    {
        // 如果当前状态是默认状态或者位置是无效位置, 直接返回
        if (CurrentStatus == OperationStatus.Default || pos == Position.Invalid)
        {
            return;
        }

        // 如果当前状态是设置起点和终点状态
        if (CurrentStatus == OperationStatus.SetStartAndEnd)
        {
            // 如果选择的格子是终点, 不做处理
            if (pos == Maze.EndPosition)
            {
                return;
            }

            // 设置新的起点
            ConvertToPosition(pos, true);
        }
        // 如果当前状态是设置障碍物状态
        else if (CurrentStatus == OperationStatus.SetObstacle)
        {
            // 如果当前格子是起点或终点, 不做处理
            if (pos == Maze.StartPosition || pos == Maze.EndPosition)
            {
                return;
            }

            // 设置障碍物
            OnGridTypeChanged?.Invoke(pos, GridType.Obstacle);
            ChangeGridType(pos);
        }
    }

    /// <summary>
    /// 格子右键事件
    /// </summary>
    /// <param name="pos">格子位置</param>
    private void GridRight(Position pos)
    {
        // 如果当前状态是默认状态或者位置是无效位置, 直接返回
        if (CurrentStatus == OperationStatus.Default || pos == Position.Invalid)
        {
            return;
        }

        // 如果当前状态是设置起点和终点状态
        if (CurrentStatus == OperationStatus.SetStartAndEnd)
        {
            // 如果选择的格子是起点, 不做处理
            if (pos == Maze.StartPosition)
            {
                return;
            }

            // 设置新的终点
            ConvertToPosition(pos, false);
        }
        // 如果当前状态是设置障碍物状态
        else if (CurrentStatus == OperationStatus.SetObstacle)
        {
            if (pos == Maze.StartPosition || pos == Maze.EndPosition)
            {
                // 如果当前格子是起点或终点, 不做处理
                return;
            }

            // 移除障碍物
            OnGridTypeChanged?.Invoke(pos, GridType.Empty);
            ChangeGridType(pos);
        }
    }

    /// <summary>
    /// 转移起点或者终点到指定位置并通知Maze类更新
    /// </summary>
    /// <param name="position">目标位置</param>
    /// <param name="isStart">是否设置为起点</param>
    private void ConvertToPosition(Position position, bool isStart)
    {
        if (isStart)
        {
            // 原起点
            var originalStart = Maze.StartPosition;

            // 如果起点已设置, 移除原起点标识
            if (originalStart != Position.Invalid)
            {
                OnGridTypeChanged?.Invoke(originalStart, GridType.Empty);
                ChangeGridType(originalStart);
            }

            // 设置新的起点
            Maze.StartPosition = position;
            OnGridTypeChanged?.Invoke(Maze.StartPosition, GridType.Start);
            ChangeGridType(Maze.StartPosition);
        }
        else
        {
            // 原终点
            var originalEnd = Maze.EndPosition;

            // 如果终点已设置, 移除原终点标识
            if (originalEnd != Position.Invalid)
            {
                OnGridTypeChanged?.Invoke(originalEnd, GridType.Empty);
                ChangeGridType(originalEnd);
            }

            // 设置新的终点
            Maze.EndPosition = position;
            OnGridTypeChanged?.Invoke(Maze.EndPosition, GridType.End);
            ChangeGridType(Maze.EndPosition);
        }
    }

    /// <summary>
    /// 绘制指定位置的格子
    /// </summary>
    /// <param name="g">绘图对象</param>
    /// <param name="row">行索引</param>
    /// <param name="col">列索引</param>
    /// <param name="type">格子类型</param>
    private static void DrawGrid(Graphics g, int row, int col, GridType type)
    {
        // 获取格子矩形区域
        var rect = new Rectangle(col * Constants.GridSize, row * Constants.GridSize, Constants.GridSize, Constants.GridSize);

        // 根据格子类型选择颜色和文本
        var fillColor = Constants.EmptyColor;
        var text = "";
        switch (type)
        {
            case GridType.Start:
                fillColor = Constants.StartColor;
                text = "S";
                break;
            case GridType.End:
                fillColor = Constants.EndColor;
                text = "E";
                break;
            case GridType.Obstacle:
                fillColor = Constants.ObstacleColor;
                break;
            case GridType.Path:
                fillColor = Constants.PathColor;
                break;
            case GridType.FadePath:
                fillColor = Constants.FadePathColor;
                break;
            case GridType.Empty:
                break;
            default:
                break;
        }

        // 绘制格子背景
        using var brush = new SolidBrush(fillColor);
        g.FillRectangle(brush, rect);

        // 绘制文本（居中）
        if (!string.IsNullOrEmpty(text))
        {
            var textSize = g.MeasureString(text, Control.DefaultFont);
            var textX = rect.X + (rect.Width - textSize.Width) / 2;
            var textY = rect.Y + (rect.Height - textSize.Height) / 2;
            using var textBrush = new SolidBrush(Color.Black);
            g.DrawString(text, Control.DefaultFont, textBrush, textX, textY);
        }
    }
}
