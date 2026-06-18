using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TEngine
{
    /// <summary>
    /// 网络模块接口。
    /// </summary>
    public interface INetworkModule
    {
        /// <summary>
        /// 服务器基础地址，例如 http://127.0.0.1:8080。
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// 默认请求超时时间（秒）。
        /// </summary>
        float DefaultTimeout { get; set; }

        /// <summary>
        /// 鉴权 Token，设置后自动附加 Authorization 请求头。
        /// </summary>
        string AuthToken { get; set; }

        /// <summary>
        /// 设置默认请求头。
        /// </summary>
        void SetDefaultHeader(string key, string value);

        /// <summary>
        /// 移除默认请求头。
        /// </summary>
        void RemoveDefaultHeader(string key);

        /// <summary>
        /// 清空所有默认请求头。
        /// </summary>
        void ClearDefaultHeaders();

        /// <summary>
        /// 发起 GET 请求。
        /// </summary>
        UniTask<NetworkResponse> GetAsync(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发起 POST 请求。
        /// </summary>
        UniTask<NetworkResponse> PostAsync(string url, string body, NetworkRequestOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发起表单 POST 请求。
        /// </summary>
        UniTask<NetworkResponse> PostFormAsync(string url, Dictionary<string, string> formFields, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 发起 PUT 请求。
        /// </summary>
        UniTask<NetworkResponse> PutAsync(string url, string body, NetworkRequestOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发起 DELETE 请求。
        /// </summary>
        UniTask<NetworkResponse> DeleteAsync(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发起 GET 请求并反序列化 JSON。
        /// </summary>
        UniTask<NetworkResponse<T>> GetJsonAsync<T>(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发起 POST 请求并序列化/反序列化 JSON。
        /// </summary>
        UniTask<NetworkResponse<TResponse>> PostJsonAsync<TRequest, TResponse>(string url, TRequest data, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 发起 POST 请求并序列化 JSON 请求体。
        /// </summary>
        UniTask<NetworkResponse> PostJsonAsync<TRequest>(string url, TRequest data, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 下载二进制数据。
        /// </summary>
        UniTask<NetworkResponse> DownloadBytesAsync(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 下载文件到本地路径。
        /// </summary>
        UniTask<NetworkResponse> DownloadFileAsync(string url, string savePath, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 取消所有进行中的请求。
        /// </summary>
        void CancelAllRequests();
    }
}
