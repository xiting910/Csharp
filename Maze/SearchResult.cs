namespace Maze;

/// <summary>
/// 搜索结果类, 用于存储搜索结果信息
/// </summary>
/// <param name="isCanceled">搜索是否被取消</param>
/// <param name="isSuccess">搜索是否成功</param>
/// <param name="pathLength">搜索路径长度</param>
internal sealed class SearchResult(bool isCanceled, bool isSuccess, int pathLength)
{
    /// <summary>
    /// 搜索是否被取消
    /// </summary>
    public bool IsCanceled { get; private init; } = isCanceled;

    /// <summary>
    /// 搜索是否成功
    /// </summary>
    public bool IsSuccess { get; private init; } = isSuccess;

    /// <summary>
    /// 搜索路径长度
    /// </summary>
    public int PathLength { get; private init; } = pathLength;
}
