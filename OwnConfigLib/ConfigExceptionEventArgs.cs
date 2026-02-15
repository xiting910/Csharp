using System;

namespace OwnConfigLib;

/// <summary>
/// 配置异常事件参数
/// </summary>
/// <param name="exception">异常对象</param>
public sealed class ConfigExceptionEventArgs(Exception exception) : EventArgs
{
    /// <summary>
    /// 获取发生的异常对象
    /// </summary>
    public Exception Exception { get; } = exception;
}
