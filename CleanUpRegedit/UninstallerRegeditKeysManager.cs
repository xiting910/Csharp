namespace CleanUpRegedit;

/// <summary>
/// 注册表卸载程序键管理器类
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class UninstallerRegeditKeysManager : IDisposable
{
    /// <summary>
    /// 卸载程序注册表键路径
    /// </summary>
    public const string UninstallerRegeditKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    /// <summary>
    /// 卸载程序注册表键
    /// </summary>
    private readonly RegistryKey _uninstallerKey = Registry.LocalMachine.OpenSubKey(UninstallerRegeditKeyPath, true)!;

    /// <summary>
    /// 获取路径下所有有效的注册表键, 同时删除无效键
    /// </summary>
    /// <param name="deleteEmptyKeys">是否删除空键</param>
    /// <returns>注册表键列表</returns>
    public List<CustomRegistryKey> GetAllKeys(bool deleteEmptyKeys)
    {
        var keys = new List<CustomRegistryKey>();

        // 遍历所有子键
        foreach (var subKeyName in _uninstallerKey.GetSubKeyNames())
        {
            // 打开子键
            using var subKey = _uninstallerKey.OpenSubKey(subKeyName);
            if (subKey is null)
            {
                continue;
            }

            // 如果子键为空键, 则删除该子键并继续
            if (subKey.SubKeyCount == 0 && subKey.ValueCount == 0)
            {
                if (deleteEmptyKeys)
                {
                    _uninstallerKey.DeleteSubKeyWithLogging(subKeyName);
                }
                continue;
            }

            // 获取子键下的 DisplayName 值作为显示名称
            var displayName = subKey.GetValue("DisplayName") as string;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "未知";
            }

            // 添加自定义注册表键对象到列表
            keys.Add(new(_uninstallerKey, subKeyName, displayName));
        }

        return keys;
    }

    /// <summary>
    /// 释放卸载程序注册表键资源
    /// </summary>
    public void Dispose() => _uninstallerKey.Dispose();
}
