# C# 编程练习项目
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
- [C# 编程练习项目](#c-编程练习项目)
  - [项目说明](#项目说明)
  - [系统要求](#系统要求)
  - [下载项目](#下载项目)
  - [注意事项](#注意事项)
  - [License](#license)
  - [其他](#其他)

## 项目说明
这个仓库中的项目是一些C#编程的尝试，旨在练习使用C#语言的各种功能和特性。主要项目包括：
- **CleanUpRegedit**: 一个用于清理Windows注册表的工具
  - 可以扫描并删除无效（没有任何子键和值）的注册表项
  - 支持通过关键字搜索要删除的注册表项，实现批量删除
  - 删除操作均会记录日志，方便用户查看删除详情
  - 注意：程序没有提供任何备份和恢复功能，请谨慎使用
- **FabricServerManager**: 一个自动管理`Windows`系统的`Carpet`端`Fabric`服务器程序
  - 设计这个程序主要是为了偷懒，不想每次都手动输入`stop`指令来关闭服务器。主要功能是在服务器无玩家活动一定时间后，程序会自动执行`stop`指令关闭服务器
  - 支持在运行时修改程序的配置，如服务器无玩家活动后自动关闭的时间间隔、服务器启动的内存参数等
  - 程序基于锂模组来识别服务器是否成功启动，可能不适用大部分`Fabric`服务器
  - 可以通过修改`Constants.cs`文件中的`ServerStartSuccessMessage`变量来指定要识别的服务器启动成功消息
  - 对于程序的识别指令执行功能，只有在装了`Carpet`模组的`Fabric`服务器并且服务器开启了`Carpet`的`recordPlayerCommand`选项时才能正常工作
  - 程序支持上下键切换历史指令，并且可以通过回车键执行指令
  - 输入行一直保持在末尾，不会被输出内容覆盖
- **Maze**: 一个使用了`WinForms`的迷宫寻路可视化程序
  - 迷宫的大小为`125x80`，可以通过鼠标按住滑动自定义设置起点、终点和障碍物
  - 随机生成迷宫算法支持完全随机生成、`DFS生成`、`递归分割生成`和`Prim生成`四种方式
  - 支持3种寻路算法，分别是`BFS`、`DFS`和`A*`，支持选择是否显示寻路过程
  - 支持迷宫的保存和加载功能
- **WindowsUpdateManager**: 一个使用了`WinForms`的Windows更新管理程序
  - 提供了可视化界面，方便用户管理Windows更新
  - 支持一键暂停更新（最多可暂停至3001/01/01）、一键恢复更新功能

## 系统要求
⚠️ **重要提醒**: 本仓库中的项目仅支持Windows操作系统运行（WindowsUpdateManager项目还需Windows版本 10 及以上）

## 下载项目
Releases页面提供了项目打包成的exe可执行文件，您可以直接下载并运行这些文件

## 注意事项
- **FabricServerManager**:
  - 只支持以fabric-server-开头、以.jar结尾的`Fabric`服务器文件名
  - 服务器只有安装了`Carpet`模组并且开启了`Carpet`的`recordPlayerCommand`选项后程序的识别指令执行功能才能正常工作
  - 如果您的服务器启动完成的最后一条信息不是`Lithium Cached BlockState Flags are disabled!`, 请下载源代码自行修改`Constants.cs`文件中的`ServerStartSuccessMessage`变量来指定要识别的服务器启动成功消息
  
## License
本项目采用 MIT 许可证，详见仓库根目录的 [LICENSE](./LICENSE) 文件

## 其他
如果您对项目有任何建议或发现问题，请提交issue或pull request