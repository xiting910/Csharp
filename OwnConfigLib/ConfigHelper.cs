using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OwnConfigLib;

/// <summary>
/// 配置库帮助类
/// </summary>
internal static class ConfigHelper
{
    /// <summary>
    /// 防抖间隔默认毫秒数
    /// </summary>
    public const int DefaultDebounceMilliseconds = 500;

    /// <summary>
    /// Json序列化选项
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// IO 操作时的缓冲区大小
    /// </summary>
    public const int BufferSize = 8192;

    /// <summary>
    /// IO 操作的默认文件选项
    /// </summary>
    public const FileOptions DefaultFileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

    /// <summary>
    /// 创建用于保存操作的 <see cref="FileStream"/>
    /// </summary>
    /// <param name="path">要保存的文件路径</param>
    /// <returns>用于保存操作的 <see cref="FileStream"/></returns>
    public static FileStream CreateSaveFileStream(string path) => new(path, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, DefaultFileOptions);

    /// <summary>
    /// 创建用于加载操作的 <see cref="FileStream"/>
    /// </summary>
    /// <param name="path">要加载的文件路径</param>
    /// <returns>用于加载操作的 <see cref="FileStream"/></returns>
    public static FileStream CreateLoadFileStream(string path) => new(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, DefaultFileOptions);
}
