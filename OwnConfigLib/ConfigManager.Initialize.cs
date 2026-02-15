using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace OwnConfigLib;

// 配置管理器类的初始化部分
public partial class ConfigManager<TConfig>
{
    /// <summary>
    /// 私有无参构造函数, 禁止调用该方法创建实例
    /// </summary>
    private ConfigManager() => throw new NotSupportedException();

    /// <summary>
    /// 程序集内可见构造函数, 提供给工厂类使用, 防止外部实例化
    /// </summary>
    /// <param name="onDispose">当实例被释放时执行的回调操作, 参数为配置文件路径</param>
    /// <param name="filePath">配置文件路径</param>
    /// <param name="fileName">配置文件名</param>
    /// <param name="directory">配置文件所在目录</param>
    /// <param name="context">原始的同步上下文, 用于触发事件</param>
    /// <param name="factory">用于创建配置对象实例的工厂方法, 参数为同步上下文</param>
    /// <param name="migrators">配置迁移器实例数组, 用于实现配置从旧版本迁移到新版本的功能</param>
    internal ConfigManager(Action<string> onDispose, string filePath, string fileName, string directory, SynchronizationContext? context, Func<SynchronizationContext?, TConfig>? factory, IConfigMigrator<TConfig>[] migrators)
    {
        // 保存释放回调, 以允许在实例被释放时执行自定义操作, 参数为配置文件路径
        _onDispose = onDispose;

        // 保存原始的同步上下文, 用于触发事件
        _originalContext = context;

        // 初始化配置对象
        _config = factory?.Invoke(context) ?? new();

        // 设置配置文件路径
        _configFilePath = filePath;

        // 设置临时文件路径, 用于原子保存操作
        _tempFilePath = filePath + ".tmp";

        // 初始化迁移路径映射表, 构建从任意版本到当前版本的最短迁移路径
        _migrationPathMap = InitPathMap(migrators);

        // 初始化保存和加载计时器, 以便在配置更新或文件更改时触发相应的操作
        _saveTimer = new(OnSaveTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        _loadTimer = new(OnLoadTimerCallback, null, Timeout.Infinite, Timeout.Infinite);

        // 初始化文件监控器
        _fileWatcher = new(directory, fileName)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };
        _fileWatcher.Created += OnFileChanged;
        _fileWatcher.Changed += OnFileChanged;
        _fileWatcher.Error += OnFileWatcherError;
    }

    /// <summary>
    /// 初始化锁, 确保初始化过程的线程安全, 防止多个线程同时初始化导致竞争条件
    /// </summary>
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <summary>
    /// 标志位, 表示配置管理器是否已经完成初始化, 防止重复初始化和确保在使用前完成必要的设置
    /// </summary>
    private bool _isInitialized;

    /// <summary>
    /// 异步初始化配置管理器, 供工厂类在创建实例后调用
    /// </summary>
    /// <param name="token">取消令牌, 用于取消初始化过程</param>
    internal async Task InitializeAsync(CancellationToken token)
    {
        // 快速检查是否已初始化
        if (_isInitialized) { return; }

        // 获取初始化锁, 确保只有一个线程能够执行初始化过程
        try { await _initLock.WaitAsync(token).ConfigureAwait(false); }
        catch (OperationCanceledException) { return; }

        try
        {
            // 再次检查是否已初始化, 防止在等待锁的过程中其他线程已经完成了初始化
            if (_isInitialized) { return; }

            // 判断配置文件是否存在
            if (File.Exists(_configFilePath))
            {
                // 异步读取文件并反序列化配置对象
                await using var stream = ConfigHelper.CreateLoadFileStream(_configFilePath);
                var jsonObj = await JsonSerializer.DeserializeAsync<JsonObject>(stream, ConfigHelper.JsonOptions, token).ConfigureAwait(false) ?? throw new JsonException("初始化失败: 配置文件内容无效");

                // 尝试将反序列化的 JSON 对象迁移到当前版本的配置对象
                if (TryMigrateConfig(jsonObj, out var hasMigrated, out var migratedConfig))
                {
                    // 将迁移后的配置对象赋值给当前配置实例
                    _fastCopyAction(_config, migratedConfig);

                    // 如果发生了迁移, 则保存迁移后的配置对象到文件
                    if (hasMigrated)
                    {
                        _ = await SaveAsync(token).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                // 如果文件不存在, 则创建一个新的配置文件
                _ = await SaveAsync(token).ConfigureAwait(false);
            }

            // 订阅配置对象的属性更改事件
            _config.PropertyChanged += OnConfigPropertyChanged;

            // 启动文件监控器, 以便监控配置文件的变化
            _fileWatcher.EnableRaisingEvents = true;

            // 标记初始化完成
            _isInitialized = true;
        }
        catch (OperationCanceledException) { /* 初始化被取消 */ }
        catch (Exception ex)
        {
            // 发生异常时触发事件
            RaiseConfigExceptionOccurred(ex);
        }
        finally
        {
            // 释放初始化锁
            _ = _initLock.Release();
        }
    }
}
