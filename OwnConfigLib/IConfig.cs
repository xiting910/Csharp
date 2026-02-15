using System;
using System.ComponentModel;

namespace OwnConfigLib;

/// <summary>
/// 配置类型接口
/// </summary>
/// <typeparam name="TConfig">配置类型</typeparam>
public interface IConfig<TConfig> : INotifyPropertyChanged where TConfig : IConfig<TConfig>
{
    /// <summary>
    /// 配置类型的当前版本号, 用于实现配置从旧版本迁移到新版本的功能
    /// </summary>
    static abstract Version CurrentVersion { get; }

    /// <summary>
    /// 配置版本号, 用于实现配置从旧版本迁移到新版本的功能
    /// </summary>
    Version Version { get; }
}
