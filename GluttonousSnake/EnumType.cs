namespace GluttonousSnake
{
    /// <summary>
    /// 方向枚举
    /// </summary>
    /// <remarks>
    /// Up: 向上
    /// Down: 向下
    /// Left: 向左
    /// Right: 向右
    /// </remarks>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// 格子类型枚举
    /// </summary>
    /// <remarks>
    /// Empty: 空
    /// SnakeHead: 蛇头
    /// SnakeBody: 蛇身
    /// Food: 食物
    /// </remarks>
    public enum GridType
    {
        Empty,
        SnakeHead,
        SnakeBody,
        Food
    }
}