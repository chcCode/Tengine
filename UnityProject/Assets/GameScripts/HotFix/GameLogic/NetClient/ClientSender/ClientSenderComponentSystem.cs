using System.Threading.Tasks;
using System;

namespace ET
{
    [EntitySystemOf(typeof(ClientSenderComponent))]
    [FriendOf(typeof(ClientSenderComponent))]
    public static partial class ClientSenderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSenderComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this ClientSenderComponent self)
        {
            self.RemoveFiberAsync().Coroutine();
        }

        private static async ETTask RemoveFiberAsync(this ClientSenderComponent self)
        {
            if (self.fiberId == 0)
            {
                return;
            }

            int fiberId = self.fiberId;
            self.fiberId = 0;
            await FiberManager.Instance.Remove(fiberId);
        }

        public static async ETTask DisposeAsync(this ClientSenderComponent self)
        {
            await self.RemoveFiberAsync();
            self.Dispose();
        }

        public static async ETTask<long> LoginAsync(this ClientSenderComponent self, string account, string password)
        {
            // Unity 客户端侧放在主线程调度，避免 ThreadPool Fiber 与主线程消息队列初始化时序不一致。
            self.fiberId = await FiberManager.Instance.Create(SchedulerType.Main, 0, SceneType.NetClient, "");
            self.netClientActorId = new ActorId(self.Fiber().Process, self.fiberId);
            Log.Info($"NetClient fiber created, actorId: {self.netClientActorId}");
            
            // MainThreadScheduler 会在 LateUpdate 末尾把新 Fiber 加入调度队列，等一帧再向 NetClient 投递消息。
            await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();

            Main2NetClient_Login main2NetClientLogin = Main2NetClient_Login.Create();
            main2NetClientLogin.OwnerFiberId = self.Fiber().Id;
            main2NetClientLogin.Account = account;
            main2NetClientLogin.Password = password;
            NetClient2Main_Login response = await self.Root().GetComponent<ProcessInnerSender>().Call(self.netClientActorId, main2NetClientLogin) as NetClient2Main_Login;
            if (response == null)
            {
                throw new Exception("Login failed: NetClient2Main_Login response is null.");
            }

            if (response.Error != ErrorCode.ERR_Success)
            {
                string message = string.IsNullOrEmpty(response.Message) ? "empty response message" : response.Message;
                throw new RpcException(response.Error, $"Login failed: error={response.Error}, message={message}");
            }

            if (response.PlayerId <= 0)
            {
                throw new Exception($"Login failed: invalid PlayerId {response.PlayerId}, error={response.Error}, message={response.Message}.");
            }

            return response.PlayerId;
        }

        public static void Send(this ClientSenderComponent self, IMessage message)
        {
            A2NetClient_Message a2NetClientMessage = A2NetClient_Message.Create();
            a2NetClientMessage.MessageObject = message;
            self.Root().GetComponent<ProcessInnerSender>().Send(self.netClientActorId, a2NetClientMessage);
        }

        public static async ETTask<IResponse> Call(this ClientSenderComponent self, IRequest request, bool needException = true)
        {
            A2NetClient_Request a2NetClientRequest = A2NetClient_Request.Create();
            a2NetClientRequest.MessageObject = request;
            using A2NetClient_Response a2NetClientResponse = await self.Root().GetComponent<ProcessInnerSender>().Call(self.netClientActorId, a2NetClientRequest) as A2NetClient_Response;
            IResponse response = a2NetClientResponse.MessageObject;
                        
            if (response.Error == ErrorCore.ERR_MessageTimeout)
            {
                throw new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: {request}, response: {response}");
            }

            if (needException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                throw new RpcException(response.Error, $"Rpc error: {request}, response: {response}");
            }
            return response;
        }

    }
}