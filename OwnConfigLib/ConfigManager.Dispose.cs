using System;
using System.Threading;
using System.Threading.Tasks;

namespace OwnConfigLib;

// 配置管理器类的释放实现部分
public partial class ConfigManager<TConfig>
{
    /// <summary>
    /// 当前实例是否已释放
    /// </summary>
    internal bool IsDisposed { get; private set; }

    /// <summary>
    /// 当实例被释放时执行的回调操作, 参数为配置文件路径
    /// </summary>
    private readonly Action<string> _onDispose;

    /// <summary>
    /// 释放锁, 用于确保在释放过程的间没有其他线程正在执行操作, 以避免在释放资源时发生竞争条件
    /// </summary>
    private readonly SemaphoreSlim _disposeLock = new(1, 1);

    /// <summary>
    /// 申请可以实现释放的 CTS
    /// </summary>
    private readonly CancellationTokenSource _canDisposeCts = new();

    /// <summary>
    /// 当前正在执行的操作数字段
    /// </summary>
    private int _activeOperations;

    /// <summary>
    /// 当前正在执行的操作数 + 1, 用于在开始一个操作时调用, 以确保在释放资源时知道有多少操作正在访问资源
    /// </summary>
    private void IncrementActiveOperations() => _ = Interlocked.Increment(ref _activeOperations);

    /// <summary>
    /// 当前正在执行的操作数 - 1, 用于在完成一个操作时调用, 以确保在释放资源时知道有多少操作正在访问资源
    /// </summary>
    private void DecrementActiveOperations()
    {
        // 如果没有正在执行的操作了且当前实例正在等待释放, 则取消等待, 以允许释放过程继续
        if (Interlocked.Decrement(ref _activeOperations) == 0 && IsDisposed)
        {
            try { _canDisposeCts.Cancel(); } catch (ObjectDisposedException) { }
        }
    }

    /// <summary>
    /// 原子操作读取当前正在执行的操作数, 用于在释放过程中检查是否有操作正在访问资源
    /// </summary>
    /// <returns>当前正在执行的操作数</returns>
    private int GetActiveOperations() => Interlocked.CompareExchange(ref _activeOperations, 0, 0);

    /// <summary><para>
    /// 释放资源, 该方法为 <see cref="DisposeAsync"/> 方法的同步包装
    /// <para></para>
    /// 警告: 该方法会阻塞调用线程直到释放完成, 可能会导致死锁
    /// </para></summary>
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    /// <summary>
    /// 异步释放资源并保存配置
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // 如果已释放, 则直接返回
        if (IsDisposed) { return; }

        // 获取释放锁, 确保只有一个线程能够执行释放过程, 以避免在释放资源时发生竞争条件
        try { await _disposeLock.WaitAsync().ConfigureAwait(false); }
        catch (ObjectDisposedException) { return; }

        try
        {
            // 再次检查是否已释放, 以防止在等待锁的过程中其他线程已经释放了当前实例, 以避免重复释放资源
            if (IsDisposed) { return; }

            // 标记为已释放
            IsDisposed = true;

            // 执行释放回调, 以允许工厂类从缓存中移除已释放的实例, 参数为配置文件路径
            _onDispose(_configFilePath);

            // 释放初始化锁
            _initLock.Dispose();

            // 取消事件订阅
            _config.PropertyChanged -= OnConfigPropertyChanged;

            // 释放文件监控器
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Created -= OnFileChanged;
            _fileWatcher.Changed -= OnFileChanged;
            _fileWatcher.Error -= OnFileWatcherError;
            _fileWatcher.Dispose();

            // 释放计时器
            await _saveTimer.DisposeAsync().ConfigureAwait(false);
            await _loadTimer.DisposeAsync().ConfigureAwait(false);

            // 等待所有正在执行的操作完成
            if (GetActiveOperations() > 0)
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, _canDisposeCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
            }

            // 释放释放 CTS
            _canDisposeCts.Dispose();

            // 如果脏标记为真, 则保存配置
            if (_isDirty) { _ = await SaveAsync(default).ConfigureAwait(false); }

            // 释放 io 锁
            _ioLock.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // 忽略在释放过程中发生的对象已被释放的异常, 说明当前实例已经被其他线程释放了
        }
        finally
        {
            // 直接释放锁
            _disposeLock.Dispose();

            // 通知垃圾回收器
            GC.SuppressFinalize(this);
        }
    }
}
