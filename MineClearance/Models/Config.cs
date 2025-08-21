namespace MineClearance.Models;

/// <summary>
/// 配置数据类
/// </summary>
internal sealed record Config
{
    /// <summary>
    /// 主窗体配置
    /// </summary>
    public MainFormConfig? MainForm { get; init; }

    /// <summary>
    /// 设置窗口配置
    /// </summary>
    public SettingFormConfig? SettingForm { get; init; }
}

/// <summary>
/// 主窗体配置
/// </summary>
internal sealed record MainFormConfig
{
    /// <summary>
    /// 获取或设置窗口左侧位置
    /// </summary>
    public int Left { get; init; }

    /// <summary>
    /// 获取或设置窗口顶部位置
    /// </summary>
    public int Top { get; init; }
}

/// <summary>
/// 设置窗口配置
/// </summary>
internal sealed record SettingFormConfig
{
    /// <summary>
    /// 获取或设置窗口左侧位置
    /// </summary>
    public int Left { get; init; }

    /// <summary>
    /// 获取或设置窗口顶部位置
    /// </summary>
    public int Top { get; init; }

    /// <summary>
    /// 获取或设置窗口宽度
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// 获取或设置窗口高度
    /// </summary>
    public int Height { get; init; }
}