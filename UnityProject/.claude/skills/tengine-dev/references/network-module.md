# 网络模块 API 速查

> **适用场景**：HTTP/REST 请求、JSON API、文件下载 | **关联文档**：[modules.md](modules.md)（GameModule 访问）、[hotfix-workflow.md](hotfix-workflow.md)（热更边界）

## 核心 API

```csharp
GameModule.Network   // INetworkModule — HTTP 网络模块
```

模块位于 `TEngine.Runtime`（AOT），热更代码通过 `GameModule.Network` 调用。首次访问时 `ModuleSystem` 按 `INetworkModule` → `NetworkModule` 约定自动创建。

---

## 配置

```csharp
GameModule.Network.BaseUrl = "http://127.0.0.1:8080";
GameModule.Network.DefaultTimeout = 10f;
GameModule.Network.AuthToken = "token";  // 自动附加 Authorization: Bearer {token}

GameModule.Network.SetDefaultHeader("X-Platform", "Android");
GameModule.Network.RemoveDefaultHeader("X-Platform");
GameModule.Network.ClearDefaultHeaders();
```

---

## HTTP 请求

```csharp
// GET
NetworkResponse resp = await GameModule.Network.GetAsync("/api/user");

// POST 文本
NetworkResponse resp = await GameModule.Network.PostAsync("/api/data", jsonBody);

// POST 表单
NetworkResponse resp = await GameModule.Network.PostFormAsync("/api/login", formFields);

// PUT / DELETE
NetworkResponse resp = await GameModule.Network.PutAsync("/api/user", body);
NetworkResponse resp = await GameModule.Network.DeleteAsync("/api/user/1");

// GET JSON
NetworkResponse<UserInfo> resp = await GameModule.Network.GetJsonAsync<UserInfo>("/api/user");

// POST JSON（请求 + 响应反序列化）
NetworkResponse<LoginResult> resp = await GameModule.Network
    .PostJsonAsync<LoginReq, LoginResult>("/api/login", req);

// POST JSON（仅序列化请求体）
NetworkResponse resp = await GameModule.Network.PostJsonAsync("/api/login", req);

// 下载
NetworkResponse resp = await GameModule.Network.DownloadBytesAsync("/api/file");
NetworkResponse resp = await GameModule.Network.DownloadFileAsync("/api/file", savePath);

// 取消全部
GameModule.Network.CancelAllRequests();
```

---

## NetworkResponse

```csharp
response.IsSuccess    // 是否成功
response.StatusCode   // HTTP 状态码
response.Text         // 响应文本
response.Data         // 响应二进制
response.Error        // 错误信息
response.IsCanceled   // 是否取消
response.IsTimeout    // 是否超时
response.Url          // 实际请求 URL

// 泛型版本
NetworkResponse<T>.Result  // JSON 反序列化结果
response.ToJsonResponse<T>()  // 手动反序列化
```

---

## NetworkRequestOptions

```csharp
var options = new NetworkRequestOptions
{
    Timeout = 30f,
    ContentType = "application/json; charset=utf-8",
    SkipDefaultContentType = false,
    Headers = new Dictionary<string, string> { { "X-Id", "1" } }
};

await GameModule.Network.GetAsync("/api/data", options, cancellationToken);
```

---

## URL 规则

- 以 `http://` 或 `https://` 开头 → 直接使用
- 否则拼接 `BaseUrl + url`（`BaseUrl` 为空时抛异常）
- 推荐业务层封装 URL，UI 不直接拼路径

---

## JSON 限制

- 使用 `Utility.Json`（默认 `JsonUtility`）
- DTO 需 `[Serializable]` + public **field**（不支持 property）
- 复杂 JSON 需替换 `RootModule.jsonHelperTypeName`

---

## 常见错误

| 错误写法 | 正确写法 | 原因 |
|---------|---------|------|
| `ModuleSystem.GetModule<INetworkModule>()` | `GameModule.Network` | 统一模块访问 |
| `Utility.Http.Get`（Coroutine） | `await GameModule.Network.GetAsync` | 异步优先，统一响应 |
| JSON 类用 property | 用 field + `[Serializable]` | JsonUtility 限制 |
| UI 销毁后不取消请求 | 传 `CancellationToken`，OnDestroy 取消 | 避免空引用 |
| 相对路径无 BaseUrl | 设置 BaseUrl 或用完整 URL | URL 拼接依赖 BaseUrl |

---

## 交叉引用

| 关联主题 | 文档 |
|---------|------|
| 模块访问 | modules.md |
| 事件通知 UI | event-system.md |
| 热更边界 | hotfix-workflow.md |
| 用户文档 | Books/3-8-网络模块.md |
