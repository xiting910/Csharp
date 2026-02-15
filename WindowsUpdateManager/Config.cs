namespace WindowsUpdateManager;

/// <summary>
/// 配置数据类
/// </summary>
internal sealed record Config
{
    /// <summary>
    /// 当前配置版本
    /// </summary>
    public byte Version { get; init; } = Constants.CurrentConfigVersion;

    /// <summary>
    /// 最新保存时间
    /// </summary>
    public DateTime LastSaved { get; init; } = DateTime.Now;

    /// <summary>
    /// 主窗体宽度
    /// </summary>
    public int MainFormWidth { get; init; }

    /// <summary>
    /// 主窗体高度
    /// </summary>
    public int MainFormHeight { get; init; }

    /// <summary>
    /// 主窗体左侧位置
    /// </summary>
    public int MainFormLeft { get; init; }

    /// <summary>
    /// 主窗体顶部位置
    /// </summary>
    public int MainFormTop { get; init; }
}
