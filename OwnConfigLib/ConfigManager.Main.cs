using System;
using System.Threading;

namespace OwnConfigLib;

/// <summary><para>
/// 配置管理器, 负责加载、保存和监控配置文件的变化, 支持自动保存和热重载功能, 同时有防抖机制
/// </para><para>
/// 实现了 <see cref="IAsyncDisposable"/> 和 <see cref="IDisposable"/> 接口,
/// 建议优先使用 <c>await using</c> 语法进行自动释放或手动调用异步释放方法 <see cref="DisposeAsync"/>
/// </para><para>
/// 初始化、保存和加载配置时发生的异常会被捕获并通过 <see cref="ConfigExceptionOccurred"/> 事件通知调用者,
/// 该事件支持在原始同步上下文 (需要在创建实例时传入同步上下文) 中触发, 以便调用者在 UI 线程中处理异常
/// </para></summary>
/// <typeparam name="TConfig">配置类型</typeparam>
public sealed partial class ConfigManager<TConfig> : IAsyncDisposable, IDisposable
where TConfig : class, IConfig<TConfig>, new()
{
    /// <summary>
    /// 防抖间隔毫秒数
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">当设置的值为负数时抛出</exception>
    public int DebounceMilliseconds
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return Interlocked.CompareExchange(ref field, 0, 0);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(DebounceMilliseconds));
            _ = Interlocked.Exchange(ref field, value);
        }
    } = ConfigHelper.DefaultDebounceMilliseconds;

    /// <summary>
    /// 是否启用配置变更自动保存锁
    /// </summary>
    private readonly Lock _autoSaveLock = new();

    /// <summary>
    /// 是否启用配置变更自动保存字段
    /// </summary>
    private volatile bool _enableAutoSave = true;

    /// <summary>
    /// 是否启用配置变更自动保存
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    public bool EnableAutoSave
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            lock (_autoSaveLock) { return _enableAutoSave; }
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            lock (_autoSaveLock) { _enableAutoSave = value; }
            if (!value) { _ = _saveTimer.Change(Timeout.Infinite, Timeout.Infinite); }
        }
    }

    /// <summary>
    /// 是否启用配置文件热重载锁
    /// </summary>
    private readonly Lock _enableHotReloadLock = new();

    /// <summary>
    /// 是否启用配置文件热重载
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    public bool EnableHotReload
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            lock (_enableHotReloadLock) { return _fileWatcher.EnableRaisingEvents; }
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            lock (_enableHotReloadLock) { _fileWatcher.EnableRaisingEvents = value; }
            if (!value) { _ = _loadTimer.Change(Timeout.Infinite, Timeout.Infinite); }
        }
    }

    /// <summary>
    /// 配置对象字段
    /// </summary>
    private readonly TConfig _config;

    /// <summary>
    /// 获取当前配置对象的引用
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    public TConfig Config
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return _config;
        }
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    public string ConfigFilePath
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return _configFilePath;
        }
    }
}
