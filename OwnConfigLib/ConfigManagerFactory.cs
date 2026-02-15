using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OwnConfigLib;

/// <summary>
/// 配置管理器工厂类, 用于创建和获取配置管理器单例实例
/// </summary>
public static class ConfigManagerFactory
{
    /// <summary>
    /// 嵌套泛型类, 用于缓存不同配置类型的配置管理器实例
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    private static class ManagerCache<TConfig> where TConfig : class, IConfig<TConfig>, new()
    {
        /// <summary>
        /// 配置管理器实例缓存, 键为配置文件路径, 值为对应的配置管理器实例
        /// </summary>
        public static readonly ConcurrentDictionary<string, ConfigManager<TConfig>> Instances = new();
    }

    /// <summary><para>
    /// 异步获取指定配置类型和指定配置文件路径的配置管理器单例
    /// </para><para>
    /// 如果对应的实例已存在且未被释放, 则直接返回该实例, 否则创建新的实例并返回
    /// </para><para>
    /// 创建实例时会使用传入的同步上下文和工厂方法, 以及配置迁移器进行初始化
    /// </para><para>
    /// 如果工厂方法未提供, 则使用默认的无参构造函数创建配置对象实例
    /// </para><para>
    /// 实例创建后会自动读取配置文件并尝试使用提供的迁移器进行配置迁移
    /// </para></summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <param name="fileName">配置文件名称</param>
    /// <param name="directory">配置文件目录</param>
    /// <param name="context">用于触发事件的同步上下文</param>
    /// <param name="factory">用于创建配置对象实例的工厂方法, 参数为同步上下文</param>
    /// <param name="token">用于取消操作的 <see cref="CancellationToken"/></param>
    /// <param name="migrators">配置迁移器实例数组, 用于实现配置从旧版本迁移到新版本的功能</param>
    /// <returns>配置管理器单例</returns>
    /// <exception cref="ArgumentNullException">当文件名无效时抛出</exception>
    /// <exception cref="ArgumentException">当文件名无效时抛出</exception>
    /// <exception cref="IOException">当发生 I/O 错误时抛出</exception>
    /// <exception cref="PathTooLongException">当路径过长时抛出</exception>
    /// <exception cref="DirectoryNotFoundException">当目录不存在且无法创建时抛出</exception>
    /// <exception cref="UnauthorizedAccessException">当没有足够权限访问目录或文件时抛出</exception>
    public static async Task<ConfigManager<TConfig>> GetConfigManagerAsync<TConfig>
    (string fileName, string directory, SynchronizationContext? context = null, Func<SynchronizationContext?, TConfig>? factory = null, CancellationToken token = default, params IConfigMigrator<TConfig>[] migrators)
    where TConfig : class, IConfig<TConfig>, new()
    {
        // 检查文件名是否有效
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));

        // 尝试创建目录并获取目录的绝对路径, 如果目录不存在则创建目录
        var fullDirectory = Directory.CreateDirectory(directory).FullName;

        // 组合目录和文件名获取配置文件的绝对路径
        var fullPath = Path.Combine(fullDirectory, fileName);

        // 使用循环处理已释放实例的重新创建
        while (true)
        {
            // 使用 GetOrAdd 方法从缓存中获取或创建配置管理器实例
            var cm = ManagerCache<TConfig>.Instances.GetOrAdd(fullPath, filePath => new(path => _ = ManagerCache<TConfig>.Instances.TryRemove(path, out _), filePath, fileName, fullDirectory, context, factory, migrators));

            // 检查实例是否已经被释放
            if (cm.IsDisposed)
            {
                // 如果实例已释放, 将其从缓存中移除
                _ = ManagerCache<TConfig>.Instances.TryRemove(fullPath, out _);

                // 立即重新开始循环获取新实例
                continue;
            }

            // 初始化配置管理器实例
            await cm.InitializeAsync(token).ConfigureAwait(false);

            // 返回配置管理器实例
            return cm;
        }
    }
}
