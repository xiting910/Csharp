namespace CleanUpRegedit;

/// <summary>
/// 注册表安装程序键管理器类
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class InstallerRegeditKeysManager : IDisposable
{
    /// <summary>
    /// 安装程序注册表键路径
    /// </summary>
    public const string InstallerRegeditKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products";

    /// <summary>
    /// 安装程序注册表键
    /// </summary>
    private readonly RegistryKey _installerKey = Registry.LocalMachine.OpenSubKey(InstallerRegeditKeyPath, true)!;

    /// <summary>
    /// 获取路径下所有有效的注册表键, 同时删除无效键
    /// </summary>
    /// <param name="deleteEmptyKeys">是否删除空键</param>
    /// <returns>注册表键列表</returns>
    public List<CustomRegistryKey> GetAllKeys(bool deleteEmptyKeys)
    {
        var keys = new List<CustomRegistryKey>();

        // 遍历所有子键
        foreach (var subKeyName in _installerKey.GetSubKeyNames())
        {
            // 打开子键
            using var subKey = _installerKey.OpenSubKey(subKeyName);
            if (subKey is null)
            {
                continue;
            }

            // 如果子键为空键, 则删除该子键并继续
            if (subKey.SubKeyCount == 0 && subKey.ValueCount == 0)
            {
                if (deleteEmptyKeys)
                {
                    _installerKey.DeleteSubKeyWithLogging(subKeyName);
                }
                continue;
            }

            // 否则, 获取该子键下的 InstallProperties 子键
            using var installPropertiesKey = subKey.OpenSubKey("InstallProperties");

            // 获取 InstallProperties 子键下的 DisplayName 值作为显示名称
            var displayName = installPropertiesKey?.GetValue("DisplayName") as string;

            // 如果显示名称为空, 则设为"未知"
            displayName ??= "未知";

            // 添加自定义注册表键对象到列表
            keys.Add(new(_installerKey, subKeyName, displayName));
        }

        return keys;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose() => _installerKey.Dispose();
}
