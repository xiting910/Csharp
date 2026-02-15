namespace CleanUpRegedit;

/// <summary>
/// 自定义注册表键类以显示可读名称和删除
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class CustomRegistryKey(RegistryKey parentKey, string keyName, string displayName)
{
    /// <summary>
    /// 当前键的父键
    /// </summary>
    private readonly RegistryKey _parentKey = parentKey;

    /// <summary>
    /// 当前注册表键名称
    /// </summary>
    private readonly string _keyName = keyName;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 重写字符串表示
    /// </summary>
    /// <returns>字符串表示</returns>
    public override string ToString() => DisplayName;

    /// <summary>
    /// 删除当前注册表键
    /// </summary>
    public void Delete() => _parentKey.DeleteSubKeyWithLogging(_keyName);
}
