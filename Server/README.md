# GameNetty Server

服务端完整说明见：

```text
../UnityProject/Docs/服务端说明.md
```

## 目录

```text
Server/
├── App/        # 程序入口
├── Core/       # ET 核心运行时
├── Loader/     # 初始化与 Hotfix.dll 加载
├── Model/      # 配置、协议、组件模型
├── Hotfix/     # 业务逻辑
└── ThirdParty/ # 第三方依赖
```

配套目录：

```text
../Config/      # 配置、协议、运行时 bytes
../Share/       # Analyzer / SourceGenerator / 工具
../Bin/         # 构建输出与运行目录
```

## 构建

```powershell
cd E:\aaa\unity\TEngine-main
dotnet build .\Server\Server.sln
```

## 启动

单进程：

```text
Server/Start Server(Single Process).bat
```

Watcher：

```text
Server/Start Watcher.bat
```

等价命令：

```powershell
cd E:\aaa\unity\TEngine-main\Bin
dotnet App.dll --Process=1 --StartConfig=StartConfig/Localhost --Console=1
dotnet App.dll --AppType=Watcher --StartConfig=StartConfig/Localhost --Console=1
```

## 协议与配置

协议源文件：

```text
../Config/Proto/
```

启动配置：

```text
../Config/Excel/StartConfig/
../Config/Generate/StartConfig/Localhost/
```

游戏配置：

```text
../Config/Excel/GameConfig/
../Config/Generate/
```
