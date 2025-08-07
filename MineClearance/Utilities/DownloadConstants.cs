namespace MineClearance.Utilities;

/// <summary>
/// 下载常量类, 提供一些下载相关的常量
/// </summary>
public static class DownloadConstants
{
    /// <summary>
    /// 更新下载速度刷新间隔(毫秒)
    /// </summary>
    public const int UpdateSpeedRefreshInterval = 300;

    /// <summary>
    /// http请求超时时间(秒)
    /// </summary>
    public const int HttpRequestTimeout = 20;

    /// <summary>
    /// 无进度一定秒后重试
    /// </summary>
    public const int NoProgressRetryInterval = 15;

    /// <summary>
    /// 无进度弹窗等待时间(秒)
    /// </summary>
    public const int NoProgressDialogWaitTime = 5;

    /// <summary>
    /// 无进度最大重试次数
    /// </summary>
    public const int NoProgressMaxRetries = 3;
}