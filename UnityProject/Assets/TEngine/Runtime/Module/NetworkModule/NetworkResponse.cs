using System;

namespace TEngine
{
    /// <summary>
    /// 网络请求响应结果。
    /// </summary>
    public class NetworkResponse
    {
        /// <summary>
        /// 请求是否成功。
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// HTTP 状态码。
        /// </summary>
        public long StatusCode { get; set; }

        /// <summary>
        /// 响应文本内容。
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 响应二进制内容。
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 错误信息。
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 是否被取消。
        /// </summary>
        public bool IsCanceled { get; set; }

        /// <summary>
        /// 是否超时。
        /// </summary>
        public bool IsTimeout { get; set; }

        /// <summary>
        /// 请求 URL。
        /// </summary>
        public string Url { get; set; }

        public static NetworkResponse Success(string url, long statusCode, string text, byte[] data = null)
        {
            return new NetworkResponse
            {
                IsSuccess = true,
                Url = url,
                StatusCode = statusCode,
                Text = text ?? string.Empty,
                Data = data
            };
        }

        public static NetworkResponse Fail(string url, long statusCode, string error, string text = null)
        {
            return new NetworkResponse
            {
                IsSuccess = false,
                Url = url,
                StatusCode = statusCode,
                Error = error ?? string.Empty,
                Text = text ?? string.Empty
            };
        }

        public static NetworkResponse Canceled(string url)
        {
            return new NetworkResponse
            {
                IsSuccess = false,
                Url = url,
                IsCanceled = true,
                Error = "Request canceled."
            };
        }

        public static NetworkResponse Timeout(string url)
        {
            return new NetworkResponse
            {
                IsSuccess = false,
                Url = url,
                IsTimeout = true,
                Error = "Request timeout."
            };
        }

        /// <summary>
        /// 将响应文本反序列化为指定类型。
        /// </summary>
        public NetworkResponse<T> ToJsonResponse<T>()
        {
            var response = new NetworkResponse<T>
            {
                IsSuccess = IsSuccess,
                StatusCode = StatusCode,
                Text = Text,
                Data = Data,
                Error = Error,
                IsCanceled = IsCanceled,
                IsTimeout = IsTimeout,
                Url = Url
            };

            if (!IsSuccess || string.IsNullOrEmpty(Text))
            {
                return response;
            }

            try
            {
                response.Result = Utility.Json.ToObject<T>(Text);
            }
            catch (Exception exception)
            {
                response.IsSuccess = false;
                response.Error = Utility.Text.Format("JSON deserialize failed: {0}", exception.Message);
            }

            return response;
        }
    }

    /// <summary>
    /// 带泛型结果的网络请求响应。
    /// </summary>
    public class NetworkResponse<T> : NetworkResponse
    {
        /// <summary>
        /// 反序列化后的结果对象。
        /// </summary>
        public T Result { get; set; }
    }
}
