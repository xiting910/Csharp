using System.ComponentModel;
using System.IO;
using System.Threading;

namespace OwnConfigLib;

// 配置管理器类的事件处理部分
public partial class ConfigManager<TConfig>
{
    /// <summary>
    /// 配置对象属性变化时触发的事件处理程序
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 设置脏标记, 因为配置对象已经发生了变化, 现在配置与文件可能不同步了
        _isDirty = true;

        // 关闭自动保存或正在重载时不处理
        if (!_enableAutoSave || _isLoading) { return; }

        // 重置计时器, 以在指定的防抖时间后触发保存操作
        _saveTimer.Change(DebounceMilliseconds, Timeout.Infinite);
    }

    /// <summary>
    /// 保存计时器的回调方法
    /// </summary>
    /// <param name="state">计时器状态对象</param>
    private async void OnSaveTimerCallback(object? state) => _ = await SaveAsync(default).ConfigureAwait(false);

    /// <summary>
    /// 文件更改事件处理程序
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">文件系统事件参数</param>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // 正在保存时不处理
        if (_isSaving) { return; }

        // 重置计时器, 以在指定的防抖时间后触发加载操作
        _loadTimer.Change(DebounceMilliseconds, Timeout.Infinite);
    }

    /// <summary>
    /// 加载计时器的回调方法
    /// </summary>
    /// <param name="state">计时器状态对象</param>
    private async void OnLoadTimerCallback(object? state) => _ = await LoadAsync(default).ConfigureAwait(false);

    /// <summary>
    /// 文件监控器错误事件处理程序
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">错误事件参数</param>
    private void OnFileWatcherError(object sender, ErrorEventArgs e) => RaiseConfigExceptionOccurred(e.GetException());
}
