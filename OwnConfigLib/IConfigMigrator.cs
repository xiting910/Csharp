using System;
using System.Text.Json.Nodes;

namespace OwnConfigLib;

/// <summary>
/// 配置迁移器接口
/// </summary>
/// <typeparam name="TConfig">此迁移器所属的最终配置类型</typeparam>
public interface IConfigMigrator<TConfig> where TConfig : IConfig<TConfig>
{
    /// <summary>
    /// 当前迁移器支持的起始版本号
    /// </summary>
    Version FromVersion { get; }

    /// <summary>
    /// 当前迁移器支持的目标版本号
    /// </summary>
    Version ToVersion { get; }

    /// <summary>
    /// 从旧版本配置迁移到指定版本的配置
    /// </summary>
    /// <param name="oldConfigJson">旧版本配置的 JSON 对象</param>
    /// <returns>指定版本的配置 JSON 对象</returns>
    JsonObject Migrate(JsonObject oldConfigJson);
}
