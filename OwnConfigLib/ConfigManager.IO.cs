using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OwnConfigLib;

// 配置管理器类的 IO 部分
public partial class ConfigManager<TConfig>
{
    /// <summary>
    /// 配置文件路径字段
    /// </summary>
    private readonly string _configFilePath;

    /// <summary>
    /// 临时存配置文件路径字段
    /// </summary>
    private readonly string _tempFilePath;

    /// <summary>
    /// 异步文件读写锁
    /// </summary>
    private readonly SemaphoreSlim _ioLock = new(1, 1);

    /// <summary>
    /// 文件系统监控器
    /// </summary>
    private readonly FileSystemWatcher _fileWatcher;

    /// <summary>
    /// 脏标记, 用于指示配置是否已不同步于文件中的配置
    /// </summary>
    private volatile bool _isDirty;

    /// <summary>
    /// 标记是否正在进行内部保存操作
    /// </summary>
    private volatile bool _isSaving;

    /// <summary>
    /// 标记是否正在从文件加载配置
    /// </summary>
    private volatile bool _isLoading;

    /// <summary>
    /// 用于实现防抖保存操作的计时器
    /// </summary>
    private readonly Timer _saveTimer;

    /// <summary>
    /// 用于实现防抖加载操作的计时器
    /// </summary>
    private readonly Timer _loadTimer;

    /// <summary>
    /// 异步立刻保存配置到文件
    /// </summary>
    /// <param name="token">用于取消保存操作的 <see cref="CancellationToken"/></param>
    /// <returns>如果成功保存配置则返回 <c>true</c>, 否则返回 <c>false</c></returns>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    public async ValueTask<bool> SaveImmediatelyAsync(CancellationToken token = default)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        // 如果已经申请取消或者正在加载则不处理
        if (token.IsCancellationRequested || _isLoading) { return false; }

        // 取消任何正在等待的保存操作
        _ = _saveTimer.Change(Timeout.Infinite, Timeout.Infinite);

        // 立刻保存配置
        return await SaveAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步从文件重新加载配置, 并返回是否成功加载
    /// </summary>
    /// <param name="token">用于取消加载操作的 <see cref="CancellationToken"/></param>
    /// <returns>如果成功加载配置则返回 <c>true</c>, 否则返回 <c>false</c></returns>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    public async ValueTask<bool> ReLoadAsync(CancellationToken token = default)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        // 如果已经申请取消或者正在保存则不处理
        if (token.IsCancellationRequested || _isSaving) { return false; }

        // 取消任何正在等待的加载操作
        _ = _loadTimer.Change(Timeout.Infinite, Timeout.Infinite);

        // 立刻加载配置
        return await LoadAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步保存配置到文件
    /// </summary>
    /// <param name="token">用于取消保存操作的 <see cref="CancellationToken"/></param>
    /// <returns>如果成功保存配置则返回 <c>true</c>, 否则返回 <c>false</c></returns>
    private async Task<bool> SaveAsync(CancellationToken token)
    {
        // 获取文件读写锁
        try { await _ioLock.WaitAsync(token).ConfigureAwait(false); }
        catch (OperationCanceledException) { return false; }

        try
        {
            // 当前正在执行的操作数加 1
            IncrementActiveOperations();

            // 将数据写入临时文件
            await using (var stream = ConfigHelper.CreateSaveFileStream(_tempFilePath))
            {
                await JsonSerializer.SerializeAsync(stream, _config, ConfigHelper.JsonOptions, token).ConfigureAwait(false);
                await stream.FlushAsync(token).ConfigureAwait(false);
            }

            // 标记正在进行内部保存操作
            _isSaving = true;

            // 更新配置文件
            if (File.Exists(_configFilePath))
            {
                // 原子替换文件
                File.Replace(_tempFilePath, _configFilePath, null);
            }
            else
            {
                // 如果原文件不存在, 直接移动
                File.Move(_tempFilePath, _configFilePath);
            }

            // 更新脏标记, 因为已经保存了配置, 所以现在配置与文件是同步的
            _isDirty = false;

            // 返回成功
            return true;
        }
        catch (OperationCanceledException)
        {
            // 保存操作被取消, 返回失败
            return false;
        }
        catch (Exception ex)
        {
            // 发生异常时触发事件, 并返回失败
            RaiseConfigExceptionOccurred(ex);
            return false;
        }
        finally
        {
            // 无论成功与否都尝试删除临时文件
            try { File.Delete(_tempFilePath); } catch { }

            // 标记不再进行内部保存操作
            _isSaving = false;

            // 当前正在执行的操作数减 1
            DecrementActiveOperations();

            // 释放文件读写锁
            _ = _ioLock.Release();
        }
    }

    /// <summary>
    /// 异步从文件加载配置
    /// </summary>
    /// <param name="token">用于取消加载操作的 <see cref="CancellationToken"/></param>
    /// <returns>如果成功加载配置则返回 <c>true</c>, 否则返回 <c>false</c></returns>    
    private async Task<bool> LoadAsync(CancellationToken token)
    {
        // 获取文件读写锁
        try { await _ioLock.WaitAsync(token).ConfigureAwait(false); }
        catch (OperationCanceledException) { return false; }

        try
        {
            // 当前正在执行的操作数加 1
            IncrementActiveOperations();

            // 标记正在加载
            _isLoading = true;

            // 异步读取文件并反序列化配置对象
            await using var stream = ConfigHelper.CreateLoadFileStream(_configFilePath);
            var loadedConfig = await JsonSerializer.DeserializeAsync<TConfig>(stream, ConfigHelper.JsonOptions, token).ConfigureAwait(false) ?? throw new JsonException("加载失败: 配置文件内容无效");

            // 更新配置对象
            _fastCopyAction(_config, loadedConfig);

            // 更新脏标记, 因为已经从文件加载了配置, 所以现在配置与文件是同步的
            _isDirty = false;

            // 返回成功
            return true;
        }
        catch (OperationCanceledException)
        {
            // 加载操作被取消, 返回失败
            return false;
        }
        catch (Exception ex)
        {
            // 发生异常时触发事件
            RaiseConfigExceptionOccurred(ex);
            return false;
        }
        finally
        {
            // 标记不再加载
            _isLoading = false;

            // 当前正在执行的操作数减 1
            DecrementActiveOperations();

            // 释放文件读写锁
            _ = _ioLock.Release();
        }
    }
}
