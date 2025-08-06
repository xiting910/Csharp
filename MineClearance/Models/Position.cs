namespace MineClearance.Models;

/// <summary>
/// 位置类, 用于表示扫雷游戏中的位置
/// </summary>
/// <param name="row">行</param>
/// <param name="col">列</param>
public class Position(int row, int col)
{
    /// <summary>
    /// 行
    /// </summary>
    public int Row { get; private init; } = row;

    /// <summary>
    /// 列
    /// </summary>
    public int Col { get; private init; } = col;

    /// <summary>
    /// 重写+运算符
    /// </summary>
    public static Position operator +(Position left, Position right)
    {
        return new(left.Row + right.Row, left.Col + right.Col);
    }

    /// <summary>
    /// 重写+运算符
    /// </summary>
    public static Position operator +(Position left, (int row, int col) right)
    {
        return new(left.Row + right.row, left.Col + right.col);
    }

    /// <summary>
    /// 重写==运算符
    /// </summary>
    public static bool operator ==(Position left, Position right)
    {
        return left.Row == right.Row && left.Col == right.Col;
    }

    /// <summary>
    /// 重写==运算符
    /// </summary>
    public static bool operator ==(Position left, (int row, int col)? obj)
    {
        if (obj == null) return false;
        return left.Row == obj.Value.row && left.Col == obj.Value.col;
    }

    /// <summary>
    /// 重写!=运算符
    /// </summary>
    public static bool operator !=(Position left, Position right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 重写!=运算符
    /// </summary>
    public static bool operator !=(Position left, (int row, int col)? obj)
    {
        return !(left == obj);
    }

    /// <summary>
    /// 重写Equals方法
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Position other)
        {
            return this == other;
        }
        return false;
    }

    /// <summary>
    /// 重写GetHashCode方法
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }
}