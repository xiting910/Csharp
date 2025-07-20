namespace Maze
{
    /// <summary>
    /// 位置类, 用于表示迷宫中的位置
    /// </summary>
    public class Position
    {
        /// <summary>
        /// 行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        public int Col { get; set; }

        /// <summary>
        /// 无效位置
        /// </summary>
        public static Position Invalid => new(-1, -1);

        /// <summary>
        /// 构造函数, 初始化位置
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }

        /// <summary>
        /// 构造函数, 初始化位置
        /// </summary>
        /// <param name="position">位置元组</param>
        public Position((int row, int col) position)
        {
            Row = position.row;
            Col = position.col;
        }

        /// <summary>
        /// 重写+运算符
        /// </summary>
        public static Position operator +(Position left, Position right)
        {
            return new Position(left.Row + right.Row, left.Col + right.Col);
        }

        /// <summary>
        /// 重写+运算符
        /// </summary>
        public static Position operator +(Position left, (int row, int col) right)
        {
            return new Position(left.Row + right.row, left.Col + right.col);
        }

        /// <summary>
        /// 重写+运算符
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">如果方向无效</exception>
        public static Position operator +(Position left, Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Position(left.Row - 1, left.Col),
                Direction.Right => new Position(left.Row, left.Col + 1),
                Direction.Down => new Position(left.Row + 1, left.Col),
                Direction.Left => new Position(left.Row, left.Col - 1),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), "无效的方向")
            };
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
}