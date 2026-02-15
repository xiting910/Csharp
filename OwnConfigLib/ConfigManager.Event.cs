using System;
using System.Threading;

namespace OwnConfigLib;

// 配置管理器类的事件部分
public partial class ConfigManager<TConfig>
{
    /// <summary>
    /// 原始的同步上下文, 用于触发事件
    /// </summary>
    private readonly SynchronizationContext? _originalContext;

    /// <summary>
    /// 发生异常时触发的事件
    /// </summary>
    public event EventHandler<ConfigExceptionEventArgs>? ConfigExceptionOccurred;

    /// <summary>
    /// 同步上下文触发配置异常事件
    /// </summary>
    /// <param name="ex">发生的异常</param>
    private void RaiseConfigExceptionOccurred(Exception ex)
    {
        if (_originalContext is null)
        {
            ConfigExceptionOccurred?.Invoke(this, new(ex));
        }
        else
        {
            _originalContext.Post(_ => ConfigExceptionOccurred?.Invoke(this, new(ex)), null);
        }
    }
}
