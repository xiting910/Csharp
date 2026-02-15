namespace Maze;

/// <summary>
/// 位置类, 用于表示迷宫中的位置
/// </summary>
/// <param name="row">行</param>
/// <param name="col">列</param>
internal sealed class Position(int row, int col)
{
    /// <summary>
    /// 行
    /// </summary>
    public int Row { get; } = row;

    /// <summary>
    /// 列
    /// </summary>
    public int Col { get; } = col;

    /// <summary>
    /// 无效位置
    /// </summary>
    public static Position Invalid => new(-1, -1);

    /// <summary>
    /// 重写+运算符
    /// </summary>
    public static Position operator +(Position left, Position right) => new(left.Row + right.Row, left.Col + right.Col);

    /// <summary>
    /// 重写+运算符
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">如果方向无效</exception>
    public static Position operator +(Position left, Direction direction) => direction switch
    {
        Direction.Up => new Position(left.Row - 1, left.Col),
        Direction.Right => new Position(left.Row, left.Col + 1),
        Direction.Down => new Position(left.Row + 1, left.Col),
        Direction.Left => new Position(left.Row, left.Col - 1),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), "无效的方向")
    };

    /// <summary>
    /// 重写==运算符
    /// </summary>
    public static bool operator ==(Position left, Position right) => left.Row == right.Row && left.Col == right.Col;

    /// <summary>
    /// 重写!=运算符
    /// </summary>
    public static bool operator !=(Position left, Position right) => !(left == right);

    /// <summary>
    /// 重写Equals方法
    /// </summary>
    public override bool Equals(object? obj) => obj is Position other && this == other;

    /// <summary>
    /// 重写GetHashCode方法
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Row, Col);
}
