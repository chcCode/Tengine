using TEngine;
using UnityEngine;

/// <summary>
/// 游戏程序主入口，挂载在启动场景的根节点上，负责初始化核心模块并开启流程驱动。
/// </summary>
public class GameEntry : MonoBehaviour
{
    void Awake()
    {
        // 预先实例化框架核心模块，确保后续流程可直接获取
        ModuleSystem.GetModule<IUpdateDriver>();
        ModuleSystem.GetModule<IResourceModule>();
        ModuleSystem.GetModule<IDebuggerModule>();
        ModuleSystem.GetModule<IFsmModule>();

        // 启动流程状态机，进入热更检查/资源初始化等流程
        Settings.ProcedureSetting.StartProcedure().Forget();

        // 入口对象需跨场景常驻，避免被卸载
        DontDestroyOnLoad(this);
    }
}