using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OwnConfigLib;

// 配置管理器类的静态成员部分
public partial class ConfigManager<TConfig>
{
    /// <summary>
    /// 通过表达式树编译生成的快速复制方法, 用于在更新配置时快速复制属性值
    /// </summary>
    private static readonly Action<TConfig, TConfig> _fastCopyAction;

    /// <summary>
    /// 静态构造函数, 生成快速复制方法, 以提高配置更新时的性能
    /// </summary>
    static ConfigManager()
    {
        // 当前配置类型
        var type = typeof(TConfig);

        // 快速复制方法的目标配置参数
        var targetParam = Expression.Parameter(type, "target");

        // 快速复制方法的源配置参数
        var sourceParam = Expression.Parameter(type, "source");

        // 构建一个表达式树, 用于快速复制配置属性值, 并编译成一个委托赋值给 _fastCopyAction
        _fastCopyAction = Expression.Lambda<Action<TConfig, TConfig>>(Expression.Block(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite).Select(p => Expression.Assign(Expression.Property(targetParam, p), Expression.Property(sourceParam, p)))), targetParam, sourceParam).Compile();
    }
}
