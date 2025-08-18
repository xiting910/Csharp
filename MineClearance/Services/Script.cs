using System.Diagnostics;
using MineClearance.Utilities;

namespace MineClearance.Services;

/// <summary>
/// 脚本类, 用于创建和启动 powershell 脚本
/// </summary>
internal static class Script
{
    /// <summary>
    /// 用于更新程序的powershell脚本路径
    /// </summary>
    private static readonly string updatePowerShellScriptPath = Path.Combine(Path.GetTempPath(), "UpdateScript.ps1");

    /// <summary>
    /// 卸载脚本路径
    /// </summary>
    private static readonly string uninstallPowerShellScriptPath = Path.Combine(Path.GetTempPath(), "UninstallScript.ps1");

    /// <summary>
    /// 删除残留的所有脚本文件
    /// </summary>
    public static void RemoveAllResidualScripts()
    {
        try
        {
            // 删除所有脚本文件
            var scriptFiles = new[]
            {
                updatePowerShellScriptPath,
                uninstallPowerShellScriptPath
            };
            foreach (var scriptFile in scriptFiles)
            {
                if (File.Exists(scriptFile))
                {
                    File.Delete(scriptFile);
                }
            }
        }
        catch {/* 忽略异常 */ }
    }

    /// <summary>
    /// 创建并启动自动更新的powershell脚本
    /// </summary>
    public static void StartAutoUpdateScript()
    {
        try
        {
            // 创建批处理脚本内容, 使用7za.exe命令解压缩
            File.WriteAllText(updatePowerShellScriptPath, $@"
                chcp 65001 > $null
                [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
                Get-Process -Name ""{Path.GetFileNameWithoutExtension(Constants.ExecutableFileName)}"" -ErrorAction SilentlyContinue | ForEach-Object {{ $_.Kill() }}
                Remove-Item -Path ""{Constants.CurrentDirectory}"" -Recurse -Force
                & ""{Constants.SevenZipExe}"" x -y ""{Constants.SevenZipPath}"" -o""{Constants.ParentDirectory}""
                Remove-Item ""{Constants.SevenZipPath}""
                Start-Process ""{Constants.ExecutableFilePath}""
                Remove-Item -Path $MyInvocation.MyCommand.Path -Force
                ", System.Text.Encoding.UTF8);

            // 启动powershell脚本
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{updatePowerShellScriptPath}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"自动更新失败: {ex.Message}\n请手动将 {Constants.SevenZipPath} 解压到目录 {Constants.ParentDirectory} 下以完成更新 (如果该目录下已经有MineClearance文件夹则将其替换)", @"自动更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 创建并启动自动卸载的powershell脚本
    /// </summary>
    /// <param name="keepData">是否保留数据</param>
    public static void StartAutoUninstallScript(bool keepData)
    {
        try
        {
            // 要写入的脚本内容
            var scriptContent = $@"
                chcp 65001 > $null
                [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
                Set-Location ""{Path.GetTempPath()}""
                Get-Process -Name ""{Path.GetFileNameWithoutExtension(Constants.ExecutableFileName)}"" -ErrorAction SilentlyContinue | ForEach-Object {{ $_.Kill() }}";

            // 删除 Constants.ParentDirectory 文件夹
            scriptContent += $@"
                Remove-Item -Path ""{Constants.ParentDirectory}"" -Recurse -Force";

            // 如果不保留数据，则删除数据目录
            if (!keepData)
            {
                scriptContent += $@"
                    Remove-Item -Path ""{Constants.DataPath}"" -Recurse -Force";
            }

            // 如果更新文件存在，则删除更新文件
            if (File.Exists(Constants.SevenZipPath))
            {
                scriptContent += $@"
                    Remove-Item -Path ""{Constants.SevenZipPath}"" -Force";
            }

            // 如果更新脚本存在，则删除更新脚本
            if (File.Exists(updatePowerShellScriptPath))
            {
                scriptContent += $@"
                    Remove-Item -Path ""{updatePowerShellScriptPath}"" -Force";
            }

            // 删除卸载脚本
            scriptContent += $@"
                Remove-Item -Path $MyInvocation.MyCommand.Path -Force";

            // 创建脚本并写入内容
            File.WriteAllText(uninstallPowerShellScriptPath, scriptContent, System.Text.Encoding.UTF8);

            // 启动powershell脚本
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{uninstallPowerShellScriptPath}\"",
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"自动卸载失败: {ex.Message}", @"自动卸载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}