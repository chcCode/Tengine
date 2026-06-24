using System.Collections.Generic;
using System.Reflection;
using GameConfig;
using GameLogic;
#if ENABLE_OBFUZ
using Obfuz;
#endif
using TEngine;
#pragma warning disable CS0436


/// <summary>
/// 游戏App。
/// </summary>
#if ENABLE_OBFUZ
[ObfuzIgnore(ObfuzScope.TypeName | ObfuzScope.MethodName)]
#endif
public partial class GameApp
{
    /// <summary>
    /// 是否启用 GameNetty 网络模块。
    /// 本地只调 UI 或未启动服务端时保持 false，避免网络初始化影响界面进入；
    /// 需要联调 Server 时改为 true。
    /// </summary>
    private static bool _enableNetwork = true;

    private static List<Assembly> _hotfixAssembly;

    /// <summary>
    /// 热更域App主入口。
    /// </summary>
    /// <param name="objects"></param>
    public static void Entrance(object[] objects)
    {
        GameEventHelper.Init();
        _hotfixAssembly = (List<Assembly>)objects[0];
        Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
        Log.Warning("======= Entrance GameApp =======");
        Utility.Unity.AddDestroyListener(Release);
        Log.Warning("======= StartGameLogic =======");
        StartGameLogic();
    }
    
    private static async void StartGameLogic()
    {
        LogLubanItemExample();
        // GameModule.UI.ShowUIAsync<BattleMainUI>();
        GameModule.UI.ShowUIAsync<LoginUI>();
        // GameModule.UI.ShowUIAsync<TestUI>();

        // 网络初始化放在 UI 显示之后，避免服务端未启动或网络模块异常时阻塞登录界面。
        if (_enableNetwork)
        {
            try
            {
                await GameModule.Network.InitializeAsync();
                long playerId = await ET.LoginHelper.Login(GameModule.Network.Root, "test", "123456");
                Log.Info("GameNetty 登录成功，PlayerId={0}", playerId);
            }
            catch (System.Exception exception)
            {
                Log.Error("GameNetty 登录失败：{0}", exception.Message);
            }
        }
        else
        {
            Log.Info("GameNetty 网络模块未启用，如需联调服务端请将 _enableNetwork 改为 true。");
        }
    }

    private static void LogLubanItemExample()
    {
        var potion = ItemConfigMgr.Instance.GetItem(1001);
        if (potion == null)
        {
            Log.Warning("Luban 示例：道具 1001 未找到，请先运行 TEngine/Luban/转表");
            return;
        }

        Log.Info("Luban 示例：{0} type={1} quality={2} price={3}",
            potion.Name, potion.Type, potion.Quality, potion.SellPrice);

        var weapons = ItemConfigMgr.Instance.GetItemsByType(EItemType.Weapon);
        Log.Info("Luban 示例：武器数量={0}", weapons.Count);
    }
    
    private static void Release()
    {
        if (_enableNetwork)
        {
            GameModule.Network.Disconnect();
        }

        SingletonSystem.Release();
        Log.Warning("======= Release GameApp =======");
    }
}