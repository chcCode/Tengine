# 网络模块 API 速查

> **适用场景**：GameNetty 客户端初始化、登录、Session 消息发送/请求 | **关联文档**：[modules.md](modules.md)（GameModule 访问）、[hotfix-workflow.md](hotfix-workflow.md)（热更边界）

## 核心 API

```csharp
GameModule.Network   // INetworkModule — GameNetty 网络模块
```

`NetworkModule` 位于 `TEngine.Runtime`，负责初始化 GameNetty ET 运行时并驱动 `FiberManager`。业务登录、协议消息与 NetClient 组件位于热更层 `GameScripts/HotFix/GameLogic/NetClient` 与 `GameProto/Generate/Message`。

---

## 初始化

```csharp
await GameModule.Network.InitializeAsync();
```

推荐在 `GameApp.StartGameLogic()` 中先初始化，再进入 UI/业务流程。

初始化内容：

- 注册 `ET.Logger`、`TimeInfo`、`FiberManager`
- 扫描 `TEngine.Runtime`、`ET.Core`、`ET.Network`、`GameLogic`、`GameProto` 中的 ET 特性类型
- 调用 `ET.Entry.Start()` 创建主 Fiber
- 保存 GameNetty `Root`

---

## 登录

```csharp
await ET.LoginHelper.Login(GameModule.Network.Root, account, password);
```

默认路由配置：

```csharp
ET.ConstValue.RouterHttpHost = "127.0.0.1";
ET.ConstValue.RouterHttpPort = 30300;
```

登录成功后，Gate `Session` 会保存到 `SessionComponent`，可通过 `GameModule.Network.Session` 获取。

---

## 发送与请求

```csharp
// Send
C2G_Ping ping = C2G_Ping.Create();
GameModule.Network.Send(ping);

// Call
C2G_Ping request = C2G_Ping.Create();
G2C_Ping response = await GameModule.Network.CallAsync<G2C_Ping>(request);
```

断开：

```csharp
GameModule.Network.Disconnect();
```

---

## 当前导入内容

```text
Assets/ET/                                      # ET Core / Network / ThirdParty
Assets/UnityWebSocket/                          # WebGL WebSocket 支持
Assets/GameScripts/HotFix/GameProto/Generate/   # 示例协议消息
Assets/GameScripts/HotFix/GameLogic/NetClient/  # 登录、路由、心跳、发送器
```

依赖包：

```json
"com.cysharp.memorypack": "https://gitee.com/game-for-all_0/MemoryPack.git?path=src/MemoryPack.Unity/Assets/Plugins/MemoryPack"
```

---

## 常见错误

| 错误 | 修复 |
|------|------|
| 未初始化就 Send/Call | 先 `await GameModule.Network.InitializeAsync()` |
| `Session` 为空 | 先 `ET.LoginHelper.Login(GameModule.Network.Root, account, password)` |
| 找不到 `MemoryPack` | 让 Unity 重新解析 `Packages/manifest.json` |
| 协议反序列化失败 | 同步前后端 opcode 与 MemoryPack 字段顺序 |
| WebGL 连接失败 | 确保服务端提供 WebSocket 入口 |

---

## 交叉引用

| 关联主题 | 文档 |
|---------|------|
| 模块访问 | modules.md |
| 热更边界 | hotfix-workflow.md |
| 用户文档 | Books/3-8-网络模块.md |
| GameNetty | https://github.com/Alex-Rachel/GameNetty/tree/main |
