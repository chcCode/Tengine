using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 网络请求可选参数。
    /// </summary>
    public class NetworkRequestOptions
    {
        /// <summary>
        /// 请求超时时间（秒）。
        /// </summary>
        public float? Timeout { get; set; }

        /// <summary>
        /// 请求头。
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 是否跳过默认 Content-Type 自动设置。
        /// </summary>
        public bool SkipDefaultContentType { get; set; }

        /// <summary>
        /// 请求 Content-Type。
        /// </summary>
        public string ContentType { get; set; } = "application/json; charset=utf-8";
    }
}
