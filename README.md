# C# 项目
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
- [C# 项目](#c-项目)
  - [项目说明](#项目说明)
    - [总览](#总览)
    - [1) CleanUpRegedit](#1-cleanupregedit)
    - [2) Maze](#2-maze)
    - [3) MineClearance](#3-mineclearance)
    - [4) OwnConfigLib](#4-ownconfiglib)
    - [5) OwnRationalLib](#5-ownrationallib)
    - [6) ServerManager](#6-servermanager)
    - [7) WindowsUpdateManager](#7-windowsupdatemanager)
  - [下载项目](#下载项目)
  - [对于开发者](#对于开发者)
    - [环境要求](#环境要求)
    - [获取源码](#获取源码)
    - [构建项目](#构建项目)
    - [运行指定项目](#运行指定项目)
    - [调试与注意事项](#调试与注意事项)
  - [许可证](#许可证)
  - [其他](#其他)

## 项目说明

### 总览

| 项目                 | 类型       | 目标框架        | 简介                                 |
| -------------------- | ---------- | --------------- | ------------------------------------ |
| CleanUpRegedit       | 控制台工具 | net10.0         | 清理无效的安装/卸载注册表项          |
| Maze                 | WinForms   | net10.0-windows | 迷宫生成与路径搜索可视化工具         |
| MineClearance        | WinForms   | net10.0-windows | 扫雷游戏（含历史记录与配置）         |
| OwnConfigLib         | 类库       | net10.0         | 配置管理库（自动保存、热重载、防抖） |
| OwnRationalLib       | 类库       | net10.0         | 任意精度有理数库 BigRational         |
| ServerManager        | 控制台工具 | net10.0         | Minecraft 服务器启动与运行管理工具   |
| WindowsUpdateManager | WinForms   | net10.0-windows | Windows 更新暂停/恢复管理工具        |

---

### 1) CleanUpRegedit

用于扫描并清理以下注册表位置中的无效项：

- `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products`
- `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`

主要功能：

- 启动时自动检测管理员权限，必要时提权重启
- 支持删除空键、按编号批量选择、按关键字筛选
- 删除前支持二次确认，操作日志会保存到本地日志文件

适用场景：系统清理、卸载残留排查

---

### 2) Maze

迷宫可视化工具，支持迷宫生成与路径搜索演示

主要功能：

- 迷宫生成算法：`Random`、`DFS`、`RecursiveDivision`、`Prim`
- 路径搜索算法：`BFS`、`DFS`、`A*`
- 支持搜索过程可视化、路径淡出显示、搜索取消
- 支持自定义起点/终点与障碍物编辑

适用场景：算法学习、教学演示、图搜索实验

---

### 3) MineClearance

WinForms 扫雷游戏

主要功能：

- 难度支持：`Easy`、`Medium`、`Hard`、`Hell`、`Custom`
- 本地历史记录持久化（JSON），并带旧版本数据迁移
- 单实例运行保护
- 自动启动项管理（注册表）与无效启动项清理
- 日志记录与异常捕获

适用场景：日常娱乐、扫雷逻辑练习

---

### 4) OwnConfigLib

通用配置管理类库，核心为 `ConfigManager<TConfig>`

主要特性：

- 基于 `INotifyPropertyChanged` 的配置对象管理
- 自动保存（`Auto Save`）与热重载（`Hot Reload`）支持
- 文件变更与保存防抖机制（默认 500ms）
- 配置版本迁移支持（`IConfigMigrator`）
- 异常事件回调，便于在 UI 线程中处理异常

适用场景：桌面应用配置管理、可热更新配置文件场景

---

### 5) OwnRationalLib

任意精度有理数类库，核心类型为 `BigRational`

主要特性：

- 基于 `BigInteger`，支持高精度分数运算
- 支持 `INumber<T>` 等现代数值接口
- 支持解析整数、小数、分数、循环小数（例如 `0.1(6)`）
- 内置常量 `Pi` 与 `E`（精度到小数点后 1000 位）
- 支持 NaN、正负无穷等特殊值

适用场景：高精度数学计算、分数精确计算

---

### 6) ServerManager

用于管理 Java 我的世界服务器进程（如游戏服务器）的控制台工具。

主要功能：

- 启动前自动检测 Java 环境
- 支持读取与保存服务器配置（jar 路径、内存参数等）
- 实时读取服务器输出并处理常见日志信息
- 支持无人在线超时自动停服
- 支持输入命令透传到服务器进程

适用场景：个人服/小型服日常运维

---

### 7) WindowsUpdateManager

Windows 更新管理工具（WinForms）

主要功能：

- 快速打开系统更新页面
- 暂停更新到指定日期（写入系统更新相关注册表项）
- 可选禁用更新服务与计划任务以“彻底暂停”
- 一键恢复更新（恢复服务、任务与暂停配置）
- 需要时可触发立即重启

注意事项：该工具会修改系统更新相关配置，请按需使用

## 下载项目

`Releases` 页面提供各项目打包后的 exe 可执行文件，可按需下载并在 Windows 上直接运行

如果您想在其他平台上使用或修改项目，建议直接克隆源码并构建运行（部分项目如 WinForms 可能仅限 Windows）

## 对于开发者

### 环境要求

- Windows 10/11（WinForms 项目建议）
- .NET 10 SDK
- 可选：Visual Studio 2022 / VS Code + C# 扩展
- `ServerManager` 需要 Java 环境（`java` 可在命令行中调用）

### 获取源码

```bash
git clone https://github.com/xiting910/Csharp.git
cd Csharp
```

### 构建项目

在仓库根目录构建全部项目：

```bash
dotnet build Csharp.slnx
```

或使用仓库提供的脚本（会先清理 `bin/obj` 再构建）：

```bat
ReBuild.bat
```

### 运行指定项目

```bash
dotnet run --project CleanUpRegedit/CleanUpRegedit.csproj
dotnet run --project Maze/Maze.csproj
dotnet run --project MineClearance/MineClearance.csproj
dotnet run --project ServerManager/ServerManager.csproj
dotnet run --project WindowsUpdateManager/WindowsUpdateManager.csproj
```

### 调试与注意事项

- `CleanUpRegedit` 和 `WindowsUpdateManager` 涉及注册表/服务操作，建议使用管理员权限运行
- WinForms 项目仅在 Windows 上可运行
- 当前仓库未提供统一测试项目，可先通过 `dotnet build` 与手动运行进行验证

## 许可证

本项目采用 MIT 许可证，详见仓库根目录的 [LICENSE](./LICENSE) 文件

## 其他

如果你对项目有建议，或发现 Bug，欢迎提交 Issue 或 Pull Request
