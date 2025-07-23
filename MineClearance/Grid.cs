namespace MineClearance;

/// <summary>
/// 表示扫雷游戏的棋盘格子
/// </summary>
/// <param name="row">行</param>
/// <param name="column">列</param>
/// <param name="type">类型</param>
/// <param name="surroundingMines">周围地雷数量, 默认为-1</param>
public class Grid(int row, int column, GridType type, int surroundingMines = -1)
{
    /// <summary>
    /// 格子所在的行
    /// </summary>
    public int Row { get; set; } = row;

    /// <summary>
    /// 格子所在的列
    /// </summary>
    public int Column { get; set; } = column;

    /// <summary>
    /// 格子类型
    /// </summary>
    public GridType Type { get; set; } = type;

    /// <summary>
    /// 周围地雷的数量, 仅在Type为Number时有效, -1表示不是数字格子
    /// </summary>
    public int SurroundingMines { get; set; } = surroundingMines;
}