using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TEngine
{
    internal class NetworkModule : Module, INetworkModule
    {
        private readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
        private readonly HashSet<UnityWebRequest> _activeRequests = new HashSet<UnityWebRequest>();
        private readonly object _lock = new object();

        public string BaseUrl { get; set; } = string.Empty;

        public float DefaultTimeout { get; set; } = 10f;

        public string AuthToken { get; set; } = string.Empty;

        public override void OnInit()
        {
            _defaultHeaders.Clear();
            _activeRequests.Clear();
            BaseUrl = string.Empty;
            DefaultTimeout = 10f;
            AuthToken = string.Empty;
        }

        public override void Shutdown()
        {
            CancelAllRequests();
            _defaultHeaders.Clear();
            BaseUrl = string.Empty;
            AuthToken = string.Empty;
        }

        public void SetDefaultHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Header key is invalid.");
            }

            _defaultHeaders[key] = value ?? string.Empty;
        }

        public void RemoveDefaultHeader(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            _defaultHeaders.Remove(key);
        }

        public void ClearDefaultHeaders()
        {
            _defaultHeaders.Clear();
        }

        public UniTask<NetworkResponse> GetAsync(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default)
        {
            string requestUrl = BuildUrl(url);
            UnityWebRequest request = UnityWebRequest.Get(requestUrl);
            ApplyHeaders(request, options);
            return SendRequestAsync(request, requestUrl, options, cancellationToken);
        }

        public UniTask<NetworkResponse> PostAsync(string url, string body, NetworkRequestOptions options = null, CancellationToken cancellationToken = default)
        {
            string requestUrl = BuildUrl(url);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
            UnityWebRequest request = new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(request, options, setContentType: !string.IsNullOrEmpty(body));
            return SendRequestAsync(request, requestUrl, options, cancellationToken);
        }

        public UniTask<NetworkResponse> PostFormAsync(string url, Dictionary<string, string> formFields, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            string requestUrl = BuildUrl(url);
            WWWForm form = new WWWForm();
            if (formFields != null)
            {
                foreach (KeyValuePair<string, string> pair in formFields)
                {
                    form.AddField(pair.Key, pair.Value);
                }
            }

            UnityWebRequest request = UnityWebRequest.Post(requestUrl, form);
            ApplyHeaders(request, options, setContentType: false);
            return SendRequestAsync(request, requestUrl, options, cancellationToken);
        }

        public UniTask<NetworkResponse> PutAsync(string url, string body, NetworkRequestOptions options = null, CancellationToken cancellationToken = default)
        {
            string requestUrl = BuildUrl(url);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
            UnityWebRequest request = new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbPUT);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(request, options, setContentType: !string.IsNullOrEmpty(body));
            return SendRequestAsync(request, requestUrl, options, cancellationToken);
        }

        public UniTask<NetworkResponse> DeleteAsync(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default)
        {
            string requestUrl = BuildUrl(url);
            UnityWebRequest request = UnityWebRequest.Delete(requestUrl);
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(request, options, setContentType: false);
            return SendRequestAsync(request, requestUrl, options, cancellationToken);
        }

        public async UniTask<NetworkResponse<T>> GetJsonAsync<T>(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default)
        {
            NetworkResponse response = await GetAsync(url, options, cancellationToken);
            return response.ToJsonResponse<T>();
        }

        public async UniTask<NetworkResponse<TResponse>> PostJsonAsync<TRequest, TResponse>(string url, TRequest data, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            NetworkResponse response = await PostJsonAsync(url, data, options, cancellationToken);
            return response.ToJsonResponse<TResponse>();
        }

        public UniTask<NetworkResponse> PostJsonAsync<TRequest>(string url, TRequest data, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new NetworkRequestOptions();
            if (!options.SkipDefaultContentType)
            {
                options.ContentType = "application/json; charset=utf-8";
            }

            string json = Utility.Json.ToJson(data);
            return PostAsync(url, json, options, cancellationToken);
        }

        public UniTask<NetworkResponse> DownloadBytesAsync(string url, NetworkRequestOptions options = null, CancellationToken cancellationToken = default)
        {
            return GetAsync(url, options, cancellationToken);
        }

        public UniTask<NetworkResponse> DownloadFileAsync(string url, string savePath, NetworkRequestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(savePath))
            {
                throw new GameFrameworkException("Save path is invalid.");
            }

            string requestUrl = BuildUrl(url);
            UnityWebRequest request = new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbGET);
            request.downloadHandler = new DownloadHandlerFile(savePath);
            ApplyHeaders(request, options, setContentType: false);
            return SendRequestAsync(request, requestUrl, options, cancellationToken);
        }

        public void CancelAllRequests()
        {
            lock (_lock)
            {
                foreach (UnityWebRequest request in _activeRequests)
                {
                    request?.Abort();
                }

                _activeRequests.Clear();
            }
        }

        private string BuildUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new GameFrameworkException("Request url is invalid.");
            }

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            if (string.IsNullOrEmpty(BaseUrl))
            {
                throw new GameFrameworkException(Utility.Text.Format("BaseUrl is empty, can not build url for '{0}'.", url));
            }

            string baseUrl = BaseUrl.TrimEnd('/');
            string relativeUrl = url.StartsWith("/") ? url : Utility.Text.Format("/{0}", url);
            return Utility.Text.Format("{0}{1}", baseUrl, relativeUrl);
        }

        private void ApplyHeaders(UnityWebRequest request, NetworkRequestOptions options, bool setContentType = true)
        {
            foreach (KeyValuePair<string, string> pair in _defaultHeaders)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }

            if (!string.IsNullOrEmpty(AuthToken))
            {
                request.SetRequestHeader("Authorization", Utility.Text.Format("Bearer {0}", AuthToken));
            }

            options ??= new NetworkRequestOptions();
            if (options.Headers != null)
            {
                foreach (KeyValuePair<string, string> pair in options.Headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }
            }

            if (setContentType && !options.SkipDefaultContentType && request.uploadHandler != null)
            {
                request.SetRequestHeader("Content-Type", options.ContentType);
            }
        }

        private async UniTask<NetworkResponse> SendRequestAsync(UnityWebRequest request, string requestUrl, NetworkRequestOptions options,
            CancellationToken cancellationToken)
        {
            float timeout = options?.Timeout ?? DefaultTimeout;
            using CancellationTokenSource timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));
            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            lock (_lock)
            {
                _activeRequests.Add(request);
            }

            try
            {
                (bool isCanceled, _) = await request.SendWebRequest().WithCancellation(linkedCts.Token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log.Warning("Network request canceled. Url: {0}", requestUrl);
                        return NetworkResponse.Canceled(requestUrl);
                    }

                    Log.Warning("Network request timeout. Url: {0}", requestUrl);
                    return NetworkResponse.Timeout(requestUrl);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return NetworkResponse.Success(requestUrl, request.responseCode, request.downloadHandler?.text, request.downloadHandler?.data);
                }

                Log.Warning("Network request failed. Url: {0}, Code: {1}, Error: {2}", requestUrl, request.responseCode, request.error);
                return NetworkResponse.Fail(requestUrl, request.responseCode, request.error, request.downloadHandler?.text);
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return NetworkResponse.Canceled(requestUrl);
                }

                return NetworkResponse.Timeout(requestUrl);
            }
            catch (Exception exception)
            {
                Log.Error("Network request exception. Url: {0}, Error: {1}", requestUrl, exception.Message);
                return NetworkResponse.Fail(requestUrl, 0, exception.Message);
            }
            finally
            {
                lock (_lock)
                {
                    _activeRequests.Remove(request);
                }

                request.Dispose();
            }
        }
    }
}
