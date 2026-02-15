using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OwnConfigLib;

// 配置管理器类的迁移器部分
public partial class ConfigManager<TConfig>
{
    /// <summary>
    /// 迁移路径映射表: Key为持有版本, Value为下一跳迁移器
    /// </summary>
    private readonly Dictionary<Version, IConfigMigrator<TConfig>> _migrationPathMap;

    /// <summary>
    /// 通过配置迁移器实例数组初始化迁移路径映射表, 构建从任意版本到当前版本的最短迁移路径
    /// </summary>
    /// <param name="migrators">配置迁移器实例数组</param>
    /// <returns>构建好的迁移路径映射表</returns>
    private static Dictionary<Version, IConfigMigrator<TConfig>> InitPathMap(IConfigMigrator<TConfig>[] migrators)
    {
        // 用于构建反向图的字典, Key 为迁移器的目标版本, Value 为所有迁移到该版本的迁移器列表
        Dictionary<Version, List<IConfigMigrator<TConfig>>> reverseGraph = [];

        // 遍历所有迁移器
        foreach (var migrator in migrators)
        {
            // 如果迁移器的目标版本在反向图中没有对应的列表, 则创建一个新的列表并添加到反向图中
            if (!reverseGraph.TryGetValue(migrator.ToVersion, out var list))
            {
                list = [];
                reverseGraph[migrator.ToVersion] = list;
            }

            // 将当前迁移器添加到反向图中对应目标版本的列表中
            list.Add(migrator);
        }

        // pathMap 记录: Key 为当前所在版本, Value 为通往目标的最佳下一跳迁移器
        Dictionary<Version, IConfigMigrator<TConfig>> pathMap = [];

        // visited 记录已访问的版本, 避免重复访问和死循环
        HashSet<Version> visited = [];

        // queue 用于广度优先搜索, 从目标版本出发向前寻找所有可达的版本
        Queue<Version> queue = new();

        // 从终点出发
        queue.Enqueue(TConfig.CurrentVersion);
        _ = visited.Add(TConfig.CurrentVersion);

        // 广度优先搜索反向图, 构建从任意版本到目标版本的最短迁移路径
        while (queue.Count > 0)
        {
            // 获取当前版本
            var currentVer = queue.Dequeue();

            // 查找所有能迁移到 currentVer 的前置版本
            if (reverseGraph.TryGetValue(currentVer, out var incomingMigrators))
            {
                // 遍历所有前置迁移器
                foreach (var migrator in incomingMigrators)
                {
                    // 获取前置版本
                    var prevVer = migrator.FromVersion;

                    // 如果该前置版本未被访问, 则将其加入路径映射表和访问队列
                    if (!visited.Contains(prevVer))
                    {
                        pathMap[prevVer] = migrator;
                        queue.Enqueue(prevVer);
                        _ = visited.Add(prevVer);
                    }
                }
            }
        }

        // 返回构建好的路径映射表
        return pathMap;
    }

    /// <summary>
    /// 尝试获取指定版本到当前版本的迁移路径
    /// </summary>
    /// <param name="fromVersion">起始版本</param>
    /// <param name="migrationPath">输出参数, 迁移路径数组</param>
    /// <returns>如果存在迁移路径则返回 <c>true</c>, 否则返回 <c>false</c></returns>
    private bool TryGetMigrationPath(Version fromVersion, [NotNullWhen(true)] out IConfigMigrator<TConfig>[]? migrationPath)
    {
        // 初始化输出参数
        migrationPath = null;

        // 用于构建迁移路径的列表
        List<IConfigMigrator<TConfig>> pathList = [];

        // 沿着路径映射表尝试向前追溯, 直到达到目标版本
        while (fromVersion != TConfig.CurrentVersion)
        {
            // 尝试获取当前版本的下一跳迁移器
            if (!_migrationPathMap.TryGetValue(fromVersion, out var migrator))
            {
                // 如果没有下一跳迁移器, 则说明路径不完整, 返回 false
                return false;
            }

            // 将下一跳迁移器添加到路径列表中
            pathList.Add(migrator);

            // 更新当前版本为迁移器的目标版本
            fromVersion = migrator.ToVersion;
        }

        // 将构建好的迁移路径列表转换为数组并赋值给输出参数
        migrationPath = [.. pathList];

        // 返回 true 表示成功获取迁移路径
        return true;
    }

    /// <summary>
    /// 尝试将配置迁移到当前版本
    /// </summary>
    /// <param name="config">需要迁移的配置实例</param>
    /// <param name="hasMigrated">输出参数, 指示是否进行了迁移</param>
    /// <param name="newConfig">输出参数, 迁移后的配置实例</param>
    /// <returns>如果迁移成功则返回 <c>true</c>, 否则返回 <c>false</c></returns>
    private bool TryMigrateConfig(JsonObject config, out bool hasMigrated, [NotNullWhen(true)] out TConfig? newConfig)
    {
        // 初始化输出参数
        hasMigrated = false;
        newConfig = null;

        // 当前配置版本
        var currentVersion = new Version();

        // 获取配置中的版本信息
        if (config.TryGetPropertyValue(nameof(IConfig<>.Version), out var versionNode) && versionNode is JsonValue versionValue && Version.TryParse(versionValue.GetValue<string>(), out var version)) { currentVersion = version; }

        // 如果当前版本已经是目标版本, 则无需迁移, 直接反序列化并返回
        if (currentVersion == TConfig.CurrentVersion)
        {
            try
            {
                newConfig = config.Deserialize<TConfig>(ConfigHelper.JsonOptions) ?? throw new JsonException("配置反序列化失败");
                return true;
            }
            catch (Exception ex)
            {
                RaiseConfigExceptionOccurred(ex);
                return false;
            }
        }

        // 尝试获取从当前配置版本到目标版本的迁移路径
        if (!TryGetMigrationPath(currentVersion, out var migrationPath)) { return false; }

        // 尝试进行迁移
        try
        {
            // 按照迁移路径依次应用每个迁移器
            foreach (var migrator in migrationPath) { config = migrator.Migrate(config); }

            // 将迁移后的 JsonObject 反序列化为目标配置类型的实例
            newConfig = config.Deserialize<TConfig>(ConfigHelper.JsonOptions) ?? throw new JsonException("迁移后的配置反序列化失败");

            // 标记已迁移
            hasMigrated = true;

            // 返回 true 表示迁移成功
            return true;
        }
        catch (Exception ex)
        {
            RaiseConfigExceptionOccurred(ex);
            return false;
        }
    }
}
