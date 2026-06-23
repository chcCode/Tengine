using Cysharp.Threading.Tasks;
using ET;

namespace TEngine
{
    /// <summary>
    /// GameNetty 网络模块接口。
    /// </summary>
    public interface INetworkModule
    {
        /// <summary>
        /// GameNetty 是否已经初始化。
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// GameNetty 主场景 Root。
        /// </summary>
        Scene Root { get; }

        /// <summary>
        /// 当前 Gate 会话。
        /// </summary>
        Session Session { get; }

        /// <summary>
        /// 初始化 GameNetty 运行时。
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// 发送无响应消息。
        /// </summary>
        void Send(IMessage message);

        /// <summary>
        /// 发送请求并等待响应。
        /// </summary>
        UniTask<TResponse> CallAsync<TResponse>(IRequest request, int timeout = 0) where TResponse : class, IResponse;

        /// <summary>
        /// 断开当前会话。
        /// </summary>
        void Disconnect();
    }
}
