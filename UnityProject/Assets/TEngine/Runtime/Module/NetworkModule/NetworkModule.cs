using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using ET;

namespace TEngine
{
    internal class NetworkModule : Module, INetworkModule, IUpdateModule
    {
        private static NetworkModule _current;

        private bool _hasUnhandledExceptionHandler;

        public bool IsInitialized { get; private set; }

        public Scene Root { get; private set; }

        public Session Session => Root?.GetComponent<SessionComponent>()?.Session;

        public override void OnInit()
        {
            _current = this;
            IsInitialized = false;
            Root = null;
        }

        public override void Shutdown()
        {
            if (_hasUnhandledExceptionHandler)
            {
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                _hasUnhandledExceptionHandler = false;
            }

            if (IsInitialized)
            {
                World.Instance.Dispose();
            }

            IsInitialized = false;
            Root = null;
            _current = null;
        }

        public async UniTask InitializeAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            if (!_hasUnhandledExceptionHandler)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                _hasUnhandledExceptionHandler = true;
            }

            EntitySerializeRegister.Init();
            World.Instance.AddSingleton<ET.Logger>().Log = new UnityLogger();
            ETTask.ExceptionHandler += ET.Log.Error;
            World.Instance.AddSingleton<TimeInfo>();
            World.Instance.AddSingleton<FiberManager>();

            Assembly[] assemblies = GetCodeAssemblies();
            World.Instance.AddSingleton<CodeTypes, Assembly[]>(assemblies);

            IStaticMethod start = new StaticMethod(typeof(Entry).Assembly, "ET.Entry", "Start");
            start.Run();

            await UniTask.WaitUntil(() => Root != null);
            IsInitialized = true;
        }

        public void Send(IMessage message)
        {
            Session session = GetSession();
            session.Send(message);
        }

        public async UniTask<TResponse> CallAsync<TResponse>(IRequest request, int timeout = 0) where TResponse : class, IResponse
        {
            Session session = GetSession();
            IResponse response = await session.Call(request, timeout);
            return response as TResponse;
        }

        public void Disconnect()
        {
            SessionComponent sessionComponent = Root?.GetComponent<SessionComponent>();
            Session session = sessionComponent?.Session;
            if (session != null)
            {
                session.Dispose();
            }

            if (sessionComponent != null)
            {
                Root.RemoveComponent<SessionComponent>();
            }
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            TimeInfo.Instance?.Update();
            FiberManager.Instance?.Update();
            FiberManager.Instance?.LateUpdate();
        }

        private static Assembly[] GetCodeAssemblies()
        {
            var assemblyMap = new Dictionary<string, Assembly>();
            AddAssembly(assemblyMap, typeof(Entity).Assembly);
            AddAssembly(assemblyMap, typeof(Entry).Assembly);
            AddAssembly(assemblyMap, typeof(NetworkModule).Assembly);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;
                if (assemblyName == "GameLogic" || assemblyName == "GameProto")
                {
                    AddAssembly(assemblyMap, assembly);
                }
            }

            return assemblyMap.Values.ToArray();
        }

        private static void AddAssembly(Dictionary<string, Assembly> assemblyMap, Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }

            string assemblyName = assembly.GetName().Name;
            if (!assemblyMap.ContainsKey(assemblyName))
            {
                assemblyMap.Add(assemblyName, assembly);
            }
        }

        private Session GetSession()
        {
            if (!IsInitialized)
            {
                throw new GameFrameworkException("GameNetty network module is not initialized.");
            }

            Session session = Session;
            if (session == null)
            {
                throw new GameFrameworkException("GameNetty session is invalid. Please login first.");
            }

            return session;
        }

        private static void SetRoot(Scene root)
        {
            if (_current == null)
            {
                return;
            }

            _current.Root = root;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ET.Log.Error(e.ExceptionObject.ToString());
        }

        [Event(SceneType.Main)]
        private class OnAppStartInitFinish : AEvent<Scene, AppStartInitFinish>
        {
            protected override async ETTask Run(Scene root, AppStartInitFinish args)
            {
                SetRoot(root);
                await ETTask.CompletedTask;
            }
        }
    }
}
